using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

[HasCommandConsoleCommands]
[CreateAssetMenu(fileName = "SystemManager", menuName = "VRMine/Managers/SystemManager", order = 0)]
public class SystemManager : Manager
{
    public const string DefaultResourcePath = "Managers/SystemManager";
    public static SystemManager GetDefault() 
    {
        if (_defaultSystemManager == null)
            _defaultSystemManager = Resources.Load<SystemManager>(DefaultResourcePath);

        return _defaultSystemManager;
        //return Resources.Load<SystemManager>(DefaultResourcePath); 
    }

    private static SystemManager _defaultSystemManager = null;

    public SystemConfig SystemConfig;
    public GraphicsConfig GraphicsConfig;

    public GameObject CameraPrefab;
    public GameObject CameraPrefab360;
    public GameObject CameraPrefabVR;
    public string TestString = "";

    public event UnityAction MainCameraChanged = delegate {};
    public event System.Action<GraphicsConfig> GraphicsConfigChanged;

    public Camera MainCamera
    {
        get
        {
            return _mainCamera;
        }
    }

    private Camera _mainCamera;

    [CommandConsoleCommand("test_res_load", "Test multile resource.load calls")]
    public static void TestCommand()
    {	
        var res1 = Resources.Load<SystemManager>(DefaultResourcePath);
        var res2 = Resources.Load<SystemManager>(DefaultResourcePath);

        res1.TestString = "Res1";

        CommandConsole.Print($"Res2 TestString: {res2.TestString}");
    }

    [CommandConsoleCommand("save_config", "Overwite the system configuration files with current values")]
    public static void CCSaveConfig()
    {
        var system = SystemManager.GetDefault();

        system.SystemConfig.SaveConfig();

        CommandConsole.Print("Config Saved");
    }

    [CommandConsoleCommand("xr_status", "Get information about VR/AR settings")]
    public static void CCGetXRStatus()
    {
        CommandConsole.Print($"XR Enabled: {XRSettings.enabled} XR Device: {XRSettings.loadedDeviceName}");
    }

    public GameObject CreateCamera(Transform parent = null)
    {
        GameObject cam = null;

        if (SystemConfig.SystemType == SystemType.CAVE)
        {
            if (CameraPrefab360 != null)
            {
                cam = Instantiate<GameObject>(CameraPrefab360);
                var cam360 = cam.GetComponent<Camera360>();
                
                cam360.BuildCameras();

                _mainCamera = cam360.ForwardCam;
            }
        }
        else
        {
            if (XRSettings.enabled && CameraPrefabVR != null)
            {
                cam = Instantiate<GameObject>(CameraPrefabVR);

                _mainCamera = cam.GetComponent<Camera>();
            }
            else if (CameraPrefab != null)
            {
                cam = Instantiate<GameObject>(CameraPrefab);

                _mainCamera = cam.GetComponent<Camera>();
            }			
        }
        
        if (cam == null)
        {
            cam = new GameObject();
            cam.AddComponent<Camera>();
        }

        if (parent != null)
        {
            cam.transform.SetParent(parent, false);
        }

        cam.transform.localPosition = Vector3.zero; 
        cam.transform.localRotation = Quaternion.identity;

        MainCameraChanged();
         
        return cam;
    }
    public void LoadGraphicsConfig()
    {
        GraphicsConfig = YAMLConfig.LoadConfig<GraphicsConfig>("graphics_config.yaml");

        RaiseGraphicsConfigChanged();
    }

    //public virtual void Awake() 
    //{
    //    Debug.Log("SystemManager Awake");		
    //}

    public virtual void OnEnable()
    {
        //Debug.Log("SystemManager OnEnable");
        //Debug.Log($"XR Enabled: {XRSettings.enabled} XR Device: {XRSettings.loadedDeviceName}");
        SystemConfig = YAMLConfig.LoadConfig<SystemConfig>("system_config_v2.yaml");

        LoadGraphicsConfig();
    }

    public void OnDisable() 
    {

    }


    private void RaiseMainCameraChanged()
    {
        var handler = MainCameraChanged;
        if (handler != null)
            handler();
    }

    private void RaiseGraphicsConfigChanged()
    {
        if (GraphicsConfig == null)
            return;

        GraphicsConfigChanged?.Invoke(GraphicsConfig);
    }
}