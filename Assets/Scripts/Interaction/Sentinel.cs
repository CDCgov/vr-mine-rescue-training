using UnityEngine;
using TMPro;
using System.Collections;
using System;
using System.IO;
using Google.Protobuf;

public class Sentinel : MonoBehaviour, IRequiresPlayerID, INetSync, IInteractableObject
{

    public BG4SimManager BG4SimManager;
    public NetworkManager NetworkManager;

	public GameObject _SentinelDisplay = null;
	//public BG4Sim _sim = null;

	public Material _SentinelRedLightMaterial = null;
	public MeshRenderer _SentinelRenderer = null;
    public MeshRenderer _SentinelRedRenderer2 = null;
    public Material SentinelGreenLightMaterial = null;
    public Renderer GreenLightRenderer = null;

    public float GreenStatusBeepInterval = 30;


	public AudioClip _AudioDoublePlusLongBeep;
	public AudioClip _AudioDoubleBeep;
	public AudioClip _AudioLongBeep;
	public AudioClip _AudioNormalStartup;
	public AudioClip _AudioErrorStartup;
	public AudioClip _AudioO2Alarm;
	public AudioClip _AudioLowO2Beeping;

	public AudioSource SrcDefault;
    public AudioSource SrcLowO2Beeping;
    public AudioSource SrcStartupErrorBeeps;
    public AudioSource SrcStatusBeep;

	private Material _originalRedMaterial;
    private Material _originalGreenMaterial;

    //private UISprite _icon;     
    public TextMeshProUGUI SmallText;
    public TextMeshProUGUI LargeText;
    public TextMeshProUGUI LabelText;
    public GameObject DisplayRedLight;
    public GameObject DisplayGreenLight;

    public GameObject Display;

	public GameObject[] DisplaySegments;

    //public NetworkedObject NetworkedObject;
    //public NetSyncBoolValue AlarmSync;

	private const int NUM_SEGMENTS = 14;

	//private bool _bFlashAlert = true;
	private bool _bRedLightOn = false;
    private bool _bGreenLightOn = false;
    private bool _bAlarmIsSilent = true;
    private bool _bLowPressureAlarmOn = false;
    private bool _bCriticalPressureAlarmOn = false;
	private float _fFlashTime = 0;
    private float _fFlashGreenTime = 0;
	//private float _flashStart = 0;
	private const float _flashInterval = 0.4f;
	private const float _flashDuration = 0.1f;

    private const float _flashGreenInterval = 0.9f;
    private const float _flashGreenDuration = 0.1f;

    private int _playerID;
    private VRNBG4SimData _simData;
    private BG4Sim _bgSim;
    private float _silenceLowPressureTimer = 0;
    private bool _autoSilence = false;
    private float _greenStatusBeepTime = 0;
    private bool _logLowPressure = false;
    private bool _logCriticalPressure = false;
    private NetworkedObject _netObj;

	private enum SentinelState
	{
		SelfTest,
		BatteryTest,
		HighPressureLeakTest,
		NormalStartup,
		ErrorStartup,
		Normal,
		Off
	}

	private SentinelState _state = SentinelState.Off;
	private float _transitionTime = 0;
	private int _leakTestStage = 0;

	public enum SentinelIcons
	{
		None,
		BatteryGood,
		BatteryLow,
		Checkmark,
		Clock,
		CloseValve,
		Error,
		Exclamation,
		OpenValve
	}

	public void OnSentinelOff(GameObject button)
	{
		SentinelOn(false);
	}

    public void SetPlayerID(int playerID)
    {
        _playerID = playerID;
    }

