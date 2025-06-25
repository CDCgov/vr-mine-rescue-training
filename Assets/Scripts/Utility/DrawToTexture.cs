using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawToTexture
{

    public static Texture Draw(Texture2D original, GameObject camPrefab)
    {

        //Texture2D tex = Texture2D.Instantiate<Texture2D>(original);

        RenderTexture rt = new RenderTexture(original.width, original.height, 24, RenderTextureFormat.Default);

        //GameObject obj = new GameObject();
        //var cam = obj.AddComponent<Camera>();
        var obj = GameObject.Instantiate<GameObject>(camPrefab);
        obj.name = "DrawToTexture";
        var cam = obj.GetComponent<Camera>();
        
        cam.targetTexture = rt;
        //UnityEngine.Rendering.HighDefinition.HDCamera h = new UnityEngine.Rendering.HighDefinition.HDCamera();
        
        //cam.backgroundColor = Color.magenta;
        cam.Render();
        cam.Render();

        //cam.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.)

        GameObject.Destroy(obj);

        return rt;
    }
}
