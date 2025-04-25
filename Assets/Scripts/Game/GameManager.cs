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

        // On demande au serveur de nous envoyer la carte
        serverClient.updateMap();
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



    private void UpdateNodesArea()
    {
        // On récupère les hexagones de la carte
        List<Tile> nodes = new List<Tile>();
        foreach (Tile tile in grid)
        {
            if (tile.Type == "node")
            {
                tile.SetupNodeRadius();
            }
        }

        
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
        UpdateNodesArea(); // Met à jour les hexagones de type node
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
        UpdateNodesArea(); // Met à jour les hexagones de type node
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


    /// <summary>
    /// Renvoie la liste des coordonnées axiales (q, r) de tous les hexagones
    /// situés à une distance hexagonale ≤ radius autour de center.
    /// </summary>
    /// <param name="center">Coordonnée axiale (q, r) du centre.</param>
    /// <param name="radius">Rayon en pas hexagonaux.</param>
    public List<Vector2Int> GetHexesCoordsInRadius(Tile center, int radius)
    {
        var results = new List<Vector2Int>();

        // Pour chaque décalage q de -radius à +radius
        for (int dq = -radius; dq <= radius; dq++)
        {
            // Pour chaque décalage r dans la plage valide pour garder q + r + s == 0
            int minDr = Math.Max(-radius, -dq - radius);
            int maxDr = Math.Min(radius, -dq + radius);

            for (int dr = minDr; dr <= maxDr; dr++)
            {
                // On a s = -dq - dr mais comme on ne stocke qu'en axial,
                // on ne l'utilise pas ici.
                // On ajoute la coordonnée axiale décalée
                results.Add(new Vector2Int(center.X + dq, center.Y + dr));
            }
        }
        // Debug.Log("Hexes in radius (" + radius + ") : " + results.Count);
        return results;
    }

    public List<Tile> GetTilesInRadius(Tile center, int radius)
    {
        List<Vector2Int> coords = GetHexesCoordsInRadius(center, radius);
        List<Tile> tiles = new List<Tile>();
        string msg = "";

        foreach (Vector2Int coord in coords)
        {
            Tile tile = GetTileAt(coord.x, coord.y);

            if (tile != null)
            {
                msg += tile.ToString() + "\n";
                tiles.Add(tile);
            }
            else
            {
                msg += coord.x + "," + coord.y + " : Not Found\n";
            }
        }
        Debug.Log("Tiles in radius (" + radius + ") : " + tiles.Count + "\n" + msg);
        return tiles;
    }










    /// <summary>
    /// Retourne le contour (enveloppe convexe) du groupe de tiles,
    /// projeté sur le plan XZ et remonté à la hauteur moyenne des tiles.
    /// </summary>
    public List<Vector3> GetTileGroupContour(List<Tile> tileGroup)
    {
        // 1) Récupère tous les coins de chaque tile
        var points = new List<Vector3>();
        foreach (var tile in tileGroup)
            points.AddRange(tile.GetCorners());

        // 2) Calcule l'enveloppe convexe sur le plan XZ
        var hull = ComputeHullOnGround(points);

        // 3) Debug
        Debug.Log(
            $"Points bruts : {points.Count}\n" +
            $"Sommets du contour : {hull.Count}\n" +
            $"Niveau (lvl) : {tileGroup[0].Lvl}"
        );

        return hull;
    }

    /// <summary>
    /// Enveloppe convexe d'un nuage de Vector3 projeté en 2D (XZ).
    /// Le résultat est remonté à la hauteur moyenne Y.
    /// </summary>
    private static List<Vector3> ComputeHullOnGround(List<Vector3> points3D)
    {
        if (points3D == null || points3D.Count <= 1)
            return new List<Vector3>(points3D);

        // Projection en XZ et tri lexicographique
        var pts = points3D
            .Select(p => new Vector2(p.x, p.z))
            .Distinct()
            .OrderBy(p => p.x)
            .ThenBy(p => p.y)
            .ToList();

        if (pts.Count <= 1)
            return new List<Vector3>(points3D);

        // Construction de la lower hull
        var lower = new List<Vector2>();
        foreach (var p in pts)
        {
            while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(p);
        }

        // Construction de la upper hull
        var upper = new List<Vector2>();
        for (int i = pts.Count - 1; i >= 0; i--)
        {
            var p = pts[i];
            while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                upper.RemoveAt(upper.Count - 1);
            upper.Add(p);
        }

        // Fusion des deux chaînes (sans doubler les extrémités)
        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);
        var hull2D = lower.Concat(upper).ToList();

        // Calcul de la hauteur Y moyenne
        float avgY = points3D.Average(p => p.y);

        // Reprojection en 3D (X, avgY, Z)
        var hull3D = hull2D
            .Select(p2 => new Vector3(p2.x, avgY, p2.y))
            .ToList();

        return hull3D;
    }

    /// <summary>
    /// Produit scalaire (2D) de (OA × OB) pour tester l’orientation.
    /// </summary>
    private static float Cross(Vector2 O, Vector2 A, Vector2 B)
    {
        return (A.x - O.x) * (B.y - O.y)
             - (A.y - O.y) * (B.x - O.x);
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
