using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GridVue : MonoBehaviour
{

    private const float gridGap = 0f;
    // private const float hexSize = 1.012f;
    private const float hexSize = 1f;
    private PresenteurCarte presenteurCarte;
    private CamController camController;


    // Prefab de l'hexagone
    [SerializeField] private GameObject tilePrefab;

    // Panel d'infos sur en hover d'une tile
    [SerializeField] private TilePanel tilePanelHover;

    // Panel d'infos sur l'état des inputs
    [SerializeField] private GameObject stateInfos;

    [Space(10)]
    [Header("Elements d'effets temporaires")]
    [SerializeField] private LineRenderer prefabLine;
    [SerializeField] private GameObject prefabSphere;

    [Space(10)]
    [Header("UI Tiles")]
    [SerializeField] private TilePanel tilePanel;
    [SerializeField] private Button buildBtn;
    [SerializeField] private Button moveBtn;
    [SerializeField] private Button rallyBtn;
    [SerializeField] private Button configBuildBtn;

    [Space(2)]
    [Header("Panels Tiles")]
    [SerializeField] private BuildPanel buildPanel;
    [SerializeField] private MovePanel movePanel;
    [SerializeField] private RallyPanel rallyPanel;

    // [SerializeField] private ConfigBuildPanel configBuildPanel;


    private Tile hoverTile = null;



    private void Start()
    {
        presenteurCarte = GetComponent<PresenteurCarte>();
        camController = Camera.main.GetComponent<CamController>();

        // Listeners des boutons de tiles
        buildBtn.onClick.AddListener(() => BuildBtnClick());
        // configBuildBtn.onClick.AddListener(() => EnableGameObject(configBuildPanel.gameObject));

        rallyBtn.onClick.AddListener(() => RallyBtnClick());

        moveBtn.onClick.AddListener(() => MoveBtnClick());
    }



    // Gestions des clicks sur la grille et des hovers
    private void Update()
    {



        // Vérification supplémentaire pour éviter les conflits avec le drag de la caméra
        if (
            Input.GetMouseButtonUp(0) // Si on clique
            && camController.isDragging == false // Si on est pas en train de bouger la caméra
            && !EventSystem.current.IsPointerOverGameObject()
        )
        {
            presenteurCarte.TraiterClick(Input.mousePosition);
        }

        // gestion des touches clavier

        // Simuler une selection de tile et un click sur le bouton de construction avec la touche B
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (presenteurCarte.SelectTile == null)
            {
                presenteurCarte.TraiterClick(Input.mousePosition);
                BuildBtnClick();
            }
        }

        // Simuler une selection de tile et un click sur le bouton de déplacement avec la touche E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (presenteurCarte.SelectTile == null)
            {
                presenteurCarte.TraiterClick(Input.mousePosition);
                MoveBtnClick();
            }
        }

        // Simuler une selection de tile et un click sur le bouton de ralliement avec la touche R
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (presenteurCarte.SelectTile == null)
            {
                presenteurCarte.TraiterClick(Input.mousePosition);
                RallyBtnClick();
            }
        }




        if (presenteurCarte.State != "move")
        {

            // Hover avec raycast
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {


                Tile newHoverTile = hit.collider.GetComponent<Tile>();
                if (newHoverTile != null)
                {

                    SetHoverPanelPosition(Input.mousePosition);
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        // regarder si on est pas au survol du tilePanelHover

                        tilePanelHover.gameObject.SetActive(false);
                        return;
                    }

                    // Si on est en hover d'une nouvelle tile, on déselectionne l'ancienne
                    if (hoverTile != null)
                    {
                        hoverTile.UnHighlightTile();
                        tilePanelHover.gameObject.SetActive(false);
                    }
                    hoverTile = newHoverTile;
                    ShowHoverPanel(hoverTile);
                    if (hoverTile.Owner == PlayerPrefs.GetString("username"))
                    {
                        hoverTile.HighlightTile();
                    }
                }
            }
            else
            {
                if (hoverTile != null)
                {
                    tilePanelHover.gameObject.SetActive(false);
                    hoverTile.UnHighlightTile();
                    hoverTile = null;
                }
            }
        }
    }



    #region Génération de la grille

    // Placement d'un hex sur la grille et ajout de ses attributs
    public void CreerTile(Hex hexData)
    {
        float[] coords = GetHexCoordinates(hexData.x, hexData.y);
        GameObject tile = Instantiate(tilePrefab, new Vector3(coords[0], -5, coords[1]), Quaternion.identity);
        // set the parent of this tile to the grid
        tile.transform.SetParent(transform);
        // set le nom de l'objet
        tile.name = $"HexObj {hexData.x}:{hexData.y}";
        tile.GetComponent<Tile>().SetupTile(hexData);
        StartCoroutine(CreerTileAnim(tile));

        // Si le hex est le hq du joueur, bouger la caméra vers lui
        if (hexData.owner == PlayerPrefs.GetString("username")
            && hexData.type.ToLower() == "hq")
        {
            Camera.main.GetComponent<CamController>().moveCamToTile(coords[0], coords[1], false);
        }

    }


    public void SupprimerTile(GameObject tile)
    {
        StartCoroutine(DestroyTileAnim(tile));
    }


    public void ModifierTile(Hex hexData, Tile tile)
    {
        tile.SetupTile(hexData);
    }


    // Convertit les coordonnées d'un hex en coordonnées pixel
    public float[] GetHexCoordinates(int x, int z)
    {
        // Axial -> Pixel (flat top)
        float px = (1.5f * hexSize + gridGap) * x;
        float pz = (Mathf.Sqrt(3f) * hexSize + gridGap) * (z + x / 2f);

        return new float[] { px, pz };
    }


    #endregion




    #region Gestion de l'affichage des différents panels


    private void DisableAllPanels()
    {
        DisableGameObject(tilePanel.gameObject);
        DisableGameObject(buildPanel.gameObject);
        DisableGameObject(movePanel.gameObject);
        DisableGameObject(rallyPanel.gameObject);
    }


    private void DisableGameObject(GameObject go)
    {
        go.SetActive(false);
    }


    // Quand un panneau se ferme, on désactive tous les autres et on désélectionne la tile
    public void ClosedPanel()
    {
        DisableAllPanels();
        presenteurCarte.SelectTile = null;

    }

    // Gestion du placement et affichage du hoverpanel
    public void ShowHoverPanel(Tile tile)
    {
        tilePanelHover.gameObject.SetActive(true);

        string title = $"{tile.Owner}";
        if (tile.Owner == "")
        {
            title = "(Neutre)";
        }
        string coords = $"({tile.X}, {tile.Y})";
        string desc = "";
        if (tile.Owner == PlayerPrefs.GetString("username"))
        {
            desc = $"Type: {tile.Type}\nLvl: {tile.Lvl}\nUnits: {tile.Units}";
            tilePanelHover.SetInfoTilePanel(title, coords, desc);
        }
        else
        {
            if (tile.Type != "")
            {
                desc = $"Type: {tile.Type}\nLvl: {tile.Lvl}";
            }
            tilePanelHover.SetInfoTilePanel(title, coords, desc);
        }
        tilePanelHover.SetInfoTilePanel(title, coords, desc);
    }

    // Placement du tilePanelHover aux position de la souris
    public void SetHoverPanelPosition(Vector3 pos)
    {
        tilePanelHover.transform.GetChild(0).position = new Vector3(pos.x + 76, pos.y - 40, 2);
    }

    #endregion





    #region Gestion des clicks sur les boutons de tiles


    #region Bouton de construction de tiles
    private void BuildBtnClick()
    {
        DisableAllPanels();
        buildPanel.gameObject.SetActive(true);
        buildPanel.SetupPanel(presenteurCarte.SelectTile);
    }

    public void BuildPanelRetour(string type = "")
    {
        presenteurCarte.BuildTile(type);
        ClosedPanel();
    }

    #endregion


    #region Bouton de déploiement d'unités
    private void MoveBtnClick()
    {
        DisableAllPanels();
        presenteurCarte.State = "move";
        HighlightTiles(presenteurCarte.GetTilesToMove());
    }

    public void DisplayMovePanel(Tile tile1, Tile tile2)
    {
        DisableAllPanels();
        movePanel.gameObject.SetActive(true);
        movePanel.SetupPanel(tile1, tile2);
    }

    public void MovePanelRetour(int units)
    {
        if (units > 0)
        {
            presenteurCarte.MoveUnits(units);
            DisableAllPanels();
        }
        else
        {
            ClosedPanel();
        }
    }

    #endregion



    #region Bouton de ralliement

    private void RallyBtnClick()
    {
        DisableAllPanels();
        rallyPanel.gameObject.SetActive(true);
        rallyPanel.SetupPanel(presenteurCarte.GetTotalUnitCount(), $"{presenteurCarte.SelectTile.X}:{presenteurCarte.SelectTile.Y}");
    }

    public void ValidateRally()
    {
        presenteurCarte.RallyUnits();
        ClosedPanel();
    }

    #endregion




    #endregion




    #region Gestion de l'affichage de l'état/State des inputs sur la grille
    public void SetupStateInfos(string state)
    {
        switch (state)
        {
            case "move":
                // Désactiver le menu de la tile 
                DisableAllPanels();
                stateInfos.GetComponentInChildren<TextMeshProUGUI>().text = "Selectionnez un hexagone vers lequel déplacer vos unités";
                stateInfos.SetActive(true);
                break;
            default:
                stateInfos.GetComponentInChildren<TextMeshProUGUI>().text = "";
                stateInfos.SetActive(false);
                break;
        }
    }


    #endregion


    #region Coroutines d'animations de tiles
    // Créé un nouvel hexagone avec une animation de pop
    private IEnumerator CreerTileAnim(GameObject tile)
    {
        float duration = 0.5f;
        float t = 0;
        Vector3 startPos = tile.transform.position;
        Vector3 endPos = new Vector3(tile.transform.position.x, 0, tile.transform.position.z);
        while (t < duration)
        {
            t += Time.deltaTime;
            tile.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
        tile.transform.position = endPos;
    }


    // Détruit un hexagone avec une animation de pop
    private IEnumerator DestroyTileAnim(GameObject tile)
    {
        if (tile == null) { yield break; }
        float duration = 0.5f;
        float t = 0;
        Vector3 startPos = tile.transform.position;
        Vector3 endPos = new Vector3(tile.transform.position.x, -5, tile.transform.position.z);
        while (t < duration)
        {
            t += Time.deltaTime;
            tile.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
        Destroy(tile);
    }


    #endregion



    #region Selection de tiles
    // Sélection d'une tuile
    public void SelectTile(Tile tile)
    {
        ShowTilePanel(tile);
        StartCoroutine(SelectTileAnim(tile));
    }
    // Désélection d'une tuile
    public void DeselectTile(Tile tile)
    {
        HideTilePanel();
        StartCoroutine(DeselectTileAnim(tile));
    }


    // Affichage du panel relatif à la tuile selectionée
    // Gère aussi l'interactabilité des boutons
    private void ShowTilePanel(Tile tile)
    {
        tilePanel.gameObject.SetActive(true);

        tilePanel.transform.position = new Vector3(tile.transform.position.x, 2f, tile.transform.position.z - 0.6f);

        string title = $"{tile.Owner}";
        string coords = $"({tile.X}, {tile.Y})";
        string desc = $"Type: {tile.Type}\nLvl: {tile.Lvl}\nUnits: {tile.Units}";
        tilePanel.SetInfoTilePanel(title, coords, desc);

        // Gestion de boutons
        if (tile.Units > 0)
        {
            // Set the move button interactable
            moveBtn.interactable = true;
        }
        else
        {
            moveBtn.interactable = false;
        }
    }

    private void HideTilePanel()
    {
        tilePanel.gameObject.SetActive(false);
    }

    #endregion



    #region Coroutine d'animations de selection

    private IEnumerator SelectTileAnim(Tile tile)
    {
        if (tile == null) { yield break; }
        float duration = 0.25f;
        float t = 0;
        Vector3 startPos = tile.transform.position;
        Vector3 endPos = new Vector3(tile.transform.position.x, 0.75f, tile.transform.position.z);
        while (t < duration)
        {
            t += Time.deltaTime;
            tile.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
        tile.transform.position = endPos;
    }


    private IEnumerator DeselectTileAnim(Tile tile)
    {
        if (tile == null) { yield break; }
        float duration = 0.25f;
        float t = 0;
        Vector3 startPos = tile.transform.position;
        Vector3 endPos = new Vector3(tile.transform.position.x, 0, tile.transform.position.z);
        while (t < duration)
        {
            t += Time.deltaTime;
            tile.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
        tile.transform.position = endPos;
    }


    #endregion


    #region Highlight des tiles

    public void UnHighlightTiles()
    {
        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
        {
            tile.UnHighlightTile();
        }
    }

    public void HighlightTiles(Tile[] tiles)
    {
        foreach (Tile tile in tiles)
        {
            tile.HighlightTile();
        }
    }

    #endregion


    #region Coroutine d'animation d'un déplacement d'unités

    public void DispatchUnitsAnim(Tile hq)
    {
        StartCoroutine(BubleAnim(hq, true));
    }

    public void RallyUnitsAnim(Tile to)
    {
        StartCoroutine(BubleAnim(to, false));
    }




    private IEnumerator BubleAnim(Tile tile, bool expand = true)
    {
        GameObject sphere = Instantiate(prefabSphere, tile.transform.position, Quaternion.identity);
        float duration = 0.75f;
        float t = 0;
        float startScale = expand ? 0 : 20;
        float endScale = expand ? 20 : 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(startScale, endScale, t / duration);
            sphere.transform.localScale = new Vector3(scale, scale, scale);
            sphere.transform.position = new Vector3(tile.transform.position.x, 0.75f, tile.transform.position.z);
            yield return null;
        }
        Destroy(sphere);
    }








    public void MoveUnitsAnim(Tile[] move)
    {

        StartCoroutine(AnimationMoveUnits(move));
    }


    private IEnumerator AnimationMoveUnits(Tile[] move)
    {
        string debugMsg = "";
        // Définir la durée totale de l'animation aller et retour
        float durationGo = 0.001f; // Durée totale pour dessiner la ligne
        float durationOg = 2f; // Durée totale pour effacer la ligne

        // 1. Convertir les données en liste de positions
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < move.Length; i++)
        {
            float[] pos = GetHexCoordinates(move[i].X, move[i].Y);
            positions.Add(new Vector3(pos[0], 0.75f, pos[1]));
            debugMsg += $"{move[i].X}:{move[i].Y} -> ";
        }

        Debug.Log(debugMsg);

        if (positions.Count == 0)
            yield break;

        LineRenderer moveUnitsLine = Instantiate(prefabLine);

        // 2. Calculer la distance totale du chemin
        float totalDistance = 0f;
        for (int i = 0; i < positions.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(positions[i], positions[i + 1]);
        }

        // 3. Animation de création (aller) sur l'ensemble du chemin
        float t = 0f;
        while (t < durationGo)
        {
            t += Time.deltaTime;
            float fraction = Mathf.Clamp01(t / durationGo);
            float distanceCovered = fraction * totalDistance;

            // Déterminer la position courante le long du chemin
            float d = 0f;
            Vector3 currentPoint = positions[0];
            int lastFullIndex = 0;
            for (int i = 0; i < positions.Count - 1; i++)
            {
                float segmentLength = Vector3.Distance(positions[i], positions[i + 1]);
                if (d + segmentLength >= distanceCovered)
                {
                    float segFraction = (distanceCovered - d) / segmentLength;
                    currentPoint = Vector3.Lerp(positions[i], positions[i + 1], segFraction);
                    lastFullIndex = i;
                    break;
                }
                d += segmentLength;
            }

            // Construire la ligne actuelle :
            // - Tous les points déjà atteints (de 0 à lastFullIndex)
            // - Le point courant interpolé sur le segment en cours
            List<Vector3> currentLine = new List<Vector3>();
            for (int i = 0; i <= lastFullIndex; i++)
            {
                currentLine.Add(positions[i]);
            }
            currentLine.Add(currentPoint);

            moveUnitsLine.positionCount = currentLine.Count;
            for (int i = 0; i < currentLine.Count; i++)
            {
                moveUnitsLine.SetPosition(i, currentLine[i]);
            }

            yield return null;
        }
        // Assurer que la ligne complète est affichée
        moveUnitsLine.positionCount = positions.Count;
        for (int i = 0; i < positions.Count; i++)
        {
            moveUnitsLine.SetPosition(i, positions[i]);
        }

        // 4. Animation d'effacement (retour) : faire disparaître la ligne depuis le début jusqu'à la fin
        t = 0f;
        while (t < durationOg)
        {
            t += Time.deltaTime;
            float fraction = Mathf.Clamp01(t / durationOg);
            float distanceErased = fraction * totalDistance;

            // Déterminer le nouveau point de départ le long du chemin
            float d = 0f;
            Vector3 newStart = positions[0];
            int firstFullIndex = 0;
            for (int i = 0; i < positions.Count - 1; i++)
            {
                float segmentLength = Vector3.Distance(positions[i], positions[i + 1]);
                if (d + segmentLength >= distanceErased)
                {
                    float segFraction = (distanceErased - d) / segmentLength;
                    newStart = Vector3.Lerp(positions[i], positions[i + 1], segFraction);
                    firstFullIndex = i + 1;
                    break;
                }
                d += segmentLength;
            }

            // Construire la nouvelle ligne à partir du nouveau point de départ jusqu'à la fin
            List<Vector3> newLine = new List<Vector3>();
            newLine.Add(newStart);
            for (int i = firstFullIndex; i < positions.Count; i++)
            {
                newLine.Add(positions[i]);
            }

            moveUnitsLine.positionCount = newLine.Count;
            for (int i = 0; i < newLine.Count; i++)
            {
                moveUnitsLine.SetPosition(i, newLine[i]);
            }

            yield return null;
        }

        // Détruire la ligne
        Destroy(moveUnitsLine.gameObject);
    }





    #endregion







}
