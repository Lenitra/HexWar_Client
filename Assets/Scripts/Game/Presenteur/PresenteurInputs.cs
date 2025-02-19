using UnityEngine;

public class PresenteurInputs : MonoBehaviour
{

    private GridVue gridVue;

    [SerializeField] private Tile selectTile1 = null;
    private Tile selectTile2 = null;

    private string state = "";


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
            if (clickedTile == null || clickedTile == selectTile1)
            {
                StartCoroutine(gridVue.DeselectTileAnim(selectTile1));
                selectTile1 = null;
                selectTile2 = null;
                state = "";
                return;
            }

            // Si on clique sur une case possédée, on change la selection
            if (clickedTile.Owner == PlayerPrefs.GetString("username") && clickedTile != selectTile1)
            {
                if (selectTile1 != null)
                {
                    StartCoroutine(gridVue.DeselectTileAnim(selectTile1));
                }
                selectTile1 = clickedTile;
                StartCoroutine(gridVue.SelectTileAnim(selectTile1));
                state = "Selected";
                
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
