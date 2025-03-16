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
#elif UNITY_STANDALONE_OSX
        platform = "macos";
#endif

        // Ajout du listener au bouton
        GetComponent<Button>().onClick.AddListener(() => UpdateGame());
    }

    public void UpdateGame()
    {
        // Vérifier la plateforme : Windows, MacOS, Linux
        if (platform == "")
        {
            // debugTest.text("Plateforme non supportée !");
            return;
        }

        StartCoroutine(DownloadAndUpdate());
    }

    IEnumerator DownloadAndUpdate()
    {
        string link = "";

        if (platform == "linux")
        {
            link = DataManager.Instance.GetData("serverIP") + "/get_last_game_version_lin";
        }
        else if (platform == "macos")
        {
            link = DataManager.Instance.GetData("serverIP") + "/get_last_game_version_mac";
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

        // Décompresser le fichier ZIP
        ExtractDownloadedZip();

        // Exécuter le launcher externe
        LaunchUpdaterScript();

        // Fermer l’application actuelle pour permettre le remplacement
        Application.Quit();
    }

    void ExtractDownloadedZip()
    {
        if (Directory.Exists(extractionPath))
            Directory.Delete(extractionPath, true);

        ZipFile.ExtractToDirectory(downloadedZipPath, extractionPath);
        debugText.text += "Fichier ZIP extrait !";
    }

    void LaunchUpdaterScript()
    {
        string updaterScriptPath = Path.Combine(extractionPath, "update.bat");

        if (!File.Exists(updaterScriptPath))
        {
            debugText.text += "Script de mise à jour introuvable !";
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = updaterScriptPath,
            WorkingDirectory = extractionPath,
            CreateNoWindow = true,
            UseShellExecute = false
        };
        Process.Start(startInfo);
        debugText.text += "Mise à jour en cours...";
    }
}
