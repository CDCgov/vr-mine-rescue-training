using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BAHDOL
{
    /// <summary>
    /// This class provides an example framework for how to trigger an animation parameter via a trigger collision
    /// </summary>
    public class TriggerAnimationExample : MonoBehaviour
    {

        private void OnTriggerEnter(Collider other)
        {
            Animator npcAnimator = other.gameObject.GetComponent<Animator>();

            if (npcAnimator != null)
            {
                {
                    npcAnimator.SetBool("crouching", true);
                }
            }
        }


        public void OnTriggerExit(Collider other)
        {
            Animator npcAnimator = other.gameObject.GetComponent<Animator>();

            if (npcAnimator != null)
            {
                npcAnimator.SetBool("crouching", false);
            }
        }
    }

}