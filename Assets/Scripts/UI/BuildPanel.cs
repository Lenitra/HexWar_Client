using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildPanel : MonoBehaviour
{

    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    
    [SerializeField] private Button next;
    [SerializeField] private Button previous;

    [SerializeField] GameObject buildingParent;

    private GameObject[] buildings;

    private int currentBuilding = 0;
    private int maxBuilding;

    [SerializeField] private Button buildBtn;

    [SerializeField] private PlayerControler playerControler;
    [SerializeField] private ServerClient serverClient;





    [SerializeField] private Button closeBtn;

    void Start()
    {
        next.onClick.AddListener(() => {
            buildings[currentBuilding].SetActive(false);
            currentBuilding = (currentBuilding + 1) % maxBuilding;
            buildings[currentBuilding].SetActive(true);
            title.text = buildings[currentBuilding].GetComponent<BuildUI>().name;
            description.text = buildings[currentBuilding].GetComponent<BuildUI>().description;
            updatePrice();
        });

        previous.onClick.AddListener(() => {
            buildings[currentBuilding].SetActive(false);
            currentBuilding = (currentBuilding - 1 + maxBuilding) % maxBuilding;
            buildings[currentBuilding].SetActive(true);
            title.text = buildings[currentBuilding].GetComponent<BuildUI>().name;
            description.text = buildings[currentBuilding].GetComponent<BuildUI>().description;
            updatePrice();
        });

        maxBuilding = buildingParent.transform.childCount;
        buildings = new GameObject[maxBuilding];

        for (int i = 0; i < buildingParent.transform.childCount; i++)
        {
            buildings[i] = buildingParent.transform.GetChild(i).gameObject;
        }

        closeBtn.onClick.AddListener(closeBtnClic);

        // Active the first building
        buildings[currentBuilding].SetActive(true);

        title.text = buildings[currentBuilding].GetComponent<BuildUI>().name;
        updatePrice();
        buildBtn.onClick.AddListener(() => playerControler.getFromBuildPanel(buildings[currentBuilding].GetComponent<BuildUI>().type));
    }

    void closeBtnClic()
    {
        playerControler.unselectTile();
        gameObject.SetActive(false);
    }

    void updatePrice(){
        serverClient.GetPrice(buildings[currentBuilding].GetComponent<BuildUI>().type, 1, (price) =>
        {
            buildBtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "¤" + price;
        });

    }

}
