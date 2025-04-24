using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;



public class Tile : MonoBehaviour
{

    [Header("Tile Attributes")]
    private int x = int.MaxValue;
    private int y = int.MaxValue;
    [SerializeField] private int units = 0;
    [SerializeField] private string owner = "";
    [SerializeField] private int lvl = 0;
    [SerializeField] private string type = "";
    [SerializeField] private string color = "0|0|0";
    [SerializeField] private string protect = "";
    [SerializeField] private bool hasShield = false;


    [Header("Prefabs des types de tiles")]
    [SerializeField] private GameObject[] typesPrefabs;



    [Header("Elements enfants")]
    [SerializeField] private MeshRenderer meshDeRendu;
    [SerializeField] private GameObject glow;
    [SerializeField] private GameObject shieldObject;
    [SerializeField] private LineRenderer nodeRadius;



    private GameObject building;
    private float gridGap = 0.05f;
    private float selectElevation = 1f;



    #region Getters & Setters
    public int X
    {
        get { return x; }
        set { x = value; }
    }

    public int Y
    {
        get { return y; }
        set { y = value; }
    }

    public int Units
    {
        get { return units; }
        set
        {
            units = value;
        }
    }

    public string Owner
    {
        get { return owner; }
        set
        {
            owner = value;
        }
    }

    public int Lvl
    {
        get { return lvl; }
        set
        {
            lvl = value;
            if (type == "node")
            {
                SetupNodeRadius(lvl + 1);
            }
        }
    }

    public string Type
    {
        get { return type; }
        set
        {
            type = value;
            SetupType();
        }
    }

    public string Color
    {
        get { return color; }
        set
        {
            color = value;
            SetupColor();
        }
    }

    public string Protect
    {
        get { return protect; }
        set
        {
            protect = value;
            if (protect != "")
            {
                System.DateTime protectDate = System.DateTime.Parse(protect);
                if (protectDate > System.DateTime.Now)
                {
                    this.HasShield = true;
                }
                else
                {
                    this.HasShield = false;
                }
            }

        }
    }


    public bool HasShield
    {
        get { return hasShield; }
        set
        {
            hasShield = value;
            SetupShield();
        }
    }

    #endregion


    // ToString 
    public string ToString()
    {
        return "Tile: " + this.X + ":" + this.Y + " | Owner: " + this.Owner + " | Type: " + this.Type + " | Lvl: " + this.Lvl;
    }



    // Convertit les coordonnées d'un hex en coordonnées pixel
    public float[] GetHexCoordinates()
    {
        // Axial -> Pixel (flat top)
        float px = (1.5f + gridGap) * X;
        float pz = (Mathf.Sqrt(3f) + gridGap) * (Y + X / 2f);

        return new float[] { px, pz };
    }

    public string Coords()
    {
        return "(" + this.X + ", " + this.Y + ")";
    }
    public void SetupTile(Hex hexData)
    {
        if (this.X == int.MaxValue || this.Y == int.MaxValue)
        {
            this.X = hexData.x;
            this.Y = hexData.y;
            this.Owner = hexData.owner;
            this.Type = hexData.type;
            this.Lvl = hexData.lvl;
            this.Color = hexData.color;
            this.Units = hexData.units;
            this.Protect = hexData.protect;
            // Set le nom de la tile
            this.gameObject.name = "Tile " + this.X + ":" + this.Y;
            // Set la position de la tile
            float[] coords = GetHexCoordinates();
            this.transform.position = new Vector3(coords[0], -5, coords[1]);
            return;
        }



        if (this.X != hexData.x || this.Y != hexData.y)
        {
            Debug.LogError("Erreur: Tentative de modification d'une tile avec des coordonnées différentes");
            return;
        }

        if (this.Lvl != hexData.lvl)
        {
            this.Lvl = hexData.lvl;
        }
        if (this.Units != hexData.units)
        {
            this.Units = hexData.units;
        }
        if (this.Owner != hexData.owner)
        {
            this.Owner = hexData.owner;
        }
        if (this.Type != hexData.type)
        {
            this.Type = hexData.type;
        }
        if (this.Color != hexData.color)
        {
            this.Color = hexData.color;
        }
        if (this.Protect != hexData.protect)
        {
            this.Protect = hexData.protect;
        }
    }




    private void SetupShield()
    {
        if (hasShield)
        {
            shieldObject.SetActive(true);
        }
        else
        {
            shieldObject.SetActive(false);
        }
    }

    private void SetupNodeRadius(int radius)
    {
        StartCoroutine(NodeRadiusCalc(radius));

    }

    private IEnumerator NodeRadiusCalc(int radius)
    {
        yield return new WaitForSeconds(1f);
        GameManager gameManager = FindObjectOfType<GameManager>();


        List<Vector3> positions = new List<Vector3>();
        positions = gameManager.GetTileGroupContour(gameManager.GetTilesInRadius(this, radius));

        nodeRadius.positionCount = positions.Count;
        nodeRadius.SetPositions(positions.ToArray());
        nodeRadius.startWidth = 0.1f;
        nodeRadius.endWidth = 0.1f;
        nodeRadius.startColor = new Color(1, 1, 1, 1);
        nodeRadius.endColor = new Color(1, 1, 1, 1);
        nodeRadius.loop = true;
        nodeRadius.gameObject.SetActive(true);
    }


