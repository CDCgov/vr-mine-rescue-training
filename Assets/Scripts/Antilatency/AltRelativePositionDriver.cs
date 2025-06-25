using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class AltRelativePositionDriver : MonoBehaviour
{
    public AltPoseDriver ReferenceDriver;
    public XRNode InputDeviceNode;
    public Transform HeadTransform;

    private InputDevice _inputDevice;
    private Vector3 _cachedPosition;
    private Quaternion _cachedRotation;

    private Vector3 _lastPositionRel;
    private Quaternion _lastRotationRel;

    // Start is called before the first frame update
    void Start()
    {
        _cachedPosition = transform.localPosition;
        _cachedRotation = Quaternion.identity;


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

        Vector3 nativePosition;
        Quaternion nativeRotation;

        Vector3 pos = transform.localPosition;
        Quaternion rot = Quaternion.identity;
        bool isTracking = false;
        if(_inputDevice.TryGetFeatureValue(CommonUsages.isTracked, out isTracking))
        {
            if (_inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out nativePosition))
            {
                pos = nativePosition;
                _cachedPosition = pos;
            }


            if (_inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out nativeRotation))
            {
                rot = nativeRotation;
                _cachedRotation = rot;
            }
        }

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
