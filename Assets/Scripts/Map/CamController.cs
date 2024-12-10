using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CamController : MonoBehaviour
{    
    private Coroutine moveCoroutine = null;   // Coroutine pour l'animation de translation
    private float smoothTime = 3; 
    private Plane groundPlane;
    private bool isDragging = false;
    private Vector3 initialMousePosition;
    private Vector3 lastMousePosition;
    private float dragThreshold = 5f; // Seuil en pixels pour détecter un glissement

    // Variables pour le zoom
    private float zoomSpeed = 10f;     // Vitesse du zoom
    private float minZoom = 10f;        // Zoom minimum (proche)
    private float maxZoom = 25f;       // Zoom maximum (éloigné)

    void Start()
    {
        // Définir un plan au niveau de y = 0 pour le mouvement sur XZ
        groundPlane = new Plane(Vector3.up, Vector3.zero);
    }

    void Update()
    {
        // Gestion du zoom avec la molette
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f && moveCoroutine == null)
        {
            float zoomAmount = scroll * zoomSpeed;

            // Sauvegarder l'ancienne position Y
            float oldY = transform.position.y;
            // Calculer la cible (avant clamp)
            float targetY = oldY - zoomAmount;

            // Limiter Y entre minZoom et maxZoom
            float newY = Mathf.Clamp(targetY, minZoom, maxZoom);

            // Calculer le delta Y effectif après clamp
            float actualDeltaY = newY - oldY;

            // Calcul du ratio en fonction de l'angle de 70°
            float angleInRadians = 70f * Mathf.Deg2Rad;
            float ratioZtoY = Mathf.Cos(angleInRadians) / Mathf.Sin(angleInRadians);

            // Calcul du nouveau Z en tenant compte du delta Y effectif
            float newZ = transform.position.z - (actualDeltaY * ratioZtoY);

            // Mettre à jour la position
            transform.position = new Vector3(transform.position.x, newY, newZ);
        }


        // Lorsque le bouton gauche de la souris est enfoncé
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = false;
                initialMousePosition = Input.mousePosition;
                lastMousePosition = Input.mousePosition;
                return; 
            }
            initialMousePosition = Input.mousePosition;
            lastMousePosition = initialMousePosition;
        }
        // Lorsque le bouton gauche de la souris est maintenu enfoncé
        else if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = false;
                initialMousePosition = Input.mousePosition;
                lastMousePosition = Input.mousePosition;
                return; 
            }
            // Calculer la distance parcourue par la souris depuis le début du clic
            float distance = (Input.mousePosition - initialMousePosition).magnitude;

            if (!isDragging && distance > dragThreshold)
            {
                // Commencer le glissement
                isDragging = true;
                Cursor.visible = false;
            }

            if (isDragging)
            {
                // Obtenir le déplacement de la souris
                Vector3 mouseDelta = Input.mousePosition - lastMousePosition;

                // Convertir le déplacement de la souris en déplacement dans le monde
                Vector3 right = Camera.main.transform.right;
                Vector3 forward = Vector3.Cross(right, Vector3.up);

                Vector3 deltaPosition = right * mouseDelta.x + forward * mouseDelta.y;

                // Déplacer la caméra en fonction du déplacement de la souris
                transform.position -= new Vector3(deltaPosition.x, 0, deltaPosition.z) * 0.01f;

                lastMousePosition = Input.mousePosition;
            }
        }
        // Lorsque le bouton gauche de la souris est relâché
        else if (Input.GetMouseButtonUp(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = false;
                initialMousePosition = Input.mousePosition;
                lastMousePosition = Input.mousePosition;
                return;
            }

            if (isDragging)
            {
                // Fin du glissement
                isDragging = false;
                Cursor.visible = true;
            }
        }
    }

    public void lookTile(GameObject tile)
    {
        Vector3 tilePos = tile.transform.position;

        // Arrêter la coroutine en cours si elle existe
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(TranslateToTile(tilePos));
    }

    IEnumerator TranslateToTile(Vector3 tilePos)
    {
        Vector3 targetPos = new Vector3(tilePos.x, transform.position.y, tilePos.z - ((transform.position.y/10)*3));
        float elapsed = 0f;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, elapsed / smoothTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos; // Ajustement final pour une position précise
        moveCoroutine = null;
    }
}