	// Use this for initialization
	void Start () 
    {
        if (BG4SimManager == null)
            BG4SimManager = BG4SimManager.GetDefault(gameObject);

        if(BG4SimManager == null)
        {
            Debug.LogError($"Sim manager was null! RACE CON");
        }
        BG4SimManager.SimDataChanged += OnBG4SimDataChanged;
        //_icon = _SentinelDisplay.GetComponentInChildren<UISprite>();

        //_icon = _SentinelDisplay.transform.Find("Panel/Sprite (CloseValve)").GetComponent<UISprite>();
        //SmallText = _SentinelDisplay.transform.Find("Display/SmallText").GetComponent<TextMesh>();
        //LargeText = _SentinelDisplay.transform.Find("Display/Text").GetComponent<TextMesh>();
        //LabelText = _SentinelDisplay.transform.Find("Display/LabelText").GetComponent<TextMesh>();
        //_display = _SentinelDisplay.transform.Find("Display").gameObject;
        //DisplayRedLight = _SentinelDisplay.transform.Find("Panel/RedLight").gameObject;

        if (_SentinelRenderer != null)
        {
            _originalRedMaterial = _SentinelRenderer.material;
        }        
        if(GreenLightRenderer != null)
        {
            _originalGreenMaterial = GreenLightRenderer.material;
        }
        if (DisplayRedLight != null)
		    DisplayRedLight.SetActive(false);

        if (DisplayGreenLight != null)
            DisplayGreenLight.SetActive(false);

		DisplayOn(true);
		_state = SentinelState.Normal;

        //DisplaySegments = new GameObject[NUM_SEGMENTS];
        //for (int i = 0; i < NUM_SEGMENTS; i++)
        //{
        //	string path = string.Format("Display/sentinelSegments/segment{0:D2}", i + 1);

        //	DisplaySegments[i] = _SentinelDisplay.transform.Find(path).gameObject;
        //}

        //_srcDefault = VRUtil.GetComponent<AudioSource>(gameObject);

        //_srcLowO2Beeping = gameObject.AddComponent<AudioSource>();
        //_srcLowO2Beeping.loop = false;
        //_srcLowO2Beeping.playOnAwake = false;
        //_srcLowO2Beeping.clip = _AudioLowO2Beeping;
        if (SrcLowO2Beeping != null)
            SrcLowO2Beeping.clip = _AudioLowO2Beeping;

        //SrcStartupErrorBeeps = gameObject.AddComponent<AudioSource>();
        //SrcStartupErrorBeeps.loop = false;
        //SrcStartupErrorBeeps.clip = _AudioErrorStartup;

        if (SrcStartupErrorBeeps != null)
            SrcStartupErrorBeeps.clip = _AudioErrorStartup;


        if (_bgSim == null)
        {
            _bgSim = GameObject.FindObjectOfType<BG4Sim>();
        }

        _greenStatusBeepTime = Time.time + UnityEngine.Random.Range(1, 15);

        if(_netObj == null)
        {
            _netObj = GetComponent<NetworkedObject>();
        }
    }

    private void OnDestroy()
    {
        if (BG4SimManager != null)
            BG4SimManager.SimDataChanged -= OnBG4SimDataChanged;
    }

    private void OnBG4SimDataChanged(VRNBG4SimData simData)
    {
        //Debug.Log($"TEST: Sentinel - {simData.PlayerID}, {simData.OxygenPressure}, local ID?: {_playerID}");
        if (simData.PlayerID == _playerID)
        {
            //Debug.Log($"Sentinel - Simdata was updated! at {Time.time}");
            _simData = simData;
        }
    }

