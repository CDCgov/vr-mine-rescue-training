using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonCenterCamera : UIButtonBase
{
    public ScenarioEditorCamera ScenarioEditorCamera;
    public bool SetViewDirection = true;
    public Vector3 ViewDirection = Vector3.down;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
                
        if(ScenarioEditorCamera == null)
        {
            ScenarioEditorCamera = (ScenarioEditorCamera)FindObjectOfType(typeof(ScenarioEditorCamera));
        }
    }



    protected override void OnButtonClicked()
    {
        if (ScenarioEditorCamera == null)
            return;


        if (SetViewDirection)
            ScenarioEditorCamera.ZoomToFit(ViewDirection);
        else
            ScenarioEditorCamera.ZoomToFit();
        //ScenarioEditorCamera.StartCenterCamera();

    }
}
