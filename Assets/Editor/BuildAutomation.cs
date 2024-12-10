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

        // Options de build communes
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            options = BuildOptions.None // Modifier en fonction des besoins, par exemple, BuildOptions.Development
        };
        // filename = "yy.MM.dd"
        string filename = System.DateTime.Now.ToString("yy.MM.dd");

        // Construire pour Linux
        options.target = BuildTarget.StandaloneLinux64;
        options.locationPathName = Path.Combine(linuxPath, "HexWar.x86_64");
        BuildPipeline.BuildPlayer(options);

        // Construire pour Windows
        options.target = BuildTarget.StandaloneWindows64;
        options.locationPathName = Path.Combine(windowsPath, "HexWar.exe");
        BuildPipeline.BuildPlayer(options);

        // Construire pour Android
        // options.target = BuildTarget.Android;
        // options.locationPathName = Path.Combine(androidPath, "HexWar.apk");
        // BuildPipeline.BuildPlayer(options);

        // Afficher un message une fois terminé
        EditorUtility.DisplayDialog("Build Completed", "All builds are completed!", "OK");
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