    private void PlaySound(AudioClip clip)
	{
		SrcDefault.PlayOneShot(clip);
	}

	
	// Update is called once per frame
	void Update()
	{
        //Debug.Log($"Sentinel update loop {Time.time}");
        if (_simData == null)
        {
            //if (BG4SimManager.GetSimData(_playerID) != null)
            //{
            //    Debug.Log($"Setinel sim data was null {_playerID}, {BG4SimManager.GetSimData(_playerID).OxygenPressure}");
            //}
            return;
        }

        if(_bgSim == null && _netObj.HasAuthority)
        {
            _bgSim = GameObject.FindObjectOfType<BG4Sim>();
            if (_bgSim == null)
            {
                Debug.LogWarning("Sentinel update couldn't find BGSim, disabling");
                this.enabled = false;
                return;
            }
        }

		if (_state != SentinelState.Normal && _state != SentinelState.Off)
		{
			if (SrcLowO2Beeping.isPlaying)
				SrcLowO2Beeping.Stop();

			if (_bRedLightOn)
				ShowRedLight(false);
            if (_bGreenLightOn)
                ShowGreenLight(false);
		}
		else
		{
            //if(NetworkedObject != null)
            //{
            //    if (!NetworkedObject.HasAuthority)
            //    {
            //        if(AlarmSync != null)
            //        {
            //            if(AlarmSync.Value != _bAlarmIsSilent)
            //            {
            //                _bAlarmIsSilent = AlarmSync.Value;
            //            }
            //        }
            //    }
            //}
            float greenDuration = _bGreenLightOn ? _flashGreenDuration : _flashGreenInterval;
            if(Time.time - _fFlashGreenTime > greenDuration)
            {
                ShowGreenLight(!_bGreenLightOn);
                _fFlashGreenTime = Time.time;
                //if (_bGreenLightOn && Time.time > _greenStatusBeepTime)
                //{
                //    SrcStatusBeep.Play();//time every 30 seconds
                //    _greenStatusBeepTime = Time.time + GreenStatusBeepInterval;
                //}
            }
            if (_bLowPressureAlarmOn)
			{
				float duration = _bRedLightOn ? _flashDuration : _flashInterval;

				if (Time.time - _fFlashTime > duration)
				{
					ShowRedLight(!_bRedLightOn);
					_fFlashTime = Time.time;
				}
                //if (!_bAlarmIsSilent)
                if (_simData.AlarmState == VRNBG4AlarmState.CriticalPressureAlarm || _simData.AlarmState == VRNBG4AlarmState.LowPressureAlarm)                
                {
                    //if (_sim.SentinelLowOxygenAlert)
                    //{
                    //    _srcDefault.PlayOneShot(_AudioO2Alarm);
                    //}
                    if (_autoSilence && Time.time > _silenceLowPressureTimer)
                    {
                        _autoSilence = false;
                        _silenceLowPressureTimer = 0;
                        if (_bgSim != null)
                        {
                            _bgSim.SilenceAlarm();
                        }
                    }
                    var playerID = _playerID;
                    if (_netObj != null)
                    {
                        if (_netObj.HasAuthority)
                        {
                            if (_simData.AlarmState == VRNBG4AlarmState.LowPressureAlarm && !_logLowPressure)
                            {
                                //Log low pressure
                                VRNLogEvent vrEv = new VRNLogEvent
                                {
                                    EventType = VRNLogEventType.SentinelLow,
                                    Message = "",
                                    Position = transform.position.ToVRNVector3(),
                                    Rotation = transform.rotation.ToVRNQuaternion(),
                                    //ObjectType = _objData.ObjectType,
                                    ObjectName = "Sentinel",
                                    SourcePlayerID = playerID,
                                    PositionMetadata = "Low Pressure",
                                };

                                _netObj.NetManager.LogSessionEvent(vrEv);
                                _logLowPressure = true;
                            }

                            if (_simData.AlarmState == VRNBG4AlarmState.CriticalPressureAlarm && !_logCriticalPressure)
                            {
                                //log critical pressure
                                VRNLogEvent vrEv = new VRNLogEvent
                                {
                                    EventType = VRNLogEventType.SentinelEmpty,
                                    Message = "",
                                    Position = transform.position.ToVRNVector3(),
                                    Rotation = transform.rotation.ToVRNQuaternion(),
                                    //ObjectType = _objData.ObjectType,
                                    ObjectName = "Sentinel",
                                    SourcePlayerID = playerID,
                                    PositionMetadata = "Critical Pressure",
                                };

                                _netObj.NetManager.LogSessionEvent(vrEv);
                                _logCriticalPressure = true;
                            }
                        }
                    }
                    if (!SrcLowO2Beeping.isPlaying)
                    {
                        //_srcLowO2Beeping.PlayDelayed(2.0f);
                        SrcLowO2Beeping.Play();
                    }
                }
				/*if (Time.time - _flashStart > _flashDuration)
				{
					_bFlashAlert = false;
					ShowRedLight(false);
				}*/

			}
			else
			{
				if (_bRedLightOn)
					ShowRedLight(false);

				if (SrcLowO2Beeping.isPlaying)
					SrcLowO2Beeping.Stop();

                _logLowPressure = false;
                _logCriticalPressure = false;
			}
		}

        switch (_state)
		{            
			case SentinelState.Off:
				//if (_sim._oxygenPressure > 10 && _sim._bO2CylOpen)
                if (_simData.OxygenPressure > 10 && _simData.OxygenCylOpen)
				{
					SentinelOn(true);
				}
				else if (!CheckLowPressureWarning())
				{
					//SetIcon(SentinelIcons.None);
				}
				break;

			case SentinelState.SelfTest:

				if (Time.time - _transitionTime > 1.5f)
					SwitchState(SentinelState.BatteryTest);
				break;

			case SentinelState.BatteryTest:

				if (BarGraphCountdown(3.0f))
				{
					//if (Time.time - _transitionTime > 4.0f)					
					SwitchState(SentinelState.HighPressureLeakTest);
				}
				break;

			case SentinelState.HighPressureLeakTest:

				if (_leakTestStage == 0)
				{
					//wait for at least 2600 psi
					DisplayPressure();
					//if (_sim._oxygenPressure > 2600)
                    if (_simData.OxygenPressure > 2600)
					{
						PlaySound(_AudioDoubleBeep);
						AdvanceLeakTestStage();
					}
					else if (Time.time - _transitionTime > 5.0f)
					{
						SwitchState(SentinelState.ErrorStartup);
					}

				}
				else if (_leakTestStage == 1)
				{
					//SetIcon(SentinelIcons.CloseValve);
					//countdown to close valve
					//if (!_sim._bO2CylOpen)
                    if (_simData.OxygenCylOpen)
					{

						//valve closed, long beep
						PlaySound(_AudioLongBeep);
						AdvanceLeakTestStage();
					}
					else if (BarGraphCountdown(30.0f))
					{
						SwitchState(SentinelState.ErrorStartup);
					}
				}
				else if (_leakTestStage == 2)
				{
					//wait about 6 seconds and pass the test  - manual says 15 but thats not how ours behaves....
					//SetIcon(SentinelIcons.Clock);

					DisplayPressure(false);
					//if (_sim._bO2CylOpen)
                    if (_simData.OxygenCylOpen)
					{
						//abort test
						SwitchState(SentinelState.ErrorStartup);
					}
					else if (BarGraphCountdown(6.0f))
					{
						//double beep as we are about to prompt user to open valve
						PlaySound(_AudioDoubleBeep);
						AdvanceLeakTestStage();
					}
				}
				else
				{
					//wait for them to open the valve
					//SetIcon(SentinelIcons.OpenValve);
					DisplayPressure(false);
					if (BarGraphCountdown(10.0f))
					{
						SwitchState(SentinelState.ErrorStartup);
					}
					//else if (_sim._bO2CylOpen)
                    else if (_simData.OxygenCylOpen)
					{
						//test completed successfully 
						//find the test instance and fire the completion event manually
						//foreach (BG4Tests.IBG4Test test in BG4Sim.Tests.Values)
						//{
						//	if (test is BG4Tests.TestHighPressureLeak)
						//	{
						//		((BG4Tests.TestHighPressureLeak)test).CompleteHighPressureLeakTest();
						//	}
						//}

						SwitchState(SentinelState.NormalStartup);
					}
				}

				break;

			case SentinelState.NormalStartup:
				DisplayOn(false);
				//SetIcon(SentinelIcons.Exclamation);

				if (Time.time - _transitionTime > 1.0f)
				{
					DisplayOn(true);
					SwitchState(SentinelState.Normal);
				}
				break;

			case SentinelState.ErrorStartup:
				if (!SrcStartupErrorBeeps.isPlaying)
				{
					SwitchState(SentinelState.NormalStartup);
				}

				break;

			case SentinelState.Normal:
				DisplayPressure();
                if (!_bLowPressureAlarmOn)
                {
                    //if (!CheckLowPressureWarning())
                    //{
                    //    //SetIcon(SentinelIcons.Clock);
                    //}
                    CheckLowPressureWarning();
                }
                else
                {
                    if (!_bCriticalPressureAlarmOn)
                    {
                        CheckCriticalPressureWarning();
                    }
                }

                //if (_sim.CheckAllClear())
                if (!_simData.LowPressure && !_simData.CriticalPressure)
                {
                    _bCriticalPressureAlarmOn = false;
                    _bLowPressureAlarmOn = false;
                }
                break;
		}

		
	}

