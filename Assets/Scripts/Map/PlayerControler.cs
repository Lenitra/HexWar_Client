using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Rendering.PostProcessing;

public class PlayerControler : MonoBehaviour
{
    [SerializeField] private RelativeTileCanvas tileRelativeCanvas;
    
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private GameObject movePanel;
    [SerializeField] private GameObject buildPanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI stateInfoText;
    [SerializeField] private GameObject stateInfoPanel;

        // UI
    [Header("HUD")]
    [SerializeField] private Button seeAllUnitsBtn;
    [SerializeField] private Button seeHQ;
    [SerializeField] private Button showOptionBtn;
    [SerializeField] private TMP_Text moneyText;

    private GridGenerator gridGenerator;
    private GameManager gameManager;
    private CamController camControler;

    private Vector3 previousMousePosition;
    private float timeClicked = 0;

    private string state = "";
    private GameObject selectedTile;
    private int selectedUnits;

    private float tmpdist = 0;
    private bool skipNextClick = false; // Ce booléen va servir à ignorer le prochain clic
    private bool seeAllUnitsBool = false;
    private float afkFrom = 0; // in seconds
    private float afkMax = 2.5f*60; // in seconds
    private float afkDisconect = 20*60; // in seconds
    private bool isafk = false;

    void Start()
    {
        gridGenerator = GetComponent<GridGenerator>();
        gameManager = GetComponent<GameManager>();
        camControler = Camera.main.GetComponent<CamController>();
        previousMousePosition = Input.mousePosition;

        seeHQ.onClick.AddListener(seeHQBtnClic);
        seeAllUnitsBtn.onClick.AddListener(seeAllUnits);
        showOptionBtn.onClick.AddListener(showOptionPanel);

        movePanel.gameObject.SetActive(false);
        buildPanel.gameObject.SetActive(false);
        upgradePanel.gameObject.SetActive(false);
        stateInfoPanel.SetActive(false);
        optionPanel.SetActive(false);

        setupOptions();
    }


    public void setupOptions()
    {
        if (PlayerPrefs.HasKey("opt_gridGap")) // Distance entre les tuiles
        {
            if (PlayerPrefs.GetFloat("opt_gridGap") < 0.1f || PlayerPrefs.GetFloat("opt_gridGap") > 1.0f)
            {
                PlayerPrefs.SetFloat("opt_gridGap", 0.1f);
            }
            gridGenerator.gridGap = PlayerPrefs.GetFloat("opt_gridGap");
        }

        if (PlayerPrefs.HasKey("opt_hexesColor")) // Couleur des tuiles
        {
            gridGenerator.hexesColor = PlayerPrefs.GetInt("opt_hexesColor") == 1;
        }

        if (PlayerPrefs.HasKey("opt_bloom")) // Flou lumineux
        {
            // Change the bloom intensity from the post process volume
            if (PlayerPrefs.GetFloat("opt_bloom") < 0.0f || PlayerPrefs.GetFloat("opt_bloom") > 2.0f)
            {
                PlayerPrefs.SetFloat("opt_bloom", 1.0f);
            }
            // get the post process volume
            PostProcessVolume volume = Camera.main.GetComponent<PostProcessVolume>();
            if (volume.profile.TryGetSettings(out Bloom bloom))
            {
                // Récupérez la valeur et appliquez-la
                bloom.intensity.value = PlayerPrefs.GetFloat("opt_bloom", 1.0f); // 1.0f est la valeur par défaut
            }
        }



        gridGenerator.destroyGrid();
    }

    public void UpdateMoney(string money)
    {
        // Mettre à jour l'argent du joueur
        moneyText.text = "¤ " + money;
    }


    public void showOptionPanel()
    {
        optionPanel.SetActive(true);
    }

    public void seeAllUnits(){
        seeAllUnitsBool = !seeAllUnitsBool;
        foreach (Transform child in transform){
            child.gameObject.GetComponent<Tile>().moreInfo.SetActive(seeAllUnitsBool);
        }
    }


    public void seeHQBtnClic(){
        if (selectedTile != null){
            unselectTile();
            tileRelativeCanvas.desactivate();
        }
        // loop through all children
        foreach (Transform child in transform){
            if (child.gameObject.GetComponent<Tile>().type.ToUpper() == "HQ" && child.gameObject.GetComponent<Tile>().owner == PlayerPrefs.GetString("username")){
                camControler.lookTile(child.gameObject);
                return;
            }
        }
    }


    void Update()
    {
        

        gameManager.togglePooling(!isafk);

        afkFrom += Time.deltaTime;
        if (Input.anyKey)
        {
            Debug.Log("No longer AFK");
            afkFrom = 0;
            isafk = false;
        }
        else
        {
            
            if (afkFrom > afkMax)
            {
                Debug.Log("AFK");
                isafk = true;
            } 
            if (afkFrom > afkDisconect)
            {
                Debug.Log("AFK for too long");
                // go to scene Home
                UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
            }
        }


        

        // quand une tuile est selectionnée
        if (selectedTile != null && camControler.canMove)
        {
            camControler.setCanMove(false);
        }

        // quand une tuile est selectionnée etqu'on est en state move (déplacement d'unités)
        // on autorise le déplacement de la caméra
        else if (selectedTile != null && state == "move")
        {
            camControler.setCanMove(true);
        }

        // Quand on déselectionne une tuile
        else if (selectedTile == null)
        {
            camControler.setCanMove(true);
        }


        if (stateInfoText.text != "")
        {
            // stateInfoText.gameObject.SetActive(true);
            stateInfoPanel.SetActive(true);
        }
        else
        {
            stateInfoPanel.SetActive(false);
            // stateInfoText.gameObject.SetActive(false);
        }

        #region Gestion des events de la souris et du clavier
        if (Input.GetMouseButton(0))
        {
            timeClicked += Time.deltaTime;
            tmpdist += Vector3.Distance(Input.mousePosition, previousMousePosition);
            previousMousePosition = Input.mousePosition;
        }


        // Quand on appuie sur A
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (state == "move" && selectedTile != null)
            {
                unselectTile();
            }
            else if (state == "" && selectedTile == null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {

                    // Vérifier si la tuile appartient au joueur
                    if (PlayerPrefs.GetString("username") == hit.collider.gameObject.GetComponent<Tile>().owner && hit.collider.gameObject.GetComponent<Tile>().units > 0)
                    {
                        selectTile(hit.collider.gameObject);
                        // comme si on avait cliqué sur le bouton move
                        moveUnitsBtnClic();
                        // comme si on avait cliqué sur le bouton de validation du formulaire
                        getFromMovePanel(hit.collider.gameObject.GetComponent<Tile>().units);
                    }
                }
            }
        }

