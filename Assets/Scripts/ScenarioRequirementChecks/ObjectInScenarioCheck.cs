using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInScenarioCheck : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ScenarioSaveLoad.Instance?.RaiseScenarioChanged();
    }

    private void OnDestroy()
    {
        ScenarioSaveLoad.Instance?.RaiseScenarioChanged();
    }
}
