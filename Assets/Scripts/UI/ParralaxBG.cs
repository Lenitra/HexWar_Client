using UnityEngine;

public class ParralaxBG : MonoBehaviour
{
    [Header("Paramètres de la Parallaxe")]
    [Tooltip("Le GameObject (ou Transform) parent qui contient les layers.")]
    [SerializeField] private Transform parallaxParent;

    [Tooltip("Facteurs de défilement pour chaque layer (x et z).")]
    [SerializeField] 
    private float[] parallaxScales;

    private Transform[] layers;
    private Vector3 lastCamPos;

    private void Awake()
    {
        if (parallaxParent == null)
        {
            Debug.LogError("ParallaxParent n'est pas assigné dans l'inspecteur !");
            return;
        }

        // On récupère tous les enfants du parallaxParent
        int childCount = parallaxParent.childCount;
        layers = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            layers[i] = parallaxParent.GetChild(i);
        }
        parallaxScales = new float[childCount];
        parallaxScales[0] = 0;
        parallaxScales[1] = 0.05f;
        parallaxScales[2] = 0.08f;
    }

    private void Start()
    {
        // On stocke la position initiale de la caméra
        lastCamPos = transform.position;

        // Vérification : le tableau parallaxScales doit avoir la même taille
        // que le nombre de layers. Sinon, on affiche un warning.
        if (layers != null && parallaxScales.Length != layers.Length)
        {
            Debug.LogWarning("Le nombre de parallaxScales ne correspond pas " +
                             "au nombre de layers. Ajustez les valeurs dans l'inspecteur !");
        }
    }

    private void LateUpdate()
    {
        if (layers == null || layers.Length == 0) return;

        // Calcul du déplacement de la caméra depuis la dernière frame
        Vector3 deltaMovement = transform.position - lastCamPos;

        // Pour chaque layer, on applique un décalage en fonction de son facteur de parallaxe
        for (int i = 0; i < layers.Length; i++)
        {
            if (i >= parallaxScales.Length) break;  // Sécurité si pas assez de scale

            float scale = parallaxScales[i];

            // On ne déplace que sur les axes X et Z
            Vector3 newPos = layers[i].position;
            newPos.x += deltaMovement.x * scale;
            newPos.z += deltaMovement.z * scale;

            layers[i].position = newPos;
        }

        // Mise à jour de la dernière position de la caméra
        lastCamPos = transform.position;
    }
}
