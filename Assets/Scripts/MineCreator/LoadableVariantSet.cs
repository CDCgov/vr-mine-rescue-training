using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LoadableVariantSet", menuName = "Scenario Editor/LoadableVariantSet", order = 100)]
public class LoadableVariantSet : ScriptableObject
{
    public List<LoadableAsset> LoadableVariants;

    public LoadableAsset GetNext(string currentAssetID)
    {
        return GetNext(currentAssetID, 1);
    }

    public LoadableAsset GetPrev(string currentAssetID)
    {
        return GetNext(currentAssetID, -1);
    }
     
    public LoadableAsset GetNext(string currentAssetID, int increment)
    {
        var index = GetIndex(currentAssetID);
        if (index < 0)
            return null;

        index = index + increment;
        if (index < 0)
            index = index % LoadableVariants.Count + LoadableVariants.Count;
        if (index >= LoadableVariants.Count)
            index %= LoadableVariants.Count;
        
        return LoadableVariants[index];
    }

    public int GetIndex(string assetID)
    {
        if (LoadableVariants == null || LoadableVariants.Count <= 0)
            return -1;

        for (int i = 0; i < LoadableVariants.Count; i++)
        {
            if (LoadableVariants[i].AssetID == assetID)
                return i;
        }

        return -1;
    }
}
