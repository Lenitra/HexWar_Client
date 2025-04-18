using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{

    private ServerClient serverClient; // Le service de communication avec le serveur
    private GameView gameView; // Le service de communication avec le serveur
    private List<Tile> grid = new List<Tile>(); // Tableau des hexagones
    [SerializeField] private GameObject tilePrefab; // Préfabriqué de la tile
    private int money; // Argent du joueur

    // Getter et setter pour money
    public int Money
    {
        get { return money; }
        set
        {
            money = value;
        }
    }


    void Start()
    {
        serverClient = GetComponent<ServerClient>();
        gameView = GetComponent<GameView>();
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

       // Ajouter les hexagones qui sont dans la nouvelle carte mais absents de la map actuelle.
        CheckAjoutHexes(data.hexMap);
        // Supprimer les hexagones présents dans la map actuelle mais absents de la nouvelle carte.
        CheckSuppressionHexes(data.hexMap);

        // Mettre à jour les attributs des tiles.
        UpdateHexesAttributes(data.hexMap);
    }





    // Ajoute les hexagones qui sont présents dans newMap mais absents dans this.hexMap.
    private void CheckAjoutHexes(Hex[] newMap)
    {
        for (int i = 0; i < newMap.Length; i++)
        {
            if (HexToTileInGrid(newMap[i]) == null)
            {
                AddTileToGrid(newMap[i]);
            }
        }
    }





    // Supprime les hexagones de this.hexMap qui ne sont plus présents dans newHexes.
    private void CheckSuppressionHexes(Hex[] newMap)
    {
        foreach (Tile tile in grid)
        {
            bool found = false;
            for (int i = 0; i < newMap.Length; i++)
            {
                if (tile.X == newMap[i].x && tile.Y == newMap[i].y)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                RemoveTileToGrid(tile);
            }
        }
    }



    // Met à jour les attributs des hexagones déjà présents.
    private void UpdateHexesAttributes(Hex[] newMap)
    {
        foreach (Hex hex in newMap)
        {
            Tile tile = HexToTileInGrid(hex);
            if (tile != null)
            {
                if (tile.Lvl != hex.lvl || tile.Units != hex.units || tile.Type != hex.type || tile.Owner != hex.owner || tile.Color != hex.color || tile.Protect != hex.protect)
                {
                    UpdateTileAttributes(hex);
                }
            }

            // TODO: Supprimer le else si vérifié que tout fonctionne.
            else
            {
                Debug.LogError("Tile non trouvée dans la grille lors de la mise à jour des attributs.");
            }

        }
    }





    #region Gestion des hexagones/tiles individuels


    private Tile HexToTileInGrid(Hex hex)
    {
        foreach (Tile tile in grid)
        {
            if (tile.X == hex.x && tile.Y == hex.y)
            {
                return tile;
            }
        }
        return null;
    }


    // Instancie une tile avec les attributs d'un hexagone.
    private void AddTileToGrid(Hex hex)
    {
        Tile tile = Instantiate(tilePrefab).GetComponent<Tile>();
        tile.transform.SetParent(transform);
        tile.SetupTile(hex);
        grid.Add(tile);
    }

    // Met à jour une tile avec les attributs d'un hexagone.
    private void UpdateTileAttributes(Hex hex)
    {
        HexToTileInGrid(hex).SetupTile(hex);
    }


    private void RemoveTileToGrid(Tile tile)
    {
        StartCoroutine(tile.DestructionCoroutine());
        grid.Remove(tile);
    }




    #endregion
    #endregion
















    // Appelée par le polling pour mettre à jour l'argent
    public void UpdateMoney(string money)
    {
        int newMoney = int.Parse(money);
        if (newMoney != this.money)
        {
            Money = newMoney;
            gameView.SetMoney(Money);
        }
    }




    #region Gestion des actions des tiles

    public void BuildTile(string[] tileCoords, string type)
    {
        serverClient.Build(tileCoords, type);
    }

    public void DestroyTile(string[] tileCoords)
    {
        serverClient.Destroy(tileCoords);
    }

    public void AskServerMoveUnitsTiles(string[] from, string[] to, int units)
    {
        serverClient.MoveUnits(from, to, units);
    }

    public void MoveUnitsServerResponse()
    {
        // presenteurCarte.CallAnimationMoveUnits();
    }



    public void AskServerRallyUnits(string[] tileCoords)
    {
        serverClient.RallyUnits(tileCoords);
    }

    public void RallyUnitsServerResponse(string[] tileCoords)
    {
        Tile rallyPoint = GameObject.Find("HexObj " + tileCoords[0] + ":" + tileCoords[1]).GetComponent<Tile>();
        // presenteurCarte.CallAnimationRallyUnits(rallyPoint);
    }


    public void AskServerDispatchUnits()
    {
        serverClient.DispatchUnits();
    }

    public void DispatchUnitsServerResponse()
    {
        // presenteurCarte.CallAnimationDispatchUnits(getHQTileTransform().GetComponent<Tile>());
    }




    #endregion








    public void nopePanel(string message)
    {
        // presenteurHUD.nopePanel(message);
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
    public string protect;


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
            {"color", color},
            {"protect", false}
        };
        return dict;
    }
}

#endregion
