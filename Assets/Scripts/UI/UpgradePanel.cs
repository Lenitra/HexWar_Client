using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePanel : MonoBehaviour
{

    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Button buildBtn;
    [SerializeField] private PlayerControler playerControler;
    [SerializeField] private ServerClient serverClient;
    [SerializeField] private Button closeBtn;
    private Tile selectedTile;

    void Start()
    {

        closeBtn.onClick.AddListener(closeBtnClic);

        buildBtn.onClick.AddListener(() => playerControler.getFromBuildPanel());
    }
    
    private void closeBtnClic()
    {
        playerControler.unselectTile();
        gameObject.SetActive(false);
    }

    public void Initialise(Tile selectedTile)
    {
        this.selectedTile = selectedTile;
        
        
        string build = selectedTile.type.Split(':')[0];
        int lvl = int.Parse(selectedTile.type.Split(':')[1]);
        string completedesc = build.ToLower() + "Infos" + (lvl + 1);
        title.text = DataManager.Instance.GetData(build.ToLower()) + " lvl." + (lvl);
        description.text = DataManager.Instance.GetData(completedesc);
        // playerControler.getPrice(build, lvl);
        // update the first child's text of buildBtn

        serverClient.GetPrice(build, lvl+1, (price) =>
        {

            buildBtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "¤" + price;
        });

        title.text = build + " lvl " + lvl.ToString();
    }
}
