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

    [Header("Boutons facultatifs")]
    [SerializeField] private Button buildButton;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button rallyButton;


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
        if (tile.Owner == PlayerPrefs.GetString("username"))
        {
            tmp += "Drones : " + tile.Units + "\n";
        }
        else
        {
            if (tile.Units == 0)
            {
                tmp += "Drones : ?\n";
            }
            else
            {
                tmp += "Drones : ~" + tile.Units + "\n";
            }
        }
        tmp += "Bat : " + tile.Type + "\n";
        tmp += "Lvl : " + tile.Lvl + "\n";
        descriptionText.text = tmp;
        if (buildButton != null)
        {
            if (tile.Lvl >= 5){
                buildButton.interactable = false;
                buildButton.GetComponentInChildren<TextMeshProUGUI>().text = "Niv. max";
            }
            else
            {
                buildButton.GetComponentInChildren<TextMeshProUGUI>().text = "Construire";
                buildButton.interactable = true;
            }
        }
        if (moveButton != null)
        {
            if (tile.Units > 0)
            {
                moveButton.interactable = true;
                moveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Déployer";
            }
            else
            {
                moveButton.interactable = false;
                moveButton.GetComponentInChildren<TextMeshProUGUI>().text = "ø drones";
            }
        }
    }



    public void SetPos(Vector3 mousePos)
    {
        // faire bouger le premier enfant du panel
        Vector3 pos = mousePos + new Vector3(80, -40, 0);
        transform.GetChild(0).transform.position = pos;
    }
    

}
