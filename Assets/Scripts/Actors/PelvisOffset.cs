using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelvisOffset : MonoBehaviour
{
    public Transform HeadTransform;
    public Transform LeftHandTransform;
    public Transform RightHandTransform;
    public float BeltOffset = -0.55f;
    public float VisorZOffset = -0.05f;
    public float PelvisCrouchModifier = 0.5f;

    //public MinerFinalIK _minerFinalIK;
    public RootMotion.FinalIK.VRIK VRIKRef;

    private float _positionWeight;
    private float _rotationWeight;
    // Start is called before the first frame update
    private void Start()
    {
        
        if(VRIKRef == null)
        {
            VRIKRef = GetComponentInParent<RootMotion.FinalIK.VRIK>();
        }
        HeadTransform = VRIKRef.solver.spine.headTarget;
        _positionWeight = VRIKRef.solver.spine.pelvisPositionWeight;
        _rotationWeight = VRIKRef.solver.spine.pelvisRotationWeight;
    }
    // Update is called once per frame
    void Update()
    {
        if (HeadTransform == null || LeftHandTransform == null || RightHandTransform == null)
        {
            HeadTransform = VRIKRef.solver.spine.headTarget;
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

        if(HeadTransform.parent.localPosition.y < PelvisCrouchModifier)
        {
            float t = (HeadTransform.parent.localPosition.y / PelvisCrouchModifier) - 1;
            VRIKRef.solver.spine.pelvisPositionWeight = Mathf.Lerp(0, _positionWeight, t);
        }
    }
}
