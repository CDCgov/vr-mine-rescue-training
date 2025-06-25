using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CustomPlaybackSpeedInput : MonoBehaviour
{
    public TMP_InputField SpeedInput;
    public TextMeshProUGUI Label;
    public UIBtnSessionPlayPause uIBtnSessionPlayPause;
    public UIDebriefPlaybackSpeedDropdown UIDebriefPlaybackSpeedDropdown;

    public TimelineController TimelineController;

    private void Start()
    {
        if (TimelineController == null)
            TimelineController = FindObjectOfType<TimelineController>();
    }

    public void OnEnter()
    {
        float speedValue = -1;
        if(float.TryParse(SpeedInput.text, out speedValue))
        {
            if(speedValue > 0)
            {
                //uIBtnSessionPlayPause.PlaybackSpeed = speedValue;
                TimelineController.PlaybackSpeed = speedValue;
                Label.text = "Enter Custom Playback Speed";
                gameObject.SetActive(false);
            }
        }
        Label.text = "Please enter a valid number.";
    }

    public void OnClose()
    {
        Label.text = "Enter Custom Playback Speed";
        UIDebriefPlaybackSpeedDropdown.RevertToPriorSpeed();
        gameObject.SetActive(false);
    }
}
