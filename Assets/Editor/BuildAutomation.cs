using UnityEditor;
using System.IO;
using System.Linq;

public class BuildAutomation
{
    [MenuItem("Build/Build All Platforms")]
    public static void BuildAllPlatforms()
    {
        // Définir les chemins de sortie pour chaque plateforme
        string basePath = "Builds";
        string linuxPath = Path.Combine(basePath, "Linux");
        string windowsPath = Path.Combine(basePath, "Windows");
        string androidPath = Path.Combine(basePath, "Android");

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

        // Construire pour Linux
        options.target = BuildTarget.StandaloneLinux64;
        options.locationPathName = Path.Combine(linuxPath, $"HexWar_v{version}.x86_64");
        Directory.CreateDirectory(linuxPath); // Assurer que le dossier existe
        BuildPipeline.BuildPlayer(options);

        // Construire pour Windows
        options.target = BuildTarget.StandaloneWindows64;
        options.locationPathName = Path.Combine(windowsPath, $"HexWar_v{version}.exe");
        Directory.CreateDirectory(windowsPath); // Assurer que le dossier existe
        BuildPipeline.BuildPlayer(options);

        // Construire pour Android
        // options.target = BuildTarget.Android;
        // options.locationPathName = Path.Combine(androidPath, $"HexWar_v{version}.apk");
        // Directory.CreateDirectory(androidPath); // Assurer que le dossier existe
        // BuildPipeline.BuildPlayer(options);


        

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
