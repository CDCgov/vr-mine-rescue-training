using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BAHDOL
{
    public class DoorWayHeadingAdjuster : MonoBehaviour
    {
        [SerializeField] private Transform waypoint;

        private void OnTriggerStay(Collider other)
        {
            if (other.GetComponent<NPC_Animator>() != null)
            {
                other.transform.rotation = Quaternion.RotateTowards(other.transform.rotation, waypoint.rotation, 1f);
            }
        }
    }
}
