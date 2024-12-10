#!/bin/bash

# Définir les variables de base
UNITY_PATH="/home/taumah/Unity/Hub/Editor/6000.0.28f1/Editor/Unity"
OUTPUT_PATH="Builds"
BUILD_METHOD="BuildAutomation.BuildAllPlatforms"


# Étape 1 : Nettoyer le dossier de sortie
if [ ! -d "$OUTPUT_PATH" ]; then
    mkdir -p "$OUTPUT_PATH"
else 
    rm -r "$OUTPUT_PATH"
    mkdir -p "$OUTPUT_PATH"
fi


# Étape 2 : Exécuter Unity pour compiler le projet
echo "Building the project..."
"$UNITY_PATH" -batchmode -nographics -quit -executeMethod "$BUILD_METHOD" > /dev/null 2>&1

if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi


# Récupérer la version du build depuis le fichier version.txt généré par Unity
VERSION=$(cat "$OUTPUT_PATH/version.txt") 
echo "Build v$VERSION completed successfully."

# Étape 3 : Créer une archive du dossier de construction
echo "Creating build archive..."

zip -r "$OUTPUT_PATH/HexWar_${VERSION}_linux.zip" "Builds/Linux/" > /dev/null 2>&1
zip -r "$OUTPUT_PATH/HexWar_${VERSION}_windows.zip" "Builds/Windows/" > /dev/null 2>&1

rm -rf "Builds/Linux"
rm -rf "Builds/Windows"
rm -rf "Builds/version.txt"


echo "Build archive created successfully."

echo "Version $VERSION archived successfully."
echo "End of build process."
exit 0
