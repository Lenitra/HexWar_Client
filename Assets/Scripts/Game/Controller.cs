using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

/// <summary>
/// Handles player interactions with tiles: selection, hover, and action panels (build, move, rally, dispatch).
/// </summary>
public class Controller : MonoBehaviour
{
    // --- References to other components ---
    private GameManager gameManager;
    private CamController camController;
    private Camera mainCamera;
    private string playerName;


    // --- Tile selection state ---
    private Tile selectedTile;
    private bool moveMode = false;


    [Header("Tile Info Panels")]
    [SerializeField] private TilePanel hoverInfoPanel;



    [Header("Action Panels")]
    [SerializeField] private BuildPanel buildPanel;
    [SerializeField] private MovePanel movePanel;

    [Header("Other Panels")]
    [SerializeField] private Button optionsButton;
    [SerializeField] private OptionsPanel optionsPanel;



    [Header("Timing Thresholds (seconds)")]
    private readonly float tapThreshold = 0.3f;
    private readonly float doubleTapMaxDelay = 0.25f;
    private readonly float longPressThreshold = 0.5f;

    [Header("Drag Detection (pixels)")]
    private readonly float dragThreshold = 10f;
    private float pressStartTime;
    private Vector2 pressStartPos;
    private bool isDragging;
    private bool longPressTriggered;

    private bool awaitingDoubleTap;
    private float lastTapTime;
    private Coroutine tapCoroutine;


    private int unitsToMove = 0; // Nombre d'unités à déplacer


    /// <summary>
    /// Currently selected tile by the player.
    /// Setting this property will handle visual feedback and info panel.
    /// </summary>
    public Tile SelectedTile
    {
        get => selectedTile;
        set
        {
            if (selectedTile != null)
            {
                // Deselect previous tile
                selectedTile.UnSelect();
            }


            if (selectedTile == value)
            {
                selectedTile = null;
            }
            else
            {
                selectedTile = value;
            }


            if (selectedTile != null)
            {
                // Select new tile
                selectedTile.Select();
                hoverInfoPanel.gameObject.SetActive(true);
                hoverInfoPanel.SetInfoTilePanel(selectedTile);
            }
            else
            {
                // Hide info panel if no tile is selected
                hoverInfoPanel.gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        // Cache references
        gameManager = GetComponent<GameManager>();
        mainCamera = Camera.main;
        camController = mainCamera.GetComponent<CamController>();

        // Retrieve current player name
        playerName = PlayerPrefs.GetString("username");

        // Initialize panels
        buildPanel.gameObject.SetActive(false);
        movePanel.gameObject.SetActive(false);
        optionsPanel.gameObject.SetActive(false);
        optionsButton.onClick.AddListener(() =>
        {
            // Show options panel
            ShowOnlyPanel(optionsPanel.gameObject);
        });
    }





    #region Gestion Inputs

    void Update()
    {
        if (EventSystem.current != null && IsPointerOverUI())
        {
            return;
        }
        // On press start
        if (Input.GetMouseButtonDown(0))
        {
            pressStartTime = Time.time;
            pressStartPos = Input.mousePosition;
            isDragging = false;
            longPressTriggered = false;
        }

        // While holding
        if (Input.GetMouseButton(0))
        {
            // Detect drag
            if (!isDragging && Vector2.Distance(Input.mousePosition, pressStartPos) > dragThreshold)
            {
                isDragging = true;
                CancelTapDetection();
                return;
            }

            if (isDragging) return;

            // Long press detection
            if (!longPressTriggered && Time.time - pressStartTime >= longPressThreshold)
            {
                longPressTriggered = true;
                CancelTapDetection();
                LongPress(GetTileUnderMouse());
                return;
            }
        }

        // On release
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging || longPressTriggered)
            {
                // Reset flags for next cycle
                isDragging = false;
                return;
            }

            float pressDuration = Time.time - pressStartTime;
            if (pressDuration > tapThreshold)
                return;

            // Tap logic
            if (awaitingDoubleTap && Time.time - lastTapTime <= doubleTapMaxDelay)
            {
                // Double tap detected
                CancelTapDetection();
                DoubleTap(GetTileUnderMouse());
                return;
            }
            else
            {
                // First tap: wait for possible second tap
                awaitingDoubleTap = true;
                lastTapTime = Time.time;
                tapCoroutine = StartCoroutine(WaitForDoubleTap());
            }
        }
    }


    private IEnumerator WaitForDoubleTap()
    {

        yield return new WaitForSeconds(doubleTapMaxDelay);
        if (awaitingDoubleTap)
        {
            Debug.Log("Single Tap");
            OneTap(GetTileUnderMouse());
        }
        awaitingDoubleTap = false;
        tapCoroutine = null;
    }


    private void CancelTapDetection()
    {
        if (tapCoroutine != null)
        {
            StopCoroutine(tapCoroutine);
            tapCoroutine = null;
        }
        awaitingDoubleTap = false;
    }


    private Tile GetTileUnderMouse()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return null;

