using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TilePanel : MonoBehaviour
{
    [Header("Texts")]
    // Panel de texte informatif sur un hex
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI coordinatesText;
    [SerializeField] private TextMeshProUGUI descriptionText;


    public void SetInfoTilePanel(string title, string coordinates, string description)
    {
        titleText.text = title;
        coordinatesText.text = coordinates;
        descriptionText.text = description;
    }


    public void SetInfoTilePanel(Tile tile)
    {
        titleText.text = tile.Owner;
        
        coordinatesText.text = tile.X + " : " + tile.Y;

        string tmp = "";
        tmp += "Drones : " + tile.Units + "\n";
        tmp += "Bat : " + tile.Type + "\n";
        tmp += "Lvl : " + tile.Lvl + "\n";
        descriptionText.text = tmp;
    }



    public void SetPos(Vector3 mousePos)
    {
        // faire bouger le premier enfant du panel
        Vector3 pos = mousePos + new Vector3(80, -40, 0);
        transform.GetChild(0).transform.position = pos;
    }
    

}
