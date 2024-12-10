using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class PlayerControler : MonoBehaviour
{
    [SerializeField] private RelativeTileCanvas tileRelativeCanvas;
    [SerializeField] private GameObject movePanel;
    [SerializeField] private GameObject buildPanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI stateInfoText;

    private GridGenerator gridGenerator;
    private GameManager gameManager;
    private CamController camControler;

    private Vector3 previousMousePosition;
    public float timeClicked = 0;

    private string state = "";
    private GameObject selectedTile;
    private int selectedUnits;

    public float tmpdist = 0;
    private bool skipNextClick = false; // Ce booléen va servir à ignorer le prochain clic

    void Start()
    {
        gridGenerator = GetComponent<GridGenerator>();
        gameManager = GetComponent<GameManager>();
        camControler = Camera.main.GetComponent<CamController>();
        previousMousePosition = Input.mousePosition;

        movePanel.gameObject.SetActive(false);
        buildPanel.gameObject.SetActive(false);
    }

    void Update()
    {

        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            timeClicked += Time.deltaTime;
            tmpdist += Vector3.Distance(Input.mousePosition, previousMousePosition);
            previousMousePosition = Input.mousePosition;
        }



        if (Input.GetMouseButtonUp(0))
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
    }

    public void buildBtnClic()
    {
        if (selectedTile != null)
        {
            if (selectedTile.GetComponent<Tile>().type == "")
            {
                buildPanel.gameObject.SetActive(true);
            }
            else if (!selectedTile.GetComponent<Tile>().type.EndsWith("5"))
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


    // TODO: Créer un panel d'infos pour les tuiles et appliquer le texte ici
    public void infosTileBtnClic()
    {
        if (selectedTile != null)
        {
            string msg = "Infos de la tuile " + selectedTile.name + "\n";
            msg += "Owner : " + selectedTile.GetComponent<Tile>().owner + "\n";
            msg += "Units : " + selectedTile.GetComponent<Tile>().units + "\n";
            msg += "Type : " + selectedTile.GetComponent<Tile>().type ;
            Debug.Log(msg);
        }
    }
    

    public void getFromMovePanel(int units)
    {
        movePanel.gameObject.SetActive(false);
        state = "move";
        selectedUnits = units;
        stateInfoText.text = "Vous avez sélectionné " + selectedUnits + " unités. Cliquez sur une case pour les déplacer.";
        tileRelativeCanvas.desactivate();
    }

    public void getFromBuildPanel(string type = "")
    {
        if (type == "")
        {
            type = selectedTile.GetComponent<Tile>().type.Split(":")[0];
        }

        buildPanel.gameObject.SetActive(false);
        upgradePanel.gameObject.SetActive(false);
        gameManager.buildBtnClic(selectedTile.name.Split(' ')[1], type);
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
