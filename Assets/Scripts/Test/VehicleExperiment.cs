using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using System.IO;
using System;

#pragma warning disable 414

public enum TestType
{
	FourVariable,
	Demo,
	AsymptoteTest,
	BreakTorqueSens,
	FrictionSens,
	MassSens,
	YellowZoneSens,
	DelaySens,
	InclineSens,
	SpeedSens,
	BrakingModel,
	MWCLocation,
	AsForce,
	AsSlip,
	ExForce,
	ExSlip,
	Baseline,
	SimpleMassTest
}

public enum TestToPerform
{
	Sensitivity,
	Validation
}

public enum ShuttleCarExpVehicles
{
	Type10SC32B,
	TypeBH20,
	TypeBH18AC,
	Type8162000
}

public enum MWCLocation
{
	Center,
	Left,
	Right
}

public enum VehicleState
{
	BrakeHold,
	SpeedUp,
	InitialSteady,
	Yellow,
	YellowBraking,
	YellowSteady,
	Red,
	RedBraking,
	Stopped
}

public struct SensVariable
{
	public float MinVal;
	public float MaxVal;
	public float NominalVal;
}

public class VehicleVariable
{
	public ShuttleCarExpVehicles Vehicle;
	public GameObject VehicleToLoad;    
	public List<GameObject> PrefabsDefinedYellowZone;
	public MWCLocation MwcLoc;
	public string ZoneName;
	public float EmptyMass;
	public float HalfMass;
	public float FullMass;
	public float Overload;
	public float NominalSpeed;
	public float NominalBrakeTorque;
	public bool IgnoreYellowZone;
	public string ZoneSetup;
	public string VehicleName;
	public int SceneGradeToLoadIndex;
}

public class VehicleExperiment : MonoBehaviour 
{
	private const float SteadyStateTime = 2;
	private const int TestPointCount = 5; //DEPRECATED: As specified in the experimental design doc. End result is Min and Vax values, 4 intervals between. Total of 5 test points
	public int TrialCount = 50;
	private const float YellowZoneSpeedLimit = 0.762f; // == 2.5 fps
	private const float YellowZoneSpeedLimitMPH = 1.70455f;
	private const float SteadyStateTolerance = 0.05f; //%
	private int _currentTestPoint = 0;
	private int _initialSpeedTrialCount;
	private int _baselineTrialCount;
	public List<TestType> TestList;
	public TestType CurrentTest;
	private Queue<VehicleVariable> Vehicles;//Queued up the four vehicles such that we go through the list performing the experiments on all vehicles
	private int _trialPerIncrement;
	private bool _ignoreYellowZone = false;
	private bool _checkTimeFlag = false;
	private bool _fileClosedFlag = false;

	//private Vector3 _StartingPosition;
	//private Quaternion _StartingRotation;
	//private Rigidbody _VehicleRigidbody;
	//private float _Dwelling;
	//private float _PriorFixedUpdateTime;
	private float _ExtremumValue;
	private float _AsymptoteValue;
	private float _ExtremumSlip;
	private float _AsymptoteSlip;
	private float _Stiffness;
	//private bool _StartDwell;
	private bool _Spawned;
	private bool _BrakesEngaged;
	private StringMessageData _TrialMessage;
	//private SimpleAICarInitialCondition CarInit;    
	private int _Trial = 0;
	private int _TotalTrialCount = 0;
	private GameObject RedPoint = null;
	private GameObject YellowPoint = null;    
	private bool _StopTriggered = false;
	private bool _YellowTriggered = false;
	private Vector3 _StopPosition;
	//private UnityStandardAssets.Vehicles.Car.CarController CarRefController;
	private double DelayTime = 0.2;//Deprecated from Demo Mode
	private double Delay = 0;//Deprecated from Demo Mode
	private float _PriorIncline = 0;//Deprecated from Demo Mode
	private float _CurrentIncline = -15;//Deprecated from Demo Mode
	private Transform _FrontOfShuttleCar;
	private bool _UserInputAwait = false;
	private bool _StopFixTrigger = false;
	private float _TorqueInput;
	private float _BrakeTorque;
	private float _yellowZone;
	private float _StiffnessVal;
	//private int _RepeatTrial = 0;
	private float _wheelRadius = 0;
	private bool _YellowBrakingEngaged = false;
	//private bool _VehicleStopped = false;
	private bool _AchievedSteadyState = false;

	//Variables to record at the end of a trial
	private float _MinDistanceToVehicle = 10000; //Arbitrary high amount
	private float _MinFrontPlane = 10000;
	private float _InitialSpeed = 0;
	//private float _BrakeForceVar = 0;
	private float _AsForce = 0;
	private float _AsSlip = 0;
	private float _ExForce = 0;
	private float _ExSlip = 0;
	private float _Incline = 0;
	private int _BadTimeStepCount = 0;
	private float _PriorFixedUpdateTime = 0;
	private string _Error = "";
	//And I can easily get the mass from the rigidbody

	private BoxCollider _bodyCollider;
	//Hold data structures of data read in from config
	public VehicleVariable _currentVehicleVar;
	public SensVariable _gradeVar;
	public SensVariable _initialSpeedVar;
	public SensVariable _rollingResistanceVar;
	public SensVariable _brakingForceVar;
	public SensVariable _lagTimeVar;
	public SensVariable _asymptoteValVar;
	public SensVariable _asymptoteSlpVar;
	public SensVariable _extremeValVar;
	public SensVariable _extremeSlpVar;
	public SensVariable _MWCXposVar;
	public List<GameObject> MWCCategoricalVar;
	public int _MWCIndex = 0;
	public List<GameObject> YellowZoneCategoricalVar;
	public int _YellowZoneCategoricalIndex = 0;
	public bool IsTwoStageBraking = false;
	public bool IsDemo = false;

	[HideInInspector]
	public string VehicleHeaderInfo;
	
	//private Queue<MassSensVariable> _massSensVars;
	private string[] _vehicleExpConfigs;
	private int SensTestIndex;
	private string _CurrentVariableValue;

	private StringBuilder _textDisplayBuilder;
	private WheelCollider[] _Wheels;
	private WheelCollider _FLWheel;
	private WheelCollider _FRWheel;
	private WheelCollider _RLWheel;
	private WheelCollider _RRWheel;
	private Rigidbody _VehRigidbody;
	private float[] _PriorVelocities; //For fricion fix
	//private float _LastVelocity = 0; //For friction calculation	
	private bool _IsExperimentStarted = false;
	private bool _WaitForFrameBuffer = false;
	private const int FrameBuffer = 3;
	private int _CurrentFrame = 0;
	private const int GraphBuffer = 10;
	private int _GraphBufferCount = 10;
	[HideInInspector]
	public bool HideGraph = false;
	[HideInInspector]
	public bool ShowPIDOutput = false;
	private VehicleState _VehState = VehicleState.SpeedUp;
	private float _SteadyStateStartTime = 0;
	//private float _StartInitialSpeedTime = 0;
	private float _InitialSpeedSSDuration = 0;
	private float _YellowZoneBrakingStartTime = 0;
	private float _YellowZoneBrakingDuration = 0;
	private float _YellowZoneSteadyStartTime = 0;
	private float _YellowZoneSteadStateDuration = 0;
	private float _RedZoneBrakingStartTime = 0;
	private float _RedZoneBrakingDuration = 0;
	private int _NumSamples = 0;
	private float _MaxDeltaTime = 0;
	private Vector3 _YellowTriggerPosition;
	private float _YellowZoneDistance;
	private Vector3 _RedTriggerPosition;
	private float _RedZoneDistance;
	public GameObject CarReference;
	public GameObject ProxSystem;
	public Transform Miner;
	public LogManager LogManagerRef;
	public GameObject ShuttleCar;
	//Prefab refs for our 4 vehicles
	public GameObject BH20Car;
	public GameObject BH18Car;
	public GameObject TenSC32B;
	public GameObject EightSixteen;
	public GameObject BH20_LargeYellow;
	public GameObject BH18_LargeYellow;
	public GameObject TenSC32B_LargeYellow;
	public GameObject EightSixteen_LargeYellow;

	public GameObject MWCCenter;//nominal position
	public GameObject MWCLeft;//positioned in unity 9 feet from center (1ft from 20ft entry wall)
	public GameObject MWCRight;
	public Transform MineWearableComponent;

	public GameObject Mine;
	//Visualization Variables
	public Text DebugDisplayLabel;
	public InputField InclineDisplayField;
	public InputField ExtremumValueDisplayField;
	public InputField AsymptoteValueDisplayField;
	public InputField ExtremumSlipDisplayField;
	public InputField AsymptoteSlipDisplayField;
	public GameObject ChaseCamera;
	public Cloth[] Curtains;//Curtains for a demo
	//public WMG_Series Series1Data;
	//public WMG_Series OutputSeriesData;
	//public WMG_Axis_Graph Graph;
	public ProxSystemController Prox;
	public Text StopDistLabel;
	public InputField BrakeTorqueField;
	public InputField MillisecondDelay;
	public InputField StiffnessField;

	public Text TextDisplayPanel;
	public Transform SideCamera_T;

	private StreamWriter continuousFile;
	private StreamWriter endOfTrialFile;

	private bool _brakingStarted = false;
	private Vector3 _brakingStartPos;
	//private float _brakingStartTime;

	[HideInInspector]
	public VehicleExperimentSceneController VehicleSceneController;
	

	public void ExperimentStart()
	{        
		_textDisplayBuilder = new StringBuilder();
		_Trial = 0;
		_ExtremumValue = 0.9f;
		ExtremumValueDisplayField.text = _ExtremumValue.ToString();
		_AsymptoteValue = 0.8f;
		AsymptoteValueDisplayField.text = _AsymptoteValue.ToString();
		_ExtremumSlip = 0.2f;
		ExtremumSlipDisplayField.text = _ExtremumSlip.ToString();
		_AsymptoteSlip = 0.5f;
		AsymptoteSlipDisplayField.text = _AsymptoteSlip.ToString();
		InclineDisplayField.text = Mine.transform.rotation.eulerAngles.x.ToString();
		//_StartDwell = false;
		_Spawned = false;
		_BrakesEngaged = false;
		_brakingStarted = false;
		_TrialMessage = new StringMessageData();
		//CarInit = gameObject.GetComponent<SimpleAICarInitialCondition>();
		//_VehicleRigidbody = _CarReference.GetComponent<Rigidbody>();        
		LogManager.LogMessageEntered += LogManager_LogMessageEntered;//Subscribing to the log manager events for demo purposes
		//We want high precision timing for an experiment. Not Unity's Time class.
		//Series1Data.pointValues = new WMG_List<Vector2>();
		//Series1Data.StartRealTimeUpdate();
		_PriorVelocities = new float[3];
		_YellowTriggerPosition = new Vector3();
		_RedTriggerPosition = new Vector3();

		for(int i=0; i<3; i++)
		{
			_PriorVelocities[i] = -1;
		}
		
		//_vehicleExpConfigs = File.ReadAllLines(Path.Combine(Application.dataPath, "vehicleConfig.txt"));
		//_TestList = new List<TestType>();
		//_TestList.Add(TestType.InclineSens);
		//_TestList.Add(TestType.SpeedSens);
		//_TestList.Add(TestType.FrictionSens);
		//_TestList.Add(TestType.BreakTorqueSens);
		//_TestList.Add(TestType.DelaySens);
		//_TestList.Add(TestType.AsForce);
		//_TestList.Add(TestType.AsSlip);
		//_TestList.Add(TestType.ExForce);
		//_TestList.Add(TestType.ExSlip);
		//_TestList.Add(TestType.MWCLocation);
		//_TestList.Add(TestType.YellowZoneSens);
		//_TestList.Add(TestType.BrakingModel);
		//_TestList.Add(TestType.MassSens);
		MWCCategoricalVar = new List<GameObject>();
		MWCCategoricalVar.Add(MWCCenter);
		MWCCategoricalVar.Add(MWCLeft);
		MWCCategoricalVar.Add(MWCRight);
		
		//Comment this out
		//Vehicles = new Queue<MassSensVariable>();
		//ParseConfigs(_vehicleExpConfigs);
		//_currentTest = _TestList[0];
		//_currentVehicleVar = Vehicles.Dequeue();
		//ShuttleCar = _currentVehicleVar.VehicleToLoad;
		SensTestIndex = 0;
		string _path = Directory.GetParent(Application.dataPath).FullName;
		_path = Path.Combine(_path, "Logs");
		if (!Directory.Exists(_path))
		{
			Directory.CreateDirectory(_path);
		}
		string _endOfTrialPath = _path;
		string twoStage = _currentVehicleVar.IgnoreYellowZone ? "OneStage" : "Twostage";
		_path = Path.Combine(_path, string.Format("{0}_Log_{1}_{2}_{3}_{4}.csv", "ContinuousLog", _currentVehicleVar.ZoneName, _currentVehicleVar.MwcLoc.ToString(), twoStage, DateTime.Now.ToString("yyyy-MM-ddTHHmmss")));
		//continuousFile = File.CreateText(_path);
		//continuousFile.WriteLine(VehicleHeaderInfo);
		if (!IsDemo)
		{
			_endOfTrialPath = Path.Combine(_endOfTrialPath, string.Format("{0}_{1}_{2}.csv", "Summary", _currentVehicleVar.VehicleToLoad.name, DateTime.Now.ToString("yyyy-MM-dd")));
			endOfTrialFile = File.CreateText(_endOfTrialPath);
			endOfTrialFile.WriteLine(VehicleHeaderInfo);
			endOfTrialFile.WriteLine("TotalTrialCount,Trial,FinalFrontPlaneDist_f,FinalClosestPointToMachine_f,MinFrontPlaneDist_f,MinClosestPointToMachine_f,TestVariable,InitialSpeed_fps,BrakeTorque_ft-lb,DelayTime_s,AsymptoteForce,AsymptoteSlip,ExtremumForce,ExtremumSlip,Incline_deg,Mass_kg,IsTwoStageBraking,MWCLocation,AchievedSteadyState,InitialSpeedSteadyStateDuration_s,YellowZoneBrakingDuration_s,YellowZoneSteadyStateDuration_s,BadTimeSteps,PrefabName,Vehicle,ZoneSetup,TrialDuration_s,RedZoneBrakingDuration_s,NumSamples,MaxDeltaTime_s,YellowZoneStopDistance_f,RedZoneStopDistance_f,TotalStoppingDistance_f,Errors");
			StringMessageData vehicleHeader = new StringMessageData();
			vehicleHeader.Message = VehicleHeaderInfo;
			LogManagerRef.AddPacketToQueue(vehicleHeader);
		}
		if (HideGraph)
		{
			//Graph.gameObject.SetActive(false);
		}
		else
		{
			//-10 to 10 for PID, 0 to 7 normally
			//Graph.yAxis.AxisMinValue = 0;
			//Graph.yAxis.AxisMaxValue = 10;
			//Graph.yAxis.AxisNumTicks = 6;
			//Graph.yAxis.hideGrid = true;
			//Graph.yAxis.MaxAutoGrow = false;
			//Graph.yAxis.SetLabelsUsingMaxMin = true;

			//Graph.xAxis.AxisMinValue = 0;
			//Graph.xAxis.AxisMaxValue = 26;
			//Graph.xAxis.AxisNumTicks = 13;
			//Graph.xAxis.hideGrid = true;
			//Graph.xAxis.MaxAutoGrow = false;
			//Graph.xAxis.SetLabelsUsingMaxMin = true;
		}
		
		//_currentTest = _TestList[0];

		//Restart();

		//_IsExperimentStarted = true;
		//ClearFirstFrame();
		_CurrentFrame = Time.frameCount;
		_WaitForFrameBuffer = true;
	}
   
