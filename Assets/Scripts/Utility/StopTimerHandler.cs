using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopTimerHandler : MonoBehaviour, ISelectableObjectAction
{
    public ScenarioTimer ScenTimer;
    public string SelectableActionName
    {
        get
        {
            return "Stop Timer";
        }
    }

    public void PerformSelectableObjectAction()
    {
        ScenTimer.StopTimer();
    }
}
