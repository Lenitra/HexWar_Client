using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;
using System.Linq;




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
            updateMap();
            yield return new WaitForSeconds(pollInterval);
        }
    }



    // Lance la récupération des données de la carte
    public void updateMap()
    {
        StartCoroutine(GetGameState());
    }

    IEnumerator GetGameState()
    {
        float startTime = Time.time;

        string url = DataManager.Instance.GetData("serverIP") + "/api/get_hex";
        UnityWebRequest request = UnityWebRequest.Get(url);

        request.SetRequestHeader("X-Auth-Token", "Bearer " + PlayerPrefs.GetString("auth_token"));
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text.ToLower();

            if (response.StartsWith("error : veuillez vous (re)connecter"))
            {
                SceneManager.LoadScene("Home");
            }
            else if (response.StartsWith("error : "))
            {
                gameManager.nopePanel(response.Substring(8));
            }
            else
            {
                string raw = request.downloadHandler.text;

                string hexes = raw.Substring(raw.IndexOf(",") + 1);
                hexes = hexes.Remove(hexes.Length - 2, 2); // supprimer deux derniers caractères

                string money = new string(raw.Substring(0, raw.IndexOf(","))
                                        .Where(char.IsDigit).ToArray());

                gameManager.UpdateMoney(money);
                gameManager.SetupTiles(hexes);
            }
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




    public void MoveUnits(string[] from, string[] to, int units)
    {
        StartCoroutine(MoveUnitsCoro(from, to, units));
    }

    IEnumerator MoveUnitsCoro(string[] from, string[] to, int units)
    {
        UnityWebRequest request = UnityWebRequest.Get(DataManager.Instance.GetData("serverIP") + "/api/move_units");
        // origin = request.form.get("origin") # format "x:y"
        // destination = request.form.get("destination") # format "x:y"
        // units = int(request.form.get("units"))
        // faire une méthode post
        request.method = "POST";
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{\"originX\":\"" + from[0] + "\",\"originY\":\"" + from[1] + "\",\"destinationX\":\"" + to[0] + "\",\"destinationY\":\"" + to[1] + "\",\"units\":" + units + "}"));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("X-Auth-Token", "Bearer " + PlayerPrefs.GetString("auth_token"));
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.downloadHandler.text.ToLower().StartsWith("error : Veillez vous (re)connecter"))
            {
                SceneManager.LoadScene("Home");
            }
            else if (request.downloadHandler.text.ToLower().StartsWith("error : "))
            {
                gameManager.nopePanel(request.downloadHandler.text.Substring(8));
            }
            else
            {
                gameManager.MoveUnitsServerResponse(from, to);
                updateMap();
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }





    public void Build(string[] tileCoords, string type)
    {
        StartCoroutine(BuildCoro(tileCoords, type));
    }

    IEnumerator BuildCoro(string[] tileCoords, string type)
    {
        UnityWebRequest request = UnityWebRequest.Get(DataManager.Instance.GetData("serverIP") + "/api/buildbat");
        request.method = "POST";
        request.SetRequestHeader("X-Auth-Token", "Bearer " + PlayerPrefs.GetString("auth_token"));
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{\"x\":" + tileCoords[0] + ",\"y\":" + tileCoords[1] + ",\"type\":\"" + type + "\"}"));
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        // debug the response
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            if (request.downloadHandler.text.ToLower().StartsWith("error : Veillez vous (re)connecter"))
            {
                SceneManager.LoadScene("Home");
            }
            else if (request.downloadHandler.text.ToLower().StartsWith("error : "))
            {
                gameManager.nopePanel(request.downloadHandler.text.Substring(8));
            }
            else
            {
                updateMap();
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }



    public void Destroy(string[] tileCoords)
    {
        StartCoroutine(DestroyCoro(tileCoords));
    }

    IEnumerator DestroyCoro(string[] tileCoords)
    {
        UnityWebRequest request = UnityWebRequest.Get(DataManager.Instance.GetData("serverIP") + "/api/destroy");
        request.method = "POST";
        request.SetRequestHeader("X-Auth-Token", "Bearer " + PlayerPrefs.GetString("auth_token"));
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{\"x\":" + tileCoords[0] + ",\"y\":" + tileCoords[1] + "}"));
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        // debug the response
        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.downloadHandler.text.ToLower().StartsWith("error : Veillez vous (re)connecter"))
            {
                SceneManager.LoadScene("Home");
            }
            else if (request.downloadHandler.text.ToLower().StartsWith("error : "))
            {
                gameManager.nopePanel(request.downloadHandler.text.Substring(8));
            }
            else
            {
                updateMap();
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }


    public void RallyUnits(string[] tileCoords)
    {
        StartCoroutine(RallyUnitsCoro(tileCoords));
    }

    IEnumerator RallyUnitsCoro(string[] tileCoords)
    {
        UnityWebRequest request = UnityWebRequest.Get(DataManager.Instance.GetData("serverIP") + "/api/rally");
        request.method = "POST";
        request.SetRequestHeader("X-Auth-Token", "Bearer " + PlayerPrefs.GetString("auth_token"));
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{\"x\":" + tileCoords[0] + ",\"y\":" + tileCoords[1] + "}"));
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        // debug the response
        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.downloadHandler.text.ToLower().StartsWith("error : Veillez vous (re)connecter"))
            {
                SceneManager.LoadScene("Home");
            }
            else if (request.downloadHandler.text.ToLower().StartsWith("error : "))
            {
                gameManager.nopePanel(request.downloadHandler.text.Substring(8));
            }
            else
            {
                gameManager.RallyUnitsServerResponse(tileCoords);
                updateMap();
            }
        }
        else
        {
            Debug.LogError("error: " + request.error);
        }
    }


    public void DispatchUnits()
    {
        StartCoroutine(DispatchUnitsCoro());
    }

    IEnumerator DispatchUnitsCoro()
    {
        UnityWebRequest request = UnityWebRequest.Get(DataManager.Instance.GetData("serverIP") + "/api/dispatch");
        request.method = "POST";
        request.SetRequestHeader("X-Auth-Token", "Bearer " + PlayerPrefs.GetString("auth_token"));
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        // debug the response
        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.downloadHandler.text.ToLower().StartsWith("error : Veillez vous (re)connecter"))
            {
                SceneManager.LoadScene("Home");
            }
            else if (request.downloadHandler.text.ToLower().StartsWith("error : "))
            {
                gameManager.nopePanel(request.downloadHandler.text.Substring(8));
            }
            else
            {
                gameManager.DispatchUnitsServerResponse();
                updateMap();
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }


    public void GetBuildPrices()
    {
        StartCoroutine(GetBuildPricesCoroutine());
    }

    private IEnumerator GetBuildPricesCoroutine()
    {

        UnityWebRequest request = UnityWebRequest.Get(DataManager.Instance.GetData("serverIP") + "/api/game_handshake_post_login/bat_stats");
        request.method = "POST";
        request.SetRequestHeader("X-Auth-Token", "Bearer " + PlayerPrefs.GetString("auth_token"));
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.downloadHandler.text.ToLower().StartsWith("error : Veillez vous (re)connecter"))
            {
                SceneManager.LoadScene("Home");
            }
            else if (request.downloadHandler.text.ToLower().StartsWith("error : "))
            {
                gameManager.nopePanel(request.downloadHandler.text.Substring(8));
            }
            else
            {
                gameManager.handshakeResponse_BatStats(request.downloadHandler.text);
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }



    public void GetWikiData()
    {
        StartCoroutine(GetWikiDataCoroutine());
    }

    private IEnumerator GetWikiDataCoroutine()
    {
        UnityWebRequest request = UnityWebRequest.Get(DataManager.Instance.GetData("serverIP") + "/api/game_handshake_post_login/wiki");
        request.method = "POST";
        request.SetRequestHeader("X-Auth-Token", "Bearer " + PlayerPrefs.GetString("auth_token"));
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.downloadHandler.text.ToLower().StartsWith("error : Veillez vous (re)connecter"))
            {
                SceneManager.LoadScene("Home");
            }
            else if (request.downloadHandler.text.ToLower().StartsWith("error : "))
            {
                gameManager.nopePanel(request.downloadHandler.text.Substring(8));
            }
            else
            {
                gameManager.handshakeResponse_Wiki(request.downloadHandler.text);
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }


    void OnApplicationQuit()
    {
        StopAllCoroutines();
    }






}
