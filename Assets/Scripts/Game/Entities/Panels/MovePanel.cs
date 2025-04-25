using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MovePanel : MonoBehaviour
{
    [Header("Elements communs")]
    [SerializeField] private Controller controller;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button validateBtn;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Slider slider;
    [SerializeField] private Button maxBtn;

    [Header("Informations visuelles")]
    [SerializeField] private TextMeshProUGUI originTileText;
    [SerializeField] private TextMeshProUGUI destinationTileText;
    [SerializeField] private Transform attackIcon;
    [SerializeField] private Transform defenceIcon;


    private Tile originTile;
    private Tile destinationTile;


    private int unitsMax;
    private int unitsToMove;





    private void Start()
    {

        // Setup des boutons généraux
        
        closeBtn.onClick.AddListener(ClosePanel);

        // Setup des boutons de déplacement
        validateBtn.onClick.AddListener(ValidateMove);

        // Setup des inputs
        inputField.onValueChanged.AddListener(OnInputFieldChange);
        slider.onValueChanged.AddListener(OnSliderChange);
        maxBtn.onClick.AddListener(() => OnInputFieldChange(unitsMax.ToString()));

    }

    private void ClosePanel()
    {
        controller.SelectedTile = null;
        originTile = null;
        destinationTile = null;
        gameObject.SetActive(false);
    
    }

    private void OnInputFieldChange(string value)
    {
        if (value == "")
        {
            slider.value = 0;
            unitsToMove = 0;
        }
        else
        {
            int units = int.Parse(value);
            if (units > unitsMax)
            {
                units = unitsMax;
                inputField.text = units.ToString();
            }
            if (units < 0)
            {
                units = 0;
                inputField.text = units.ToString();
            }
            slider.value = units;
            unitsToMove = units;
        }
    }


    private void OnSliderChange(float value)
    {
        int units = (int)value;
        inputField.text = units.ToString();
        unitsToMove = units;
    }




    public void SetupPanel(Tile origin, Tile destination)
    {
        originTile = origin;
        destinationTile = destination;
        
        // Setup des textes d'informations sur les tuiles
        originTileText.text = TilesInfos(origin);
        destinationTileText.text = TilesInfos(destination);

        if (origin.Owner == destination.Owner)
        {
            attackIcon.gameObject.SetActive(false);
            defenceIcon.gameObject.SetActive(true);
        }
        else
        {
            attackIcon.gameObject.SetActive(true);
            defenceIcon.gameObject.SetActive(false);
        }

        // Setup des inputs
        slider.maxValue = origin.Units;
        slider.value = 0;


        unitsMax = origin.Units;
        unitsToMove = 0;
    }

    private string TilesInfos(Tile tile){
        string toret = "";
        toret += $"({tile.X}, {tile.Y})";
        toret += $"\n{tile.Owner}";
        if (tile.Owner == PlayerPrefs.GetString("username"))
        {
            toret += $"\n{tile.Units} drones";
        }
        return toret;
    }

    private void ValidateMove()
    {
        controller.MoveTile(originTile, destinationTile, unitsToMove);
        ClosePanel();
    }

}
