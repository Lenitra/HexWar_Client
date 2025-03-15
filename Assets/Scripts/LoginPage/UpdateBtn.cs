using UnityEngine;
using UnityEngine.UI;

public class UpdateBtn : MonoBehaviour
{
    
    // on start, add a listener to the component button

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => UpdateGame());
    }

    // Update the map
    public void UpdateGame()
    {
        // open the link :
        Application.OpenURL(DataManager.Instance.GetData("serverIP") + " /");
    }
}



// using UnityEngine;
// using UnityEngine.Networking;
// using System.Collections;
// using System.IO;
// using System.IO.Compression;
// using System.Diagnostics;

// public class GameUpdater : MonoBehaviour
// {
//     public string updateUrl = "http://VOTRE_IP:PORT/get_last_game_version";
//     private string downloadedZipPath;
//     private string extractionPath;

//     void Start()
//     {
//         downloadedZipPath = Path.Combine(Application.persistentDataPath, "game_update.zip");
//         extractionPath = Path.Combine(Application.persistentDataPath, "game_update");

//         StartCoroutine(DownloadAndUpdate());
//     }

//     IEnumerator DownloadAndUpdate()
//     {
//         // Commencer le téléchargement
//         UnityWebRequest www = UnityWebRequest.Get(updateUrl);
//         www.downloadHandler = new DownloadHandlerFile(downloadedZipPath);

//         var operation = www.SendWebRequest();

//         while (!operation.isDone)
//         {
//             // Optionnel : afficher une barre de progression
//             UnityEngine.Debug.Log($"Téléchargement en cours : {www.downloadProgress * 100}%");
//             yield return null;
//         }

//         if (www.result != UnityWebRequest.Result.Success)
//         {
//             UnityEngine.Debug.LogError($"Erreur de téléchargement : {www.error}");
//             yield break;
//         }

//         UnityEngine.Debug.Log("Téléchargement terminé !");

//         // Décompresser le fichier ZIP
//         ExtractDownloadedZip();

//         // Exécuter le launcher externe
//         LaunchUpdaterScript();

//         // Fermer l’application actuelle pour permettre le remplacement
//         Application.Quit();
//     }

//     void ExtractDownloadedZip()
//     {
//         if (Directory.Exists(extractionPath))
//             Directory.Delete(extractionPath, true);

//         ZipFile.ExtractToDirectory(downloadedZipPath, extractionPath);
//         UnityEngine.Debug.Log("Extraction terminée !");
//     }

//     void LaunchUpdaterScript()
//     {
//         string updaterScriptPath = Path.Combine(extractionPath, "update.bat");

//         if (!File.Exists(updaterScriptPath))
//         {
//             UnityEngine.Debug.LogError("Le script de mise à jour est introuvable !");
//             return;
//         }

//         // Lancez le script de mise à jour
//         ProcessStartInfo startInfo = new ProcessStartInfo()
//         {
//             FileName = updaterScriptPath,
//             WorkingDirectory = extractionPath,
//             CreateNoWindow = true,
//             UseShellExecute = false
//         };
//         Process.Start(startInfo);
//         UnityEngine.Debug.Log("Script de mise à jour lancé.");
//     }
// }
