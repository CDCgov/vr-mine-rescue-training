using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTimerActionHandle : MonoBehaviour, ISelectableObjectAction
{
    public ScenarioTimer ScenTimer;
    public string SelectableActionName
    {
        get
        {
            return "Reset Timer";
        }
    }

    public void PerformSelectableObjectAction()
    {
        ScenTimer.ResetTimer();
    }
}
