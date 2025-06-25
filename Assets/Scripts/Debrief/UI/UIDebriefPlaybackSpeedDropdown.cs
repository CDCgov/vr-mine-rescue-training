using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class UIDebriefPlaybackSpeedDropdown : MonoBehaviour
{
    public TMP_Dropdown SpeedDropdown;
    public UIBtnSessionPlayPause uIBtnSessionPlayPause;
    public GameObject CustomTimeEntryPanel;
    public TimelineController TimelineController;

    private int _savedSpeed = 1;
    // Start is called before the first frame update
    void Start()
    {
        if(SpeedDropdown == null)
        {
            SpeedDropdown = GetComponent<TMP_Dropdown>();
        }
        if (uIBtnSessionPlayPause)
        {
            uIBtnSessionPlayPause = FindObjectOfType<UIBtnSessionPlayPause>();
        }
        if (TimelineController == null)
            TimelineController = FindObjectOfType<TimelineController>();
        SpeedDropdown.onValueChanged.AddListener(OnSpeedChange);
    }

    private void OnSpeedChange(int arg0)
    {
        switch (SpeedDropdown.value)
        {
            case 0:
                //uIBtnSessionPlayPause.PlaybackSpeed = 0.5f;
                TimelineController.PlaybackSpeed = 0.5f;
                _savedSpeed = 0;
                break;
            case 1:
                //uIBtnSessionPlayPause.PlaybackSpeed = 1f;
                TimelineController.PlaybackSpeed = 1f;
                _savedSpeed = 1;
                break;
            case 2:
                //uIBtnSessionPlayPause.PlaybackSpeed = 1.5f;
                TimelineController.PlaybackSpeed = 1.5f;
                _savedSpeed = 2;
                break;
            case 3:
                //uIBtnSessionPlayPause.PlaybackSpeed = 5f;
                TimelineController.PlaybackSpeed = 5f;
                _savedSpeed = 3;
                break;
            case 4:
                //uIBtnSessionPlayPause.PlaybackSpeed = 10f;
                TimelineController.PlaybackSpeed = 10f;
                _savedSpeed = 4;
                break;
            case 5:
                CustomTimeEntryPanel.SetActive(true);
                break;
            default:
                break;
        }
        
    }

    public void RevertToPriorSpeed()
    {
        SpeedDropdown.SetValueWithoutNotify(_savedSpeed);
    }
}
