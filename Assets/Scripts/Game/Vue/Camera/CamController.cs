using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CamController : MonoBehaviour
{
    [Header("Paramètres de déplacement")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float dragThreshold = 50f; // Seuil de déclenchement du drag

    [Header("Paramètres de zoom")]
    private float zoomSpeed = 10f;
    private float minZoom = 10f;
    private float maxZoom = 40f;

    public bool isDragging = false;
    private Vector3 initialMousePosition;
    private Vector3 lastMousePosition;

    private Plane groundPlane;
    private Camera mainCam;


    private void Start()
    {
        // Plan de référence pour les mouvements sur le plan XZ (y = 0)
        groundPlane = new Plane(Vector3.up, Vector3.zero);
        mainCam = Camera.main;
    }

    private void Update()
    {
        // Si le déplacement est désactivé, réinitialiser le drag et afficher le curseur
        if (!canMove)
        {
            ResetDrag();
            return;
        }

        HandleZoom();
        HandleMouseDrag();
        HandleKeyboardMovement();
    }

    #region Gestion du Zoom

    private void HandleZoom()
    {
        // Zoom via molette de souris
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            ApplyZoom(scroll * zoomSpeed);
        }

        // Zoom tactile (pinch)
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // Ignorer le zoom si l'une des touches est sur l'UI
            if (IsTouchOverUI(touch0) || IsTouchOverUI(touch1))
                return;

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevTouchDelta = (touch0PrevPos - touch1PrevPos).magnitude;
            float currTouchDelta = (touch0.position - touch1.position).magnitude;
            float deltaMagnitudeDiff = prevTouchDelta - currTouchDelta;

            // Facteur de sensibilité (à ajuster selon vos besoins)
            float pinchSpeedFactor = 0.02f;
            float zoomAmount = deltaMagnitudeDiff * pinchSpeedFactor * zoomSpeed;
            // L'inversion du signe permet d'harmoniser avec le comportement de la molette
            ApplyZoom(-zoomAmount);
        }
    }

    private void ApplyZoom(float zoomAmount)
    {
        float oldY = transform.position.y;
        float targetY = oldY - zoomAmount;
        float newY = Mathf.Clamp(targetY, minZoom, maxZoom);
        float actualDeltaY = newY - oldY;

        // Calcul du déplacement sur l'axe Z en fonction d'un angle de 70°
        float angleInRadians = 70f * Mathf.Deg2Rad;
        float ratioZtoY = Mathf.Cos(angleInRadians) / Mathf.Sin(angleInRadians);
        float newZ = transform.position.z - (actualDeltaY * ratioZtoY);

        transform.position = new Vector3(transform.position.x, newY, newZ);
    }

    #endregion

    #region Gestion du Drag Souris

    private void HandleMouseDrag()
    {
        // Pour éviter de gérer le drag souris quand il y a des touches tactiles actives
        if (Input.touchCount > 0)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
            {
                Cursor.visible = true;
                isDragging = false;
                initialMousePosition = Input.mousePosition;
                lastMousePosition = Input.mousePosition;
                return;
            }
            initialMousePosition = Input.mousePosition;
            lastMousePosition = initialMousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            if (IsPointerOverUI() && !isDragging)
            {
                Cursor.visible = true;
                isDragging = false;
                initialMousePosition = Input.mousePosition;
                lastMousePosition = Input.mousePosition;
                return;
            }
            
            // Si le drag n'est pas encore lancé, vérifier le seuil
            if (!isDragging && Vector3.Distance(Input.mousePosition, initialMousePosition) > dragThreshold)
            {
                isDragging = true;
                Cursor.visible = false;
            }

            if (isDragging)
            {
                Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
                // Calculer les directions droite et avant en se basant sur la caméra
                Vector3 right = mainCam.transform.right;
                Vector3 forward = Vector3.Cross(right, Vector3.up);

                // Conversion du déplacement de la souris en déplacement dans le monde
                Vector3 deltaPosition = (right * mouseDelta.x + forward * mouseDelta.y) * 0.01f;
                transform.position -= new Vector3(deltaPosition.x, 0, deltaPosition.z);

                lastMousePosition = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                ResetDrag();

            }
        }
    }


    private void ResetDrag()
    {
        StartCoroutine(DelayedCursorActivation());
    }
    // coroutine pour ajouter un délai de 5 frames avant de réactiver le curseur
    private IEnumerator DelayedCursorActivation()
    {
        for (int i = 0; i < 2; i++)
        {
            yield return null;
        }
        Cursor.visible = true;
        isDragging = false;
        initialMousePosition = Input.mousePosition;
        lastMousePosition = Input.mousePosition;
    }

    #endregion

    #region Déplacement Clavier

    private void HandleKeyboardMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (Mathf.Approximately(horizontalInput, 0f) && Mathf.Approximately(verticalInput, 0f))
            return;

        Vector3 right = mainCam.transform.right;
        Vector3 forward = Vector3.Cross(right, Vector3.up);

        // S'assurer que le mouvement reste sur le plan horizontal
        right.y = 0f;
        forward.y = 0f;
        right.Normalize();
        forward.Normalize();

        Vector3 moveDirection = right * horizontalInput + forward * verticalInput;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    #endregion

    #region Vérifications UI

    /// <summary>
    /// Vérifie si la position de la souris est sur l'UI.
    /// </summary>
    /// <returns>True si la souris sur l'UI, sinon false.</returns>
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// Vérifie si un touch donné est sur l'UI.
    /// </summary>
    /// <param name="touch">Le touch à vérifier.</param>
    /// <returns>True si le touch est sur l'UI, sinon false.</returns>
    private bool IsTouchOverUI(Touch touch)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
    }

    #endregion


    #region Fonctions de déplacement vers quelque chose
    public void moveCamToTile(float x, float z, bool delay = true)
    {
        Vector3 offset = new Vector3(0, 0, -4);

        Vector3 targetPos = new Vector3(x, 10, z) + offset;

        if (delay)
        {
            StartCoroutine(MoveCamAnim(targetPos));
        }
        else
        {
            transform.position = targetPos;
        }
    }


    // animation du déplacement de la caméra
    private IEnumerator MoveCamAnim(Vector3 targetPos)
    {
        float duration = 0.5f;
        float t = 0;
        Vector3 startPos = transform.position;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, t / duration);
            yield return null;
        }
    }

    #endregion

}
