using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using UnityEditor.Animations;

namespace BAHDOL
{

    public enum AnimationState { Walking, Running, Exit_Refuge, Pick_Up_Item, Crouch_Idle, Crouch_Walk, Idle, Seated, Seated_To_Standing, Refuge_Exit_Interior, Refuge_Exit_Exterior, Dizzy, Sitting_On_Ground, Dying, Dance, Unconscious, Dead,NONE };
    public enum TurnDirection { Left, Right, Neither };
    /// <summary>
    /// This class manages the animator and provides helper functions to transition between the various animation states 
    /// </summary>
    public class NPC_Animator : MonoBehaviour
    {
        [Tooltip("Reference to animator")]
        [SerializeField]
        private Animator animator = null;

        [SerializeField]
        AnimationState selectedAnimation = AnimationState.Idle;

        [Tooltip("This should default to Idle in most cases. However for NPC beginning in the refuge, this should default to 'Seated'")]
        [SerializeField]
        public AnimationState stateToStartIn = AnimationState.Idle;

        [Tooltip(" Used to determine the direction animator will turn")]
        public TurnDirection turnDirection = TurnDirection.Left;

        #region Built-In Functions

        private void Awake()
        {
            if (animator == null)
            {
                gameObject.GetComponent<Animator>();
            }
        }

        private void Start()
        {
            SetTurnDirection();
            JumpToAnimationState(stateToStartIn);
        }
        #endregion
        #region Custom Functions

        /// <summary>
        /// This function automatically begins to play the selected animation state
        /// This does not take into account transitions between animations
        /// Best used for Debug or animation feedback
        /// </summary>
        /// <param name="animationToPlay"></param>
        public void JumpToAnimationState(AnimationState animationToPlay)
        {
            //Debug.Log(animationToPlay.ToString());
            //if(animationToPlay == AnimationState.Crouch_Idle) 
            //{                
            //    animator.SetBool("crouching", true);
            //}
            //TransitionToAnimation(animationToPlay);
            ClearParameters();
            animator.Play(animationToPlay.ToString(), 0, 0);
        }

        public void ClearParameters()
        {
            foreach (var param in animator.parameters)
            {
                if(param.name == "TurnLeft" || param.name == "TurnRight")
                {
                    continue;
                }
                animator.SetBool(param.name, false);
            }
        }

        /// <summary>
        /// EasyButton inspector version for playing animations
        /// </summary>
        //[EasyButtons.Button]
        public void PlaySelectedAnimation()
        {            
            JumpToAnimationState(selectedAnimation);
        }

        /// <summary>
        /// This function is good for testing and activating natural transitions between the states of
        /// a mechanim tree. This toggles the different parameters.
        /// </summary>
        //[EasyButtons.Button]
        public void TransitionToAnimationState()
        {
            TransitionToAnimation(selectedAnimation);
        }

        private void SetTurnDirection()
        {
            if (turnDirection == TurnDirection.Left)
            {
                animator.SetBool("TurnLeft", true);
                animator.SetBool("TurnRight", false);
            }
            else if(turnDirection == TurnDirection.Right)
            {
                animator.SetBool("TurnLeft", false);
                animator.SetBool("TurnRight", true);
            }
            else
            {
                animator.SetBool("TurnLeft", false);
                animator.SetBool("TurnRight", false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationToPlay"></param>
        public void TransitionToAnimation(AnimationState animationToPlay)
        {
            //animator.SetBool("StandUp", true);
            //animator.SetBool("Seated", false);
            //animator.SetBool("SitOnGround", false);
            //animator.SetBool("crouching", false);
            ClearParameters();
            //TODO: Add a system wide bool flush to false and then just set the proper ones true
            switch (animationToPlay)
            {
                case AnimationState.Idle:
                    {
                        //animator.SetBool("walking", false);
                        //animator.SetBool("atDestination", true);
                        //animator.SetBool("running", false);
                        //animator.SetBool("crouching", false);
                        animator.SetTrigger("StandUp");
                        break;
                    }
                case AnimationState.Walking:
                    {
                        //animator.SetBool("walking", true);
                        //animator.SetBool("atDestination", false);
                        //animator.SetBool("running", false);
                        //animator.SetBool("crouching", false);
                        animator.SetTrigger("StandUp");
                        break;
                    }
                case AnimationState.Running:
                    {
                        //animator.SetBool("walking", false);
                        //animator.SetBool("atDestination", false);
                        //animator.SetBool("running", true);
                        //animator.SetBool("crouching", false);
                        animator.SetTrigger("StandUp");
                        break;
                    }
                case AnimationState.Crouch_Idle:
                    {
                        //animator.SetBool("walking", false);
                        animator.SetBool("atDestination", true);
                        //animator.SetBool("running", false);
                        animator.SetBool("crouching", true);
                        break;
                    }
                case AnimationState.Crouch_Walk:
                    {
                        animator.SetBool("walking", true);
                        //animator.SetBool("atDestination", false);
                        //animator.SetBool("running", false);
                        animator.SetBool("crouching", true);
                        break;
                    }
                case AnimationState.Refuge_Exit_Interior:
                    {
                        //animator.SetBool("walking", false);
                        //animator.SetBool("atDestination", false);
                        //animator.SetBool("running", false);
                        //animator.SetBool("crouching", false);
                        animator.SetTrigger("InteriorExit"); //This trigger is reset in the state script ExitRefuge
                        break;
                    }
                case AnimationState.Refuge_Exit_Exterior:
                    {
                        //animator.SetBool("walking", false);
                        //animator.SetBool("atDestination", false);
                        //animator.SetBool("running", false);
                        //animator.SetBool("crouching", false);
                        animator.SetTrigger("ExteriorExit"); //This trigger is reset in the state script ExitRefuge
                        break;
                    }
                case AnimationState.Seated:
                    //animator.SetBool("StandUp", false);
                    //animator.SetBool("atDestination", false);
                    //animator.SetBool("running", false);
                    //animator.SetBool("crouching", false);
                    //animator.SetBool("Seated", true);
                    animator.SetTrigger("Sit");
                    break;
                case AnimationState.Sitting_On_Ground:
                    //animator.SetBool("StandUp", false);
                    //animator.SetBool("atDestination", false);
                    //animator.SetBool("running", false);
                    //animator.SetBool("crouching", false);
                    animator.SetTrigger("SitOnGround");
                    
                    break;
                case AnimationState.Dying:
                    //animator.SetBool("StandUp", false);
                    //animator.SetBool("atDestination", false);
                    //animator.SetBool("running", false);
                    //animator.SetBool("crouching", false);
                    animator.SetBool("Death", true);
                    break;
                case AnimationState.Dizzy:
                    animator.SetTrigger("Dizzy");
                    break;
                case AnimationState.Unconscious:
                    animator.SetTrigger("Unconscious");
                    break;
                case AnimationState.NONE:
                    {
                        Debug.Log("No Animation State Selected", this);
                        break;
                    }

                default:
                    break;

            }
        }

        public void SetVelocity(float forward, float lateral)
        {
            animator.SetFloat("Forward", forward, 0.3f, Time.deltaTime);
            animator.SetFloat("Lateral", lateral, 0.3f, Time.deltaTime);
        }

        public void SetVertical(float vertical)
        {
            animator.SetFloat("Vertical", vertical);
        }

        #endregion

        public bool GetWalkingOrIdle()
        {
            bool walking = animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
            return walking;//animator.GetBool("walking") || animator.GetBool("atDestination") || 
        }

        public bool IsInAnimationState(string stateToCheck)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(stateToCheck);
        }
    }
}
