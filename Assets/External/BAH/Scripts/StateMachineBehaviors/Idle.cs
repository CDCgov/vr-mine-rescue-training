using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace BAHDOL.StateMachine
{

    public class Idle : StateMachineBehaviour
    {
        private NavMeshAgent _agent;
        private FollowCharacter _follow;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            _agent = animator.gameObject.GetComponent<NavMeshAgent>();
            _follow = animator.gameObject.GetComponent<FollowCharacter>();
            if (_agent != null)
            {
                //FollowCharacter followCharacter = animator.gameObject.GetComponent<FollowCharacter>();

                if (animator.GetBool("InteriorExit") == false && animator.GetBool("ExteriorExit") == false)
                {
                    //if (!animator.GetBool("StandUp"))
                    {
                        Debug.Log("re-enable agent");
                        animator.SetFloat("Forward", 0.0f);
                        animator.SetFloat("Lateral", 0.0f);
                        //if (_agent.enabled && _follow != null && _follow.shouldFollowTarget && _follow.characterToFollow != null)
                        if (_agent.enabled)
                        {
                            _follow.ForceEnableMovement();
                        }

                    }
                }
            }

        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
            //if (_agent != null && _follow != null)
            //{
            //    _agent.enabled = false;
            //    _follow.enabled = false;
            //}
            

        }

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
