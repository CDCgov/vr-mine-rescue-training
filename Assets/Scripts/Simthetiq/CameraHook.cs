/*
 * NAME:CameraHook.cs
 * DESC:Search for all the camera in the scene, apply the PostProcess Script then deactivate itself
*/


using UnityEngine;
//using UnityEngine.PostProcessing;

public class CameraHook : MonoBehaviour {

    // // Use this for initialization
    // public PostProcessingProfile postProcessingProfile;
    // private Camera[] cameras;


    // void LateUpdate()
    // {
    //     cameras = FindObjectsOfType<Camera>();

    //     for (int i = 0; i<cameras.Length; i++)
    //     {
    //       PostProcessingBehaviour tempCamPP = cameras[i].GetComponent<PostProcessingBehaviour>();
    //         if (tempCamPP == null)
    //         {
                
    //             tempCamPP = cameras[i].gameObject.AddComponent<PostProcessingBehaviour>();
    //         }

    //         tempCamPP.profile = postProcessingProfile;
    //         enabled = false;
    //     }
    // }


}
