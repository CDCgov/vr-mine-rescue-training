using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRHapticShake : MonoBehaviour
{
    private List<InputDevice> _xrDevices;
    private bool _allowShake = false;
    private bool _currentlyShaking = false;
    private float _delay = 1;
    // Start is called before the first frame update
    void Start()
    {
        _xrDevices = new List<InputDevice>();
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

    private void InputDevices_deviceConnected(InputDevice inputDevice)
    {
        _xrDevices.Add(inputDevice);
    }

    private void InputDevices_deviceDisconnected(InputDevice inputDevice)
    {
        _xrDevices.Remove(inputDevice);
    }

    // Update is called once per frame
    void Update()
    {
        if (_allowShake && (Time.time > _delay))
        {
            foreach(InputDevice device in _xrDevices)
            {
                device.SendHapticImpulse(0, 0.5f, 1);
            }
            _delay = Time.time + 1;
            _currentlyShaking = true;
        }
        else
        {
            if (_currentlyShaking)
            {
                foreach (InputDevice device in _xrDevices)
                {
                    device.StopHaptics();
                }
                _currentlyShaking = false;
            }
        }
    }

    public void EnableShake(bool shake)
    {
        _allowShake = shake;
    }
}
