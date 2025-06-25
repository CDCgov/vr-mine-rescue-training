using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.HighDefinition;

public class ComponentInfo_Decal : ModularComponentInfo, ISaveableComponent
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "Decal";
    public string DecalPath = "";
    public DecalProjector DecalProjectorReference;
    public Texture2D DefaultTexture;

    private Texture2D _texture;
    private UnityWebRequest www;

    public void LoadInfo(SavedComponent component)
    {
        DecalPath = component.GetParamValueAsStringByName("DecalPath");
        PerformLoadDecal();
    }

    public string[] SaveInfo()
    {
        return new string[] { "DecalPath|" + DecalPath };
    }

    public string SaveName()
    {
        return componentName;
    }

    //public void SelectDecalPath()
    //{
    //    var path = StandaloneFileBrowser.OpenFilePanel("Select Decal File", Application.dataPath, "png", false);
    //    if(path != null)
    //    {
    //        DecalPath = System.IO.Path.Combine(path);
    //    }
    //    LoadDecal();
    //}

    public void PerformLoadDecal()
    {
        LoadDecal();
    }

    private void LoadDecal()
    {
        if (DecalPath != "")
        {
            www = UnityWebRequestTexture.GetTexture("file://" + DecalPath);

            www.SendWebRequest().completed += ComponentInfo_Decal_completed;
            
            //yield return www.SendWebRequest();
            //if (www.result == UnityWebRequest.Result.Success) {
            //    Debug.Log($"Success: {www.result.ToString()}");
            //    _texture = DownloadHandlerTexture.GetContent(www);
            //    _texture.alphaIsTransparency = true;
            //}
            //else
            //{
            //    Debug.Log($"Bad result: {www.error}");
            //}
            
        }
        else
        {
            _texture = DefaultTexture;
            //_texture.alphaIsTransparency = true;

            Material newDecalMat = new Material(DecalProjectorReference.material);
            newDecalMat.SetTexture("_BaseMap", _texture);
            //newDecalMat.mainTexture = _texture;
            DecalProjectorReference.material = newDecalMat;
        }
        
        //Material newDecalMat = new Material(DecalProjectorReference.material);
        //newDecalMat.SetTexture("_BaseMap", _texture);
        ////newDecalMat.mainTexture = _texture;
        //DecalProjectorReference.material = newDecalMat;
    }

    private void ComponentInfo_Decal_completed(AsyncOperation obj)
    {
        if (www.result == UnityWebRequest.Result.Success)
        {
            _texture = DownloadHandlerTexture.GetContent(www);
            Debug.Log($"Success {_texture.name}, {_texture.width}: {_texture.height}");
            
            //_texture.alphaIsTransparency = true;
        }
        else
        {
            _texture = DefaultTexture;
        }

        Material newDecalMat = new Material(DecalProjectorReference.material);
        newDecalMat.EnableKeyword("_BaseColorMap");
        newDecalMat.SetTexture("_BaseColorMap", _texture);
        //newDecalMat.mainTexture = _texture;
        DecalProjectorReference.material = newDecalMat;

    }
}
