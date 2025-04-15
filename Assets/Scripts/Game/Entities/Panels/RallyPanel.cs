using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RallyPanel : MonoBehaviour
{
    [SerializeField] private Controller controller;

    [Header("Elements communs")]

    [SerializeField] private Button closeButton;
    [SerializeField] private Button validateBtn;

    [Header("Elements enfants")]
    [SerializeField] private TextMeshProUGUI description;



    void Start()
    {
        // Setup des boutons généraux
        // closeButton.onClick.AddListener(gridVue.ClosedPanel);

        // Setup des boutons de déplacement
        // validateBtn.onClick.AddListener(gridVue.ValidateRally);
    }



    public void SetupPanel(int totalUnits, string coords)
    {
        description.text = $"Confirmez le déplacement de {totalUnits} drones vers {coords}";
    }

}
