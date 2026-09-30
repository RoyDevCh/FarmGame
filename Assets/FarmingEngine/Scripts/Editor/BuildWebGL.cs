using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// WebGL 打包入口（供命令行 -executeMethod 调用）。
/// 用法：
///   Unity.exe -batchmode -nographics -quit -projectPath &lt;工程目录&gt; \
///     -executeMethod BuildWebGL.Build -logFile &lt;日志&gt;
/// 输出目录：&lt;工程目录&gt;/Builds/WebGL
/// </summary>
public class BuildWebGL
{
    private static readonly string[] Scenes = new[]
    {
        "Assets/FarmingEngine/Scenes/Farm.unity",
        "Assets/FarmingEngine/Scenes/House.unity",
        "Assets/FarmingEngine/Scenes/Test.unity",
        "Assets/FarmingEngine/Scenes/Mine.unity"
    };

    public static void Build()
    {
        string outDir = System.Environment.GetEnvironmentVariable("FARMGAME_WEBGL_OUT");
        if (string.IsNullOrEmpty(outDir))
            outDir = "Builds/WebGL";

        BuildPlayerOptions opt = new BuildPlayerOptions();
        opt.scenes = Scenes;
        opt.locationPathName = outDir;
        opt.target = BuildTarget.WebGL;
        opt.options = BuildOptions.None;

        Debug.Log("[BuildWebGL] output = " + outDir);
        Debug.Log("[BuildWebGL] compression = " + PlayerSettings.WebGL.compressionFormat
                  + "  decompressionFallback = " + PlayerSettings.WebGL.decompressionFallback
                  + "  memorySize = " + PlayerSettings.WebGL.memorySize);

        BuildReport report = BuildPipeline.BuildPlayer(opt);
        BuildSummary s = report.summary;

        if (s.result != BuildResult.Succeeded)
        {
            Debug.LogError("[BuildWebGL] FAILED result=" + s.result + " errors=" + s.totalErrors);
            EditorApplication.Exit(1);
        }

        Debug.Log("[BuildWebGL] SUCCESS totalSize=" + s.totalSize
                  + " bytes (" + (s.totalSize / 1048576.0).ToString("F1") + " MB)"
                  + "  time=" + s.totalTime.TotalSeconds.ToString("F1") + "s"
                  + "  warnings=" + s.totalWarnings);
        EditorApplication.Exit(0);
    }
}
