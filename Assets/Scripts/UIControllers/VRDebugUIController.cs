using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class VRDebugUIController : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public GameObject DebugUIRoot;
    public UIVRSpawnObjectListController SpawnObjectListController;

    private bool _lastButtonState = false;
    private bool _lastTriggerState = false;
    private List<InputDevice> _xrDevices;


    private Canvas _canvas;
    private XRInteractorLineVisual[] _lineVisuals;

    //public bool DebugMode
    //{
    //    get => _debugMode;
    //    set
    //    {
    //        _debugMode = value;
    //    }
    //}
    private void Awake()
    {
        _xrDevices = new List<InputDevice>();
    }

    void OnEnable()
    {
        List<InputDevice> allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);
        foreach (InputDevice device in allDevices)
            InputDevices_deviceConnected(device);

        InputDevices.deviceConnected += InputDevices_deviceConnected;
        InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;

        if (SpawnObjectListController == null)
            SpawnObjectListController = GetComponentInChildren<UIVRSpawnObjectListController>(true);
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= InputDevices_deviceConnected;
        InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
        _xrDevices.Clear();

        if (SpawnObjectListController != null)
        {
            SpawnObjectListController.ClearSelection();
        }
    }

    private void InputDevices_deviceConnected(InputDevice device)
    {
        bool discardedValue;
        if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out discardedValue))
        {
            _xrDevices.Add(device); 
        }
    }

    private void InputDevices_deviceDisconnected(InputDevice device)
    {
        if (_xrDevices.Contains(device))
            _xrDevices.Remove(device);
    }

    private void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _canvas = DebugUIRoot.GetComponent<Canvas>();
        DebugUIRoot.SetActive(false);

        SceneManager.sceneLoaded += OnSceneLoaded;

        
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        EnableUI(false);
    }

    private void EnableLineVisual(bool bEnable)
    {
        if (bEnable)
            _lineVisuals = FindObjectsOfType<XRInteractorLineVisual>();

        if (_lineVisuals != null)
        {
            foreach (var lineVis in _lineVisuals)
            {
                if (lineVis != null)
                    lineVis.enabled = bEnable;
            }
        }
    }

    private void ToggleUI()
    {
        var active = DebugUIRoot.activeSelf;

        EnableUI(!active);
    }

    private void SpawnSelectedObject(Transform source)
    {
        //var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //obj.transform.position = source.position;

        if (SpawnObjectListController == null || SpawnObjectListController.SelectedObject == null)
            return;

        var ray = new Ray(source.position, source.forward);

        Debug.Log($"VRDebugUI: Spawning object {SpawnObjectListController.SelectedObject.ObjectName} at {ray.ToString()}");
        SpawnObjectListController.SelectedObject.Spawn(ray, LayerMask.GetMask("Floor", "Default"), NetworkManager.GetDefault(gameObject));


    }

    public void CloseMenu()
    {
        EnableUI(false);
    }

    private void EnableUI(bool bEnable)
    {
        if (bEnable)
        {
            var cam = Camera.main;

            var pos = cam.transform.position + cam.transform.forward * 1.2f;
            pos.y = 0;
            transform.position = pos;

            _canvas.worldCamera = cam;
            EnableLineVisual(true);

            var camEuler = cam.transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, camEuler.y, 0);

            DebugUIRoot.SetActive(true);
        }
        else
        {
            EnableLineVisual(false);
            DebugUIRoot.SetActive(false);

            if (SpawnObjectListController != null)
            {
                SpawnObjectListController.ClearSelection();
            }
        }
    }

    void Update()
    {
        bool buttonState = false;
        bool triggerState = false;
        //if (!_debugMode)
        //{
        //    return;
        //}

        if (PlayerManager != null && PlayerManager.CurrentPlayer != null && 
            !PlayerManager.CurrentPlayer.DebugInterfaceEnabled)
            return;

        foreach (var device in _xrDevices)
        {
            bool tempState;

            tempState = false;
            buttonState = device.TryGetFeatureValue(CommonUsages.secondaryButton, out tempState) // did get a value
                        && tempState // the value we got
                        || buttonState; // cumulative result from other controllers

            //tempState = false;
            //triggerState = device.TryGetFeatureValue(CommonUsages.triggerButton, out tempState) // did get a value
            //            && tempState // the value we got
            //            || triggerState; // cumulative result from other controllers
            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out tempState))
            {
                if (tempState && !_lastTriggerState)
                {
                    var controllers = FindObjectsOfType<AltRelativePositionDriver>();
                    foreach (var controller in controllers)
                    {
                        if (((device.characteristics & InputDeviceCharacteristics.Left) > 0 && controller.InputDeviceNode == XRNode.LeftHand) ||
                            ((device.characteristics & InputDeviceCharacteristics.Right) > 0 && controller.InputDeviceNode == XRNode.RightHand))
                        {
                            SpawnSelectedObject(controller.transform);
                            break;
                        }
                    }
                }

                triggerState = tempState || triggerState;
            }

        }

        _lastTriggerState = triggerState;

        if (buttonState != _lastButtonState) // Button state changed since last frame
        {
            if (buttonState)
                ToggleUI();
            _lastButtonState = buttonState;
        }

        //if (triggerState != _lastTriggerState)
        //{
        //    if (triggerState)
        //    {
        //        if (SpawnObjectListController != null && SpawnObjectListController.SelectedObject != null)
        //        {

        //        }
        //    }

        //    _lastTriggerState = triggerState;
        //}

        if (_canvas != null)
        {
            //var graphics = GraphicRegistry.GetGraphicsForCanvas(_canvas);
            //foreach (var graphic in graphics)
            //{
                
             
            //}
        }
    }
}
