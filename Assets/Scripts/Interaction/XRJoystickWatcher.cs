using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

[System.Serializable]
public class PrimaryJoystickEvent : UnityEvent<Vector2> { }

[System.Serializable]
public class SecondaryJoystickEvent : UnityEvent<Vector2> { }

[System.Serializable]
public class PrimaryGripEvent : UnityEvent<float> { }

[System.Serializable]
public class PrimaryTriggerEvent : UnityEvent<float> { }

[System.Serializable]
public class SecondaryGripEvent : UnityEvent<float> { }

[System.Serializable]
public class SecondaryTriggerEvent : UnityEvent<float> { }

[System.Serializable]
public class PrimaryButtonEvent : UnityEvent<bool> { }


public class XRJoystickWatcher : MonoBehaviour
{
    public Vector2 LeftJoystickPosition;
    public Vector2 RightJoystickPosition;

    public PrimaryJoystickEvent primaryJoystickPress;
    private Vector2 _lastAxisVal;
    private List<InputDevice> _devicesWithPrimaryAxis;

    public SecondaryJoystickEvent secondaryJoystickPress;
    private Vector2 _lastSecondaryAxisVal;
    private List<InputDevice> _devicesWithSecondaryAxis;

    public PrimaryGripEvent primaryGripPress;
    private float _lastPrimGripVal;
    public PrimaryTriggerEvent primaryTriggerPress;
    private float _lastPrimTrigVal;

    public SecondaryGripEvent secondaryGripPress;
    private float _lastSecGripVal;
    public SecondaryTriggerEvent secondaryTriggerPress;
    private float _lastSecTrigVal;

    public UnityEvent<bool> PrimaryJoystickClicked;
    private bool _lastPrimaryJoystickClickVal;
    public UnityEvent<bool> SecondaryJoystickClicked;
    private bool _lastSecondaryJoystickClickVal;

    public PrimaryButtonEvent primaryButtonPress;
    public bool PrimaryButtonValue;
    private bool _lastButtonVal;

    private InputDevice _RightHand;
    private InputDevice _LeftHand;
    private float _delay = 10;

    private void Awake()
    {
        LeftJoystickPosition = RightJoystickPosition = Vector2.zero;

        if (primaryJoystickPress == null)
        {
            primaryJoystickPress = new PrimaryJoystickEvent();
        }

        if(secondaryJoystickPress == null)
        {
            secondaryJoystickPress = new SecondaryJoystickEvent();
        }

        _devicesWithPrimaryAxis = new List<InputDevice>();
        _devicesWithSecondaryAxis = new List<InputDevice>();
    }

    private void OnEnable()
    {
        List<InputDevice> allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);
        
