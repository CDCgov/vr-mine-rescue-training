using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollDeathBehavior : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //NPCController npc = animator.GetComponent<NPCController>();
        
        //if(npc != null)
        //{
        //    npc.Ragdoll(true);
        //}
    }

    //public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    NPCTriggerDeath death = animator.GetComponent<NPCTriggerDeath>();

    //    if (death != null)
    //    {
    //        death.Ragdoll(false);
    //    }
    //}
}
