using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PresenteurCarte : MonoBehaviour
{

    private GridVue gridVue;
    private GameManager gameManager;

    private string state = "";
    private Tile selectTile = null;
    private Tile selectTile2 = null;




    #region Getters & Setters

    public Tile SelectTile
    {
        get { return selectTile; }
        set
        {
            SelectTile2 = null;
            if (selectTile != null)
            {
                gridVue.DeselectTile(selectTile);
            }

            selectTile = value;

            if (selectTile != null)
            {
                gridVue.SelectTile(selectTile);
            }
        }
    }

    public Tile SelectTile2
    {
        get { return selectTile2; }
        set { selectTile2 = value; }
    }

    public string State
    {
        get { return state; }
        set
        {
            state = value;
            gridVue.SetupStateInfos(state);
        }
    }

    #endregion




    private void Start()
    {
        gridVue = GetComponent<GridVue>();
        gameManager = GetComponent<GameManager>();
    }



    #region Modifications de la grille (Création, Suppression, Modification)
    public void CreerTile(Hex hex)
    {
        gridVue.CreerTile(hex);
    }


    public void SupprimerTile(Hex hex)
    {
        gridVue.SupprimerTile(GameObject.Find($"HexObj {hex.x}:{hex.y}"));
    }


    public void ModifierTile(Hex hex)
    {
        gridVue.ModifierTile(hex, GameObject.Find($"HexObj {hex.x}:{hex.y}").GetComponent<Tile>());
    }

    #endregion



    #region Traitement des clics sur la grille
    public void TraiterClick(Vector2 mousePosition)
    {

        // Récupérer la tile cliquée
        Tile clickedTile = checkRaycast(mousePosition);


        // Si on est pas dans l'état de déplacement d'unités
        if (state != "move")
        {

            // Si on clique sur la case selectionnée ou dans le vide, on annule la selection 
            if (clickedTile == null || clickedTile == SelectTile)
            {
                SelectTile = null;
                State = "";
                return;
            }

            // Si on clique sur une case possédée, on change la selection
            if (clickedTile.Owner == PlayerPrefs.GetString("username") && clickedTile != SelectTile)
            {
                SelectTile = clickedTile;
                State = "Selected";
                return;
            }

            // Si on clique sur une case qui n'est pas a soi
            if (clickedTile.Owner != PlayerPrefs.GetString("username") && State != "move")
            {
                SelectTile = null;
                State = "";
                return;
            }
        }


        else if (state == "move" && SelectTile != null)
        {

            // Si on clique sur une case autre que la case selectionnée
            if (clickedTile != null && clickedTile != SelectTile)
            {
                // Si la case cliquée est une case de destination possible
                if (GetTilesToMove().Contains(clickedTile))
                {
                    SelectTile2 = clickedTile;
                    gridVue.DisplayMovePanel(SelectTile, SelectTile2);
                    State = "";
                    gridVue.UnHighlightTiles();
                    return;
                }
            }
            else
            {
                SelectTile = null;
                SelectTile2 = null;
                State = "";
                gridVue.UnHighlightTiles();
                return;
            }
        }
    }



    private Tile checkRaycast(Vector2 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Tile tile = hit.transform.GetComponent<Tile>();
            if (tile != null)
            {
                return tile;
            }
        }
        return null;
    }

    #endregion



    #region Gestion des bontons pouractions de tile(s) (Build, Move, Rally)

    // Demande de construction (envoi des données depuis le panel de build/upgrade)
    public void BuildTile(string type)
    {
        string[] tileCoords = { SelectTile.X.ToString(), SelectTile.Y.ToString() };
        gameManager.BuildTile(tileCoords, type);
        SelectTile = null;
    }


    // Demande de déplacement d'unités depuis GridVue
    public void MoveUnits(int units)
    {
        gameManager.AskServerMoveUnitsTiles(new string[] { SelectTile.X.ToString(), SelectTile.Y.ToString() }, new string[] { SelectTile2.X.ToString(), SelectTile2.Y.ToString() }, units);
    }

    // Appel de l'animation de déplacement d'unités avec argument le tableau de tiles à traverser # format : ["x1:y1", "x2:y2", ...]
    public void CallAnimationMoveUnits(string[] move)
    {
        gridVue.MoveUnitsAnim(move);
    }

    // Demande de ralliement d'unités depuis GridVue
    public void RallyUnits()
    {
        gameManager.AskServerRallyUnits(new string[] { SelectTile.X.ToString(), SelectTile.Y.ToString() });
    }

    #endregion



    #region Utils
    // Retourne la liste des cases sur lesquelles on peut déplacer des unités
    public Tile[] GetTilesToMove()
    {
        List<Tile> toret = new List<Tile>();


        for (int i = 0; i < (gameManager.HexMap).Count; i++)
        {
            Tile tile = GameObject.Find($"HexObj {gameManager.HexMap[i].x}:{gameManager.HexMap[i].y}").GetComponent<Tile>();
            // La tile d'origine ne peut pas être une tile de destination
            if (tile == SelectTile)
            {
                continue;
            }

            if (tile.Owner == PlayerPrefs.GetString("username"))
            {
                toret.Add(tile);
                continue;
            }

            // Vérifier si une des tuiles adjacentes ont comme owner le joueur
            List<(int, int)> neighbors = NeighborsFlatTop(tile.X, tile.Y);
            for (int j = 0; j < neighbors.Count; j++)
            {

                Tile neighbor = null;

                try
                {
                    neighbor = GameObject.Find($"HexObj {neighbors[j].Item1}:{neighbors[j].Item2}").GetComponent<Tile>();
                }
                catch (System.Exception)
                {
                    continue;
                }



                if (neighbor.Owner == PlayerPrefs.GetString("username"))
                {
                    toret.Add(tile);
                    break;
                }
                continue;
            }


        }


        // Supprimer tout les doublons 
        toret = toret.Distinct().ToList();
        return toret.ToArray();
    }


    public static List<(int, int)> NeighborsFlatTop(int x, int z)
    {
        return new List<(int, int)>
        {
            (x,     z - 1),  // N
            (x + 1, z - 1),  // NE
            (x + 1, z),      // SE
            (x,     z + 1),  // S
            (x - 1, z + 1),  // SW
            (x - 1, z)       // NW
        };
    }

    public int GetTotalUnitCount()
    {
        return gameManager.GetAllUnits(PlayerPrefs.GetString("username"));
    }

    #endregion

}
