using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MineSegmentAssetLoader : PlacableAssetLoader
{
    const string pathToSegmentsBundle = "AssetBundles/assets/mineSegments";

    public List<GameObject> segments;

    GameObject activeTileAssetRow;

    // Start is called before the first frame update
    void Start()
    {
        //LoadAssetBundle(pathToSegmentsBundle);

        for (int i = 0; i < segments.Count; i++)
        {
            CreateUIAsset(segments[i]);
        }
    }

    void LoadAssetBundle(string pathToBundle)
    {
        AssetBundle loadedBundle
        = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, pathToBundle));
        Debug.Log(loadedBundle.name);
        if (loadedBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            return;
        }

        Object[] loadedAssets = loadedBundle.LoadAllAssets();
        foreach (Object asset in loadedAssets)
        {
            CreateUIAsset(asset);
        }
    }

    void CreateUIAsset(Object obj)
    {
        if (activeTileAssetRow == null || activeTileAssetRow.transform.childCount >= maxAssetsInRow)
        {
            activeTileAssetRow = Instantiate(uiRowObject, uiContentObj.transform);

        }
        GameObject go = Instantiate(uiObject, activeTileAssetRow.transform);
        AssetUIObject uiObj = go.GetComponent<AssetUIObject>();
        //uiObj.Initialize(obj as GameObject);
    }
}
