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
    [SerializeField] private TextMeshProUGUI buildTextTitle;
    [SerializeField] private TextMeshProUGUI buildTextDescription;
    [SerializeField] private SpriteRenderer buildSpriteRenderer;
    private int buildIndex = 0;

    [Header("Build Body - Buildings")]
    [SerializeField] private Sprite[] buildSprite;


    [Space(10)]
    [Header("Upgrade Body")]
    [SerializeField] private Button upgradeBtnDestroy;
    [SerializeField] private Button upgradeBtnValidate;




    public void SetupPanel(bool isBuildPanel, Tile tile)
    {
        // Activer le bon body
        if (isBuildPanel)
        {
            buildBody.SetActive(true);
            upgradeBody.SetActive(false);
        }
        else
        {
            buildBody.SetActive(false);
            upgradeBody.SetActive(true);
        }
    }



    private void SetupBuildBody(Tile tile)
    {

    }




}
