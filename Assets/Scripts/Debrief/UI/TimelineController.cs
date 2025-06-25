using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using System;

public class TimelineController : SceneManagerBase
{
    public Slider TimelineSlider;
    public Slider ScreenTwoSlider;
    public List<Slider> LeftEventSliders;
    public List<Slider> RightEventSliders;
    public List<UIRightSnapButton> RightSnapButtons;
    public List<UILeftSnapButton> LeftSnapButtons;
    public DebriefSceneLoader DebriefSceneLoader;
    
    public Button PlayBtn;
    public Button PlayBtnScrn2;
    public TMPro.TextMeshProUGUI TimestampField1;
    public TMPro.TextMeshProUGUI TimestampField2;
    public SessionEventManager DebriefItemSpawner;
    public List<TMPro.TMP_Dropdown> PlaybackSpeedDropwdowns;
    public List<SessionTeamstopState> Teamstops;
    UISelectHandler scrubberSelect;
    UISelectHandler scrubberSelectScrn2;
    bool isScrubberSelectedPrev;
    PlayPauseButtonHandler PlayPauseButtonHandler;
    PlayPauseButtonHandler PlayPauseButtonHandlerScrn2;


    bool _prevPlayState = false;

    private float _playbackSpeed = 1;
    public float PlaybackSpeed
    {
        get { return _playbackSpeed; }
        set
        {
            _playbackSpeed = value;
            if (SessionPlaybackControl != null && SessionPlaybackControl.IsPlaying)
            {
                SessionPlaybackControl.Play(_playbackSpeed);
            }
        }
    }

    public float TotalTime
    {
        get { return _totalTime; }
        set
        {
            _totalTime = value;
            TimelineSlider.wholeNumbers = false;
            TimelineSlider.maxValue = value;
            ScreenTwoSlider.wholeNumbers = false;
            ScreenTwoSlider.maxValue = value;
        }
    }
    public float StartTime
    {
        set
        {
            TimelineSlider.minValue = value;
            ScreenTwoSlider.minValue = value;
        }
    }
    public float SetTime
    {
        set
        {
            _timelineValue = value;
        }
    }

    public float EventStartTime
    {
        get { return _eventTime; }
        set
        {
            _eventTime = value;
        }
    }
    private float _totalTime;
    private float _timelineValue = 0;
    private float _eventTime = 0;
    private float _endEventSlideDelta = 0;
    //private bool _isPlaying = false;
    private bool _leftEventSnapped = false;
    private bool _rightEventSnapped = false;
    private SessionTeamstopState cachedTeamstop;

    public SessionPlaybackControl SessionPlaybackControl;


    public static TimelineController GetDefault(GameObject self)
    {
        return self.GetDefaultManager<TimelineController>("TimelineController");
    }

    // Start is called before the first frame update
    void Start()
    {
        scrubberSelect = TimelineSlider.GetComponentInChildren<UISelectHandler>();
        scrubberSelectScrn2 = ScreenTwoSlider.GetComponentInChildren<UISelectHandler>();
        PlayPauseButtonHandler = PlayBtn.GetComponent<PlayPauseButtonHandler>();
        PlayPauseButtonHandlerScrn2 = PlayBtnScrn2.GetComponent<PlayPauseButtonHandler>();
        if(SessionPlaybackControl == null)
        {
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);
        }
        //SessionPlaybackControl.SessionLoadedAction += UpdateSliders;
        if(DebriefItemSpawner == null)
        {
            DebriefItemSpawner = FindObjectOfType<SessionEventManager>();
        }

        if(DebriefSceneLoader == null)
        {
            DebriefSceneLoader = FindObjectOfType<DebriefSceneLoader>();
            DebriefSceneLoader.InitiateSceneLoad += ResetTimeline;
        }

        foreach(Slider lSlide in LeftEventSliders)
        {
            lSlide.onValueChanged.AddListener(delegate { UpdateEventSlider(lSlide, LeftEventSliders); });
        }

        foreach(Slider rSlide in RightEventSliders)
        {
            rSlide.onValueChanged.AddListener(delegate { UpdateEventSlider(rSlide, RightEventSliders); });
        }

        foreach(TMPro.TMP_Dropdown tMP_Dropdown in PlaybackSpeedDropwdowns)
        {
            tMP_Dropdown.onValueChanged.AddListener(delegate { UpdatePlaybackDropdown(tMP_Dropdown); });
        }

