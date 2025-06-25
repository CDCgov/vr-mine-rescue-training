using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevAssetBuilder : MonoBehaviour
{
    public string TargetFolder;

    public string TargetFileName;

    // Start is called before the first frame update
    void Start()
    {
        ExternalAssetMetadata data = new ExternalAssetMetadata();
        data.SourceFolder = TargetFolder;
        data.GeometryFilename = TargetFileName;
        data.AssetName = "Temp Dev Asset";

        ExternalAssetBuilder.BuildGeometryObject(data, LoadableAssetManager.GetDefault(gameObject));
    }
}
