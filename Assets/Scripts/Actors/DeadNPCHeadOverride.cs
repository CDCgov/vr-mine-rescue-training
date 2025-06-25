using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadNPCHeadOverride : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        DeadNPCHeadControl deadNPCHeadControl;

        if(animator.TryGetComponent<DeadNPCHeadControl>(out deadNPCHeadControl))
        {
            deadNPCHeadControl.PerformOverride = true;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        DeadNPCHeadControl deadNPCHeadControl;

        if (animator.TryGetComponent<DeadNPCHeadControl>(out deadNPCHeadControl))
        {
            deadNPCHeadControl.PerformOverride = false;
        }
    }
}
