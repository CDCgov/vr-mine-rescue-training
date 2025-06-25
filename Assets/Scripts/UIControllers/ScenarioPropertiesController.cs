using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;
using System.Collections;
using NIOSH_MineCreation;

public class ScenarioPropertiesController : TabsController
{
    public SystemManager SystemManager;
    public LoadableAssetManager LoadableAssetManager;
    public POIManager POIManager;

    public List<GameObject> Contents = new List<GameObject>();
    public RectTransform TabsParent;
    public InspectorField UIScale;

    [Header("Scenario Tab")]
    public Toggle StretcherToggle;   
    public Toggle LinklineToggle;    
    public Toggle CalibrationToggle;    
    public InspectorField BG4Field;
    public InspectorField MasterVolume;
    public GridPropertiesManager GridProperties;
    public TMP_Dropdown ThirdPersonDropdown;
    public ToggleSwitch AlarmEnabled;
    public ToggleSwitch AlarmEnabledAllowToggle;
    public ToggleSwitch BlockCameraOutOfBoundsToggle;

    //initial values
    private bool _spawnStretcher = false;
    private bool _spawnLinkline = false;
    private bool _allowCalibrationToggle = false;
    private float _BG4FieldSaveVal = 20;
    private float _masterVol = 100;
    //private int _profileSelection = 0;
    //private bool _silenceAlarms = false;

    [Header("Tiles Tab")]
    public ToggleSwitch RoofBoltToggle;
    public SliderField RoofBoltSliderField;
    public SliderField RoofboltOffsetSliderField;
    public SliderField PillarWidthField;
    public SliderField PillarLengthField;
    public SliderField SeamHeightField;
    public SliderField EntryWidthField;
    public SliderField RockdustField;
    public ToggleSwitch CornerCurtainToggle;
    public NIOSH_MineCreation.MineBuilderUI MineCreator;

    //initial values
    private float _roofBoltSpacingValue;
    private float _roofBoltOffsetValue;
    private float _pillarWidthValue;
    private float _pillarLengthValue;
    private float _seamHeightValue;
    private float _entryWidthValue;
    private float _rockdustValue;
    private bool _boltsEnabled;

    private Toggle _RoofBoltToggleField;

    [Header("General Tab")]
    public Toggle CustomPathToggle;    
    public TextMeshProUGUI RootPathText;    
    public TextMeshProUGUI RootPathHeader;
    public TMP_Text ExternalAssetsPathText;
    public TMP_Text ScenariosPathText;
    public TMP_Text SessionLogsPathText;
    public Button ExternalAssetsButton;
    public Button ScenariosButton;
    public Button SessionLogsButton;
    public Button RootPathButton;
    public Button ContinueCustomRootButton;
    public Button CancelCustomRootButton;
    public GameObject CustomRootWarningBox;
    public TMP_Dropdown UnitDropdown;

    //private bool _unitDrowdownCancelCache;
    //private bool _customPathToggleCancelCache;
    //private string _rootPathTextCancelCache;

    [Header("Ventillation Tab")]
    public TMP_Dropdown VentDropdown;
    public List<TMP_InputField> StaticVentFields;
    public int StaticVentDropdownValue = 1;

    //private ComponentInfo_SceneVentillation _sceneVentilation;

    //initial values
    private float _oxygen = 0.208f;
    private float _methane = 0;
    private float _co = 0;
    private float _h2s = 0;
    private int _initMFireSelection = 0;

    public Color EnabledColor = Color.white;
    public Color DisabledColor = Color.gray;
    
    private SystemConfig _systemConfig;
    private StandaloneFileBrowserWindows _fileBrowser;
    private const string _rootSubFolder = @"\VRMine";
    private GameObject _spawnedStretcher;
    private GameObject _spawnedLinkline;
    //private ComponentInfo_MineSceneConfig _mineSceneConfiguration;
    //private LoadableAssetManager _loadableAssetManager;
    //private POIManager _poiManager;
    private Transform _playerSpawnPoint = null;
    private Canvas _canvas;

    private bool _buildMineFlag = false;
    //private bool _populatedTileTabValues = false;
    //private float _seamHeight = 0;
    //private float _pillarWidth = 0;
    //private float _pillarLength = 0;
    //private float _entryWidth = 0;
    //private float _boltSpacing = 0;
    //private float _boltRibOffset = 0;

    const float METERS_TO_FEET = 3.28084f;

    //OnEnabled Cache for Save vs Cancel?

    private bool _initialized = false;

    public class MinerProfileOptionData : TMP_Dropdown.OptionData
    {
        public string MinerProfileID;
    }