	/*
	void FixedUpdate()
	{
		if (_sim.SentinelLowOxygenAlert)
		{
			StartFlashingRedLight();
		}
	}

	void StartFlashingRedLight()
	{
		_flashStart = Time.time;
		_bFlashAlert = true;
	}*/

	void DisplayOn(bool bOn)
	{
		if (bOn)
		{
            if (Display != null)
			    Display.SetActive(true);
		}
		else
		{
            if (Display != null)
			    Display.SetActive(false);

			//SetIcon(SentinelIcons.None);
		}
	}

	void ShowRedLight(bool bShow)
	{
		if (bShow && !_bRedLightOn)
		{
            if(DisplayRedLight != null)
                DisplayRedLight.SetActive(true);
            if (_SentinelRenderer != null)
            {
                _SentinelRenderer.material = _SentinelRedLightMaterial;                
            }
            if(_SentinelRedRenderer2 != null)
            {
                _SentinelRedRenderer2.material = _SentinelRedLightMaterial;
            }
			_bRedLightOn = true;
		}
		else if (!bShow && _bRedLightOn)
		{
            if(DisplayRedLight != null)
                DisplayRedLight.SetActive(false);
            if (_SentinelRenderer != null)
            {
                _SentinelRenderer.material = _originalRedMaterial;
            }
            if(_SentinelRedRenderer2 != null)
            {
                _SentinelRedRenderer2.material = _originalRedMaterial;
            }
			_bRedLightOn = false;
		}
	}

