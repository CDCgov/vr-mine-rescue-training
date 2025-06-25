using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class OculusInput : VRInputHandler
{
    //private List<InputDevice> leftHandDevices;
    //private List<InputDevice> rightHandDevices;

    private InputDevice _LeftHandDevice;
    private InputDevice _RightHandDevice;
    private void Start()
    {
        var leftHandDevices = new List<InputDevice>();
        var rightHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

        if (leftHandDevices.Count == 1)
        {
            _LeftHandDevice = leftHandDevices[0];
            //Debug.Log(string.Format("Device name '{0}' with role '{1}'", device.name, device.role.ToString()));
            Debug.Log("Found left hand.");
        }
        else if (leftHandDevices.Count > 1)
        {
            Debug.Log("Found more than one left hand!");
        }
        if (rightHandDevices.Count == 1)
        {
            _RightHandDevice = rightHandDevices[0];
            Debug.Log("Found right hand.");
            //Debug.Log(string.Format("Device name '{0}' with role '{1}'", device.name, device.role.ToString()));
        }
        else if (rightHandDevices.Count > 1)
        {
            Debug.Log("Found more than one right hand!");
        }
    }
    //private void FixedUpdate()
    //{
        
    //}

    //private override void Update()
    //{
        
    //    _LeftHandDevice.TryGetFeatureValue(CommonUsages.trigger, out LeftTriggerAxis);
    //    _LeftHandDevice.TryGetFeatureValue(CommonUsages.deviceVelocity, out LeftHandVelocity);
    //}
    public override void OnRightTrigger()
    {
        
    }

    public override void OnLeftTrigger()
    {
        
    }

    public override void GetLeftControllerData()
    {
        _LeftHandDevice.TryGetFeatureValue(CommonUsages.trigger, out LeftTriggerAxis);
        _LeftHandDevice.TryGetFeatureValue(CommonUsages.deviceVelocity, out LeftHandVelocity);
        //_LeftHandDevice.TryGetFeatureValue(CommonUsages.deviceAcceleration, out LeftHandAcceleration);
    }

    public override void GetRightControllerData()
    {
        _RightHandDevice.TryGetFeatureValue(CommonUsages.trigger, out RightTriggerAxis);
        _RightHandDevice.TryGetFeatureValue(CommonUsages.deviceVelocity, out RightHandVelocity);
        //_RightHandDevice.TryGetFeatureValue(CommonUsages.deviceAcceleration, out RightHandAcceleration);
    }
}
