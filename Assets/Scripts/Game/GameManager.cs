using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    private ServerClient serverClient; // Le service de communication avec le serveur
    private GameView gameView; // Le service de communication avec le serveur
    private List<Tile> grid = new List<Tile>(); // Tableau des hexagones
    [SerializeField] private GameObject tilePrefab; // Préfabriqué de la tile
    private int money; // Argent du joueur
    private string playerName; // Nom du joueur
    [SerializeField] private ErrorPanel errorPanel;

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



    // TODO: Erreur dans cette méthode, elle ne renvoie pas les bon hex
    public List<Tile> GetTilesInRadius(Tile centerTile, int radius)
    {
        List<Tile> tilesInRadius = new List<Tile>();
        Vector2Int center = new Vector2Int(centerTile.X, centerTile.Y);

        for (int dq = -radius; dq <= radius; dq++)
        {
            for (int dr = Mathf.Max(-radius, -dq - radius); dr <= Mathf.Min(radius, -dq + radius); dr++)
            {
                int q = center.x + dq;
                int r = center.y + dr;

                Tile tile = GetTileAt(q, r);
                if (tile != null)
                {
                    tilesInRadius.Add(tile);
                }
            }
        }

        Debug.Log($"Tiles in radius {radius} around ({center.x}, {center.y}): {tilesInRadius.Count}");
        return tilesInRadius;
    }


    // TODO: vérifier le bon fonctionnement
    public List<Vector3> GetTileGroupContour(List<Tile> tileGroup)
    {
        HashSet<Vector2Int> groupCoords = new HashSet<Vector2Int>(
            tileGroup.Select(tile => new Vector2Int(tile.X, tile.Y))
        );

        HashSet<(Vector3, Vector3)> rawEdges = new HashSet<(Vector3, Vector3)>();

        foreach (Tile tile in tileGroup)
        {
            Vector3[] corners = tile.GetCorners();

            for (int i = 0; i < 6; i++)
            {
                Vector2Int neighborCoord = GetNeighborAxial(new Vector2Int(tile.X, tile.Y), i);

                if (!groupCoords.Contains(neighborCoord))
                {
                    Vector3 a = corners[i];
                    Vector3 b = corners[(i + 1) % 6];
                    rawEdges.Add(NormalizeEdge(a, b));
                }
            }
        }

        return OrderEdgePoints(rawEdges);
    }




    #region Auto-générées
    private Vector2Int GetNeighborAxial(Vector2Int coord, int direction)
    {
        // Directions pour hexagones flat-topped
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(+1, 0), new Vector2Int(+1, -1), new Vector2Int(0, -1),
        new Vector2Int(-1, 0), new Vector2Int(-1, +1), new Vector2Int(0, +1)
        };
        return coord + directions[direction];
    }

    private (Vector3, Vector3) NormalizeEdge(Vector3 a, Vector3 b)
    {
        return (a.x < b.x || (Mathf.Approximately(a.x, b.x) && a.z < b.z)) ? (a, b) : (b, a);
    }

    private List<Vector3> OrderEdgePoints(HashSet<(Vector3, Vector3)> edges)
    {
        Dictionary<Vector3, List<Vector3>> edgeMap = new Dictionary<Vector3, List<Vector3>>();

        foreach (var (a, b) in edges)
        {
            if (!edgeMap.ContainsKey(a)) edgeMap[a] = new List<Vector3>();
            if (!edgeMap.ContainsKey(b)) edgeMap[b] = new List<Vector3>();

            edgeMap[a].Add(b);
            edgeMap[b].Add(a);
        }

        Vector3 start = edgeMap.First().Key;
        List<Vector3> ordered = new List<Vector3> { start };

        Vector3 current = start;
        Vector3 previous = Vector3.negativeInfinity;

        while (true)
        {
            var nextPoints = edgeMap[current].Where(p => p != previous).ToList();
            if (nextPoints.Count == 0) break;

            Vector3 next = nextPoints[0];
            ordered.Add(next);

            previous = current;
            current = next;

            if (next == start) break;
        }

        return ordered;
    }



    #endregion





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

    public void MoveUnitsServerResponse(string[] from, string[] to)
    {
        Tile fromTile = GetTileAt(int.Parse(from[0]), int.Parse(from[1]));
        Tile toTile = GetTileAt(int.Parse(to[0]), int.Parse(to[1]));
        gameView.DrawPathLine(new Tile[] { fromTile, toTile }); // Appel de l'effet de déplacement sur la tile d'origine et la tile de destination
    }



    public void RallyUnits(string[] tileCoords)
    {
        serverClient.RallyUnits(tileCoords);
    }

    public void RallyUnitsServerResponse(string[] tileCoords)
    {
        // Récupérer la tile de rallye
        Tile rallyTile = GetTileAt(int.Parse(tileCoords[0]), int.Parse(tileCoords[1]));
        gameView.ScanEffect(rallyTile, false); // Appel de l'effet de scan sur la tile de rallye
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
        errorPanel.init(message);
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

    public int GetAllPlayerUnits()
    {
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
