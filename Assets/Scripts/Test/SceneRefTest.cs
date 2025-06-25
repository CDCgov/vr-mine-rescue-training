using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneRefTest : MonoBehaviour
{

    public Scene TestScene;

    // Use this for initialization
    void Start()
    {
        int numScenes = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < numScenes; i++)
        {
            var scene = SceneManager.GetSceneByBuildIndex(i);
            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            Debug.Log($"Scene {i}: {scene.name} {scenePath}");
        }		

    }

    // Update is called once per frame
    void Update()
    {

    }
}
