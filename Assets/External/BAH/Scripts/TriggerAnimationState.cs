using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BAHDOL
{
    public class TriggerAnimationState : MonoBehaviour
    {

        public FollowCharacter CurrentFollowCharacter;
        public AnimationState animationToTrigger;

        [SerializeField] private bool shouldSetAcceleration;
        public float accelerationToSet;

        [SerializeField] private bool shouldSetHeight;
        public float heightToSet;

        private void OnTriggerEnter(Collider other)
        {

            FollowCharacter followCharacter = other.gameObject.GetComponent<FollowCharacter>();

            
            if (followCharacter)
            {
                CurrentFollowCharacter = followCharacter;

                if (shouldSetHeight)
                {
                    followCharacter.navAgent.baseOffset = heightToSet;
                }

                if (followCharacter != null)
                {
                    Debug.Log("triggered and animator found");
                    //followCharacter.navAgent.enabled = false;
                    if (followCharacter.navAgent.isOnNavMesh)
                        followCharacter.navAgent.isStopped = true;

                    followCharacter.triggeredByField = true;
                    //followCharacter.npcAnimator.JumpToAnimationState(animationToTrigger);
                    followCharacter.npcAnimator.TransitionToAnimation(animationToTrigger);
                }
                if (shouldSetAcceleration)
                {
                    followCharacter.navAgent.acceleration = accelerationToSet;
                }
            }
            else
            {
                //Debug.Log("Nav mesh was not found");
            }
        }

        
    }
}