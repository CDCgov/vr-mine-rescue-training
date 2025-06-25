using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimIKTest : MonoBehaviour
{
    public Transform Target;
    public AvatarIKGoal IKGoal;

    Animator _animator;
    
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void OnAnimatorIK()
    {
        if (_animator != null)
        {
            _animator.SetLookAtPosition(Target.transform.position);
            _animator.SetLookAtWeight(1);

            _animator.SetIKPosition(IKGoal, Target.transform.position);
            _animator.SetIKPositionWeight(IKGoal, 1.0f);
        }
        else
        {
            Debug.LogError("No Animator Present!");
            gameObject.SetActive(false);
        }
    }
}
