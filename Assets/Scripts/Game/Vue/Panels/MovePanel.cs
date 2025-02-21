using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovePanel : MonoBehaviour
{
    [Header("Elements communs")]
    [SerializeField] private GridVue gridVue;
    [SerializeField] private Button closeBtn;


    private int unitsMax = 0;
    private int unitsToMove = 0;
    




    private void Start()
    {
        // TODO: Récupération des prix des bâtiments depuis le serveur pour la construction et l'upgrade

        // Setup des boutons généraux
        closeBtn.onClick.AddListener(gridVue.ClosedPanel);

        // Setup des boutons de build 
        buildBtnNext.onClick.AddListener(() => NextBuild());
        buildBtnPrevious.onClick.AddListener(() => PreviousBuild());
        buildBtnValidate.onClick.AddListener(() => ValidateBuild());

        // Setup des boutons d'upgrade
        // upgradeBtnDestroy.onClick.AddListener(() => gridVue.DestroyTile());
        upgradeBtnValidate.onClick.AddListener(() => ValidateUpgrade());
    }



    public void SetupPanel(Tile tile)
    {
        // Activer le bon body
        if (tile.Type == "")
        {
            buildBody.SetActive(true);
            upgradeBody.SetActive(false);
            SetupBuildBody();
        }
        else
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
        buildBtnValidate.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "¤ " + prices[buildIndex+1][0];
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
        gridVue.BuildPanelRetour(buildType[buildIndex]);
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
            case "hq":
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

        if ((lvlMax))
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
        gridVue.BuildPanelRetour();
    }



    #endregion



}
