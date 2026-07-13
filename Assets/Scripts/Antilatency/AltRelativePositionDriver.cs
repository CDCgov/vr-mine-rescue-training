using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using Unity.XR.OpenVR;

public class AltRelativePositionDriver : MonoBehaviour
{
    public AltPoseDriver ReferenceDriver;
    public XRNode InputDeviceNode;
    public Transform HeadTransform;

    public InputActionReference ControllerPose;

    private UnityEngine.XR.InputDevice _inputDevice;
    private Vector3 _cachedPosition;
    private Quaternion _cachedRotation;

    private Vector3 _lastPositionRel;
    private Quaternion _lastRotationRel;

    private XRController _xrController;
    private Pose _pose;

    private InputAction _actionControllerPose;

    // Start is called before the first frame update
    void Start()
    {
        _cachedPosition = transform.localPosition;
        _cachedRotation = Quaternion.identity;

        //FindXRController();

        _actionControllerPose = ControllerPose.action;
    }

    //private void FindXRController()
    //{
    //    switch (InputDeviceNode)
    //    {
    //        case XRNode.LeftHand:
    //            _xrController = XRController.leftHand;
    //            break;

    //        case XRNode.RightHand:
    //            _xrController = XRController.rightHand;
    //            break;
    //    }

    //}

    private void UpdateDevicePosition(out Vector3 pos, out Quaternion rot, out bool isTracking )
    {
        pos = transform.localPosition;
        rot = Quaternion.identity;
        isTracking = false;

        if (!_inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out isTracking))
        {
            return;
        }

        //FindXRController();

        if (_actionControllerPose != null)
        {
            var pose = _actionControllerPose.ReadValue<PoseState>();
            pos = pose.position;
            rot = pose.rotation;
        }
        else if (_xrController != null)
        {
            pos = _xrController.devicePosition.ReadValue();
            rot = _xrController.deviceRotation.ReadValue();
        }
        else
        {
            Vector3 nativePosition;
            Quaternion nativeRotation;

            if (_inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out nativePosition))
            {
                pos = nativePosition;
            }


            if (_inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out nativeRotation))
            {
                rot = nativeRotation;
            }
        }

        _cachedPosition = pos;
        _cachedRotation = rot;
    }

    void UpdateTracking()
    {
        //keep trying to find the specified input device node
        if (!_inputDevice.isValid)
        {
            //Debug.LogError($"Couldn't initialize input device for node {InputDeviceNode.ToString()}");
            _inputDevice = InputDevices.GetDeviceAtXRNode(InputDeviceNode);
            if (!_inputDevice.isValid)
                return;
        }


        //Vector3 pos = transform.localPosition;
        //Quaternion rot = Quaternion.identity;
        //bool isTracking = false;
        //if(_inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out isTracking))
        //{
        //    UpdateDevicePosition(out pos, out rot);
        //}
        UpdateDevicePosition(out Vector3 pos, out Quaternion rot, out bool isTracking);

        //if (ReferenceDriver != null)
        //{
        //    //pos += ReferenceDriver.Offset;
        //    //rot = ReferenceDriver.RotOffset * nativeRotation;
        //    ReferenceDriver.OculusToAlt(ref pos, ref rot);
        //}

        if (isTracking)
        {
            transform.localPosition = pos;
            transform.localRotation = rot;

            if (HeadTransform != null)
            {
                //update the last valid position in the space of the head transform
                _lastPositionRel = HeadTransform.InverseTransformPoint(transform.position);
                _lastRotationRel = Quaternion.Inverse(HeadTransform.localRotation) * rot;

                //_lastRotationRel = rot * HeadTransform.localRotation;
                // HeadTransform.localRotation * rot;
            }
        }
        else
        {
            if (HeadTransform != null)
            {
                //transform the last valid position back from the coordinate space of the head tranform
                var worldSpace = HeadTransform.TransformPoint(_lastPositionRel);
                transform.position = worldSpace;// + (Random.insideUnitSphere * 0.03f);

                transform.localRotation = HeadTransform.localRotation * _lastRotationRel;
            }
        }


    }

    void OnBeforeRender()
    {
        UpdateTracking();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTracking();
    }
}
