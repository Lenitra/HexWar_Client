using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LoginSys : MonoBehaviour
{

    [SerializeField] private TMP_InputField LOGINusernameInput;
    [SerializeField] private TMP_InputField LOGINpasswordInput;
    [SerializeField] private Toggle LOGINrememberToggle;
    [SerializeField] private Button LOGINbutton;

    [SerializeField] private Button registerButton;
    [SerializeField] private Button exitButton;


    void Start()
    {
        // Ajouter un listener au bouton de connexion pour démarrer le processus de connexion avec les entrées utilisateur
        LOGINbutton.onClick.AddListener(() => StartLogin(LOGINusernameInput.text, LOGINpasswordInput.text));

        // Charger les informations de connexion sauvegardées (si elles existent)
        LOGINusernameInput.text = PlayerPrefs.GetString("username");
        LOGINpasswordInput.text = PlayerPrefs.GetString("password");

        // Démarrer la vérification de la version du jeu
        StartCoroutine(checkVersion());

        // Ajouter un listener au bouton d'inscription pour ouvrir l'URL d'inscription dans le navigateur par défaut
        registerButton.onClick.AddListener(() => Application.OpenURL(DataManager.Instance.GetData("serverIP") + "/login"));

        // Ajouter un listener au bouton de sortie pour quitter l'application
        exitButton.onClick.AddListener(() => Application.Quit());


        // Focus par défaut sur le champ de nom d'utilisateur
        // if (string.IsNullOrEmpty(LOGINusernameInput.text))
        // {
        LOGINusernameInput.Select();
        LOGINusernameInput.ActivateInputField();
        // }
    }


    void Update()
    {
        // Si le champ est focus et que l'utilisateur appuie sur Tab
        if (LOGINusernameInput.isFocused && Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("L'utilisateur a appuyé sur Tab alors que le champ est focus.");
            LOGINpasswordInput.Select();
            LOGINpasswordInput.ActivateInputField();
        }


        if (LOGINpasswordInput.isFocused && Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("L'utilisateur a appuyé sur Tab alors que le champ2 est focus.");
            LOGINusernameInput.Select();
            LOGINusernameInput.ActivateInputField();
        }

        // Si le champ est focus et que l'utilisateur appuie sur Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("L'utilisateur a appuyé sur Enter alors que le champ est focus.");
            StartLogin(LOGINusernameInput.text, LOGINpasswordInput.text);
        }

        // Si le champ est focus et que l'utilisateur appuie sur Enter du pav num
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Debug.Log("L'utilisateur a appuyé sur Enter du pav num alors que le champ est focus.");
            StartLogin(LOGINusernameInput.text, LOGINpasswordInput.text);
        }
    }



    IEnumerator checkVersion()
    {

#if UNITY_EDITOR
        Debug.Log("Mode éditeur");
        string url = DataManager.Instance.GetData("serverIP") + "/get-version";
        string version = Application.version;


        using (UnityWebRequest request = new UnityWebRequest(url, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Envoyer la requête
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Erreur de connexion: " + request.error);
            }
            else
            {
                string responseText = request.downloadHandler.text; // Récupérer la réponse du serveur
                string msg = "";
                msg += ("Version serveur: " + responseText.Split('"')[3]);
                msg += ("\nVersion locale: " + version);
                Debug.Log(msg);
                if (long.Parse(responseText.Split('"')[3]) > long.Parse(version))
                {
                    Debug.Log("Version du jeu obsolète");
                    // UnityEngine.SceneManagement.SceneManager.LoadScene("VersionError");
                }
            }
        }
#else

        string url = DataManager.Instance.GetData("serverIP") + "/get-version";
        string version = Application.version;


        using (UnityWebRequest request = new UnityWebRequest(url, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Envoyer la requête
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Erreur de connexion: " + request.error);
            }
            else
            {
                string responseText = request.downloadHandler.text; // Récupérer la réponse du serveur
                string msg = "";
                msg += ("Version serveur: " + responseText.Split('"')[3]);
                msg += ("\nVersion locale: " + version);
                Debug.Log(msg);
                if (long.Parse(responseText.Split('"')[3]) > long.Parse(version))
                {
                    Debug.Log("Version du jeu obsolète");
                    UnityEngine.SceneManagement.SceneManager.LoadScene("VersionError");
                }
            }
        }

#endif

    }




    // Méthode pour démarrer le processus de connexion
    public void StartLogin(string username, string password)
    {
        // LOGINloading.gameObject.SetActive(true);
        StartCoroutine(Login(username, password));
    }


    // Coroutine pour envoyer la requête POST au serveur Flask
    IEnumerator Login(string username, string password)
    {
        string url = DataManager.Instance.GetData("serverIP") + "/game-login";

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
            // LOGINloading.gameObject.SetActive(false);

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


                if (LOGINrememberToggle.isOn)
                {
                    PlayerPrefs.SetString("username", LOGINusernameInput.text);
                    PlayerPrefs.SetString("password", LOGINpasswordInput.text);
                }


                // ajouter le données du serveur dans les PlayerPrefs
                PlayerPrefs.SetString("username", responseJson["username"]);
                PlayerPrefs.SetString("color", responseJson["color"]);
                PlayerPrefs.SetInt("money", int.Parse(responseJson["money"]));


                // changement de scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
            }
        }
    }

    // Coroutine pour l'effet de shake sur le bouton de connexion en cas d'échec de connexion
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
