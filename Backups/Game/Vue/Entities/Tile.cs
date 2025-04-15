using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



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
    // [SerializeField] private TextMeshPro tileInfosOnMap;
    private GameObject building;



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
            SetupNodeRadius();
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

    private void SetupNodeRadius()
    {
        nodeRadius.enabled = false;
        if (Type == "node")
        {

            nodeRadius.enabled = true;
            nodeRadius.startWidth = 0.1f;
            nodeRadius.endWidth = 0.1f;
            List<Vector3> contour = new List<Vector3>();
            switch (lvl)
            {
                case 1:
                    // setup les points de la ligne renderer pour faire un cercle
                    contour = GenerateHexOutlinePoints(1);
                    nodeRadius.positionCount = contour.Count;
                    nodeRadius.SetPositions(contour.ToArray());
                    break;
                case 2:
                    contour = GenerateHexOutlinePoints(2);
                    nodeRadius.positionCount = contour.Count;
                    nodeRadius.SetPositions(contour.ToArray());
                    break;
                case 3:
                    contour = GenerateHexOutlinePoints(3);
                    nodeRadius.positionCount = contour.Count;
                    nodeRadius.SetPositions(contour.ToArray());
                    break;
                case 4:
                    contour = GenerateHexOutlinePoints(4);
                    nodeRadius.positionCount = contour.Count;
                    nodeRadius.SetPositions(contour.ToArray());
                    break;
                case 5:
                    contour = GenerateHexOutlinePoints(5);
                    nodeRadius.positionCount = contour.Count;
                    nodeRadius.SetPositions(contour.ToArray());
                    break;
                default:
                    nodeRadius.positionCount = 0;
                    break;
            }
        }
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



    public void PreSelect()
    {
        HighlightTile();
    }

    public void UnPreSelect()
    {
        UnHighlightTile();
    }








    private List<Vector3> GenerateHexOutlinePoints(int radius)
    {



        float r0 = 0; 
        float r1 = 0; 
        float r2 = 0; 
        int [] hexagonOffsets = new int[0];
        if (radius == 1 ){
            hexagonOffsets = new int[] { 0,1,0};
            r0 = 2.66f;
            r1 = 2f;
        }
        else if (radius == 2 ){
            hexagonOffsets = new int[] { 0,2,1,2,0};
            r0 = 4.45f;
            r1 = 4.1f;
            r2 = 3.625f;
        }
        else if (radius == 3 ){
            hexagonOffsets = new int[] { 0,0,1,1,0,1,1,0,1};
        }
        else if (radius == 4 ){
            hexagonOffsets = new int[] { 0,0,1,1,0,1,1,0,1,1,0,1};
        }
        else if (radius == 5 ){
            hexagonOffsets = new int[] { 0,0,1,1,0,1,1,0,1,1,0,1};
        }



        List<Vector3> points = new List<Vector3>();
        int nbPoints = NbPointsFromRadius(radius);

        for (int i = 0; i < nbPoints; i++)
        {
            float angleDeg = (360f / nbPoints) * i;
            float angleRad = angleDeg * Mathf.Deg2Rad; // Décalage de 20 degrés

            float currentRadius = 0;

            switch (hexagonOffsets[i % hexagonOffsets.Length]){
                case 0:
                    currentRadius = r0;
                    break;
                case 1:
                    currentRadius = r1;
                    break;
                case 2:
                    currentRadius = r2;
                    break;
            }

            float x = Mathf.Cos(angleRad) * currentRadius;
            float z = Mathf.Sin(angleRad) * currentRadius;

            points.Add(new Vector3(x, 0.03f, z));
        }

        return points;
    }

    private int NbPointsFromRadius(int radius)
    {
        return 6 + 12 * radius;
    }
    


}
