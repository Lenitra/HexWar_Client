using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{

    private ServerClient serverClient; // Le service de communication avec le serveur
    private PresenteurCarte presenteurCarte; // Le presenteurCarte
    private PresenteurHUD presenteurHUD; // Le presenteurCarte
    private List<Hex> hexMap = new List<Hex>(); // Tableau des hexagones
    private int money; // Argent du joueur

    // Getter et setter pour money
    public int Money
    {
        get { return money; }
        set
        {
            money = value;
            presenteurHUD.UpdateMoney(money);
        }
    }
    public List<Hex> HexMap { get { return hexMap; } }


    void Start()
    {
        presenteurCarte = GetComponent<PresenteurCarte>();
        presenteurHUD = GetComponent<PresenteurHUD>();
        serverClient = GetComponent<ServerClient>();
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
        // if (data.hexMap == this.hexMap){return;}

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
            presenteurCarte.SupprimerTile(hex);
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
    private void UpdateHexesAttributes(Hex[] newMap)
    {
        for (int i = 0; i < newMap.Length; i++)
        {
            for (int j = 0; j < this.hexMap.Count; j++)
            {
                if (newMap[i].x == this.hexMap[j].x && newMap[i].y == this.hexMap[j].y)
                {
                    if (newMap[i] != this.hexMap[j])
                    {
                        // Debug.Log($"Mise à jour de l'hexagone ({newMap[i].x}, {newMap[i].y})");
                        this.hexMap[j] = newMap[i];
                        presenteurCarte.ModifierTile(this.hexMap[j]);
                    }
                }
            }
        }
    }

    #endregion




    // Appelée par le polling pour mettre à jour l'argent
    public void UpdateMoney(string money)
    {
        int newMoney = int.Parse(money);
        if (newMoney != this.money)
        {
            Money = newMoney;
        }
    }




    #region Gestion des actions des tiles

    public void BuildTile(string[] tileCoords, string type)
    {
        serverClient.Build(tileCoords, type);
    }


    public void AskServerMoveUnitsTiles(string[] from, string[] to, int units)
    {
        serverClient.MoveUnits(from, to, units);
    }


    #endregion






    public int GetAllUnits(string username = "")
    {
        int totalUnits = 0;
        foreach (Hex hex in hexMap)
        {
            if (username == "")
            {
                totalUnits += hex.units;
            }
            else
            {
                if (hex.owner == username)
                {
                    totalUnits += hex.units;
                }
            }

        }

        return totalUnits;

    }




    public void MoveUnitsServerResponse(string[] move)
    {
        presenteurCarte.CallAnimationMoveUnits(move);
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