        foreach (InputDevice device in allDevices)
            InputDevices_deviceConnected(device);
        
        
        InputDevices.deviceConnected += InputDevices_deviceConnected;
        InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= InputDevices_deviceConnected;
        InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
    }

    private void InputDevices_deviceConnected(InputDevice device)
    {
        Vector2 discardedValue;
        _RightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        _LeftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out discardedValue) && device == _RightHand)
        {
            _devicesWithPrimaryAxis.Add(device);
        }

        if(device.TryGetFeatureValue(CommonUsages.primary2DAxis, out discardedValue) && device == _LeftHand)
        {
            _devicesWithSecondaryAxis.Add(device);
        }
    }

    private void InputDevices_deviceDisconnected(InputDevice device)
    {
        if (_devicesWithPrimaryAxis.Contains(device))
            _devicesWithPrimaryAxis.Remove(device);

        if (_devicesWithSecondaryAxis.Contains(device))
            _devicesWithSecondaryAxis.Remove(device);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 tempState = new Vector2();
        float pGripState = new float();
        float pTriggerState = new float();
        bool pButtonState = new bool();
        RightJoystickPosition = Vector2.zero;

        //This actually forced the controllers to remain on. Investigate for calibration mode.
        //if(Time.time > _delay)
        //{
        //    foreach(var device in _devicesWithPrimaryAxis)
        //    {
        //        device.SendHapticImpulse(0, 0.5f, 1f);
        //    }
        //    foreach (var device in _devicesWithSecondaryAxis)
        //    {
        //        device.SendHapticImpulse(0, 0.5f, 1f);
        //    }
        //    _delay = Time.time + 1;
        //}
        foreach (var device in _devicesWithPrimaryAxis)
        {
            Vector2 primaryAxisState = new Vector2();
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out primaryAxisState))
            {
                tempState = primaryAxisState;
                RightJoystickPosition = RightJoystickPosition + primaryAxisState;
            }
            float primaryGripState = new float();
            if(device.TryGetFeatureValue(CommonUsages.grip, out primaryGripState))
            {
                pGripState = primaryGripState;
            }
            float primaryTriggerState = new float();
            if(device.TryGetFeatureValue(CommonUsages.trigger, out primaryTriggerState))
            {
                pTriggerState = primaryTriggerState;
            }
            bool primaryButtonState = new bool();
            if(device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonState))
            {
                pButtonState = primaryButtonState;
            }

            bool primaryJoystickClickVal = false;
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out primaryJoystickClickVal))
            {
                if (primaryJoystickClickVal != _lastPrimaryJoystickClickVal)
                    PrimaryJoystickClicked?.Invoke(primaryJoystickClickVal);
                _lastPrimaryJoystickClickVal = primaryJoystickClickVal;
            }
        }
        if (tempState != _lastAxisVal)
        {
            primaryJoystickPress.Invoke(tempState);
            _lastAxisVal = tempState;
        }
        if(pGripState != _lastPrimGripVal)
        {
            primaryGripPress.Invoke(pGripState);
            _lastPrimGripVal = pGripState;
        }
        if(pTriggerState != _lastPrimTrigVal)
        {
            primaryTriggerPress.Invoke(pTriggerState);
            _lastPrimTrigVal = pTriggerState;
        }
        if(pButtonState != _lastButtonVal)
        {
            primaryButtonPress.Invoke(pButtonState);
            _lastButtonVal = pButtonState;
        }

        Vector2 secondaryTempState = new Vector2();
        float sGripState = new float();
        float sTriggerState = new float();
        LeftJoystickPosition = Vector2.zero;
        foreach(var device in _devicesWithSecondaryAxis)
        {
            Vector2 secondaryAxisState = new Vector2();
            if(device.TryGetFeatureValue(CommonUsages.primary2DAxis, out secondaryAxisState))
            {
                secondaryTempState = secondaryAxisState;
                LeftJoystickPosition = LeftJoystickPosition + secondaryAxisState;
            }
            float secondaryGripState = new float();
            if(device.TryGetFeatureValue(CommonUsages.grip, out secondaryGripState))
            {
                sGripState = secondaryGripState;
            }
            float secondaryTriggerState = new float();
            if(device.TryGetFeatureValue(CommonUsages.trigger, out secondaryTriggerState))
            {
                sTriggerState = secondaryTriggerState;
            }
            bool secondaryJoystickClickVal = false;
            if (device.TryGetFeatureValue(CommonUsages.secondary2DAxisClick, out secondaryJoystickClickVal))
            {
                if (secondaryJoystickClickVal != _lastSecondaryJoystickClickVal)
                    SecondaryJoystickClicked?.Invoke(secondaryJoystickClickVal);
                _lastSecondaryJoystickClickVal = secondaryJoystickClickVal;

            }
        }
        if(secondaryTempState != _lastSecondaryAxisVal)
        {
            secondaryJoystickPress.Invoke(secondaryTempState);
            _lastSecondaryAxisVal = secondaryTempState;
        }
        if(sGripState != _lastSecGripVal)
        {
            secondaryGripPress.Invoke(sGripState);
            _lastSecGripVal = sGripState;
        }
        if(sTriggerState != _lastSecTrigVal)
        {
            secondaryTriggerPress.Invoke(sTriggerState);
            _lastSecTrigVal = sTriggerState;          
        }
    }

    public void ShakeControllers(bool shake)
    {

    }
}
