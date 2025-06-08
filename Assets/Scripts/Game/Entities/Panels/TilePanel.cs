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




    public void SetInfoTilePanel(Tile tile)
    {
        titleText.text = tile.Owner;

        coordinatesText.text = tile.X + " : " + tile.Y;

        string tmp = "";
        if (tile.Owner == PlayerPrefs.GetString("username"))
        {
            tmp += "Drones : " + tile.Units + "\n";
        }
        else
        {
            if (tile.Units == 0)
            {
                tmp += "Drones : 0+\n";
            }
            else
            {
                tmp += "Drones : " + tile.Units + "+\n";
            }
        }
        tmp += "Bat : " + tile.Type + "\n";
        tmp += "Lvl : " + tile.Lvl + "\n";
        descriptionText.text = tmp;
    }

}
