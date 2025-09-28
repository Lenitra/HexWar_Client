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
    private CamController camController; // Le service de communication avec le serveur

    private Dictionary<int, Tile> grid = new(); // Dictionnaire des cases de la carte (ID -> Tile)
    public Dictionary<int, User> users = new(); // Dictionnaire des utilisateurs (ID -> User)
    [SerializeField] private GameObject tilePrefab; // Préfabriqué de la tile

    private bool mapLoaded = false;





    void Start()
    {
        serverClient = GetComponent<ServerClient>();
        gameView = GetComponent<GameView>();
        camController = Camera.main.GetComponent<CamController>();

        // TODO: Récupérer des données du serveur
        // serverClient.GetBuildPrices();
        // serverClient.GetWiki();
        serverClient.GetPlayerInfo();
        serverClient.UpdateMap();
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
    private void MapLoadedSuccessfully()
    {
        foreach (Tile tile in grid.Values)
        {
            if (tile.GetBuild() == "node")
            {
                camController.MoveCamToTile(tile.GetX(), tile.GetY(), true);
                break;
            }
        }
    }

    public void UpdateMap(TileApi[] tiles)
    {
        foreach (TileApi tileApi in tiles)
        {
            // Vérifier si la tile existe déjà dans le dictionnaire
            if (grid.ContainsKey(tileApi.id))
            {
                // Mettre à jour la tile existante
                grid[tileApi.id].SetData(tileApi);


                // Si l'utilisateur qui possède la case n'est pas dans le dictionnaire, le récupérer
                if (!users.ContainsKey(tileApi.user_id) && tileApi.user_id != 0 && tileApi.user_id != int.MinValue)
                {
                    serverClient.GetUser(tileApi.user_id);
                }

                // Mettre à jour les données de l'utilisateur sur la tile
                users.TryGetValue(tileApi.user_id, out User user);
                grid[tileApi.id].SetUserData(user);



            }
            else
            {
                // Si la tile n'existe pas, la créer
                StartCoroutine(CreateTile(tileApi));
                if (!users.ContainsKey(tileApi.user_id) && tileApi.user_id != 0 && tileApi.user_id != int.MinValue)
                {
                    serverClient.GetUser(tileApi.user_id);
                }
            }
        }

        if (tiles.Length > 0 && !mapLoaded)
        {
            mapLoaded = true;
            MapLoadedSuccessfully();
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



    #region Gestion du dictionaire des utilisateurs
    public void UpdateUsers(User[] usersArray)
    {
        // Mettre à jour le dictionnaire des utilisateurs
        foreach (User user in usersArray)
        {
            if (users.ContainsKey(user.id))
            {
                users[user.id] = user; // Mettre à jour l'utilisateur existant
            }
            else
            {
                users.Add(user.id, user); // Ajouter un nouvel utilisateur
            }
        }
    }

    #endregion


}
