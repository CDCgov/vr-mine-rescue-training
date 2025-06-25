using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NIOSH_EditorLayers;
using System.Linq;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Scenario Editor/LoadableAssetCollection")]
public class LoadableAssetCollection : ScriptableObject
{
    [FormerlySerializedAs("loadedAssetMasterList")]
    public List<LoadableAsset> LoadableAssets = new List<LoadableAsset>();
    public List<LoadableVariantSet> LoadableVariantSets = new List<LoadableVariantSet>();


    Dictionary<LayerManager.EditorLayer, List<LoadableAsset>> loadedAssetCategories = new Dictionary<LayerManager.EditorLayer, List<LoadableAsset>>();

    public void BuildCategories()
    {
        loadedAssetCategories.Clear();
        List<LoadableAsset> sortedList = LoadableAssets.OrderBy(o => o.GetAssetWindowName()).ToList();
        
        foreach(LoadableAsset asset in sortedList)
        {
            LayerManager.EditorLayer assetLayer = asset.GetLayer();
            if (loadedAssetCategories.ContainsKey(assetLayer))
            {
                loadedAssetCategories[assetLayer].Add(asset);
            }
            else
            {
                List<LoadableAsset> tempAssets = new List<LoadableAsset>();
                tempAssets.Add(asset);                
                loadedAssetCategories.Add(assetLayer, tempAssets);
            }
        }
        
    }

    //public LoadableAsset GetLoadableAsset(string assetName)
    //{
    //    LoadableAsset asset = null;
    //    IEnumerable<LoadableAsset> assets = LoadableAssets.Where(a => a.DisplayName == assetName);
    //    if(assets.Count() > 0)
    //    {
    //        asset = assets.First();
    //    }

    //    if (asset != null)
    //    {
    //        return asset;
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Could not find asset by the name of: " + assetName);
    //        return null;
    //    }
    //}

    public List<LoadableAsset> GetLoadableAssetsByCategory(LayerManager.EditorLayer layer)
    {
        if(loadedAssetCategories.ContainsKey(layer))
        {
            return new List<LoadableAsset>(loadedAssetCategories[layer]);
        }
        else
        {
            Debug.LogError("No category called: " + layer.ToString() + " found.");
            return new List<LoadableAsset>();
        }
    }

    //public List<LoadableAsset> GetLoadableAssetsByCategory(LayerManager.EditorLayer layer, TileType tileType)
    //{
    //    if (layer == LayerManager.EditorLayer.Mine) {
    //        if (loadedAssetCategories.ContainsKey(layer)){
    //            List<LoadableAsset> tileList = new List<LoadableAsset>();
    //            List<LoadableAsset> categoryFullList = new List<LoadableAsset>(loadedAssetCategories[layer]);
    //            foreach(LoadableAsset asset in categoryFullList)
    //            {
    //                if (asset.GetAsset(true).GetComponent<Min>){

    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        if (loadedAssetCategories.ContainsKey(layer))
    //        {
    //            return new List<LoadableAsset>(loadedAssetCategories[layer]);
    //        }
    //        else
    //        {
    //            Debug.LogError("No category called: " + layer.ToString() + " found.");
    //            return new List<LoadableAsset>();
    //        }
    //    }
    //}

    public List<LoadableAsset> GetAllLoadableAssets()
    {
        return LoadableAssets;
    }    
}
