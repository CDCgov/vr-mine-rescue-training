using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UISliderEndEventScrub : MonoBehaviour
{
    public SessionEventManager SessionEventManager;

    public SessionPlaybackControl SessionPlaybackControl;
    public TimelineController TimelineController;
    
    

    private Slider _slider;
    private bool _locked = false;
    private float _delta = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (SessionPlaybackControl == null)
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);
        if (SessionEventManager == null)
            SessionEventManager = SessionEventManager.GetDefault(gameObject);

        if (TimelineController == null)
        {
            TimelineController = FindObjectOfType<TimelineController>();
        }

        _slider = GetComponent<Slider>();

        _slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float arg0)
    {
        if (_slider.value < TimelineController.TimelineSlider.value)
        {
            _slider.SetValueWithoutNotify(TimelineController.TimelineSlider.value);
        }
        if (SessionPlaybackControl.CurrentSessionLog == null ||
            SessionPlaybackControl.CurrentSessionLog.Duration <= 0)
            return;
        float val = _slider.value / _slider.maxValue;
        float timelineVal = TimelineController.TimelineSlider.value / TimelineController.TimelineSlider.maxValue;
        if(val != timelineVal)
        {
            _delta = val - timelineVal;
        }

        float time = val * SessionPlaybackControl.CurrentSessionLog.Duration
            + SessionPlaybackControl.CurrentSessionLog.StartTime;
        //Debug.Log("Scrubbed event slider to: " + time);
        //SessionPlaybackControl.CurrentSessionLog.EventTimeWindow = SessionPlaybackControl.CurrentSessionLog.CurrentTime - time;
        SessionEventManager.EventEndTime = time;
        SessionPlaybackControl.EventEndTime = time;
        SessionEventManager.Scrub(time);
    }

    private void Update()
    {
        if (!_locked)
        {
            _slider.value = (TimelineController.TimelineSlider.value + _delta);
        }
    }

    public void SetLocked()
    {
        //float sliderPoint = (time - SessionPlaybackControl.CurrentSessionLog.StartTime) / SessionPlaybackControl.CurrentSessionLog.Duration;
        //SessionEventManager.Scrub(time);
        //_slider.value = sliderPoint;
        _locked = true;
    }

    public void SetUnlocked()
    {
        _delta = 0;
        //_slider.value = (TimelineController.TimelineSlider.value + _delta);
        _locked = false;
        //SessionEventManager.Scrub((TimelineController.TimelineSlider.value + _delta));
    }
}
