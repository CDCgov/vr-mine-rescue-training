using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadKickstarter : MonoBehaviour
{
    public ScenarioSaveLoad saveLoad;
    // Start is called before the first frame update
    void Start()
    {
        saveLoad.LoadScenarioFromFile("Test", false, false);
    }
}
