using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private GridGenerator gridGenerator;
    private CamController camControler;
    private ServerClient serverClient;

    // UI
    [Header("UI")]
    [SerializeField] private Button seeAllUnitsBtn;
    [SerializeField] private Button seeHQ;
    [SerializeField] private TMP_Text moneyText;

    
    public bool seeAllUnitsBool = false;


    // EFFECTS
    [Header("Effects")]
    [SerializeField] private LineRenderer moveUnitsLine;


    void Start(){

        serverClient = GetComponent<ServerClient>();
        gridGenerator = GetComponent<GridGenerator>();
        camControler = Camera.main.GetComponent<CamController>();

        // Events listeners
        seeAllUnitsBtn.onClick.AddListener(seeAllUnits);
        seeHQ.onClick.AddListener(seeHQBtnClic);
    }

    public void seeHQBtnClic(){
        // loop through all children
        foreach (Transform child in transform){
            if (child.gameObject.GetComponent<Tile>().type.Split(':')[0] == "hq" && child.gameObject.GetComponent<Tile>().owner == PlayerPrefs.GetString("username")){
                camControler.lookTile(child.gameObject);
                return;
            }
        }
    }

    public void seeAllUnits(){
        seeAllUnitsBool = !seeAllUnitsBool;
        foreach (Transform child in transform){
            child.gameObject.GetComponent<Tile>().moreInfo.SetActive(seeAllUnitsBool);
        }
    }


    public void UpdateMoney(string money)
    {
        // Mettre à jour l'argent du joueur
        moneyText.text = "¤ " + money;
        PlayerPrefs.SetInt("money", int.Parse(money));
    }


    public void SetupTiles(string jsonData)
    {
        // Désérialiser le JSON dans une structure adaptée à JsonUtility
        GameData gameData = JsonUtility.FromJson<GameData>(jsonData);

        // Vérification que les données de jeu ne soient pas nulles
        if (gameData == null || gameData.hexes == null || gameData.hexes.Length == 0)
        {
            return;
        }

        // Créer un dictionnaire à partir des hexes pour faciliter la manipulation
        Dictionary<string, HexData> hexDictionary = gameData.hexes
            .ToDictionary(hex => hex.key, hex => (HexData)hex);

        // Créer une liste des données d'hexagones
        List<Dictionary<string, object>> tilesData = hexDictionary.Values
            .Where(hex => hex != null) // S'assurer que hex n'est pas null
            .Select(hex => hex.ToDictionary())
            .ToList();

        // Vérification de la taille de la liste
        if (tilesData.Count == 0)
        {
        }

        // Mettre à jour la grille
        gridGenerator.UpdateGrid(tilesData);
    }


    public void buildBtnClic(string tile, string type)
    {
        // send a http request to the server
        serverClient.build(tile, type);
    }


    public void moveUnitsBtnClic(string origin , string destination, int units)
    {
        // send a http request to the server
        serverClient.moveUnits(origin, destination, units);
    }





    public IEnumerator moveUnitsAnimation(string[] moves, float animationDuration = 0.1f, float retractDuration = 0.01f)
    {
        // moves is an array of strings with the format ["x:z","x:z","x:z",...]
        moveUnitsLine.positionCount = moves.Length;

        Vector3[] positions = new Vector3[moves.Length];

        // Get the positions of the tiles based on grid coordinates
        for (int i = 0; i < moves.Length; i++)
        {
            string[] coords = moves[i].Split(':');

            int x = int.Parse(coords[0]);
            int z = int.Parse(coords[1]);
            float x1 = gridGenerator.GetHexCoordinates(x, z)[0];
            float z1 = gridGenerator.GetHexCoordinates(x, z)[1];

            positions[i] = new Vector3(x1, 0.5f, z1); // Store each position
        }

        // Animate the drawing of the line
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector3 startPosition = positions[i];
            Vector3 endPosition = positions[i + 1];
            float elapsedTime = 0f;

            // Interpolate between startPosition and endPosition over time
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / animationDuration); // Ensure t is between 0 and 1
                Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, t);
                
                // Set the line renderer positions dynamically
                moveUnitsLine.positionCount = i + 2; // Update position count for smooth transition
                moveUnitsLine.SetPosition(i, startPosition); // Set the initial position
                moveUnitsLine.SetPosition(i + 1, currentPosition); // Update the current position

                yield return null; // Wait for the next frame
            }

            // Once the interpolation is complete, set the end position of the current segment
            moveUnitsLine.SetPosition(i + 1, endPosition);
        }

        yield return new WaitForSeconds(0.5f); // Optional delay after the line is fully drawn

        // Animate the retraction of the line (from origin to destination)
        for (int i = 1; i < positions.Length; i++) // Start from 1 to retract from origin
        {
            float elapsedTime = 0f;

            // Interpolate the retraction between the first and the next point
            while (elapsedTime < retractDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / retractDuration); // Ensure t is between 0 and 1
                
                // Gradually move the first point to the second point
                Vector3 currentPosition = Vector3.Lerp(positions[i - 1], positions[i], t);

                // Update the first segment to retract the line from the origin
                moveUnitsLine.SetPosition(0, currentPosition); 

                // Keep the remaining part of the line as it is
                for (int j = 1; j < positions.Length - i; j++)
                {
                    moveUnitsLine.SetPosition(j, positions[j + i]);
                }

                yield return null; // Wait for the next frame
            }

            // After the retraction, reduce the position count to remove the retracted point
            moveUnitsLine.positionCount -= 1;
        }

        // Finally, clear the line renderer completely
        moveUnitsLine.positionCount = 0;
    }






}






// Classe GameData pour contenir le tableau des hexagones
[Serializable]
public class GameData
{
    public HexData[] hexes;  // Tableau d'hexagones
}

[Serializable]
public class HexData
{
    public string key;
    public int units;
    public string type;
    public string owner;
    public string color;

    // La conversion en dictionnaire
    public Dictionary<string, object> ToDictionary()
    {
        var dict = new Dictionary<string, object>
        {
            {"type", type},
            {"owner", owner},
            {"units", units},
            {"key", key},
            {"color", color}
        };
        return dict;
    }
}