    void ShowGreenLight(bool bShow)
    {
        if (bShow && !_bGreenLightOn)
        {
            if(DisplayGreenLight != null)
                DisplayGreenLight.SetActive(true);
            if (GreenLightRenderer != null)
            {
                GreenLightRenderer.material = SentinelGreenLightMaterial;
            }
            _bGreenLightOn = true;
        }
        else if (!bShow && _bGreenLightOn)
        {
            if (DisplayGreenLight != null)
                DisplayGreenLight.SetActive(false);
            if (GreenLightRenderer != null)
            {
                GreenLightRenderer.material = _originalGreenMaterial;
            }

            _bGreenLightOn = false;
        }
    }

    bool CheckLowPressureWarning()
	{
        //if (_sim.CheckLowPressure())
        if (_simData.LowPressure)
        {
            //SetIcon(SentinelIcons.OpenValve);
            _bAlarmIsSilent = false;
            _bLowPressureAlarmOn = true;
            //if (NetworkedObject != null && AlarmSync != null)
            //{
            //    if (NetworkedObject.HasAuthority)
            //    {
            //        AlarmSync.Value = false;
            //    }
            //}
            if (!_autoSilence)
            {
                _autoSilence = true;
                _silenceLowPressureTimer = Time.time + 30;
            }
            return true;
        }
        else
        {
            _autoSilence = false;
            //if (_sim.CheckAllClear())
            if (!_simData.LowPressure && !_simData.CriticalPressure)
            {
                _bCriticalPressureAlarmOn = false;
                _bLowPressureAlarmOn = false;
                _silenceLowPressureTimer = 0;
            }
            return false;
        }
	}

    bool CheckCriticalPressureWarning()
    {
        //if (_sim.CheckCriticalPressure())
        if (_simData.CriticalPressure)
        {
            _bAlarmIsSilent = false;            
            _bCriticalPressureAlarmOn = true;
            //if (NetworkedObject != null && AlarmSync != null)
            //{
            //    if (NetworkedObject.HasAuthority)
            //    {
            //        AlarmSync.Value = false;
            //    }
            //}
            return true;
        }
        else
        {
            //if (_sim.CheckAllClear())
            if (!_simData.LowPressure && !_simData.CriticalPressure)
            {
                _bCriticalPressureAlarmOn = false;
                _bLowPressureAlarmOn = false;
            }
            return false;
        }
    }

    public void SilenceAlarm()
    {
        Debug.Log("Trigger pressed on sentinel");
        if(_simData.AlarmState == VRNBG4AlarmState.CriticalPressureAlarm || _simData.AlarmState == VRNBG4AlarmState.LowPressureAlarm)
        {
           if(_bgSim != null)
            {
                _bgSim.SilenceAlarm();
            }
        }
        /*
        if (_bLowPressureAlarmOn || _bCriticalPressureAlarmOn)
        {
            _bAlarmIsSilent = true;
            
            //if(NetworkedObject != null && AlarmSync != null)
            //{
            //    if (NetworkedObject.HasAuthority)
            //    {
            //        AlarmSync.Value = true;
            //    }
            //}
            Debug.Log("Alarm should be silenced");
        }
        */
    }

	void AdvanceLeakTestStage()
	{
		_leakTestStage++;
		_transitionTime = Time.time;
	}

	void DisplayPressure(bool bShowTime = true)
	{
        var o2pressure = _simData.OxygenPressure;
        o2pressure /= 10;
        o2pressure *= 10;

        SetSmallText(o2pressure.ToString("F0"));
        //Debug.Log($"Sentinel pressure update: {o2pressure.ToString("F0")}");
        if (bShowTime)
        {
            //SetLargeText((_sim._oxygenPressure * 0.08f).ToString("F0"));
            SetLargeText(_simData.RemainingTime.ToString("F0"));
        }
        else
            SetLargeText("");

		float percent = (float)_simData.OxygenPressure / (float)_simData.BaselinePressure;
		if (percent > 1)
			percent = 1;

		SetBarGraph(percent);
	}

