using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class LoginSys : MonoBehaviour
{

    [SerializeField] private InputField LOGINusernameInput;
    [SerializeField] private InputField LOGINpasswordInput;
    [SerializeField] private GameObject LOGINloading;
    [SerializeField] private Button LOGINbutton;


    void Start()
    {
        LOGINbutton.onClick.AddListener(() => StartLogin(LOGINusernameInput.text, LOGINpasswordInput.text));
    }


    // Méthode pour démarrer le processus de connexion
    public void StartLogin(string username, string password)
    {   
        LOGINloading.gameObject.SetActive(true);
        StartCoroutine(Login(username, password));
    }

    // Coroutine pour envoyer la requête POST au serveur Flask
    IEnumerator Login(string username, string password)
    {
        string url = DataManager.Instance.GetData("serverIP") + "/login";
        
        // Création des données JSON à envoyer
        string jsonData = "{\"username\":\"" + username + "\",\"password\":\"" + password + "\"}";
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);
        
        // Configuration de la requête UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        // Envoi de la requête et attente de la réponse
        yield return request.SendWebRequest();
        
        // Gestion des erreurs de connexion
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Erreur de connexion: " + request.error);
        }
        else
        {
            // Traitement de la réponse du serveur
            string responseText = request.downloadHandler.text;
            Debug.Log("Réponse du serveur: " + responseText);
            LOGINloading.gameObject.SetActive(false);

            if (responseText == "NOPE")
            {                
                // Connexion échouée
                Debug.Log("Connexion échouée");
                StartCoroutine(shakeUI());
            }
            else
            {
                responseText = responseText.Replace("{", ""); // Supprimer les accolades
                responseText = responseText.Replace("}", ""); // Supprimer les accolades
                responseText = responseText.Replace("\"", ""); // Supprimer les guillemets
                responseText = responseText.Replace(" ", ""); // Supprimer les espaces
                responseText = responseText.Replace("\n", ""); // Supprimer les retours à la ligne


                // Diviser en paires clé-valeur
                string[] keyValuePairs = responseText.Split(',');

                // Créer un dictionnaire pour stocker les valeurs extraites
                Dictionary<string, string> responseJson = new Dictionary<string, string>();

                // Parcourir les paires clé-valeur
                foreach (string pair in keyValuePairs)
                {
                    // Ajouter la clé et la valeur au dictionnaire
                    responseJson.Add(pair.Split(':')[0], pair.Split(':')[1]);
                }

                // debug all response
                string tmp = "";
                foreach (KeyValuePair<string, string> pair in responseJson)
                {
                    tmp += pair.Key + " : " + pair.Value + "\n";
                }
                Debug.Log(tmp);

                


                // ajouter le username dans les PlayerPrefs
                PlayerPrefs.SetString("username", responseJson["username"]);
                PlayerPrefs.SetString("color", responseJson["color"]);
                PlayerPrefs.SetInt("money", int.Parse(responseJson["money"]));


                // changement de scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
            }
        }
    }
    
    IEnumerator shakeUI()
    {
        LOGINpasswordInput.text = "";
        Vector3 initialPos = LOGINbutton.GetComponent<RectTransform>().localPosition; // Correction de la déclaration
        RectTransform rectTransform = LOGINbutton.GetComponent<RectTransform>(); // Stockage du RectTransform pour éviter de le rappeler plusieurs fois

        for (int i = 0; i < 7; i++)
        {
            rectTransform.localPosition = initialPos + new Vector3(5, 0, 0);
            yield return new WaitForSeconds(0.05f);
            rectTransform.localPosition = initialPos - new Vector3(5, 0, 0); // Ajout d'un déplacement inverse pour l'effet de shake
            yield return new WaitForSeconds(0.05f);
        }

        rectTransform.localPosition = initialPos;
    }

}
