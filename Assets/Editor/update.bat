@echo off
REM Vérifier qu'un argument (le chemin d'installation) est bien fourni
if "%~1"=="" (
    echo Erreur : chemin d'installation manquant.
    exit /b 1
)

set "installPath=%~1"
set "updatePath=%~dp0"

REM Attendre quelques secondes pour être sûr que le jeu soit fermé
timeout /t 2 /nobreak >nul

REM Supprimer le contenu du dossier d'installation sans supprimer le dossier lui-même
echo Suppression du contenu de %installPath%
pushd "%installPath%"
for /d %%D in (*) do (
    rmdir /s /q "%%D"
)
for %%F in (*) do (
    del /q "%%F"
)
popd

REM Copier l'intégralité des fichiers et dossiers du dossier d'extraction (updatePath) vers le dossier d'installation
echo Copie des nouveaux fichiers depuis %updatePath% vers %installPath%
xcopy "%updatePath%\*" "%installPath%\" /E /Y /I

REM Attendre quelques instants pour que la copie se termine
timeout /t 2 /nobreak >nul

REM Lancer l'exécutable mis à jour (à adapter selon le nom réel de votre jeu)
echo Lancement de l'application mise à jour...
start "" "%installPath%\NyxsImperium.exe"

exit
