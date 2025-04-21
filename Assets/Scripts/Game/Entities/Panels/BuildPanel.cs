using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildPanel : MonoBehaviour
{
    [Header("Elements communs")]
    [SerializeField] private Controller controller;
    [SerializeField] private Button closeBtn;

    [Header("Bodies")]
    [SerializeField] private GameObject buildBody;
    [SerializeField] private GameObject upgradeBody;






    [Header("Build Body")]
    [SerializeField] private Button buildBtnValidate;
    [SerializeField] private Button buildBtnNext;
    [SerializeField] private Button buildBtnPrevious;
    [SerializeField] private TextMeshProUGUI buildTextTitle;
    [SerializeField] private TextMeshProUGUI buildTextDescription;
    [SerializeField] private Image buildSpriteRenderer;
    private int buildIndex = 0;



    [Header("Build Body - Buildings")]
    [SerializeField] private Sprite[] buildSprite;



    [Space(10)]
    [Header("Upgrade Body")]
    [SerializeField] private Button upgradeBtnDestroy;
    [SerializeField] private Button upgradeBtnValidate;
    [SerializeField] private TextMeshProUGUI upgradeTextTitle;
    [SerializeField] private TextMeshProUGUI upgradeTextDescription;


    [Header("Upgrade Body - Buildings")]
    private string[] upgradeType = new string[] { "Node", "Excavateur", "Usine de drones", "Radar" };
    private string[] buildTitles = new string[] {"Node", "Excavateur", "Usine de drones", "Radar" };
    private string[] buildType = new string[] {"Node", "Miner", "Barrack", "Radar" };
    private string[] buildDescriptions = new string[] {
        "Permet de gérer un secteur",
        "Génère des nanites",
        "Construit des drones",
        "Augmente portée de vision de l'hexagone"
    };


    // Prix des upgrades dans lordre : QG, Excavateur, Usine de drones, Radar
    private string[][] prices = new string[][] {
        new string[] { "0", "500", "1500", "3000", "5000" }, // HQ
        new string[] { "75", "300", "500", "800", "1500" }, // Miner
        new string[] { "150", "300", "500", "800", "1500" }, // Barrack
        new string[] { "100", "300", "500", "800", "1500" } // Radar
    };


    private Tile tileSelected;



    private void Start()
    {
        // TODO: Récupération des prix des bâtiments depuis le serveur pour la construction et l'upgrade

        // Setup des boutons généraux
        closeBtn.onClick.AddListener(ClosePanel);

        // Setup des boutons de build 
        buildBtnNext.onClick.AddListener(() => NextBuild());
        buildBtnPrevious.onClick.AddListener(() => PreviousBuild());
        buildBtnValidate.onClick.AddListener(() => ValidateBuild());

        // Setup des boutons d'upgrade
        // TODO: (ligne commentée ci-dessous) : à faire si on veut pouvoir détruire un bâtiment
        // upgradeBtnDestroy.onClick.AddListener(() => gridVue.BuildPanelDestroy());
        upgradeBtnValidate.onClick.AddListener(() => ValidateUpgrade());
    }


    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }


    public void SetupPanel(Tile tile)
    {
        tileSelected = tile;

        // Activer le bon body
        if (tile.Type == "")
        {
            buildBody.SetActive(true);
            upgradeBody.SetActive(false);
            SetupBuildBody();
        }
        else // si il y a déjà une construction sur la tile        
        {
            upgradeBody.SetActive(true);
            buildBody.SetActive(false);
            SetupUpgradeBody(tile.Type, tile.Lvl);
        }
    }


    #region Build Body

    private void SetupBuildBody()
    {
        buildTextTitle.text = buildTitles[buildIndex];
        buildTextDescription.text = buildDescriptions[buildIndex];
        buildSpriteRenderer.sprite = buildSprite[buildIndex];
        buildBtnValidate.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "¤ " + prices[buildIndex][0];
    }


    private void NextBuild()
    {
        buildIndex++;
        if (buildIndex >= buildTitles.Length)
        {
            buildIndex = 0;
        }

        SetupBuildBody();
    }

    private void PreviousBuild()
    {
        buildIndex--;
        if (buildIndex < 0)
        {
            buildIndex = buildTitles.Length - 1;
        }

        SetupBuildBody();
    }


    private void ValidateBuild()
    {
        controller.BuildTile(tileSelected, buildType[buildIndex]);
        gameObject.SetActive(false);
    }

    #endregion




    #region Upgrade Body
    private void SetupUpgradeBody(string type, int lvl)
    {
        int index = -1;
        string descr = "";
        bool lvlMax = false;

        switch (type.ToLower())
        {
            case "node":
                index = 0;
                descr = "Augmente les le niveau maximum des améliorations";
                lvlMax = lvl >= prices[index].Length;
                if (!lvlMax)
                {
                    descr += "\nNiv. " + lvl + " -> " + (lvl + 1);
                }
                break;
            case "miner":
                index = 1;
                descr = "Augmente la production de nanites";
                lvlMax = lvl >= prices[index].Length;
                if (!lvlMax)
                {
                    descr += "\nNiv. " + lvl + " -> " + (lvl + 1);
                    descr += "\nProd. " + lvl + "/h -> " + (lvl + 1) + "/h";
                }
                break;
            case "barrack":
                index = 2;
                descr = "Augmente la vitesse de production des drones";
                lvlMax = lvl >= prices[index].Length;
                if (!lvlMax)
                {
                    descr += "\nNiv. " + lvl + " -> " + (lvl + 1);
                    descr += "\nProd. " + lvl + "/h -> " + (lvl + 1) + "/h";
                }
                break;
            case "radar":
                index = 3;
                descr = "Augmente la portée de vision";
                lvlMax = lvl >= prices[index].Length;
                if (!lvlMax)
                {
                    descr += "\nNiv. " + lvl + " -> " + (lvl + 1);
                    descr += "\nPortée " + (2 + lvl) + " -> " + (3 + lvl);
                }
                break;
        }
        
        if (lvlMax)
        {
            upgradeBtnValidate.interactable = false;
            upgradeBtnValidate.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "MAX";
        }
        else
        {
            upgradeBtnValidate.interactable = true;
            upgradeBtnValidate.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "¤ " + prices[index][lvl];
        }


        upgradeTextTitle.text = "Amélioration\n" + upgradeType[index];
        upgradeTextDescription.text = descr;

    }



    private void ValidateUpgrade()
    {
        controller.BuildTile(tileSelected, upgradeType[0]);
        ClosePanel();
    }



    #endregion



}
