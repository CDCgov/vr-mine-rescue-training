using BAHDOL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StateEnablesFollow : StateMachineBehaviour
{
    private FollowCharacter _follow;
    private NavMeshAgent _agent;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_follow == null)
            _follow = animator.GetComponent<FollowCharacter>();
        if(_agent == null)
            _agent = animator.GetComponent<NavMeshAgent>();
        if (_follow != null && _agent != null)
        {
            //_follow.enabled = true;
            _follow.shouldFollowTarget = true;
            _agent.enabled = true;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_follow == null)
            _follow = animator.GetComponent<FollowCharacter>();
        if (_agent == null)
            _agent = animator.GetComponent<NavMeshAgent>();
        if (_follow != null && _agent != null)
        {
            _follow.shouldFollowTarget = false;
            //_follow.enabled = false;
            _agent.enabled = false;
        }
    }
}
