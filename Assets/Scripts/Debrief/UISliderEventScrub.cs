using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class UISliderEventScrub : MonoBehaviour
{
    public SessionPlaybackControl SessionPlaybackControl;
    public TimelineController TimelineController;
    public SessionEventManager DebriefItemSpawner;

    private Slider _slider;

    private bool _locked = false;
    private EventTrigger _et;
    private float _delta = 0;
    private bool _allowDelta = false;
    // Start is called before the first frame update
    void Start()
    {
        if (SessionPlaybackControl == null)
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);

        if (TimelineController == null)
        {
            TimelineController = FindObjectOfType<TimelineController>();
        }
        if(DebriefItemSpawner == null)
        {
            DebriefItemSpawner = FindObjectOfType<SessionEventManager>();
        }
        _slider = GetComponent<Slider>();

        _slider.onValueChanged.AddListener(OnSliderChanged);

    }

    public void OnPointerDown()
    {
        //if (_locked)
        //{
        //    _locked = false;
        //    TimelineController.SnapLeftEventSlider(false);
        //}

        _allowDelta = true;
        if(_slider == null)
        {
            _slider = GetComponent<Slider>();
        }

        if (TimelineController.TimelineSlider.value - _delta < 0)
        {
            //_delta = 0;
            _slider.value = 0;
            float timelineVal = TimelineController.TimelineSlider.value / TimelineController.TimelineSlider.maxValue;
            _delta = timelineVal - (_slider.value / _slider.maxValue);
        }
        else
        {
            _slider.value = TimelineController.TimelineSlider.value - _delta;
        }

        if (_slider.value == 0)
        {
            _allowDelta = false;
        }
    }

    private void OnSliderChanged(float arg0)
    {
        if(_slider.value > TimelineController.TimelineSlider.value)
        {
            _slider.SetValueWithoutNotify(TimelineController.TimelineSlider.value);
        }
        if (SessionPlaybackControl.CurrentSessionLog == null ||
            SessionPlaybackControl.CurrentSessionLog.Duration <= 0)
            return;
        float val = _slider.value / _slider.maxValue;

        float timelineVal = TimelineController.TimelineSlider.value / TimelineController.TimelineSlider.maxValue;
        if (val != timelineVal)
        {
            _delta = timelineVal - val;
        }

        float time = val * SessionPlaybackControl.CurrentSessionLog.Duration
            + SessionPlaybackControl.CurrentSessionLog.StartTime;
        Debug.Log("Scrubbed event slider to: " + time);
        //SessionPlaybackControl.CurrentSessionLog.EventTimeWindow = SessionPlaybackControl.CurrentSessionLog.CurrentTime - time;
        DebriefItemSpawner.EventStartTime = time;
        SessionPlaybackControl.EventStartTime = time;
        DebriefItemSpawner.Scrub(time);

        if(_slider.value != 0)
        {
            if(!_locked)
                _allowDelta = true;
        }
        else
        {
            if(!_locked)
                _allowDelta = false;
        }
    }

    private void Update()
    {
        if (!_locked)
        {

            if (_allowDelta)
            {
                if (TimelineController.TimelineSlider.value - _delta < 0)
                {
                    //_delta = 0;
                    _slider.value = 0;
                    float timelineVal = TimelineController.TimelineSlider.value / TimelineController.TimelineSlider.maxValue;
                    _delta = timelineVal - (_slider.value / _slider.maxValue);
                }
                else
                {
                    _slider.value = TimelineController.TimelineSlider.value - _delta;
                }

                //if(_slider.value == 0)
                //{
                //    _allowDelta = false;
                //}
            }
            else
            {
                if (_slider.value > TimelineController.TimelineSlider.value)
                {
                    _slider.value = TimelineController.TimelineSlider.value;
                }
            }
            //if (_slider.value > TimelineController.TimelineSlider.value)
            //{
            //    _slider.value = TimelineController.TimelineSlider.value;
            //}

        }
    }


    public void SetLocked()
    {
        //float sliderPoint = (time - SessionPlaybackControl.CurrentSessionLog.StartTime) / SessionPlaybackControl.CurrentSessionLog.Duration;
        //_slider.value = sliderPoint;
        _locked = true;
        _allowDelta = false;
    }

    public void SetUnlocked()
    {
        if(_slider == null)
        {
            _slider = GetComponent<Slider>();
        }
        _slider.value = 0;
        _locked = false;
        _allowDelta = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }
}
