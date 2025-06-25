using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnSessionPlayPause : MonoBehaviour
{
    public SessionPlaybackControl SessionPlaybackControl;
    public TimelineController TimelineController;

    public float PlaybackSpeed = 1.0f;
    public bool TogglePause = true;

    private Button _button;

    // Start is called before the first frame update
    void Start()
    {
        if (SessionPlaybackControl == null)
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);

        if (TimelineController == null)
            TimelineController = TimelineController.GetDefault(gameObject);
            //TimelineController = FindObjectOfType<TimelineController>();

        _button = GetComponent<Button>();

        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        TimelineController = TimelineController.GetDefault(gameObject);
        //if (SessionPlaybackControl.CurrentPlaybackSpeed > 0 && TogglePause)
        //    SessionPlaybackControl.Play(0);
        //else
        //    SessionPlaybackControl.Play(PlaybackSpeed);
        TimelineController.PlayButton();
    }
}
