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

}
