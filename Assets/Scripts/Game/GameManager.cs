using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    private ServerClient serverClient; // Le service de communication avec le serveur
    private GameView gameView; // Le service de communication avec le serveur
    private Dictionary<int, Tile> grid = new Dictionary<int, Tile>(); // Dictionnaire des cases de la carte (ID -> Tile)
    [SerializeField] private GameObject tilePrefab; // Préfabriqué de la tile





    void Start()
    {
        serverClient = GetComponent<ServerClient>();
        gameView = GetComponent<GameView>();

        // Récupérer des données du serveur
        // serverClient.GetBuildPrices();
        // serverClient.GetWiki();
        serverClient.GetPlayerInfo();


    }



    #region Gestion des infos du joueur
    public void UpdatePlayerInfo(PlayerInfoApi playerInfo)
    {
        gameView.SetPlayerName(playerInfo.username);
        gameView.SetMoney(playerInfo.money);
        gameView.SetPower(playerInfo.power);
    }
    #endregion



    #region Gestion de la carte

    public void UpdateMap(TileApi[] tiles)
    {
        foreach (TileApi tileApi in tiles)
        {
            // Vérifier si la tile existe déjà dans le dictionnaire
            if (grid.ContainsKey(tileApi.id))
            {
                // Mettre à jour la tile existante
                grid[tileApi.id].SetData(tileApi);
            }
            else
            {
                // Si la tile n'existe pas, la créer
                StartCoroutine(CreateTile(tileApi));
            }
        }
    }


    IEnumerator CreateTile(TileApi tileApi)
    {
        // Vérifier une dernière fois que la tile n'existe pas (sécurité contre les appels multiples)
        if (grid.ContainsKey(tileApi.id))
        {
            yield break; // Sortir de la coroutine si la tile existe déjà
        }

        // Instancier la tile
        GameObject tileObject = Instantiate(tilePrefab, new Vector3(tileApi.x * 1.1f, 0, tileApi.y * 1.1f), Quaternion.identity);
        Tile tile = tileObject.GetComponent<Tile>();

        if (tile != null)
        {
            tile.SetData(tileApi);
            // Ajouter la tile au dictionnaire avec son ID comme clé
            grid[tileApi.id] = tile;
        }
        else
        {
            // Si le component Tile n'est pas trouvé, détruire l'objet pour éviter les fuites mémoire
            Destroy(tileObject);
            Debug.LogError($"Le prefab tilePrefab ne contient pas de component Tile pour l'ID {tileApi.id}");
        }

        yield return null;
    }


    #endregion




}
