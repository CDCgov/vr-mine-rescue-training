using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRSceneChangeHotkeys : MonoBehaviour
{
    public InputBindingManager InputBindingManager;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        if (InputBindingManager == null)
            InputBindingManager = InputBindingManager.GetDefault();

        InputBindingManager.RegisterAction("LoadProxExperiment", "VRSceneChange", LoadProxExperiment);
        InputBindingManager.RegisterAction("LoadSurfaceMine", "VRSceneChange", LoadSurfaceMine);
        InputBindingManager.RegisterAction("LoadUndergroundMine", "VRSceneChange", LoadUndergroundMine);
        InputBindingManager.RegisterAction("LoadBH20Garage", "VRSceneChange", LoadBH20Garage);

    }


    private void OnDisable()
    {
        InputBindingManager.UnregisterAction("LoadProxExperiment");
        InputBindingManager.UnregisterAction("LoadSurfaceMine");
        InputBindingManager.UnregisterAction("LoadUndergroundMine");
        InputBindingManager.UnregisterAction("LoadBH20Garage");
    }

    private void LoadProxExperiment()
    {
        Debug.Log("Loading experiment VP_HMD");
        ExperimentManager.CCLoadExperiment("VP_HMD");
    }

    private void LoadSurfaceMine()
    {
        SceneManager.LoadScene("VRDemoScene");
    }

    private void LoadUndergroundMine()
    {
        SceneManager.LoadScene("SurfaceMine_HMD");
    }

    private void LoadBH20Garage()
    {
        SceneManager.LoadScene("BH20Garage");
    }
}
