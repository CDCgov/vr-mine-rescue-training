using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomZoomInput : MonoBehaviour
{
    public TMP_InputField SpeedInput;
    public TextMeshProUGUI Label;
    public UIBtnSessionPlayPause uIBtnSessionPlayPause;
    public UIDebriefPlaybackSpeedDropdown UIDebriefPlaybackSpeedDropdown;

    public void OnEnter()
    {
        float speedValue = -1;
        if (float.TryParse(SpeedInput.text, out speedValue))
        {
            if (speedValue > 0)
            {
                uIBtnSessionPlayPause.PlaybackSpeed = speedValue;
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
