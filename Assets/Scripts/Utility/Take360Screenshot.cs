using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Take360Screenshot : MonoBehaviour
{
    public RenderTexture cubemapLeft;
    public RenderTexture cubemapRight;
    public RenderTexture equirect;
    public bool renderStereo = true;
    public float stereoSeparation = 0.064f;
    public string SaveFolder = "C:\\Screenshot360";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
        {
            TakeScreenshot();
        }
    }

    void TakeScreenshot()
    {
        Camera cam = GetComponent<Camera>();

        if (cam == null)
        {
            cam = GetComponentInParent<Camera>();
        }

        if (cam == null)
        {
            Debug.Log("stereo 360 capture node has no camera or parent camera");
        }

        if (renderStereo)
        {
            cam.stereoSeparation = stereoSeparation;
            cam.RenderToCubemap(cubemapLeft, 63, Camera.MonoOrStereoscopicEye.Left);
            cam.RenderToCubemap(cubemapRight, 63, Camera.MonoOrStereoscopicEye.Right);
        }
        else
        {
            cam.RenderToCubemap(cubemapLeft, 63, Camera.MonoOrStereoscopicEye.Mono);
        }

        //optional: convert cubemaps to equirect

        if (equirect == null)
        {
            return;
        }

        if (renderStereo)
        {
            cubemapLeft.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Left);
            cubemapRight.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Right);
        }
        else
        {
            cubemapLeft.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Mono);

            //Texture2D tex = new Texture2D(equirect.width, equirect.height, TextureFormat.RGB24, false);
            Texture2D tex = new Texture2D(equirect.width, equirect.height * 2, TextureFormat.RGB24, false);
            RenderTexture.active = equirect;
            tex.ReadPixels(new Rect(0, 0, equirect.width, equirect.height), 0, 0);
            tex.ReadPixels(new Rect(0, 0, equirect.width, equirect.height), 0, equirect.height);
            RenderTexture.active = null;
            //var imgData = ImageConversion.EncodeToJPG(equirect);

            var imgData = tex.EncodeToJPG();
            File.WriteAllBytes(Path.Combine(SaveFolder, System.Guid.NewGuid().ToString() + ".jpg"), imgData);
        }
    }
}
