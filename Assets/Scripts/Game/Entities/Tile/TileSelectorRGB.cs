using UnityEngine;
using System.Collections;


public class TileSelectorRGB : MonoBehaviour
{

    private MeshRenderer meshDeRendu;
    // coroutine
    private Coroutine changeColorCoroutine;



    void Awake()
    {
        meshDeRendu = GetComponent<MeshRenderer>();
    }


    void OnEnable(){
        changeColorCoroutine = StartCoroutine(PulseColorCoroutine());
    }

    void OnDisable(){
        StopCoroutine(changeColorCoroutine);
        meshDeRendu.material.SetColor("_EmissionColor", new Color(1.24487412f, 1.24487412f, 1.24487412f, 1));
    }



    // Coroutine pour faire un effet de changement de couleur continu sur le mesh de rendu


    private IEnumerator ChangeColorRDMCoroutine()
    {
        while (true)
        {
            Color currentColor = meshDeRendu.material.GetColor("_EmissionColor");
            float time = 0;
            float duration = 0.5f;
            // générer une couleur aléatoire
            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            while (time < duration)
            {
                time += Time.deltaTime;
                meshDeRendu.material.SetColor("_EmissionColor", Color.Lerp(currentColor, color, time / duration));
                yield return null;
            }
        }

    }

    private IEnumerator PulseColorCoroutine()
    {
        // Applique une couleur par défaut au début
        meshDeRendu.material.SetColor("_EmissionColor", new Color(1.24487412f, 1.24487412f, 1.24487412f, 1));

        while (true)
        {
            Color currentColor = meshDeRendu.material.GetColor("_EmissionColor");
            float time = 0;
            float duration = 0.5f;
            Color color = new Color(0,0,0);
            // si la couleur est au à 50, 50, 50, on la fait passer à 255, 255, 255
            if (currentColor.r == 1.24487412f)
            {
                color = new Color(4, 4, 4, 1);
            } else {
                color = new Color(1.24487412f, 1.24487412f, 1.24487412f, 1);
            }
            while (time < duration)
            {
                time += Time.deltaTime;
                meshDeRendu.material.SetColor("_EmissionColor", Color.Lerp(currentColor, color, time / duration));
                yield return null;
            }
        }

    }

}
