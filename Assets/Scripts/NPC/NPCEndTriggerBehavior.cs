using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCEndTriggerBehavior : MonoBehaviour
{
    public Transform[] GoToPositions;
    public Animator CustomEndAnimation;
    

    private int _goToIndex = 0;
    
    private List<BAHDOL.FollowCharacter> _followCharacters;

    private void Start()
    {
        _followCharacters = new List<BAHDOL.FollowCharacter>();
    }
    // Update is called once per frame
    void Update()
    {
        if(_followCharacters.Count > 0)
        {
            for(int i = 0; i < _followCharacters.Count; i++)
            {
                if (_followCharacters[i].shouldFollowTarget)
                {
                    if(Vector3.Distance(_followCharacters[i].transform.position, GoToPositions[i].position) < 1)
                    {
                        _followCharacters[i].shouldFollowTarget = false;
                        ActivateAnimation(_followCharacters[i].gameObject);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        BAHDOL.FollowCharacter followCharacter = other.GetComponent<BAHDOL.FollowCharacter>();
        if(followCharacter != null)
        {            
            followCharacter.distanceToKeep = 0;
            NavMeshAgent navAgent = followCharacter.GetComponent<NavMeshAgent>();
            if(navAgent != null)
            {
                navAgent.stoppingDistance = 0;
            }
            if (_goToIndex < GoToPositions.Length)
            {
                _followCharacters.Add(followCharacter);
                followCharacter.characterToFollow = GoToPositions[_goToIndex];
                _goToIndex++;
            }
            else
            {
                followCharacter.shouldFollowTarget = false;
                ActivateAnimation(followCharacter.gameObject);
            }
        }
    }

    private void ActivateAnimation(GameObject go)
    {
        NavMeshAgent agent = go.GetComponent<NavMeshAgent>();
        if(agent != null)
        {
            agent.enabled = false;
        }
        BAHDOL.NPC_Animator bahAnim = go.GetComponent<BAHDOL.NPC_Animator>();
        if(bahAnim != null)
        {
            bahAnim.enabled = false;
        }
        Animator animator = go.GetComponent<Animator>();
        animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Dance");
    }
}
