using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{

    [SerializeField] private GameObject hexPrefab;
    private float hexSize = 0.5f;
    private float animmYOffset = 1.5f; 
    private float gridGap = 0.1f;


    public GameObject getHex(int x, int z){
        foreach (Transform child in transform){
            if (child.name == "Hexagon " + x + ":" + z){
                return child.gameObject;
            }
        }
        return null;
    }


    public void UpdateGrid(List<Dictionary<string, object>> tilesData)
    {
        foreach (Dictionary<string, object> tileData in tilesData)
        {
            int x = int.Parse(tileData["key"].ToString().Split(':')[0]);
            int z = int.Parse(tileData["key"].ToString().Split(':')[1]);
            
            GameObject tile = getHex(x, z);
            if (tile != null)
            {
                Tile tileComponent = tile.GetComponent<Tile>();

                int units = (int)tileData["units"];
                string owner = (string)tileData["owner"];
                string type = (string)tileData["type"];
                string color = (string)tileData["color"];

                // Regrouper les mises à jour dans setupTile si nécessaire
                if (tileComponent.units != units || tileComponent.owner != owner || tileComponent.type != type)
                {
                    tileComponent.setupTile(units, owner, type, color); // Appel unique si quelque chose a changé
                }
            
            }
            else
            {
                StartCoroutine(InstantiateHexagon(x, z, tileData));
            }
        }
        
        // delete the hexagons that are not in the tilesData
        foreach (Transform child in transform){
            if (child.name.Contains("Hexagon") && !tilesData.Exists(tile => tile["key"].ToString() == child.name.Split(' ')[1])){
                Destroy(child.gameObject);
            }
        }
    }

    IEnumerator InstantiateHexagon(int x, int z, Dictionary<string, object> tileData){
        // if the tile is not created, create it
        GameObject hex = GameObject.Instantiate(hexPrefab);
        hex.name = "Hexagon " + x + ":" + z;
        hex.transform.SetParent(this.transform);

        hex.transform.position = new Vector3(GetHexCoordinates(x, z)[0], 0, GetHexCoordinates(x, z)[1]);

        yield return new WaitForSeconds(0.1f);
        hex.GetComponent<Tile>().setupTile((int)tileData["units"], (string)tileData["owner"], (string)tileData["type"], (string)tileData["color"]);


    }



    public void GenerateTiles(List<Dictionary<string, object>> tilesData)
    {
        foreach (Dictionary<string, object> tileData in tilesData)
        {
            int x = (int)tileData["x"];
            int z = (int)tileData["z"];
            string owner = (string)tileData["owner"];
            int units = (int)tileData["units"];
            string type_id = (string)tileData["type"];
            string color = (string)tileData["color"];

            GameObject hex = GameObject.Instantiate(hexPrefab);
            hex.name = "Hexagon " + x + "" + z;
            hex.transform.SetParent(this.transform);
            hex.transform.position = new Vector3(GetHexCoordinates(x, z)[0], 0, GetHexCoordinates(x, z)[1]);
            hex.GetComponent<Tile>().setupTile(units, owner, type_id, color);
        }
    }


    // Permet de récupérer les coordonnées dans unity à partir de la position dans le tableau
    public float[] GetHexCoordinates(int x, int z) 
    {
        // Calculer les dimensions d'un hexagone
        float hexWidth = hexSize * 2f; // Largeur (diamètre) de l'hexagone
        float hexHeight = Mathf.Sqrt(3) * hexSize; // Hauteur de l'hexagone (sqrt(3) * taille)
        
        // Décalage horizontal entre les colonnes avec le gridGap
        float offsetX = (hexWidth + gridGap) * 0.75f; 
        
        // Calcul de la position X et Z en fonction des coordonnées x et z
        float xPos = x * offsetX;
        float zPos = z * (hexHeight + gridGap); // Prendre en compte le gridGap dans la position verticale

        // Décaler les hexagones sur les lignes impaires
        if (x % 2 == 1) {
            zPos += (hexHeight + gridGap) / 2f; // Décalage pour les lignes impaires
        }

        // Retourner les coordonnées X et Z sous forme de tableau
        return new float[] { xPos, zPos };
    }



    IEnumerator AnimateHexagon(GameObject hex){
        for (int i = 0; i < animmYOffset*10+1; i++){
            hex.transform.position = new Vector3(hex.transform.position.x, hex.transform.position.y + 0.1f, hex.transform.position.z);
            yield return new WaitForSeconds(0.0001f);
        }
        for (int i = 0; i < 5; i++){
            hex.transform.position = new Vector3(hex.transform.position.x, hex.transform.position.y - 0.2f, hex.transform.position.z);
            yield return new WaitForSeconds(0.0001f);
        }
    }



}
