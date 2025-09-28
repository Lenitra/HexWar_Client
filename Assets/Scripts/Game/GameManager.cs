using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    private ServerClient serverClient; // Le service de communication avec le serveur
    private GameView gameView; // Le service de communication avec le serveur
    private List<Tile> grid = new List<Tile>(); // Tableau des hexagones
    [SerializeField] private GameObject tilePrefab; // Préfabriqué de la tile

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingSlider;
    private bool loadedMap = false; // Indique si la carte est chargée




    void Start()
    {
        serverClient = GetComponent<ServerClient>();
        gameView = GetComponent<GameView>();

        // Récupérer des données du serveur
        // serverClient.GetBuildPrices();
        // serverClient.GetWiki();
        serverClient.GetPlayerInfo();

        // On demande au serveur de nous envoyer la carte
        // serverClient.updateMap();
    }



    #region Gestion des infos du joueur
    public void UpdatePlayerInfo(PlayerInfoApi playerInfo)
    {
        gameView.SetPlayerName(playerInfo.username);
        gameView.SetMoney(playerInfo.money);
    }



    #endregion




}
