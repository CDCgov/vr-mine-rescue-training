using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Google.Protobuf;
using System;
using UnityEngine.SceneManagement;

public class GasMeterDisplay : MonoBehaviour, IInteractableObject
{
    public const int GasMeterMaxCOReading = 9999;
    public const int GasMeterMaxH2SReading = 9999;

    public VentilationManager VentilationManager;
	//variable declaration
	public MineNetwork MineNetwork;
    public StaticMineAtmosphere DefaultAtmosphere;
	private MineAtmosphere _atmosphere;

	public TextMeshPro MethaneDisplay;
	public TextMeshPro CarbonMonoxideDisplay;
	public TextMeshPro HydrogenSulfideDisplay;
	public TextMeshPro OxygenDisplay;

    public AudioSource AlarmAudio;
    public GameObject AlarmLight;
    public GameObject VolumeOnObj;
    public GameObject VolumeOffObj;

    public bool EnableAlarm = true;

    private float _timeForAlarm = 0;
    private float _alarmDelay = 1f;
    private float _turnOffLightTime = 0;
    private float _turnOffLightDelay = 0.1f;

    private bool _bAlarmOn = false;
    private bool _bIsSilenced = false;

    private NetworkedObject _netObj;
    private bool _isDebrief = false;
    //private MineSceneConfiguration _sceneConfiguration;
    

	void Start()
	{
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        //if (VentilationManager != null)
        //    VentilationManager.ReceivedMineAtmosphere += OnReceivedMineAtmosphere;

		MineNetwork = GameObject.FindObjectOfType<MineNetwork>();        
		_atmosphere = new MineAtmosphere();

        _isDebrief = SceneManager.GetSceneByName("DebriefMainScene").isLoaded;

        //_sceneConfiguration = FindObjectOfType<MineSceneConfiguration>();
        //if(_sceneConfiguration != null)
        //{
        //    _bIsSilenced = _sceneConfiguration.SilenceAlarms;
        //}

        _bIsSilenced = !ScenarioSaveLoad.Settings.AlarmEnabled;

        if (_bIsSilenced)
        {
            if (VolumeOffObj != null && VolumeOnObj != null)
            {
                VolumeOffObj.SetActive(true);
                VolumeOnObj.SetActive(false);
            }
        }
        else
        {
            if (VolumeOffObj != null && VolumeOnObj != null)
            {
                VolumeOffObj.SetActive(false);
                VolumeOnObj.SetActive(true);
            }
        }

        if(TryGetComponent<NetworkedObject>(out _netObj))
        {
            _netObj.RegisterMessageHandler(OnSilence);
            if (!_netObj.HasAuthority)
            {
                if (VolumeOffObj != null && VolumeOnObj != null)
                {
                    VolumeOffObj.SetActive(false);
                    VolumeOnObj.SetActive(false);
                }
            }
        }

		InvokeRepeating("GasDisplayUpdate", 0.0f, 0.5f);
	}

    

    private void OnSilence(string messageType, CodedInputStream reader)
    {
        if(messageType == "SILENCE")
        {
            _bIsSilenced = true;
        }

        if(messageType == "SILENCE_OFF")
        {
            _bIsSilenced = false;
        }
    }

    private void Update()
    {
        if (_bAlarmOn && !_isDebrief)
        {
            if(Time.time > _timeForAlarm)
            {
                if (!_bIsSilenced)
                {
                    AlarmAudio.Play();
                }
                AlarmLight.SetActive(true);
                _timeForAlarm = Time.time + _alarmDelay;
                _turnOffLightTime = Time.time + _turnOffLightDelay;
            }

            if(AlarmLight.activeSelf && Time.time > _turnOffLightTime)
            {
                AlarmLight.SetActive(false);
            }
        }
        else
        {
            if (AlarmLight.activeSelf)
            {
                AlarmLight.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        //if (VentilationManager != null)
        //    VentilationManager.ReceivedMineAtmosphere -= OnReceivedMineAtmosphere;
    }

    private void OnReceivedMineAtmosphere(Vector3 worldPos, MineAtmosphere atm)
    {
        if (!gameObject)
            return;

        if (Vector3.Distance(worldPos, transform.position) < 2.0f)
        {
            _atmosphere = atm;
            UpdateTextFields();
        }
    }

    void GasDisplayUpdate()
	{

        if (VentilationManager != null)
        {
            //VentilationManager.RequestMineAtmosphere(transform.position);

            MineAtmosphere atm;
            if (VentilationManager.GetMineAtmosphere(transform.position, out atm))
            {
                _atmosphere = atm;
                UpdateTextFields();
            }
        }
	
    }

    void UpdateTextFields()
    {
        MethaneDisplay.text = _atmosphere.MethaneText;

        int co = (int)(Mathf.Round(_atmosphere.CarbonMonoxide * 1000000.0f));
        if (co > GasMeterMaxCOReading)
            co = GasMeterMaxCOReading;
        //CarbonMonoxideDisplay.text = _Atmosphere.CarbonMonoxide.ToString ("F1");

        if((co >= 50 || _atmosphere.Oxygen <= 0.194f || _atmosphere.Methane >= 0.01f) && EnableAlarm)
        {
            
            _bAlarmOn = true;
        }
        else
        {
            _bAlarmOn = false;
            //_bIsSilenced = false;
        }

        CarbonMonoxideDisplay.text = co.ToString();

        int h2s4 = (int)(Mathf.Round(_atmosphere.HydrogenSulfide * 1000000.0f));
        if(h2s4 > GasMeterMaxH2SReading)
        {
            h2s4 = GasMeterMaxH2SReading;
        }
        //HydrogenSulfideDisplay.text = _Atmosphere.HydrogenSulfide.ToString ("F2");
        HydrogenSulfideDisplay.text = h2s4.ToString();

        OxygenDisplay.text = _atmosphere.OxygenText;

    }

    public void SilenceGasMeter()
    {
        if (!ScenarioSaveLoad.Settings.AlarmEnabledAllowToggle)
            return;

        if (!_bIsSilenced)
        {
            _bIsSilenced = true;
            if (_netObj != null && _netObj.HasAuthority)
            {
                _netObj.SendMessage("SILENCE", new VRNTextMessage());
            }
            //if (AlarmLight != null)
            //{
            //    AlarmLight.SetActive(false);
            //}
            if (VolumeOffObj != null && VolumeOnObj != null)
            {
                VolumeOffObj.SetActive(true);
                VolumeOnObj.SetActive(false);
            }
        }
        else
        {
            _bIsSilenced = false;
            if (_netObj != null && _netObj.HasAuthority)
            {
                _netObj.SendMessage("SILENCE_OFF", new VRNTextMessage());
            }
            if (VolumeOffObj != null && VolumeOnObj != null)
            {
                VolumeOffObj.SetActive(false);
                VolumeOnObj.SetActive(true);
            }
        }
        
    }

    public MineAtmosphere GetAtmosphere()
    {        
        return _atmosphere;
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
        SilenceGasMeter();
    }

    public void OnDeactivated(Transform interactor)
    {

    }
}