	bool BarGraphCountdown(float seconds)
	{
		bool bExpired = false;
		float percent = 1.0f - (Time.time - _transitionTime) / seconds;
		if (percent <= 0)
			bExpired = true;

		percent = Mathf.Clamp(percent, 0, 1);
		SetBarGraph(percent);

		return bExpired;
	}

	void SwitchState(SentinelState state)
	{
		switch (state)
		{
			case SentinelState.SelfTest:
				SetLargeText("888");
				SetSmallText("8888");
				//SetIcon(SentinelIcons.Checkmark);
				PlaySound(_AudioDoublePlusLongBeep);
				break;

			case SentinelState.BatteryTest:
				SetSmallText("");
				SetLargeText("900");
				SetBarGraph(1.0f);
				//SetIcon(SentinelIcons.BatteryGood);
				break;

			case SentinelState.HighPressureLeakTest:
				_leakTestStage = 0;
				//SetIcon(SentinelIcons.Clock);
				break;

			case SentinelState.Normal:
				//SetIcon(SentinelIcons.Clock);
				break;

			case SentinelState.NormalStartup:
				PlaySound(_AudioNormalStartup);
				break;

			case SentinelState.ErrorStartup:
				//SetIcon(SentinelIcons.Error);
				SrcStartupErrorBeeps.Play();
				break;

			case SentinelState.Off:
				DisplayOn(false);
				break;
		}

		if (state != SentinelState.Off)
			DisplayOn(true);

		_transitionTime = Time.time;
		_state = state;
	}

	public void SentinelOn(bool bOn)
	{
		if (bOn && _state == SentinelState.Off)
		{
			SwitchState(SentinelState.SelfTest);			
		}
		else if (!bOn)
		{
			SwitchState(SentinelState.Off);
		}
	}

	//public void SetIcon(SentinelIcons icon)
	//{
	//	switch (icon)
	//	{
	//		case SentinelIcons.BatteryGood:
	//			_icon.spriteName = "BatteryGood";
	//			break;

	//		case SentinelIcons.BatteryLow:
	//			_icon.spriteName = "BatteryLow";
	//			break;

	//		case SentinelIcons.Checkmark:
	//			_icon.spriteName = "Checkmark";
	//			break;

	//		case SentinelIcons.Clock:
	//			_icon.spriteName = "Clock";
	//			break;

	//		case SentinelIcons.CloseValve:
	//			_icon.spriteName = "CloseValve";
	//			break;

	//		case SentinelIcons.Error:
	//			_icon.spriteName = "Error";
	//			break;

	//		case SentinelIcons.Exclamation:
	//			_icon.spriteName = "Exclamation";
	//			break;

	//		case SentinelIcons.OpenValve:
	//			_icon.spriteName = "OpenValve";
	//			break;
	//	}

	//	if (icon == SentinelIcons.None)
	//		_icon.gameObject.SetActive(false);
	//	else
	//		_icon.gameObject.SetActive(true);
	//}

	public void SetLargeText(string text)
	{
		LargeText.text = text;
	}

	public void SetSmallText(string text)
	{
        //Debug.Log($"Setting small text: {text}");
        SmallText.text = text;
	}

	public void SetLabelText(string text)
	{
		LabelText.text = text;
	}

	public void SetBarGraph(float percent)
	{
		int cutoff = (int)Mathf.Floor(percent * (NUM_SEGMENTS - 1));

		for (int i = 0; i < NUM_SEGMENTS; i++)
		{
			if (i <= cutoff)
				DisplaySegments[i].SetActive(true);
			else
				DisplaySegments[i].SetActive(false);
		}
	}

    public bool NeedsUpdate()
    {
        return true;
    }

    //TODO: Change this to not use INetSync

    public void WriteObjState(CodedOutputStream writer)
    {
        //BinaryWriter bw = new BinaryWriter(writer);
        //bw.Write((Int32)_playerID);
        writer.WriteInt32(_playerID);
    }

    public void SyncObjState(CodedInputStream reader)
    {
        //BinaryReader br = new BinaryReader(reader);
        //_playerID = br.ReadInt32();
        _playerID = reader.ReadInt32();
    }


    public void OnJoystickPressed(Transform interactor, bool pressed)
    {

    }

    public void OnPrimaryButtonPressed(Transform interactor, bool pressed)
    {

    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {

    }

    public void OnPickedUp(Transform interactor)
    {

    }

    public void OnDropped(Transform interactor)
    {

    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        SilenceAlarm();
    }

    public void OnDeactivated(Transform interactor)
    {

    }
}
