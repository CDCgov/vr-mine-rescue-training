using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SimpleScreenshot : MonoBehaviour
{
    public int SizeMultiplier = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Directory.CreateDirectory("Screenshots");
            string filename = string.Format("Screenshots/Screenshot_{0}.png", System.DateTime.Now.ToString("yyyy-M-dd_HH-mm-ss"));
            Debug.Log($"Taking screenshot {filename}");

            ScreenCapture.CaptureScreenshot(filename, SizeMultiplier);
        }
    }
}
