using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Le presenteurCarte
    private PresenteurCarte presenteurCarte;

    // Tableau des hexagones
    [SerializeField] private List<Hex> hexMap;

    void Start()
    {
        presenteurCarte = GetComponent<PresenteurCarte>();
    }



    #region Gestion de la carte

    // Appelée par le polling pour mettre à jour la carte.
    // Format la réponse du serveur en tableau d'hexagones.
    public void SetupTiles(string tiles)
    {
        // Exemple de tiles :
        // [{"x":0,"y":0,"lvl":1,"units":1,"type":"plain","owner":"player1","color":"#ff0000"}, ...]

        // Désérialisation du JSON en GameData
        GameData data = JsonUtility.FromJson<GameData>("{\"hexMap\":" + tiles + "}");
        if (data == null)
        {
            Debug.LogError("Erreur de désérialisation du JSON");
            return;
        }

        // ATTENTION : La comparaison directe entre data.hexMap et this.hexMap ne fonctionnera pas comme prévu.
        // Tu pourrais implémenter une comparaison sur les coordonnées ou un autre critère si nécessaire.
        // if (data.hexMap == this.hexMap) { return; }

        AjustHexesCount(data.hexMap);
        UpdateHexesAttributes(data.hexMap);
    }

    // Ajuste la liste des hexagones en ajoutant ceux manquants et en supprimant ceux qui ne sont plus présents.
    private void AjustHexesCount(Hex[] newHexes)
    {
        // Ajouter les hexagones qui sont dans la nouvelle carte mais absents de la map actuelle.
        CheckAjoutHexes(newHexes);
        // Supprimer les hexagones présents dans la map actuelle mais absents de la nouvelle carte.
        CheckSuppressionHexes(newHexes);
    }

    // Supprime les hexagones de this.hexMap qui ne sont plus présents dans newHexes.
    private void CheckSuppressionHexes(Hex[] newMap)
    {
        List<Hex> hexagonesASupprimer = new List<Hex>();

        for (int i = 0; i < this.hexMap.Count; i++)
        {
            bool found = false;
            for (int j = 0; j < newMap.Length; j++)
            {
                if (this.hexMap[i].x == newMap[j].x && this.hexMap[i].y == newMap[j].y)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                // Noter l'hexagone à supprimer
                hexagonesASupprimer.Add(this.hexMap[i]);
            }
        }
        // Supprimer les hexagones excédentaires
        foreach (Hex hex in hexagonesASupprimer)
        {
            // TODO: Appeler le presenteurCarte pour supprimer l'hexagone visuellement
            this.hexMap.Remove(hex);
        }
    }

    // Ajoute les hexagones qui sont présents dans newMap mais absents dans this.hexMap.
    private void CheckAjoutHexes(Hex[] newMap)
    {
        for (int i = 0; i < newMap.Length; i++)
        {
            bool found = false;
            for (int j = 0; j < this.hexMap.Count; j++)
            {
                if (newMap[i].x == this.hexMap[j].x && newMap[i].y == this.hexMap[j].y)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                // Debug.Log($"Ajout de l'hexagone ({newMap[i].x}, {newMap[i].y})");

                // Ajouter la tuile aux données du modèle
                this.hexMap.Add(newMap[i]);
                // Créer la tuile visuellement
                presenteurCarte.CreerTile(newMap[i]);
            }
        }
    }

    // Met à jour les attributs des hexagones déjà présents.
    // TODO: Appels au presenteurCarte pour mettre à jour visuellement les hexagones.
    private void UpdateHexesAttributes(Hex[] newMap)
    {
        for (int i = 0; i < newMap.Length; i++)
        {
            for (int j = 0; j < this.hexMap.Count; j++)
            {
                if (newMap[i].x == this.hexMap[j].x && newMap[i].y == this.hexMap[j].y)
                {
                    if (newMap[i].lvl != this.hexMap[j].lvl)
                    {
                        Debug.Log($"Lvl de l'hexagone ({newMap[i].x}, {newMap[i].y}) a changé de {this.hexMap[j].lvl} à {newMap[i].lvl}");
                        this.hexMap[j].lvl = newMap[i].lvl;
                    }
                    if (newMap[i].units != this.hexMap[j].units)
                    {
                        Debug.Log($"Units de l'hexagone ({newMap[i].x}, {newMap[i].y}) a changé de {this.hexMap[j].units} à {newMap[i].units}");
                        this.hexMap[j].units = newMap[i].units;
                    }
                    if (newMap[i].type != this.hexMap[j].type)
                    {
                        Debug.Log($"Type de l'hexagone ({newMap[i].x}, {newMap[i].y}) a changé de {this.hexMap[j].type} à {newMap[i].type}");
                        this.hexMap[j].type = newMap[i].type;
                    }
                    if (newMap[i].owner != this.hexMap[j].owner)
                    {
                        Debug.Log($"Owner de l'hexagone ({newMap[i].x}, {newMap[i].y}) a changé de {this.hexMap[j].owner} à {newMap[i].owner}");
                        this.hexMap[j].owner = newMap[i].owner;
                    }
                    if (newMap[i].color != this.hexMap[j].color)
                    {
                        Debug.Log($"Color de l'hexagone ({newMap[i].x}, {newMap[i].y}) a changé de {this.hexMap[j].color} à {newMap[i].color}");
                        this.hexMap[j].color = newMap[i].color;
                    }
                }
            }
        }
    }

    #endregion




    // Appelée par le polling pour mettre à jour l'argent
    public void UpdateMoney(string money)
    {
        // TODO: Implémenter la mise à jour de l'argent
    }

    public string moveUnitsAnimation(string[] move)
    {
        // TODO: Implémenter l'animation du déplacement d'unités
        return "";
    }
}

#region Classes de données

[Serializable]
public class GameData
{
    public Hex[] hexMap;
}

[Serializable]
public class Hex
{
    public int x;
    public int y;
    public int lvl;
    public int units;
    public string type;
    public string owner;
    public string color;

    // Conversion en dictionnaire (le champ _id n'étant pas présent, il n'est pas inclus)
    public Dictionary<string, object> ToDictionary()
    {
        var dict = new Dictionary<string, object>
        {
            {"x", x},
            {"y", y},
            {"lvl", lvl},
            {"units", units},
            {"type", type},
            {"owner", owner},
            {"color", color}
        };
        return dict;
    }
}

#endregion
