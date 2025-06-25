using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using TMPro;

public class VehicleExperimentSceneController : MonoBehaviour {

    private VehicleExperiment _CurrentExperiment;    
    private List<VehicleVariable> Vehicles;
    public int _VehicleIndex = 0;
    private VehicleVariable _CurrentVehicleTest;
    private SensVariable _gradeVar;
    private SensVariable _initialSpeedVar;
    private SensVariable _rollingResistanceVar;
    private SensVariable _brakingForceVar;
    private SensVariable _lagTimeVar;
    private SensVariable _asymptoteValVar;
    private SensVariable _asymptoteSlpVar;
    private SensVariable _extremeValVar;
    private SensVariable _extremeSlpVar;
    private Scene _LoadedScene;
    private List<TestType> _TestList;
    private string _VehicleHeaderInfo;

    private List<GameObject> MWCCategoricalVar;
    private int _MWCIndex = 0;
    private List<GameObject> YellowZoneCategoricalVar;
    private int _YellowZoneCategoricalIndex = 0;
    private bool IsTwoStageBraking = false;
    private bool Loading = false;
    private bool _IsDemo = false;
    

    private string[] _vehicleExpConfigs;

    private int _trialPerIncrement;
    private int _trialCount;

    public LogManager LogManagerRef;
    public int MWCCount = 3;
    public bool HideGraph = true;
    public bool ShowPIDOutput = false;
    public float TimeScale = 1;
    //Prefabs
    //public GameObject TenSC32B_ExtraSmall;
    //public GameObject TenSC32B_Small;
    //public GameObject TenSC32B_Large;
    //public GameObject BH20Car_ExtraSmall;
    //public GameObject BH20Car_Small;
    //public GameObject BH20Car_Large;
    //public GameObject BH18Car_ExtraSmall;
    //public GameObject BH18Car_Small;
    //public GameObject BH18Car_Large;
    //public GameObject EightSixteen_ExtraSmall;
    //public GameObject EightSixteen_Small;
    //public GameObject EightSixteen_Large;

    public GameObject[] TenSC32BPrefabs;
    public GameObject[] BH20Prefabs;
    public GameObject[] BH18Prefabs;
    public GameObject[] EightSixteenPrefabs;

    [Space]
    public string[] GradeScenes;
    //private Queue<VehicleVariable> TenSCSet;
    //private Queue<VehicleVariable> BH20Set;
    //private Queue<VehicleVariable> BH18Set;
    //private Queue<VehicleVariable> EightSixteenSet;
    private Queue<VehicleVariable> ExperimentSet;
    private Regex regex = new Regex(@"(?<=_)(.+?)(?=Prox)");

    private static TextMeshProUGUI _statusText; 

    public static void SetStatusText(string text)
    {
        if (_statusText == null)
        {
            var statusTextObj = GameObject.Find("ExperimentStatusText");
            if (statusTextObj != null)
                _statusText = statusTextObj.GetComponent<TextMeshProUGUI>();

            if (_statusText == null)
                return;
        }

        _statusText.text = text;

    }

