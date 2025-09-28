using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;




public class ServerClient : MonoBehaviour
{
    private GameManager gameManager;

    // Interval d'update de la carte en secondes
    private float pollInterval = 0.5f;

    // Coroutine continue du polling
    private Coroutine pollingCoroutine;



    void Start()
    {
        gameManager = GetComponent<GameManager>();
        SetPooling();
    }


    #region Pooling régulier

    public void SetPooling(float pollInterval = 0.5f)
    {
        this.pollInterval = pollInterval;
        if (pollingCoroutine != null)
        {
            StopCoroutine(pollingCoroutine);
        }
        pollingCoroutine = StartCoroutine(PollGameState());
    }

    // Coroutine pour le polling
    private IEnumerator PollGameState()
    {
        while (true)
        {
            UpdateMap();
            yield return new WaitForSeconds(this.pollInterval);
        }
    }

    #endregion





    #region Récupération d'infos sur le joueur
    public void GetPlayerInfo()
    {
        StartCoroutine(GetPlayerInfoCoro());
    }

    IEnumerator GetPlayerInfoCoro()
    {
        string url = DataManager.Instance.GetData("serverIP") + "/user/";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("auth_token"));
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            PlayerInfoApi playerInfo = JsonUtility.FromJson<PlayerInfoApi>(json);
            gameManager.UpdatePlayerInfo(playerInfo);
        }
        else
        {
            Debug.LogError("Erreur de requête: " + request.error);
            if (request.responseCode == 401)
            {
                Debug.LogWarning("Token invalide ou expiré. Redirection.");
                SceneManager.LoadScene("Home");
            }
        }
    }
    #endregion



    private void UpdateMap()
    {
        StartCoroutine(UpdateMapCoro());
    }

    IEnumerator UpdateMapCoro()
    {
        string url = DataManager.Instance.GetData("serverIP") + "/tiles/";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("auth_token"));
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            json = "{\"tiles\":" + json + "}";
            Debug.Log(json);
            MapApi mapData = JsonUtility.FromJson<MapApi>(json);
            TileApi[] tiles = mapData.tiles;
            gameManager.UpdateMap(tiles);
        }
        else
        {
            Debug.LogError("Erreur de requête: " + request.error);
            if (request.responseCode == 401)
            {
                Debug.LogWarning("Token invalide ou expiré. Redirection.");
                SceneManager.LoadScene("Home");
            }
        }
    }

}


// Classe pour désérialiser les infos du joueur
[Serializable]
public class PlayerInfoApi
{
    public int id;
    public string uid;
    public string username;
    public string color;
    public int money;
    public string last_activity;
    public int power;
    public string no_calc_power_end;
}


// Classe pour désérialiser les infos de la carte
[Serializable]
public class MapApi
{
    public TileApi[] tiles;
}



// Classe pour désérialiser les infos des tiles
[Serializable]
public class TileApi
{
    public int id;
    public int user_id;
    public int x;
    public int y;
    public int lvl;
    public string build;
    public int drone;
    public DateTime shield;
}