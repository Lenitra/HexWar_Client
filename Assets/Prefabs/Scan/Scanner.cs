using UnityEngine;

public class Scanner : MonoBehaviour
{
    public float scanSpeed = 10f;
    public float scanWidth = 1f;
    public float maxRadius = 20f;
    public Color scanColor = Color.cyan;

    private float scanRadius = 0f;
    private bool scanning = false;
    private bool inward = false;

    private Renderer[] scannableRenderers;

    void Start()
    {
        scannableRenderers = FindObjectsOfType<Renderer>();

        foreach (var rend in scannableRenderers)
        {
            foreach (var mat in rend.materials)
            {
                if (mat.HasProperty("_ScanOrigin"))
                {
                    mat.SetVector("_ScanOrigin", transform.position);
                    mat.SetColor("_ScanColor", scanColor);
                    mat.SetFloat("_ScanWidth", scanWidth);
                    mat.SetFloat("_ScanRadius", 0f);
                }
            }
        }
    }

    void Update()
    {
        if (!scanning) return;

        scanRadius += (inward ? -1 : 1) * scanSpeed * Time.deltaTime;

        foreach (var rend in scannableRenderers)
        {
            foreach (var mat in rend.materials)
            {
                if (mat.HasProperty("_ScanRadius"))
                {
                    mat.SetFloat("_ScanRadius", scanRadius);
                }
            }
        }

        if ((!inward && scanRadius >= maxRadius) || (inward && scanRadius <= 0f))
        {
            ResetScanEffect(); // 🧼 Nettoie l’effet avant destruction
            scanning = false;
            Destroy(gameObject);
        }
    }

    public void StartScanOutward()
    {
        scanRadius = 0f;
        inward = false;
        scanning = true;
    }

    public void StartScanInward()
    {
        scanRadius = maxRadius;
        inward = true;
        scanning = true;
    }

    private void ResetScanEffect()
    {
        foreach (var rend in scannableRenderers)
        {
            foreach (var mat in rend.materials)
            {
                if (mat.HasProperty("_ScanRadius"))
                {
                    mat.SetFloat("_ScanRadius", -999f); // Valeur hors-scan
                }
            }
        }
    }
}
