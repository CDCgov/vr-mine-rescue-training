using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBtnScenarioEditorSwitchGizmos : UIButtonBase 
{
    public Placer Placer;
    public GizmoKind GizmosToActivate;

    protected override void Start()
    {
        base.Start();

        if (Placer == null)
            Placer = FindObjectOfType<Placer>();
    }

    protected override void OnButtonClicked()
    {
        if (Placer == null)
            return;

        Placer.SwitchActiveGizmos(GizmosToActivate);
    }
}
