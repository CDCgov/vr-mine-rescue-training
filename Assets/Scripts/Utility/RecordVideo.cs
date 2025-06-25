using UnityEngine;
using System.Collections;
using System.IO;

public class RecordVideo : MonoBehaviour
{
    public int RecordFrameRate = 60;

    private string _folder;
    private bool _recording = false;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.R) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        if (Input.GetKeyDown(KeyCode.ScrollLock))
        {
            if (_recording)
                StopRecording();
            else
                StartRecording();
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            Debug.Log("Taking Screenshot");
            TakeScreenshot();
        }
        

        if (_recording)
        {
            string fileName = string.Format("{0}/{1:D06}.png", _folder, Time.frameCount.ToString());
            //Debug.LogFormat("Saving Screen {0}", fileName);
            ScreenCapture.CaptureScreenshot(fileName);
        }
    }

    private void OnGUI()
    {
        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Print)
        {
            Debug.Log("Taking Screenshot");
            TakeScreenshot();
        }
    }

    public static void TakeScreenshot()
    {
        Directory.CreateDirectory("Screenshots");
        string filename = string.Format("Screenshots/Screenshot_{0}.png", System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        ScreenCapture.CaptureScreenshot(filename, 6);
    }

    public void StartRecording()
    {
        _folder = "Rec_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        Debug.LogFormat("Recording to {0}", _folder);

        Directory.CreateDirectory(_folder);

        Time.captureFramerate = RecordFrameRate;
        _recording = true;
    }

    public void StopRecording()
    {
        Time.captureFramerate = 0;
        _recording = false;
        Debug.Log("Recording Stopped");
    }
}
