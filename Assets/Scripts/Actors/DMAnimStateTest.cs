using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DMAnimStateTest : MonoBehaviour, ISelectableObjectAction
{
    public string ActionName;
    public BAHDOL.AnimationState AnimationState;
    public string ForcedAnimationStateName;

    public string SelectableActionName => ActionName;

    public void PerformSelectableObjectAction()
    {
        if (!TryGetComponent<NPCController>(out var npc))
            return;

        if (TryGetComponent<NetworkedObject>(out var netObj))
        {
            if (!netObj.HasAuthority)
            {
                netObj.RequestOwnership();
            }
        }

        if (ForcedAnimationStateName != null && ForcedAnimationStateName.Length > 0)
        {
            npc.ForceAnimationState(ForcedAnimationStateName);
        }
        else
        {
            npc.ForceAnimationState(AnimationState);
        }
    }

   
}
