using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridVue : MonoBehaviour
{

    private const float gridGap = 0.1f;
    private const float hexSize = 0.5f;
    private PresenteurCarte presenteurCarte;
    private CamController camController;


    // Prefab de l'hexagone
    [SerializeField] private GameObject tilePrefab;

    [SerializeField] private TilePanel tilePanel;

    [Space(10)]
    [Header("UI Tiles")]

    [SerializeField] private Button buildBtn;
    [SerializeField] private Button moveBtn;
    [SerializeField] private Button rallyBtn;
    [SerializeField] private Button configBuildBtn;

    // [Space(10)]
    // [Header("Panels Tiles")]
    // [SerializeField] private BuildPanel buildPanel;
    // [SerializeField] private MovePanel movePanel;
    // [SerializeField] private RallyPanel rallyPanel;
    // [SerializeField] private ConfigBuildPanel configBuildPanel;
    
    // [SerializeField] private ModalPanel modalPanel;




    private void Start()
    {
        presenteurCarte = GetComponent<PresenteurCarte>();
        camController = Camera.main.GetComponent<CamController>();

        // Listeners des boutons de tiles
        
        // buildBtn.onClick.AddListener(() => EnableGameObject(buildPanel.gameObject));
        // moveBtn.onClick.AddListener(() => EnableGameObject(movePanel.gameObject));
        // rallyBtn.onClick.AddListener(() => EnableGameObject(rallyPanel.gameObject));
        // configBuildBtn.onClick.AddListener(() => EnableGameObject(configBuildPanel.gameObject));
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

    #endregion



    #region utils

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

    // TODO: Uncoment when panels are implemented
    private void DisableAllPanels()
    {
        DisableGameObject(tilePanel.gameObject);
        // DisableGameObject(buildPanel.gameObject);
        // DisableGameObject(upgradePanel.gameObject);
        // DisableGameObject(movePanel.gameObject);
        // DisableGameObject(rallyPanel.gameObject);
    }
    
    
    private void DisableGameObject(GameObject go)
    {
        go.SetActive(false);
    }

    private void EnableGameObject(GameObject go)
    {
        DisableAllPanels();
        go.SetActive(true);
    }

    #endregion



    #region Gestion des retours des différents panels
    public void BuildPanelRetour(string type)
    {
        DisableAllPanels();
        // TODO: Gérer l'envoi au presenteur
    }

    public void UpgradePanelRetour()
    {
        DisableAllPanels();
        // TODO: Gérer l'envoi au presenteur
    }

    public void MovePanelRetour()
    {
        DisableAllPanels();
        // TODO: Gérer l'envoi au presenteur
    }


    public void RallyPanelRetour()
    {
        DisableAllPanels();
        // TODO: Gérer l'envoi au presenteur
    }

    #endregion




    #region Coroutine d'animations de tiles
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

        tilePanel.transform.position = new Vector3(tile.transform.position.x, 1.15f, tile.transform.position.z - 0.1f);

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






}
