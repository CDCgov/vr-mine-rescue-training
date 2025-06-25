using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTrucks360 : MonoBehaviour
{
    public SystemManager SystemManager;
    public GameObject CameraPrefab;
    //public Text OverlapDisplay;
    //public Text CamDistance;
    public Text CamFov;
    public List<GameObject> Trucks;

    public int NumScreens = 6;
    public float Overlap = 10;

    public Camera ForwardCam
    {
        get
        {
            return _forwardCam;
        }
    }


    // public float CamPos = 0;
    // private float CamPosTrack = 0;
    private float _aspect;

    //private GlobalFog[] _fog;
    //private Transform[] _fogSources;

    private Camera _forwardCam;
    private bool _initialized = false;

    private void Awake()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
    }

    // Use this for initialization
    void Start()
    {
        if (!_initialized)
            BuildCameras();
    }


    public void BuildCameras()
    {
        NumScreens = SystemManager.SystemConfig.NumScreens;
        Overlap = SystemManager.SystemConfig.Overlap;

        Camera originalCamera = GetComponent<Camera>();
        if (originalCamera != null)
            _aspect = originalCamera.aspect / (float)NumScreens;
        else
        {
            float displayAspect = Screen.width / Screen.height;
            _aspect = displayAspect / (float)NumScreens;
        }

        //OverlapDisplay.text = "Overlap: " + Overlap;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Camera>() != null)
                Destroy(child.gameObject);
        }


        Camera origCam = GetComponent<Camera>();
        if (origCam != null)
            origCam.enabled = false;

        float hfov = (360.0f + Overlap) / (float)NumScreens;

        float rotation = 360.0f / (float)NumScreens;
        float screenWidth = 1.0f / (float)NumScreens;
        //float aspect = origCam.aspect / (float)NumScreens;
        float aspect = _aspect;

        float vfov = hfov * Mathf.Deg2Rad;
        vfov = Mathf.Atan(Mathf.Tan(0.5f * vfov) / aspect) * 2;
        vfov *= Mathf.Rad2Deg;

        int uiCamMask = LayerMask.GetMask("UI");

        Color bgColor = SystemManager.SystemConfig.CAVEBackgroundColor.ToColor();


        for (int i = 0; i < NumScreens; i++)
        {
            GameObject gCam = (GameObject)Instantiate(CameraPrefab);
            gCam.name = string.Format("Camera {0}", i);

            var uiCamObj = new GameObject("UICam");
            uiCamObj.transform.SetParent(gCam.transform, false);
            var uiCam = uiCamObj.AddComponent<Camera>();


            Camera cam = gCam.GetComponent<Camera>();
            if (origCam != null)
                cam.CopyFrom(origCam);

            //gCam.transform.SetParent(transform, false);
            //gCam.transform.SetParent(transform, false);//Cause of the vertical positioning bug
            //gCam.transform.localRotation = Quaternion.AngleAxis(rotation * i, Vector3.up);

            Rect r = new Rect(screenWidth * i, 0, screenWidth, 1);
            cam.rect = r;

            //cam.fieldOfView = fov / aspect;
            cam.fieldOfView = vfov;
            cam.aspect = _aspect;
            cam.backgroundColor = bgColor;

            uiCam.CopyFrom(cam);
            uiCam.cullingMask = uiCamMask;
            uiCam.clearFlags = CameraClearFlags.Depth;
            uiCam.depth = 50;


            if (i == 0)
                _forwardCam = cam;

            if(i < 2)
            {
                gCam.transform.SetParent(Trucks[0].transform, true);
                gCam.transform.localPosition = new Vector3(0, 3f, -15);
                if (i % 2 == 0)
                {
                    gCam.transform.localRotation = Quaternion.AngleAxis(-30, Vector3.up);
                }
                else
                {
                    gCam.transform.localRotation = Quaternion.AngleAxis(30, Vector3.up);
                }
            }
            else if(i >= 2 && i < 4)
            {
                gCam.transform.parent = Trucks[1].transform;
                gCam.transform.localPosition = new Vector3(0, 3f, -15);
                if (i % 2 == 0)
                {
                    gCam.transform.localRotation = Quaternion.AngleAxis(-30, Vector3.up);
                }
                else
                {
                    gCam.transform.localRotation = Quaternion.AngleAxis(30, Vector3.up);
                }
            }
            else
            {
                gCam.transform.parent = Trucks[2].transform;
                gCam.transform.localPosition = new Vector3(0, 3f, -15);
                if (i % 2 == 0)
                {
                    gCam.transform.localRotation = Quaternion.AngleAxis(-30, Vector3.up);
                }
                else
                {
                    gCam.transform.localRotation = Quaternion.AngleAxis(30, Vector3.up);
                }
            }

        }

        _initialized = true;

    }

    // void Update()
    // {
    // 	float minDist = float.MaxValue;

    // 	minDist -= 10;
    // 	float fogPercent = 1.0f - (minDist / 30.0f);
    // 	fogPercent = Mathf.Clamp(fogPercent, 0.001f, 1.0f);

    // 	SetFog(fogPercent);

    // }

    void OnDrawGizmos()
    {
        //Debug.DrawLine(new Vector3(0, 1.494f, 0), new Vector3(0, 1.494f, 6), Color.red);
    }
}
