using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    private CamController camControler;
    [SerializeField] private GameObject hexPrefab;

    // Rayon (en Unity) d'un hex "plat en haut"
    private float hexSize = 0.5f;
    private float animmYOffset = 1.5f;
    private float gridGap = 0.1f;
    private bool firstPool = true;

    void Start()
    {
        camControler = Camera.main.GetComponent<CamController>();
    }

    public GameObject getHex(int x, int z)
    {
        foreach (Transform child in transform)
        {
            if (child.name == "Hexagon " + x + ":" + z)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public void UpdateGrid(List<Dictionary<string, object>> tilesData)
    {
        // Mise à jour ou instanciation
        foreach (Dictionary<string, object> tileData in tilesData)
        {
            // Récupère x, z (qui sont en réalité x,y axiaux)
            int x = int.Parse(tileData["key"].ToString().Split(':')[0]);
            int z = int.Parse(tileData["key"].ToString().Split(':')[1]);

            GameObject tile = getHex(x, z);
            if (tile != null)
            {
                // Déjà présent : on met à jour
                Tile tileComponent = tile.GetComponent<Tile>();

                int units = (int)tileData["units"];
                string owner = (string)tileData["owner"];
                string type = (string)tileData["type"];
                string color = (string)tileData["color"];

                // Actualiser seulement si changements
                if (tileComponent.units != units
                    || tileComponent.owner != owner
                    || tileComponent.type != type)
                {
                    tileComponent.setupTile(units, owner, type, color);
                }
            }
            else
            {
                // Hex à instancier
                StartCoroutine(InstantiateHexagon(x, z, tileData));
            }
        }

        // Supprimer les hex qui ne sont plus présents
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Hexagon"))
            {
                string childKey = child.name.Split(' ')[1]; // "x:z"
                if (!tilesData.Exists(tile => tile["key"].ToString() == childKey))
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    IEnumerator InstantiateHexagon(int x, int z, Dictionary<string, object> tileData)
    {
        // Création
        GameObject hex = Instantiate(hexPrefab);
        hex.name = "Hexagon " + x + ":" + z;
        hex.transform.SetParent(this.transform);

        // Positionnement en Unity
        float[] coords = GetHexCoordinates(x, z);
        hex.transform.position = new Vector3(coords[0], 0, coords[1]);

        yield return null;

        // Setup
        int units = (int)tileData["units"];
        string owner = (string)tileData["owner"];
        string typeId = (string)tileData["type"];
        string color = (string)tileData["color"];

        Tile hextile = hex.GetComponent<Tile>();
        hextile.setupTile(units, owner, typeId, color);
        
        // Centrer la caméra si c'est le premier HQ
        if (firstPool)
        {
            if (hextile.type.Split(':')[0].ToUpper() == "HQ"
                && hextile.owner == PlayerPrefs.GetString("username"))
            {
                camControler.lookTile(hex);
                firstPool = false;
            }
        }
    }


    // TODO: delete -> Not used
    // public void GenerateTiles(List<Dictionary<string, object>> tilesData)
    // {
    //     foreach (Dictionary<string, object> tileData in tilesData)
    //     {
    //         int x = (int)tileData["x"];
    //         int z = (int)tileData["z"]; // encore une fois, "z" = "y" axial
    //         string owner = (string)tileData["owner"];
    //         int units = (int)tileData["units"];
    //         string type_id = (string)tileData["type"];
    //         string color = (string)tileData["color"];

    //         GameObject hex = Instantiate(hexPrefab);
    //         hex.name = "Hexagon " + x + ":" + z;
    //         hex.transform.SetParent(this.transform);

    //         float[] coords = GetHexCoordinates(x, z);
    //         hex.transform.position = new Vector3(coords[0], 0, coords[1]);

    //         hex.GetComponent<Tile>().setupTile(units, owner, type_id, color);
    //     }
    // }

    /// <summary>
    /// GetHexCoordinates(x, z) :
    /// ICI, x et z sont en réalité les coordonnées axiales (q, r)
    /// pour un "flat top" (base plate).
    /// 
    /// Formule standard Red Blob Games :
    ///   px = 1.5f * hexSize * x
    ///   pz = sqrt(3) * hexSize * (z + x/2)
    /// 
    /// On ajoute (éventuellement) le "gridGap" si vous voulez espacer un peu.
    /// </summary>
    public float[] GetHexCoordinates(int x, int z)
    {
        // Axial -> Pixel (flat top)
        float px = 1.5f * hexSize * x;
        float pz = Mathf.Sqrt(3f) * hexSize * (z + x / 2f);

        // Si vous voulez ajouter un "gridGap", vous pouvez
        // px += x * gridGap;
        // pz += z * gridGap; 
        // ... ou toute autre logique

        return new float[] { px, pz };
    }

    IEnumerator AnimateHexagon(GameObject hex)
    {
        // Montée
        for (int i = 0; i < animmYOffset * 10 + 1; i++)
        {
            hex.transform.position += new Vector3(0, 0.1f, 0);
            yield return new WaitForSeconds(0.0001f);
        }
        // Redescente
        for (int i = 0; i < 5; i++)
        {
            hex.transform.position -= new Vector3(0, 0.2f, 0);
            yield return new WaitForSeconds(0.0001f);
        }
    }
}