    protected override void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);
        if (POIManager == null)
            POIManager = POIManager.GetDefault(gameObject);

        _systemConfig = SystemManager.SystemConfig;
        _canvas = GetComponentInParent<Canvas>();

        base.Start();

        if (ThirdPersonDropdown.options == null)
            ThirdPersonDropdown.options = new List<TMP_Dropdown.OptionData>();

        ThirdPersonDropdown.options.Clear();

        foreach (var profile in LoadableAssetManager.GetAllMinerProfiles())
        {
            var opt = new MinerProfileOptionData
            {
                MinerProfileID = profile.MinerProfileID,
                text = profile.DisplayName,
            };
            ThirdPersonDropdown.options.Add(opt);
        }



        ///subscribe event listeners
        ScenarioSaveLoad.Instance.onLoadComplete += OnScenarioLoaded;
        //RoofBoltToggle.onToggleComplete.AddListener(delegate { RoofBoltSliderField.SetInteractableUI(RoofBoltToggle.GetToggleButtonState()); });
        //RoofBoltToggle.onToggleComplete.AddListener(delegate { RoofboltOffsetSliderField.SetInteractableUI(RoofBoltToggle.GetToggleButtonState()); });
        //RoofBoltToggle.onToggleComplete.AddListener(delegate { EnableRoofBolts(RoofBoltToggle.GetToggleButtonState()); });
        CustomPathToggle.onValueChanged.AddListener(delegate { SetRootPathInteractive(CustomPathToggle.isOn); });
        VentDropdown.onValueChanged.AddListener(delegate { SetStaticVentFieldInteractive(VentDropdown.value); });

        ExternalAssetsButton.onClick.AddListener(delegate { ViewFileLocation("External Assets", _systemConfig.ExternalAssetsFolder); });
        ScenariosButton.onClick.AddListener(delegate { ViewFileLocation("Scenarios", _systemConfig.ScenariosFolder); });
        SessionLogsButton.onClick.AddListener(delegate { ViewFileLocation("Session Logs", _systemConfig.SessionLogsFolder); });

        RootPathButton.onClick.AddListener(delegate { CustomRootWarningBox.SetActive(true); });
        CancelCustomRootButton.onClick.AddListener(delegate { CustomRootWarningBox.SetActive(false); });
        ContinueCustomRootButton.onClick.AddListener(SetRootFilePathCustom);
        //CalibrationToggle.onValueChanged.AddListener(delegate { SetCalibration(CalibrationToggle.isOn); });
        //StretcherToggle.onValueChanged.AddListener(delegate { ToggleStretcher(StretcherToggle.isOn); });
        //LinklineToggle.onValueChanged.AddListener(delegate { ToggleLinkline(LinklineToggle.isOn); });
        //BG4Field.onSubmitValue.AddListener(SetBG4);
        //MasterVolume.onSubmitValue.AddListener(SetMasterVolume);
        //UIScale.onSubmitValue.AddListener(SetUIScale);

        UnitDropdown.onValueChanged.AddListener((selValue) =>
        { 
            SetUnit(UnitDropdown.value == 0 ? MeasurementUnits.Metric : MeasurementUnits.Imperial); 
        });

        //RoofBoltSliderField.onSubmitValue.AddListener(SetBoltSpacing);
        //RoofboltOffsetSliderField.onSubmitValue.AddListener(SetBoltOffset);
        //CornerCurtainToggle.onToggleComplete.AddListener(delegate { SetCornerCurtainState(CornerCurtainToggle.GetToggleButtonState()); });
        //SilenceToggle.onValueChanged.AddListener(delegate { SetSilenceAlarms(SilenceToggle.isOn); });

        RockdustField.onSubmitValue.AddListener(SetRockDustLevel);

        //_mineSceneConfiguration = FindObjectOfType<ComponentInfo_MineSceneConfig>();
        //bool configFound = true;
        //if(_mineSceneConfiguration == null)
        //{
        //    configFound = false;
        //    GameObject config = LoadableAssetManager.InstantiateEditorAsset("SCENECONFIG");
        //    config.SetActive(true);
        //    Debug.Log("Loading config?");
        //    _mineSceneConfiguration = config.GetComponent<ComponentInfo_MineSceneConfig>();
        //}
        //List<TMP_Dropdown.OptionData> ThirdPersonOptions = new List<TMP_Dropdown.OptionData>();
        //for (int i = 0; i < _mineSceneConfiguration.MineProfiles.MineProfileList.Count; i++)
        //{
        //    ThirdPersonOptions.Add(new TMP_Dropdown.OptionData(_mineSceneConfiguration.MineProfiles.MineProfileList[i].DisplayName));
        //}
        //ThirdPersonDropdown.AddOptions(ThirdPersonOptions);
        //ThirdPersonDropdown.onValueChanged.AddListener(_mineSceneConfiguration.SetMinerProfile);
        //ThirdPersonDropdown.SetValueWithoutNotify(_mineSceneConfiguration.ProfileSelection);
        //_profileSelection = _mineSceneConfiguration.ProfileSelection;

        //if (configFound) 
        //{ 
        //    _sceneVentilation = FindObjectOfType<ComponentInfo_SceneVentillation>();
        //}
        //else
        //{
        //    _sceneVentilation = _mineSceneConfiguration.GetComponent<ComponentInfo_SceneVentillation>();
        //}

        //_oxygen = _sceneVentilation.StaticAtmosphere.Oxygen*100;
        //float co = _sceneVentilation.StaticAtmosphere.CarbonMonoxide * 1000000;
        //_co = Mathf.RoundToInt(co);
        //float h2s = _sceneVentilation.StaticAtmosphere.HydrogenSulfide * 1000000;
        //_h2s = Mathf.RoundToInt(h2s);

        //_methane = _sceneVentilation.StaticAtmosphere.Methane*100;
        //_initMFireSelection = _sceneVentilation.UseMFire ? 0 : 1;
        //VentDropdown.SetValueWithoutNotify(_initMFireSelection);

        //StaticVentFields[0].GetComponent<InspectorField>().SetDisplayedValue(_oxygen);
        //StaticVentFields[1].GetComponent<InspectorField>().SetDisplayedValue(_methane);
        //StaticVentFields[2].GetComponent<InspectorField>().SetDisplayedValue(_co);
        //StaticVentFields[3].GetComponent<InspectorField>().SetDisplayedValue(_h2s);

        //if (ScenarioSaveLoad.Instance != null && ScenarioSaveLoad.Instance.Settings != null)
        //{
        //    RockdustField.SetSliderValues(0, 100, ScenarioSaveLoad.Instance.Settings.RockDustLevel * 100.0f);
        //    _rockdustValue = ScenarioSaveLoad.Instance.Settings.RockDustLevel;
        //    Debug.Log($"Setting rock dust cache: {_rockdustValue}");
        //}




        //Debug.Log("Should have added listeners here");


        ///initialize UI element interactivity
        //SetRootPathInteractive(CustomPathToggle.isOn);
        //SetStaticVentFieldInteractive(VentDropdown.value);

        //_loadableAssetManager = LoadableAssetManager.GetDefault(null);
        //_poiManager = POIManager.GetDefault(null);

        //Better way to do this? -BDM



        //if (ScenarioSaveLoad.Instance != null && ScenarioSaveLoad.Instance.Settings != null)
        //{
        //    RockdustField.SetCurrentValue(ScenarioSaveLoad.Instance.Settings.RockDustLevel * 100.0f);
        //}        
        //UnitDropdown.SetValueWithoutNotify(MineCreator.unitToggle.GetToggleButtonState() ? 0 : 1);
        //Debug.Log($"Trying to set new length? {_pillarLength}");
        //Debug.Log($"Is metric? {MineCreator.unitToggle.GetToggleButtonState()}");

        //RoofBoltSliderField.ChangeUnitOfMeasure(MineCreator.unitToggle.GetToggleButtonState());
        //RoofboltOffsetSliderField.ChangeUnitOfMeasure(MineCreator.unitToggle.GetToggleButtonState());
        //PillarWidthField.ChangeUnitOfMeasure(MineCreator.unitToggle.GetToggleButtonState());
        //PillarLengthField.ChangeUnitOfMeasure(MineCreator.unitToggle.GetToggleButtonState());
        //SeamHeightField.ChangeUnitOfMeasure(MineCreator.unitToggle.GetToggleButtonState());
        //EntryWidthField.ChangeUnitOfMeasure(MineCreator.unitToggle.GetToggleButtonState());
        //if(GridProperties != null)
        //{
        //    GridProperties.SetUnit(MineCreator.unitToggle.GetToggleButtonState());
        //}


        //if (MineCreator.tileSet == MineSettings.TileSet.Stone)
        //{
        //    SeamHeightField.SetHashIncrement(5);
        //}
        //else
        //{
        //    SeamHeightField.SetHashIncrement(1);
        //}

        //Changing from unreliable mine creator settings to ScenarioSaveLoad that is now available
        //var mineTileSet = LoadableAssetManager.GetMineTileset(ScenarioSaveLoad.Instance.Settings.MineSettings.tileSet);
        //float convert = mineTileSet.UnitScale == UnitsEditor.Metric ? 1 : (1/METERS_TO_FEET);
        //SeamHeightField.SetSliderValues(mineTileSet.SeamHeightMinValue * convert, mineTileSet.SeamHeightMaxValue * convert, ScenarioSaveLoad.Instance.Settings.MineSettings.seamHeight);
        //PillarLengthField.SetSliderValues(mineTileSet.PillarLengthMinValue * convert, mineTileSet.PillarLengthMaxValue * convert, ScenarioSaveLoad.Instance.Settings.MineSettings.pillarLength);
        ////PillarLengthField.ForceValue(_pillarLength);
        //PillarWidthField.SetSliderValues(mineTileSet.PillarWidthMinValue * convert, mineTileSet.PillarWidthMaxValue * convert, ScenarioSaveLoad.Instance.Settings.MineSettings.pillarWidth);
        //EntryWidthField.SetSliderValues(mineTileSet.EntryWidthMinValue * convert, mineTileSet.EntryWidthMaxValue * convert, ScenarioSaveLoad.Instance.Settings.MineSettings.entryWidth);
        //Debug.Log($"Setting bolt options: min-{mineTileSet.BoltSpacingMinValue * convert}, max-{mineTileSet.BoltSpacingMaxValue * convert}");
        //RoofBoltSliderField.SetSliderValues(mineTileSet.BoltSpacingMinValue * convert, mineTileSet.BoltSpacingMaxValue * convert, ScenarioSaveLoad.Instance.Settings.MineSettings.BoltSpacing);
        //RoofboltOffsetSliderField.SetSliderValues(MineCreator.BoltRibOffset_SliderField.GetMin(), MineCreator.BoltRibOffset_SliderField.GetMax(), ScenarioSaveLoad.Instance.Settings.MineSettings.BoltRibOffset);


        //_RoofBoltToggleField = RoofBoltToggle.GetComponent<Toggle>();
        //if (MineCreator.tileSet == NIOSH_MineCreation.MineSettings.TileSet.Coal)
        //{
        //    //RoofBoltToggle.ToggleWithoutNotify(true);
        //    RoofBoltToggle.gameObject.SetActive(false);
        //    _boltsEnabled = true;
        //}
        //else
        //{
        //    _boltsEnabled = MineCreator.BoltsToggle.GetToggleButtonState();

        //    _RoofBoltToggleField.SetIsOnWithoutNotify(_boltsEnabled);
        //    RoofBoltSliderField.SetInteractableUI(_boltsEnabled);
        //    RoofboltOffsetSliderField.SetInteractableUI(_boltsEnabled);
        //}        
        //_roofBoltSpacingValue = ScenarioSaveLoad.Instance.Settings.MineSettings.BoltSpacing;        
        //_roofBoltOffsetValue = ScenarioSaveLoad.Instance.Settings.MineSettings.BoltRibOffset;

        //_spawnedStretcher = GameObject.FindGameObjectWithTag("Stretcher");
        //_spawnedLinkline = GameObject.FindGameObjectWithTag("LinkLine");

        //if (_spawnedStretcher != null)
        //{
        //    _spawnStretcher = true;
        //    StretcherToggle.SetIsOnWithoutNotify(true);
        //    if (StretcherToggle.gameObject.activeSelf)
        //    {
        //        StretcherToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_spawnStretcher);
        //    }
        //}
        //else
        //{
        //    _spawnStretcher = false;
        //    StretcherToggle.SetIsOnWithoutNotify(false);
        //    if (StretcherToggle.gameObject.activeSelf)
        //    {
        //        StretcherToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_spawnStretcher);
        //    }
        //}
        //if (_spawnedLinkline != null)
        //{
        //    _spawnLinkline = true;
        //    LinklineToggle.SetIsOnWithoutNotify(true);
        //    if (LinklineToggle.gameObject.activeSelf)
        //    {
        //        LinklineToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_spawnLinkline);
        //    }
        //}
        //else
        //{
        //    _spawnLinkline = false;
        //    LinklineToggle.SetIsOnWithoutNotify(false);
        //    if (LinklineToggle.gameObject.activeSelf)
        //    {
        //        LinklineToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_spawnLinkline);
        //    }
        //}

        //var settings = ScenarioSaveLoad.Settings;

        //CalibrationToggle.SetIsOnWithoutNotify(settings.AllowSelfCalibration);
        //_allowCalibrationToggle = settings.AllowSelfCalibration;
        //if (CalibrationToggle.gameObject.activeSelf)
        //{
        //    CalibrationToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_allowCalibrationToggle);
        //}

        //SilenceToggle.SetIsOnWithoutNotify(settings.SilenceAlarms);
        //_silenceAlarms = settings.SilenceAlarms;
        //if (SilenceToggle.gameObject.activeSelf)
        //{
        //    SilenceToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_silenceAlarms);
        //}

        //CornerCurtainToggle.ToggleControl.SetIsOnWithoutNotify(settings.MineSettings.EnableCornerCurtains);
        //if (CornerCurtainToggle.gameObject.activeSelf)
        //{
        //    CornerCurtainToggle.ToggleInstantly(settings.MineSettings.EnableCornerCurtains);
        //}

        //_BG4FieldSaveVal = _mineSceneConfiguration.BG4Duration;
        //BG4Field.SetDisplayedValue(_BG4FieldSaveVal);
        //_masterVol = _mineSceneConfiguration.MasterVolume;
        //MasterVolume.SetDisplayedValue(_masterVol);

        //Debug.Log($"End populate: {Time.time}");
        //_populatedTileTabValues = true;

        CaptureInitialValues();
        UpdateControls();

        _initialized = true;
    }

    private void OnScenarioLoaded()
    {
        CaptureInitialValues();
        //UpdateControls();
    }

    private void CaptureInitialValues()
    {
        var settings = ScenarioSaveLoad.Settings;

        _spawnedStretcher = GameObject.FindGameObjectWithTag("Stretcher");
        _spawnedLinkline = GameObject.FindGameObjectWithTag("LinkLine");

        _spawnStretcher = _spawnedStretcher != null ? true : false;
        _spawnLinkline = _spawnedLinkline != null ? true : false;

        _allowCalibrationToggle = settings.AllowSelfCalibration;
        _BG4FieldSaveVal = settings.BG4DurationMinutes;
        _masterVol = settings.MasterVolume;
        //private int _profileSelection = 0;
        //_silenceAlarms = settings.SilenceAlarms;

        _roofBoltSpacingValue = settings.MineSettings.BoltSpacing;
        _roofBoltOffsetValue = settings.MineSettings.BoltRibOffset;
        _pillarWidthValue = settings.MineSettings.pillarWidth;
        _pillarLengthValue = settings.MineSettings.pillarLength;
        _seamHeightValue = settings.MineSettings.seamHeight;

        _entryWidthValue = settings.MineSettings.entryWidth;
        _rockdustValue = settings.RockDustLevel;
        _boltsEnabled = settings.MineSettings.BoltSpacing > 0 ? true : false;

        //_initMFireSelection = settings.UseMFire ? 0 : 1;
        _oxygen = settings.StaticAtmosphere.Oxygen;
        _methane = settings.StaticAtmosphere.Methane;
        _h2s = settings.StaticAtmosphere.HydrogenSulfide;
        _co = settings.StaticAtmosphere.CarbonMonoxide;
    }

    private void RestoreInitialValues()
    {
        var settings = ScenarioSaveLoad.Settings;

        //_spawnStretcher = _spawnedStretcher != null ? true : false;
        //_spawnLinkline = _spawnedLinkline != null ? true : false;

        settings.AllowSelfCalibration = _allowCalibrationToggle;
        settings.BG4DurationMinutes = _BG4FieldSaveVal;
        settings.MasterVolume = _masterVol;
        //settings.SilenceAlarms = _silenceAlarms;

        settings.MineSettings.BoltSpacing = _roofBoltSpacingValue;
        settings.MineSettings.BoltRibOffset = _roofBoltOffsetValue;
        settings.MineSettings.pillarWidth = _pillarWidthValue;
        settings.MineSettings.pillarLength = _pillarLengthValue;
        settings.MineSettings.seamHeight = _seamHeightValue;

        settings.MineSettings.entryWidth = _entryWidthValue;
        settings.RockDustLevel = _rockdustValue;
        _boltsEnabled = settings.MineSettings.BoltSpacing > 0 ? true : false;

        //settings.UseMFire = _initMFireSelection == 1 ? true : false;
        settings.StaticAtmosphere.Oxygen = _oxygen;
        settings.StaticAtmosphere.Methane = _methane;
        settings.StaticAtmosphere.HydrogenSulfide = _h2s;
        settings.StaticAtmosphere.CarbonMonoxide = _co;
        GridProperties.RevertParameters();
    }

    void UpdateControls()
    {
        var settings = ScenarioSaveLoad.Settings;

        SetUnit(_systemConfig.DisplayUnits);

        bool customPath = _systemConfig.RootDataFolderOverride == null ? false : true;
        CustomPathToggle.SetIsOnWithoutNotify(customPath);
        SetRootPathInteractive(customPath);

        UIScale.SetDisplayedValue(_systemConfig.UIScale);
        UnitDropdown.SetValueWithoutNotify(MineCreator.unitToggle.GetToggleButtonState() ? 0 : 1);

        AlarmEnabled.ToggleInstantly(settings.AlarmEnabled);
        AlarmEnabledAllowToggle.ToggleInstantly(settings.AlarmEnabledAllowToggle);
        BlockCameraOutOfBoundsToggle.ToggleInstantly(settings.BlockCameraOutOfBounds);


        if (MineCreator.tileSet == MineSettings.TileSet.Stone)
        {
            SeamHeightField.SetHashIncrement(5);
        }
        else
        {
            SeamHeightField.SetHashIncrement(1);
        }

        int selMinerProfileIndex = 0;
        for (int i = 0; i < ThirdPersonDropdown.options.Count; i++)
        {
            var opt = (MinerProfileOptionData)ThirdPersonDropdown.options[i];
            if (opt.MinerProfileID == settings.MinerProfileID)
            {
                selMinerProfileIndex = i;
                break;
            }
        }
        ThirdPersonDropdown.SetValueWithoutNotify(selMinerProfileIndex);
        ThirdPersonDropdown.RefreshShownValue();

        //_mineSceneConfiguration = FindObjectOfType<ComponentInfo_MineSceneConfig>();
        //ThirdPersonDropdown.SetValueWithoutNotify(_mineSceneConfiguration.ProfileSelection);
        //_profileSelection = _mineSceneConfiguration.ProfileSelection;

        //_sceneVentilation = FindObjectOfType<ComponentInfo_SceneVentillation>();        
        //float co = _sceneVentilation.StaticAtmosphere.CarbonMonoxide * 1000000;
        //_co = Mathf.RoundToInt(co);
        //float h2s = _sceneVentilation.StaticAtmosphere.HydrogenSulfide * 1000000;
        //_h2s = Mathf.RoundToInt(h2s);

        //_methane = _sceneVentilation.StaticAtmosphere.Methane * 100;
        //_initMFireSelection = _sceneVentilation.UseMFire ? 0 : 1;

        var mfireSelection = settings.UseMFire ? 0 : 1;
        VentDropdown.SetValueWithoutNotify(mfireSelection);
        SetStaticVentFieldInteractive(mfireSelection);

        StaticVentFields[0].GetComponent<InspectorField>().SetDisplayedValue(settings.StaticAtmosphere.Oxygen * 100.0f);
        StaticVentFields[1].GetComponent<InspectorField>().SetDisplayedValue(settings.StaticAtmosphere.Methane * 100.0f);
        StaticVentFields[2].GetComponent<InspectorField>().SetDisplayedValue(settings.StaticAtmosphere.CarbonMonoxide * 1000000.0f);
        StaticVentFields[3].GetComponent<InspectorField>().SetDisplayedValue(settings.StaticAtmosphere.HydrogenSulfide * 1000000.0f);

        RockdustField.SetSliderValues(0, 100, settings.RockDustLevel * 100.0f);        

        var mineTileSet = LoadableAssetManager.GetMineTileset(settings.MineSettings.tileSet);
        float convert = mineTileSet.UnitScale == UnitsEditor.Metric ? 1 : (1/METERS_TO_FEET);
        SeamHeightField.SetSliderValues(mineTileSet.SeamHeightMinValue * convert, mineTileSet.SeamHeightMaxValue * convert, settings.MineSettings.seamHeight);
        PillarLengthField.SetSliderValues(mineTileSet.PillarLengthMinValue * convert, mineTileSet.PillarLengthMaxValue * convert, settings.MineSettings.pillarLength);
        //PillarLengthField.ForceValue(_pillarLength);
        PillarWidthField.SetSliderValues(mineTileSet.PillarWidthMinValue * convert, mineTileSet.PillarWidthMaxValue * convert, settings.MineSettings.pillarWidth);
        EntryWidthField.SetSliderValues(mineTileSet.EntryWidthMinValue * convert, mineTileSet.EntryWidthMaxValue * convert, settings.MineSettings.entryWidth);
        //Debug.Log($"Setting bolt options: min-{mineTileSet.BoltSpacingMinValue * convert}, max-{mineTileSet.BoltSpacingMaxValue * convert}");
        RoofBoltSliderField.SetSliderValues(mineTileSet.BoltSpacingMinValue * convert, mineTileSet.BoltSpacingMaxValue * convert, settings.MineSettings.BoltSpacing);
        RoofboltOffsetSliderField.SetSliderValues(MineCreator.BoltRibOffset_SliderField.GetMin(), MineCreator.BoltRibOffset_SliderField.GetMax(), settings.MineSettings.BoltRibOffset);

        _RoofBoltToggleField = RoofBoltToggle.GetComponent<Toggle>();
        //if (MineCreator.tileSet == NIOSH_MineCreation.MineSettings.TileSet.Coal)
        //{
        //    //RoofBoltToggle.ToggleWithoutNotify(true);
        //    RoofBoltToggle.gameObject.SetActive(false);
        //    _boltsEnabled = true;
        //}
        //else
        //{
        //    _boltsEnabled = MineCreator.BoltsToggle.GetToggleButtonState();
        //    //RoofBoltToggle.GetComponent<Toggle>().isOn = _boltsEnabled;
        //    //RoofBoltToggle.Toggle(_boltsEnabled);
        //    //RoofBoltToggle.ToggleWithoutNotify(_boltsEnabled);
        //    _RoofBoltToggleField.SetIsOnWithoutNotify(_boltsEnabled);
        //    RoofBoltSliderField.SetInteractableUI(_boltsEnabled);
        //    RoofboltOffsetSliderField.SetInteractableUI(_boltsEnabled);
        //}

        var boltsEnabled = settings.MineSettings.BoltSpacing > 0 ? true : false;
        RoofBoltToggle.gameObject.SetActive(true);
        //RoofBoltToggle.ToggleWithoutNotify(boltsEnabled);        
        _RoofBoltToggleField.SetIsOnWithoutNotify(boltsEnabled);
        RoofBoltSliderField.SetInteractableUI(boltsEnabled);
        RoofboltOffsetSliderField.SetInteractableUI(boltsEnabled);

        //_roofBoltSpacingValue = ScenarioSaveLoad.Instance.Settings.MineSettings.BoltSpacing;
        //_roofBoltOffsetValue = ScenarioSaveLoad.Instance.Settings.MineSettings.BoltRibOffset;

        _spawnedStretcher = GameObject.FindGameObjectWithTag("Stretcher");
        _spawnedLinkline = GameObject.FindGameObjectWithTag("LinkLine");

        StretcherToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_spawnedStretcher != null ? true : false);
        LinklineToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_spawnedLinkline != null ? true : false);

        //if (_spawnedStretcher != null)
        //{
        //    _spawnStretcher = true;
        //    StretcherToggle.SetIsOnWithoutNotify(true);
        //    if (StretcherToggle.gameObject.activeSelf)
        //    {
        //        StretcherToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_spawnStretcher);
        //    }
        //}
        //else
        //{
        //    _spawnStretcher = false;
        //    StretcherToggle.SetIsOnWithoutNotify(false);
        //    if (StretcherToggle.gameObject.activeSelf)
        //    {
        //        StretcherToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_spawnStretcher);
        //    }
        //}
        //if (_spawnedLinkline != null)
        //{
        //    _spawnLinkline = true;
        //    LinklineToggle.SetIsOnWithoutNotify(true);
        //    if (LinklineToggle.gameObject.activeSelf)
        //    {
        //        LinklineToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_spawnLinkline);
        //    }
        //}
        //else
        //{
        //    _spawnLinkline = false;
        //    LinklineToggle.SetIsOnWithoutNotify(false);
        //    if (LinklineToggle.gameObject.activeSelf)
        //    {
        //        LinklineToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_spawnLinkline);
        //    }
        //}

        //if (_mineSceneConfiguration != null && _mineSceneConfiguration.AllowSelfCalibration)
        //{
        //    CalibrationToggle.SetIsOnWithoutNotify(_mineSceneConfiguration.AllowSelfCalibration);
        //    _allowCalibrationToggle = _mineSceneConfiguration.AllowSelfCalibration;
        //    if (CalibrationToggle.gameObject.activeSelf)
        //    {
        //        CalibrationToggle.GetComponent<ToggleSwitch>().ToggleInstantly(_allowCalibrationToggle);
        //    }
        //}

        CalibrationToggle.GetComponent<ToggleSwitch>().ToggleInstantly(settings.AllowSelfCalibration);

        //CornerCurtainToggle.ToggleControl.SetIsOnWithoutNotify(ScenarioSaveLoad.Instance.Settings.MineSettings.EnableCornerCurtains);
        //if (CornerCurtainToggle.gameObject.activeSelf)
        //{
        //    CornerCurtainToggle.ToggleInstantly(ScenarioSaveLoad.Instance.Settings.MineSettings.EnableCornerCurtains);
        //}

        CornerCurtainToggle.ToggleInstantly(settings.MineSettings.EnableCornerCurtains);

        //_BG4FieldSaveVal = _mineSceneConfiguration.BG4Duration;
        BG4Field.SetDisplayedValue(settings.BG4DurationMinutes);
        //_masterVol = _mineSceneConfiguration.MasterVolume;
        MasterVolume.SetDisplayedValue(settings.MasterVolume * 100.0f);
    }

    private void UpdateFilePathDisplays()
    {
        if (ExternalAssetsPathText == null || ScenariosPathText == null || SessionLogsPathText == null ||
            RootPathText == null || _systemConfig == null)
            return;

        SetPathText(ExternalAssetsPathText, _systemConfig.ExternalAssetsFolder);
        SetPathText(ScenariosPathText, _systemConfig.ScenariosFolder);
        SetPathText(SessionLogsPathText, _systemConfig.SessionLogsFolder);

        SetPathText(RootPathText, _systemConfig.RootDataFolder);
    }

    private void SetPathText(TMP_Text textObj, string text)
    {
        if (textObj == null)
            return;

        if (text == null)
            text = "";

        textObj.text = text;

        if (textObj.TryGetComponent<MenuTooltip>(out var menuTooltip))
        {
            menuTooltip.SetTooltipText(text);
        }

    }

    

    private void OnDestroy()
    {
        ///unsubscribe event listeners
        //RoofBoltToggle.onToggleComplete.RemoveListener(delegate { RoofBoltSliderField.SetInteractableUI(RoofBoltToggle.GetToggleButtonState()); });
        //RoofBoltToggle.onToggleComplete.RemoveListener(delegate { RoofboltOffsetSliderField.SetInteractableUI(RoofBoltToggle.GetToggleButtonState()); });
        //RoofBoltToggle.onToggleComplete.RemoveListener(delegate { EnableRoofBolts(RoofBoltToggle.GetToggleButtonState()); });
        CustomPathToggle.onValueChanged.RemoveListener(delegate { SetRootPathInteractive(CustomPathToggle.isOn); });
        VentDropdown.onValueChanged.RemoveListener(delegate { SetStaticVentFieldInteractive(VentDropdown.value); });

        ExternalAssetsButton.onClick.RemoveListener(delegate { ViewFileLocation("External Assets", _systemConfig.ExternalAssetsFolder); });
        ScenariosButton.onClick.RemoveListener(delegate { ViewFileLocation("Scenarios", _systemConfig.ScenariosFolder); });
        SessionLogsButton.onClick.RemoveListener(delegate { ViewFileLocation("Session Logs", _systemConfig.SessionLogsFolder); });

        RootPathButton.onClick.RemoveListener(delegate { CustomRootWarningBox.SetActive(true); });
        CancelCustomRootButton.onClick.RemoveListener(delegate { CustomRootWarningBox.SetActive(false); });
        ContinueCustomRootButton.onClick.RemoveListener(SetRootFilePathCustom);
        //CalibrationToggle.onValueChanged.RemoveListener(delegate { SetCalibration(CalibrationToggle.isOn); });
        //StretcherToggle.onValueChanged.RemoveListener(delegate { ToggleStretcher(StretcherToggle.isOn); });
        //LinklineToggle.onValueChanged.RemoveListener(delegate { ToggleLinkline(LinklineToggle.isOn); });
        //BG4Field.onSubmitValue.RemoveListener(SetBG4);
        //MasterVolume.onSubmitValue.RemoveListener(SetMasterVolume);
        //UIScale.onSubmitValue.RemoveListener(SetUIScale);
        UnitDropdown.onValueChanged.RemoveAllListeners();
        RockdustField.onSubmitValue.RemoveListener(SetRockDustLevel);
        //RoofBoltSliderField.onSubmitValue.RemoveListener(SetBoltSpacing);
        //RoofboltOffsetSliderField.onSubmitValue.RemoveListener(SetBoltOffset);
        //ThirdPersonDropdown.onValueChanged.RemoveListener(_mineSceneConfiguration.SetMinerProfile);
        //CornerCurtainToggle.onToggleComplete.RemoveListener(delegate { SetCornerCurtainState(CornerCurtainToggle.GetToggleButtonState()); });
        //SilenceToggle.onValueChanged.RemoveListener(delegate { SetSilenceAlarms(SilenceToggle.isOn); });
    }

    protected override void ChangeTab(int newTab)
    {
        base.ChangeTab(newTab);
        //foreach (GameObject go in Contents)
        //{
        //    go.SetActive(false);
        //}

        for (int i = 0; i < Contents.Count; i++)
        {
            if (i != newTab)
            {
                Contents[i].SetActive(false);
            }
        }
        Contents[newTab].SetActive(true);
    }

    private void SetUnit(MeasurementUnits measurementUnit)
    {
        bool useMetric = measurementUnit == MeasurementUnits.Metric;

        RoofBoltSliderField.ChangeUnitOfMeasure(useMetric);
        RoofboltOffsetSliderField.ChangeUnitOfMeasure(useMetric);
        PillarWidthField.ChangeUnitOfMeasure(useMetric);
        PillarLengthField.ChangeUnitOfMeasure(useMetric);        
        SeamHeightField.ChangeUnitOfMeasure(useMetric);
        EntryWidthField.ChangeUnitOfMeasure(useMetric);

        //if (MineCreator.tileSet == MineSettings.TileSet.Stone)
        //{
        //    //Debug.Log($"Setting seam height hash marks?");
        //    SeamHeightField.SetHashIncrement(1);
        //}
        //else
        //{
        //    SeamHeightField.SetHashIncrement(5);
        //}
        //SeamHeightField.UpdateHashMarks();

        if(GridProperties != null)
        {
            GridProperties.SetUnit(useMetric);
        }
    }

    //private void SetCalibration(bool canCalibrate)
    //{
    //    if(_mineSceneConfiguration == null)
    //    {
    //        _mineSceneConfiguration = FindObjectOfType<ComponentInfo_MineSceneConfig>();
    //    }

    //    if(_mineSceneConfiguration != null)
    //        _mineSceneConfiguration.SetAllowCalibration(canCalibrate);
    //}

    //private void SetSilenceAlarms(bool silence)
    //{
    //    if (_mineSceneConfiguration == null)
    //    {
    //        _mineSceneConfiguration = FindObjectOfType<ComponentInfo_MineSceneConfig>();
    //    }

    //    if (_mineSceneConfiguration != null)
    //        _mineSceneConfiguration.SetSilenceAlarms(silence);
    //}

    private void ToggleStretcher(bool useStretcher)
    {
        if (useStretcher)
        {
            Vector3 spawnPosition = Vector3.zero;
            if (_playerSpawnPoint == null)
            {
                foreach (var item in POIManager.ActivePOIs)
                {
                    if(item.POIType == POIType.SpawnPoint)
                    {
                        _playerSpawnPoint = item.transform;
                        spawnPosition = _playerSpawnPoint.position;
                    }
                }
            }
            else
            {
                spawnPosition = _playerSpawnPoint.position;
            }

            _spawnedStretcher = LoadableAssetManager.InstantiateEditorAsset("STRETCHER_FULL");
            Debug.Log($"Spawning stretcher");
            
            PlacablePrefab spawnedPrefab = _spawnedStretcher.GetComponent<PlacablePrefab>();

            if (spawnedPrefab == null)
            {
                spawnedPrefab = _spawnedStretcher.AddComponent<PlacablePrefab>();
            }

            _spawnedStretcher.SetActive(true);
            _spawnedStretcher.transform.position = spawnPosition;
            _spawnedStretcher.transform.parent = _playerSpawnPoint;
        }
        else
        {
            if(_spawnedStretcher != null)
                Destroy(_spawnedStretcher);
        }
        
    }

    private void ToggleLinkline(bool useLinkline)
    {
        if (useLinkline)
        {
            Vector3 spawnPosition = Vector3.zero;
            if (_playerSpawnPoint == null)
            {
                foreach (var item in POIManager.ActivePOIs)
                {
                    if (item.POIType == POIType.SpawnPoint)
                    {
                        _playerSpawnPoint = item.transform;
                        spawnPosition = _playerSpawnPoint.position;
                    }
                }
            }
            else
            {
                spawnPosition = _playerSpawnPoint.position;
            }

            _spawnedLinkline = LoadableAssetManager.InstantiateEditorAsset("LINK_LINE");
            
            PlacablePrefab spawnedPrefab = _spawnedLinkline.GetComponent<PlacablePrefab>();

            if (spawnedPrefab == null)
            {
                spawnedPrefab = _spawnedLinkline.AddComponent<PlacablePrefab>();
            }

            _spawnedLinkline.SetActive(true);
            _spawnedLinkline.transform.position = spawnPosition;
            _spawnedLinkline.transform.parent = _playerSpawnPoint;
        }
        else
        {
            if(_spawnedLinkline != null)
                Destroy(_spawnedLinkline);
        }
    }

    //private void SetBG4(float duration, bool onsubmit)
    //{
    //    if (_mineSceneConfiguration == null)
    //    {
    //        _mineSceneConfiguration = FindObjectOfType<ComponentInfo_MineSceneConfig>();
    //    }

    //    if (_mineSceneConfiguration != null)
    //        _mineSceneConfiguration.SetBG4(duration);
    //}

    private void SetUIScale(float scale, bool onSubmit)
    {
        Debug.Log($"Setting UI scale at time {Time.time}");
        if(_canvas == null)
        {
            return;
        }
        //_canvas.scaleFactor = scale;
    }

    //private void SetMasterVolume(float vol, bool onSubmit)
    //{
    //    if (_mineSceneConfiguration == null)
    //    {
    //        _mineSceneConfiguration = FindObjectOfType<ComponentInfo_MineSceneConfig>();
    //    }

    //    if (_mineSceneConfiguration != null)
    //        _mineSceneConfiguration.SetMasterVolume(vol);
    //}

    private void SetRootPathInteractive(bool isInteractable)
    {
        RootPathHeader.color = isInteractable ? EnabledColor: DisabledColor;
        RootPathText.color = isInteractable ? EnabledColor : DisabledColor;
        RootPathButton.interactable = isInteractable;

        if (!isInteractable)
            SetRootPathToDefault();

        UpdateFilePathDisplays();
    }

    private void SetStaticVentFieldInteractive(int value)
    {
        bool isInteractable = (value == StaticVentDropdownValue);
        foreach (TMP_InputField field in StaticVentFields)
        {
            field.interactable = isInteractable;
        }
    }

    public void ViewFileLocation(string windowTitle, string directory)
    {
        _fileBrowser = new StandaloneFileBrowserWindows();
        _fileBrowser.OpenFolderPanel(windowTitle, directory, false);
    }

    public void SetRootFilePathCustom()
    {
        _fileBrowser = new StandaloneFileBrowserWindows();
        string[] paths = _fileBrowser.OpenFolderPanel("Root Path", _systemConfig.RootDataFolder, false);
        
        if (paths.Length > 0)
        {
            string text = paths[0];
            ///remove subfolder ending if selected by user as root to prevent nested sub folders
            //if (text.EndsWith(_rootSubFolder))
            //{
            //    int subFolderLength = _rootSubFolder.Length;
            //    text = text.Remove(text.Length -subFolderLength, subFolderLength);

            //}
            _systemConfig.RootDataFolderOverride = text;
        }

        RootPathText.text = _systemConfig.RootDataFolder;
        CustomRootWarningBox.SetActive(false);
        //To Do : Prompt Save and Restart Application

        UpdateFilePathDisplays();
    }

    private void SetRootPathToDefault()
    {
        _systemConfig.RootDataFolderOverride = null;
        RootPathText.text = _systemConfig.RootDataFolder;

        UpdateFilePathDisplays();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //if(_spawnedStretcher == null && StretcherToggle.isOn)
        //{
        //    StretcherToggle.isOn = false;
        //    _spawnStretcher = false;
        //}
        //if (_spawnedLinkline == null && LinklineToggle.isOn)
        //{
        //    LinklineToggle.isOn = false;
        //    _spawnLinkline = false;
        //}

        if (!_initialized)
            return;

        CaptureInitialValues();
        UpdateControls();
        
    }

    public void ProcessSave()
    {
        var settings = ScenarioSaveLoad.Settings;
        //Debug.Log($"Processing save");

        _spawnedStretcher = GameObject.FindGameObjectWithTag("Stretcher");
        _spawnedLinkline = GameObject.FindGameObjectWithTag("LinkLine");

        var stretcherSpawned = _spawnedStretcher != null ? true : false;
        var linklineSpawned = _spawnedLinkline != null ? true : false;

        if (linklineSpawned != LinklineToggle.isOn)
        {
            ToggleLinkline(LinklineToggle.isOn);
        }
        if (stretcherSpawned != StretcherToggle.isOn)
        {
            ToggleStretcher(StretcherToggle.isOn);
        }

        //if (_spawnLinkline != LinklineToggle.isOn)
        //{
        //    _spawnLinkline = LinklineToggle.isOn;
        //}
        //if (_spawnStretcher != StretcherToggle.isOn)
        //{
        //    _spawnStretcher = StretcherToggle.isOn;
        //}
        //if (_allowCalibrationToggle != CalibrationToggle.isOn)
        //{
        //    _allowCalibrationToggle = CalibrationToggle.isOn;
        //}
        _systemConfig.UIScale = Mathf.Clamp(UIScale.GetCurrentValue(), 0.5f, 3f);
        _systemConfig.DisplayUnits = (MeasurementUnits)(UnitDropdown.value + 1);

        //_rockdustValue = ScenarioSaveLoad.Instance.Settings.RockDustLevel;

        //_roofBoltSpacingValue = ScenarioSaveLoad.Instance.Settings.MineSettings.BoltSpacing;
        //_roofBoltOffsetValue = ScenarioSaveLoad.Instance.Settings.MineSettings.BoltRibOffset;

        var minerProfileIndex = ThirdPersonDropdown.value;
        if (minerProfileIndex >= 0 && minerProfileIndex < ThirdPersonDropdown.options.Count)
        {
            var minerProfile = ThirdPersonDropdown.options[minerProfileIndex] as MinerProfileOptionData;
            if (minerProfile != null)
            {
                settings.MinerProfileID = minerProfile.MinerProfileID;
                Debug.Log($"ScenarioPropertiesController: Changed miner profile ID to {minerProfile.MinerProfileID}");
            }
        }

        if (GridProperties != null)
        {
            GridProperties.SaveParameters();
        }

        settings.UseMFire = VentDropdown.value == 0 ? true : false;
        if (float.TryParse(StaticVentFields[0].text, out var oxygen))
            settings.StaticAtmosphere.Oxygen = oxygen / 100.0f;

        if (float.TryParse(StaticVentFields[1].text, out var methane))
            settings.StaticAtmosphere.Methane = methane / 100.0f;

        if (int.TryParse(StaticVentFields[2].text, out var co))
            settings.StaticAtmosphere.CarbonMonoxide = ((float)co) / 1000000.0f;

        if (int.TryParse(StaticVentFields[3].text, out var h2s))
            settings.StaticAtmosphere.HydrogenSulfide = ((float)h2s) / 1000000.0f;

        //Debug.Log($"Saving static vent values: O2 - {_oxygen}, methane - {_methane}, CO = {_co}, H2S - {_h2s}");
        //MineAtmosphere mineAtmo = new MineAtmosphere();
        //mineAtmo.Oxygen = _oxygen/100;
        //mineAtmo.Methane = _methane/100;
        //mineAtmo.CarbonMonoxide = (float)_co / 1000000;
        //mineAtmo.HydrogenSulfide = (float)_h2s / 1000000;
        //_sceneVentilation.StaticAtmosphere = mineAtmo;
        //_initMFireSelection = VentDropdown.value;

        _RoofBoltToggleField = RoofBoltToggle.GetComponent<Toggle>();
        if (_RoofBoltToggleField.isOn)
        {
            settings.MineSettings.BoltSpacing = RoofBoltSliderField.GetCurrentValue();
            settings.MineSettings.BoltRibOffset = RoofboltOffsetSliderField.GetCurrentValue();
        }
        else
            settings.MineSettings.BoltSpacing = 0;

        settings.AllowSelfCalibration = CalibrationToggle.isOn;
        settings.MineSettings.EnableCornerCurtains = CornerCurtainToggle.ToggleControl.isOn;
        settings.BG4DurationMinutes = BG4Field.GetCurrentValue();
        settings.MasterVolume = Mathf.Clamp01(MasterVolume.GetCurrentValue() / 100.0f);

        settings.AlarmEnabled = AlarmEnabled.ToggleControl.isOn;
        settings.AlarmEnabledAllowToggle = AlarmEnabledAllowToggle.ToggleControl.isOn;
        settings.BlockCameraOutOfBounds = BlockCameraOutOfBoundsToggle.ToggleControl.isOn;

        //float bg4Val;
        //float masterVol;
        //float.TryParse(BG4Field.GetComponent<TMP_InputField>().text, out bg4Val);
        //float.TryParse(MasterVolume.GetComponent<TMP_InputField>().text, out masterVol);
        //SetBG4(bg4Val, true);
        //SetMasterVolume(masterVol, true);
        //_BG4FieldSaveVal = bg4Val;
        //_masterVol = masterVol;

        //systemConfig.UIScale = UIScale.GetCurrentValue();
        //if (_RoofBoltToggleField != null)
        //{
        //    _boltsEnabled = _RoofBoltToggleField.isOn;
        //}
        //_profileSelection = ThirdPersonDropdown.value;

        _systemConfig.SaveConfig();
        CaptureInitialValues();
        ScenarioSaveLoad.Instance.RaiseMineSettingsChanged();

        gameObject.SetActive(false);
    }

    public void ProcessCancel()
    {
        var settings = ScenarioSaveLoad.Settings;
        //Debug.Log($"Processing cancel");
        //if (_spawnLinkline != LinklineToggle.isOn)
        //{
        //    //ToggleSwitch llToggleSwitch = LinklineToggle.GetComponent<ToggleSwitch>();
        //    //llToggleSwitch.GetToggleControl().SetIsOnWithoutNotify(_spawnLinkline);
        //    ToggleLinkline(_spawnLinkline);
            
        //}
        //if (_spawnStretcher != StretcherToggle.isOn)
        //{
        //    //ToggleSwitch stretchToggleSwitch = StretcherToggle.GetComponent<ToggleSwitch>();
        //    //stretchToggleSwitch.GetToggleControl().SetIsOnWithoutNotify(_spawnStretcher);
        //    ToggleStretcher(_spawnStretcher);
        //}

        //if(_allowCalibrationToggle != CalibrationToggle.isOn)
        //{
        //    ToggleSwitch calToggle = CalibrationToggle.GetComponent<ToggleSwitch>();
        //    calToggle.GetToggleControl().SetIsOnWithoutNotify(_allowCalibrationToggle);
        //    SetCalibration(_allowCalibrationToggle);
        //}

        //if(_silenceAlarms != SilenceToggle.isOn)
        //{
        //    ToggleSwitch silToggle = SilenceToggle.GetComponent<ToggleSwitch>();
        //    silToggle.GetToggleControl().SetIsOnWithoutNotify(_silenceAlarms);
        //    SetSilenceAlarms(_silenceAlarms);
        //}
        //UIScale.SetDisplayedValue(_systemConfig.UIScale);
        //SetUIScale(_systemConfig.UIScale, true);
        //SetUnit(_systemConfig.DisplayUnits);
        //UnitDropdown.SetValueWithoutNotify(_systemConfig.DisplayUnits == MeasurementUnits.Metric ? 0 : 1);

        ////if(RoofBoltSliderField.GetCurrentValue() != _boltSpacing)
        ////    RoofBoltSliderField.ForceValue(_roofBoltSpacingValue);
        ////if(RoofboltOffsetSliderField.GetCurrentValue() != _boltRibOffset)
        ////    RoofboltOffsetSliderField.ForceValue(_roofBoltOffsetValue);

        //SetBoltSpacing(_roofBoltSpacingValue, true);
        //SetBoltOffset(_roofBoltOffsetValue, true);
        ////RoofBoltToggle.ToggleInstantly(_boltsEnabled);
        //RoofBoltToggle.GetToggleControl().SetIsOnWithoutNotify(_boltsEnabled);
        //EnableRoofBolts(_boltsEnabled);

        //RockdustField.ForceValue(_rockdustValue * 100.0f);
        //if(ScenarioSaveLoad.Instance.Settings.RockDustLevel != _rockdustValue)
        //    ScenarioSaveLoad.Instance.SetRockDustLevel(_rockdustValue);
        ////PropertiesBuildMine();
        //if(GridProperties != null)
        //{
        //    GridProperties.RevertParameters();
        //}
        //StaticVentFields[0].text = _oxygen.ToString();
        //StaticVentFields[1].text = _methane.ToString();
        //StaticVentFields[2].text = _co.ToString();
        //StaticVentFields[3].text = _h2s.ToString();
        //VentDropdown.value = _initMFireSelection;
        //ThirdPersonDropdown.SetValueWithoutNotify(_profileSelection);
        ////_mineSceneConfiguration.SetMinerProfile(_profileSelection);

        //SetBG4(_BG4FieldSaveVal,true);
        //SetMasterVolume(_masterVol, true);
        //UIScale.SetDisplayedValue(_systemConfig.UIScale);

        RestoreInitialValues();
        ScenarioSaveLoad.Instance.RaiseMineSettingsChanged();

        gameObject.SetActive(false);
    }

    //public void ProcessApply()
    //{
    //    _buildMineFlag = true;
    //    PropertiesBuildMine();
    //}
    ///// <summary>
    ///// DEPRECATED
    ///// </summary>
    //void PropertiesBuildMine()
    //{
    //    if (!_buildMineFlag)
    //    {
    //        return;
    //    }
    //    float _unitMultiplier = 1;
    //    //if(UnitDropdown.value == 1)
    //    //{
    //    //    _unitMultiplier = METERS_TO_FEET;
    //    //}
    //    NIOSH_MineCreation.MineSettings newSettings = new NIOSH_MineCreation.MineSettings(MineCreator.tileSet, 0, MineCreator.GetNumEntries(), MineCreator.GetNumCrosscuts(),
    //                                                    SeamHeightField.GetCurrentValue() / _unitMultiplier,
    //                                                    PillarWidthField.GetCurrentValue() / _unitMultiplier,
    //                                                    PillarLengthField.GetCurrentValue() / _unitMultiplier,
    //                                                    EntryWidthField.GetCurrentValue() / _unitMultiplier,
    //                                                    RoofBoltSliderField.GetCurrentValue() / _unitMultiplier,
    //                                                    RoofboltOffsetSliderField.GetCurrentValue() / _unitMultiplier,
    //                                                    false);
    //    MineCreator.GenerateMine(newSettings);
    //}

    public void SetRockDustLevel(float value, bool generate)
    {
        if(value != ScenarioSaveLoad.Settings.RockDustLevel)
            ScenarioSaveLoad.Instance.SetRockDustLevel(value / 100.0f);
    }

    //public void SetCornerCurtainState(bool useCorner)
    //{
    //    ScenarioSaveLoad.Instance.EnableCornerCurtain(useCorner);
    //}

    //public void SetBoltSpacing(float value, bool generate)
    //{
    //    ScenarioSaveLoad.Instance.SetBoltSpacing(value);
    //    //Debug.Log($"Bolt space updated in scenario editor? {value}");
    //}

    //public void SetBoltOffset(float value, bool generate)
    //{
    //    float _unitMultiplier = 1;
    //    ScenarioSaveLoad.Instance.SetBoltOffset(value / _unitMultiplier);
    //}

    //void EnableRoofBolts(bool enable)
    //{
    //    Debug.Log($"Enable bolts? {enable}");
    //    if (enable)
    //    {
    //        ScenarioSaveLoad.Instance.SetBoltSpacing(_roofBoltSpacingValue);
    //    }
    //    else
    //    {
    //        ScenarioSaveLoad.Instance.SetBoltSpacing(0);
    //    }
        
    //}

}
