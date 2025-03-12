#!/usr/bin/env python3
import os
import shutil
import subprocess
import sys
import paramiko

# Pour faciliter le débogage, activez DEBUG à True
DEBUG = True

# Définir les variables de base
# Adaptation selon la plateforme
if os.name == 'nt':  # Windows
    UNITY_PATH = r"C:\Program Files\Unity\Hub\Editor\6000.0.28f1\Editor\Unity.exe"
else:  # Linux/Ubuntu
    UNITY_PATH = "/home/taumah/Unity/Hub/Editor/6000.0.28f1/Editor/Unity"
    PROJECT_PATH = os.path.abspath("/home/taumah/Bureau/HexWar_Client")  # À adapter !

# Ajoutez le chemin de votre projet Unity ici

OUTPUT_PATH = "Builds"

# Étape 1 : Nettoyer le dossier de sortie
if os.path.exists(OUTPUT_PATH):
    print(f"Suppression du dossier {OUTPUT_PATH}...")
    shutil.rmtree(OUTPUT_PATH)
os.makedirs(OUTPUT_PATH, exist_ok=True)

# Étape 2 : Exécuter Unity pour compiler le projet
print("Lancement de Unity pour builder le projet...")

# Préparer la commande Unity avec le chemin du projet
unity_command = [
    UNITY_PATH,
    "-batchmode",
    "-projectPath", PROJECT_PATH,
    "-nographics",
    "-quit",
    "-executeMethod",
    "BuildAutomation.BuildAllPlatforms",
]

try:
    result = subprocess.run(
        unity_command,
        stdout=subprocess.PIPE if DEBUG else subprocess.DEVNULL,
        stderr=subprocess.PIPE if DEBUG else subprocess.DEVNULL,
        text=True  # Pour obtenir les sorties en chaîne de caractères
    )
    if DEBUG:
        print("Sortie standard de Unity :")
        print(result.stdout)
        print("Sortie d'erreur de Unity :")
        print(result.stderr)
except Exception as e:
    print(f"Erreur lors de l'exécution de Unity : {e}")
    sys.exit(1)

if result.returncode != 0:
    print("Le build a échoué !")
    sys.exit(1)

# Récupérer la version du build depuis le fichier version.txt
version_file = os.path.join(OUTPUT_PATH, "version.txt")
if not os.path.exists(version_file):
    print("Fichier version.txt non trouvé, le build a peut-être échoué.")
    sys.exit(1)

with open(version_file, 'r') as vf:
    VERSION = vf.read().strip()

print(f"Build v{VERSION} terminé avec succès.")

# Étape 3 : Créer une archive du dossier de construction
print("Création des archives de build...")

linux_build_path = os.path.join(OUTPUT_PATH, "Linux")
windows_build_path = os.path.join(OUTPUT_PATH, "Windows")
android_build_path = os.path.join(OUTPUT_PATH, "Android")

if os.path.exists(linux_build_path):
    archive_path = f"{OUTPUT_PATH}/HexWar_{VERSION}_linux"
    shutil.make_archive(archive_path, 'zip', linux_build_path)
    shutil.rmtree(linux_build_path)
    print(f"Archive Linux créée avec succès sous {archive_path}.zip")

if os.path.exists(windows_build_path):
    archive_path = f"{OUTPUT_PATH}/HexWar_{VERSION}_windows"
    shutil.make_archive(archive_path, 'zip', windows_build_path)
    shutil.rmtree(windows_build_path)
    print(f"Archive Windows créée avec succès sous {archive_path}.zip")

# Si besoin, décommentez pour Android
# if os.path.exists(android_build_path):
#     archive_path = f"{OUTPUT_PATH}/HexWar_{VERSION}_android"
#     shutil.make_archive(archive_path, 'zip', android_build_path)
#     shutil.rmtree(android_build_path)
#     print(f"Archive Android créée avec succès sous {archive_path}.zip")

# Supprimer le fichier version.txt une fois les archives créées
version_txt_path = os.path.join(OUTPUT_PATH, "version.txt")
if os.path.exists(version_txt_path):
    os.remove(version_txt_path)

print("Toutes les archives ont été créées avec succès.")
print(f"Version {VERSION} archivée avec succès.")
print("Fin du processus de build.\n\n")

# Informations de connexion SSH
hostname = "212.227.52.171"  # Adresse IP du VPS
port = 22  # Port SSH par défaut
username = "root"
password = input(
    f"Ctrl + C pour annuler l'upload.\nEntrez le mot de passe pour root {hostname} : "
)

# Effacer la console
os.system('cls' if os.name == 'nt' else 'clear')
print("Build OK")
print("Archives OK")
print("Mise à jour du dépôt sur le serveur...")

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

    try:
        # Se placer dans le répertoire distant
        sftp_client.chdir(remote_dir)
    except IOError:
        # Si le dossier n'existe pas, on le crée
        sftp_client.mkdir(remote_dir)
        sftp_client.chdir(remote_dir)

    # Nettoyer le contenu du dossier distant (uniquement les fichiers)
    for file in sftp_client.listdir():
        remote_file = os.path.join(remote_dir, file)
        try:
            sftp_client.remove(remote_file)
            print(f"Fichier {remote_file} supprimé sur le serveur.")
        except Exception as e:
            print(f"Impossible de supprimer {remote_file} : {e}")

    # Upload des fichiers
    for f in local_files:
        local_path = os.path.join(OUTPUT_PATH, f)
        remote_path = f  # Puisqu'on est déjà dans remote_dir
        if os.path.isfile(local_path):
            sftp_client.put(local_path, remote_path)
            print(f"Fichier {local_path} uploadé avec succès dans {remote_dir}.")

    # Reboot du serveur 
    stdin, stdout, stderr = ssh_client.exec_command("reboot")
    print("Commande de reboot envoyée au serveur.")

    # Fermeture des connexions SFTP et SSH
    sftp_client.close()
    ssh_client.close()

except Exception as e:
    print(f"Erreur lors de l'upload : \n{e}")