    // Use this for initialization
    void Start () {
        Vehicles = new List<VehicleVariable>();
        YellowZoneCategoricalVar = new List<GameObject>();
        ExperimentSet = new Queue<VehicleVariable>();
        _vehicleExpConfigs = File.ReadAllLines(Path.Combine(Application.dataPath, "vehicleConfig.txt"));
        ParseConfigs(_vehicleExpConfigs);

        Time.timeScale = TimeScale;
        
        _CurrentVehicleTest = ExperimentSet.Dequeue();//Queue's up the first category (Vehicle-ZoneSize-MWC Location-Two Stage Braking)
        _TestList = new List<TestType>();
        if (_IsDemo)
        {
            _TestList.Add(TestType.SimpleMassTest);
        }
        else
        {
            _TestList.Add(TestType.SpeedSens);
            _TestList.Add(TestType.BreakTorqueSens);
            _TestList.Add(TestType.DelaySens);
            _TestList.Add(TestType.AsForce);
            _TestList.Add(TestType.AsSlip);
            _TestList.Add(TestType.ExForce);
            _TestList.Add(TestType.ExSlip);
            _TestList.Add(TestType.InclineSens);
            _TestList.Add(TestType.MassSens);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        Timer.Init();
        Timer.StartTimer();

        LoadNewScene();
    }
    
    

    /// <summary>
    /// Obsolete
    /// </summary>
    public void TestComplete()
    {
        if (Vehicles.Count > 0)
        {
            _CurrentVehicleTest = Vehicles[_VehicleIndex];
            LoadNewScene(true);
        }
        else
        {
            StringMessageData end = new StringMessageData();
            end.Message = "Experiment complete";
            LogManagerRef.AddPacketToQueue(end);
            Application.Quit();
        }
    }

    private void LoadNewScene(bool firstLoad = false)
    {
        //if (!firstLoad)
        //{
        //    //Unload prior scene
        //    SceneManager.UnloadSceneAsync(_LoadedScene);
        //}
        //else
        //{
        //    _CurrentVehicleTest = Vehicles[_VehicleIndex];
        //}
        Loading = true;
        StringMessageData configuration = new StringMessageData();
        configuration.Message = "Yellow Zone Category: " + _YellowZoneCategoricalIndex + "MWC Index: " + _MWCIndex + "TwoStageBraking: " + IsTwoStageBraking + "Vehicle Index: " + _VehicleIndex;
        LogManagerRef.AddPacketToQueue(configuration);
        if (_IsDemo)
        {
            Debug.Log("Scene loading: " + GradeScenes[_CurrentVehicleTest.SceneGradeToLoadIndex] + ": " + _CurrentVehicleTest.SceneGradeToLoadIndex);
            SceneManager.LoadScene(GradeScenes[_CurrentVehicleTest.SceneGradeToLoadIndex], LoadSceneMode.Additive);
        }
        else
        {
            SceneManager.LoadScene("VehicleExperiment", LoadSceneMode.Additive);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {        
        if(scene.name.Contains("VehicleExperiment"))
        {
            if (Loading)
            {
                Debug.Log("Scene loaded!");
                _LoadedScene = scene;
                Loading = false;
                _CurrentExperiment = FindObjectOfType<VehicleExperiment>();
                _CurrentExperiment.IsDemo = _IsDemo;
                _CurrentExperiment.VehicleHeaderInfo = MakeHeader(_CurrentVehicleTest);
                _CurrentExperiment.LogManagerRef = LogManagerRef;
                _CurrentExperiment.VehicleSceneController = this;
                //_CurrentExperiment.ShuttleCar = _CurrentVehicleTest.PrefabsDefinedYellowZone[_YellowZoneCategoricalIndex];
                _CurrentExperiment.CurrentTest = _TestList[0];                
                _CurrentExperiment._currentVehicleVar = _CurrentVehicleTest;
                if (!_IsDemo)
                {
                    _CurrentExperiment._gradeVar = _gradeVar;
                }
                _CurrentExperiment._initialSpeedVar = _initialSpeedVar;
                _CurrentExperiment._rollingResistanceVar = _rollingResistanceVar;
                _CurrentExperiment._brakingForceVar = _brakingForceVar;
                _CurrentExperiment._lagTimeVar = _lagTimeVar;
                _CurrentExperiment._asymptoteValVar = _asymptoteValVar;
                _CurrentExperiment._asymptoteSlpVar = _asymptoteSlpVar;
                _CurrentExperiment._extremeValVar = _extremeValVar;
                _CurrentExperiment._extremeSlpVar = _extremeSlpVar;
                _CurrentExperiment.HideGraph = HideGraph;
                _CurrentExperiment.ShowPIDOutput = ShowPIDOutput;
                //_CurrentExperiment.MWCCategoricalVar = MWCCategoricalVar;
                //_CurrentExperiment._MWCIndex = _MWCIndex;
                //_CurrentExperiment.YellowZoneCategoricalVar = YellowZoneCategoricalVar;
                //_CurrentExperiment._YellowZoneCategoricalIndex = _YellowZoneCategoricalIndex;
                //_CurrentExperiment.IsTwoStageBraking = IsTwoStageBraking;
                _CurrentExperiment.TrialCount = _trialCount;
                //_CurrentExperiment._YellowZoneCategoricalIndex = _YellowZoneCategoricalIndex;
                _CurrentExperiment.VehicleHeaderInfo = MakeHeader(_CurrentVehicleTest);
                _CurrentExperiment.TestList = _TestList;
                _CurrentExperiment.ExperimentStart();//Changed Start() to ExperimentStart()
            }
        }        
    }

    /// <summary>
    /// Obsolete
    /// </summary>
    /// <returns></returns>
    public bool CompleteMWCCategorySet()
    {
        bool isComplete = false;
        _MWCIndex++;
        if(_MWCIndex >= 3)
        {
            isComplete = true;
            _MWCIndex = 0;
        }
        return isComplete;
    }
    /// <summary>
    /// Obsolete
    /// </summary>
    /// <returns></returns>
    public bool CompleteYellowZoneCategorySet()
    {
        bool isComplete = false;
        _YellowZoneCategoricalIndex++;
        if (_YellowZoneCategoricalIndex >= YellowZoneCategoricalVar.Count)
        {
            isComplete = true;
            _YellowZoneCategoricalIndex = 0;
        }
        return isComplete;
    }
    /// <summary>
    /// Obsolete
    /// </summary>
    /// <returns></returns>
    public bool CompleteTwoStageBrakingCategorySet()
    {
        IsTwoStageBraking = true;
        return IsTwoStageBraking;
    }

    /// <summary>
    /// Called by the experiment once the Mass sensitivity (the last of the continuous variables to check) test is completed. Unloads the scene.
    /// </summary>
    /// <returns></returns>
    public bool CompleteMassSet()
    {
        //_CurrentVehicleTest = Vehicles.Dequeue();
        SceneManager.UnloadSceneAsync(_LoadedScene);
        return true;
    }

    /// <summary>
    /// Called once the experiment scene is unloaded. Queues up the next vehicle.
    /// </summary>
    /// <param name="scene"></param>
    void OnSceneUnloaded(Scene scene)
    {
        if (scene.name.Contains("VehicleExperiment"))
        {
            //if (_YellowZoneCategoricalIndex < (YellowZoneCategoricalVar.Count - 1))
            //{
            //    _YellowZoneCategoricalIndex++;
            //}
            //else if (_MWCIndex < (2))
            //{
            //    _MWCIndex++;
            //}
            //else if (!IsTwoStageBraking)
            //{
            //    IsTwoStageBraking = true;
            //}
            //else
            //{
            //    _VehicleIndex++;
            //    if (_VehicleIndex < Vehicles.Count)
            //    {
            //        _CurrentVehicleTest = Vehicles[_VehicleIndex];

            //        //_YellowZoneCategoricalIndex = 0;
            //        //_MWCIndex = 0;
            //        //IsTwoStageBraking = false;
            //    }
            //    else
            //    {
            //        StringMessageData complete = new StringMessageData();
            //        complete.Message = "Experiment Complete";
            //        LogManagerRef.AddPacketToQueue(complete);
            //        Application.Quit();
            //        return;
            //    }
            //}

            if (ExperimentSet.Count > 0)
            {
                _CurrentVehicleTest = ExperimentSet.Dequeue();
                Debug.Log(_CurrentVehicleTest.SceneGradeToLoadIndex + " index on Unload");
                //_YellowZoneCategoricalIndex = 0;
                //_MWCIndex = 0;
                //IsTwoStageBraking = false;
            }
            else
            {
                StringMessageData complete = new StringMessageData();
                complete.Message = "Experiment Complete";
                LogManagerRef.AddPacketToQueue(complete);
                Application.Quit();

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }
            LoadNewScene();
        }
    }

    private void ParseConfigs(string[] vehicleExpConfigs)
    {
        for (int i = 0; i < vehicleExpConfigs.Length; i++)
        {
            if (vehicleExpConfigs[i][0] == '#')
            {
                continue;
            }
            string[] splitLine = vehicleExpConfigs[i].Split('\t');
            if (splitLine.Length < 1)
            {
                continue;
            }
            switch (splitLine[0])
            {
                case "Demo":
                    _IsDemo = true;
                    break;
                case "TrialsPerIncrement":
                    if (int.TryParse(splitLine[1], out _trialPerIncrement))
                    {
                        StringMessageData trialPerInc = new StringMessageData();
                        trialPerInc.Message = "Trials Per Increment: " + _trialPerIncrement;
                        LogManagerRef.AddPacketToQueue(trialPerInc);
                    }
                    break;
                case "TrialsCount":
                    if (int.TryParse(splitLine[1], out _trialCount))
                    {
                        StringMessageData trialCount = new StringMessageData();
                        trialCount.Message = "Trials Count: " + _trialCount;
                        LogManagerRef.AddPacketToQueue(trialCount);
                    }
                    break;
                case "Grade":
                    bool minGradeCheck = float.TryParse(splitLine[1], out _gradeVar.MinVal);
                    bool nominalGradeCheck = float.TryParse(splitLine[2], out _gradeVar.NominalVal);
                    bool maxGradeCheck = float.TryParse(splitLine[3], out _gradeVar.MaxVal);
                    if (minGradeCheck && nominalGradeCheck && maxGradeCheck)
                    {
                        StringMessageData gradeMessage = new StringMessageData();
                        gradeMessage.Message = "Grade Variables: " + _gradeVar.MinVal + " min, " + _gradeVar.NominalVal + " nom, " + _gradeVar.MaxVal + " max";
                        LogManagerRef.AddPacketToQueue(gradeMessage);
                    }
                    else
                    {
                        Debug.LogError("Failure to read Grade var");
                    }
                    break;
                case "InitialSpeed":
                    bool minSpeedCheck = float.TryParse(splitLine[1], out _initialSpeedVar.MinVal);
                    bool nominalSpeedCheck = float.TryParse(splitLine[2], out _initialSpeedVar.NominalVal);
                    bool maxSpeedCheck = float.TryParse(splitLine[3], out _initialSpeedVar.MaxVal);

                    if (minSpeedCheck && nominalSpeedCheck && maxSpeedCheck)
                    {
                        StringMessageData initSpeedMsg = new StringMessageData();
                        //Convert speeds to mph from f/s
                        //_initialSpeedVar.MinVal = 0.681818f * _initialSpeedVar.MinVal;
                        //_initialSpeedVar.NominalVal = 0.681818f * _initialSpeedVar.NominalVal;
                        //_initialSpeedVar.MaxVal = 0.681818f * _initialSpeedVar.MaxVal;
                        initSpeedMsg.Message = "Speed Variables: " + _initialSpeedVar.MinVal + " min, " + _initialSpeedVar.NominalVal + " nom, " + _initialSpeedVar.MaxVal + " max";
                        LogManagerRef.AddPacketToQueue(initSpeedMsg);
                    }
                    else
                    {
                        Debug.LogError("Failure to read speed var");
                    }
                    break;
                case "RollingResistance":
                    bool minResCheck = float.TryParse(splitLine[1], out _rollingResistanceVar.MinVal);
                    bool nominalResCheck = float.TryParse(splitLine[2], out _rollingResistanceVar.NominalVal);
                    bool maxResCheck = float.TryParse(splitLine[3], out _rollingResistanceVar.MaxVal);

                    if (minResCheck && nominalResCheck && maxResCheck)
                    {
                        StringMessageData frictionMsg = new StringMessageData();
                        frictionMsg.Message = "Resistance Variables: " + _rollingResistanceVar.MinVal + " min, " + _rollingResistanceVar.NominalVal + " nom, " + _rollingResistanceVar.MaxVal + " max";
                        LogManagerRef.AddPacketToQueue(frictionMsg);
                    }
                    else
                    {
                        Debug.LogError("Failure to read resistance var");
                    }
                    break;
                case "AsymptoteValue":
                    bool minAVCheck = float.TryParse(splitLine[1], out _asymptoteValVar.MinVal);
                    bool nomAVCheck = float.TryParse(splitLine[2], out _asymptoteValVar.NominalVal);
                    bool maxAVCheck = float.TryParse(splitLine[3], out _asymptoteValVar.MaxVal);
                    if (minAVCheck && nomAVCheck && maxAVCheck)
                    {
                        StringMessageData avMsg = new StringMessageData();
                        avMsg.Message = "Asymptote Value Variables: " + _asymptoteValVar.MinVal + " min, " + _asymptoteValVar.NominalVal + " nom, " + _asymptoteValVar.MaxVal + " max";
                        LogManagerRef.AddPacketToQueue(avMsg);
                    }
                    break;
                case "AsymptoteSlip":
                    bool minASCheck = float.TryParse(splitLine[1], out _asymptoteSlpVar.MinVal);
                    bool nomASCheck = float.TryParse(splitLine[2], out _asymptoteSlpVar.NominalVal);
                    bool maxASCheck = float.TryParse(splitLine[3], out _asymptoteSlpVar.MaxVal);
                    if (minASCheck && nomASCheck && maxASCheck)
                    {
                        StringMessageData asMsg = new StringMessageData();
                        asMsg.Message = "Asymptote Slip Variables: " + _asymptoteSlpVar.MinVal + " min, " + _asymptoteSlpVar.NominalVal + " nom, " + _asymptoteSlpVar.MaxVal + " max";
                        LogManagerRef.AddPacketToQueue(asMsg);
                    }
                    break;
                case "ExtremumValue":
                    bool minEVCheck = float.TryParse(splitLine[1], out _extremeValVar.MinVal);
                    bool nomEVCheck = float.TryParse(splitLine[2], out _extremeValVar.NominalVal);
                    bool maxEVCheck = float.TryParse(splitLine[3], out _extremeValVar.MaxVal);
                    if (minEVCheck && nomEVCheck && maxEVCheck)
                    {
                        StringMessageData evMsg = new StringMessageData();
                        evMsg.Message = "Extremum Value Variables: " + _extremeValVar.MinVal + " min, " + _extremeValVar.NominalVal + " nom, " + _extremeValVar.MaxVal + " max";
                        LogManagerRef.AddPacketToQueue(evMsg);
                    }
                    break;
                case "ExtremumSlip":
                    bool minESCheck = float.TryParse(splitLine[1], out _extremeSlpVar.MinVal);
                    bool nomESCheck = float.TryParse(splitLine[2], out _extremeSlpVar.NominalVal);
                    bool maxESCheck = float.TryParse(splitLine[3], out _extremeSlpVar.MaxVal);
                    if (minESCheck && nomESCheck && maxESCheck)
                    {
                        StringMessageData esMsg = new StringMessageData();
                        esMsg.Message = "Extremum Slip Variables: " + _extremeSlpVar.MinVal + " min, " + _extremeSlpVar.NominalVal + " nom, " + _extremeSlpVar.MaxVal + " max";
                        LogManagerRef.AddPacketToQueue(esMsg);
                    }
                    break;
                case "BrakingForce":
                    bool minBFCheck = float.TryParse(splitLine[1], out _brakingForceVar.MinVal);
                    bool nominalBFCheck = float.TryParse(splitLine[2], out _brakingForceVar.NominalVal);
                    bool maxBFCheck = float.TryParse(splitLine[3], out _brakingForceVar.MaxVal);
                    if (minBFCheck && nominalBFCheck && maxBFCheck)
                    {
                        StringMessageData bfMsg = new StringMessageData();
                        bfMsg.Message = "Braking Force Variables: " + _brakingForceVar.MinVal + " min, " + _brakingForceVar.NominalVal + " nom, " + _brakingForceVar.MaxVal + " max";
                        LogManagerRef.AddPacketToQueue(bfMsg);
                    }
                    else
                    {
                        Debug.LogError("Failure to read braking var");
                    }
                    break;
                case "LagTime":
                    bool minLagCheck = float.TryParse(splitLine[1], out _lagTimeVar.MinVal);
                    bool nominalLagCheck = float.TryParse(splitLine[2], out _lagTimeVar.NominalVal);
                    bool maxLagCheck = float.TryParse(splitLine[3], out _lagTimeVar.MaxVal);
                    if (minLagCheck && nominalLagCheck && maxLagCheck)
                    {
                        StringMessageData lagMsg = new StringMessageData();
                        lagMsg.Message = "Lag Time Variables: " + _lagTimeVar.MinVal + " min, " + _lagTimeVar.NominalVal + " nom, " + _lagTimeVar.MaxVal + " max";
                        LogManagerRef.AddPacketToQueue(lagMsg);
                    }
                    else
                    {
                        Debug.LogError("Failure to read lag time var");
                    }
                    break;
                case "10SC32B":
                    VehicleVariable import = new VehicleVariable();
                    //VehicleVariable 
                    import.Vehicle = ShuttleCarExpVehicles.Type10SC32B;
                    //import.VehicleToLoad = TenSC32B_Small;
                    //import.PrefabsDefinedYellowZone = new List<GameObject>();
                    //import.PrefabsDefinedYellowZone.Add(TenSC32B_Small);
                    //import.PrefabsDefinedYellowZone.Add(TenSC32B_Large);
                    bool emptyVal = float.TryParse(splitLine[1], out import.EmptyMass);
                    bool halfVal = float.TryParse(splitLine[2], out import.HalfMass);
                    bool fullVal = float.TryParse(splitLine[3], out import.FullMass);
                    bool overVal = float.TryParse(splitLine[4], out import.Overload);
                    bool nomSpeedVal = float.TryParse(splitLine[5], out import.NominalSpeed);
                    bool nomBrakeTorque = float.TryParse(splitLine[6], out import.NominalBrakeTorque);
                    if (emptyVal && halfVal && fullVal && overVal && nomSpeedVal && nomBrakeTorque)
                    {
                        StringMessageData sc32Msg = new StringMessageData();
                        sc32Msg.Message = "10SC32 Variables: " + import.EmptyMass + " empty, " + import.HalfMass + " half, " + import.FullMass + " full, " + import.Overload + " overload, " + import.NominalSpeed + " nomSpd, " + import.NominalBrakeTorque + " nomBrake";
                        LogManagerRef.AddPacketToQueue(sc32Msg);
                        for(int j = 0; j < TenSC32BPrefabs.Length; j++)
                        {
                            for(int k = 0; k < 3; k++) //MWC Location categories
                            {
                                for (int l = 0; l < 2; l++) // Two stage braking categories
                                {
                                    VehicleVariable vVariable = new VehicleVariable();
                                    vVariable.Vehicle = ShuttleCarExpVehicles.Type10SC32B;
                                    vVariable.MwcLoc = (MWCLocation)k;
                                    vVariable.ZoneName = TenSC32BPrefabs[j].name;
                                    vVariable.VehicleToLoad = TenSC32BPrefabs[j];
                                    vVariable.EmptyMass = import.EmptyMass;
                                    vVariable.HalfMass = import.HalfMass;
                                    vVariable.FullMass = import.FullMass;
                                    vVariable.Overload = import.Overload;
                                    vVariable.NominalSpeed = import.NominalSpeed;
                                    vVariable.NominalBrakeTorque = import.NominalBrakeTorque;
                                    vVariable.IgnoreYellowZone = (l == 1); //simple way of casting 0 = false 1 = true
                                    vVariable.VehicleName = "10SC32B";
                                    
                                    vVariable.ZoneSetup = regex.Match(TenSC32BPrefabs[j].name).ToString();
                                    if (_IsDemo)
                                    {
                                        for (int q = 0; q < GradeScenes.Length; q++)
                                        {
                                            vVariable.SceneGradeToLoadIndex = q;
                                            ExperimentSet.Enqueue(vVariable);
                                            
                                        }
                                        l = 2;
                                        k = 3;
                                    }
                                    else
                                    {
                                        ExperimentSet.Enqueue(vVariable);
                                    }
                                }
                            }
                            //VehicleVariable vVariable = new VehicleVariable();
                            //vVariable.Vehicle = ShuttleCarExpVehicles.Type10SC32B;
                            //vVariable.ZoneName
                        }

                        //Vehicles.Add(import);
                    }
                    else
                    {
                        Debug.LogError("Failure to read vehicle var");
                    }

                    break;
                case "BH20":
                    //_TestQueue.Enqueue(TestType.MassSens);
                    VehicleVariable bh20 = new VehicleVariable();

                    bh20.Vehicle = ShuttleCarExpVehicles.TypeBH20;
                    //bh20.VehicleToLoad = BH20Car_Small;
                    //bh20.PrefabsDefinedYellowZone = new List<GameObject>();
                    //bh20.PrefabsDefinedYellowZone.Add(BH20Car_Small);
                    //bh20.PrefabsDefinedYellowZone.Add(BH20Car_Large);

                    bool emptyVal2 = float.TryParse(splitLine[1], out bh20.EmptyMass);
                    bool halfVal2 = float.TryParse(splitLine[2], out bh20.HalfMass);
                    bool fullVal2 = float.TryParse(splitLine[3], out bh20.FullMass);
                    bool overVal2 = float.TryParse(splitLine[4], out bh20.Overload);
                    bool nomSpeedVal2 = float.TryParse(splitLine[5], out bh20.NominalSpeed);
                    bool nomBrakeTor2 = float.TryParse(splitLine[6], out bh20.NominalBrakeTorque);
                    if (emptyVal2 && halfVal2 && fullVal2 && overVal2 && nomSpeedVal2 && nomBrakeTor2)
                    {
                        StringMessageData bh20Msg = new StringMessageData();
                        bh20Msg.Message = "BH20 Variables: " + bh20.EmptyMass + " empty, " + bh20.HalfMass + " half, " + bh20.FullMass + " full, " + bh20.Overload + " overload, " + bh20.NominalSpeed + " nomSpd, " + bh20.NominalBrakeTorque + " nomBrake";
                        LogManagerRef.AddPacketToQueue(bh20Msg);
                        //Vehicles.Add(bh20);
                        for (int j = 0; j < BH20Prefabs.Length; j++)
                        {
                            for (int k = 0; k < 3; k++)//MWC Location categories
                            {
                                for (int l = 0; l < 2; l++)//Two stage braking categories
                                {
                                    VehicleVariable vVariable = new VehicleVariable();
                                    vVariable.Vehicle = ShuttleCarExpVehicles.TypeBH20;
                                    vVariable.MwcLoc = (MWCLocation)k;
                                    vVariable.ZoneName = BH20Prefabs[j].name;
                                    vVariable.VehicleToLoad = BH20Prefabs[j];
                                    vVariable.EmptyMass = bh20.EmptyMass;
                                    vVariable.HalfMass = bh20.HalfMass;
                                    vVariable.FullMass = bh20.FullMass;
                                    vVariable.Overload = bh20.Overload;
                                    vVariable.NominalSpeed = bh20.NominalSpeed;
                                    vVariable.NominalBrakeTorque = bh20.NominalBrakeTorque;
                                    vVariable.IgnoreYellowZone = (l == 1); //simple way of casting 0 = false 1 = true
                                    vVariable.VehicleName = "BH20";
                                    
                                    vVariable.ZoneSetup = regex.Match(BH20Prefabs[j].name).ToString();

                                    if (_IsDemo)
                                    {
                                        for (int q = 0; q < GradeScenes.Length; q++)
                                        {
                                            VehicleVariable v2 = new VehicleVariable();
                                            v2.Vehicle = ShuttleCarExpVehicles.TypeBH20;
                                            v2.MwcLoc = (MWCLocation)k;
                                            v2.ZoneName = BH20Prefabs[j].name;
                                            v2.VehicleToLoad = BH20Prefabs[j];
                                            v2.EmptyMass = bh20.EmptyMass;
                                            v2.HalfMass = bh20.HalfMass;
                                            v2.FullMass = bh20.FullMass;
                                            v2.Overload = bh20.Overload;
                                            v2.NominalSpeed = bh20.NominalSpeed;
                                            v2.NominalBrakeTorque = bh20.NominalBrakeTorque;
                                            v2.IgnoreYellowZone = (l == 1); //simple way of casting 0 = false 1 = true
                                            v2.VehicleName = "BH20";

                                            v2.ZoneSetup = regex.Match(BH20Prefabs[j].name).ToString();
                                            v2.SceneGradeToLoadIndex = q;
                                            ExperimentSet.Enqueue(v2);
                                        }
                                        l = 2;
                                        k = 3;
                                    }
                                    else
                                    {
                                        ExperimentSet.Enqueue(vVariable);
                                    }
                                }
                            }
                            //VehicleVariable vVariable = new VehicleVariable();
                            //vVariable.Vehicle = ShuttleCarExpVehicles.Type10SC32B;
                            //vVariable.ZoneName
                        }
                    }
                    else
                    {
                        Debug.LogError("Failure to read vehicle var");
                    }

                    break;
                case "BH18AC":
                    // _TestQueue.Enqueue(TestType.MassSens);
                    VehicleVariable bh18ac = new VehicleVariable();
                    bh18ac.Vehicle = ShuttleCarExpVehicles.TypeBH18AC;
                    //bh18ac.VehicleToLoad = BH18Car_Small;
                    //bh18ac.PrefabsDefinedYellowZone = new List<GameObject>();
                    //bh18ac.PrefabsDefinedYellowZone.Add(BH18Car_Small);
                    //bh18ac.PrefabsDefinedYellowZone.Add(BH18Car_Large);

                    bool emptyVal3 = float.TryParse(splitLine[1], out bh18ac.EmptyMass);
                    bool halfVal3 = float.TryParse(splitLine[2], out bh18ac.HalfMass);
                    bool fullVal3 = float.TryParse(splitLine[3], out bh18ac.FullMass);
                    bool overVal3 = float.TryParse(splitLine[4], out bh18ac.Overload);
                    bool nomSpeedVal3 = float.TryParse(splitLine[5], out bh18ac.NominalSpeed);
                    bool nomBrakeTor3 = float.TryParse(splitLine[6], out bh18ac.NominalBrakeTorque);
                    if (emptyVal3 && halfVal3 && fullVal3 && overVal3 && nomSpeedVal3 && nomBrakeTor3)
                    {
                        StringMessageData bh18Msg = new StringMessageData();
                        bh18Msg.Message = "BH18AC Variables: " + bh18ac.EmptyMass + " empty, " + bh18ac.HalfMass + " half, " + bh18ac.FullMass + " full, " + bh18ac.Overload + " overload, " + bh18ac.NominalSpeed + " nomSpd, " + bh18ac.NominalBrakeTorque + " nomBrake";
                        LogManagerRef.AddPacketToQueue(bh18Msg);
                        //Vehicles.Add(bh18ac);
                        for (int j = 0; j < BH18Prefabs.Length; j++)
                        {
                            for (int k = 0; k < 3; k++)//MWC Location categories
                            {
                                for (int l = 0; l < 2; l++)//Two Stage braking categories
                                {
                                    VehicleVariable vVariable = new VehicleVariable();
                                    vVariable.Vehicle = ShuttleCarExpVehicles.TypeBH18AC;
                                    vVariable.MwcLoc = (MWCLocation)k;
                                    vVariable.ZoneName = BH18Prefabs[j].name;
                                    vVariable.VehicleToLoad = BH18Prefabs[j];
                                    vVariable.EmptyMass = bh18ac.EmptyMass;
                                    vVariable.HalfMass = bh18ac.HalfMass;
                                    vVariable.FullMass = bh18ac.FullMass;
                                    vVariable.Overload = bh18ac.Overload;
                                    vVariable.NominalSpeed = bh18ac.NominalSpeed;
                                    vVariable.NominalBrakeTorque = bh18ac.NominalBrakeTorque;
                                    vVariable.IgnoreYellowZone = (l == 1); //simple way of casting 0 = false 1 = true
                                    vVariable.VehicleName = "BH18";
                                    vVariable.ZoneSetup = regex.Match(BH18Prefabs[j].name).ToString();

                                    if (_IsDemo)
                                    {
                                        for (int q = 0; q < GradeScenes.Length; q++)
                                        {
                                            VehicleVariable v2 = new VehicleVariable();
                                            v2.Vehicle = ShuttleCarExpVehicles.TypeBH18AC;
                                            v2.MwcLoc = (MWCLocation)k;
                                            v2.ZoneName = BH18Prefabs[j].name;
                                            v2.VehicleToLoad = BH18Prefabs[j];
                                            v2.EmptyMass = bh18ac.EmptyMass;
                                            v2.HalfMass = bh18ac.HalfMass;
                                            v2.FullMass = bh18ac.FullMass;
                                            v2.Overload = bh18ac.Overload;
                                            v2.NominalSpeed = bh18ac.NominalSpeed;
                                            v2.NominalBrakeTorque = bh18ac.NominalBrakeTorque;
                                            v2.IgnoreYellowZone = (l == 1); //simple way of casting 0 = false 1 = true
                                            v2.VehicleName = "BH18";
                                            v2.ZoneSetup = regex.Match(BH18Prefabs[j].name).ToString();
                                            v2.SceneGradeToLoadIndex = q;
                                            ExperimentSet.Enqueue(v2);
                                        }
                                        l = 2;
                                        k = 3;
                                    }
                                    else
                                    {
                                        ExperimentSet.Enqueue(vVariable);
                                    }
                                }
                            }
                            //VehicleVariable vVariable = new VehicleVariable();
                            //vVariable.Vehicle = ShuttleCarExpVehicles.Type10SC32B;
                            //vVariable.ZoneName
                        }
                    }
                    else
                    {
                        Debug.LogError("Failure to read vehicle var");
                    }

                    break;
                case "816-2000":
                    VehicleVariable eight16 = new VehicleVariable();
                    eight16.Vehicle = ShuttleCarExpVehicles.Type8162000;
                    //eight16.VehicleToLoad = EightSixteen_Small;
                    
                    //eight16.PrefabsDefinedYellowZone = new List<GameObject>();
                    //eight16.PrefabsDefinedYellowZone.Add(EightSixteen_Small);
                    //eight16.PrefabsDefinedYellowZone.Add(EightSixteen_Large);

                    bool emptyVal4 = float.TryParse(splitLine[1], out eight16.EmptyMass);
                    bool halfVal4 = float.TryParse(splitLine[2], out eight16.HalfMass);
                    bool fullVal4 = float.TryParse(splitLine[3], out eight16.FullMass);
                    bool overVal4 = float.TryParse(splitLine[4], out eight16.Overload);
                    bool nomSpeedVal4 = float.TryParse(splitLine[5], out eight16.NominalSpeed);
                    bool nomBrakeTor4 = float.TryParse(splitLine[6], out eight16.NominalBrakeTorque);
                    if (emptyVal4 && halfVal4 && fullVal4 && overVal4 && nomSpeedVal4 && nomBrakeTor4)
                    {
                        StringMessageData eightMsg = new StringMessageData();
                        eightMsg.Message = "816-2000 Variables: " + eight16.EmptyMass + " empty, " + eight16.HalfMass + " half, " + eight16.FullMass + " full, " + eight16.Overload + " overload, " + eight16.NominalSpeed + " nomSpd, " + eight16.NominalBrakeTorque + " nomBrake";
                        LogManagerRef.AddPacketToQueue(eightMsg);
                        //Vehicles.Add(eight16);
                        for (int j = 0; j < EightSixteenPrefabs.Length; j++)
                        {
                            for (int k = 0; k < 3; k++)//MWC Location categories
                            {
                                for (int l = 0; l < 2; l++)//Two stage braking categories
                                {
                                    VehicleVariable vVariable = new VehicleVariable();
                                    vVariable.Vehicle = ShuttleCarExpVehicles.TypeBH20;
                                    vVariable.MwcLoc = (MWCLocation)k;
                                    vVariable.ZoneName = EightSixteenPrefabs[j].name;
                                    vVariable.VehicleToLoad = EightSixteenPrefabs[j];
                                    vVariable.EmptyMass = eight16.EmptyMass;
                                    vVariable.HalfMass = eight16.HalfMass;
                                    vVariable.FullMass = eight16.FullMass;
                                    vVariable.Overload = eight16.Overload;
                                    vVariable.NominalSpeed = eight16.NominalSpeed;
                                    vVariable.NominalBrakeTorque = eight16.NominalBrakeTorque;
                                    vVariable.IgnoreYellowZone = (l == 1); //simple way of casting 0 = false 1 = true
                                    vVariable.VehicleName = "CH816C";
                                    vVariable.ZoneSetup = regex.Match(EightSixteenPrefabs[j].name).ToString();

                                    if (_IsDemo)
                                    {
                                        for (int q = 0; q < GradeScenes.Length; q++)
                                        {
                                            VehicleVariable v2 = new VehicleVariable();
                                            v2.Vehicle = ShuttleCarExpVehicles.TypeBH20;
                                            v2.MwcLoc = (MWCLocation)k;
                                            v2.ZoneName = EightSixteenPrefabs[j].name;
                                            v2.VehicleToLoad = EightSixteenPrefabs[j];
                                            v2.EmptyMass = eight16.EmptyMass;
                                            v2.HalfMass = eight16.HalfMass;
                                            v2.FullMass = eight16.FullMass;
                                            v2.Overload = eight16.Overload;
                                            v2.NominalSpeed = eight16.NominalSpeed;
                                            v2.NominalBrakeTorque = eight16.NominalBrakeTorque;
                                            v2.IgnoreYellowZone = (l == 1); //simple way of casting 0 = false 1 = true
                                            v2.VehicleName = "CH816C";
                                            v2.ZoneSetup = regex.Match(EightSixteenPrefabs[j].name).ToString();
                                            v2.SceneGradeToLoadIndex = q;
                                            ExperimentSet.Enqueue(v2);
                                        }
                                        l = 2;
                                        k = 3;
                                    }
                                    else
                                    {
                                        ExperimentSet.Enqueue(vVariable);
                                    }
                                }
                            }
                            //VehicleVariable vVariable = new VehicleVariable();
                            //vVariable.Vehicle = ShuttleCarExpVehicles.Type10SC32B;
                            //vVariable.ZoneName
                        }
                    }
                    else
                    {
                        Debug.LogError("Failure to read vehicle var");
                    }

                    break;
                default:
                    break;
            }

            //LogManagerRef.AddPacketToQueue(_TrialStartMessage);
        }
    }
    string MakeHeader(VehicleVariable veh)
    {
        string output = "";
        string twoStage = veh.IgnoreYellowZone ? "OneStage" : "Twostage";
        output = string.Format("Vehicle&ZoneName: {0}, MWCLoc: {1}, TwoStageBraking: {2}, NominalSpeed: {3}, NominalBrakeTorque: {4}, NominalMass: {5}, NominalGrade: {6}, NominalFric-AsVal: {7}, NominalFric-AsSlip: {8}, NominalFric-ExVal: {9}, NominalFric-ExSlip: {10}, NominalLagTime: {11}, TimeScale: {12}", veh.ZoneName, veh.MwcLoc.ToString(), twoStage, veh.NominalSpeed, veh.NominalBrakeTorque, veh.EmptyMass, _gradeVar.NominalVal, _asymptoteValVar.NominalVal,_asymptoteSlpVar.NominalVal,_extremeValVar.NominalVal,_extremeSlpVar.NominalVal, _lagTimeVar.NominalVal, Time.timeScale);
        return output;
    }
}
