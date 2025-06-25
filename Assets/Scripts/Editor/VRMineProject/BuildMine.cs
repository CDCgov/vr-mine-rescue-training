using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using System.IO;
using System.Linq;
using Process = System.Diagnostics.Process;
using UnityEditor.Build.Reporting;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;

using static UnityEditor.AddressableAssets.Build.ContentUpdateScript;

public class BuildMine
{

    public static string BuildPath = @"C:\VRMine-Build";
    public static string BenchmarkPath = @"C:\VRMine-Benchmark\VRMineBenchmark.exe";
    public static string ExeName = @"VRMine";
    public static string NetworkPath = @"\\cdc\project\NIOSH_OMSHR_SelfEscape\VRMine\VRMine-Build";

    public static string RelayBuildPath = @"C:\VRMine-Relay\VRMineRelay.exe";

    private static string BuildExe
    {
        get
        {
            return Path.Combine(BuildPath, ExeName + ".exe");
        }
    }

    private static string BuildDataDir
    {
        get
        {
            return Path.Combine(BuildPath, ExeName + "_Data");
        }
    }

    private static string BuildMfireDir
    {
        get
        {
            return Path.Combine(BuildPath, "MFireServer");
        }
    }

    [PostProcessBuildAttribute(100)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {

        string buildFolder = Path.GetDirectoryName(pathToBuiltProject);
        string exeName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
        string dataDir = $"{buildFolder}\\{exeName}_Data";

        Debug.Log($"Data path: {Application.dataPath} Built To: {buildFolder} DataDir: {dataDir}");


    }

    private static void CopyFiles(string source, string destination)
    {
        string sourceDirectory = Path.GetDirectoryName(source);
        string sourceFiles = Path.GetFileName(source);

        string[] files = Directory.GetFiles(sourceDirectory, sourceFiles, SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            File.Copy(file, Path.Combine(destination, fileName), true);
        }
    }

    static BuildPlayerOptions GetDefaultOptions()
    {
        BuildPlayerOptions opts = new BuildPlayerOptions();

        opts.scenes = (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
        opts.locationPathName = BuildExe;
        opts.target = BuildTarget.StandaloneWindows64;
        opts.options = BuildOptions.None;

        return opts;
    }

    [MenuItem("Build/Build Local", false, 0)]
    static void BuildMineLocalDev()
    {
        GitVersion.UpdateVersion();
        GitVersion.SaveVersion();

        ContentUpdateScript.GroupFilterFunc = null;
        bool success = AddressableBuild.BuildAddressables();
        if (!success)
        {
            Debug.LogError("Addressable build failed.");
        }

        //string[] scenes = (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
        Directory.CreateDirectory(BuildPath);
        //Directory.CreateDirectory(BuildMfireDir);
        //BuildPipeline.BuildPlayer(scenes, BuildExe, BuildTarget.StandaloneWindows64, BuildOptions.None);

        BuildPlayerOptions opts = GetDefaultOptions();
        BuildReport report = BuildPipeline.BuildPlayer(opts);

        //Copy files that do not get automatically included into the build here
        CopyFiles(Path.Combine(Application.dataPath, "*.xml"), BuildDataDir);
        CopyFiles(Path.Combine(Application.dataPath, "*.txt"), BuildDataDir);
        CopyFiles(Path.Combine(Application.dataPath, "*.chm"), BuildDataDir);
        //CopyFiles(Path.Combine(Application.dataPath, "../MFireServer/*.*"), BuildMfireDir);

        Debug.Log("Done");
    }

    [MenuItem("Build/Build Local Without Addressables", false, 0)]
    static void BuildMineLocalNoAddr()
    {
        GitVersion.UpdateVersion();
        GitVersion.SaveVersion();

        //bool success = AddressableBuild.BuildAddressables();
        //if (!success) 
        //{
        //    Debug.LogError("Addressable build failed.");
        //}

        //string[] scenes = (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
        Directory.CreateDirectory(BuildPath);
        //Directory.CreateDirectory(BuildMfireDir);
        //BuildPipeline.BuildPlayer(scenes, BuildExe, BuildTarget.StandaloneWindows64, BuildOptions.None);

        BuildPlayerOptions opts = GetDefaultOptions();
        BuildReport report = BuildPipeline.BuildPlayer(opts);

        //Copy files that do not get automatically included into the build here
        CopyFiles(Path.Combine(Application.dataPath, "*.xml"), BuildDataDir);
        CopyFiles(Path.Combine(Application.dataPath, "*.txt"), BuildDataDir);
        CopyFiles(Path.Combine(Application.dataPath, "*.chm"), BuildDataDir);
        //CopyFiles(Path.Combine(Application.dataPath, "../MFireServer/*.*"), BuildMfireDir);

        Debug.Log("Done");
    }

    [MenuItem("Build/Build Local DevVersion", false, 0)]
    static void BuildMineLocal()
    {
        GitVersion.UpdateVersion();
        GitVersion.SaveVersion();

        ContentUpdateScript.GroupFilterFunc = null;
        bool success = AddressableBuild.BuildAddressables();
        if (!success)
        {
            Debug.LogError("Addressable build failed.");
        }

        //string[] scenes = (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
        Directory.CreateDirectory(BuildPath);
        //Directory.CreateDirectory(BuildMfireDir);
        //BuildPipeline.BuildPlayer(scenes, BuildExe, BuildTarget.StandaloneWindows64, BuildOptions.None);

        BuildPlayerOptions opts = GetDefaultOptions();
        opts.options |= BuildOptions.Development;
        opts.options |= BuildOptions.AllowDebugging;
        BuildReport report = BuildPipeline.BuildPlayer(opts);

        //Copy files that do not get automatically included into the build here
        CopyFiles(Path.Combine(Application.dataPath, "*.xml"), BuildDataDir);
        CopyFiles(Path.Combine(Application.dataPath, "*.txt"), BuildDataDir);
        CopyFiles(Path.Combine(Application.dataPath, "*.chm"), BuildDataDir);
        //CopyFiles(Path.Combine(Application.dataPath, "../MFireServer/*.*"), BuildMfireDir);

        Debug.Log("Done");
    }

    [MenuItem("Build/Build Scripts Only", false, 0)]
    static void BuildScriptsOnly()
    {
        GitVersion.UpdateVersion();
        GitVersion.SaveVersion();

        BuildPlayerOptions opts = GetDefaultOptions();
        opts.options |= BuildOptions.Development;
        opts.options |= BuildOptions.BuildScriptsOnly;
        opts.options |= BuildOptions.AutoRunPlayer;
        opts.options |= BuildOptions.AllowDebugging;

        BuildReport report = BuildPipeline.BuildPlayer(opts);
    }

    [MenuItem("Build/Build Local and Run", false, 2)]
    static void BuildMineAndRun()
    {
        BuildMineLocal();
        Process.Start(BuildExe);
    }

    [MenuItem("Build/Build to SelfEscape", false, 4)]
    static void BuildMineOnNetwork()
    {
        string dateSubdirectory = System.DateTime.Now.Year + "_" + System.DateTime.Now.Month.ToString("00") + "_" + System.DateTime.Now.Day.ToString("00");
        BuildMineLocal();
        Process.Start("robocopy.exe", string.Format("{0} {1} /mir", BuildPath, Path.Combine(NetworkPath, dateSubdirectory)));
    }

    [MenuItem("Build/Bake Lighting", false, 5)]
    static void BakeLighting()
    {
        if (EditorUtility.DisplayDialog("Confirm Bake Lighting", "Light baking is a time consuming process (30 mins or more).", "Bake", "Cancel"))
        {
            Lightmapping.GIWorkflowMode initialMode = Lightmapping.giWorkflowMode;
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
            Lightmapping.Bake();
            Lightmapping.giWorkflowMode = initialMode;
        }
    }

    [MenuItem("Build/Run Last Build", false, 20)]
    static void BuildMineExecuteLast()
    {
        Process.Start(BuildExe);
    }

    [MenuItem("Build/Build Benchmark Application", false, 30)]
    static void BuildBenchmarkApp()
    {
        GitVersion.UpdateVersion();
        GitVersion.SaveVersion();

        ContentUpdateScript.GroupFilterFunc = null;
        bool success = AddressableBuild.BuildAddressables();
        if (!success)
        {
            Debug.LogError("Addressable build failed.");
        }

        //string[] scenes = (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
        Directory.CreateDirectory(Path.GetDirectoryName(BenchmarkPath));
        //Directory.CreateDirectory(BuildMfireDir);
        //BuildPipeline.BuildPlayer(scenes, BuildExe, BuildTarget.StandaloneWindows64, BuildOptions.None);

        BuildPlayerOptions opts = GetDefaultOptions();
        opts.scenes = new string[] { "Assets/Scenes/TestScenes/BenchmarkScene.unity" };
        opts.locationPathName = BenchmarkPath;

        BuildReport report = BuildPipeline.BuildPlayer(opts);

        //Copy files that do not get automatically included into the build here
        //CopyFiles(Path.Combine(Application.dataPath, "*.xml"), BuildDataDir);
        //CopyFiles(Path.Combine(Application.dataPath, "*.txt"), BuildDataDir);
        //CopyFiles(Path.Combine(Application.dataPath, "*.chm"), BuildDataDir);
        //CopyFiles(Path.Combine(Application.dataPath, "../MFireServer/*.*"), BuildMfireDir);

        Debug.Log("Done");
    }

    [MenuItem("Build/Build Relay", false, 500)]
    static void BuildRelay()
    {

        //ContentUpdateScript.GroupFilterFunc = (group) =>
        //{
        //    return false;
        //};

        //bool success = AddressableBuild.BuildAddressables();
        //if (!success)
        //{
        //    Debug.LogError("Addressable build failed.");
        //}

        AddressableBuild.CleanContent();

        //AddressableAssetSettings.CleanPlayerContent

        //string[] scenes = (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
        Directory.CreateDirectory(Path.GetDirectoryName(RelayBuildPath));
        //Directory.CreateDirectory(BuildMfireDir);
        //BuildPipeline.BuildPlayer(scenes, BuildExe, BuildTarget.StandaloneWindows64, BuildOptions.None);

        BuildPlayerOptions opts = GetDefaultOptions();
        opts.locationPathName = RelayBuildPath;
        opts.scenes = new string[] { "Assets/Scenes/Relay/MainScene.unity" };
        opts.subtarget = (int)StandaloneBuildSubtarget.Server;
        BuildReport report = BuildPipeline.BuildPlayer(opts);

        Debug.Log("Done");
    }
}
