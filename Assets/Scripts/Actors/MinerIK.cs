using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerIK : MonoBehaviour
{
    public Transform HeadPosition;
    public Vector3 LookPosition;
    public Transform RightHandTarget;
    public Transform LeftHandTarget;
    public float PositionCorrectionAmount = 0.1f; //Meters;

    public Animator _animator;

    private Quaternion _rightCorrectionRotation;
    private Quaternion _leftCorrectionRotation;

    private Vector3 _rightCorrectionPosition;
    private Vector3 _leftCorrectionPosition;

    void Start()
    {
        if(_animator == null)
            _animator = GetComponent<Animator>();
        if (_animator == null)
            Debug.LogError($"No animator present on {gameObject.name}!");

        _rightCorrectionRotation = Quaternion.AngleAxis(-90, Vector3.forward);
        _leftCorrectionRotation = Quaternion.AngleAxis(90, Vector3.forward);


    }

    void OnAnimatorIK()
    {
        if (_animator == null)
            return;

        //if (_animator != null)
        //{
        //if (LookDirection != null && HeadPosition != null)
        if (LookPosition != Vector3.zero)
        {
            //_animator.SetLookAtPosition(HeadPosition.transform.position + LookDirection);
            _animator.SetLookAtPosition(LookPosition);
            _animator.SetLookAtWeight(1);
        }
        else
        {
            _animator.SetLookAtWeight(0);
        }

        if (RightHandTarget != null)
        {
            _rightCorrectionPosition = RightHandTarget.transform.position + RightHandTarget.transform.right * PositionCorrectionAmount;//palm outward correction
            _rightCorrectionPosition = _rightCorrectionPosition + RightHandTarget.transform.forward * -0.07f;//this has to be done as the IK "hand" is more accurately described as the **root** of the hand...aka wrist
                                                                                                             //_animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandTarget.transform.position);
            _animator.SetIKPosition(AvatarIKGoal.RightHand, _rightCorrectionPosition);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);

            _animator.SetIKRotation(AvatarIKGoal.RightHand, RightHandTarget.transform.rotation * _rightCorrectionRotation);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);
        }

        if (LeftHandTarget != null)
        {
            _leftCorrectionPosition = LeftHandTarget.transform.position + LeftHandTarget.transform.right * PositionCorrectionAmount * -1;
            _leftCorrectionPosition = _leftCorrectionPosition + LeftHandTarget.transform.forward * -0.07f;

            //_animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandTarget.transform.position);
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftCorrectionPosition);
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);

            _animator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandTarget.transform.rotation * _leftCorrectionRotation);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
        }

        //}
        //else
        //{
        //	Debug.LogError("No Animator Present!");
        //	this.enabled = false;
        //}
    }
}
