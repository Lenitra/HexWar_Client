#!/usr/bin/env python3
import os
import shutil
import subprocess
import sys
import paramiko
import os
import requests


# Définir les variables de base
# Adaptation selon la plateforme
if os.name == 'nt':  # Windows
    UNITY_PATH = r"C:\Program Files\Unity\Hub\Editor\6000.0.28f1\Editor\Unity.exe"
else:  # Linux/Ubuntu
    UNITY_PATH = "/home/taumah/Unity/Hub/Editor/6000.0.28f1/Editor/Unity"

OUTPUT_PATH = "Builds"

# Étape 1 : Nettoyer le dossier de sortie
if os.path.exists(OUTPUT_PATH):
    shutil.rmtree(OUTPUT_PATH)
os.makedirs(OUTPUT_PATH, exist_ok=True)

# Étape 2 : Exécuter Unity pour compiler le projet
print("Building the project...")
result = subprocess.run(
    [
        UNITY_PATH,
        "-batchmode",
        "-nographics",
        "-quit",
        "-executeMethod",
        "BuildAutomation.BuildAllPlatforms",
    ],
    stdout=subprocess.DEVNULL,
    stderr=subprocess.DEVNULL,
)

if result.returncode != 0:
    print("Build failed!")
    sys.exit(1)

# Récupérer la version du build depuis le fichier version.txt
version_file = os.path.join(OUTPUT_PATH, "version.txt")
if not os.path.exists(version_file):
    print("No version file found, build might have failed.")
    sys.exit(1)

with open(version_file, 'r') as vf:
    VERSION = vf.read().strip()

print(f"Build v{VERSION} completed successfully.")

# Étape 3 : Créer une archive du dossier de construction
print("Creating build archive...")

linux_build_path = os.path.join(OUTPUT_PATH, "Linux")
windows_build_path = os.path.join(OUTPUT_PATH, "Windows")
android_build_path = os.path.join(OUTPUT_PATH, "Android")

if os.path.exists(linux_build_path):
    shutil.make_archive(f"{OUTPUT_PATH}/HexWar_{VERSION}_linux", 'zip', linux_build_path)
    shutil.rmtree(linux_build_path)
    print(f"Linux archive created successfully.")

if os.path.exists(windows_build_path):
    shutil.make_archive(f"{OUTPUT_PATH}/HexWar_{VERSION}_windows", 'zip', windows_build_path)
    shutil.rmtree(windows_build_path)
    print(f"Windows archive created successfully.")

if os.path.exists(android_build_path):
    shutil.make_archive(f"{OUTPUT_PATH}/HexWar_{VERSION}_android", 'zip', android_build_path)
    shutil.rmtree(android_build_path)
    print(f"Windows archive created successfully.")

# Supprimer le fichier version.txt une fois les archives créées
version_txt_path = os.path.join(OUTPUT_PATH, "version.txt")
if os.path.exists(version_txt_path):
    os.remove(version_txt_path)

print("Build archive created successfully.")
print(f"Version {VERSION} archived successfully.")
print("End of build process.")

print("")
print("")
print("")

# Informations de connexion SSH
hostname = "217.160.99.153"  # Adresse IP du VPS
port = 22  # Port SSH par défaut
username = "root"
password = input(
    f"Ctrl + C pour ne pas uploader sur le serveur\nVotre mot de passe pour root {hostname} : "
)

# clear la console
os.system('cls' if os.name == 'nt' else 'clear')
print("Build OK")
print("Archives OK")
print("Updating server repository...")

# Chemin du fichier local et chemin sur le VPS
local_files = os.listdir(OUTPUT_PATH)
remote_dir = "/root/HexWar_Builds"

# Création d'un client SSH
ssh_client = paramiko.SSHClient()
ssh_client.set_missing_host_key_policy(paramiko.AutoAddPolicy())

try:
    # Connexion au serveur
    ssh_client.connect(
        hostname=hostname, port=port, username=username, password=password
    )

    # Ouverture d'une session SFTP
    sftp_client = ssh_client.open_sftp()

    # Se placer dans le répertoire distant
    sftp_client.chdir(remote_dir)

    # Nettoyer le contenu du dossier distant
    for file in sftp_client.listdir():
        sftp_client.remove(file)

    # Upload des fichiers
    for f in local_files:
        local_path = os.path.join(OUTPUT_PATH, f)
        # On donne un nom de fichier complet côté distant
        # Comme on a déjà fait chdir, un simple nom de fichier suffit
        remote_path = f

        # S'assurer que local_path pointe bien vers un fichier
        if os.path.isfile(local_path):
            sftp_client.put(local_path, remote_path)
            print(f"Fichier {local_path} uploadé avec succès dans {remote_dir}.")

    # reboot du serveur 
    stdin, stdout, stderr = ssh_client.exec_command("reboot")


except Exception as e:
    print(f"Erreur lors de l'upload : \n{e}")


