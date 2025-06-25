using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRInputManager : SceneManagerBase
{
    public static XRInputManager GetDefault(GameObject self)
    {
        var manager = self.GetDefaultManager<XRInputManager>("XRInputManager", true);
        //DontDestroyOnLoad(manager.gameObject);

        return manager;
    }

    public class InputDeviceState
    {
        public InputDevice Device;

        public event Action<bool> IsTrackedChanged;
        //public Action<Vector2> PrimaryAxisChanged;
        public event Action<bool> PrimaryAxisClicked;
        //public event Action<float> TriggerChanged;
        public event Action<bool> TriggerPressed;
        //public event Action<float> GripChanged;
        public event Action<bool> GripPressed;
        //public Action<float> ThumbTouchChanged;
        public event Action<bool> PrimaryButtonClicked;
        public event Action<bool> SecondaryButtonClicked;
        //public Action<float> BatteryLevelChanged;

        public bool IsTracked
        {
            get { return _isTracked; }
            private set 
            {
                if (_isTracked != value)
                {
                    IsTrackedChanged?.Invoke(value);
                    _isTracked = value;
                }
            }
        }

        public Vector2 PrimaryAxis = Vector2.zero;        
        public bool PrimaryAxisClickValue
        {
            get { return _primaryAxisClickValue; }
            private set
            {
                if (_primaryAxisClickValue != value)
                {
                    
                    PrimaryAxisClicked?.Invoke(value);
                    _primaryAxisClickValue = value;
                }
            }
        }

        public float Trigger = 0;
        public float Grip = 0;

        public bool TriggerBool
        {
            get { return _triggerBool; }
            private set
            {
                if (_triggerBool != value)
                {
                    //if (value)
                    //    Debug.Log($"Trigger pressed {Device.name}");
                    TriggerPressed?.Invoke(value);
                    _triggerBool = value;
                }
            }
        }

        public bool GripBool
        {
            get { return _gripBool; }
            private set
            {
                if (_gripBool != value)
                {
                    GripPressed?.Invoke(value);
                    _gripBool = value;
                }
            }
        }

        public bool PrimaryButton
        {
            get { return _primaryButton; }
            private set
            {
                if (_primaryButton != value)
                {
                    PrimaryButtonClicked?.Invoke(value);
                    _primaryButton = value;
                }
            }
        }

        public bool SecondaryButton
        {
            get { return _secondaryButton; }
            private set
            {
                if (_secondaryButton != value)
                {
                    SecondaryButtonClicked?.Invoke(value);
                    _secondaryButton = value;
                }
            }
        }
        public float BatteryLevel = 0;
        public Vector3 Velocity = Vector3.zero;
        public Vector3 AngularVelocity = Vector3.zero;

        private bool _isTracked = false;
        //private Vector2 _primaryAxis = Vector2.zero;
        private bool _primaryAxisClickValue = false;
        //private float _trigger = 0;
        //private float _grip = 0;
        private bool _triggerBool;
        private bool _gripBool;
        private bool _primaryButton = false;
        private bool _secondaryButton = false;
        //private float _batteryLevel = 0;

        public void UpdateDevice(InputDevice device)
        {
            Vector2 vec2Val = Vector2.zero;
            float floatVal = 0;
            bool boolVal = false;

            if (device.TryGetFeatureValue(CommonUsages.isTracked, out boolVal))
                IsTracked = boolVal;
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out vec2Val))
                PrimaryAxis = vec2Val;
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out boolVal))
                PrimaryAxisClickValue = boolVal;
            if (device.TryGetFeatureValue(CommonUsages.trigger, out floatVal))
                Trigger = floatVal;
            if (device.TryGetFeatureValue(CommonUsages.grip, out floatVal))
                Grip = floatVal;
            if (device.TryGetFeatureValue(CommonUsages.primaryButton, out boolVal))
                PrimaryButton = boolVal;
            if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out boolVal))
                SecondaryButton = boolVal;
            if (device.TryGetFeatureValue(CommonUsages.batteryLevel, out floatVal))
                BatteryLevel = floatVal;

            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out boolVal))
                TriggerBool = boolVal;
            if (device.TryGetFeatureValue(CommonUsages.gripButton, out boolVal))
                GripBool = boolVal;

            device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Velocity);
            device.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out AngularVelocity);
        }
    }

    public InputDeviceState GetInputDevice(XRNode node)
    {
        if (_inputDevices == null)
            return null;

        InputDeviceState state;
        if (!_inputDevices.TryGetValue(node, out state))
        {
            state = new InputDeviceState();
            _inputDevices[node] = state;
        }

        return state;
        //if (!_inputDevices.TryGetValue(node, out var device))
        //    return null;

        //return device;
    }

    private Dictionary<XRNode, InputDeviceState> _inputDevices;

    private void Awake()
    {
        _inputDevices = new Dictionary<XRNode, InputDeviceState>();
    }

    private void Start()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
        InputDevices.deviceDisconnected += OnDeviceDisconnected;

        Util.DontDestroyOnLoad(gameObject);

        InitializeInputDevices();
    }

    private void OnDestroy()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
        InputDevices.deviceDisconnected -= OnDeviceDisconnected;
    }
    private void OnDeviceDisconnected(InputDevice obj)
    {
        InitializeInputDevices();
    }

    private void OnDeviceConnected(InputDevice obj)
    {
        InitializeInputDevices();
    }

    private void InitializeInputDevices()
    {
        var leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        var rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        //_inputDevices[XRNode.LeftHand] = leftHand;
        //_inputDevices[XRNode.RightHand] = rightHand;
        SetInputDevice(XRNode.LeftHand, leftHand);
        SetInputDevice(XRNode.RightHand, rightHand);
    }

    private void SetInputDevice(XRNode node, InputDevice device)
    {
        InputDeviceState state;
        if (!_inputDevices.TryGetValue(node, out state))
            state = new InputDeviceState();

        state.Device = device;

        _inputDevices[node] = state;
    }


    // Update is called once per frame
    void Update()
    {
        foreach (var kvp in _inputDevices)
        {
            ProcessInputDevice(kvp.Key, kvp.Value);
        }
    }

    private void ProcessInputDevice(XRNode node, InputDeviceState state)
    {
        if (state == null)
            return;

        state.UpdateDevice(state.Device);
    }
}