        Tile tile;
        if (mainCamera.RaycastTile(out tile))
        {
            return tile;
        }
        return null;
    }


    #endregion






    #region Gestion des interactions avec les tiles

    private void OneTap(Tile tile)
    {
        if (moveMode)
        {
            if (gameManager.GetValidMoveDestination(SelectedTile).Contains(tile))
            {
                MoveUnits(SelectedTile, tile, unitsToMove);
                gameManager.UnHighlightAllTiles();
                moveMode = false;
                return;
            }
            else
            {
                if (SelectedTile == tile)
                {
                    SelectedTile = null;
                }
                Debug.LogWarning("Invalid tile for movement.");
                return;
            }
        }
        else
        {
            SelectedTile = tile;
        }
    }

    private void DoubleTap(Tile tile)
    {
        if (tile == null || moveMode) return;

        if (tile.Owner != playerName || tile.Units <= 0)
        {
            Debug.LogWarning("Tile cannot be selected for unit movement.");
            return;
        }

        if (SelectedTile != tile)
        {
            SelectedTile = tile;
        }

        movePanel.gameObject.SetActive(true);
        movePanel.SetupPanel(tile);
    }

    private void LongPress(Tile tile)
    {
        if (tile == null || moveMode) return;

        if (tile.Owner != playerName)
        {
            Debug.LogWarning("Tile cannot be selected for actions.");
            return;
        }


        if (SelectedTile != tile)
        {
            SelectedTile = tile;
        }
        buildPanel.gameObject.SetActive(true);
        buildPanel.SetupPanel(tile);

    }

    #endregion




    #region Validation des paneaux d'action/panels

    public void BuildTile(Tile tile, string type)
    {
        if (SelectedTile == null || SelectedTile.Owner != playerName)
            return;
        string[] tileCoords = { tile.X.ToString(), tile.Y.ToString() };
        gameManager.BuildTile(tileCoords, type);
        SelectedTile = null;
    }

    public void DestroyTile(Tile tileSelected)
    {
        if (SelectedTile == null || SelectedTile.Owner != playerName)
            return;
        string[] tileCoords = { tileSelected.X.ToString(), tileSelected.Y.ToString() };
        gameManager.DestroyTile(tileCoords);
        SelectedTile = null;
    }

    public void ValidateMovePanel(Tile origin, int unitsCount)
    {
        if (unitsCount <= 0)
        {
            moveMode = false;
            gameManager.UnHighlightAllTiles();
            SelectedTile = null;
            return;
        }
        unitsToMove = unitsCount;
        gameManager.HighlightMoveTiles(origin);
        moveMode = true;
    }


    private void MoveUnits(Tile origin, Tile destination, int untisCount)
    {
        string[] originCoods = { origin.X.ToString(), origin.Y.ToString() };
        string[] destinationCoods = { destination.X.ToString(), destination.Y.ToString() };
        gameManager.MoveUnitsTile(originCoods, destinationCoods, untisCount);
        Debug.Log($"Moving {untisCount} units from ({origin.X}, {origin.Y}) to ({destination.X}, {destination.Y})");
        SelectedTile = null;
    }


    public void RallyTile(Tile tileSelected)
    {
        if (SelectedTile == null || SelectedTile.Owner != playerName)
            return;
        string[] tileCoords = { tileSelected.X.ToString(), tileSelected.Y.ToString() };
        gameManager.RallyUnits(tileCoords);
        SelectedTile = null;
    }

    #endregion


    #region Gestion du panel d'options
    public void SetUsername(string username)
    {
        String[] unauthorizedChars = { " ", "/", "\\", ":", "*", "?", "\"", "<", ">", "|", "@" };

        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("Username cannot be empty.");
            return;
        }
        if (username.Length > 20 || username.Length < 3)
        {
            Debug.LogWarning("Username must be between 3 and 20 characters.");
            return;
        }
        if (Array.Exists(unauthorizedChars, c => username.Contains(c)))
        {
            Debug.LogWarning("Username contains unauthorized characters.");
            return;
        }
        

        PlayerPrefs.SetString("username", username);
        PlayerPrefs.Save();
        Debug.Log($"Username set to: {username}");
    }

    public void SetColor(string colorHex)
    {
        if (string.IsNullOrEmpty(colorHex) || !UnityEngine.ColorUtility.TryParseHtmlString(colorHex, out UnityEngine.Color color))
        {
            Debug.LogWarning("Invalid color format. Use hex format like #RRGGBB.");
            return;
        }

        PlayerPrefs.SetString("color", colorHex);
        PlayerPrefs.Save();
        Debug.Log($"Color set to: {colorHex}");
    }

    public void SetCameraSpeed(float speed)
    {

        PlayerPrefs.SetFloat("opt_cam_speed", speed);
        PlayerPrefs.Save();
        camController.SetupOptions();
        Debug.Log($"Camera speed set to: {speed}");
    }

    public void SetCameraZoom(float zoom)
    {

        PlayerPrefs.SetFloat("opt_cam_zoom", zoom);
        PlayerPrefs.Save();
        camController.SetupOptions();
        Debug.Log($"Camera zoom set to: {zoom}");
    }

    #endregion



    #region UI Helpers
    private bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults.Count > 0;
    }


    private void ShowOnlyPanel(GameObject panel)
    {
        // Hide info panels
        // selectedInfoPanel.gameObject.SetActive(false);
        // hoverInfoPanel.gameObject.SetActive(false);

        // // Hide action panels
        // buildPanel.gameObject.SetActive(false);
        // movePanel.gameObject.SetActive(false);
        // rallyPanel.gameObject.SetActive(false);
        // dispatchPanel.gameObject.SetActive(false);

        // Show requested panel
        panel.SetActive(true);
    }
    #endregion
}

/// <summary>
/// Méthodes d'extension pour Camera afin de faire un raycast et récupérer les composants Tile.
/// </summary>
public static class CameraExtensions
{
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
