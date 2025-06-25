using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BG4Sim : MonoBehaviour
{
    public LoadableAssetManager LoadableAssetManager;

    public bool _bO2CylOpen = false;
    public bool _bLowPressureWarning = false;
    public bool _bCriticalPressureWarning = false;
    public bool SentinelLowOxygenAlert = false;
    public int StartingOxygenPressure = 3000;
    public int BaselinePressure = 3000;//What target pressure of the BG4 oxygen cylinder to use as a baseline
    public int EndPressureTarget = 700;
    public int DepletionTime = 1800;//How long to take to get to end pressure, in real seconds
    public int SimDurationTime = 10800;//Standard duration of a BG4, (4 hours), 14400 4 hours, 10800 expected time
    public int LowPressureValue = 700;
    public int CriticalPressureValue = 145;
    public int RemainingTimeDebug = 0;

    public int OxygenPressure
    {
        get
        {
            return (int)_oxygenPressure;
        }
        set
        {
            _oxygenPressure = (double)value;
            _alarmSilenced = false;
        }
    }

    private double _depletionRate;
    private double _displayDepletionRate;

    //public NetSyncFloatValue NetSyncFloatValue;
    //public NetworkedObject NetworkedObject;
    private bool _runSim = true;
    //private float _startTime = 0;
    //private int _startPressure = 0;
    private bool _alarmSilenced = false;
    private bool _lowPressure = false;
    private bool _criticalPressure = false;
    private double _oxygenPressure = 3000;

    public bool IsRunning
    {
        get 
        { 
            return _runSim; 
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //if(NetworkedObject == null)
        //{
        //    gameObject.GetComponent<NetworkedObject>();
        //}
        //if(NetSyncFloatValue == null)
        //{
        //    gameObject.GetComponent<NetSyncFloatValue>();
        //}
        //if (NetworkedObject != null)
        //{
        //    if (NetworkedObject.HasAuthority)
        //    {
        //        NetSyncFloatValue.ValueToSync = (float)_oxygenPressure;
        //    }
        //}

        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        MinerProfile minerProfile = null;

        if (ScenarioSaveLoad.Settings != null && ScenarioSaveLoad.Settings.MinerProfileID != null)
            minerProfile = LoadableAssetManager.FindMinerProfile(ScenarioSaveLoad.Settings.MinerProfileID);

        if (minerProfile != null && !minerProfile.EnableBG4)
        {
            PauseSim();
            ResetSim();
            _oxygenPressure = StartingOxygenPressure;
        }
        else
        {
            ResetSim();
            ResumeSim();
        }

        //if (MineSceneConfiguration.CurrentScene != null)
        //{
        //    DepletionTime = (int)(MineSceneConfiguration.CurrentScene.BG4DurationMinutes * 60.0f);
        //}
        DepletionTime = (int)(ScenarioSaveLoad.Settings.BG4DurationMinutes * 60.0f);

        _depletionRate = ((double)BaselinePressure - (double)EndPressureTarget) / (double)DepletionTime;
        _displayDepletionRate = ((double)BaselinePressure - (double)EndPressureTarget) / (double)SimDurationTime;
        //_startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (_runSim)
        {
            //if (NetworkedObject != null)
            //{
            //    if (NetworkedObject.HasAuthority)
            //    {
            //        DecreaseOxygen();
            //        NetSyncFloatValue.ValueToSync = (float)_oxygenPressure;
            //    }
            //    else
            //    {
            //        _oxygenPressure = (int)NetSyncFloatValue.ValueToSync;
            //    }
            //}
            //else
            //{
            //    DecreaseOxygen();
            //}
            //UpdateSimulation(Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (_runSim)
        {
            UpdateSimulation(Time.fixedDeltaTime);
        }
    }

    public void SilenceAlarm(bool silenceAlarm = true)
    {
        _alarmSilenced = silenceAlarm;
    }

    public void UpdateSimulation(float deltaTime)
    {
        //OxygenPressure = Mathf.RoundToInt(Mathf.Lerp(_startPressure, EndPressureTarget, (Time.time - _startTime) / DepletionTime));

        _oxygenPressure -= _depletionRate * deltaTime;
        if (_oxygenPressure < 0)
        {
            _oxygenPressure = 0;
        }
        else if (_oxygenPressure > BaselinePressure)
        {
            _oxygenPressure = BaselinePressure;
        }

        if (!_lowPressure && OxygenPressure < LowPressureValue)
        {
            _lowPressure = true;
            _alarmSilenced = false;
        }

        if (!_criticalPressure && OxygenPressure < CriticalPressureValue)
        {
            _criticalPressure = true;
            _alarmSilenced = false;
        }

        if (OxygenPressure >= LowPressureValue)
        {
            _lowPressure = false;
            _alarmSilenced = false;
        }
        if (OxygenPressure >= CriticalPressureValue)
            _criticalPressure = false;
    }

    public void ResetSim()
    {
        _oxygenPressure = StartingOxygenPressure + (int)Random.Range(-200, 0);
        _bO2CylOpen = false;
        _bLowPressureWarning = false;
        _bCriticalPressureWarning = false;
        SentinelLowOxygenAlert = false;
    }
    public void PauseSim()
    {
        _runSim = false;
        //_startPressure = OxygenPressure;
    }
    public void ResumeSim()
    {
        _runSim = true;
        //_startTime = Time.time;
    }

    public bool CheckLowPressure()
    {
        //return _oxygenPressure < LowPressureValue;
        return _lowPressure;
    }

    public bool CheckCriticalPressure()
    {
        //return _oxygenPressure < CriticalPressureValue;
        return _criticalPressure;
    }

    public VRNBG4AlarmState GetAlarmState()
    {
        if (_alarmSilenced)
            return VRNBG4AlarmState.Silenced;

        if (_criticalPressure)
            return VRNBG4AlarmState.CriticalPressureAlarm;

        if (_lowPressure)
            return VRNBG4AlarmState.LowPressureAlarm;

        return VRNBG4AlarmState.Off;
    }

    public bool CheckAllClear()
    {
        return OxygenPressure > LowPressureValue;
    }

    public int GetRemainingTime()
    {
        /*
        int remainder = 0;
        float t = Mathf.Clamp(((float)(OxygenPressure - LowPressureValue) / (float)(BaselinePressure - LowPressureValue)), 0, 1);
        //Debug.Log(_oxygenPressure + ": ox " + BaselinePressure + ": baseline " + t + ": t value");
        float timeLerp = Mathf.Lerp(0, SimDurationTime, t);
        //Debug.Log(timeLerp);
        remainder = Mathf.RoundToInt(timeLerp / 60);
        RemainingTimeDebug = remainder;
        return remainder;*/

        var pressureToLow = _oxygenPressure - (double)LowPressureValue;
        var remainingTime = pressureToLow / _displayDepletionRate / 60.0;
        if (remainingTime < 0)
            remainingTime = 0;
        return (int)remainingTime;
    }
}
