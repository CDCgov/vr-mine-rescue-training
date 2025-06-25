using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools;


[assembly: TestPlayerBuildModifier(typeof(PlayModeTestsBuildModifier))]
public class PlayModeTestsBuildModifier : ITestPlayerBuildModifier
{
    public BuildPlayerOptions ModifyOptions(BuildPlayerOptions playerOptions)
    {
        Debug.Log("PlayModeTestsBuildModifier Active");

        //playerOptions.options |= BuildOptions.AllowDebugging;

        List<string> sceneList = null;
        string[] scenes = playerOptions.scenes;
        if (scenes == null)
            sceneList = new List<string>();
        else
            sceneList = new List<string>(scenes);

        //sceneList.Add("Assets/Tests/Scenes/" + NetworkManagerPlayModeTests.TestSceneName + ".unity");

        foreach (var scene in AssetDatabase.FindAssets("t:SceneAsset", new string[] { "Assets" }))
        {
            var sceneName = AssetDatabase.GUIDToAssetPath(scene);
            sceneList.Add(sceneName);
        }

        playerOptions.scenes = sceneList.ToArray();
        return playerOptions;
    }
}
