using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DebriefScreenSwitcher : MonoBehaviour
{
    public Camera ScreenOneOverviewCamera;
    public Camera ScreenTwoTeamCamera;
    public GameObject ScreenOneGUI;
    public GameObject ScreenTwoGUI;
    public GameObject ResearcherCamera;
    public GameObject OverviewCamera;

    public SystemManager SystemManager;
    public DebriefSceneLoader DebriefSceneLoader;

    private DebriefScreenActivationState _state = DebriefScreenActivationState.ScreenOneActive;
    // Start is called before the first frame update
    void Start()
    {
        if(SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }
        _state = SystemManager.SystemConfig.DebriefScreenActivationState;
        if (_state == DebriefScreenActivationState.BothActive)
        {
            if (Display.displays.Length > 1)
            {
                Display.displays[1].Activate();
            }
            else
            {
                //SystemManager.SystemConfig.DebriefScreenActivationState = DebriefScreenActivationState.ScreenOneActive;
                _state = DebriefScreenActivationState.ScreenOneActive;
            }
        }
        //if(DebriefSceneLoader == null)
        //{
        //    DebriefSceneLoader = FindObjectOfType<DebriefSceneLoader>();
        //    DebriefSceneLoader.SceneLoaded += SceneLoaded;
        //}
        //SetState(DebriefScreenActivationState.ScreenTwoActive);
        //SetState(_state);
        StartCoroutine(ScreenInit());
    }

    private void SceneLoaded()
    {
        SetState(DebriefScreenActivationState.ScreenTwoActive);
        SetState(_state);
    }

    IEnumerator ScreenInit()
    {
        SetState(DebriefScreenActivationState.ScreenTwoActive);
        yield return new WaitForSeconds(0.1f);
        SetState(_state);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.KeypadPlus))
        {
            if(_state == DebriefScreenActivationState.ScreenOneActive)
            {
                _state = DebriefScreenActivationState.ScreenTwoActive;
            }
            else
            {
                _state = DebriefScreenActivationState.ScreenOneActive;
            }
            SetState(_state);
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.KeypadPlus) && Display.displays.Length > 1)
        {
            if (_state == DebriefScreenActivationState.BothActive)
            {
                _state = DebriefScreenActivationState.ScreenOneActive;
            }
            else
            {
                if(!Display.displays[1].active)
                    Display.displays[1].Activate();
                _state = DebriefScreenActivationState.BothActive;
                
            }
            SetState(_state);
        }
    }

    void SetState(DebriefScreenActivationState state)
    {
        ScenarioEditorCamera scenarioCam = null;
        Canvas canvas = ScreenTwoGUI.GetComponent<Canvas>();
        Camera[] cameraSc2 = ResearcherCamera.GetComponentsInChildren<Camera>();

        switch (state)
        {
            case DebriefScreenActivationState.ScreenOneActive:
                ResearcherCamera.SetActive(false);
                ScreenTwoGUI.SetActive(false);
                ScreenOneGUI.SetActive(true);
                OverviewCamera.SetActive(true);

                if (ResearcherCamera.TryGetComponent<ScenarioEditorCamera>(out scenarioCam))
                {
                    Vector3 pos = scenarioCam.PivotTransform.position;
                    pos.y = OverviewCamera.transform.position.y;

                    OverviewCamera.transform.position = pos;
                }

                break;
            case DebriefScreenActivationState.ScreenTwoActive:
                ScreenOneGUI.SetActive(false);
                OverviewCamera.SetActive(false);
                ResearcherCamera.SetActive(true);
                ScreenTwoGUI.SetActive(true);
                ScreenTwoTeamCamera.targetDisplay = 0;

                if (ResearcherCamera.TryGetComponent<ScenarioEditorCamera>(out scenarioCam))
                {
                    Vector3 pos, target;
                    target = OverviewCamera.transform.position;
                    target.y = 0;

                    pos = target + new Vector3(0, 7, -7);

                    //scenarioCam.PositionCamera(pos, Quaternion.LookRotation(target - pos, Vector3.up));
                    scenarioCam.FocusTarget(target);
                }

                foreach (Camera cam in cameraSc2)
                {
                    cam.targetDisplay = 0;
                }
                canvas.targetDisplay = 0;
                canvas.worldCamera = ResearcherCamera.GetComponentInChildren<Camera>();
                //ScreenTwoTeamCamera.gameObject.SetActive(false);
                break;
            case DebriefScreenActivationState.BothActive:
                ScreenOneGUI.SetActive(true);
                OverviewCamera.SetActive(true);
                ResearcherCamera.SetActive(true);
                ScreenTwoGUI.SetActive(true);
                canvas.targetDisplay = 1;
                ScreenTwoTeamCamera.targetDisplay = 1;
                foreach (Camera cam in cameraSc2)
                {
                    cam.targetDisplay = 1;
                }
                break;
            default:
                break;
        }
    }
}
