using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UISliderSessionScrub : MonoBehaviour
{
    public SessionPlaybackControl SessionPlaybackControl;
    public TimelineController TimelineController;
    public SessionEventManager DebriefItemSpawner;

    private Slider _slider;

    // Start is called before the first frame update
    void Start()
    {
        if (SessionPlaybackControl == null)
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);

        _slider = GetComponent<Slider>();

        _slider.onValueChanged.AddListener(OnSliderChanged);

        if(TimelineController == null)
        {
            TimelineController = FindObjectOfType<TimelineController>();
        }

        if(DebriefItemSpawner == null)
        {
            DebriefItemSpawner = FindObjectOfType<SessionEventManager>();
        }
    }

    private void OnSliderChanged(float arg0)
    {
        if (SessionPlaybackControl.CurrentSessionLog == null ||
            SessionPlaybackControl.CurrentSessionLog.Duration <= 0)
            return;

        float val = _slider.value / _slider.maxValue;

        float time = val * SessionPlaybackControl.CurrentSessionLog.Duration
            + SessionPlaybackControl.CurrentSessionLog.StartTime;

        //Debug.Log($"Seeking to {time:F2}");

        SessionPlaybackControl.Scrub(time);
        //DebriefItemSpawner.Scrub(time);
        TimelineController.SetTime = time;
    }
}
