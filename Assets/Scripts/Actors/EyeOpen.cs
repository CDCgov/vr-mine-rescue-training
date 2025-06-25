using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeOpen : StateMachineBehaviour
{
    private EyeLidControl _eyeLidControl;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_eyeLidControl == null)
        {
            _eyeLidControl = animator.GetComponentInChildren<EyeLidControl>();
        }

        if (_eyeLidControl != null)
        {
            _eyeLidControl.EyeState = EyeState.Open;
        }
    }
}
