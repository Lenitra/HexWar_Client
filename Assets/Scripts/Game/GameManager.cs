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
    private string playerName; // Nom du joueur

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
        playerName = PlayerPrefs.GetString("username");
    }



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







    #region Fonction utilitaire pour récupérer des groupes de tiles

    private Tile GetTileAt(int x, int y)
    {
        foreach (Tile tile in grid)
        {
            if (tile.X == x && tile.Y == y)
            {
                return tile;
            }
        }
        return null;
    }

    private List<Tile> GetPlayerTiles()
    {
        List<Tile> playerTiles = new List<Tile>();
        foreach (Tile tile in grid)
        {
            if (tile.Owner == playerName)
            {
                playerTiles.Add(tile);
            }
        }
        return playerTiles;
    }

    private List<Tile> GetAdjacentTiles(Tile tile)
    {
        List<Tile> adjacentTiles = new List<Tile>();
        int x = tile.X;
        int y = tile.Y;

        // Ajout des hexagones adjacents (en diagonale et en croix)
        adjacentTiles.Add(GetTileAt(x - 1, y));
        adjacentTiles.Add(GetTileAt(x + 1, y));
        adjacentTiles.Add(GetTileAt(x, y - 1));
        adjacentTiles.Add(GetTileAt(x, y + 1));
        adjacentTiles.Add(GetTileAt(x + 1, y - 1));
        adjacentTiles.Add(GetTileAt(x - 1, y + 1));

        // Supprime les hexagones nuls
        adjacentTiles.RemoveAll(tile => tile == null);
        return adjacentTiles;
    }


    // Récupère les hexagones du joueur
    // Récupère également toutes les tiles adjacentes aux tiles du joueur
    private List<Tile> GetValidMoveDestination(Tile originTile)
    {
        List<Tile> validTiles = new List<Tile>();

        // Ajoute les hexagones appartement au joueur
        foreach (Tile tile in grid)
        {
            if (tile.Owner == playerName && validTiles.Contains(tile) == false)
            {
                validTiles.Add(tile);
            }

            if (tile.Owner == playerName)
            {

                // Récupère les hexagones adjacents de tile 
                List<Tile> adjacentTilesToTile = GetAdjacentTiles(tile);
                foreach (Tile adjacentTile in adjacentTilesToTile)
                {
                    if (!validTiles.Contains(adjacentTile))
                    {
                        validTiles.Add(adjacentTile);
                    }
                }
            }

        }

        validTiles.Remove(originTile); // Enlève la tile d'origine de la liste des destinations valides


        return validTiles;
    }

    #endregion





    #region Gestion de l'envoi des panels d'actions des tiles au serveur

    public void BuildTile(string[] tileCoords, string type)
    {
        serverClient.Build(tileCoords, type);
    }

    public void DestroyTile(string[] tileCoords)
    {
        serverClient.Destroy(tileCoords);
    }

    public void MoveUnitsTile(string[] from, string[] to, int units)
    {
        serverClient.MoveUnits(from, to, units);
    }

    public void MoveUnitsServerResponse()
    {
        // TODO : appeler la fonction de l'animation de déplacement des unités depuis le gameView
    }



    public void RallyUnits(string[] tileCoords)
    {
        serverClient.RallyUnits(tileCoords);
    }

    public void RallyUnitsServerResponse(string[] tileCoords)
    {
        // TODO: Appeler la fonction de l'animation de rally des unités depuis le gameView
        // Tile rallyPoint = GameObject.Find("HexObj " + tileCoords[0] + ":" + tileCoords[1]).GetComponent<Tile>();
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
        // TODO : afficher le panneau d'erreur avec un message
        // Penser à le récupérer dans les attributs de la classe
    }



    // Met en surbrillance les hexagones où le joueur peut se déplacer.
    public void HighlightMoveTiles(Tile originTile)
    {
        List<Tile> tiles = GetValidMoveDestination(originTile);
        foreach (Tile tile in tiles)
        {
            tile.HighlightTile();
        }
    }

    public void UnHighlightAllTiles()
    {
        foreach (Tile tile in grid)
        {
            tile.UnHighlightTile();
        }
    }

    public int GetAllPlayerUnits(){
        int totalUnits = 0;
        List<Tile> playerTiles = GetPlayerTiles();
        foreach (Tile tile in playerTiles)
        {
            totalUnits += tile.Units;
        }
        return totalUnits;
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
