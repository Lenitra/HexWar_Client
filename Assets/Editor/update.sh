#!/bin/bash
# update.sh - Script de mise à jour pour Linux

# Attendre 2 secondes pour laisser le temps à l'application actuelle de se fermer
sleep 2

# cd dans le dossier du script
cd "$(dirname "$0")"

# Déterminer le répertoire du script (celui de l'archive extraite)
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Définir le dossier du jeu : ici, on considère que le dossier d'installation se trouve deux niveaux au-dessus du script
GAME_DIR="$SCRIPT_DIR/../"

echo "Mise à jour en cours..."

# Copier tous les fichiers extraits dans le dossier du jeu
cp -r "$SCRIPT_DIR/"* "$GAME_DIR/"

# Supprimer le dossier du script
rm -rf "$SCRIPT_DIR"

echo "Mise à jour terminée, redémarrage du jeu..."

# Lancer l'exécutable du jeu (à adapter selon le nom réel de votre binaire)
"$GAME_DIR/NyxsImperium.exe" &

# Fin du script
exit 0
