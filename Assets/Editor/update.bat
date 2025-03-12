@echo off
REM update.bat - Script de mise à jour pour Windows

REM Attendre 2 secondes pour laisser le temps à l'application actuelle de se fermer
timeout /t 2 /nobreak >nul

REM Se positionner dans le dossier du script
cd /d "%~dp0"

REM Définir le dossier du jeu : ici, on considère que le dossier d'installation se trouve un niveau au-dessus du script
set "GAME_DIR=%~dp0..\"

echo Mise à jour en cours...

REM Copier tous les fichiers extraits dans le dossier du jeu
xcopy /s /e /y "%~dp0*" "%GAME_DIR%"

echo Mise à jour terminée, redémarrage du jeu...

REM Lancer l'exécutable du jeu (à adapter selon le nom réel de votre binaire)
start "" "%GAME_DIR%NyxsImperium.exe"

REM Création d'un script temporaire pour supprimer le dossier de mise à jour
(
    echo @echo off
    echo timeout /t 2 /nobreak >nul
    REM Supprimer le dossier contenant le script de mise à jour
    echo rd /s /q "%~dp0"
) > "%TEMP%\cleanup.bat"

REM Lancer le script de nettoyage dans une nouvelle fenêtre
start "" "%TEMP%\cleanup.bat"

exit /b
