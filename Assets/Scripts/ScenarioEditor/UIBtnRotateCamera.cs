using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnRotateCamera : UIButtonBase
{
    public Placer Placer;
    public ScenarioEditorCamera ScenarioEditorCamera;

    public Vector3 ViewDirection = Vector3.down;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        if (Placer == null)
            Placer = Placer.GetDefault();

        if (ScenarioEditorCamera == null)
        {
            ScenarioEditorCamera = (ScenarioEditorCamera)FindObjectOfType(typeof(ScenarioEditorCamera));
        }
    }



    protected override void OnButtonClicked()
    {
        if (ScenarioEditorCamera == null)
            return;

        if (Placer != null && Placer.SelectedObject != null)
        {
            if (!ScenarioEditorCamera.IsOrbitEnabled)
                ScenarioEditorCamera.FocusTarget(Placer.SelectedObject.transform.position);
        }

        ScenarioEditorCamera.RotateCamera(ViewDirection);

    }
}
