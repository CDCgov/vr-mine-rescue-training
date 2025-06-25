using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltBehavior : MonoBehaviour
{
    public Transform HeadTransform;
    public Transform LeftHandTransform;
    public Transform RightHandTransform;
    public float BeltOffset = -0.55f;
    public float VisorZOffset = 0.05f;

    public bool HideBeltWhenHeadUp = false;
    public GameObject BeltModel;

    private float[] _rotationAverage;
    private int _avgIndex = 0;
    private float _sum = 0;

    private void Start()
    {
        _rotationAverage = new float[60];
        for(int i=0; i < 60; i++)
        {
            _rotationAverage[i] = 0;
        }
    }

    void Update()
    {
        if(HeadTransform == null || LeftHandTransform == null || RightHandTransform == null)
        {
            return;
        }
        Vector3 headAdjust = HeadTransform.localPosition;
        Vector3 headRotation = HeadTransform.localEulerAngles;
        Vector3 beltRotation = transform.localEulerAngles;

        //var headVector = HeadTransform.up;
        //if (Vector3.Dot(headVector, Vector3.up) > 0.9f)
        //    headVector = HeadTransform.forward;


        Vector3 headVector = new Vector3(0, 1, 1);
        headVector = HeadTransform.TransformDirection(headVector);
        if (Vector3.Dot(headVector, Vector3.up) > 0.95f)
            headVector = HeadTransform.forward;

        Vector3 dir = Vector3.ProjectOnPlane(headVector, Vector3.up);
        dir = dir.normalized;

        transform.forward = dir;

        //var euler = HeadTransform.eulerAngles;
        //euler.x = 0;
        //euler.z = 0;
        //transform.rotation = Quaternion.Euler(euler);

        headAdjust.y = headAdjust.y + BeltOffset;
        if (headAdjust.y < 0)
        {
            headAdjust.y = 0;
        }

        //transform.localPosition = headAdjust + new Vector3(0, 0, VisorZOffset);
        transform.localPosition = headAdjust;
        transform.Translate(0, 0, VisorZOffset, Space.Self);
    }
}