	#region OldConfig
	private void ParseConfigs(string[] vehicleExpConfigs)
	{
		for(int i = 0; i < vehicleExpConfigs.Length; i++)
		{
			if(vehicleExpConfigs[i][0] == '#')
			{
				continue;
			}
			string[] splitLine = vehicleExpConfigs[i].Split('\t');
			if(splitLine.Length < 1)
			{
				continue;
			}            
			switch (splitLine[0])
			{
				case "TrialsPerIncrement":                    
					if (int.TryParse(splitLine[1],out _trialPerIncrement))
					{
						StringMessageData trialPerInc = new StringMessageData();
						trialPerInc.Message = "Trials Per Increment: " + _trialPerIncrement;
						LogManagerRef.AddPacketToQueue(trialPerInc);
					}
					break;
				case "Grade":
					bool minGradeCheck = float.TryParse(splitLine[1], out _gradeVar.MinVal);
					bool nominalGradeCheck = float.TryParse(splitLine[2], out _gradeVar.NominalVal);
					bool maxGradeCheck = float.TryParse(splitLine[3], out _gradeVar.MaxVal);
					if(minGradeCheck && nominalGradeCheck && maxGradeCheck)
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

					if(minSpeedCheck && nominalSpeedCheck && maxSpeedCheck)
					{
						StringMessageData initSpeedMsg = new StringMessageData();
						//Convert speeds to mph from f/s
						_initialSpeedVar.MinVal = 0.681818f * _initialSpeedVar.MinVal;
						_initialSpeedVar.NominalVal = 0.681818f * _initialSpeedVar.NominalVal;
						_initialSpeedVar.MaxVal = 0.681818f * _initialSpeedVar.MaxVal;
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
					if(minAVCheck && nomAVCheck && maxAVCheck)
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
				case "MWCPosition":
					bool minMWCPos = float.TryParse(splitLine[1], out _MWCXposVar.MinVal);
					bool nomMWCPos = float.TryParse(splitLine[2], out _MWCXposVar.NominalVal);
					bool maxMWCPos = float.TryParse(splitLine[3], out _MWCXposVar.MaxVal);
					if (minMWCPos && nomMWCPos && maxMWCPos)
					{
						StringMessageData esMsg = new StringMessageData();
						esMsg.Message = "Extremum Slip Variables: " + _MWCXposVar.MinVal + " min, " + _MWCXposVar.NominalVal + " nom, " + _MWCXposVar.MaxVal + " max";
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
					import.Vehicle = ShuttleCarExpVehicles.Type10SC32B;
					import.VehicleToLoad = TenSC32B;
					bool emptyVal = float.TryParse(splitLine[1], out import.EmptyMass);
					bool halfVal = float.TryParse(splitLine[2], out import.HalfMass);
					bool fullVal = float.TryParse(splitLine[3], out import.FullMass);
					bool overVal = float.TryParse(splitLine[4], out import.Overload);
					bool nomSpeedVal = float.TryParse(splitLine[5], out import.NominalSpeed);
					if (emptyVal && halfVal && fullVal && overVal && nomSpeedVal)
					{
						StringMessageData sc32Msg = new StringMessageData();
						sc32Msg.Message = "10SC32 Variables: " + import.EmptyMass + " empty, " + import.HalfMass + " half, " + import.FullMass + " full, " + import.Overload + " overload, " + import.NominalSpeed + " nomSpd";
						LogManagerRef.AddPacketToQueue(sc32Msg);
						Vehicles.Enqueue(import);
					}
					else
					{
						Debug.LogError("Failure to read vehicle var");
					}
					
					break;
				case "BH20":
					//_TestQueue.Enqueue(TestType.MassSens);
					VehicleVariable bh20 = new VehicleVariable();
					//bh20.PrefabsDefinedYellowZone = new Queue<GameObject>();

					bh20.Vehicle = ShuttleCarExpVehicles.TypeBH20;
					bh20.VehicleToLoad = BH20Car;
					
					//bh20.PrefabsDefinedYellowZone.Enqueue()

					bool emptyVal2 = float.TryParse(splitLine[1], out bh20.EmptyMass);
					bool halfVal2 = float.TryParse(splitLine[2], out bh20.HalfMass);
					bool fullVal2 = float.TryParse(splitLine[3], out bh20.FullMass);
					bool overVal2 = float.TryParse(splitLine[4], out bh20.Overload);
					bool nomSpeedVal2 = float.TryParse(splitLine[5], out bh20.NominalSpeed);
					if (emptyVal2 && halfVal2 && fullVal2 && overVal2 && nomSpeedVal2)
					{
						StringMessageData bh20Msg = new StringMessageData();
						bh20Msg.Message = "BH20 Variables: " + bh20.EmptyMass + " empty, " + bh20.HalfMass + " half, " + bh20.FullMass + " full, " + bh20.Overload + " overload, " + bh20.NominalSpeed + " nomSpd";
						LogManagerRef.AddPacketToQueue(bh20Msg);
						Vehicles.Enqueue(bh20);
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
					bh18ac.VehicleToLoad = BH18Car;
					bool emptyVal3 = float.TryParse(splitLine[1], out bh18ac.EmptyMass);
					bool halfVal3 = float.TryParse(splitLine[2], out bh18ac.HalfMass);
					bool fullVal3 = float.TryParse(splitLine[3], out bh18ac.FullMass);
					bool overVal3 = float.TryParse(splitLine[4], out bh18ac.Overload);
					bool nomSpeedVal3 = float.TryParse(splitLine[5], out bh18ac.NominalSpeed);
					if (emptyVal3 && halfVal3 && fullVal3 && overVal3 && nomSpeedVal3)
					{
						StringMessageData bh18Msg = new StringMessageData();
						bh18Msg.Message = "BH18AC Variables: " + bh18ac.EmptyMass + " empty, " + bh18ac.HalfMass + " half, " + bh18ac.FullMass + " full, " + bh18ac.Overload + " overload, " + bh18ac.NominalSpeed + " nomSpd";
						LogManagerRef.AddPacketToQueue(bh18Msg);
						Vehicles.Enqueue(bh18ac);
					}
					else
					{
						Debug.LogError("Failure to read vehicle var");
					}
					
					break;
				case "816-2000":
					VehicleVariable eight16 = new VehicleVariable();
					eight16.Vehicle = ShuttleCarExpVehicles.Type8162000;
					eight16.VehicleToLoad = EightSixteen;
					bool emptyVal4 = float.TryParse(splitLine[1], out eight16.EmptyMass);
					bool halfVal4 = float.TryParse(splitLine[2], out eight16.HalfMass);
					bool fullVal4 = float.TryParse(splitLine[3], out eight16.FullMass);
					bool overVal4 = float.TryParse(splitLine[4], out eight16.Overload);
					bool nomSpeedVal4 = float.TryParse(splitLine[5], out eight16.NominalSpeed);
					if (emptyVal4 && halfVal4 && fullVal4 && overVal4 && nomSpeedVal4)
					{
						StringMessageData eightMsg = new StringMessageData();
						eightMsg.Message = "816-2000 Variables: " + eight16.EmptyMass + " empty, " + eight16.HalfMass + " half, " + eight16.FullMass + " full, " + eight16.Overload + " overload, " + eight16.NominalSpeed + " nomSpd";
						LogManagerRef.AddPacketToQueue(eightMsg);
						Vehicles.Enqueue(eight16);
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
	#endregion
	/// <summary>
	/// Logging handle for the experiment
	/// </summary>
	/// <param name="arg0"></param>
	private void LogManager_LogMessageEntered(LogPacket arg0)
	{

		MobileEquipmentData data = null;
		if (Timer.GetTime() < 20)
		{
			if (!HideGraph)
			{
				if (arg0.GetType() == typeof(MobileEquipmentData))
				{
					data = arg0 as MobileEquipmentData;
					if (data != null && Timer.GetTime() > 0.25f)
					{
						if (_GraphBufferCount > GraphBuffer)
						{
							//Series1Data.pointValues.Add(new Vector2((float)Timer.GetTime(), data.Velocity * 3.28084f));
							_GraphBufferCount = 0;
						}
						else
						{
							_GraphBufferCount++;
						}
					}
				}
			}
			if (arg0.GetType() == typeof(EventLogData))
			{
				DebugDisplayLabel.text = arg0.ToString();
			}
		}
	}

	void Update()
	{
		//If we're in a condition where we are waiting for user input, flag it back to allow respawning, mostly for demos
		if (Timer.GetTime() >= 25 && _UserInputAwait) 
		{
			if(Input.GetKeyUp(KeyCode.R))
				_UserInputAwait = false;
		}

		//Clear frames when the level loads
		if(_WaitForFrameBuffer && Time.frameCount > (_CurrentFrame + FrameBuffer))
		{
			_WaitForFrameBuffer = false;
			Restart();
			_IsExperimentStarted = true;
		}

		/*
		StringBuilder sbStatus = new StringBuilder();
		sbStatus.AppendFormat("{0} ", _VehState.ToString());
		var wheelCols = CarRefController.GetWheelColliders();
		for (int i = 0; i < wheelCols.Length; i++)
		{
			WheelCollider wc = wheelCols[i];
			sbStatus.AppendFormat("{0:F2} ", wc.brakeTorque);
		}

		VehicleExperimentSceneController.SetStatusText(sbStatus.ToString());
		*/
		UpdateStatusText();
	}

	private void UpdateStatusText()
	{
		if (CarReference == null || _currentVehicleVar == null && _currentVehicleVar != null && _currentVehicleVar.VehicleToLoad != null)
			return;

		StringBuilder sbStatus = new StringBuilder();

		/*
		switch (_currentVehicleVar.Vehicle)
		{
			case ShuttleCarExpVehicles.TypeBH20:
				sbStatus.AppendFormat("Ram Car ");
				break;

			case ShuttleCarExpVehicles.TypeBH18AC:
				sbStatus.AppendFormat("BH18AC ");
				break;

			case ShuttleCarExpVehicles.Type8162000:
				sbStatus.AppendFormat("816 ");
				break;

			case ShuttleCarExpVehicles.Type10SC32B:
				sbStatus.AppendFormat("10SC32B ");
				break;
		}*/
		sbStatus.AppendFormat("{0} ", _currentVehicleVar.VehicleToLoad.name);

		//sbStatus.AppendFormat("GradeScene:{0} ", _currentVehicleVar.SceneGradeToLoadIndex);
		switch (_currentVehicleVar.SceneGradeToLoadIndex)
		{
			case 0:
				sbStatus.Append("-5 ");
				break;

			case 1:
				sbStatus.Append("0 ");
				break;

			case 2:
				sbStatus.Append("5 ");
				break;
		}


		float vehicleMass = CarReference.GetComponent<Rigidbody>().mass;

		if (Mathf.Approximately(vehicleMass,_currentVehicleVar.EmptyMass))
			sbStatus.AppendFormat("Empty ");
		else if (Mathf.Approximately(vehicleMass, _currentVehicleVar.FullMass))
			sbStatus.AppendFormat("Full ");
		else if (Mathf.Approximately(vehicleMass, _currentVehicleVar.HalfMass))
			sbStatus.AppendFormat("Half ");
		else if (Mathf.Approximately(vehicleMass, _currentVehicleVar.Overload))
			sbStatus.AppendFormat("Overloaded ");
		else
			sbStatus.Append("Unknown Mass ");

		/*
		sbStatus.AppendFormat("Mass:{0:F2} ", vehicleMass);

		if (CarRefController.IsYellowZone)
			sbStatus.Append("IsYellowZone ");
		if (CarRefController.RedZoneOverride)
			sbStatus.Append("RedZoneOverride ");

		sbStatus.AppendFormat("{0} ", _VehState.ToString());
		var wheelCols = CarRefController.GetWheelColliders();
		for (int i = 0; i < wheelCols.Length; i++)
		{
			WheelCollider wc = wheelCols[i];
			sbStatus.AppendFormat("{0:F2} ", wc.brakeTorque);
		}*/

		VehicleExperimentSceneController.SetStatusText(sbStatus.ToString());
	}
	
	// void FixedUpdate()
	// {
	// 	if (_IsExperimentStarted)
	// 	{
			
	// 		_textDisplayBuilder.Length = 0;
	// 		_textDisplayBuilder.AppendFormat("{0,-26}: {1:F2}\n", "Incline", _PriorIncline);
	// 		_textDisplayBuilder.AppendFormat("{0,-26}: {1:F2}\n", "Extremum Value", _ExtremumValue);
	// 		_textDisplayBuilder.AppendFormat("{0,-26}: {1:F2}\n", "Asymptote Value", _AsymptoteValue);
	// 		_textDisplayBuilder.AppendFormat("{0,-26}: {1:F2}\n", "Extremum Slip", _ExtremumSlip);
	// 		_textDisplayBuilder.AppendFormat("{0,-26}: {1:F2}\n", "Asymptote Slip", _AsymptoteSlip);
	// 		_textDisplayBuilder.AppendFormat("{0,-26}: {1}\n", "Brakes Engaged", _BrakesEngaged ? "Yes" : "No");

	// 		//Default values of the stop distance label
	// 		StopDistLabel.text = string.Format("Stopping Distance (ft): {0,6}       Distance To PWD (ft): {1,6}", "N/A", "N/A");
	// 		if (_Spawned && ShuttleCar != null && _FrontOfShuttleCar != null)
	// 		{
	// 			_NumSamples++;
	// 			if (_PriorFixedUpdateTime == 0)
	// 			{
	// 				_PriorFixedUpdateTime = Time.time;
	// 			}
	// 			else
	// 			{
	// 				float deltaTime = Time.time - _PriorFixedUpdateTime;
	// 				if (deltaTime > (Time.fixedDeltaTime * 1.2f))
	// 				{
	// 					_BadTimeStepCount++;
	// 				}
	// 				_PriorFixedUpdateTime = Time.time;
	// 				if(deltaTime > _MaxDeltaTime)
	// 				{
	// 					_MaxDeltaTime = deltaTime;
	// 				}
	// 			}
	// 			Vector3 shuttleCarPos = _FrontOfShuttleCar.position;
	// 			Vector3 minerPos = Miner.position;

				
	// 			shuttleCarPos.x = minerPos.x;

	// 			float DistanceToPerson = 0; // Vector3.Distance(shuttleCarPos, minerPos);

	// 			//compute distance to person as distance along the forward vector the the shuttle car spawner transform
	// 			Vector3 distanceToPersonVector = minerPos - shuttleCarPos;
	// 			DistanceToPerson = Vector3.Dot(distanceToPersonVector, transform.forward);

	// 			float ForwardDistanceToPerson = (minerPos.z - _FrontOfShuttleCar.position.z) / Mathf.Cos(Mine.transform.rotation.eulerAngles.x * Mathf.Deg2Rad);                
	// 			float ClosestPoint = Vector3.Distance(minerPos, _bodyCollider.ClosestPoint(minerPos));

	// 			DistanceToPerson = DistanceToPerson * 3.28084f;
	// 			ForwardDistanceToPerson = ForwardDistanceToPerson * 3.28084f;
	// 			ClosestPoint = ClosestPoint * 3.28084f;
	// 			if(ForwardDistanceToPerson < _MinFrontPlane)
	// 			{
	// 				_MinFrontPlane = ForwardDistanceToPerson;
	// 			}
	// 			if(ClosestPoint < _MinDistanceToVehicle)
	// 			{
	// 				_MinDistanceToVehicle = ClosestPoint;
	// 			}

	// 			float StopDistance = Vector3.Distance(_StopPosition, CarReference.transform.position);
	// 			StopDistance = StopDistance * 3.28084f;//meters to feet

	// 			string distanceToPersonText = null;
	// 			if (DistanceToPerson >= 0)
	// 				distanceToPersonText = string.Format("{0,6:F2}", DistanceToPerson);
	// 			else
	// 				distanceToPersonText = "<color=#ff0000ff>COLLISION</color>";

	// 			//Setting values for our text label displaying stopping distances
	// 			if (_brakingStarted)
	// 			{
	// 				Vector3 stopVector = CarReference.transform.position - _brakingStartPos;
	// 				StopDistLabel.text = string.Format("Stopping Distance (ft): {0,6:F2}    Distance To MWC (ft): {1,6}", stopVector.magnitude * 3.28084f, distanceToPersonText);
	// 			}
	// 			else
	// 			{
	// 				StopDistLabel.text = string.Format("Stopping Distance (ft): {0,6}    Distance To MWC (ft): {1,6}", "N/A", distanceToPersonText);
	// 			}

	// 			float velocity = CarReference.transform.InverseTransformDirection(CarReference.GetComponent<Rigidbody>().velocity).z * 3.28084f; //Convert vehicle velocity to feet per second rather than meters per second
	// 			for (int i = _PriorVelocities.Length - 1; i > 0; i--)
	// 			{
	// 				_PriorVelocities[i] = _PriorVelocities[i - 1];                    
	// 			}
	// 			_PriorVelocities[0] = _VehRigidbody.velocity.magnitude;
				
	// 			if (!HideGraph)
	// 			{
	// 				//Series1Data.pointValues.Add(new Vector2((float)Timer.GetTime(), CarReference.GetComponent<Rigidbody>().velocity.magnitude * 3.28084f));
	// 				Series1Data.pointValues.Add(new Vector2((float)Timer.GetTime(), velocity));
	// 			}
	// 			StringMessageData str = new StringMessageData();
	// 			str.Message = Timer.GetTime() + ", " + _CurrentVariableValue + ", " + ForwardDistanceToPerson + ", " + ClosestPoint + ", "+ velocity + ", " + CarReference.transform.position.x + ", " + CarReference.transform.position.y + ", " + CarReference.transform.position.z + ", " + _Trial;
	// 			//LogManagerRef.AddPacketToQueue(str);
	// 			float rpmToradps = 2 * Mathf.PI / 60;
	// 			//Debug.Log(_RRWheel.name);
	// 			string output = ""; 
	// 			output = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34}", Timer.GetTime(), CarReference.transform.position.x * 3.28084f, CarReference.transform.position.y * 3.28084f, CarReference.transform.position.z * 3.28084f, CarReference.transform.rotation.x, CarReference.transform.rotation.y, CarReference.transform.rotation.z, CarReference.transform.rotation.w, Miner.transform.position.x * 3.28084f, Miner.transform.position.y * 3.28084f, Miner.transform.position.z * 3.28084f, _VehRigidbody.velocity.magnitude * 3.28084f, _VehRigidbody.angularVelocity.x, _VehRigidbody.angularVelocity.y, _VehRigidbody.angularVelocity.z, _FLWheel.rpm * rpmToradps, _FRWheel.rpm * rpmToradps, _RLWheel.rpm * rpmToradps, _RRWheel.rpm * rpmToradps, _FLWheel.motorTorque * 0.7375621f, _FRWheel.motorTorque * 0.7375621f, _RLWheel.motorTorque * 0.7375621f, _RRWheel.motorTorque * 0.7375621f, _FLWheel.brakeTorque * 0.7375621f, _FRWheel.brakeTorque * 0.7375621f, _RLWheel.brakeTorque * 0.7375621f, _RRWheel.brakeTorque * 0.7375621f, _FLWheel.isGrounded, _FRWheel.isGrounded, _RLWheel.isGrounded, _RRWheel.isGrounded, ForwardDistanceToPerson, ClosestPoint, Prox.ActiveProxZone.ToString(), _VehState);
	// 			if (!_fileClosedFlag && !IsDemo)
	// 			{
	// 				try
	// 				{
	// 					continuousFile.WriteLine(output);
	// 					continuousFile.Flush();
	// 				}
	// 				catch
	// 				{
	// 					Debug.LogError("Failed to write to continuous file! Trial number: " + _TotalTrialCount);
	// 					Application.Quit();
	// 				}
	// 			}
	// 			if (Timer.GetTime() >= 90 && !_UserInputAwait) //Trial automatically restarts after 25 seconds have passed
	// 			{
	// 				_TrialMessage.Message = _CurrentVariableValue + ", " + StopDistance + ", " +  ForwardDistanceToPerson + ", " + ClosestPoint + ", " + velocity + ", " + _Trial;
	// 				LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 				//string message = Timer.GetTime() + "," +  _TrialMessage.Message;
	// 				// endOfTrialFile.WriteLine(message);
	// 				string message = "";
	// 				string twoStage = _currentVehicleVar.IgnoreYellowZone ? "OneStage" : "Twostage";
	// 				if(_BadTimeStepCount > 0)
	// 				{
	// 					_Error += "Bad time step(s) detected; ";
	// 				}
	// 				_Error += "Timeout";
	// 				message = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33}", _TotalTrialCount, _Trial, ForwardDistanceToPerson, ClosestPoint, _MinFrontPlane, _MinDistanceToVehicle, _CurrentVariableValue, _InitialSpeed, _BrakeTorque, DelayTime, _AsForce, _AsSlip, _ExForce, _ExSlip, _Incline, _VehRigidbody.mass, twoStage, Miner.name, _AchievedSteadyState, _InitialSpeedSSDuration, _YellowZoneBrakingDuration, _YellowZoneSteadStateDuration, _BadTimeStepCount, _currentVehicleVar.VehicleToLoad.name, _currentVehicleVar.VehicleName, _currentVehicleVar.ZoneSetup, ((float)Timer.GetTime()).ToString(), _RedZoneBrakingDuration.ToString(), _NumSamples, _MaxDeltaTime, _YellowZoneDistance * 3.28084f, _RedZoneDistance * 3.28084f, (_YellowZoneDistance + _RedZoneDistance) * 3.28084f, _Error);
	// 				if (!IsDemo)
	// 				{
	// 					endOfTrialFile.WriteLine(message);
	// 				}
	// 				Restart();
	// 			}

	// 			if (Timer.GetTime() <= 2)
	// 			{
	// 				CarReference.GetComponent<Rigidbody>().velocity = Vector3.zero;
	// 				//CarReference.GetComponent<Rigidbody>().velocity = (CarReference.transform.forward) * (CarRefController.MaxSpeed * 0.44704f);//Float value is mph to met/s
	// 				Prox.EnableZoneVisualization(new global::ProxSystem.VisOptions(true, true));
	// 				CarRefController.SetMaxSpeed(0);
	// 				CarRefController.Move(0, 0, -1, 0);
	// 			}
	// 			if (Timer.GetTime() > 2)
	// 			{
	// 				WheelHit hit;
	// 				_Wheels[0].GetGroundHit(out hit);
	// 				//Debug.Log(hit.force + ", " + hit.forwardSlip);

	// 				Vector3 eulerAngles = CarRefController.transform.eulerAngles;
	// 				eulerAngles.y = 0;
	// 				CarRefController.transform.rotation = Quaternion.Euler(eulerAngles);



	// 				switch (_VehState)
	// 				{
	// 					case VehicleState.SpeedUp:
	// 						CarRefController.SetMaxSpeed(_InitialSpeed * 0.681818f);//To MPH friendly for unity car controller
	// 						CarRefController.Move(0, 1, 0, 0);

	// 						eulerAngles = CarRefController.transform.eulerAngles;
	// 						eulerAngles.y = 0;
	// 						CarRefController.transform.rotation = Quaternion.Euler(eulerAngles);

	// 						//Get the error from the car controller to determine steady state
	// 						//if (_VehRigidbody.velocity.magnitude >= CarRefController.GetMaxSpeedMPS() || Mathf.Approximately(_VehRigidbody.velocity.magnitude, CarRefController.GetMaxSpeedMPS())){                                
	// 						//    _VehState = VehicleState.InitialSteady;
	// 						//    _SteadyStateStartTime = (float)Timer.GetTime();
	// 						//}
	// 						/*
	// 						bool isSteadyState = true;
	// 						for(int i = 0; i < _PriorVelocities.Length; i++)
	// 						{
								
	// 							if (_PriorVelocities[i] * 3.28084f < (_InitialSpeed - (_InitialSpeed * SteadyStateTolerance)))
	// 							{
	// 								isSteadyState = false;
	// 							}                                
	// 						} */

							
	// 						bool isSteadyState = false;
	// 						if (_VehRigidbody.velocity.magnitude * 3.28084f >= (_InitialSpeed - (_InitialSpeed * SteadyStateTolerance)))
	// 						{
	// 							isSteadyState = true;
	// 						}

	// 						if (isSteadyState)//Steady state if our PID output is nearing zero. Less than 1%
	// 						{
	// 							_VehState = VehicleState.InitialSteady;
	// 							_SteadyStateStartTime = (float)Timer.GetTime();
	// 						}

	// 						if (Prox.ActiveProxZone == ProxZone.YellowZone)
	// 						{
	// 							_VehState = VehicleState.Yellow;
	// 							_YellowTriggerPosition = CarReference.transform.position;
	// 							if (!HideGraph)
	// 							{
	// 								WMG_Series YellowSeries = Graph.addSeries();
	// 								YellowSeries.pointColor = Color.yellow;
	// 								YellowSeries.lineColor = Color.yellow;
	// 								YellowSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 0));
	// 								YellowSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 10));
	// 								YellowSeries.seriesName = "Yellow";
	// 							}
	// 							Delay = Timer.GetTime() + DelayTime;
	// 							_Error += "Never Achieved Steady State; ";
	// 							//_InitialSpeedSSDuration = (float)Timer.GetTime() - _SteadyStateStartTime;                                                               
	// 						}
	// 						//_PriorVelocities[0] = _PriorVelocities[1];
	// 						//_PriorVelocities[1] = _VehRigidbody.velocity.magnitude;
	// 						break;
	// 					case VehicleState.InitialSteady:

	// 						eulerAngles = CarRefController.transform.eulerAngles;
	// 						eulerAngles.y = 0;
	// 						CarRefController.transform.rotation = Quaternion.Euler(eulerAngles);

	// 						CarRefController.Move(0, 1, 0, 0);
	// 						_AchievedSteadyState = true;
	// 						if(Prox.ActiveProxZone == ProxZone.YellowZone)
	// 						{
	// 							_VehState = VehicleState.Yellow;
	// 							_YellowTriggerPosition = CarReference.transform.position;
	// 							if (!HideGraph)
	// 							{
	// 								WMG_Series YellowSeries = Graph.addSeries();
	// 								YellowSeries.pointColor = Color.yellow;
	// 								YellowSeries.lineColor = Color.yellow;
	// 								YellowSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 0));
	// 								YellowSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 10));
	// 								YellowSeries.seriesName = "Yellow";
	// 							}
	// 							Delay = Timer.GetTime() + DelayTime;
	// 							_InitialSpeedSSDuration = (float)Timer.GetTime() - _SteadyStateStartTime;
	// 							if(_InitialSpeedSSDuration < SteadyStateTime)
	// 							{
	// 								_Error += "Steady state less than two seconds";
	// 							}
	// 						}
	// 						break;
	// 					case VehicleState.Yellow:
	// 						if (!_brakingStarted)
	// 						{
	// 							_brakingStarted = true;
	// 							_brakingStartPos = CarReference.transform.position;
	// 							//_brakingStartTime = Time.time;
	// 						}

	// 						if(!_ignoreYellowZone && Timer.GetTime() >= (Delay - Time.fixedDeltaTime))//Ensure that the state transition behavior happens exactly at our delay time, addressing Will's comment
	// 						{
	// 							_VehState = VehicleState.YellowBraking;
	// 							//Moving these to be only in the Yellow Braking state, see above note about ensuring behaviors occur in proper location
	// 							//_YellowZoneBrakingStartTime = (float)Timer.GetTime();
	// 							//CarRefController.SetMaxSpeed(YellowZoneSpeedLimitMPH);
	// 							//CarRefController.IsYellowZone = true;
	// 							_checkTimeFlag = true;
	// 							//CarRefController.Move(0, 0, -0.25f,0);//Perform the first slowdown here, ensures that we don't wait another update, the movement call should only happen in the state itself
	// 						}
	// 						else
	// 						{
	// 							CarRefController.Move(0, 1, 0, 0);
	// 						}

	// 						if (Prox.ActiveProxZone == ProxZone.RedZone)
	// 						{
	// 							_StopPosition = CarReference.transform.position;
	// 							_RedTriggerPosition = CarReference.transform.position;
	// 							_YellowZoneDistance = Vector3.Distance(CarReference.transform.position, _YellowTriggerPosition);
	// 							if (!HideGraph)
	// 							{
	// 								WMG_Series RedSeries = Graph.addSeries();
	// 								RedSeries.pointColor = Color.red;
	// 								RedSeries.lineColor = Color.red;
	// 								RedSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 0));
	// 								RedSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 10));
	// 								RedSeries.seriesName = "Red";
	// 							}
	// 							_VehState = VehicleState.Red;
	// 							CarRefController.Move(0, 1, 0, 0);
	// 							Delay = Timer.GetTime() + DelayTime;
	// 							if (!_ignoreYellowZone)
	// 							{
	// 								_Error += "Never started Yellow Zone Braking; ";
	// 							}
	// 						}

	// 						break;
	// 					case VehicleState.YellowBraking:
	// 						_YellowBrakingEngaged = true;
	// 						CarRefController.IsYellowZone = true;
	// 						CarRefController.SetMaxSpeed(YellowZoneSpeedLimitMPH);
	// 						if (_checkTimeFlag)//Prevent replicating behavior, ensure that the yellow braking start time reflects this
	// 						{
	// 							_YellowZoneBrakingStartTime = (float)Timer.GetTime();                                                             
	// 							_checkTimeFlag = false;
	// 						}
							
	// 						bool isYellowSteady = true;
	// 						for (int i = 0; i < _PriorVelocities.Length; i++)
	// 						{
	// 							if (_PriorVelocities[i] > (YellowZoneSpeedLimit + (YellowZoneSpeedLimit * SteadyStateTolerance)))
	// 							{
	// 								isYellowSteady = false;
	// 							}
	// 						}
	// 						if (isYellowSteady)//OLD DEBUGGING METHOD: _VehRigidbody.velocity.magnitude <= YellowZoneSpeedLimit
	// 						{
	// 							_VehState = VehicleState.YellowSteady;
	// 							//CarRefController.SetMaxSpeed(YellowZoneSpeedLimitMPH);
	// 							CarRefController.Move(0, 1, 0, 0);
	// 							_YellowZoneBrakingDuration += ((float)Timer.GetTime() - _YellowZoneBrakingStartTime);
	// 							_YellowZoneSteadyStartTime = (float)Timer.GetTime();
	// 						}
	// 						else
	// 						{
	// 							CarRefController.Move(0, 0, -0.25f, 0);
	// 						}

	// 						if (Prox.ActiveProxZone == ProxZone.RedZone)
	// 						{
	// 							_StopPosition = CarReference.transform.position;
	// 							_RedTriggerPosition = CarReference.transform.position;
	// 							_YellowZoneDistance = Vector3.Distance(CarReference.transform.position, _YellowTriggerPosition);
	// 							if (!HideGraph)
	// 							{
	// 								WMG_Series RedSeries = Graph.addSeries();
	// 								RedSeries.pointColor = Color.red;
	// 								RedSeries.lineColor = Color.red;
	// 								RedSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 0));
	// 								RedSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 10));
	// 								RedSeries.seriesName = "Red";                                   
	// 							}
	// 							_VehState = VehicleState.Red;
	// 							CarRefController.IsYellowZone = false;
	// 							CarRefController.Move(0, 1, 0, 0);
	// 							Delay = Timer.GetTime() + DelayTime;                                
	// 							_Error += "Never achieved Yellow Zone Steady State; ";                              
	// 						}
	// 						//_PriorVelocities[0] = _PriorVelocities[1];
	// 						//_PriorVelocities[1] = _VehRigidbody.velocity.magnitude;
	// 						break;
	// 					case VehicleState.YellowSteady:
	// 						if(Prox.ActiveProxZone == ProxZone.RedZone)
	// 						{
	// 							_StopPosition = CarReference.transform.position;
	// 							_RedTriggerPosition = CarReference.transform.position;
	// 							_YellowZoneDistance = Vector3.Distance(CarReference.transform.position, _YellowTriggerPosition);                              
	// 							if (!HideGraph)
	// 							{
	// 								WMG_Series RedSeries = Graph.addSeries();
	// 								RedSeries.pointColor = Color.red;
	// 								RedSeries.lineColor = Color.red;
	// 								RedSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 0));
	// 								RedSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 10));
	// 								RedSeries.seriesName = "Red";
	// 							}
	// 							_VehState = VehicleState.Red;
	// 							CarRefController.Move(0, 1, 0, 0);
	// 							Delay = Timer.GetTime() + DelayTime;
	// 							_YellowZoneSteadStateDuration = (float)Timer.GetTime() - _YellowZoneSteadyStartTime;
	// 							if(_YellowZoneSteadStateDuration < SteadyStateTime)
	// 							{
	// 								_Error += "Yellow zone steady state under 2 seconds; ";
	// 							}
	// 						}
	// 						if(_VehRigidbody.velocity.magnitude > (YellowZoneSpeedLimit + YellowZoneSpeedLimit * SteadyStateTolerance))
	// 						{
	// 							_VehState = VehicleState.YellowBraking;
	// 							_Error += "Yellow Braking After Steady State; ";
	// 							_YellowZoneBrakingStartTime = (float)Timer.GetTime();
	// 							_YellowZoneSteadStateDuration = (float)Timer.GetTime() - _YellowZoneSteadyStartTime;
	// 						}
	// 						CarRefController.IsYellowZone = false;
	// 						CarRefController.Move(0, 1, 0, 0);
	// 						break;
	// 					case VehicleState.Red:
							
	// 						if(Timer.GetTime() >= (Delay-Time.fixedDeltaTime))//See comments on Yellow zone braking above
	// 						{
	// 							_VehState = VehicleState.RedBraking;
	// 							//_RedZoneBrakingStartTime = (float)Timer.GetTime();
	// 							//CarRefController.RedZoneOverride = true;
	// 							//CarRefController.SetMaxSpeed(0);
	// 							//CarRefController.Move(0, 0, -1, 0);
	// 							if (_YellowBrakingEngaged)
	// 							{
	// 								_YellowZoneBrakingDuration += (float)Timer.GetTime() - _YellowZoneBrakingStartTime;
	// 								_YellowZoneSteadStateDuration = 0;
	// 								_Error += "Never achieved yellow zone steady state.";//Needed to add an error catch here for the case of yellow braking behavior occuring during red zone delay
	// 							}
	// 							_checkTimeFlag = true;                              
	// 						}
	// 						else
	// 						{
	// 							if (_YellowBrakingEngaged)//For the condition of still performing yellow braking when entering the red zone (yet still in the delay time) 
	// 							{
	// 								bool isYellowSteady2 = true;
	// 								for (int i = 0; i < _PriorVelocities.Length; i++)
	// 								{
	// 									if (_PriorVelocities[i] > (YellowZoneSpeedLimit + (YellowZoneSpeedLimit * SteadyStateTolerance)))
	// 									{
	// 										isYellowSteady2 = false;
	// 									}
	// 								}
	// 								if (isYellowSteady2)
	// 								{
	// 									CarRefController.SetMaxSpeed(YellowZoneSpeedLimitMPH);
	// 									CarRefController.Move(0, 0, -0.25f, 0);
	// 									CarRefController.IsYellowZone = true;
	// 									_YellowBrakingEngaged = false;
	// 									_YellowZoneSteadStateDuration = 0;
	// 									_YellowZoneBrakingDuration += (float)Timer.GetTime() - _YellowZoneBrakingStartTime;
	// 								}
	// 								else
	// 								{
	// 									CarRefController.Move(0, 1, 0, 0);
	// 									CarRefController.IsYellowZone = true;
	// 								}
	// 							}
	// 							else
	// 							{
	// 								CarRefController.Move(0, 1, 0, 0);
	// 							}
	// 						}
	// 						break;
	// 					case VehicleState.RedBraking:
	// 						CarRefController.IsYellowZone = false;
	// 						CarRefController.RedZoneOverride = true;
	// 						CarRefController.SetMaxSpeed(0);

	// 						if (_checkTimeFlag)
	// 						{
	// 							_RedZoneBrakingStartTime = (float)Timer.GetTime();
	// 							_checkTimeFlag = false;                            
	// 						}

	// 						if (_VehRigidbody.velocity.magnitude < 0.008f) //Approximation (uncertainty in the unity model can potentially mean it's never *actually* zero)
	// 						{
	// 							_VehState = VehicleState.Stopped;
	// 							_SteadyStateStartTime = (float)Timer.GetTime();
	// 							_RedZoneBrakingDuration = (float)Timer.GetTime() - _RedZoneBrakingStartTime;
	// 							_RedZoneDistance = Vector3.Distance(CarReference.transform.position, _RedTriggerPosition);
	// 							CarRefController.RedZoneOverride = true;
	// 							CarRefController.Move(0, 0, -1, 0);
	// 						}
	// 						else
	// 						{
	// 							CarRefController.RedZoneOverride = true;
	// 							CarRefController.Move(0, 0, -1, 0);
	// 						}                            
	// 						break;
	// 					case VehicleState.Stopped:
	// 						CarRefController.RedZoneOverride = true;
	// 						CarRefController.SetMaxSpeed(0);
	// 						if (Timer.GetTime() > (_SteadyStateStartTime + SteadyStateTime))
	// 						{
	// 							//Write the end of trial log!!
	// 							//Log the Timer.GetTime
	// 							string message = "";
	// 							string twoStage = _currentVehicleVar.IgnoreYellowZone ? "OneStage" : "Twostage";
	// 							if (_BadTimeStepCount > 0)
	// 							{
	// 								_Error += "Bad time step(s) detected; ";
	// 							}
	// 							message = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33}", _TotalTrialCount, _Trial, ForwardDistanceToPerson, ClosestPoint, _MinFrontPlane, _MinDistanceToVehicle, _CurrentVariableValue, _InitialSpeed, _BrakeTorque, DelayTime, _AsForce, _AsSlip, _ExForce, _ExSlip, _Incline, _VehRigidbody.mass, twoStage, Miner.name, _AchievedSteadyState, _InitialSpeedSSDuration, _YellowZoneBrakingDuration, _YellowZoneSteadStateDuration, _BadTimeStepCount, _currentVehicleVar.VehicleToLoad.name, _currentVehicleVar.VehicleName, _currentVehicleVar.ZoneSetup, ((float)Timer.GetTime()).ToString(), _RedZoneBrakingDuration.ToString(), _NumSamples, _MaxDeltaTime, _YellowZoneDistance * 3.28084f, _RedZoneDistance * 3.28084f, (_YellowZoneDistance + _RedZoneDistance) * 3.28084f, _Error);
	// 							if (!IsDemo)
	// 							{
	// 								endOfTrialFile.WriteLine(message);
	// 							}
	// 							Restart();
	// 						}
	// 						CarRefController.IsYellowZone = false;
	// 						CarRefController.RedZoneOverride = true;
	// 						CarRefController.Move(0, 0, -1, 0);
	// 						break;
	// 					default:
	// 						break;
	// 				}
	// 				#region OldExperimentControl
	// 				//switch (Prox.ActiveProxZone)
	// 				//{
	// 				//    case ProxZone.None:
	// 				//        break;
	// 				//    case ProxZone.GreenZone:
	// 				//        CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 1, 0f, 0f);
	// 				//        break;
	// 				//    case ProxZone.YellowZone:                            
	// 				//        if (!_YellowTriggered)
	// 				//        {
	// 				//            WMG_Series YellowSeries = Graph.addSeries();
	// 				//            YellowSeries.pointColor = Color.yellow;
	// 				//            YellowSeries.lineColor = Color.yellow;
	// 				//            YellowSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 0));
	// 				//            YellowSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 7));
	// 				//            YellowSeries.seriesName = "Yellow";
	// 				//            _YellowTriggered = true;
	// 				//            Delay = Timer.GetTime() + DelayTime;
	// 				//        }
	// 				//        if (!_ignoreYellowZone)
	// 				//        {
	// 				//            if (_YellowTriggered && Timer.GetTime() >= Delay)
	// 				//            {                                    
	// 				//                //Debug.Log("Yellow: " + _VehRigidbody.velocity.magnitude + "," + YellowZoneSpeedLimit);
	// 				//                if (CarReference.GetComponent<Rigidbody>().velocity.magnitude > YellowZoneSpeedLimit)
	// 				//                {
	// 				//                    //Debug.Log("In here!");
	// 				//                    CarRefController.Move(0, 0, -0.25f, 0f); //was -0.25f 

	// 				//                }
	// 				//                else
	// 				//                {
	// 				//                    CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().SetMaxSpeed(YellowZoneSpeedLimitMPH);
	// 				//                    CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 1, 0, 0f);
	// 				//                }

	// 				//            }
	// 				//            else
	// 				//            {
	// 				//                CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 1, 0f, 0f);
	// 				//            }
	// 				//        }
	// 				//        else
	// 				//        {
	// 				//            CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 1, 0f, 0f);
	// 				//        }

	// 				//        break;
	// 				//    case ProxZone.RedZone:

	// 				//        if (_StopTriggered)
	// 				//        {
	// 				//            //StopDistLabel.text = StopDistance.ToString("F2") + " feet, " + ForwardDistanceToPerson.ToString("F2") + " feet from the tracker";
	// 				//            StopDistLabel.text = string.Format("Stopping Distance (ft): {0,6:F2}       Distance To MWC (ft): {1,6:F2}", StopDistance, DistanceToPerson);
	// 				//            _textDisplayBuilder.AppendFormat("{0,-26}: {1:F2}\n", "Stopping Distance (ft)", StopDistance);
	// 				//            _textDisplayBuilder.AppendFormat("{0,-26}: {1:F2}\n", "Distance To MWC (ft)", ForwardDistanceToPerson);
	// 				//        }
	// 				//        else
	// 				//        {
	// 				//            //Debug.Log("Red zone triggered");
	// 				//            Prox.EnableZoneVisualization(new ProxSystem.VisOptions(true, true));
	// 				//            _StopPosition = CarReference.transform.position;
	// 				//            WMG_Series RedSeries = Graph.addSeries();
	// 				//            RedSeries.pointColor = Color.red;
	// 				//            RedSeries.lineColor = Color.red;
	// 				//            RedSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 0));
	// 				//            RedSeries.pointValues.Add(new Vector2((float)Timer.GetTime(), 7));
	// 				//            RedSeries.seriesName = "Red";
	// 				//            Delay = Timer.GetTime() + DelayTime;
	// 				//            _StopTriggered = true;
	// 				//        }
	// 				//        if (_StopTriggered && Timer.GetTime() >= Delay)
	// 				//        {
	// 				//            Rigidbody rb = CarReference.GetComponent<Rigidbody>();
	// 				//            //CarRefController.SetBrakeTorque(_TorqueInput);
	// 				//            if (rb.velocity.magnitude > 0)
	// 				//            {
	// 				//                if (_PriorVelocities[0] != -1)
	// 				//                {
	// 				//                    if (!_StopFixTrigger && (Mathf.Abs(rb.velocity.magnitude - _PriorVelocities[1]) > 0.01f) && rb.velocity.magnitude > 0.05f)//This aims address a problem with the vehicle physics model at low speeds when stopping (vehicle skids forever when it should not) 0.05 mps is based on observation of when the unrealistic behavior takes place
	// 				//                    {
	// 				//                        _PriorVelocities[0] = _PriorVelocities[1];
	// 				//                        _PriorVelocities[1] = rb.velocity.magnitude;
	// 				//                        CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 0, -1f, 0);
	// 				//                    }
	// 				//                    else
	// 				//                    {
	// 				//                        if(Mathf.Approximately(_VehRigidbody.velocity.magnitude, _PriorVelocities[1]))
	// 				//                        {
	// 				//                            Debug.Log("Approx Equal");
	// 				//                        }
	// 				//                        if ((Mathf.Abs(rb.velocity.magnitude - _PriorVelocities[1]) < 0.0005f))
	// 				//                        {
	// 				//                            Debug.Log("Triggered " + _CurrentIncline + ", " + rb.velocity.magnitude);
	// 				//                            float targetVel = _PriorVelocities[1] - (Mathf.Abs(_PriorVelocities[0] - _PriorVelocities[1]));
	// 				//                            if (targetVel < 0)
	// 				//                            {
	// 				//                                targetVel = 0;
	// 				//                            }
	// 				//                            _PriorVelocities[0] = _PriorVelocities[1];
	// 				//                            _PriorVelocities[1] = targetVel;
	// 				//                            rb.velocity = targetVel * rb.velocity.normalized;
	// 				//                            _StopFixTrigger = true;
	// 				//                        }
	// 				//                        else
	// 				//                        {
	// 				//                            CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 0, -1f, 0);
	// 				//                        }
	// 				//                        //CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 0, -1f, 0);
	// 				//                    }
	// 				//                }
	// 				//                else
	// 				//                {
	// 				//                    _PriorVelocities[0] = _PriorVelocities[1];
	// 				//                    _PriorVelocities[1] = rb.velocity.magnitude;
	// 				//                    CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 0, -1, 0);
	// 				//                }
	// 				//            }
	// 				//            else
	// 				//            {
	// 				//                //_StopFixTrigger = false;
	// 				//            }
	// 				//        }
	// 				//        else
	// 				//        {
	// 				//            //float lerpedYellowSpeed = Mathf.InverseLerp(0.61f, 1.828f, CarReference.GetComponent<Rigidbody>().velocity.magnitude);
	// 				//            //float lerpedYellowTorque = Mathf.Lerp(_TorqueInput, 2000, (1.828f - CarReference.GetComponent<Rigidbody>().velocity.magnitude) / (1.828f - 0.61f));
	// 				//            //CarRefController.SetBrakeTorque(lerpedYellowTorque);
	// 				//            //lerpedYellowSpeed = lerpedYellowSpeed * -1;
	// 				//            if (!_ignoreYellowZone)
	// 				//            {
	// 				//                if (CarReference.GetComponent<Rigidbody>().velocity.magnitude > YellowZoneSpeedLimit)
	// 				//                {
	// 				//                    CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 0, -0.25f, 0f);
	// 				//                }
	// 				//                else
	// 				//                {
	// 				//                    CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 1, 0, 0f);
	// 				//                    CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().SetMaxSpeed(YellowZoneSpeedLimitMPH);
	// 				//                }
	// 				//            }
	// 				//            else
	// 				//            {
	// 				//                CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 1, 0f, 0f);
	// 				//            }
	// 				//        }
	// 				//        break;
	// 				//    default:
	// 				//        break;
	// 				//}
	// 				#endregion
	// 				// Old code for stopping the vehicle when not utilizing proximity. Newer code used prox zones to trigger stops instead. This is kept for preservation/history.
	// 				// if(Timer.GetTime() >= 0 && Timer.GetTime() < 8)
	// 				// {
	// 				//     CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 1, 0f, 0f);
	// 				// }
	// 				// else if (Timer.GetTime() >= 8 && Timer.GetTime() < 10)
	// 				// {
	// 				//     if(CarReference.GetComponent<Rigidbody>().velocity.magnitude >= 1.5f)
	// 				//     {
	// 				//         CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 0, -0.5f, 0f);
	// 				//     }
	// 				//     else
	// 				//     {
	// 				//         CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 1, 0, 0f);
	// 				//         CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().SetMaxSpeed(3);
	// 				//     }
	// 				// }            
	// 				//if (!_BrakesEngaged)
	// 				//{
	// 				//     //if (CarReference.transform.position.z <= 72) //Brakes always engage after the vehicle enters this Z coordinate (Unity space)
	// 				//     //{
	// 				//     //    CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().ContinuousMove(0, 0, 0, 1, true);
	// 				//     //    _BrakesEngaged = true;
	// 				//     //}
	// 				//     if (Timer.GetTime() >= 10) //Brakes always engage after 10 seconds
	// 				//     {
	// 				//         CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>().Move(0, 0, -1f, 0);
	// 				//         //_BrakesEngaged = true;
	// 				//     }
	// 				//}
	// 			}
	// 			else
	// 			{
	// 				CarRefController.SetMaxSpeed(0);
	// 				CarRefController.Move(0, 0, -1, 0);
	// 			}
	// 			if (!HideGraph && ShowPIDOutput)
	// 			{
	// 				OutputSeriesData.pointValues.Add(new Vector2((float)Timer.GetTime(), CarRefController.GetOutput()));
	// 			}
	// 		}
	// 		else
	// 		{
	// 			if (Timer.GetTime() >= 5)
	// 			{
	// 				Restart();
	// 			}
	// 		}

	// 		if (TextDisplayPanel != null)
	// 		{
	// 			TextDisplayPanel.text = _textDisplayBuilder.ToString();
	// 		}

			
	// 	}
	// }

	/// <summary>
	/// Resets everything for the experiment between trials.
	/// </summary>
	public void Restart()
	{		
		Timer.StopTimer();
		Timer.ResetTimer();
		//_ignoreYellowZone = true;//Replace with variable to load later
		ChaseCamera.transform.parent = null;        
		Destroy(CarReference);
		_Spawned = false;
		//Set the loading into a new function to be invoked at a delay (prevent unintended collision)
		Invoke("LoadNewCar", 1);
  //      SetMWCLocation(_currentVehicleVar.MwcLoc);
  //      //SetYellowZonePrefab();
  //      ShuttleCar = _currentVehicleVar.VehicleToLoad;
  //      CarReference = Instantiate(ShuttleCar, transform);
  //      BoxCollider[] boxes = CarReference.GetComponentsInChildren<BoxCollider>();
  //      foreach(BoxCollider col in boxes)
  //      {
  //         if(col.GetComponent<FieldBasedProxSystem>() == null && col.GetComponent<ProxSystemController>() == null && col.GetComponent<BoxProxSystem>() == null)
  //          {
  //              _bodyCollider = col;
  //          }
  //      }
		//CarReference.transform.localPosition = Vector3.zero;
		//CarReference.transform.localRotation = Quaternion.identity;
		//CarReference.GetComponent<Rigidbody>().velocity = Vector3.zero;
  //      _FrontOfShuttleCar = CarReference.transform.Find("FrontOfCarTransform");
  //      Vector3 arbitraryPointForward = CarReference.transform.position;
  //      arbitraryPointForward.z = arbitraryPointForward.z + 12;//Setting well forward of the machine based on center point
  //      _FrontOfShuttleCar.transform.position = _bodyCollider.ClosestPoint(arbitraryPointForward);
		//#region CodeOnlyForDemos
		//foreach(Cloth curtain in Curtains)
		//{
		//	CapsuleCollider[] capsuleHelpers = CarReference.GetComponentsInChildren<CapsuleCollider>();
		//	curtain.capsuleColliders = capsuleHelpers;                        
		//}
		//ChaseCamera.transform.parent = CarReference.transform;
		//ChaseCamera.transform.localPosition = new Vector3(0.9147f, 1.9f, -2.31f);//change this to reset the camera!
  //      ChaseCamera.transform.rotation = Quaternion.Euler(7.607f, -12.414f, 0);
  //      #endregion
  //      Physics.IgnoreLayerCollision(19, 19);
		//CarRefController = CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();
  //      SetVariablesToNominal();
  //      ConfigureTest(CurrentTest);
  //      //ConfigureTest(TestType.Baseline);
		////CarReference.GetComponent<MobileEquipmentLogHandle>().IDNumber = _Trial;
		//wheels = CarReference.GetComponentsInChildren<WheelCollider>();		
  //      //BoxCollider frontPlane = _FrontOfShuttleCar.gameObject.AddComponent<BoxCollider>();
  //      //Vector3 size = frontPlane.size;
  //      //frontPlane.isTrigger = true;
  //      //size.x = 6;
  //      //size.y = 1;
  //      //size.z = 0.001f;
  //      //frontPlane.size = size;
		


		//Prox = CarReference.GetComponent<ProxSystemController>();		
		//Graph.deleteSeries();
		//Graph.deleteSeries();
		//Graph.deleteSeries();
		//Series1Data = Graph.addSeries();
		//Series1Data.pointColor = Color.green;
		//Series1Data.lineColor = Color.green;
		//Series1Data.seriesName = _Trial.ToString();
		//StopDistLabel.text = "";        
		//Timer.StartTimer();
		//_Spawned = true;
		//_BrakesEngaged = false;
		//_StopTriggered = false;
		//_YellowTriggered = false;
		//_StopFixTrigger = false;
		//for(int i=0; i < 2; i++)
		//{
		//	_PriorVelocities[i] = -1;
		//}
		//_LastVelocity = 0;
	}

	// private void LoadNewCar()
	// {
	// 	_brakingStarted = false;
	// 	SetMWCLocation(_currentVehicleVar.MwcLoc);
	// 	//SetYellowZonePrefab();
	// 	ShuttleCar = _currentVehicleVar.VehicleToLoad;
	// 	_VehState = VehicleState.SpeedUp;
	// 	CarReference = Instantiate(ShuttleCar, transform);

	// 	BoxCollider[] boxes = CarReference.GetComponentsInChildren<BoxCollider>();
	// 	foreach (BoxCollider col in boxes)
	// 	{
	// 		if (col.GetComponent<FieldBasedProxSystem>() == null && col.GetComponent<ProxSystemController>() == null && col.GetComponent<BoxProxSystem>() == null)
	// 		{
	// 			_bodyCollider = col;
	// 		}
	// 	}
	// 	CarReference.transform.localPosition = Vector3.zero;
	// 	CarReference.transform.localRotation = Quaternion.identity;
	// 	_VehRigidbody = CarReference.GetComponent<Rigidbody>();
	// 	CarReference.GetComponent<Rigidbody>().velocity = Vector3.zero;
	// 	_FrontOfShuttleCar = CarReference.transform.Find("FrontOfCarTransform");
	// 	Vector3 arbitraryPointForward = CarReference.transform.position;
	// 	arbitraryPointForward.z = arbitraryPointForward.z + 12;//Setting well forward of the machine based on center point
	// 	_FrontOfShuttleCar.transform.position = _bodyCollider.ClosestPoint(arbitraryPointForward);
	// 	#region CodeOnlyForDemos
	// 	foreach (Cloth curtain in Curtains)
	// 	{
	// 		CapsuleCollider[] capsuleHelpers = CarReference.GetComponentsInChildren<CapsuleCollider>();
	// 		curtain.capsuleColliders = capsuleHelpers;
	// 	}
	// 	ChaseCamera.transform.parent = CarReference.transform;

	// 	var driverPosTransform = CarReference.transform.Find("DriverCamPos");
	// 	if (driverPosTransform != null)
	// 	{
	// 		ChaseCamera.transform.position = driverPosTransform.position;
	// 		ChaseCamera.transform.rotation = driverPosTransform.rotation;
	// 	}
	// 	//ChaseCamera.transform.localPosition = new Vector3(0.88f, 1.57f, -1.64f);//change this to reset the camera!
	// 	//ChaseCamera.transform.rotation = Quaternion.Euler(7.607f, -12.414f, 0);
	// 	#endregion
	// 	Physics.IgnoreLayerCollision(19, 19);
	// 	CarRefController = CarReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();
	// 	CarRefController.RedZoneOverride = false;
	// 	CarRefController.SetMaxSpeed(0);
	// 	CarRefController.ResetDeltas();//The integral term and dt can get messed up here for some reason on a restart unless they get cleared
	// 	SetVariablesToNominal();
	// 	ConfigureTest(CurrentTest);
	// 	//ConfigureTest(TestType.Baseline);
	// 	//CarReference.GetComponent<MobileEquipmentLogHandle>().IDNumber = _Trial;
	// 	_Wheels = CarReference.GetComponentsInChildren<WheelCollider>();
	// 	foreach(WheelCollider wheel in _Wheels)
	// 	{
	// 		Debug.Log(wheel.name);
	// 		switch (wheel.name)
	// 		{
	// 			case "FL_WheelCol":
	// 				_FLWheel = wheel;
	// 				break;
	// 			case "FR_WheelCol":
	// 				_FRWheel = wheel;
	// 				break;
	// 			case "RL_WheelCol":
	// 				_RLWheel = wheel;
	// 				break;
	// 			case "RR_WheelCol":
	// 				_RRWheel = wheel;
	// 				break;
	// 		}
	// 	}
	// 	//BoxCollider frontPlane = _FrontOfShuttleCar.gameObject.AddComponent<BoxCollider>();
	// 	//Vector3 size = frontPlane.size;
	// 	//frontPlane.isTrigger = true;
	// 	//size.x = 6;
	// 	//size.y = 1;
	// 	//size.z = 0.001f;
	// 	//frontPlane.size = size;



	// 	Prox = CarReference.GetComponent<ProxSystemController>();
	// 	if (!HideGraph)
	// 	{
	// 		Graph.deleteSeries();
	// 		Graph.deleteSeries();
	// 		Graph.deleteSeries();
	// 		if (ShowPIDOutput)
	// 		{
	// 			Graph.deleteSeries();//REMOVE THIS LATER WHEN REMOVING PID OUTPUT SERIES 
	// 		}      
	// 		Series1Data = Graph.addSeries();
	// 		Series1Data.pointColor = Color.green;
	// 		Series1Data.lineColor = Color.green;
	// 		Series1Data.seriesName = _Trial.ToString();
	// 		if (ShowPIDOutput)
	// 		{
	// 			OutputSeriesData = Graph.addSeries();
	// 			OutputSeriesData.pointColor = Color.blue;
	// 			OutputSeriesData.lineColor = Color.blue;
	// 			OutputSeriesData.seriesName = "PID Output";
	// 		}
	// 	}
	// 	StopDistLabel.text = "";
	// 	Timer.StartTimer();
	// 	_Spawned = true;
	// 	_BrakesEngaged = false;
	// 	_StopTriggered = false;
	// 	_YellowTriggered = false;
	// 	_StopFixTrigger = false;
	// 	_VehState = VehicleState.SpeedUp;
	// 	_MaxDeltaTime = 0;
	// 	_GraphBufferCount = 10;
	// 	for (int i = 0; i < 2; i++)
	// 	{
	// 		_PriorVelocities[i] = -1;
	// 	}
	// 	//_LastVelocity = 0;
	// }
	
	// //Old code, see ConfigureTest for the updated experiment
	// void IncrementWheelParams(float ratio)
	// {
	// 	_ExtremumValue += 0.1f;
	// 	_AsymptoteValue = _ExtremumValue * ratio;
	// 	WheelCollider[] wheels = CarReference.GetComponentsInChildren<WheelCollider>();        
	// 	foreach(WheelCollider wheel in wheels)
	// 	{
	// 		WheelFrictionCurve friction = wheel.forwardFriction;
	// 		friction.extremumValue = _ExtremumValue;
	// 		friction.asymptoteValue = _AsymptoteValue;
	// 		wheel.forwardFriction = friction;
	// 	}        
	// }

	// void SetVariablesToNominal()
	// {
	// 	_MinDistanceToVehicle = 10000; //Arbitrary high amount
	// 	_MinFrontPlane = 10000;
	// 	_InitialSpeed = _currentVehicleVar.NominalSpeed;
	// 	//_BrakeForceVar = _currentVehicleVar.NominalBrakeTorque;
	// 	_BrakeTorque = _currentVehicleVar.NominalBrakeTorque;
	// 	_AsForce = _asymptoteValVar.NominalVal;
	// 	_AsSlip = _asymptoteSlpVar.NominalVal;
	// 	_ExForce = _extremeValVar.NominalVal;
	// 	_ExSlip = _extremeSlpVar.NominalVal;
	// 	_Incline = _gradeVar.NominalVal;
	// 	_BadTimeStepCount = 0;
	// 	_PriorFixedUpdateTime = 0;
	// 	_Error = "";
	// 	_RedZoneBrakingStartTime = 0;
	// 	_RedZoneBrakingDuration = 0;
	// 	_YellowZoneDistance = 0;
	// 	_RedZoneDistance = 0;
	// 	_NumSamples = 0;
	// 	//CarRefController.SetMaxSpeed(_initialSpeedVar.NominalVal);
	// 	Quaternion rot = Quaternion.Euler(-(_gradeVar.NominalVal), 0, 0);
	// 	Mine.transform.rotation = rot;
	// 	CarRefController.GetComponent<Rigidbody>().mass = _currentVehicleVar.EmptyMass;
	// 	float maxSpeed = _currentVehicleVar.NominalSpeed * 0.681818f; //feet per second to MPH friendly to Car Controller
	// 	CarRefController.SetMaxSpeed(maxSpeed);
		
	// 	DelayTime = (double)_lagTimeVar.NominalVal;
	// 	float wheelRadius = 0;
	// 	WheelCollider[] wheels = CarReference.GetComponentsInChildren<WheelCollider>();
	// 	foreach (WheelCollider wheel in wheels)//Update all wheel colliders with new values
	// 	{
	// 		WheelFrictionCurve friction = wheel.forwardFriction;            
	// 		friction.asymptoteSlip = _asymptoteSlpVar.NominalVal;
	// 		friction.asymptoteValue = _asymptoteValVar.NominalVal;
	// 		friction.extremumSlip = _extremeSlpVar.NominalVal;
	// 		friction.extremumValue = _extremeValVar.NominalVal;
	// 		wheel.forwardFriction = friction;
	// 		wheelRadius = wheel.radius;
	// 	}
	// 	float torque = (_currentVehicleVar.NominalBrakeTorque * 4.44822f); //converting lbf to N
	// 	torque = torque * wheelRadius;//Multiply by moment arm (wheel's radius) to solve for the torque on the wheel
	// 	//Debug.Log(torque + "," + torque * 4);
	// 	_wheelRadius = wheelRadius;
	// 	CarRefController.SetBrakeTorque(torque * 4);//Variable is per-wheel, controller wants all 4 combined (it can do 2-wheel drive or 4-wheel drive) 
	// 	_BrakeTorque = torque * 0.73756f;
	// 	//_BrakeForceVar = torque * 0.7375f;//Nm to ft-lb     
	// 	_ignoreYellowZone = _currentVehicleVar.IgnoreYellowZone;//2 stage braking
	// 	_YellowTriggerPosition = Vector3.zero;
	// 	_RedTriggerPosition = Vector3.zero;      
		
	// }

	// /// <summary>
	// /// Method that gets called every time the shuttle car gets instantiated. Populates the friction values or other variables of interest to check in a test.
	// /// </summary>
	// /// <param name="type"></param>
	// void ConfigureTest(TestType type = TestType.FourVariable)
	// {
	// 	if (continuousFile != null && !IsDemo)
	// 	{
	// 		continuousFile.Close();
	// 	}
	// 	string _path = Directory.GetParent(Application.dataPath).FullName;
	// 	_path = Path.Combine(_path, "Logs");
	// 	string today = String.Format("{0}{1}{2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
	// 	_path = Path.Combine(_path, today);
	// 	string prefabName = _currentVehicleVar.VehicleToLoad.name;
	// 	_path = Path.Combine(_path, prefabName);
	// 	_path = Path.Combine(_path, type.ToString());
	// 	if (!Directory.Exists(_path)){
	// 		Directory.CreateDirectory(_path);
	// 	}
	// 	string contFileName = String.Format("Log_{0}_{1}_T{2}.csv", _currentVehicleVar.VehicleToLoad.name,today,_TotalTrialCount.ToString("00000"));
	// 	if (!IsDemo)
	// 	{
	// 		continuousFile = File.CreateText(Path.Combine(_path, contFileName));
	// 	}
	// 	//populate the header with sim variables....might need to put these calls at the *end* of this function
	// 	//0 to 34 variables to fill!
	// 	switch (type)
	// 	{
	// 		//A series of 40 tests performed where I varied one of the 4 friction curve values available on the wheel colliders
	// 		case TestType.FourVariable: 
				
	// 			switch (_Trial)
	// 			{
	// 				case 1:
	// 					_AsymptoteValue = 0.8f;
	// 					_ExtremumValue = 0.4f;
	// 					break;
	// 				case 11:
	// 					_AsymptoteValue = 0;
	// 					_ExtremumValue = 2;
	// 					break;
	// 				case 21:
	// 					_AsymptoteValue = 2;
	// 					_ExtremumValue = 4;
	// 					_ExtremumSlip = 0;
	// 					_AsymptoteSlip = 1f;
	// 					break;
	// 				case 31:
	// 					_AsymptoteValue = 2;
	// 					_ExtremumValue = 4;
	// 					_ExtremumSlip = 0.2f;
	// 					_AsymptoteSlip = 0.2f;
	// 					break;
	// 				default:
	// 					break;
	// 			}

	// 			if (_Trial < 11)
	// 			{
	// 				_ExtremumValue = _ExtremumValue + 0.2f;
	// 			}
	// 			else if (_Trial < 21)
	// 			{

	// 				_AsymptoteValue = _AsymptoteValue + 0.1f;
	// 			}
	// 			else if (_Trial < 31)
	// 			{
	// 				_ExtremumSlip = _ExtremumSlip + 0.1f;
	// 			}
	// 			else if (_Trial < 41)
	// 			{
	// 				_AsymptoteSlip = _AsymptoteSlip + 0.1f;
	// 			}
	// 			else
	// 			{
	// 				_TrialMessage.Message = "Complete";
	// 				LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 				Debug.Log("Test complete");
	// 				gameObject.SetActive(false);
	// 				Application.Quit();
	// 			}

	// 			_TrialMessage.Message = "ExSl," + _ExtremumSlip + ",ExVa," + _ExtremumValue + ",AsSl," + _AsymptoteSlip + ",AsVa," + _AsymptoteValue;
	// 			LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 			endOfTrialFile.WriteLine(_TrialMessage.Message);
	// 			WheelCollider[] wheels = CarReference.GetComponentsInChildren<WheelCollider>();
	// 			foreach (WheelCollider wheel in wheels)//Update all wheel colliders with new values
	// 			{
	// 				WheelFrictionCurve friction = wheel.forwardFriction;
	// 				friction.extremumValue = _ExtremumValue;
	// 				friction.asymptoteValue = _AsymptoteValue;
	// 				friction.extremumSlip = _ExtremumSlip;
	// 				friction.asymptoteSlip = _AsymptoteSlip;
	// 				wheel.forwardFriction = friction;
	// 			}
	// 			break;
	// 		case TestType.Demo:
				
	// 			//_AsymptoteValue = 2;
	// 			//_ExtremumValue = 4;
	// 			//_ExtremumSlip = 0.2f;
	// 			//_AsymptoteSlip = 0.5f;
	// 			//AsymptoteSlipDisplayField.text = _AsymptoteSlip.ToString();
	// 			//AsymptoteValueDisplayField.text = _AsymptoteValue.ToString();
	// 			//ExtremumSlipDisplayField.text = _ExtremumSlip.ToString();
	// 			//ExtremumValueDisplayField.text = _ExtremumValue.ToString();
	// 			float updateBrakeTorque;
	// 			if(float.TryParse(ExtremumValueDisplayField.text, out _ExtremumValue))
	// 			{
	// 				_TrialMessage.Message = "Extremum value changed: " + _ExtremumValue;
	// 				LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 				endOfTrialFile.WriteLine(_TrialMessage.Message);
	// 			}
	// 			if (float.TryParse(AsymptoteValueDisplayField.text, out _AsymptoteValue))
	// 			{
	// 				_TrialMessage.Message = "Asymptote value changed: " + _AsymptoteValue;
	// 				LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 				endOfTrialFile.WriteLine(_TrialMessage.Message);
	// 			}
	// 			if (float.TryParse(ExtremumSlipDisplayField.text, out _ExtremumSlip))
	// 			{
	// 				_TrialMessage.Message = "Extremum slip changed: " + _ExtremumSlip;
	// 				LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 				endOfTrialFile.WriteLine(_TrialMessage.Message);
	// 			}
	// 			if (float.TryParse(AsymptoteSlipDisplayField.text, out _AsymptoteSlip))
	// 			{
	// 				_TrialMessage.Message = "Asymptote slip changed: " + _AsymptoteSlip;
	// 				LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 				endOfTrialFile.WriteLine(_TrialMessage.Message);
	// 			}
	// 			if(float.TryParse(BrakeTorqueField.text, out updateBrakeTorque))
	// 			{
	// 				CarRefController.SetBrakeTorque(updateBrakeTorque);
	// 				_TrialMessage.Message = "Brake torque: " + updateBrakeTorque;
	// 				LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 				endOfTrialFile.WriteLine(_TrialMessage.Message);
	// 				Debug.Log("Brake torque field changed: " + updateBrakeTorque);
	// 				_TorqueInput = updateBrakeTorque;
	// 			}

	// 			if (float.TryParse(StiffnessField.text, out _Stiffness))
	// 			{                    
	// 				_TrialMessage.Message = "Stiffness: " + _Stiffness;
	// 				LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 				endOfTrialFile.WriteLine(_TrialMessage.Message);
	// 			}

	// 			float incline;

	// 			if(float.TryParse(InclineDisplayField.text, out incline))
	// 			{
	// 				incline = Mathf.Round(incline);
	// 				if (incline > 10)
	// 				{
	// 					incline = 10;
	// 				}
	// 				//if(incline < 0)
	// 				//{
	// 				//    incline = 0;
	// 				//}
	// 				_TrialMessage.Message = "Incline input: " + incline;
	// 				LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 				endOfTrialFile.WriteLine(_TrialMessage.Message);
	// 			}
	// 			double delayIn;
	// 			if(double.TryParse(MillisecondDelay.text, out delayIn))
	// 			{
	// 				DelayTime = delayIn / 1000;
	// 				//WJH: Temporarily removed GUI control for this
	// 				//DelayTime = 0.5f;
	// 			}				
				
	// 			if(_Trial > 1)
	// 			{
					
	// 				if (incline == _PriorIncline)
	// 				{
	// 					incline = incline + 5;
	// 					if(incline > 10)
	// 					{
	// 						incline = 0;                            
	// 					}
						
	// 				}
	// 				if(incline == 10)
	// 				{
	// 					_UserInputAwait = true;
	// 				}
	// 				Quaternion newRotation = Quaternion.Euler(incline, 0, 0);
	// 				_CurrentIncline = incline;
	// 				Mine.transform.rotation = newRotation;
	// 				_PriorIncline = incline;
	// 				//_PriorIncline = incline;
	// 			}
	// 			InclineDisplayField.text = (Mathf.Round(Mine.transform.rotation.eulerAngles.x)).ToString();
	// 			Vector3 sideCamPos = SideCamera_T.position;
	// 			sideCamPos.y = -2.987078f - SideCamera_T.position.z * Mathf.Tan(incline * (Mathf.PI / 180));
	// 			SideCamera_T.position = sideCamPos;

	// 			if(_Trial > 24)
	// 			{
	// 				_TrialMessage.Message = "Complete";
	// 				LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 				endOfTrialFile.WriteLine(_TrialMessage.Message);
	// 				Debug.Log("Test complete");
	// 				gameObject.SetActive(false);
	// 				Application.Quit();
	// 			}
				
	// 			WheelCollider[] wheels2 = CarReference.GetComponentsInChildren<WheelCollider>();
	// 			//update all wheel colliders with new friction values
	// 			foreach (WheelCollider wheel in wheels2)
	// 			{
	// 				WheelFrictionCurve friction = wheel.forwardFriction;
	// 				friction.extremumValue = _ExtremumValue;
	// 				friction.asymptoteValue = _AsymptoteValue;
	// 				friction.extremumSlip = _ExtremumSlip;
	// 				friction.asymptoteSlip = _AsymptoteSlip;
	// 				friction.stiffness = _Stiffness;
	// 				wheel.forwardFriction = friction;
	// 			}
	// 			break;
	// 		case TestType.AsymptoteTest:
				
	// 			_AsymptoteValue = 0.8f;
	// 			_ExtremumValue = 4;
	// 			_ExtremumSlip = 0.2f;
	// 			_AsymptoteSlip = 0.5f;                
	// 			//if (_Trial > 1)
	// 			//    Mine.transform.Rotate(-1, 0, 0);
	// 			InclineDisplayField.text = (Mine.transform.rotation.eulerAngles.x - 360).ToString();
	// 			if(_Trial > 10)
	// 			{
	// 				_AsymptoteValue += 0.2f;
	// 			}
	// 			if (_Trial > 20)
	// 			{
	// 				_TrialMessage.Message = "Complete";
	// 				LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 				endOfTrialFile.WriteLine(_TrialMessage.Message);
	// 				Debug.Log("Test complete");
	// 				gameObject.SetActive(false);
	// 				Application.Quit();
	// 			}

	// 			_TrialMessage.Message = "ExSl," + _ExtremumSlip + ",ExVa," + _ExtremumValue + ",AsSl," + _AsymptoteSlip + ",AsVa," + _AsymptoteValue;
	// 			LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 			endOfTrialFile.WriteLine(_TrialMessage.Message);
	// 			WheelCollider[] wheels3 = CarReference.GetComponentsInChildren<WheelCollider>();
	// 			//Update all wheel colliders with new friction values
	// 			foreach (WheelCollider wheel in wheels3)
	// 			{
	// 				WheelFrictionCurve friction = wheel.forwardFriction;
	// 				friction.extremumValue = _ExtremumValue;
	// 				friction.asymptoteValue = _AsymptoteValue;
	// 				friction.extremumSlip = _ExtremumSlip;
	// 				friction.asymptoteSlip = _AsymptoteSlip;
	// 				friction.stiffness = 1f;
	// 				wheel.forwardFriction = friction;
	// 			}
	// 			AsymptoteSlipDisplayField.text = _AsymptoteSlip.ToString();
	// 			AsymptoteValueDisplayField.text = _AsymptoteValue.ToString();
	// 			ExtremumSlipDisplayField.text = _ExtremumSlip.ToString();
	// 			ExtremumValueDisplayField.text = _ExtremumValue.ToString();
	// 			break;
	// 		case TestType.BreakTorqueSens:
	// 			//if (_Trial != 0)
	// 			//{
	// 			//	CarRefController.SetBrakeTorque(_Trial * 100);
	// 			//	//NOTE: Cannot set a BrakeTorque <= 0;
	// 			//	_BrakeTorque = (_Trial * 100);
	// 			//}
	// 			//else
	// 			//{
	// 			//	CarRefController.SetBrakeTorque(1);
	// 			//	_BrakeTorque = 1;
	// 			//}
	// 			float currentTorqueVal = _brakingForceVar.MinVal + _Trial * ((_brakingForceVar.MaxVal - _brakingForceVar.MinVal) /( TrialCount - 1));//Test point count set to be inclusive
	// 			currentTorqueVal = Mathf.Clamp(currentTorqueVal, _brakingForceVar.MinVal, _brakingForceVar.MaxVal);

	// 			float torque = (currentTorqueVal * 4.44822f); //converting lbf to N
	// 			currentTorqueVal = currentTorqueVal * _wheelRadius;//Multiply by moment arm (wheel's radius) to solve for the torque on the wheel
	// 			_BrakeTorque = currentTorqueVal * 0.73756f; //convert N-m to ft-lb
	// 			CarRefController.SetBrakeTorque(currentTorqueVal*4);
	// 			_CurrentVariableValue = "BrakeTorque";
	// 			DelayTime = (double)_lagTimeVar.NominalVal;
	// 			if (_Trial == 0)
	// 			{
	// 				StringMessageData brakeSt = new StringMessageData();
	// 				brakeSt.Message = "Timestamp, Brake Torque, Stop Distance, Distance To Person, Closest Point, Final Velocity, Trial";
	// 				//continuousFile.WriteLine("Timestamp, Brake Torque, Forward Distance to Person(front plane)(ft), Closest Point, Velocity, Xpos, YPos, Zpos, Trial#");
	// 				LogManagerRef.AddPacketToQueue(brakeSt);
	// 				//endOfTrialFile.WriteLine(brakeSt.Message);
	// 			}
	// 			//double delayIn2;
	// 			//if (double.TryParse(MillisecondDelay.text, out delayIn2))
	// 			//{
	// 			//	DelayTime = delayIn2 / 1000;
	// 			//	//WJH: Temporarily removed GUI control for this
	// 			//	//DelayTime = 0.5f;
	// 			//}
	// 			if(_Trial >= TrialCount)
	// 			{
	// 				SensTestIndex++;
	// 				CurrentTest = TestList[SensTestIndex];
	// 				//_currentTestPoint = 0;
	// 				_Trial = 0;
	// 				StringMessageData brakeDone = new StringMessageData();
	// 				brakeDone.Message = "Brake Sensitivity Test Complete";
	// 				LogManagerRef.AddPacketToQueue(brakeDone);
	// 				//endOfTrialFile.WriteLine(brakeDone.Message);
	// 				_Spawned = false;
	// 				continuousFile.Close();
	// 				File.Delete(Path.Combine(_path, contFileName));                    
	// 				_fileClosedFlag = true;
	// 				Restart();
	// 				return;
	// 			}                		
	// 			WheelCollider[] wheels4 = CarReference.GetComponentsInChildren<WheelCollider>();
	// 			//update all wheel colliders with new friction values
	// 			foreach (WheelCollider wheel in wheels4)
	// 			{
	// 				WheelFrictionCurve friction = wheel.forwardFriction;
	// 				friction.extremumValue = _ExtremumValue;
	// 				friction.asymptoteValue = _AsymptoteValue;
	// 				friction.extremumSlip = _ExtremumSlip;
	// 				friction.asymptoteSlip = _AsymptoteSlip;
	// 				friction.stiffness = 0.25f;
	// 				wheel.forwardFriction = friction;
	// 			}
	// 			//_Trial++;
	// 			break;
	// 		case TestType.FrictionSens:
				
	// 			float stiffnessVal = _rollingResistanceVar.MinVal + _Trial*((_rollingResistanceVar.MaxVal-_rollingResistanceVar.MinVal)/(TrialCount - 1));//Test point count set to be inclusive
	// 			//double delayIn3;
	// 			_CurrentVariableValue = stiffnessVal.ToString();
	// 			if (_Trial == 0)
	// 			{
	// 				StringMessageData frSt = new StringMessageData();
	// 				frSt.Message = "Timestamp, Friction Val, Stop Distance, Distance To Person (ft), Closest Point, Final Velocity (f/s), Trial";
	// 				//continuousFile.WriteLine("Timestamp, Friction Stiffness, Forward Distance to Person(front plane)(ft), Closest Point (ft), Velocity (f/s), Xpos (m), YPos (m), Zpos (m), Trial#");
	// 				LogManagerRef.AddPacketToQueue(frSt);
	// 				//endOfTrialFile.WriteLine(frSt.Message);                    
	// 			}
	// 			if (_Trial >= TrialCount)
	// 			{
	// 				SensTestIndex++;
	// 				CurrentTest = TestList[SensTestIndex];
	// 				_currentTestPoint = 0;
	// 				_Trial = 0;
	// 				StringMessageData frDone = new StringMessageData();
	// 				frDone.Message = "Friction Sensitivity Test Complete";
	// 				LogManagerRef.AddPacketToQueue(frDone);
	// 				//endOfTrialFile.WriteLine(frDone.Message);
	// 				//continuousFile.Close();
	// 				Restart();
	// 				return;
	// 			}
	// 			//if (_Trial >= _trialPerIncrement)
	// 			//{
	// 			//    _Trial = 0;
	// 			//    _currentTestPoint++;
	// 			//    Restart();
	// 			//    return;
	// 			//}
	// 			DelayTime = (double)_lagTimeVar.NominalVal;
				

	// 			WheelCollider[] wheels5 = CarReference.GetComponentsInChildren<WheelCollider>();
	// 			foreach (WheelCollider wheel in wheels5)//update all wheel colliders with new friction values
	// 			{
	// 				WheelFrictionCurve friction = wheel.forwardFriction;
	// 				friction.extremumValue = _ExtremumValue;
	// 				friction.asymptoteValue = _AsymptoteValue;
	// 				friction.extremumSlip = _ExtremumSlip;
	// 				friction.asymptoteSlip = _AsymptoteSlip;
	// 				friction.stiffness = stiffnessVal;
	// 				wheel.forwardFriction = friction;
	// 			}
	// 			_StiffnessVal = stiffnessVal;
	// 			break;
	// 		case TestType.MassSens:
				
	// 			if (_Trial == 0)
	// 			{
	// 				StringMessageData massSt = new StringMessageData();
	// 				massSt.Message = "Timestamp, Mass (kg), Stop Distance (ft), Distance To Person(ft), Closest Point(ft), Final Velocity (ft/s), Trial";
	// 				//continuousFile.WriteLine("Timestamp, Mass (kg), Forward Distance to Person(front plane)(ft), Closest Point(ft), Velocity(ft/s), Xpos (m), YPos (m), Zpos (m), Trial#");
	// 				LogManagerRef.AddPacketToQueue(massSt);
	// 				//endOfTrialFile.WriteLine(massSt.Message);
	// 			}

	// 			if (_Trial >= TrialCount)
	// 			{
	// 				SensTestIndex++;
	// 				if(SensTestIndex >= TestList.Count)
	// 				{
	// 					SensTestIndex = 0;
	// 				}
	// 				StringMessageData massDone = new StringMessageData();
	// 				massDone.Message = _currentVehicleVar.Vehicle.ToString() + " Mass Sensitivity Test Complete";
	// 				//endOfTrialFile.WriteLine(massDone.Message);
	// 				Debug.Log(_currentVehicleVar.Vehicle.ToString() + " Mass Sensitivity Test Complete");
	// 				_Trial = 0;
	// 				CurrentTest = TestList[SensTestIndex];
	// 				_Spawned = false;
	// 				continuousFile.Close();
	// 				File.Delete(Path.Combine(_path, contFileName));
	// 				_fileClosedFlag = true;
	// 				VehicleSceneController.CompleteMassSet();
	// 				//if (Vehicles.Count > 0)
	// 				//{
						
	// 				//    LogManagerRef.AddPacketToQueue(massDone);
	// 				//    _currentVehicleVar = Vehicles.Dequeue();
	// 				//    ShuttleCar = _currentVehicleVar.VehicleToLoad;
	// 				//    _Trial = 0;
	// 				//    SensTestIndex = 0;
	// 				//    _currentTest = _TestList[SensTestIndex];
	// 				//    _currentTestPoint = 0;
	// 				//    VehicleSceneController.CompleteMassSet();
	// 				//    Restart();
	// 				//    return;
	// 				//}
	// 				//else
	// 				//{
	// 				//    StringMessageData expDone = new StringMessageData();
	// 				//    expDone.Message = _currentVehicleVar.Vehicle.ToString() + " Mass Sensitivity Test Complete, Experiment Complete";
	// 				//    Debug.Log(_currentVehicleVar.Vehicle.ToString() + " Mass Sensitivity Test Complete, Experiment Complete");
	// 				//    LogManagerRef.AddPacketToQueue(expDone);
	// 				//    Application.Quit();
	// 				//}
	// 				return;
	// 			}
	// 			//if (_Trial >= _trialPerIncrement)
	// 			//{
	// 			//    _Trial = 0;
	// 			//    _currentTestPoint++;
	// 			//    Restart();
	// 			//    return;
	// 			//}
	// 			DelayTime = (double)_lagTimeVar.NominalVal;

	// 			float mass = _currentVehicleVar.EmptyMass + _Trial * (_currentVehicleVar.Overload - _currentVehicleVar.EmptyMass) /(TrialCount - 1);//Test point count set to be inclusive
	// 			CarReference.GetComponent<Rigidbody>().mass = mass;
	// 			_CurrentVariableValue = "Mass";
				
	// 			break;
	// 		case TestType.YellowZoneSens:
				
	// 			//FieldBasedProxSystem fieldProx = CarReference.GetComponentInChildren<FieldBasedProxSystem>();
	// 			double delayIn5;
	// 			if (double.TryParse(MillisecondDelay.text, out delayIn5))
	// 			{
	// 				DelayTime = delayIn5 / 1000;					
	// 			}
	// 			//float yellowZone = (_Trial * 0.01f);
	// 			//_yellowZone = yellowZone;
	// 			//if (yellowZone > 0.2f) //Trial ends after brake torque test of 10,000
	// 			//{
	// 			//	_TrialMessage.Message = "Complete";
	// 			//	LogManagerRef.AddPacketToQueue(_TrialMessage);
	// //                endOfTrialFile.WriteLine(_TrialMessage.Message);
	// //                Debug.Log("Test complete");
	// 			//	gameObject.SetActive(false);
	// 			//	Application.Quit();
	// 			//}

	// 			//for (int i = 0; i < fieldProx.FieldGenerators.Count; i++)
	// 			//{                    
	// 			//	FieldGenerator gen = fieldProx.FieldGenerators[i];
	// 			//	gen.B_Yellow = yellowZone;
	// 			//	fieldProx.FieldGenerators[i] = gen;
	// 			//}
	// 			break;
	// 		case TestType.DelaySens:                
				
	// 			if (_Trial == 0)
	// 			{
	// 				StringMessageData delSt = new StringMessageData();
	// 				delSt.Message = "Timestamp, Delay (s), Stop Distance (ft), Distance To Person(ft), Closest Point(ft), Final Velocity (ft/s), Trial";
	// 				//continuousFile.WriteLine("Timestamp, Delay (s), Forward Distance to Person(front plane)(ft), Closest Point(ft), Velocity (ft/s), Xpos, YPos, Zpos, Trial#");
	// 				LogManagerRef.AddPacketToQueue(delSt);
	// 				//endOfTrialFile.WriteLine(delSt.Message);
	// 			}
	// 			if (_Trial >= TrialCount)
	// 			{
	// 				SensTestIndex++;
	// 				CurrentTest = TestList[SensTestIndex];
	// 				_currentTestPoint = 0;
	// 				_Trial = 0;
	// 				StringMessageData delDone = new StringMessageData();
	// 				delDone.Message = "Delay Sensitivity Test Complete";
	// 				LogManagerRef.AddPacketToQueue(delDone);
	// 				//endOfTrialFile.WriteLine(delDone.Message);
	// 				_Spawned = false;
	// 				continuousFile.Close();
	// 				File.Delete(Path.Combine(_path, contFileName));
	// 				_fileClosedFlag = true;
	// 				Restart();
	// 				return;
	// 			}
	// 			DelayTime = _lagTimeVar.MinVal + _Trial * ((_lagTimeVar.MaxVal - _lagTimeVar.MinVal) / (TrialCount - 1));//Test point count set to be inclusive
				
	// 			_CurrentVariableValue = "Delay";
	// 			//Debug.Log(_lagTimeVar.MinVal + "," + _lagTimeVar.NominalVal + "," + _lagTimeVar.MaxVal + ",cur: " + DelayTime);
	// 			//if (_Trial >= _trialPerIncrement)
	// 			//{
	// 			//    _Trial = 0;
	// 			//    _currentTestPoint++;
	// 			//    Restart();
	// 			//    return;
	// 			//}
	// 			break;
	// 		case TestType.InclineSens:
	// 			DelayTime = (double)_lagTimeVar.NominalVal;
	// 			if (_Trial == 0)
	// 			{
	// 				StringMessageData incStart = new StringMessageData();
	// 				incStart.Message = "Timestamp, Incline (deg), Stop Distance(ft), Distance To Person(ft), Closest Point(ft), Final Velocity(ft/s), Trial";
	// 				//continuousFile.WriteLine("Timestamp, Incline (deg), Forward Distance to Person(front plane)(ft), Closest Point(ft), Velocity(ft/s), Xpos, YPos, Zpos, Trial#");
	// 				LogManagerRef.AddPacketToQueue(incStart);
	// 				//endOfTrialFile.WriteLine(incStart.Message);
	// 			}
	// 			if (_Trial >= TrialCount)
	// 			{
	// 				SensTestIndex++;
	// 				CurrentTest = TestList[SensTestIndex];
	// 				_currentTestPoint = 0;
	// 				_Trial = 0;
	// 				StringMessageData incDone = new StringMessageData();
	// 				incDone.Message = "Incline Sensitivity Test Complete";
	// 				LogManagerRef.AddPacketToQueue(incDone);
	// 				//endOfTrialFile.WriteLine(incDone.Message);
	// 				_Spawned = false;
	// 				continuousFile.Close();
	// 				File.Delete(Path.Combine(_path, contFileName));
	// 				_fileClosedFlag = true;
	// 				Restart();
	// 				return;
	// 			}
	// 			//if (_Trial >= _trialPerIncrement)
	// 			//{
	// 			//    _Trial = 0;
	// 			//    _currentTestPoint++;
	// 			//    Restart();
	// 			//    return;
	// 			//}

	// 			_CurrentIncline = _gradeVar.MinVal + _Trial * ((_gradeVar.MaxVal - _gradeVar.MinVal) /(TrialCount - 1));//Test point count set to be inclusive
	// 			_Incline = _CurrentIncline;
	// 			_CurrentVariableValue = "Incline";                
	// 			Quaternion rot = Quaternion.Euler(-_CurrentIncline, 0, 0);                
	// 			Mine.transform.rotation = rot;
	// 			//_Trial++;
	// 			break;
	// 		case TestType.Baseline:
	// 			DelayTime = (double)500 / 1000;

	// 			if(_Trial > 100)
	// 			{
	// 				_TrialMessage.Message = "Complete";
	// 				LogManagerRef.AddPacketToQueue(_TrialMessage);
	// 				endOfTrialFile.WriteLine(_TrialMessage.Message);
	// 				Debug.Log("Test complete");
	// 				gameObject.SetActive(false);
	// 				Application.Quit();
	// 			}
	// 			break;
	// 		case TestType.SpeedSens:
	// 			if (_Trial == 0)
	// 			{
	// 				StringMessageData spdSt = new StringMessageData();
	// 				spdSt.Message = "Timestamp, Initial Speed (ft/s), Stop Distance(ft), Distance To Person(ft), Closest Point(ft), Final Velocity(ft/s), Trial";
	// 				//continuousFile.WriteLine("Timestamp, Initial Speed (ft/s), Forward Distance to Person(front plane)(ft), Closest Point(ft), Velocity(ft/s), Xpos, YPos, Zpos, Trial#");
	// 				LogManagerRef.AddPacketToQueue(spdSt);
	// 				//endOfTrialFile.WriteLine(spdSt.Message);
	// 			}
	// 			if (_Trial >= TrialCount)
	// 			{
	// 				SensTestIndex++;
	// 				CurrentTest = TestList[SensTestIndex];
	// 				_currentTestPoint = 0;
	// 				_Trial = 0;
	// 				StringMessageData spdDone = new StringMessageData();
	// 				spdDone.Message = "Initial Speed Sensitivity Test Complete";
	// 				LogManagerRef.AddPacketToQueue(spdDone);
	// 				//endOfTrialFile.WriteLine(spdDone.Message);
	// 				_Spawned = false;
	// 				continuousFile.Close();
	// 				File.Delete(Path.Combine(_path, contFileName));
	// 				_fileClosedFlag = true;
	// 				Restart();
	// 				return;
	// 			}
	// 			//if (_Trial >= _trialPerIncrement)
	// 			//{
	// 			//    _Trial = 0;
	// 			//    _currentTestPoint++;
	// 			//    Restart();
	// 			//    return;
	// 			//}

	// 			DelayTime = (double)_lagTimeVar.NominalVal;
	// 			float speed = _initialSpeedVar.MinVal + _Trial * ((_initialSpeedVar.MaxVal - _initialSpeedVar.MinVal) /(TrialCount - 1));//Test point count set to be inclusive
	// 			_InitialSpeed = speed;
	// 			//_CurrentVariableValue = speed.ToString();
	// 			_CurrentVariableValue = "InitialSpeed";
	// 			speed = speed * 0.681818f;//To MPH from FPS
	// 			//_CurrentVariableValue = speed.ToString();
	// 			CarRefController.SetMaxSpeed(speed);
	// 			break;
	// 		case TestType.AsForce:
	// 			if(_Trial == 0)
	// 			{
	// 				StringMessageData spdSt = new StringMessageData();
	// 				spdSt.Message = "Timestamp,Asymptote Force, Stop Distance(ft), Distance To Person(ft), Closest Point(ft), Final Velocity(ft/s),Trial";
	// 				//continuousFile.WriteLine("Timestamp, Asymptote Force, Forward Distance to Person(front plane)(ft), Closest Point(ft), Velocity(ft/s), Xpos, YPos, Zpos, Trial#");
	// 				LogManagerRef.AddPacketToQueue(spdSt);
	// 				//endOfTrialFile.WriteLine(spdSt.Message);
	// 			}
	// 			if(_Trial >= TrialCount)
	// 			{
	// 				SensTestIndex++;
	// 				CurrentTest = TestList[SensTestIndex];
	// 				_currentTestPoint = 0;
	// 				_Trial = 0;
	// 				StringMessageData spdDone = new StringMessageData();
	// 				spdDone.Message = "Asymptote Force Sensitivity Test Complete";
	// 				LogManagerRef.AddPacketToQueue(spdDone);
	// 				//endOfTrialFile.WriteLine(spdDone.Message);
	// 				_Spawned = false;
	// 				continuousFile.Close();
	// 				File.Delete(Path.Combine(_path, contFileName));
	// 				_fileClosedFlag = true;
	// 				Restart();
	// 				return;
	// 			}
	// 			float force = _asymptoteValVar.MinVal + (_Trial * ((_asymptoteValVar.MaxVal - _asymptoteValVar.MinVal)/ (TrialCount - 1)));
	// 			_AsForce = force;
	// 			WheelCollider[] AVWheels = CarReference.GetComponentsInChildren<WheelCollider>();
	// 			foreach (WheelCollider wheel in AVWheels)//update all wheel colliders with new friction values
	// 			{
	// 				WheelFrictionCurve friction = wheel.forwardFriction;
	// 				friction.asymptoteValue = force;
	// 				wheel.forwardFriction = friction;
	// 			}
	// 			_CurrentVariableValue = "AsymptoteForce";
	// 			break;
	// 		case TestType.AsSlip:
	// 			if (_Trial == 0)
	// 			{
	// 				StringMessageData spdSt = new StringMessageData();
	// 				spdSt.Message = "Timestamp,Asymptote Slip,Stop Distance(ft),Distance To Person(ft),Closest Point(ft),Final Velocity(ft/s),Trial";
	// 				//continuousFile.WriteLine("Timestamp, Asymptote Slip, Forward Distance to Person(front plane)(ft), Closest Point(ft), Velocity(ft/s), Xpos, YPos, Zpos, Trial#");
	// 				LogManagerRef.AddPacketToQueue(spdSt);
	// 				//endOfTrialFile.WriteLine(spdSt.Message);
	// 			}
	// 			if (_Trial >= TrialCount)
	// 			{
	// 				SensTestIndex++;
	// 				CurrentTest = TestList[SensTestIndex];
	// 				_currentTestPoint = 0;
	// 				_Trial = 0;
	// 				StringMessageData spdDone = new StringMessageData();
	// 				spdDone.Message = "Asymptote Slip Sensitivity Test Complete";
	// 				LogManagerRef.AddPacketToQueue(spdDone);
	// 				//endOfTrialFile.WriteLine(spdDone.Message);
	// 				_Spawned = false;
	// 				continuousFile.Close();
	// 				File.Delete(Path.Combine(_path, contFileName));
	// 				_fileClosedFlag = true;
	// 				Restart();
	// 				return;
	// 			}
	// 			float slp = _asymptoteSlpVar.MinVal + (_Trial * ((_asymptoteSlpVar.MaxVal-_asymptoteSlpVar.MinVal) / (TrialCount-1)));
	// 			_AsSlip = slp;
	// 			WheelCollider[] ASWheels = CarReference.GetComponentsInChildren<WheelCollider>();
	// 			foreach (WheelCollider wheel in ASWheels)//update all wheel colliders with new friction values
	// 			{
	// 				WheelFrictionCurve friction = wheel.forwardFriction;
	// 				friction.asymptoteSlip = slp;
	// 				wheel.forwardFriction = friction;
	// 			}
	// 			_CurrentVariableValue = "AsymptoteSlip";
	// 			break;
	// 		case TestType.ExForce:
	// 			if (_Trial == 0)
	// 			{
	// 				StringMessageData spdSt = new StringMessageData();
	// 				spdSt.Message = "Timestamp,Extremum Force,Stop Distance(ft),Distance To Person(ft),Closest Point(ft),Final Velocity(ft/s),Trial";
	// 				//continuousFile.WriteLine("Timestamp, Extremum Force, Forward Distance to Person(front plane)(ft), Closest Point(ft), Velocity(ft/s), Xpos, YPos, Zpos, Trial#");
	// 				LogManagerRef.AddPacketToQueue(spdSt);
	// 				//endOfTrialFile.WriteLine(spdSt.Message);
	// 			}
	// 			if (_Trial >= TrialCount)
	// 			{
	// 				SensTestIndex++;
	// 				CurrentTest = TestList[SensTestIndex];
	// 				_currentTestPoint = 0;
	// 				_Trial = 0;
	// 				StringMessageData spdDone = new StringMessageData();
	// 				spdDone.Message = "Extremum Force Sensitivity Test Complete";
	// 				LogManagerRef.AddPacketToQueue(spdDone);
	// 				//endOfTrialFile.WriteLine(spdDone.Message);
	// 				_Spawned = false;
	// 				continuousFile.Close();
	// 				File.Delete(Path.Combine(_path, contFileName));
	// 				_fileClosedFlag = true;
	// 				Restart();
	// 				return;
	// 			}
	// 			float Eforce = _extremeValVar.MinVal + (_Trial * ((_extremeValVar.MaxVal - _extremeValVar.MinVal) / (TrialCount - 1)));
	// 			_ExForce = Eforce;
	// 			WheelCollider[] EVWheels = CarReference.GetComponentsInChildren<WheelCollider>();
	// 			foreach (WheelCollider wheel in EVWheels)//update all wheel colliders with new friction values
	// 			{
	// 				WheelFrictionCurve friction = wheel.forwardFriction;
	// 				friction.extremumValue = Eforce;
	// 				wheel.forwardFriction = friction;
	// 			}
	// 			_CurrentVariableValue = "ExtremumForce";
	// 			break;
	// 		case TestType.ExSlip:
	// 			if (_Trial == 0)
	// 			{
	// 				StringMessageData spdSt = new StringMessageData();
	// 				spdSt.Message = "Timestamp,Extremum Slip,Stop Distance(ft),Distance To Person(ft),Closest Point(ft),Final Velocity(ft/s),Trial";
	// 				//continuousFile.WriteLine("Timestamp, Extremum Slip, Forward Distance to Person(front plane)(ft), Closest Point(ft), Velocity(ft/s), Xpos, YPos, Zpos, Trial#");
	// 				LogManagerRef.AddPacketToQueue(spdSt);
	// 				//endOfTrialFile.WriteLine(spdSt.Message);
	// 			}
	// 			if (_Trial >= TrialCount)
	// 			{
	// 				SensTestIndex++;
	// 				CurrentTest = TestList[SensTestIndex];
	// 				_currentTestPoint = 0;
	// 				_Trial = 0;
	// 				StringMessageData spdDone = new StringMessageData();
	// 				spdDone.Message = "Extremum Slip Sensitivity Test Complete";
	// 				LogManagerRef.AddPacketToQueue(spdDone);
	// 				//endOfTrialFile.WriteLine(spdDone.Message);
	// 				_Spawned = false;
	// 				continuousFile.Close();
	// 				File.Delete(Path.Combine(_path, contFileName));
	// 				_fileClosedFlag = true;
	// 				Restart();
	// 				return;
	// 			}
	// 			float Eslp = _extremeSlpVar.MinVal + (_Trial * ((_extremeSlpVar.MaxVal - _extremeSlpVar.MinVal) / (TrialCount - 1)));
	// 			_ExSlip = Eslp;
	// 			WheelCollider[] ESWheels = CarReference.GetComponentsInChildren<WheelCollider>();
	// 			foreach (WheelCollider wheel in ESWheels)//update all wheel colliders with new friction values
	// 			{
	// 				WheelFrictionCurve friction = wheel.forwardFriction;
	// 				friction.asymptoteSlip = Eslp;
	// 				wheel.forwardFriction = friction;
	// 			}
	// 			_CurrentVariableValue = "ExtremumSlip";
	// 			break;
	// 		case TestType.SimpleMassTest:
	// 			if (_Trial == 0)
	// 			{
	// 				StringMessageData massSt = new StringMessageData();
	// 				massSt.Message = "Timestamp, Mass (kg), Stop Distance (ft), Distance To Person(ft), Closest Point(ft), Final Velocity (ft/s), Trial";
	// 				//continuousFile.WriteLine("Timestamp, Mass (kg), Forward Distance to Person(front plane)(ft), Closest Point(ft), Velocity(ft/s), Xpos (m), YPos (m), Zpos (m), Trial#");
	// 				LogManagerRef.AddPacketToQueue(massSt);
	// 				//endOfTrialFile.WriteLine(massSt.Message);
	// 			}

	// 			if (_Trial >= 3)
	// 			{
	// 				SensTestIndex++;
	// 				if (SensTestIndex >= TestList.Count)
	// 				{
	// 					SensTestIndex = 0;
	// 				}
	// 				StringMessageData massDone = new StringMessageData();
	// 				massDone.Message = _currentVehicleVar.Vehicle.ToString() + " Mass Test Complete";
	// 				//endOfTrialFile.WriteLine(massDone.Message);
	// 				Debug.Log(_currentVehicleVar.Vehicle.ToString() + " Mass Test Complete");
	// 				_Trial = 0;
	// 				//CurrentTest = TestList[SensTestIndex];
	// 				_Spawned = false;
	// 				_fileClosedFlag = true;
	// 				VehicleSceneController.CompleteMassSet();
	// 				return;
	// 			}                
	// 			DelayTime = (double)_lagTimeVar.NominalVal;

	// 			float massV = _currentVehicleVar.EmptyMass;
	// 			switch (_Trial)
	// 			{
	// 				case 0:
	// 					massV = _currentVehicleVar.EmptyMass;
	// 					break;
	// 				case 1:
	// 					massV = _currentVehicleVar.FullMass;
	// 					break;
	// 				case 2:
	// 					massV = _currentVehicleVar.Overload;
	// 					break;
	// 			}
	// 			CarReference.GetComponent<Rigidbody>().mass = massV;
	// 			_CurrentVariableValue = "Mass";
	// 			break;
	// 		default:
	// 			break;
	// 	}

	// 	string message = "";
	// 	string twoStage = _currentVehicleVar.IgnoreYellowZone ? "OneStage" : "Twostage";
	// 	message = String.Format("PrefabName: {0}, TestVariable: {1}, InitialSpeed: {2}_fps, BrakeTorque: {3}_ft-lb, DelayTime: {4}_s, AsForce: {5}, AsSlip: {6}, ExForce: {7}, ExSlip: {8}, Incline: {9}_deg, Mass: {10}_kg, BrakingMethod: {11}, MWCLocation: {12}", _currentVehicleVar.VehicleToLoad.name, _CurrentVariableValue, _InitialSpeed, _BrakeTorque, DelayTime, _AsForce, _AsSlip, _ExForce, _ExSlip, _Incline, _VehRigidbody.mass, twoStage, Miner.name);
	// 	try
	// 	{
	// 		if (!IsDemo)
	// 		{
	// 			continuousFile.WriteLine(message);
	// 			continuousFile.WriteLine("Time_s, V_Xpos_ft, V_Ypos_ft, V_Zpos_ft, V_Xrot, V_Yrot, V_Zrot, V_Wrot, MWC_Xpos_ft, MWC_Ypos_ft, MWC_Zpos_ft, V_Velocity_fps, V_AngularVelocityX_rad/s, V_AngularVelocityY_rad/s, V_AngularVelocityZ_rad/s, FL_WheelSpeed_rad/s, FR_WheelSpeed_rad/s, RL_WheelSpeed_rad/s, RR_WheelSpeed_rad/s, FL_AccelTorque_ft-lb, FR_AccelTorque_ft-lb, RL_AccelTorque_ft-lb, RR_AccelTorque_ft-lb, FL_BrakeTorque_ft-lb, FR_BrakeTorque_ft-lb, RL_BrakeTorque_ft-lb, RR_BrakeTorque_ft-lb, FL_IsGrounded, FR_IsGrounded, RL_IsGrounded, RR_IsGrounded, StopPlane_ft, StopMachine_ft, ProxZone, VehicleState");
	// 		}
	// 	}
	// 	catch
	// 	{
	// 		Debug.LogError("Failed to save to continuous logging!");
	// 		Application.Quit();
	// 	}

	// 	//UpdateStatusText();

	// 	_Trial++;
	// 	_TotalTrialCount++;        
	// }

	// private void SetMWCLocation(MWCLocation loc)
	// {
	// 	//Vector3 posAdjust = MineWearableComponent.transform.localPosition;
	// 	//posAdjust.x = xPos;
	// 	//MineWearableComponent.transform.localPosition = posAdjust;
	// 	//foreach(GameObject obj in MWCCategoricalVar)
	// 	//{
	// 	//    obj.SetActive(false);
	// 	//}
	// 	//mwc.SetActive(true);
	// 	//Miner = mwc.transform;

	// 	ImpactProxEffect proxEffect = null;

	// 	MWCCenter.SetActive(false);
	// 	MWCLeft.SetActive(false);
	// 	MWCRight.SetActive(false);
	// 	switch (loc)
	// 	{
	// 		case MWCLocation.Center:
	// 			MWCCenter.SetActive(true);
	// 			Miner = MWCCenter.transform;
	// 			proxEffect = MWCCenter.GetComponent<ImpactProxEffect>();
	// 			break;
	// 		case MWCLocation.Left:
	// 			MWCLeft.SetActive(true);
	// 			Miner = MWCLeft.transform;
	// 			proxEffect = MWCLeft.GetComponent<ImpactProxEffect>();
	// 			break;
	// 		case MWCLocation.Right:
	// 			MWCRight.SetActive(true);
	// 			Miner = MWCRight.transform;
	// 			proxEffect = MWCRight.GetComponent<ImpactProxEffect>();
	// 			break;
	// 		default:
	// 			break;
	// 	}

	// 	if (proxEffect != null)
	// 		proxEffect.Reset();
	// }

	//OBSOLETE
	private void SetYellowZonePrefab()
	{
		Debug.Log("Cur veh:" + _currentVehicleVar.Vehicle.ToString() + " YellowZonecount: " + _currentVehicleVar.PrefabsDefinedYellowZone.Count + " YelZonIndex:" + _YellowZoneCategoricalIndex);
		ShuttleCar = _currentVehicleVar.PrefabsDefinedYellowZone[_YellowZoneCategoricalIndex];        
	}

	/// <summary>
	/// Exponential fit for use in slowing down yellow zone, note: we can just as easily apply torque to this as well. This is a simple fit, more refined models simulating regenerative braking are likely in order.
	/// </summary>
	/// <param name="t">Current time between 0 and final time</param>
	/// <param name="vMax">Max velocity</param>
	/// <param name="vMin">Minimum velocity (what you aim for)</param>
	/// <param name="finalTime">How long it takes to reach your final velocity</param>
	/// <returns></returns>
	private float VelocityFit(float t, float vMax, float vMin, float finalTime)
	{
		float output = 0;
		output = vMax * Mathf.Pow((Mathf.Pow((vMin / vMax), (1 / finalTime))),t);

		return output;
	}
	private void OnDestroy()
	{
		if (!IsDemo)
		{
			endOfTrialFile.Flush();
			continuousFile.Flush();
			endOfTrialFile.Close();
			continuousFile.Close();
		}
	}
}