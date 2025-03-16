#!/bin/bash

# Vérifier qu'un argument (le chemin d'installation) est bien fourni
if [ -z "$1" ]; then
    echo "Erreur : chemin d'installation manquant."
    exit 1
fi

installPath="$1"
# Déterminer le dossier où se trouve le script (dossier d'extraction)
updatePath="$(dirname "$0")"

# Attendre quelques secondes pour être sûr que l'application soit fermée
sleep 2

echo "Suppression du contenu de $installPath"
# Supprimer le contenu du dossier d'installation (fichiers et dossiers, y compris les fichiers cachés)
rm -rf "$installPath"/*
rm -rf "$installPath"/.[!.]*
rm -rf "$installPath"/..?*

echo "Copie des nouveaux fichiers depuis $updatePath vers $installPath"
# Copier tous les fichiers et dossiers (inclus les fichiers cachés)
cp -r "$updatePath"/* "$installPath"
cp -r "$updatePath"/.[!.]* "$installPath" 2>/dev/null
cp -r "$updatePath"/..?* "$installPath" 2>/dev/null

# Attendre quelques instants pour être sûr que la copie soit terminée
sleep 2

echo "Lancement de l'application mise à jour..."
# Lancer l'exécutable mis à jour (à adapter selon le nom réel de votre jeu)
"$installPath/NyxsImperium.x86_64" &

exit 0
