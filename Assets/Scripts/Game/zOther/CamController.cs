using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CamController : MonoBehaviour
{
    [Header("Paramètres de déplacement")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private float moveSpeed = 10f;           // Peut être utilisé pour ajuster la sensibilité du pan tactile
    [SerializeField] private float dragThreshold = 50f;       // Seuil de déclenchement du drag (en pixels)

    [Header("Paramètres de zoom")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 29f;

    public bool isDragging = false;
    private Vector2 initialTouchPosition;
    private Vector2 lastTouchPosition;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (!canMove)
        {
            ResetDrag();
            return;
        }

        HandleTouchPan();
        HandleTouchZoom();
    }

    #region Gestion du Pan Tactile (Android)

    private void HandleTouchPan()
    {
        // On ne gère que le cas d'un seul doigt pour le pan
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // Si le doigt commence
            if (touch.phase == TouchPhase.Began)
            {
                // Si touch sur UI, on n'initie pas le drag
                if (IsTouchOverUI(touch))
                {
                    isDragging = false;
                    initialTouchPosition = touch.position;
                    lastTouchPosition = touch.position;
                    return;
                }

                initialTouchPosition = touch.position;
                lastTouchPosition = touch.position;
            }
            // Si le doigt bouge
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                // Si on est toujours au-dessus d'une UI et que le drag n'a pas commencé, on skip
                if (IsTouchOverUI(touch) && !isDragging)
                {
                    isDragging = false;
                    initialTouchPosition = touch.position;
                    lastTouchPosition = touch.position;
                    return;
                }

                // Vérifier si on doit déclencher le drag
                if (!isDragging && Vector2.Distance(touch.position, initialTouchPosition) > dragThreshold)
                {
                    isDragging = true;
                }

                if (isDragging)
                {
                    Vector2 touchDelta = touch.position - lastTouchPosition;

                    // Calculer directions droite et avant basées sur la caméra
                    Vector3 right = mainCam.transform.right;
                    Vector3 forward = Vector3.Cross(right, Vector3.up);

                    // Empêcher tout mouvement vertical
                    right.y = 0f;
                    forward.y = 0f;
                    right.Normalize();
                    forward.Normalize();

                    // Conversion déplacement tactile en déplacement dans le monde
                    // On applique un facteur de sensibilité (moveSpeed * deltaTime)
                    Vector3 deltaPosition = (right * touchDelta.x + forward * touchDelta.y) * (moveSpeed * Time.deltaTime * 0.1f);
                    transform.position -= new Vector3(deltaPosition.x, 0f, deltaPosition.z);

                    lastTouchPosition = touch.position;
                }
            }
            // Si le doigt lève
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (isDragging)
                {
                    ResetDrag();
                }
            }
        }
        else
        {
            // Aucune ou plusieurs touches : réinitialiser le drag si actif
            if (isDragging)
                ResetDrag();
        }
    }

    private void ResetDrag()
    {
        StartCoroutine(DelayedDragEnd());
    }

    private IEnumerator DelayedDragEnd()
    {
        // Petit délai de quelques frames avant de réactiver isDragging = false
        for (int i = 0; i < 2; i++)
            yield return null;

        isDragging = false;
        // Réinitialiser les positions pour éviter un saut au prochain drag
        if (Input.touchCount > 0)
        {
            initialTouchPosition = Input.GetTouch(0).position;
            lastTouchPosition = initialTouchPosition;
        }
    }

    #endregion

    #region Gestion du Zoom Tactile (Android)

    private void HandleTouchZoom()
    {
        // Deux doigts -> pinch pour zoom
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // Ignorer si un des deux doigts est sur l'UI
            if (IsTouchOverUI(touch0) || IsTouchOverUI(touch1))
                return;

            Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;

            float prevDist = (prevPos0 - prevPos1).magnitude;
            float currDist = (touch0.position - touch1.position).magnitude;
            float deltaDist = prevDist - currDist;

            // Facteur de sensibilité (ajustable via zoomSpeed)
            float pinchFactor = 0.02f;
            float zoomAmount = deltaDist * pinchFactor * zoomSpeed;

            // Inversion du signe pour matcher molette
            ApplyZoom(-zoomAmount);
        }
    }

    private void ApplyZoom(float zoomAmount)
    {
        float oldY = transform.position.y;
        float targetY = oldY - zoomAmount;
        float newY = Mathf.Clamp(targetY, minZoom, maxZoom);
        float deltaY = newY - oldY;

        // Calcul du déplacement sur Z en fonction d'un angle de 70°
        float angleRad = 70f * Mathf.Deg2Rad;
        float ratioZtoY = Mathf.Cos(angleRad) / Mathf.Sin(angleRad);
        float newZ = transform.position.z - (deltaY * ratioZtoY);

        transform.position = new Vector3(transform.position.x, newY, newZ);
    }

    #endregion

    #region Vérifications UI

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

    #region Fonctions annexes (déplacement programmatique)

    public void MoveCamToTile(float x, float z, bool delay = true)
    {
        Vector3 offset = new Vector3(0f, 0f, -4f);
        Vector3 targetPos = new Vector3(x, 10f, z) + offset;

        if (delay)
            StartCoroutine(MoveCamAnim(targetPos));
        else
            transform.position = targetPos;
    }

    private IEnumerator MoveCamAnim(Vector3 targetPos)
    {
        float duration = 0.5f;
        float t = 0f;
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
