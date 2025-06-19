using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UpdateBtn : MonoBehaviour
{
    private string downloadedZipPath;
    private string extractionPath;
    private string platform = "";

    [SerializeField] private Text debugText;


    void Start()
    {
        // Initialisation des chemins dans Start() pour éviter l'erreur
        downloadedZipPath = Path.Combine(Application.persistentDataPath, "game_update.zip");
        extractionPath = Path.Combine(Application.persistentDataPath, "game_update");

        // Configuration de la plateforme
#if UNITY_STANDALONE_WIN
        platform = "windows";
#elif UNITY_STANDALONE_LINUX
        platform = "linux"; 
#endif

        // Ajout du listener au bouton
        GetComponent<Button>().onClick.AddListener(() => UpdateGame());
    }

    public void UpdateGame()
    {
        if (platform == "")
        {
            debugText.text = "Plateforme non supportée pour la mise à jour automatique.";
            return;
        }

        // Afficher un message de démarrage
        debugText.text = "Démarrage de la mise à jour...";
        // Lancer la coroutine de téléchargement et mise à jour
        StartCoroutine(DownloadAndUpdate());
        // Désactiver le bouton pour éviter les clics multiples
        GetComponent<Button>().interactable = false;
        
    }

    IEnumerator DownloadAndUpdate()
    {
        // cleanup previous update
        if (Directory.Exists(extractionPath))
        {
            Directory.Delete(extractionPath, true);
        }
        if (File.Exists(downloadedZipPath))
        {
            File.Delete(downloadedZipPath);
        }

        string link = "";

        if (platform == "linux")
        {
            link = DataManager.Instance.GetData("serverIP") + "/get_last_game_version_lin";
        }
        else if (platform == "windows")
        {
            link = DataManager.Instance.GetData("serverIP") + "/get_last_game_version_win";
        }

        // Commencer le téléchargement
        UnityWebRequest www = UnityWebRequest.Get(link);
        www.downloadHandler = new DownloadHandlerFile(downloadedZipPath);

        var operation = www.SendWebRequest();

        while (!operation.isDone)
        {
            debugText.text = $"Téléchargement en cours : {www.downloadProgress * 100}%";
            yield return null;
        }

        if (www.result != UnityWebRequest.Result.Success)
        {
            debugText.text += ($"Erreur de téléchargement : {www.error}");
            yield break;
        }

        // Debug.Log("Téléchargement terminé !");

        // Extraire le contenu du zip
        ZipFile.ExtractToDirectory(downloadedZipPath, extractionPath);

        // Exécuter le launcher externe
        LaunchUpdaterScript();

        // Fermer l’application actuelle pour permettre le remplacement
        Application.Quit();
    }


    void LaunchUpdaterScript()
    {
        // Détermination du chemin du script de mise à jour en fonction de la plateforme
        string updaterScriptPath = "";
        if (platform == "linux")
        {
            updaterScriptPath = Path.Combine(extractionPath, "update.sh");
        }
        else if (platform == "windows")
        {
            updaterScriptPath = Path.Combine(extractionPath, "update.bat");
        }
        else
        {
            debugText.text += "Plateforme non supportée pour la mise à jour automatique.";
            return;
        }

        // Vérifier que le script de mise à jour existe
        if (!File.Exists(updaterScriptPath))
        {
            debugText.text += "Script de mise à jour introuvable !";
            return;
        }

        // Détermination du dossier d'installation (ici, le dossier parent du dossier Data)
        string installPath = Directory.GetParent(Application.dataPath).FullName;

        // Préparation des arguments :
        // - Le premier argument est le dossier d'installation
        // - Le deuxième argument est le dossier d'extraction (contenant le nouveau build)
        string arguments = $"\"{installPath}";

        // Configuration du ProcessStartInfo pour lancer le script externe
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = updaterScriptPath,
            Arguments = arguments,
            WorkingDirectory = extractionPath,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        // Lancement du script de mise à jour
        Process.Start(startInfo);
        debugText.text += "Mise à jour en cours...";
    }


}
