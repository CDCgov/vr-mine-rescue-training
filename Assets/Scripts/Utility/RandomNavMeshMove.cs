using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RandomNavMeshMove : MonoBehaviour
{
    private NavMeshAgent _navAgent;

    private float _lastDestChangeTime;
    //private NPCTriggerDeath _npcTriggerDeath;
    private bool _allowMovement = true;
    private NPCController _npcController;

    // Start is called before the first frame update
    void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        TryGetComponent<NPCController>(out _npcController);
        //_npcTriggerDeath = GetComponent<NPCTriggerDeath>();
        //_npcTriggerDeath.OnDeath += _npcTriggerDeath_OnDeath;
    }

    //private void _npcTriggerDeath_OnDeath()
    //{
    //    Debug.Log($"Death triggered on NPC Move");
    //    _allowMovement = false;
    //}

    void MoveRandom() 
    {
        if (_npcController != null && !_npcController.CanMove)
            return;

        var pos = transform.position;
        var dir = Random.insideUnitSphere;
        dir.y = 0;

        pos += dir.normalized * 5.0f;

        if (!NavMesh.SamplePosition(pos, out var hit, 2.0f, NavMesh.AllAreas))
            return;

        _navAgent.destination = hit.position;
        _lastDestChangeTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (_navAgent.isOnNavMesh)
        {
            if ((_navAgent.remainingDistance < 0.6f || (Time.time - _lastDestChangeTime) > 5.0f) && _allowMovement)
                MoveRandom();
        }
    }
}
