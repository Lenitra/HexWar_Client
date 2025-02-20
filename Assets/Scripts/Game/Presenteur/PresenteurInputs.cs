using UnityEngine;

public class PresenteurInputs : MonoBehaviour
{

    private GridVue gridVue;

    private Tile selectTile1 = null;
    private Tile selectTile2 = null;

    private string state = "";


    #region Getters & Setters

    public Tile SelectTile1
    {
        get { return selectTile1; }
        set { selectTile1 = value; }
    }

    public Tile SelectTile2
    {
        get { return selectTile2; }
        set { selectTile2 = value; }
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

    public void TraiterClick(Vector2 mousePosition)
    {

        // Récupérer la tile cliquée
        Tile clickedTile = checkRaycast(mousePosition);


        // Si on est pas dans l'état de déplacement d'unités
        if (state != "move"){

            // Si on clique sur la case selectionnée ou dans le vide, on annule la selection 
            if (clickedTile == null || clickedTile == SelectTile1)
            {
                gridVue.DeselectTile(SelectTile1);
                SelectTile1 = null;
                SelectTile2 = null;
                State = "";
                return;
            }

            // Si on clique sur une case possédée, on change la selection
            if (clickedTile.Owner == PlayerPrefs.GetString("username") && clickedTile != selectTile1)
            {
                if (SelectTile1 != null)
                {
                    gridVue.DeselectTile(SelectTile1);
                }
                SelectTile1 = clickedTile;
                State = "Selected";
                gridVue.SelectTile(selectTile1);
                
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







}
