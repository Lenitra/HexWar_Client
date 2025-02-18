using UnityEngine;

public class RelativeTileCanvas : MonoBehaviour
{

    [SerializeField] private GameObject lockPanel;
    [SerializeField] private GameObject lockPanelBLUR;
    [SerializeField] private PlayerControler playerControler;


    [SerializeField] private GameObject tileInfoPanel;
    [SerializeField] private TMPro.TextMeshProUGUI tileInfoText;
    private UltimateRadialMenu tileInfoMenu;
    private Tile selectedTile;

    private UltimateRadialButtonInfo tileInfoBtn1; // Attaque
    private UltimateRadialButtonInfo tileInfoBtn2; // Déplacement
    private UltimateRadialButtonInfo tileInfoBtn3; // Fermer
    private UltimateRadialButtonInfo tileInfoBtn4; // Construction


    void Start()
    {
        tileInfoMenu = tileInfoPanel.GetComponent<UltimateRadialMenu>();


        // Attk Move
        tileInfoBtn1 = new UltimateRadialButtonInfo();
        tileInfoMenu.RegisterButton( moveUnitsBtnClic, tileInfoBtn1, buttonIndex: 0 );

        // Button Move
        tileInfoBtn2 = new UltimateRadialButtonInfo();
        tileInfoMenu.RegisterButton( moveUnitsBtnClic, tileInfoBtn2, buttonIndex: 1 );

        // Button Close
        tileInfoBtn3 = new UltimateRadialButtonInfo();
        tileInfoMenu.RegisterButton(playerControler.unselectTile, tileInfoBtn3, buttonIndex: 2);

        // Button Build
        tileInfoBtn4 = new UltimateRadialButtonInfo();
        tileInfoMenu.RegisterButton( buildBtnClic, tileInfoBtn4, buttonIndex: 3 );
    }


    public void selectTile(Tile tile){
        lookTile(tile.gameObject);
        activateTileInfoPanel(tile);
        bool move, build;
        if (tile.owner == PlayerPrefs.GetString("username")){
            move = true;
            build = true;
        } else {
            move = false;
            build = false;
        }
        if (tile.type.EndsWith("5")){
            build = false;
        }
        if (tile.units <= 0){
            move = false;
        } else {
            move = true;
        }
        setUpButtons(build, move);
        
        if (selectedTile.type != ""){
            tileInfoText.text = "<sprite=0> " + selectedTile.units + "\n<sprite=2> " + DataManager.Instance.GetData(selectedTile.type.Split(':')[0].ToLower()) + "\n<sprite=1> " + selectedTile.type.Split(':')[1];
        }
        else {
            tileInfoText.text = "<sprite=0> " + selectedTile.units;
        }

        // on click to lockPanel, it's the same as clicking on the close button
        lockPanel.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(playerControler.unselectTile);
    }



    // Permet de désactiver les boutons de construction et de déplacement si besoin
    private void setUpButtons(bool build = true, bool attk = true, bool move = false){
        if (!build){
            tileInfoBtn4.DisableButton();
        } else {
            tileInfoBtn4.EnableButton();
        }
        if (!attk){
            tileInfoBtn1.DisableButton();
        } else {
            tileInfoBtn1.EnableButton();
        }
        if (!move){
            tileInfoBtn2.DisableButton();
        } else {
            tileInfoBtn2.EnableButton();
        }
    }





    private void buildBtnClic()
    {
        desactivate();
        playerControler.buildBtnClic();
        gameObject.SetActive(true);
        lockPanelBLUR.SetActive(true);
    }

    private void moveUnitsBtnClic()
    {
        desactivate();
        playerControler.moveUnitsBtnClic();
        gameObject.SetActive(true);
        lockPanelBLUR.SetActive(true);
    }


    private void lookTile(GameObject tile){
        transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 2.5f, tile.transform.position.z -0.5f);
    }

    public void activateTileInfoPanel(Tile tile){
        desactivate();
        gameObject.SetActive(true);
        selectedTile = tile;
        tileInfoPanel.SetActive(true);
        lockPanel.SetActive(true);
    }

    public void desactivate(){
        gameObject.SetActive(false);
        tileInfoPanel.SetActive(false);
        lockPanel.SetActive(false);
        lockPanelBLUR.SetActive(false);
    }


}
