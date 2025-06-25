using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BAHDOL.StateMachine
{

    /// <summary>
    /// As an addition to the crouching system, additional capability had to be added to this animation state
    /// By halting the AI when they are transitioning, it keeps characters from sliding along the ground
    /// </summary>
    public class CrouchToStand : StateMachineBehaviour
    {
        //private FollowCharacter followCharacter;
        //// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    followCharacter = animator.gameObject.GetComponent<FollowCharacter>();
        //    if (followCharacter != null)
        //    {
        //        followCharacter.StopFollow();
        //    }
        //}

        //// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        ////override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        ////{
        ////    
        ////}

        //// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    if (followCharacter != null)
        //    {
        //        followCharacter.StartFollow();
        //    }
        //}

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }

}