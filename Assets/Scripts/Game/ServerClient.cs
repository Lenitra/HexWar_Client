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
            // updateMap();
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
