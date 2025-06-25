using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState
{
    Unknown = 0,
    Alive,
    Unconscious,
    Dead
}

public class NPCAnimState : StateMachineBehaviour
{
    public NPCState State = NPCState.Alive;
    public bool CanMove = true;
    public bool Ragdoll = false;

    public bool CanInteractWith
    {
        get
        {
            if (State == NPCState.Alive || State == NPCState.Unconscious)
                return true;
            else
                return false;
        }
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent<NPCController>(out var npc))
        {
            npc.SetAnimStateData(this, stateInfo);
        }

        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    public override string ToString()
    {
        return string.Format("state: {0}, move: {1}, ragdoll:{2}",
            this.State.ToString(), CanMove, Ragdoll);
    }

}
