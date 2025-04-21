using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class RallyPanel : MonoBehaviour
{
    [SerializeField] private Controller controller;

    [Header("Elements communs")]

    [SerializeField] private Button closeButton;
    [SerializeField] private Button validateBtn;

    [Header("Elements enfants")]
    [SerializeField] private TextMeshProUGUI description;

    private Tile tileToRally;

    void Start()
    {
        // Setup des boutons généraux
        closeButton.onClick.AddListener(close);

        // Setup des boutons de déplacement
        validateBtn.onClick.AddListener(sendRallyPoint);
    }

    private void close()
    {
        gameObject.SetActive(false);
    }

    public void SetupPanel(int totalUnits, Tile tile)
    {
        description.text = $"Confirmez le déplacement de {totalUnits} drones vers {tile.Coords()}";
        tileToRally = tile;
    }

    private void sendRallyPoint()
    {
        controller.RallyTile(tileToRally);
        close();
    }

}
