using UnityEngine;

public class PresenteurCarte : MonoBehaviour
{

    private GridVue gridVue;

    private string state = "";
    private Tile selectTile = null;



    #region Getters & Setters

    public Tile SelectTile
    {
        get { return selectTile; }
        set
        {
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

    public string State
    {
        get { return state; }
        set { state = value; }
    }

    #endregion




    private void Start()
    {
        gridVue = GetComponent<GridVue>();
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



    #region Gestion des actions de tile(s)





    #endregion

}
