using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CamController : MonoBehaviour
{
    [Header("Paramètres de déplacement")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float dragThreshold = 50f; // Pour tactile

    [Header("Paramètres de zoom")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 29f;

    [Header("PC Controls")]
    [SerializeField] private float mousePanSpeed = 0.5f; // Sensibilité drag souris
    [SerializeField] private float keyboardPanSpeed = 10f; // Sensibilité clavier
    [SerializeField] private float mouseZoomSpeed = 10f; // Sensibilité zoom molette

    public bool isDragging = false;
    private Vector2 initialTouchPosition;
    private Vector2 lastTouchPosition;
    private Vector3 lastMousePosition;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        // Appliquer les facteurs de sensibilité

        SetupOptions();
    }


    public void SetupOptions()
    {
        float camSpeedSensitivity = PlayerPrefs.GetFloat("opt_cam_speed", 1.0f);
        float camZoomSensitivity = PlayerPrefs.GetFloat("opt_cam_zoom", 1.0f);
        moveSpeed *= camSpeedSensitivity;
        mousePanSpeed *= camSpeedSensitivity;
        mouseZoomSpeed *= camZoomSensitivity;
        zoomSpeed *= camZoomSensitivity;
    }



    private void Update()
    {
        if (!canMove)
        {
            ResetDrag();
            return;
        }

        HandlePCPan();
        HandlePCZoom();
        HandleTouchPan();
        HandleTouchZoom();
    }

    #region PAN PC
    private void HandlePCPan()
    {
        // 1. PAN CLAVIER
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
        {
            // Directions basées sur la caméra
            Vector3 right = mainCam.transform.right;
            Vector3 forward = Vector3.Cross(right, Vector3.up);

            right.y = 0f; forward.y = 0f;
            right.Normalize(); forward.Normalize();

            Vector3 delta = (right * h + forward * v) * (keyboardPanSpeed * Time.deltaTime);
            transform.position += new Vector3(delta.x, 0f, delta.z);
        }

        // 2. PAN SOURIS (clic gauche drag, pas sur UI)
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            lastMousePosition = Input.mousePosition;
            isDragging = true;
        }
        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;

            // Directions caméra
            Vector3 right = mainCam.transform.right;
            Vector3 forward = Vector3.Cross(right, Vector3.up);

            right.y = 0f; forward.y = 0f;
            right.Normalize(); forward.Normalize();

            Vector3 delta = (right * mouseDelta.x + forward * mouseDelta.y) * (mousePanSpeed * Time.deltaTime);
            transform.position -= new Vector3(delta.x, 0f, delta.z);

            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
    #endregion

    #region ZOOM PC
    private void HandlePCZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            ApplyZoom(scroll * mouseZoomSpeed);
        }
    }
    #endregion

    #region PAN TACTILE (Mobile)
    private void HandleTouchPan()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
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
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                if (IsTouchOverUI(touch) && !isDragging)
                {
                    isDragging = false;
                    initialTouchPosition = touch.position;
                    lastTouchPosition = touch.position;
                    return;
                }

                if (!isDragging && Vector2.Distance(touch.position, initialTouchPosition) > dragThreshold)
                {
                    isDragging = true;
                }

                if (isDragging)
                {
                    Vector2 touchDelta = touch.position - lastTouchPosition;
                    Vector3 right = mainCam.transform.right;
                    Vector3 forward = Vector3.Cross(right, Vector3.up);
                    right.y = 0f; forward.y = 0f;
                    right.Normalize(); forward.Normalize();

                    Vector3 deltaPosition = (right * touchDelta.x + forward * touchDelta.y) * (moveSpeed * Time.deltaTime * 0.1f);
                    transform.position -= new Vector3(deltaPosition.x, 0f, deltaPosition.z);

                    lastTouchPosition = touch.position;
                }
            }
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
            if (isDragging)
                ResetDrag();
        }
    }
    #endregion

    #region ZOOM TACTILE (Mobile)
    private void HandleTouchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (IsTouchOverUI(touch0) || IsTouchOverUI(touch1))
                return;

            Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;

            float prevDist = (prevPos0 - prevPos1).magnitude;
            float currDist = (touch0.position - touch1.position).magnitude;
            float deltaDist = prevDist - currDist;

            float pinchFactor = 0.02f;
            float zoomAmount = deltaDist * pinchFactor * zoomSpeed;
            ApplyZoom(-zoomAmount);
        }
    }
    #endregion

    private void ApplyZoom(float zoomAmount)
    {
        float oldY = transform.position.y;
        float targetY = oldY - zoomAmount;
        float newY = Mathf.Clamp(targetY, minZoom, maxZoom);
        float deltaY = newY - oldY;

        float angleRad = 70f * Mathf.Deg2Rad;
        float ratioZtoY = Mathf.Cos(angleRad) / Mathf.Sin(angleRad);
        float newZ = transform.position.z - (deltaY * ratioZtoY);

        transform.position = new Vector3(transform.position.x, newY, newZ);
    }

    #region Gestion UI universelle
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
    private bool IsTouchOverUI(Touch touch)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
    }
    #endregion

    private void ResetDrag()
    {
        StartCoroutine(DelayedDragEnd());
    }
    private IEnumerator DelayedDragEnd()
    {
        for (int i = 0; i < 2; i++)
            yield return null;
        isDragging = false;
        if (Input.touchCount > 0)
        {
            initialTouchPosition = Input.GetTouch(0).position;
            lastTouchPosition = initialTouchPosition;
        }
    }

    #region Déplacement programmatique (inchangé)
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