        // Quand on appuie sur B
        else if (Input.GetKeyDown(KeyCode.B))
        {
            if (state == "" && selectedTile == null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {

                    // Vérifier si la tuile appartient au joueur
                    if (PlayerPrefs.GetString("username") == hit.collider.gameObject.GetComponent<Tile>().owner)
                    {
                        selectTile(hit.collider.gameObject);
                        tileRelativeCanvas.desactivate();
                        // comme si on avait cliqué sur le bouton build
                        buildBtnClic();
                    }
                }
            }
        }



        // gestion du clic 
        else if (Input.GetMouseButtonUp(0))
        {
            // Si on a marqué que le prochain clic doit être ignoré
            if (skipNextClick)
            {
                skipNextClick = false; // Réinitialiser pour le clic suivant
                timeClicked = 0;
                tmpdist = 0;
                return; // On ignore ce clic entièrement
            }

            if (timeClicked < 0.25f)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    timeClicked = 0;
                    tmpdist = 0;
                    return;
                }

                if (state == "")
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        
                        if (selectedTile == null)
                        {
                            // Vérifier si la tuile appartient au joueur
                            if (PlayerPrefs.GetString("username") == hit.collider.gameObject.GetComponent<Tile>().owner)
                            {
                                selectTile(hit.collider.gameObject);
                            }
                        }
                        else
                        {
                            unselectTile();
                        }
                    }
                }
                else if (state == "move")
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    camControler.canMove = true;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.gameObject.CompareTag("Tile"))
                        {
                            gameManager.moveUnitsBtnClic(selectedTile.name.Split(' ')[1], hit.collider.gameObject.name.Split(' ')[1], selectedUnits);
                            unselectTile();
                        }
                    }
                }
            }

            timeClicked = 0;
            tmpdist = 0;
        }
        #endregion
    }


    #region Fonctions de gestion des boutons dans le menu radial

    public void buildBtnClic()
    {
        if (selectedTile != null)
        {
            if (selectedTile.GetComponent<Tile>().type == "")
            {
                buildPanel.gameObject.SetActive(true);
            }
            else if (!selectedTile.GetComponent<Tile>().lvl == 5)
            {
                upgradePanel.gameObject.GetComponent<UpgradePanel>().Initialise(selectedTile.GetComponent<Tile>());
                upgradePanel.gameObject.SetActive(true);
            }
        }
    }

    
    public void moveUnitsBtnClic()
    {
        if (selectedTile != null && selectedTile.GetComponent<Tile>().units > 0)
        {
            movePanel.gameObject.SetActive(true);
            movePanel.GetComponent<MovePanel>().init(selectedTile.GetComponent<Tile>().units);
        }
    }


    #endregion
    

    // Validation du formulaire de nombre d'units à déplacer
    public void getFromMovePanel(int units)
    {
        movePanel.gameObject.SetActive(false);
        state = "move";
        selectedUnits = units;
        stateInfoText.text = "Vous avez sélectionné " + selectedUnits + " unités. Cliquez sur une case pour les déplacer.";
        tileRelativeCanvas.desactivate();
    }

    // Validation du formulaire de construction
    public void getFromBuildPanel(string type = "")
    {
        if (type == "")
        {
            type = selectedTile.GetComponent<Tile>().type;
        }
        int lvl = selectedTile.GetComponent<Tile>().lvl;
        

        buildPanel.gameObject.SetActive(false);
        upgradePanel.gameObject.SetActive(false);
        gameManager.buildBtnClic(selectedTile.name.Split(' ')[1], type, lvl);
        unselectTile();
    }


    public void selectTile(GameObject tile)
    {
        selectedTile = tile;
        selectedTile.GetComponent<Tile>().select();
        camControler.lookTile(selectedTile);
        StartAnimatingTileInfoPanel(true);
    }

    public void unselectTile()
    {
        if (selectedTile != null)
        {
            selectedTile.GetComponent<Tile>().unselect();
            selectedTile = null;
        }
        stateInfoText.text = "";
        state = "";
        StartAnimatingTileInfoPanel(false);

        // Ici, on signale qu'on a fermé le menu => le prochain clic doit être ignoré
        skipNextClick = true;
    }

    public void StartAnimatingTileInfoPanel(bool show)
    {
        if (show && selectedTile != null)
        {
            tileRelativeCanvas.selectTile(selectedTile.GetComponent<Tile>());
        }
        else
        {
            tileRelativeCanvas.desactivate();
        }
    }




}
