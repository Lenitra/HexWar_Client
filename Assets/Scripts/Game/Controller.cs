using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles player interactions with tiles: selection, hover, and action panels (build, move, rally, dispatch).
/// </summary>
public class Controller : MonoBehaviour
{
    // --- References to other components ---
    private GameManager gameManager;
    private CamController camController;
    private Camera mainCamera;

    // --- Player data ---
    private string playerName;

    // --- Tile selection state ---
    private Tile hoverTile;
    private Tile selectedTile;
    private bool awaitingSecondSelection;
    private Tile secondSelectedTile;

    [Header("Tile Info Panels")]
    [SerializeField] private TilePanel hoverInfoPanel;
    [SerializeField] private TilePanel selectedInfoPanel;

    [Header("Action Buttons")]
    [SerializeField] private Button buildButton;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button rallyButton;
    [SerializeField] private Button dispatchButton;

    [Header("Action Panels")]
    [SerializeField] private BuildPanel buildPanel;
    [SerializeField] private MovePanel movePanel;
    [SerializeField] private RallyPanel rallyPanel;
    [SerializeField] private DispatchPanel dispatchPanel;

    /// <summary>
    /// Currently selected tile by the player.
    /// Setting this property will handle visual feedback and info panel.
    /// </summary>
    private Tile SelectedTile
    {
        get => selectedTile;
        set
        {
            // Deselect previous tile if exists
            if (selectedTile != null)
                StartCoroutine(selectedTile.UnSelectCoroutine());

            selectedTile = value;

            // Update info panel and visual selection
            if (selectedTile != null)
            {
                selectedInfoPanel.SetInfoTilePanel(selectedTile);
                selectedInfoPanel.gameObject.SetActive(true);
                StartCoroutine(selectedTile.SelectCoroutine());
            }
            else
            {
                selectedInfoPanel.gameObject.SetActive(false);
            }
        }
    }

    #region Unity Lifecycle

    private void Start()
    {
        // Cache references
        gameManager = GetComponent<GameManager>();
        mainCamera = Camera.main;
        camController = mainCamera.GetComponent<CamController>();

        // Retrieve current player name
        playerName = PlayerPrefs.GetString("username");

        // Wire up button callbacks
        buildButton.onClick.AddListener(OnBuildButtonClicked);
        moveButton.onClick.AddListener(OnMoveButtonClicked);
        rallyButton.onClick.AddListener(OnRallyButtonClicked);
        dispatchButton.onClick.AddListener(OnDispatchButtonClicked);
    }

    private void Update()
    {
        HandleMouseClick();
        HandleMouseHover();
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// Handles selection/deselection of tiles on mouse click.
    /// Prevents interaction when dragging camera or over UI.
    /// </summary>
    private void HandleMouseClick()
    {
        if (!Input.GetMouseButtonUp(0) || camController.isDragging || EventSystem.current.IsPointerOverGameObject())
            return;

        if (mainCamera.RaycastTile(out Tile hitTile))
        {
            if (!awaitingSecondSelection)
                ProcessPrimarySelection(hitTile);
            else
                secondSelectedTile = hitTile;
        }
        else if (!awaitingSecondSelection)
        {
            // Clicked outside any tile: clear selection
            SelectedTile = null;
        }
    }

    /// <summary>
    /// Handles hover highlighting and info panel updates.
    /// </summary>
    private void HandleMouseHover()
    {
        if (mainCamera.RaycastTile(out Tile hitTile) && !camController.isDragging && !EventSystem.current.IsPointerOverGameObject())
        {
            if (hoverTile != hitTile)
                UpdateHoverTile(hitTile);

            hoverInfoPanel.SetPos(Input.mousePosition);
        }
        else if (hoverTile != null)
        {
            UpdateHoverTile(null);
        }
    }

    #endregion

    #region Tile Selection Logic

    /// <summary>
    /// Process primary tile selection: validates ownership and toggles selection.
    /// </summary>
    /// <param name="tile">The tile clicked on.</param>
    private void ProcessPrimarySelection(Tile tile)
    {
        if (tile == null || tile.Owner != playerName)
        {
            SelectedTile = null;
            return;
        }

        // Toggle selection off if clicking the same tile
        if (tile == selectedTile)
        {
            SelectedTile = null;
            return;
        }

        SelectedTile = tile;
    }

    /// <summary>
    /// Updates the currently hovered tile: highlights and updates info panel.
    /// </summary>
    /// <param name="tile">The tile being hovered over.</param>
    private void UpdateHoverTile(Tile tile)
    {
        // Remove highlight from previous hoverTile
        if (hoverTile != null)
            hoverTile.UnHighlightTile();

        hoverTile = tile;

        if (hoverTile != null)
        {
            if (hoverTile.Owner == playerName)
                hoverTile.HighlightTile();

            hoverInfoPanel.SetInfoTilePanel(hoverTile);
            hoverInfoPanel.gameObject.SetActive(true);
        }
        else
        {
            hoverInfoPanel.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Button Callbacks

    /// <summary>
    /// Called when Build button is clicked. Opens build panel on selected tile.
    /// </summary>
    private void OnBuildButtonClicked()
    {
        if (selectedTile == null || selectedTile.Owner != playerName)
            return;

        buildPanel.SetupPanel(selectedTile);
        ShowOnlyPanel(buildPanel.gameObject);
        SelectedTile = null;
    }

    /// <summary>
    /// Called when Move button is clicked. Enters second tile selection mode.
    /// </summary>
    private void OnMoveButtonClicked()
    {
        if (selectedTile == null)
            return;

        awaitingSecondSelection = true;
        // Further handling of second selection can be implemented here.
    }

    private void OnRallyButtonClicked() { /* TODO: Implement rally action */ }
    private void OnDispatchButtonClicked() { /* TODO: Implement dispatch action */ }

    #endregion

    #region UI Helpers

    /// <summary>
    /// Shows the provided panel and hides all other action and info panels.
    /// </summary>
    /// <param name="panel">The panel GameObject to show.</param>
    private void ShowOnlyPanel(GameObject panel)
    {
        // Hide info panels
        selectedInfoPanel.gameObject.SetActive(false);
        hoverInfoPanel.gameObject.SetActive(false);

        // Hide action panels
        buildPanel.gameObject.SetActive(false);
        movePanel.gameObject.SetActive(false);
        rallyPanel.gameObject.SetActive(false);
        dispatchPanel.gameObject.SetActive(false);

        // Show requested panel
        panel.SetActive(true);
    }

    #endregion
}

/// <summary>
/// Extension methods for Camera to raycast and retrieve Tile components.
/// </summary>
public static class CameraExtensions
{
    /// <summary>
    /// Raycasts from screen point into world and returns the Tile component if hit.
    /// </summary>
    public static bool RaycastTile(this Camera camera, out Tile tile)
    {
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            tile = hit.collider.GetComponent<Tile>();
            return tile != null;
        }

        tile = null;
        return false;
    }
}
