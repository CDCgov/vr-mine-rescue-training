using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTimerHandler : MonoBehaviour, ISelectableObjectAction
{
    public ScenarioTimer ScenTimer;
    public string SelectableActionName
    {
        get
        {
            return "Start Timer";
        }
    }

    public void PerformSelectableObjectAction()
    {
        ScenTimer.StartTimer();
    }
}
