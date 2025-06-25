using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableMultiMonitor : MonoBehaviour
{
    public KeyCode EnableMultiMonitorHotkey = KeyCode.Space;

    private bool _multiMonitorEnabled = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"MultiMonitor: {Display.displays.Length} monitors found");

        for (int i = 0; i < Display.displays.Length; i++)
        {
            var display = Display.displays[i];
            Debug.Log($"MultiMonitor: Display {i}: {display.systemWidth}x{display.systemHeight}");
        }


    }

    private void EnableSecondMonitor()
    {
        if (_multiMonitorEnabled)
            return;

        _multiMonitorEnabled = true;

        var mainDisplay = Display.displays[0];
        Screen.SetResolution(mainDisplay.systemWidth, mainDisplay.systemHeight, FullScreenMode.FullScreenWindow);

        if (Display.displays.Length > 1)
        {
            var cam = GetComponent<Camera>();
            var display = Display.displays[1];
            display.Activate();
        }

    }

    private void SwapMonitors()
    {
        var cameras = FindObjectsOfType<Camera>();

        foreach (var cam in cameras)
        {
            if (cam.targetDisplay == 0)
                cam.targetDisplay = 1;
            else
                cam.targetDisplay = 0;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(EnableMultiMonitorHotkey))
        {
            if (!_multiMonitorEnabled)
            {
                EnableSecondMonitor();
            }
            else
            {
                SwapMonitors();
            }
            //this.enabled = false;
        }
    }
}
