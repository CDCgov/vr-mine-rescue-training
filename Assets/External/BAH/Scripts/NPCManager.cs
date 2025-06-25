using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This singleton manager provides a foundation to control, find, and manage all NPCs in the scene. 
/// This class contains the follow/stop event that can be triggered from the users controller
/// </summary>
/// 
namespace BAHDOL
{

    public class NPCManager : MonoBehaviour
    {
        public static NPCManager Instance;
        private List<FollowCharacter> npc_List = new List<FollowCharacter>();
        public UnityEvent followToggle = new UnityEvent();

        /// <summary>
        /// Allows the script to be accessed anywhere as a singleton
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Able to register all NPCs to the manager
        /// Potentially useful for future development
        /// </summary>
        /// <param name="character"></param>
        public void RegisterNPC(FollowCharacter character)
        {
            npc_List.Add(character);
        }

        /// <summary>
        /// Public event able to be invoked and subscribed to which will halt or resume all NPC following of the player
        /// </summary>
        //[EasyButtons.Button]
        public void ToggleFollowing()
        {
            followToggle.Invoke();
        }
    }
}
