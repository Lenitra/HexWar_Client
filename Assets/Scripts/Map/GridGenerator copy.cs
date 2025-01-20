// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class GridGenerator : MonoBehaviour
// {
//     private CamController camControler;
//     [SerializeField] private GameObject hexPrefab;
//     private float hexSize = 0.5f;
//     private float animmYOffset = 1.5f;
//     private float gridGap = 0.1f;
//     private bool firstPool = true;

//     void Start()
//     {
//         camControler = Camera.main.GetComponent<CamController>();
//     }

//     public GameObject getHex(int x, int z)
//     {
//         // Recherche un hex existant parmi les enfants du GameObject courant
//         foreach (Transform child in transform)
//         {
//             if (child.name == "Hexagon " + x + ":" + z)
//             {
//                 return child.gameObject;
//             }
//         }
//         return null;
//     }

//     public void UpdateGrid(List<Dictionary<string, object>> tilesData)
//     {
//         // Mettre à jour ou instancier chaque tuile
//         foreach (Dictionary<string, object> tileData in tilesData)
//         {
//             int x = int.Parse(tileData["key"].ToString().Split(':')[0]);
//             int z = int.Parse(tileData["key"].ToString().Split(':')[1]);

//             GameObject tile = getHex(x, z);
//             if (tile != null)
//             {
//                 // Déjà présent : on met à jour son contenu si nécessaire
//                 Tile tileComponent = tile.GetComponent<Tile>();

//                 int units = (int)tileData["units"];
//                 string owner = (string)tileData["owner"];
//                 string type = (string)tileData["type"];
//                 string color = (string)tileData["color"];

//                 // Actualiser seulement si des changements ont eu lieu
//                 if (tileComponent.units != units
//                     || tileComponent.owner != owner
//                     || tileComponent.type != type)
//                 {
//                     tileComponent.setupTile(units, owner, type, color);
//                 }
//             }
//             else
//             {
//                 // Pas encore créé : on l’instancie
//                 StartCoroutine(InstantiateHexagon(x, z, tileData));
//             }
//         }

//         // Supprimer les hex qui ne sont plus présents dans tilesData
//         foreach (Transform child in transform)
//         {
//             if (child.name.Contains("Hexagon"))
//             {
//                 string childKey = child.name.Split(' ')[1];
//                 if (!tilesData.Exists(tile => tile["key"].ToString() == childKey))
//                 {
//                     Destroy(child.gameObject);
//                 }
//             }
//         }
//     }

//     IEnumerator InstantiateHexagon(int x, int z, Dictionary<string, object> tileData)
//     {
//         // Crée un nouvel hex
//         GameObject hex = GameObject.Instantiate(hexPrefab);
//         hex.name = "Hexagon " + x + ":" + z;
//         hex.transform.SetParent(this.transform);

//         // Calcule sa position
//         float[] coords = GetHexCoordinates(x, z);
//         hex.transform.position = new Vector3(coords[0], 0, coords[1]);

//         yield return new WaitForSeconds(0.1f);

//         // Setup initial de la tuile
//         int units = (int)tileData["units"];
//         string owner = (string)tileData["owner"];
//         string typeId = (string)tileData["type"];
//         string color = (string)tileData["color"];

//         hex.GetComponent<Tile>().setupTile(units, owner, typeId, color);

//         // Si c’est le premier tour de boucle, on centre la caméra sur le HQ du joueur
//         if (firstPool)
//         {
//             Tile tileComp = hex.GetComponent<Tile>();
//             // Suppose que le type est du style "hq:..." et qu’on compare à PlayerPrefs
//             if (tileComp.type.Split(':')[0] == "hq" && tileComp.owner == PlayerPrefs.GetString("username"))
//             {
//                 camControler.lookTile(hex);
//                 firstPool = false;
//             }
//         }
//     }

//     public void GenerateTiles(List<Dictionary<string, object>> tilesData)
//     {
//         foreach (Dictionary<string, object> tileData in tilesData)
//         {
//             int x = (int)tileData["x"];
//             int z = (int)tileData["z"];
//             string owner = (string)tileData["owner"];
//             int units = (int)tileData["units"];
//             string type_id = (string)tileData["type"];
//             string color = (string)tileData["color"];

//             GameObject hex = GameObject.Instantiate(hexPrefab);
//             hex.name = "Hexagon " + x + "" + z;
//             hex.transform.SetParent(this.transform);

//             float[] coords = GetHexCoordinates(x, z);
//             hex.transform.position = new Vector3(coords[0], 0, coords[1]);

//             hex.GetComponent<Tile>().setupTile(units, owner, type_id, color);
//         }
//     }

//     // Conversion (x,z) -> position dans Unity pour un offset "even-q" pointy-top
//     public float[] GetHexCoordinates(int x, int z)
//     {
//         // Dimensions de l'hexagone
//         float hexWidth = hexSize * 2f;             // Diamètre horizontal
//         float hexHeight = Mathf.Sqrt(3f) * hexSize; // Hauteur pointy-top

//         // Décalage horizontal entre les colonnes, en tenant compte du gap
//         float offsetX = (hexWidth + gridGap) * 0.75f;

//         // Position de base (col * offsetX, row * (hexHeight + gridGap))
//         float xPos = x * offsetX;
//         float zPos = z * (hexHeight + gridGap);

//         // Pour un offset even-q : 
//         //   - les colonnes "paires" (x % 2 == 0) sont décalées d’une demi-hauteur
//         //   - les colonnes impaires ne sont pas décalées
//         if ((x % 2) == 0)
//         {
//             zPos += (hexHeight + gridGap) / 2f;
//         }

//         return new float[] { xPos, zPos };
//     }

//     IEnumerator AnimateHexagon(GameObject hex)
//     {
//         // Animation de montée
//         for (int i = 0; i < animmYOffset * 10 + 1; i++)
//         {
//             hex.transform.position = new Vector3(
//                 hex.transform.position.x,
//                 hex.transform.position.y + 0.1f,
//                 hex.transform.position.z
//             );
//             yield return new WaitForSeconds(0.0001f);
//         }
//         // Animation de redescente
//         for (int i = 0; i < 5; i++)
//         {
//             hex.transform.position = new Vector3(
//                 hex.transform.position.x,
//                 hex.transform.position.y - 0.2f,
//                 hex.transform.position.z
//             );
//             yield return new WaitForSeconds(0.0001f);
//         }
//     }
// }
