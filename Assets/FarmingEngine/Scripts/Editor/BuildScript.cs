using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildScript
{
    public static void Build()
    {
        BuildPlayerOptions opt = new BuildPlayerOptions();
        opt.scenes = new[] {
            "Assets/FarmingEngine/Scenes/Farm.unity",
            "Assets/FarmingEngine/Scenes/House.unity",
            "Assets/FarmingEngine/Scenes/Test.unity",
            "Assets/FarmingEngine/Scenes/Mine.unity"
        };
        opt.locationPathName = "Builds/FarmGame-final-test/FarmGame.exe";
        opt.target = BuildTarget.StandaloneWindows64;
        opt.options = BuildOptions.None;
        BuildReport report = BuildPipeline.BuildPlayer(opt);
        if (report.summary.result != BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
        else
        {
            UnityEngine.Debug.Log("BUILD SUCCESS");
            EditorApplication.Exit(0);
        }
    }
}
