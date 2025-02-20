using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildPanel : MonoBehaviour
{
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
    private string[] buildTitles = new string[] { "Excavateur", "Usine de drones", "Radar"};
    private string[] buildDescriptions = new string[] { 
        "Génère des nanites", 
        "Construit des drones", 
        "Augmente portée de vision de l'hexagone"
    };

    [Space(10)]
    [Header("Upgrade Body")]
    [SerializeField] private Button upgradeBtnDestroy;
    [SerializeField] private Button upgradeBtnValidate;





    private void Start()
    {
        buildBtnNext.onClick.AddListener(() => NextBuild());
        buildBtnPrevious.onClick.AddListener(() => PreviousBuild());
        buildBtnValidate.onClick.AddListener(() => ValidateBuild());
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
            buildBody.SetActive(false);
            upgradeBody.SetActive(true);
        }
    }


    #region Build Body

    private void SetupBuildBody()
    {
        buildTextTitle.text = buildTitles[buildIndex];
        buildTextDescription.text = buildDescriptions[buildIndex];
        buildSpriteRenderer.sprite = buildSprite[buildIndex];
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

    }

    #endregion


}
