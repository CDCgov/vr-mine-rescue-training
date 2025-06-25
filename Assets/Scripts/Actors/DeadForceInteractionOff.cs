using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadForceInteractionOff : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //CustomXRInteractable xr; 
        //if(animator.TryGetComponent<CustomXRInteractable>(out xr))
        //{
        //    xr.enabled = false;
        //}

        //Debug.Log($"AnimationState: Dead");

        //if (animator.TryGetComponent<NPCController>(out var npc))
        //{
        //    npc.IsAlive = false;
        //}
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //CustomXRInteractable xr;
        //if (animator.TryGetComponent<CustomXRInteractable>(out xr))
        //{
        //    xr.enabled = true;
        //}

        //Debug.Log($"AnimationState: Alive");

        //if (animator.TryGetComponent<NPCController>(out var npc))
        //{
        //    npc.IsAlive = true;
        //}
    }
}
