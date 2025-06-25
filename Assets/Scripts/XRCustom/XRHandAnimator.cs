using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class XRHandAnimator : MonoBehaviour
{
    public XRInputManager XRInputManager = null;
    public UnityEngine.XR.XRNode XRDeviceNode;

    private Animator _animator;
    private XRInputManager.InputDeviceState _deviceState;

    private int _triggerID = -1;
    private int _gripID = -1;

    void Start()
    {
        if (XRInputManager == null)
            XRInputManager = XRInputManager.GetDefault(gameObject);

        _animator = GetComponent<Animator>();

        _deviceState = XRInputManager.GetInputDevice(XRDeviceNode);
        _triggerID = Animator.StringToHash("Trigger");
        _gripID = Animator.StringToHash("Gripped");
    }

    // Update is called once per frame
    void Update()
    {
        if (_deviceState == null || _animator == null)
            return;

        if (_deviceState.Grip > 0.5f)
            _animator.SetBool(_gripID, true);
        else
            _animator.SetBool(_gripID, false);

        if (_deviceState.Trigger > 0.5f)
            _animator.SetBool(_triggerID, true);
        else
            _animator.SetBool(_triggerID, false);
    }
}
