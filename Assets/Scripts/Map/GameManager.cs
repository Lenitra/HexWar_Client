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
    private PlayerControler playerControler;
    // coroutine du pooling régulier
    private Coroutine poolingCoroutine;
    private bool isPooling = true;

    // EFFECTS
    [Header("Effects")]
    [SerializeField] private LineRenderer moveUnitsLine;

    void Start()
    {
        serverClient = GetComponent<ServerClient>();
        gridGenerator = GetComponent<GridGenerator>();
        camControler = Camera.main.GetComponent<CamController>();
        playerControler = GetComponent<PlayerControler>();

        // faire le premier pool de la map
        serverClient.updateMap();
        // lancer le pooling régulier
        poolingCoroutine = StartCoroutine(pooling());
    }

    public void togglePooling(bool state)
    {
        if (state && !isPooling)
        {
            poolingCoroutine = StartCoroutine(pooling());
        }
        if (!state && isPooling)
        {
            StopCoroutine(poolingCoroutine);
        }
    }

    // Coroutine de pooling régulier
    private IEnumerator pooling()
    {
        while (true)
        {
            // Pooling de la map et de l'argent
            serverClient.updateMap();
            yield return new WaitForSeconds(1f);
        }
    }

    // Depuis le régulier pool du serveur, actualise la valeur de l'argent du joueur et appelle le PlayerControler pour mettre à jour la vue
    public void UpdateMoney(string money)
    {
        PlayerPrefs.SetInt("money", int.Parse(money));
        playerControler.UpdateMoney(money);
    }

    // Met à jour la grille de jeu en recevant les données JSON des hexagones
    public void SetupTiles(string jsonData)
    {
        // JsonUtility ne supporte pas les tableaux en racine.
        // On enveloppe donc le JSON dans un objet avec une propriété "hexes".
        string wrappedJson = "{\"hexes\":" + jsonData + "}";
        GameData gameData = JsonUtility.FromJson<GameData>(wrappedJson);

        // Vérifier que les données de jeu ne soient pas nulles
        if (gameData == null || gameData.hexes == null || gameData.hexes.Length == 0)
        {
            return;
        }

        // Créer un dictionnaire pour stocker les données des hexagones
        Dictionary<string, HexData> hexDictionary = gameData.hexes.ToDictionary(hex => $"{hex.x}:{hex.y}");
        

        // Créer une liste des données d'hexagones pour mettre à jour la grille
        List<Dictionary<string, object>> tilesData = hexDictionary.Values
            .Select(hex => hex.ToDictionary())
            .ToList();

        // Mettre à jour la grille via le générateur
        gridGenerator.UpdateGrid(tilesData);
    }

    public void buildBtnClic(string tile, string type, int lvl)
    {
        // send a http request to the server
        serverClient.build(tile, type);
    }

    public void moveUnitsBtnClic(string origin, string destination, int units)
    {
        // send a http request to the server
        serverClient.moveUnits(origin, destination, units);
    }

    // Animation de déplacement des unités
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
                float t = Mathf.Clamp01(elapsedTime / animationDuration);
                Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, t);

                // Set the line renderer positions dynamically
                moveUnitsLine.positionCount = i + 2;
                moveUnitsLine.SetPosition(i, startPosition);
                moveUnitsLine.SetPosition(i + 1, currentPosition);

                yield return null;
            }

            // Once the interpolation is complete, set the end position of the current segment
            moveUnitsLine.SetPosition(i + 1, endPosition);
        }

        yield return new WaitForSeconds(0.5f);

        // Animate the retraction of the line (from origin to destination)
        for (int i = 1; i < positions.Length; i++)
        {
            float elapsedTime = 0f;
            while (elapsedTime < retractDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / retractDuration);
                Vector3 currentPosition = Vector3.Lerp(positions[i - 1], positions[i], t);
                moveUnitsLine.SetPosition(0, currentPosition);
                for (int j = 1; j < positions.Length - i; j++)
                {
                    moveUnitsLine.SetPosition(j, positions[j + i]);
                }
                yield return null;
            }
            moveUnitsLine.positionCount -= 1;
        }

        moveUnitsLine.positionCount = 0;
    }
}

// Classe GameData pour contenir le tableau des hexagones
[Serializable]
public class GameData
{
    public HexData[] hexes;  // Cette propriété recevra le tableau d'hexagones enveloppé.
}

[Serializable]
public class HexData
{
    public int x;
    public int y;
    public int lvl;  // Utilisation d'un int nullable pour accepter null
    public int units;
    public string type;
    public string owner;
    public string color;

    // Conversion en dictionnaire (le champ _id n'étant pas présent, il n'est pas inclus)
    public Dictionary<string, object> ToDictionary()
    {
        var dict = new Dictionary<string, object>
        {
            {"x", x},
            {"y", y},
            {"lvl", lvl},
            {"units", units},
            {"type", type},
            {"owner", owner},
            {"color", color}
        };
        return dict;
    }
}
