using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;



public class Controller : MonoBehaviour
{

    private GameManager gameManager; // Référence au Model
    private CamController camController; // Référence au CamController


    // Références aux tiles
    private Tile hoverTile; // Tile survolée par la souris
    private Tile selectedTile; // Tile sélectionnée par le joueur
    private bool secondSelection = false; // Pour savoir si on doit faire une deuxième sélection de tile (pour le déplacement par exemple)
    private Tile secondSelectedTile; // Deuxième tile sélectionnée par le joueur (pour le déplacement par exemple)


    [Header("Références")]
    [SerializeField] private TilePanel tilePanelHover; // Panneau d'info d'une tile survolée
    [SerializeField] private TilePanel tilePanelSelected; // Panneau d'info d'une tile survolée


    [Header("Références aux boutons")]
    [SerializeField] private Button buildBtn; // Bouton de construction
    [SerializeField] private Button moveBtn; // Bouton de déplacement
    [SerializeField] private Button rallyBtn; // Bouton de ralliement
    [SerializeField] private Button dispatchBtn; // Bouton de ralliement


    [Header("Panels d'interaction")]
    [SerializeField] private BuildPanel buildPanel; // Panel de construction
    [SerializeField] private MovePanel movePanel; // Panel de déplacement
    [SerializeField] private RallyPanel rallyPanel; // Panel de ralliement
    [SerializeField] private DispatchPanel dispatchPanel; // Panel de ralliement









    // Setters

    // Gère le changement de la tile sélectionnée par le joueur

    public void SetSelectedTile(Tile tile)
    {
        // Cas ou tile est null
        // Cas ou tile appartient pas au joueur 
        // Cas ou tile est la même que la tile déjà sélectionnée => SelectedTile == null 

        if (tile != null)
        {
            if (tile.Owner != PlayerPrefs.GetString("username"))
            {
                tile = null;
            }
            else if (tile == selectedTile)
            {
                tile = null;
            }
            else
            {
                if (selectedTile != null)
                {
                    StartCoroutine(selectedTile.UnSelectCoroutine());
                }
                selectedTile = tile;
                tilePanelSelected.SetInfoTilePanel(selectedTile);
                tilePanelSelected.gameObject.SetActive(true);
                StartCoroutine(selectedTile.SelectCoroutine());
            }
        }

        if (tile == null)
        {
            if (selectedTile != null)
            {
                StartCoroutine(selectedTile.UnSelectCoroutine());
            }
            selectedTile = null;
            tilePanelSelected.gameObject.SetActive(false);
        }
    }


    // Gère le changement de la tile survolée par la souris 
    // et met à jour le panneau d'info (tilePanelHover)
    public void SetHoverTile(Tile tile)
    {

        if (hoverTile != null)
        {
            hoverTile.UnHighlightTile();
        }
        hoverTile = tile;
        if (hoverTile != null)
        {
            if (tile.Owner == PlayerPrefs.GetString("username"))
                hoverTile.HighlightTile();
            tilePanelHover.SetInfoTilePanel(hoverTile);
            tilePanelHover.gameObject.SetActive(true);
        }
        else
        {
            tilePanelHover.gameObject.SetActive(false);
        }

    }








    void Start()
    {
        gameManager = GetComponent<GameManager>();
        camController = Camera.main.GetComponent<CamController>();

        // Setup des boutons généraux
        buildBtn.onClick.AddListener(() => BuildBtnClick());
        moveBtn.onClick.AddListener(() => MoveBtnClick());
        // rallyBtn.onClick.AddListener(() => RallyBtnClick());
        // dispatchBtn.onClick.AddListener(() => DispatchBtnClick());
    }




    #region Gestion des boutons

    #region Build
    private void BuildBtnClick()
    {
        // Si on a pas de tile sélectionnée, on ne fait rien
        if (selectedTile == null) return;

        // On ouvre le panel de construction/upgrade
        buildPanel.SetupPanel(selectedTile);
        buildPanel.gameObject.SetActive(true);
    }

    public void BuildPanelValidate()
    {
        // TODO: envoyer la commande de construction au serveur
    }

    #endregion

    #region Move
    private void MoveBtnClick()
    {
        // Si on a pas de tile sélectionnée, on ne fait rien
        if (selectedTile == null) return;

        secondSelection = true; // On active le mode de sélection de la deuxième tile
        
    }
    #endregion

    #region Rally
    #endregion

    #region Dispatch
    #endregion

    #endregion







    #region update()
    private void Update()
    {

        // Vérification supplémentaire pour éviter les conflits avec le drag de la caméra
        if (
            Input.GetMouseButtonUp(0) // Si on clique
            && camController.isDragging == false // Si on est pas en train de bouger la caméra
            && !EventSystem.current.IsPointerOverGameObject() // Si on n'est pas sur un UI
        )
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit touch))
            {
                if (!secondSelection){
                    SetSelectedTile(touch.collider.GetComponent<Tile>());
                }
                else{
                    secondSelectedTile = touch.collider.GetComponent<Tile>();
                }
            }
            else
            {
                if (!secondSelection)
                    // Si on clique en dehors de la carte, on désélectionne la tile
                    SetSelectedTile(null);
            }
        }

        // TODO
        #region TODO: gestion clavier pour les boutons de construction, de déplacement et de ralliement

        // // Simuler une selection de tile et un click sur le bouton de construction avec la touche B
        // if (Input.GetKeyDown(KeyCode.Q))
        // {
        //     if (presenteurCarte.SelectTile == null)
        //     {
        //         presenteurCarte.TraiterClick(Input.mousePosition);
        //         BuildBtnClick();
        //     }
        // }

        // // Simuler une selection de tile et un click sur le bouton de déplacement avec la touche E
        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     if (presenteurCarte.SelectTile == null)
        //     {
        //         presenteurCarte.TraiterClick(Input.mousePosition);
        //         MoveBtnClick();
        //     }
        // }

        // // Simuler une selection de tile et un click sur le bouton de ralliement avec la touche R
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     if (presenteurCarte.SelectTile == null)
        //     {
        //         presenteurCarte.TraiterClick(Input.mousePosition);
        //         RallyBtnClick();
        //     }
        // }

        #endregion

        // Si la souris est en hover sur une tile
        if (
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)
            && hit.collider.GetComponent<Tile>() != null // Si on est en train de survoler une tile
            && camController.isDragging == false // Si on est pas en train de bouger la caméra
            && !EventSystem.current.IsPointerOverGameObject() // Si on n'est pas sur un UI
            )
        {
            // Si la tile survolée est différente de la tile actuelle
            // (pour éviter de faire des appels inutiles)
            if (hoverTile != hit.collider.GetComponent<Tile>())
            {
                SetHoverTile(hit.collider.GetComponent<Tile>());
            }

            tilePanelHover.SetPos(Input.mousePosition);

        }
        else if (hoverTile != null)
        {
            SetHoverTile(null);
        }
    }





    #endregion



}
