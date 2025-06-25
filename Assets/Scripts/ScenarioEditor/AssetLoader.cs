using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NIOSH_EditorLayers;
using System;

public class AssetLoader : MonoBehaviour
{
    //public LoadableAssetCollection Loadables;
    public static AssetLoader Instance;
    public bool IsEditorMode = false;

    public void OnEnable()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        //if (Loadables != null)
        //{
        //    Loadables.BuildCategories();
        //}
        Util.DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += CheckActiveScene;
    }

    //public GameObject GetPlaceableAsset(string assetName)
    //{
    //    GameObject obj = null;
    //    try
    //    {
    //        obj = Loadables.GetLoadableAsset(assetName).GetAsset(IsEditorMode);
    //    }
    //    catch(Exception e)
    //    {
    //        Debug.LogWarning("Asset: " + assetName + " not found in asset library");
    //    }
    //    return obj;
    //}

    //public IEnumerator GetAssetsByCategory(LayerManager.EditorLayer layer, Action<List<LoadableAsset>> callback)
    //{
    //    callback(Loadables.GetLoadableAssetsByCategory(layer));
    //    yield return null;
    //}


    void CheckActiveScene(Scene scene, LoadSceneMode mode)
    {
        IsEditorMode = SceneManager.GetSceneByName("BAH_ScenarioEditor").isLoaded;
    }
}
