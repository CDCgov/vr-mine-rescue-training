using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateHelper : MonoBehaviour
{
    public System.Action UpdateCallback;


    // Update is called once per frame
    void Update()
    {
        if (UpdateCallback != null)
            UpdateCallback();
    }
}
