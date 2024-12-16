#!/usr/bin/env python3
import os
import shutil
import subprocess
import sys

# Définir les variables de base
# Adaptation selon la plateforme
if os.name == 'nt':  # Windows
    UNITY_PATH = r"C:\Program Files\Unity\Hub\Editor\6000.0.28f1\Editor\Unity.exe"
else:  # Linux/Ubuntu
    UNITY_PATH = "/home/taumah/Unity/Hub/Editor/6000.0.28f1/Editor/Unity"

OUTPUT_PATH = "Builds"

def main():
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

    if os.path.exists(linux_build_path):
        shutil.make_archive(f"{OUTPUT_PATH}/HexWar_{VERSION}_linux", 'zip', linux_build_path)
        shutil.rmtree(linux_build_path)

    if os.path.exists(windows_build_path):
        shutil.make_archive(f"{OUTPUT_PATH}/HexWar_{VERSION}_windows", 'zip', windows_build_path)
        shutil.rmtree(windows_build_path)

    # Supprimer le fichier version.txt une fois les archives créées
    version_txt_path = os.path.join(OUTPUT_PATH, "version.txt")
    if os.path.exists(version_txt_path):
        os.remove(version_txt_path)

    print("Build archive created successfully.")
    print(f"Version {VERSION} archived successfully.")
    print("End of build process.")
    sys.exit(0)

if __name__ == "__main__":
    main()