    private void SetupColor()
    {
        Material material = meshDeRendu.material;

        // Forcer le matériau en mode Fadedd
        material.SetFloat("_Mode", 2); // 2 correspond à Fade dans le shader standard
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.SetOverrideTag("RenderType", "Transparent");

        if (string.IsNullOrEmpty(owner))
        {
            // Tile non possédée : couleur noire avec alpha à 0.75 pour un effet de fade
            Color fadeColor = new Color(0f, 0f, 0f, 0f);
            material.SetColor("_Color", fadeColor);
            material.SetColor("_EmissionColor", fadeColor);

        }
        else
        {
            // Récupération et ajustement de la couleur du propriétaire
            string[] rgb = this.Color.Split('|');
            int minColor = 20;

            // Vérifier et ajuster pour que chaque composante soit au moins à 20
            for (int i = 0; i < 3; i++)
            {
                int value = int.Parse(rgb[i]);
                if (value < minColor)
                    minColor = value;
            }
            if (minColor < 20)
            {
                for (int i = 0; i < 3; i++)
                {
                    int value = int.Parse(rgb[i]);
                    rgb[i] = (value + 20 - minColor).ToString();
                }
            }

            // Création de la couleur avec alpha = 1 (opaque) tout en restant en mode Fade
            Color ownerColor = new Color(
                float.Parse(rgb[0]) / 255f,
                float.Parse(rgb[1]) / 255f,
                float.Parse(rgb[2]) / 255f,
                1f
            );
            material.SetColor("_Color", ownerColor);
            material.SetColor("_EmissionColor", ownerColor);

            // Ajuster la position et l'échelle du mesh pour la tile possédée
            Transform baseTransform = meshDeRendu.transform;
            // baseTransform.localScale = new Vector3(baseTransform.localScale.x, baseTransform.localScale.y, 2f);
            // baseTransform.localPosition = new Vector3(baseTransform.localPosition.x, -0.126f, baseTransform.localPosition.z);

        }
    }





    private void SetupType()
    {
        int index = -1;
        switch (this.type.ToLower())
        {
            case "node":
                index = 0;
                break;
            case "miner":
                index = 1;
                break;
            case "barrack":
                index = 2;
                break;
            case "radar":
                index = 3;
                break;
            default:
                index = -1;
                break;
        }

        // instancier le prefab avec l'index
        if (index != -1)
        {
            building = Instantiate(typesPrefabs[index], this.transform);
            building.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        }
        else
        {
            Destroy(building);
        }

    }


    public void HighlightTile()
    {
        glow.SetActive(true);
    }

    public void UnHighlightTile()
    {
        glow.SetActive(false);
    }




void Update()
{
    Vector3[] corners = GetCorners();

    for (int i = 0; i < 6; i++)
    {
        Debug.DrawLine(corners[i], corners[(i + 1) % 6], UnityEngine.Color.red);
    }
}




    #region Systèmes pour le calcul de points dans l'espace

    public Vector3 GetHexCenter()
    {
        float[] coords = GetHexCoordinates();
        return new Vector3(coords[0], this.transform.position.y, coords[1]);
    }

    public Vector3[] GetCorners(float hexSize = 1f)
    {
        Vector3[] corners = new Vector3[6];

        Vector3 center = this.transform.position;

        for (int i = 0; i < 6; i++)
        {
            float angleDeg = 60 * i; // 30° offset for flat-topped hexes
            float angleRad = Mathf.Deg2Rad * angleDeg;

            float x = center.x + Mathf.Cos(angleRad) * hexSize;
            float z = center.z + Mathf.Sin(angleRad) * hexSize;

            corners[i] = new Vector3(x, -5f, z);
        }



        return corners;
    }

    #endregion









    #region Animations et coroutines
    public IEnumerator DestructionCoroutine()
    {
        // TODO : ajouter une animation de destruction de la tile
        yield return new WaitForSeconds(0.5f);
        Destroy(this.gameObject);
    }

    public IEnumerator SelectCoroutine()
    {
        float[] coords = GetHexCoordinates();
        this.transform.position = new Vector3(coords[0], -5, coords[1]);
        Vector3 pos = new Vector3(this.transform.position.x, this.transform.position.y + selectElevation, this.transform.position.z);
        float duration = 0.5f;
        float elapsedTime = 0f;
        Vector3 startPos = this.transform.position;
        while (elapsedTime < duration)
        {
            this.transform.position = Vector3.Lerp(startPos, pos, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        this.transform.position = pos;
    }

    public IEnumerator UnSelectCoroutine()
    {
        float[] coords = GetHexCoordinates();
        this.transform.position = new Vector3(coords[0], -5 + selectElevation, coords[1]);
        Vector3 endPos = new Vector3(this.transform.position.x, this.transform.position.y - selectElevation, this.transform.position.z);
        Vector3 startPos = this.transform.position;

        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            this.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / duration));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        this.transform.position = endPos;
    }
    #endregion

}
