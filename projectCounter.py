# Sert à compter le nombre de lignes de code dans un projet.

import os
import sys

# parcourir l'arborecence du projet à partir du dossier courant
def countLines(path: str, ext: str ) -> int:
    total_lines = 0
    for root, dirs, files in os.walk(path):
        for file in files:
            if file.endswith(ext):
                with open(os.path.join(root, file), "r") as f:
                    try:
                        total_lines += len(f.readlines())
                    except:
                        pass
    return total_lines


if __name__ == "__main__":
    path = os.getcwd()
    print("Nombre de lignes en C# : ", countLines(path, ".cs"))
    print("Nombre de lignes total : ", countLines(path, ""))