        if(LeftSnapButtons == null)
        {
            LeftSnapButtons = new List<UILeftSnapButton>();
            foreach(UILeftSnapButton uILeftSnapButton in FindObjectsOfType<UILeftSnapButton>())
            {
                LeftSnapButtons.Add(uILeftSnapButton);
            }
        }
        if (RightSnapButtons == null)
        {
            RightSnapButtons = new List<UIRightSnapButton>();
            foreach (UIRightSnapButton uIRightSnapButton in FindObjectsOfType<UIRightSnapButton>())
            {
                RightSnapButtons.Add(uIRightSnapButton);
            }
        }
        if (Teamstops == null)
            Teamstops = new List<SessionTeamstopState>();

        SessionPlaybackControl.SessionScrubbed += UpdateTime;
    }

    private void ResetTimeline()
    {
        _timelineValue = 0;
        UpdateTime();
    }

    void UpdateTime()
    {
        
        _timelineValue = SessionPlaybackControl.CurrentTimestamp;
        float slideVal = ((_timelineValue - SessionPlaybackControl.CurrentSessionLog.StartTime) / SessionPlaybackControl.CurrentSessionLog.Duration) * TimelineSlider.maxValue;
        TimelineSlider.SetValueWithoutNotify(slideVal);
        ScreenTwoSlider.SetValueWithoutNotify(slideVal);
        //Debug.Log("Time invoke happened " + _timelineValue);
        TimeSpan timeSp = TimeSpan.FromSeconds((double)(SessionPlaybackControl.CurrentTimestamp - SessionPlaybackControl.CurrentSessionLog.StartTime));
        string timeStr = timeSp.Hours.ToString("00") + ":" + timeSp.Minutes.ToString("00") + ":" + timeSp.Seconds.ToString("00");
        
        TimestampField1.text = timeStr;
        TimestampField2.text = timeStr;
        DebriefItemSpawner.Scrub(_timelineValue);
        SessionTeamstopState sts = GetCurrentTeamstop();
        if (_leftEventSnapped)
        {
            float startTime = 0;
            if (sts.TeamstopStartTime == 0)
            {
                startTime = SessionPlaybackControl.CurrentSessionLog.StartTime;
            }
            else
            {
                startTime = sts.TeamstopStartTime;
            }
            LeftEventSliders[0].value = EventValueToSliderValue(startTime, LeftEventSliders[0]);
            float lSlid = LeftEventSliders[0].value;
            foreach (Slider slider in LeftEventSliders)
            {
                if (slider.value != LeftEventSliders[0].value)
                {
                    slider.SetValueWithoutNotify(LeftEventSliders[0].value);
                }                
            }
        }
        if (_rightEventSnapped)
        {
            float endTime = 0;
            if (sts.TeamstopEndTime == -1)
            {
                endTime = SessionPlaybackControl.CurrentSessionLog.EndTime;
            }
            else
            {
                endTime = sts.TeamstopEndTime;
            }
            RightEventSliders[0].value = EventValueToSliderValue(endTime, RightEventSliders[0]);
            float slid = RightEventSliders[0].value;
            foreach (Slider slider in RightEventSliders)
            {
                if (slider.value != slid)
                {
                    slider.SetValueWithoutNotify(slid);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SessionPlaybackControl.IsPlaying)
        {
            //_timelineValue += Time.deltaTime;
            //_timelineValue = ((SessionPlaybackControl.CurrentTimestamp - SessionPlaybackControl.CurrentSessionLog.StartTime) / SessionPlaybackControl.CurrentSessionLog.Duration) * TotalTime;
            float slideVal = ((_timelineValue - SessionPlaybackControl.CurrentSessionLog.StartTime) / SessionPlaybackControl.CurrentSessionLog.Duration) * TimelineSlider.maxValue;
            TimelineSlider.SetValueWithoutNotify(slideVal);
            ScreenTwoSlider.SetValueWithoutNotify(slideVal);
            //if(SessionPlaybackControl.CurrentPlaybackSpeed == 0)
            //{
            //    _isPlaying = false;
            //}
            SessionTeamstopState sts = GetCurrentTeamstop();
            if(cachedTeamstop != sts)
            {
                Debug.Log($"Teamstop info: {sts.TeamstopIndex} - {sts.TeamstopStartTime}, {sts.TeamstopEndTime}");
                cachedTeamstop = sts;
            }
            if (_leftEventSnapped)
            {
                float startTime = 0;
                if(sts.TeamstopStartTime == 0)
                {
                    startTime = SessionPlaybackControl.CurrentSessionLog.StartTime;
                }
                else
                {
                    startTime = sts.TeamstopStartTime;
                }
                LeftEventSliders[0].value = EventValueToSliderValue(startTime, LeftEventSliders[0]);
                float lSlid = LeftEventSliders[0].value;
                foreach (Slider slider in LeftEventSliders)
                {
                    if (slider.value != lSlid)
                    {
                        slider.SetValueWithoutNotify(lSlid);
                    }
                }
            }
            if (_rightEventSnapped)
            {
                float endTime = 0;
                if(sts.TeamstopEndTime == -1)
                {
                    endTime = SessionPlaybackControl.CurrentSessionLog.EndTime;
                }
                else
                {
                    endTime = sts.TeamstopEndTime;
                }
                RightEventSliders[0].value = EventValueToSliderValue(endTime, RightEventSliders[0]);
                float slid = RightEventSliders[0].value;
                foreach (Slider slider in RightEventSliders)
                {
                    if (slider.value != slid)
                    {
                        slider.SetValueWithoutNotify(slid);
                    }
                }
            }
        }
        //if (Input.GetKeyUp(KeyCode.Space))
        //{
        //    //_isPlaying = !_isPlaying;
        //    PlayPauseButtonHandler.GetComponent<Button>().onClick.Invoke();
        //}

        //if (_prevPlayState != _isPlaying)
        //{
        //    PlayPauseButtonHandler.IsPlaying = _isPlaying;
        //    //PlayPauseButtonHandler.GetComponent<Button>().onClick.Invoke();
        //    PlayPauseButtonHandlerScrn2.IsPlaying = _isPlaying;
        //    //PlayPauseButtonHandlerScrn2.GetComponent<Button>().onClick.Invoke();
        //    _prevPlayState = _isPlaying;
        //}
        if (SessionPlaybackControl.CurrentSessionLog != null)
        {
            if(SessionPlaybackControl.CurrentTimestamp >= 0.01f)
            {
                //string timeStr = TimeSpan.FromSeconds((double)(SessionPlaybackControl.CurrentTimestamp - SessionPlaybackControl.CurrentSessionLog.StartTime)).ToString("HH:mm: ss.ff");
                //TimestampField1.text = timeStr;
                //TimestampField2.text = timeStr;
                //TimestampField1.text = (SessionPlaybackControl.CurrentTimestamp - SessionPlaybackControl.CurrentSessionLog.StartTime).ToString("F2");
                //TimestampField2.text = (SessionPlaybackControl.CurrentTimestamp - SessionPlaybackControl.CurrentSessionLog.StartTime).ToString("F2");
            }
            else
            {
                TimestampField1.text = "00:00";
                TimestampField2.text = "00:00";
            }
        }
    }

    //public void Scrub()
    //{
    //    //if (!scrubberSelect.IsSelected)
    //    //{
    //    //    if (isScrubberSelectedPrev)
    //    //    {
    //    //        // Edge event, user has just released the mouse button.
    //    //        // Set the timeline value to match the slider position.
    //    //        _isPlaying = false;
    //    //        _timelineValue = TimelineSlider.value;
    //    //        Debug.Log($"The value is scrubbed to {TimelineSlider.value}");
    //    //    }
    //    //    else
    //    //    {
    //    //        // Not selected, slider follows the timeline.
    //    //        TimelineSlider.value = _timelineValue;
    //    //    }
    //    //}

    //    if (scrubberSelect.IsSelected)
    //    {
    //        //_isPlaying = false;
    //        _timelineValue = TimelineSlider.value;
    //        ScreenTwoSlider.SetValueWithoutNotify(_timelineValue);
    //        Debug.Log($"Timeline scrubbed to {TimelineSlider.value} from screen 1");
            
    //    }

    //    if (scrubberSelectScrn2.IsSelected)
    //    {
    //        //_isPlaying = false;
    //        _timelineValue = ScreenTwoSlider.value;
    //        TimelineSlider.SetValueWithoutNotify(_timelineValue);
    //        Debug.Log($"Timeline scrubbed to {ScreenTwoSlider.value} from screen 2");
    //    }
    //    //isScrubberSelectedPrev = scrubberSelect.IsSelected;

        
    //}

    public void PlayButton()
    {
        //_isPlaying = !_isPlaying;

        //if(SessionPlaybackControl == null)
        //{
        //    SessionPlaybackControl = SessionPlaybackControl.GetDefault();
        //}
        if (!SessionPlaybackControl.IsPlaying)
        {
            SessionPlaybackControl.Play(PlaybackSpeed);
        }
        else
        {
            SessionPlaybackControl.Play(0);
        }
    }
    public void StopButton()
    {
        //_isPlaying = false;
    }

    public float UpdateEventSlider(Slider slideSource, List<Slider> sliders)
    {
        foreach (Slider sl in sliders)
        {
            if (sl != slideSource && sl.value != slideSource.value)
            {
                sl.SetValueWithoutNotify(slideSource.value);
            }
        }

        return slideSource.value;
    }
    
    public void UpdatePlaybackDropdown(TMPro.TMP_Dropdown source)
    {
        foreach(TMPro.TMP_Dropdown item in PlaybackSpeedDropwdowns)
        {
            if(item != source)
            {
                item.SetValueWithoutNotify(source.value);
            }
        }
    }

    private SessionTeamstopState GetCurrentTeamstopInTimeline(float time)
    {
        SessionTeamstopState teamstop = new SessionTeamstopState();
        //SessionTeamstopState teamstop = SessionPlaybackControl.CurrentSessionLog.CurrentTeamstopState;

        teamstop.TeamstopIndex = -100;
        foreach (SessionTeamstopState teamstopState in Teamstops)
        {
            if (time >= teamstopState.TeamstopStartTime && time < teamstopState.TeamstopEndTime)
            {
                Debug.Log($"Found teamstop: {teamstopState.TeamstopIndex}");
                teamstop = teamstopState;
            }
        }
        return teamstop;
    }

    private SessionTeamstopState GetCurrentTeamstop()
    {
        SessionTeamstopState teamstop = SessionPlaybackControl.CurrentSessionLog.CurrentTeamstopState;
        return teamstop;
    }

    public void GoToNextTeamstop()
    {
        SessionTeamstopState sessionTeamstopState = GetCurrentTeamstop();
        if(sessionTeamstopState.TeamstopIndex != -100)
        {
            foreach(SessionTeamstopState state in Teamstops)
            {
                if(state.TeamstopIndex == sessionTeamstopState.TeamstopIndex)
                {
                    float endTime ;
                    if (state.TeamstopEndTime == -1)
                    {
                        endTime = SessionPlaybackControl.CurrentSessionLog.EndTime;
                    }
                    else
                    {
                        endTime = state.TeamstopEndTime;
                    }
                    Debug.Log($"Scrub to teamstop: {state.TeamstopIndex}");
                    SessionPlaybackControl.Scrub(endTime);
                }
            }
        }
    }

    public void GoToPreviousTeamstop()
    {
        SessionTeamstopState sessionTeamstopState = GetCurrentTeamstop();
        if (sessionTeamstopState.TeamstopIndex != -100)
        {
            foreach (SessionTeamstopState state in Teamstops)
            {
                if (state.TeamstopIndex == sessionTeamstopState.TeamstopIndex - 1)
                {
                    float startTime;
                    if (state.TeamstopStartTime == 0)
                    {
                        startTime = SessionPlaybackControl.CurrentSessionLog.StartTime;
                    }
                    else
                    {
                        startTime = state.TeamstopStartTime;
                    }
                    Debug.Log($"Scrub to teamstop: {state.TeamstopIndex}");
                    SessionPlaybackControl.Scrub(startTime);
                }
            }
        }
    }

    public void SnapRightEventSlider(bool isSnapped)
    {
        _rightEventSnapped = !_rightEventSnapped;
        if (_rightEventSnapped)
        {
            float time = SliderToEventValue(RightEventSliders[0].value, RightEventSliders[0]);
            Debug.Log($"I thinks the end point is: {GetCurrentTeamstopInTimeline(time).TeamstopEndTime} in teamstop {GetCurrentTeamstopInTimeline(time).TeamstopIndex}");
            //RightEventSliders[0].value = EventValueToSliderValue(GetCurrentTeamstopInTimeline(time).TeamstopEndTime, RightEventSliders[0]);
            RightEventSliders[0].value = EventValueToSliderValue(GetCurrentTeamstop().TeamstopEndTime, RightEventSliders[0]);
        }
        else
        {
            RightEventSliders[0].value = EventValueToSliderValue(_timelineValue, RightEventSliders[0]);
        }
        foreach (Slider slider in RightEventSliders)
        {
            if (slider.value != RightEventSliders[0].value)
            {
                slider.SetValueWithoutNotify(RightEventSliders[0].value);
            }
            if (_rightEventSnapped)
            {
                slider.GetComponent<UISliderEndEventScrub>().SetLocked();
            }
            else
            {
                slider.GetComponent<UISliderEndEventScrub>().SetUnlocked();
            }
        }

        foreach(UIRightSnapButton uIRightSnapButton in RightSnapButtons)
        {
            if (_rightEventSnapped)
            {
                uIRightSnapButton.SnappedIcon();
            }
            else
            {
                uIRightSnapButton.ReleasedIcon();
            }
        }
    }

    public void SnapLeftEventSlider(bool isSnapped)
    {
        _leftEventSnapped = !_leftEventSnapped;
        if (_leftEventSnapped)
        {
            float time = SliderToEventValue(LeftEventSliders[0].value, LeftEventSliders[0]);
            Debug.Log($"I thinks the start point is: {GetCurrentTeamstopInTimeline(time).TeamstopStartTime} in teamstop {GetCurrentTeamstopInTimeline(time).TeamstopIndex}");
            //LeftEventSliders[0].value = EventValueToSliderValue(GetCurrentTeamstopInTimeline(time).TeamstopStartTime, LeftEventSliders[0]);
            LeftEventSliders[0].value = EventValueToSliderValue(GetCurrentTeamstop().TeamstopStartTime, LeftEventSliders[0]);
        }
        else
        {
            LeftEventSliders[0].value = EventValueToSliderValue(_timelineValue, LeftEventSliders[0]);
        }
        foreach (Slider slider in LeftEventSliders)
        {
            if (slider.value != LeftEventSliders[0].value)
            {
                slider.SetValueWithoutNotify(LeftEventSliders[0].value);
            }
            if (_leftEventSnapped)
            {
                slider.GetComponent<UISliderEventScrub>().SetLocked();
            }
            else
            {
                slider.GetComponent<UISliderEventScrub>().SetUnlocked();
            }
        }

        foreach (UILeftSnapButton uILeftSnapButton in LeftSnapButtons)
        {
            if (_leftEventSnapped)
            {
                uILeftSnapButton.SnappedIcon();
            }
            else
            {
                uILeftSnapButton.ReleasedIcon();
            }
        }

        if(LeftEventSliders[0].value > RightEventSliders[0].value)
        {
            RightEventSliders[0].value = LeftEventSliders[0].value;
            RightSnapButtons[0].RightSnapClicked();
            _rightEventSnapped = true;
        }
    }

    private float TimeValueToSliderValue(float time)
    {
        float slideVal = ((time - SessionPlaybackControl.CurrentSessionLog.StartTime) / SessionPlaybackControl.CurrentSessionLog.Duration) * TimelineSlider.maxValue;
        return slideVal;
    }
    private float SliderValueToTimeValue(float slideVal)
    {
        float timeValue = ((slideVal / TimelineSlider.maxValue) * SessionPlaybackControl.CurrentSessionLog.Duration) + SessionPlaybackControl.CurrentSessionLog.StartTime;
        return timeValue;
    }

    private float EventValueToSliderValue(float time, Slider slider)
    {
        float sliderValue = ((time - SessionPlaybackControl.CurrentSessionLog.StartTime) / SessionPlaybackControl.CurrentSessionLog.Duration) * slider.maxValue;
        return sliderValue;
    }

    private float SliderToEventValue(float sliValue, Slider slider)
    {
        float t = ((sliValue / slider.maxValue) * SessionPlaybackControl.CurrentSessionLog.Duration) + SessionPlaybackControl.CurrentSessionLog.StartTime;
        return t;
    }
}
