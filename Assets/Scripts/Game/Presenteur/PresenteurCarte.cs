using UnityEngine;

public class PresenteurCarte : MonoBehaviour
{

    private GridVue gridVue;



    private void Awake()
    {
        gridVue = GetComponent<GridVue>();
    }




    public void CreerTile(Hex hex)
    {
        gridVue.CreerTile(hex);
    }


    public void SupprimerTile(Hex hex){
        gridVue.SupprimerTile(GameObject.Find($"HexObj {hex.x}:{hex.y}"));
    }
    
    
    public void ModifierTile(Hex hex){
        gridVue.ModifierTile(hex, GameObject.Find($"HexObj {hex.x}:{hex.y}").GetComponent<Tile>());
    }







}
