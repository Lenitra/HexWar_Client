using UnityEditor;
using System.IO;
using System.Linq;
using System.IO.Compression;

public class BuildAutomation
{
    [MenuItem("Build/Build All Platforms")]
    public static void BuildAllPlatforms()
    {


        // Définir les chemins de sortie pour chaque plateforme
        string basePath = "Builds";
        string windowsPath = Path.Combine(basePath, "Windows");
        string androidPath = Path.Combine(basePath, "Android");

        // supprimer le contenu du dossier de build
        if (Directory.Exists(basePath))
        {
            Directory.Delete(basePath, true);
        }

        // Créer le dossier de build
        Directory.CreateDirectory(basePath);

        // Générer une version basée sur la date
        string version = System.DateTime.Now.ToString("yyMMddHHmm");
        PlayerSettings.bundleVersion = version;

        // Créer un fichier version dans le dossier de build
        string versionFilePath = Path.Combine(basePath, "version.txt");
        File.WriteAllText(versionFilePath, version);

        // Options de build communes
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            options = BuildOptions.None // Modifier en fonction des besoins, par exemple, BuildOptions.Development
        };




        // Construire pour Windows
        options.target = BuildTarget.StandaloneWindows64;
        options.locationPathName = Path.Combine(windowsPath, $"NyxsImperium.exe");
        Directory.CreateDirectory(windowsPath); // Assurer que le dossier existe
        BuildPipeline.BuildPlayer(options);
        // Ajouter le fichier /update.bat sans le dossier windows
        File.Copy("Assets/Editor/update.bat", Path.Combine(windowsPath, "update.bat"));

        // Construire pour Android en .apk
        options.target = BuildTarget.Android;
        options.locationPathName = Path.Combine(androidPath, $"NyxsImperium.apk");
        Directory.CreateDirectory(androidPath); // Assurer que le dossier existe
        BuildPipeline.BuildPlayer(options);





        // Chemins de sortie pour compression des builds
        string windowsZipPath = Path.Combine(basePath, $"NyxsImperium_{version}_windows.zip");

        // Compresser le dossier Linux et Windows en les plaçant dans le dossier 'Builds' (basePath)
        ZipFile.CreateFromDirectory(windowsPath, windowsZipPath);


        // Afficher un message une fois terminé
        EditorUtility.DisplayDialog("Build Completed", $"All builds are completed! Version: {version}", "OK");
    }

    // Méthode pour récupérer toutes les scènes activées dans le build
    private static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }
}
