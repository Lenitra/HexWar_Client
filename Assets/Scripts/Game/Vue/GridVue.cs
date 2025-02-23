using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GridVue : MonoBehaviour
{

    private const float gridGap = 0.1f;
    private const float hexSize = 0.5f;
    private PresenteurCarte presenteurCarte;
    private CamController camController;


    // Prefab de l'hexagone
    [SerializeField] private GameObject tilePrefab;

    // Panel d'infos sur la tile
    [SerializeField] private TilePanel tilePanel;
    // Panel d'infos sur l'état des inputs
    [SerializeField] private GameObject stateInfos;

    [Space(10)]
    [Header("Elements d'effets temporaires")]
    [SerializeField] private LineRenderer moveUnitsLine;

    [Space(10)]
    [Header("UI Tiles")]

    [SerializeField] private Button buildBtn;
    [SerializeField] private Button moveBtn;
    [SerializeField] private Button rallyBtn;
    [SerializeField] private Button configBuildBtn;

    [Space(2)]
    [Header("Panels Tiles")]
    [SerializeField] private BuildPanel buildPanel;
    [SerializeField] private MovePanel movePanel;
    // [SerializeField] private RallyPanel rallyPanel;
    // [SerializeField] private ConfigBuildPanel configBuildPanel;

    // [SerializeField] private ModalPanel modalPanel;


    private Tile hoverTile = null;



    private void Start()
    {
        presenteurCarte = GetComponent<PresenteurCarte>();
        camController = Camera.main.GetComponent<CamController>();

        // Listeners des boutons de tiles
        buildBtn.onClick.AddListener(() => EnableGameObject(buildPanel.gameObject));
        // configBuildBtn.onClick.AddListener(() => EnableGameObject(configBuildPanel.gameObject));

        // rallyBtn.onClick.AddListener(() => EnableGameObject(rallyPanel.gameObject));

        moveBtn.onClick.AddListener(() => MoveBtnClick());
    }



    // Gestions des clicks sur la grille
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





        // Hover avec raycast
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                // Définir le cursor sur pointer
                Cursor.SetCursor(Resources.Load<Texture2D>("Sprites/UI/cursor_pointer"), Vector2.zero, CursorMode.Auto);

                if (presenteurCarte.State != "move")
                {
                    if (hoverTile != null && hoverTile != tile)
                    {
                        hoverTile.UnPreSelect();
                        hoverTile = tile;
                        hoverTile.PreSelect();
                        return;
                    }
                    else if (hoverTile == null)
                    {
                        hoverTile = tile;
                        hoverTile.PreSelect();
                        return;
                    }
                }
                else
                {
                    if (hoverTile != null && hoverTile != tile)
                    {
                        hoverTile.HideInfos();
                        hoverTile = tile;
                        hoverTile.ShowInfos();
                        return;
                    }
                    else if (hoverTile == null)
                    {
                        hoverTile = tile;
                        hoverTile.ShowInfos();
                        return;
                    }
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
    }


    private void DisableGameObject(GameObject go)
    {
        go.SetActive(false);
    }

    private void EnableGameObject(GameObject go)
    {
        DisableAllPanels();
        go.SetActive(true);
        string name = go.name;

        Tile tile = presenteurCarte.SelectTile;

        switch (name)
        {
            case "Build Panel":
                buildPanel.SetupPanel(tile);
                break;
            case "Move Panel":
                // movePanel.SetupPanel(tile);
                break;
            case "Rally Panel":
                // rallyPanel.SetupPanel(tile);
                break;
            case "Config BuildPanel":
                // configBuildPanel.SetupPanel(tile);
                break;
        }

    }


    // Quand un panneau se ferme, on désactive tous les autres et on désélectionne la tile
    public void ClosedPanel()
    {
        DisableAllPanels();
        presenteurCarte.SelectTile = null;
    }

    #endregion





    #region Gestion des clicks sur les boutons de tiles


    #region Bouton de construction de tiles
    public void BuildPanelRetour(string type = "")
    {
        presenteurCarte.BuildTile(type);
        ClosedPanel();
    }

    #endregion


    #region Bouton de déploiement d'unités
    private void MoveBtnClick()
    {
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
        }
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
    public void SelectTile(Tile tile)
    {
        ShowTilePanel(tile);
        StartCoroutine(SelectTileAnim(tile));
    }

    public void DeselectTile(Tile tile)
    {
        HideTilePanel();
        StartCoroutine(DeselectTileAnim(tile));
    }


    // Placement et affichage du panel relatif à la tuile selectionée
    private void ShowTilePanel(Tile tile)
    {
        tilePanel.gameObject.SetActive(true);

        tilePanel.transform.position = new Vector3(tile.transform.position.x, 2f, tile.transform.position.z - 0.6f);

        string title = $"{tile.Owner}";
        string coords = $"({tile.X}, {tile.Y})";
        string desc = $"Type: {tile.Type}\nLvl: {tile.Lvl}\nUnits: {tile.Units}";
        tilePanel.SetInfoTilePanel(title, coords, desc);
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
            tile.UnPreSelect();
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

    public void MoveUnitsAnim(string[] move)
    {

        StartCoroutine(AnimationMoveUnits(move));
    }



    private IEnumerator AnimationMoveUnits(string[] move)
    {
        // Définir la durée totale de l'animation aller et retour
        float durationGo = 0f; // Durée totale pour dessiner la ligne
        float durationOg = 2f; // Durée totale pour effacer la ligne

        // 1. Convertir les données en liste de positions
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < move.Length; i++)
        {
            string[] coords = move[i].Split(':');
            float[] pos = GetHexCoordinates(int.Parse(coords[0]), int.Parse(coords[1]));
            positions.Add(new Vector3(pos[0], 0.75f, pos[1]));
        }
        if (positions.Count == 0)
            yield break;

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

        // Effacer complètement la ligne
        moveUnitsLine.positionCount = 0;
    }





    #endregion

}
