using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiMonitorTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"MultiMonitor: {Display.displays.Length} monitors found");

        var mainDisplay = Display.displays[0];
        Screen.SetResolution(mainDisplay.systemWidth, mainDisplay.systemHeight, FullScreenMode.FullScreenWindow);

        if (Display.displays.Length > 1)
        {
            var cam = GetComponent<Camera>();
            var display = Display.displays[1];
            display.Activate();            
        }

        for (int i = 0; i < Display.displays.Length; i++)
        {
            var display = Display.displays[i];
            Debug.Log($"MultiMonitor: Display {i}: {display.systemWidth}x{display.systemHeight}");

            //display.SetRenderingResolution(display.systemWidth, display.systemHeight);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
