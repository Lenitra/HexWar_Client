using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Threading.Tasks;


public class ServerClient : MonoBehaviour
{
    private GameManager gameManager;
    private float pollInterval = 0.5f; // Interval d'update de la carte en secondes



    void Start()
    {
        gameManager = GetComponent<GameManager>();
        updateMap();
    }



    public void updateMap()
    {
        // lancer la coroutine 
        StartCoroutine(GetGameState());
    }

    IEnumerator GetGameState()
    {
        float startTime = Time.time;
        UnityWebRequest request = UnityWebRequest.Get(DataManager.Instance.GetData("serverIP")+"/get_hex/"+ PlayerPrefs.GetString("username"));
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            // Debug.LogError("Error: " + request.error);
        }
        else
        {
            // if request.downloadHandler.text start with "error" then we have an error
            if (request.downloadHandler.text.ToLower().StartsWith("error : "))
            {
                // change the scene to the login scene
                SceneManager.LoadScene("Home");
                // Debug.LogError("error: " + request.downloadHandler.text);
            }
            else
            {
                string hexes = request.downloadHandler.text;
                // delete characters jusqu'à la première virgule
                hexes = hexes.Substring(hexes.IndexOf(",") + 1);
                // supprimer le dernier caractère
                hexes = hexes.Remove(hexes.Length - 1, 1);
                hexes = hexes.Remove(hexes.Length - 1, 1);
                Debug.Log("hexes: " + hexes);



                string money = request.downloadHandler.text;
                // garder les caractères jusqu'à la première virgule
                money = money.Substring(0, money.IndexOf(","));
                // garder uniquement les chiffres
                money = new string(money.Where(char.IsDigit).ToArray());


                gameManager.UpdateMoney(money);
                gameManager.SetupTiles(hexes);
            }
        }
        // Debug.Log("PollGameState took: " + (Time.time - startTime) + " seconds");

    }

    
    
    public void moveUnits(string from, string to, int units)
    {
        StartCoroutine(MoveUnitsCoro(from, to, units));
    }
    
    IEnumerator MoveUnitsCoro(string from, string to, int units)
    {
        UnityWebRequest request = UnityWebRequest.Get(DataManager.Instance.GetData("serverIP")+"/move_units/" + from + "/" + to + "/" + units);
        yield return request.SendWebRequest();
        // debug the response
        if (request.result != UnityWebRequest.Result.Success)
        {
            // Debug.LogError("Error: " + request.error);
        }
        else
        {
            if (request.downloadHandler.text.ToLower().StartsWith("error : "))
            {
                // change the scene to the login scene
                SceneManager.LoadScene("Home");
                // Debug.LogError("error: " + request.downloadHandler.text);
            }

            else
            {
                if (request.downloadHandler.text != "NOPE")
                {
                    // we recieve a list of moves
                    string tmp = request.downloadHandler.text;
                    // delete all [ and ]
                    tmp = tmp.Replace("[", "");
                    tmp = tmp.Replace("]", "");
                    tmp = tmp.Replace("\"", "");

                    string[] moves = tmp.Split(',');

                    tmp = "";
                    // debug inside the array
                    foreach (string move in moves)
                    {
                        tmp += move + " ";
                    }

                    StartCoroutine(gameManager.moveUnitsAnimation(moves));
                    updateMap();
                }
            }
        }
    }
    


    public void build(string tile, string type, int lvl)
    {
        StartCoroutine(BuildCoro(tile, type, lvl));
    }

    IEnumerator BuildCoro(string tile, string type)
    {
        UnityWebRequest request = UnityWebRequest.Get(DataManager.Instance.GetData("serverIP") + "/buildbat/" + tile + "/" + type + "/" + lvl);
        yield return request.SendWebRequest();
        // debug the response
        if (request.result != UnityWebRequest.Result.Success)
        {
            updateMap();
        }
        else
        {
            if (request.downloadHandler.text.ToLower().StartsWith("error : "))
            {
                // change the scene to the login scene
                SceneManager.LoadScene("Home");
                // Debug.LogError("error: " + request.downloadHandler.text);
            }
        }
    }


    public void GetPrice(string build, int lvl, Action<int> onComplete)
    {
        StartCoroutine(GetPriceCoroutine(build, lvl, onComplete));
    }

    private IEnumerator GetPriceCoroutine(string build, int lvl, Action<int> onComplete)
    {
        UnityWebRequest request = UnityWebRequest.Get(DataManager.Instance.GetData("serverIP") + "/get_price/" + build + "/" + lvl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {   
            int price = int.Parse(request.downloadHandler.text);
            onComplete?.Invoke(price);
        }
        else
        {
            // Gérer les erreurs ici
            onComplete?.Invoke(-1);
        }
    }


    
    void OnApplicationQuit()
    {
        StopAllCoroutines();
    }


}
