using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BAHDOL
{
    /// <summary>
    /// FollowCharacter.cs handles what animation states should be firing, 
    /// as well as when the character should be following the player
    /// </summary>
    [RequireComponent(typeof(NPC_Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class FollowCharacter : MonoBehaviour
    {
        [Tooltip("Nav mesh agent follow target.  ")]
        public Transform characterToFollow;

        //Self Contained variables
        [Tooltip("Nav mesh agent for navigation ")]
        public NavMeshAgent navAgent;

        [Tooltip("The npc animator used for controlling the AI's animation ")]
        public NPC_Animator npcAnimator;

        [Tooltip("Follow Distance fir characterToFollow. Please account for the targets and AI's radius ")]
        public float distanceToKeep = 3.5f;

        [Tooltip("if true, will allow npc to follow target ")]
        public bool shouldFollowTarget;


        //public Vector3 WarpOffset;

        [Tooltip("used for communicating to the animator that the animation was or was not triggered but the environment like a trigger")]
        public bool triggeredByField = false;
        public bool HasAuthority = false;

        public Animator UnityAnimator;
        private NavMeshHit hit = new NavMeshHit();

        private Vector3 _currentDestination;
        private float _lastDestUpdate = -1;

        #region Built-In Functions
        /// <summary>
        /// Auto assign the different components if they are not previously assigned
        /// </summary>
        private void Awake()
        {
            if (navAgent == null)
            {
                navAgent = gameObject.GetComponent<NavMeshAgent>();
            }

            if (npcAnimator == null)
            {
                npcAnimator = gameObject.GetComponent<NPC_Animator>();
            }

            _currentDestination = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        }

        /// <summary>
        /// Registers the NPCs to the NPC Manager.
        /// Registers each NPC's listeners to the ToggleFollow event
        /// </summary>
        void Start()
        {
            if (NPCManager.Instance != null)
            {
                NPCManager.Instance.RegisterNPC(this);
                NPCManager.Instance.followToggle.AddListener(ToggleFollow);
            }

            navAgent.stoppingDistance = distanceToKeep;
        }


        // Update is called once per frame
        void Update()
        {
            Follow();
            HandleAnimations();
        }

        void OnDrawGizmos()
        {
            CheckIfConnectedNavmesh();
        }

        #endregion

        #region Custom Functions
        /// <summary>
        /// Created to clean up the update loop, 
        /// Tells the NPC to follow a specific transform
        /// </summary>
        public void Follow()
        {
            if (!shouldFollowTarget)
                return;

            if (characterToFollow != null && npcAnimator.GetWalkingOrIdle())
            {
                var dest = characterToFollow.position;

                float elapsed = Time.time - _lastDestUpdate;

                if (navAgent.isOnNavMesh && elapsed > 1.0f)
                {
                    var destChangeDist = Vector3.Distance(dest, _currentDestination);

                    //repeated calls to SetDestination cause the animation/pathing to glitch
                    //only call if the destination has really changed
                    if (destChangeDist > 0.25f)
                    {
                        navAgent.SetDestination(dest);
                        _currentDestination = dest;
                        _lastDestUpdate = Time.time;
                    }
                }

                if(Vector3.Distance(dest, transform.position) > 7.5)
                {
                    Vector3 dir = (transform.position - characterToFollow.position).normalized;
                    Vector3 telepos = dest + 2 * (dir);
                    navAgent.Warp(telepos);
                }
                if ((navAgent.remainingDistance < navAgent.stoppingDistance))
                {
                    navAgent.updateRotation = false;
                    FaceTarget(characterToFollow.position);
                }
                else
                {
                    navAgent.updateRotation = true;
                }
            }

        }

        private void FaceTarget(Vector3 destination)
        {
            Vector3 lookPos = destination - transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.1f);
            
        }

        /// <summary>
        /// Monitors and controls animation states fro the following of the character
        /// by controlling the parameters in its animation tree
        /// </summary>
        public void HandleAnimations()
        {
            //if (navAgent.isOnNavMesh)
            //{
            //    navAgent.SamplePathPosition(NavMesh.AllAreas, 0.0f, out hit);
            //}


            //float forward = transform.InverseTransformDirection(navAgent.velocity.normalized).z;
            //float lateral = transform.InverseTransformDirection(navAgent.velocity.normalized).x;

            var velocity = transform.InverseTransformDirection(navAgent.velocity.normalized);
            var forward = velocity.z;
            var lateral = velocity.x;

            npcAnimator.SetVelocity(forward, lateral);

            //if (hit.mask == 8)
            //{

            //}
            //else
            //{

            //}
        }

        public void StopFollow()
        {
            shouldFollowTarget = false;
            navAgent.isStopped = true;
        }

        public void StartFollow()
        {
            shouldFollowTarget = true;
            navAgent.isStopped = false;
            _lastDestUpdate = -1;
        }

        public void ForceEnableMovement()
        {
            StopAllCoroutines();
            StartCoroutine(TryToConnectToNavMesh());
        }



        //If the mesh is disconnected, we'll try to reconnect it 
        IEnumerator TryToConnectToNavMesh()
        {
            int count = 0;
            while (count < 3 && !navAgent.isOnNavMesh)
            {
                count++;
                navAgent.enabled = false;
                navAgent.enabled = true;
                navAgent.isStopped = false;

                yield return new WaitForSecondsRealtime(0.33f);
            }
        }

        //Here we are debugging to check if the AI is connected to the navmesh.
        //We have had issues were the AI would get disconnected for some reason. This is a great indicator and useful for debugging.
        public void CheckIfConnectedNavmesh()
        {
            if (navAgent.isOnNavMesh)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position, .05f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position, .05f);
            }
        }



        public void CheckIfInactive()
        {

        }

        //Used to to toggle the follow functionality in the AI navigation.
        public void ToggleFollow()
        {
            if (shouldFollowTarget)
            {
                StopFollow();
            }
            else
            {
                StartFollow();
            }
        }
        #endregion
    }
}
