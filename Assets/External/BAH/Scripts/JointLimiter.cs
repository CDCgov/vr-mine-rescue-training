using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BAHDOL
{
    public class JointLimiter : MonoBehaviour
    {
        public List<RootMotion.FinalIK.RotationLimit> rotationLimits;

        public RootMotion.FinalIK.VRIK vrik;

        private void Start()
        {
            //Disabling the update functionality so we can manually update it. 
            for (int i = 0; i < rotationLimits.Count; i++)
            {
                rotationLimits[i].enabled = false;

            }
            //We are subscribing to the AfterVRIK update loop
            vrik.solver.OnPostUpdate += AfterVRIK;
        }

        //For each FinalIK RotationLimit added to the array, we will update the script after the IK update loop
        private void AfterVRIK()
        {
            for (int i = 0; i < rotationLimits.Count; i++)
            {
                rotationLimits[i].Apply();
            }
        }
    }

}