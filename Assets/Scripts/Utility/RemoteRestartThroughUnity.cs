using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class RemoteRestartThroughUnity : MonoBehaviour
{  
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.End))
        {
            RestartTheComputer();
        }
    }

    public void RestartTheComputer()
    {
        UnityEngine.Debug.Log("OH GOD WHAT HAVE YOU DO--");
        Process.Start("shutdown", "/r /t 0");
    }
}
