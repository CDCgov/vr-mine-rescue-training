using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VehicleProxHMDKeybinds : MonoBehaviour
{

    public ExperimentManager ExperimentManager;

    // Use this for initialization
    void Start()
    {
        if (ExperimentManager == null)
            ExperimentManager = ExperimentManager.GetDefault();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.X))
        {
            Time.timeScale = 1;
            ExperimentManager.TerminateExperiment();
            SceneManager.LoadScene("VRDemoScene");
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Time.timeScale = 0;
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Time.timeScale = 1;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ExperimentManager.SkipCurrentTrial();
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ExperimentManager.TerminateExperiment();
            ExperimentManager.CCLoadExperiment("VP_HMD");
        }
    }
}
