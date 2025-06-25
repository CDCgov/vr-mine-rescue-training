using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using CsvHelper;
using UnityStandardAssets.Vehicles.Car;
using System.Globalization;

#pragma warning disable 414

[System.Serializable]
public class MachineConstants
{
	public string Name;
	public float EmptyWeight;
	public float LoadCapacity;
	public float InitialSpeed = 5;
	public float BrakeTorqueFtLb = 10000;
	public float AccelTorqueFtLb = 10000;
	public float WheelWeight = 200;
	public float WheelDampingRate = 10;
}

[System.Serializable]
public struct ProxOffsetConfig
{
	public string Name;
	public ProxMachineOffsets Yellow;
	public ProxMachineOffsets Red;
}

[System.Serializable]
public struct ProxMachineOffsets
{
	public float Front;
	public float Right;
	public float Left;
	public float Back;
}

public enum MotorControlState
{
	NormalForward,
	YellowBraking,
	YellowSteady,
	Red,
	Stopped
}

[CreateAssetMenu(fileName = "VehicleProxExperiment", menuName = "VRMine/Experiments/VehicleProxExperiment", order = 0)]
public class VehicleProxExperiment : Experiment
{

	private const int FrameDelay = 5;
	private const float PhysicsPreloadSteps = 250;

	private const float SteadyStateTime = 2;
	private const float YellowZoneSpeedLimit = 0.762f; // == 2.5 fps
	private const float YellowZoneSpeedLimitMPH = 1.70455f;
	private const float SteadyStateTolerance = 0.05f; //%

	public const float ConvFPSToMPH = 0.681818f;
	public const float ConvMPHToFPS = 1.0f / ConvFPSToMPH;
	public const float ConvFPSToMS = 0.3048f;
	public const float ConvMSToFPS = 1.0f / ConvFPSToMS;

	public const float ConvFTLBToNM = 1.356f;
	public const float ConvNMToFTLB = 1.0f / ConvFTLBToNM;

	public const float ConvLBFToN = 4.44822f;
	public const float ConvNToLBF = 1.0f / ConvLBFToN;

	public const float ConvFtToM = 0.3048f;
	public const float ConvMToFt = 1.0f / ConvFtToM;

	public const float ConvINToM = 0.0254f;
	public const float ConvMToIN = 1.0f / ConvINToM;

	public const float ConvLBToKG = 0.453592f;
	public const float ConvKGToLB = 1.0f / ConvLBToKG;

	public const float ConvRPMToRadPS = 2 * Mathf.PI / 60;

	public bool EnableDetailedLogs = true;
	public bool ShowPIDOutput = true;

	public GameObject[] MachinePrefabs;
	public MachineConstants[] MachineConstants;
	public ProxOffsetConfig[] ProxConfigs;


	public VehicleProxTrialSettings CurrentTrialSettings
	{
		get { return _trialSettings; }
	}

	public float ElapsedTime
	{
		get { return _elapsedTime; }
	}

	public Vector3 CurrentVelocity
	{
		get
		{
			if (_vehRigidbody != null)
			{
				return _vehRigidbody.velocity;
			}
			else
			{
				return Vector3.zero;
			}
		}
	}

	//public CarController CarController
	//{
	//	get
	//	{
	//		return _carController;
	//	}
	//}

	VehicleProxTrialSettings _trialSettings;
	MachineConstants _machineConstants;
	private float _initialSpeedFPS;
	private float _brakeTorqueNM;
	private float _accelTorqueNM;
	private float _wheelWeightLb;
	private float _wheelDampingRate;

	private Transform _miner;
	private GameObject _carReference;
	private GameObject _proxSystemObj;
	private ProxSystemController _proxSystem;
	private BoxProxSystem _boxProx;
	private Transform _frontOfShuttleCar;
	private GameObject _redPoint = null;
	private GameObject _yellowPoint = null;


	//private UnityStandardAssets.Vehicles.Car.CarController _carController;
	private BoxCollider _bodyCollider;

	//machine dimension values
	private Vector3 _localMachineCenter; //center of the machine relative to the machine prefab's origin
	private float _localMachineFrontDist; //distance from the machine's origin to the front of the machine chassis
	private float _wheelBase;
	private float _wheelFrontTrack;
	private float _wheelRearTrack;
	private float _backToWheel;
	private float _frontToWheel;
	private float _chassisCenterHeight;
	private float _chassisGroundClearance;


	private float _minDistanceToVehicle = 10000; //Arbitrary high amount
	private float _minFrontPlaneDist = 10000;
	private bool _yellowBrakingEngaged = false;
	//private bool _VehicleStopped = false;
	private bool _achievedSteadyState = false;
	private Vector3 _yellowTriggerPosition;
	private float _yellowTriggerTime = 0;
	private float _yellowZoneDistance;
	private Vector3 _redTriggerPosition;
	private float _redTriggerTime = 0;
	private float _redZoneDistance;
	//private float _steadyStateStartTime = 0;
	//private float _StartInitialSpeedTime = 0;
	// private float _initialSpeedSSDuration = 0;
	// private float _yellowZoneBrakingStartTime = 0;
	// private float _yellowZoneBrakingDuration = 0;
	// private float _yellowZoneSteadyStartTime = 0;
	// private float _yellowZoneSteadyStateDuration = 0;
	// private float _redZoneBrakingStartTime = 0;
	// private float _redZoneBrakingDuration = 0;

	private float _timeEnteredInitialSteady = 0;
	private float _initialSteadyDuration = 0;
	private float _timeEnteredYellow = 0;
	private float _yellowDuration = 0;
	private float _timeEnteredYellowBraking = 0;
	private float _yellowBrakingDuration = 0;
	private float _timeEnteredYellowSteady = 0;
	private float _yellowSteadyDuration = 0;
	private float _timeEnteredRed = 0;
	private float _redDuration = 0;
	private float _timeEnteredRedBraking = 0;
	private float _redBrakingDuration = 0;

	private float _timeStopped = 0; //time in the trial the vehicle entered the stopped state
	private VehicleState _vehState = VehicleState.BrakeHold;
	private MotorControlState _motorState = MotorControlState.NormalForward;
	//private double _delayTime = 0.2;//Deprecated from Demo Mode
	private double _delay = 0;//Deprecated from Demo Mode
	private string _error = "";
	private bool _ignoreYellowZone = false;
	private bool _checkTimeFlag = false;
	private bool _brakesEngaged;
	private float _machineWeightLb = 0;

	private bool _trialComplete = false;

	private Plane _groundPlane;
	private bool _brakingStarted = false;
	private bool _brakingOcurredInYellowSteady = false;
	private Vector3 _brakingStartPos;
	private float _frontPlaneDist;
	private float _distanceToVehicle;
	private float _currentForwardSlip;
	private float _currentSideSlip;

	private double _totalYellowSlip;
	private int _totalYellowSlipSamples;

	private double _totalRedSlip;
	private int _totalRedSlipSamples;
	//private float _trialStartTime;
	//private float _trialEndTime;
	private float _elapsedTime;
	private int _startFrame;

	private WheelCollider[] _Wheels;
	private WheelCollider _FLWheel;
	private WheelCollider _FRWheel;
	private WheelCollider _RLWheel;
	private WheelCollider _RRWheel;
	private Rigidbody _vehRigidbody;
	private float[] _PriorVelocities; //For friction fix
	private CollisionLog _collisionLog;

	private VehicleProxExperimentSceneData _scene;

	private int _numSamples = 0;
	//private float _PriorFixedUpdateTime;
	private int _BadTimeStepCount;
	private float _MaxDeltaTime;
	private ExperimentManager _manager;
	private bool _initialized = false;

	private CsvWriter _csvLog;
	private StreamWriter _csvStream;

	public override bool Initialized
	{
		get
		{
			return _initialized;
		}
	}

	public override bool Complete
	{
		get
		{
			return _trialComplete;
		}
	}

	private void OnEnable()
	{

	}

	private void OnDisable()
	{
		Debug.Log("VehicleProxExperiment Disabled");
		CloseLogFile();
	}

	public override string GetScenePath(TrialSettings settings)
	{
		string sceneName;

		sceneName = ((VehicleProxTrialSettings)settings).SceneName;

		int numScenes = SceneManager.sceneCountInBuildSettings;
		for (int i = 0; i < numScenes; i++)
		{
			var scene = SceneManager.GetSceneByBuildIndex(i);
			var scenePath = SceneUtility.GetScenePathByBuildIndex(i);

			if (scenePath.Contains(sceneName))
				return scenePath;
		}

		throw new System.Exception($"Couldn't find experiment scene in build: {sceneName}");
	}

	public override TrialSettings ParseTrialSettings(Dictionary<string, ExperimentVal> settings)
	{
		if (_trialSettings == null)
			_trialSettings = new VehicleProxTrialSettings();

		_trialSettings.LoadSettings(settings);

		var proxConfigName = _trialSettings.ProxConfig.ToLower();
		bool foundConfig = false;

		//if the prox config is set to a specific config, override the prox machine offset values
		if (ProxConfigs != null && ProxConfigs.Length > 0)
		{
			foreach (var config in ProxConfigs)
			{
				if (config.Name.ToLower() == proxConfigName)
				{
					_trialSettings.ProxYFront = config.Yellow.Front;
					_trialSettings.ProxYLeft = config.Yellow.Left;
					_trialSettings.ProxYRight = config.Yellow.Right;
					_trialSettings.ProxYBack = config.Yellow.Back;

					_trialSettings.ProxRFront = config.Red.Front;
					_trialSettings.ProxRLeft = config.Red.Left;
					_trialSettings.ProxRRight = config.Red.Right;
					_trialSettings.ProxRBack = config.Red.Back;

					foundConfig = true;
					break;
				}
			}
		}

		if (!foundConfig && proxConfigName.ToLower() != "custom")
		{
			throw new System.Exception($"Invalid prox config specified: {proxConfigName}");
		}

		return _trialSettings;
	}

	public override IEnumerator Initialize(TrialSettings settings,
		string sessionName, string blockName, int trialNum)
	{
		Physics.IgnoreLayerCollision(19, 19);
		System.GC.Collect();
		Time.fixedDeltaTime = settings.ManualUpdateTimestep;

		//skip first frame
		yield return null;

		_manager = ExperimentManager.GetDefault();

		try
		{
			_session = sessionName;
			_block = blockName;
			_trialNum = trialNum;

			_numSamples = 0;
			_MaxDeltaTime = 0;

			var scenObj = GameObject.Find("SceneData");
			_scene = scenObj.GetComponent<VehicleProxExperimentSceneData>();


			switch (_trialSettings.BrakingModel.ToLower())
			{
				case "onestage":
					_ignoreYellowZone = true;
					break;

				case "twostage":
					_ignoreYellowZone = false;
					break;

				default:
					throw new System.Exception("Invalid braking model in experiment config");
			}


			_PriorVelocities = new float[3];
			for (int i = 0; i < 3; i++)
			{
				_PriorVelocities[i] = -1;
			}

			//set mine slope
			Quaternion newRotation = Quaternion.Euler(_trialSettings.Slope * -1, 0, 0);
			_scene.Mine.transform.rotation = newRotation;
			_groundPlane = new Plane(_scene.Mine.transform.up, _scene.Mine.transform.position);


			//Time.timeScale = _trialSettings.TimeScale;

		}
		catch (System.Exception ex)
		{
			Debug.LogError(ex.Message);
			_manager.TerminateExperiment(ex.Message);
		}

		_scene.Mine.SetActive(false);
		for (int i = 0; i < 5; i++)
		{
			Physics.Simulate(_trialSettings.ManualUpdateTimestep);
			yield return null;
		}
		_scene.Mine.SetActive(true);

		SpawnShuttleCar();

		//turn on brake torque on the wheel colliders for stabilization phase
		//_carController.BrakePercentOverride = 1.0f;
		//_carController.Move(0, 0, -1, 0, settings.ManualUpdateTimestep);

		//if (EnableDetailedLogs) WriteLogEntry();
		yield return null;
		//if (EnableDetailedLogs) WriteLogEntry();

		for (int i = 0; i < PhysicsPreloadSteps; i++)
		{
			Physics.Simulate(settings.ManualUpdateTimestep);
			//_carController.Move(0, 0, -1, 0, settings.ManualUpdateTimestep);
			ForceAlignMachine();

			//if (EnableDetailedLogs) WriteLogEntry();
		}

		for (int i = 0; i < FrameDelay; i++)
		{
			if (Physics.simulationMode == SimulationMode.Script)
			{
				Physics.Simulate(settings.ManualUpdateTimestep);
				//_carController.Move(0, 0, -1, 0, settings.ManualUpdateTimestep);
				Vector3 eulerAngles = _vehRigidbody.rotation.eulerAngles;
				eulerAngles.y = 0;
				_vehRigidbody.rotation = Quaternion.Euler(eulerAngles);
			}
			//yield return null;
		}
		yield return null;

		if (EnableDetailedLogs)
		{
			CreateLogFile();
		}

		_proxSystem.EnableZoneVisualization(new global::ProxSystem.VisOptions(true, true));

		_initialized = true;
	}

	private void ForceAlignMachine()
	{
		Vector3 eulerAngles = _vehRigidbody.rotation.eulerAngles;
		eulerAngles.y = 0;
		_vehRigidbody.rotation = Quaternion.Euler(eulerAngles);
	}

	public override void StartExperiment()
	{
		//_PriorFixedUpdateTime = Time.time;
		//_trialStartTime = Time.time;
		_elapsedTime = 0;
		_minDistanceToVehicle = 10000; //Arbitrary high amount
		_minFrontPlaneDist = 10000;
		_BadTimeStepCount = 0;
		_error = "";
		_startFrame = Time.frameCount;

		_totalYellowSlip = 0;
		_totalYellowSlipSamples = 0;
		_totalRedSlip = 0;
		_totalRedSlipSamples = 0;


		// _redZoneBrakingStartTime = 0;
		// _redZoneBrakingDuration = 0;

		_timeEnteredInitialSteady = 0;
		_timeEnteredYellow = 0;
		_timeEnteredYellowBraking = 0;
		_timeEnteredYellowSteady = 0;
		_timeEnteredRed = 0;
		_timeEnteredRedBraking = 0;
		_timeStopped = 0;

		_initialSteadyDuration = 0;
		_yellowDuration = 0;
		_yellowBrakingDuration = 0;
		_yellowSteadyDuration = 0;
		_redDuration = 0;
		_redBrakingDuration = 0;

		_yellowZoneDistance = -1;
		_redZoneDistance = -1;
		_numSamples = 0;

		if (EnableDetailedLogs)
		{
			WriteLogEntry();
		}

	}

	private MachineConstants FindMachineConstants(string machineType)
	{
		foreach (var mconst in MachineConstants)
		{
			if (mconst.Name == machineType)
			{
				return mconst;
			}
		}

		return null;
	}

	private GameObject FindMachinePrefab(string prefabName)
	{
		foreach (var prefab in MachinePrefabs)
		{
			if (prefab.name == prefabName)
			{
				return prefab;
			}
		}

		return null;
	}

	private float GetElapsedTime()
	{
		//return Time.time - _trialStartTime;
		return _elapsedTime;
	}

	private void CreateLogFile()
	{
		//string logFile = Path.Combine(_manager.GetLogFolder(), $"{_session}-{_block}-{_carController.MachineType}-t{_trialNum}.csv");
		//_csvStream = new StreamWriter(logFile, false, System.Text.Encoding.UTF8);
		_csvLog = new CsvWriter(_csvStream, CultureInfo.InvariantCulture);

		_csvLog.WriteField("Time_s");
		_csvLog.WriteField("VehicleState");
		_csvLog.WriteField("MotorControlState");
		_csvLog.WriteField("ProxZone");

		_csvLog.WriteField("FrontPlaneDist_ft");
		_csvLog.WriteField("DistToVehicle_ft");
		_csvLog.WriteField("CollisionEvents");
		//_csvLog.WriteField("AvgWheelForce");
		_csvLog.WriteField("AvgForwardSlip");
		_csvLog.WriteField("AvgSideSlip");

		_csvLog.WriteField("V_Xpos_ft");
		_csvLog.WriteField("V_Ypos_ft");
		_csvLog.WriteField("V_Zpos_ft");
		_csvLog.WriteField("V_Xrot");
		_csvLog.WriteField("V_Yrot");
		_csvLog.WriteField("V_Zrot");
		_csvLog.WriteField("V_Wrot");
		_csvLog.WriteField("MWC_Xpos_ft");
		_csvLog.WriteField("MWC_Ypos_ft");
		_csvLog.WriteField("MWC_Zpos_ft");
		_csvLog.WriteField("V_Velocity_fps");
		_csvLog.WriteField("V_AngularVelocityX_rad/s");
		_csvLog.WriteField("V_AngularVelocityY_rad/s");
		_csvLog.WriteField("V_AngularVelocityZ_rad/s");
		_csvLog.WriteField("FL_WheelSpeed_rpm");
		_csvLog.WriteField("FR_WheelSpeed_rpm");
		_csvLog.WriteField("RL_WheelSpeed_rpm");
		_csvLog.WriteField("RR_WheelSpeed_rpm");
		_csvLog.WriteField("FL_AccelTorque_ft-lb");
		_csvLog.WriteField("FR_AccelTorque_ft-lb");
		_csvLog.WriteField("RL_AccelTorque_ft-lb");
		_csvLog.WriteField("RR_AccelTorque_ft-lb");
		_csvLog.WriteField("FL_BrakeTorque_ft-lb");
		_csvLog.WriteField("FR_BrakeTorque_ft-lb");
		_csvLog.WriteField("RL_BrakeTorque_ft-lb");
		_csvLog.WriteField("RR_BrakeTorque_ft-lb");
		_csvLog.WriteField("FL_IsGrounded");
		_csvLog.WriteField("FR_IsGrounded");
		_csvLog.WriteField("RL_IsGrounded");
		_csvLog.WriteField("RR_IsGrounded");

		// WriteWheelHitHeader(_csvLog, "FL");
		// WriteWheelHitHeader(_csvLog, "FR");
		// WriteWheelHitHeader(_csvLog, "RL");
		// WriteWheelHitHeader(_csvLog, "RR");

		_csvLog.NextRecord();
	}

	private void WriteLogEntry()
	{
		if (_csvLog == null)
			return;

		_csvLog.WriteField(GetElapsedTime());
		_csvLog.WriteField(_vehState.ToString());
		_csvLog.WriteField(_motorState.ToString());
		_csvLog.WriteField(_proxSystem.ActiveProxZone.ToString());

		_csvLog.WriteField(_frontPlaneDist);
		_csvLog.WriteField(_distanceToVehicle);
		_csvLog.WriteField(_collisionLog.EnterCount + _collisionLog.ExitCount + _collisionLog.StayCount);
		_csvLog.WriteField(_currentForwardSlip);
		_csvLog.WriteField(_currentSideSlip);

		_csvLog.WriteField(_vehRigidbody.position.x * ConvMToFt);
		_csvLog.WriteField(_vehRigidbody.position.y * ConvMToFt);
		_csvLog.WriteField(_vehRigidbody.position.z * ConvMToFt);
		_csvLog.WriteField(_vehRigidbody.rotation.x);
		_csvLog.WriteField(_vehRigidbody.rotation.y);
		_csvLog.WriteField(_vehRigidbody.rotation.z);
		_csvLog.WriteField(_vehRigidbody.rotation.w);
		_csvLog.WriteField(_miner.transform.position.x * ConvMToFt);
		_csvLog.WriteField(_miner.transform.position.y * ConvMToFt);
		_csvLog.WriteField(_miner.transform.position.z * ConvMToFt);
		_csvLog.WriteField(_vehRigidbody.velocity.magnitude * ConvMSToFPS);
		_csvLog.WriteField(_vehRigidbody.angularVelocity.x);
		_csvLog.WriteField(_vehRigidbody.angularVelocity.y);
		_csvLog.WriteField(_vehRigidbody.angularVelocity.z);
		_csvLog.WriteField(_FLWheel.rpm);
		_csvLog.WriteField(_FRWheel.rpm);
		_csvLog.WriteField(_RLWheel.rpm);
		_csvLog.WriteField(_RRWheel.rpm);
		_csvLog.WriteField(_FLWheel.motorTorque * ConvNMToFTLB);
		_csvLog.WriteField(_FRWheel.motorTorque * ConvNMToFTLB);
		_csvLog.WriteField(_RLWheel.motorTorque * ConvNMToFTLB);
		_csvLog.WriteField(_RRWheel.motorTorque * ConvNMToFTLB);
		_csvLog.WriteField(_FLWheel.brakeTorque * ConvNMToFTLB);
		_csvLog.WriteField(_FRWheel.brakeTorque * ConvNMToFTLB);
		_csvLog.WriteField(_RLWheel.brakeTorque * ConvNMToFTLB);
		_csvLog.WriteField(_RRWheel.brakeTorque * ConvNMToFTLB);
		_csvLog.WriteField(_FLWheel.isGrounded);
		_csvLog.WriteField(_FRWheel.isGrounded);
		_csvLog.WriteField(_RLWheel.isGrounded);
		_csvLog.WriteField(_RRWheel.isGrounded);


		//float avgForce = 0;
		// float avgForwardSlip = 0;
		// float avgSideSlip = 0;

		// if (ComputeWheelSlip(out avgForwardSlip, out avgSideSlip))
		// {
		// 	//_csvLog.WriteField(avgForce);
		// 	_csvLog.WriteField(avgForwardSlip);
		// 	_csvLog.WriteField(avgSideSlip);
		// }
		// else
		// {
		// 	//_csvLog.WriteField("ERROR");
		// 	_csvLog.WriteField("ERROR");
		// 	_csvLog.WriteField("ERROR");
		// }


		// WriteWheelHitEntry(_csvLog, _FLWheel);
		// WriteWheelHitEntry(_csvLog, _FRWheel);
		// WriteWheelHitEntry(_csvLog, _RLWheel);
		// WriteWheelHitEntry(_csvLog, _RRWheel);

		_csvLog.NextRecord();

	}

	private bool ComputeWheelSlip(out float avgForwardSlip, out float avgSideSlip)
	{
		WheelHit hit;
		int count = 0;
		avgForwardSlip = 0;
		avgSideSlip = 0;

		if (_Wheels == null || _Wheels.Length != 4)
			return false;

		for (int i = 0; i < 4; i++)
		{
			WheelCollider wheel = _Wheels[i];
			if (wheel.GetGroundHit(out hit))
			{
				count++;
				avgForwardSlip += hit.forwardSlip;
				avgSideSlip += hit.sidewaysSlip;
			}
		}

		if (count == 4)
		{
			avgForwardSlip /= 4.0f;
			avgSideSlip /= 4.0f;
		}

		return true;

	}

	private void WriteWheelHitHeader(CsvWriter csv, string prefix)
	{
		csv.WriteField($"{prefix}_Force");
		csv.WriteField($"{prefix}_FwdSlip");
		csv.WriteField($"{prefix}_SideSlip");
	}

	private void WriteWheelHitEntry(CsvWriter csv, WheelCollider wheel)
	{
		WheelHit hit;
		if (wheel.GetGroundHit(out hit))
		{
			csv.WriteField(hit.force);
			csv.WriteField(hit.forwardSlip);
			csv.WriteField(hit.sidewaysSlip);
		}
		else
		{
			for (int i = 0; i < 3; i++)
			{
				csv.WriteField("ERROR");
			}
		}
	}

	private void CloseLogFile()
	{
		Debug.Log("Closing log file");
		if (_csvStream != null)
		{
			_csvStream.Flush();
		}

		if (_csvLog != null)
		{
			_csvLog.Dispose();
			_csvLog = null;
		}

		if (_csvStream != null)
		{
			_csvStream.Dispose();
			_csvStream = null;
		}

	}

	private void WriteSessionLogHeader(CsvWriter csv)
	{
		csv.WriteField("Trial");
		csv.WriteField("Block");
		csv.WriteField("SceneName");
		csv.WriteField("PrefabName");
		csv.WriteField("MachineType");
		csv.WriteField("ProxConfig");
		csv.WriteField("TrialCompleted");
		csv.WriteField("FinalFrontPlaneDist_f");
		csv.WriteField("FinalClosestPointToMachine_f");
		csv.WriteField("MinFrontPlaneDist_f");
		csv.WriteField("MinClosestPointToMachine_f");
		csv.WriteField("HorizontalDeviation_ft");
		csv.WriteField("InitialSpeedMultiplier");
		csv.WriteField("BrakeTorqueMultiplier");
		csv.WriteField("InitialSpeed_fps");
		csv.WriteField("BrakeTorque_ft-lb");
		csv.WriteField("AccelTorque_ft-lb");
		csv.WriteField("DelayTime_s");
		csv.WriteField("AsymptoteForce");
		csv.WriteField("AsymptoteSlip");
		csv.WriteField("ExtremumForce");
		csv.WriteField("ExtremumSlip");
		csv.WriteField("Incline_deg");
		csv.WriteField("MachineWeight_lb");
		csv.WriteField("LoadLevel");
		csv.WriteField("BrakingModel");
		csv.WriteField("MWCLocation");
		csv.WriteField("AchievedSteadyState");

		// csv.WriteField("InitialSpeedSteadyStateDuration_s");
		// csv.WriteField("YellowZoneBrakingDuration_s");
		// csv.WriteField("YellowZoneSteadyStateDuration_s");
		csv.WriteField("InitialSteadyDuration_s");
		csv.WriteField("YellowDuration_s");
		csv.WriteField("YellowSteadyDuration_s");
		csv.WriteField("YellowBrakingDuration_s");
		csv.WriteField("AvgYellowZoneSlip");

		csv.WriteField("RedDuration_s");
		csv.WriteField("RedBrakingDuration_s");
		csv.WriteField("AvgRedZoneSlip");

		csv.WriteField("TrialDuration_s");
		csv.WriteField("NumSamples");
		csv.WriteField("NumFrames");
		csv.WriteField("MaxDeltaTime_s");

		csv.WriteField("YellowZoneStopDistance_f");
		csv.WriteField("RedZoneStopDistance_f");
		csv.WriteField("TotalStoppingDistance_f");

		csv.WriteField("M_Width_in");
		csv.WriteField("M_Wheelbase_in");
		csv.WriteField("M_WheelTrackFront_in");
		csv.WriteField("M_WheelTrackRear_in");
		csv.WriteField("M_TireRadius_in");
		csv.WriteField("M_GroundClearance_in");
		csv.WriteField("M_Thickness_in");
		csv.WriteField("M_Length_in");
		csv.WriteField("M_BackToWheel_in");
		csv.WriteField("M_FrontToWheel_in");
		csv.WriteField("M_ChassisCenterHeight_in");

		csv.WriteField("M_Cent_Offset_X_ft");
		csv.WriteField("M_Cent_Offset_Y_ft");
		csv.WriteField("M_Cent_Offset_Z_ft");

		csv.WriteField("YellowFrontDist_ft");
		csv.WriteField("YellowFrontSize_ft");
		csv.WriteField("RedFrontDist_ft");
		csv.WriteField("RedFrontSize_ft");
		csv.WriteField("CentOfMass_X_ft");
		csv.WriteField("CentOfMass_Y_ft");
		csv.WriteField("CentOfMass_Z_ft");
		csv.WriteField("InertiaTensor_X");
		csv.WriteField("InertiaTensor_Y");
		csv.WriteField("InertiaTensor_Z");
		csv.WriteField("InertiaTensorRot_X");
		csv.WriteField("InertiaTensorRot_Y");
		csv.WriteField("InertiaTensorRot_Z");
		csv.WriteField("InertiaTensorRot_W");
		csv.WriteField("NumCollisionEvents");
		csv.WriteField("WheelWeight_lb");
		csv.WriteField("WheelDamping");
		csv.WriteField("UpdateMethod");
		csv.WriteField("PhysicsTimestep");
		csv.WriteField("WheelSubsteps");
		csv.WriteField("TimestepsPerFrame");
		csv.WriteField("Errors");
		csv.NextRecord();
	}

	private void WriteSessionLogEntry(CsvWriter csv)
	{
		float wheelRadius = _FLWheel.radius;
		if (_FRWheel.radius != wheelRadius ||
			_RLWheel.radius != wheelRadius ||
			_RRWheel.radius != wheelRadius)
		{
			wheelRadius = -1;
			_error += " Wheel radii don't match; ";
		}

		if (_bodyCollider.transform.localRotation != Quaternion.identity)
		{
			_error += " Collider transform is rotated!; ";
		}

		Vector3 machineCenterOffset = _bodyCollider.center + _bodyCollider.transform.localPosition;
		Vector3 machineSize = _bodyCollider.size;

		Vector3 proxCenterOffset = Vector3.zero;
		Bounds proxYellowBounds = new Bounds();
		Bounds proxRedBounds = new Bounds();

		if (_boxProx != null)
		{
			proxCenterOffset = _boxProx.CenterOffset;
			proxYellowBounds = _boxProx.YellowZoneBounds;
			proxRedBounds = _boxProx.RedZoneBounds;
		}
		else
		{
			_error += " No Box prox system!; ";
		}

		float machineFrontDist = (machineSize.z / 2.0f) + machineCenterOffset.z;

		float yellowFrontDist = proxCenterOffset.z + proxYellowBounds.center.z + proxYellowBounds.extents.z;
		float yellowFrontSize = yellowFrontDist - machineFrontDist;

		float redFrontDist = proxCenterOffset.z + proxRedBounds.center.z + proxRedBounds.extents.z;
		float redFrontSize = redFrontDist - machineFrontDist;

		Vector3 startPos = _scene.ShuttleCarSpawn.transform.position;
		Vector3 finalPos = _vehRigidbody.position;
		float horizontalDeviation = startPos.x - finalPos.x;

		float avgYellowSlip;
		float avgRedSlip;

		if (_totalYellowSlipSamples > 0)
		{
			avgYellowSlip = (float)(_totalYellowSlip / (double)_totalYellowSlipSamples);
		}
		else
		{
			avgYellowSlip = -1;
		}

		if (_totalRedSlipSamples > 0)
		{
			avgRedSlip = (float)(_totalRedSlip / (double)_totalRedSlipSamples);
		}
		else
		{
			avgRedSlip = -1;
		}


		csv.WriteField(_trialNum);
		csv.WriteField(_block);
		csv.WriteField(_trialSettings.SceneName);
		csv.WriteField(_trialSettings.MachinePrefabName);
		//csv.WriteField(_carController.MachineType);
		csv.WriteField(_trialSettings.ProxConfig);
		csv.WriteField(_trialComplete ? "TRUE" : "FALSE");
		csv.WriteField(_frontPlaneDist);
		csv.WriteField(_distanceToVehicle);
		csv.WriteField(_minFrontPlaneDist);
		csv.WriteField(_minDistanceToVehicle);
		csv.WriteField(horizontalDeviation * ConvMToFt);
		csv.WriteField(_trialSettings.InitialSpeedMultiplier);
		csv.WriteField(_trialSettings.BrakeTorqueMultiplier);
		csv.WriteField(_initialSpeedFPS);
		csv.WriteField(_brakeTorqueNM * ConvNMToFTLB);
		csv.WriteField(_accelTorqueNM * ConvNMToFTLB);
		csv.WriteField(_trialSettings.LagTime);
		csv.WriteField(_trialSettings.AsymptoteForce);
		csv.WriteField(_trialSettings.AsymptoteSlip);
		csv.WriteField(_trialSettings.ExtremumForce);
		csv.WriteField(_trialSettings.ExtremumSlip);
		csv.WriteField(_trialSettings.Slope);
		csv.WriteField(_vehRigidbody.mass * ConvKGToLB);
		csv.WriteField(_trialSettings.LoadLevel);
		csv.WriteField(_trialSettings.BrakingModel);
		csv.WriteField(_trialSettings.MWCLocation);
		csv.WriteField(_achievedSteadyState);
		// csv.WriteField(_initialSpeedSSDuration);
		// csv.WriteField(_yellowZoneBrakingDuration);
		// csv.WriteField(_yellowZoneSteadyStateDuration);
		csv.WriteField(_initialSteadyDuration);
		csv.WriteField(_yellowDuration);
		csv.WriteField(_yellowSteadyDuration);
		csv.WriteField(_yellowBrakingDuration);
		csv.WriteField(avgYellowSlip);

		csv.WriteField(_redDuration);
		csv.WriteField(_redBrakingDuration);
		csv.WriteField(avgRedSlip);

		csv.WriteField(_elapsedTime);
		csv.WriteField(_numSamples);
		csv.WriteField(Time.frameCount - _startFrame);
		csv.WriteField(_MaxDeltaTime);

		csv.WriteField(_yellowZoneDistance * ConvMToFt);
		csv.WriteField(_redZoneDistance * ConvMToFt);
		csv.WriteField((_yellowZoneDistance + _redZoneDistance) * ConvMToFt);



		csv.WriteField(machineSize.x * ConvMToIN);
		csv.WriteField(_wheelBase * ConvMToIN);
		csv.WriteField(_wheelFrontTrack * ConvMToIN);
		csv.WriteField(_wheelRearTrack * ConvMToIN);
		csv.WriteField(wheelRadius * ConvMToIN);
		csv.WriteField(_chassisGroundClearance * ConvMToIN);
		csv.WriteField(machineSize.y * ConvMToIN);
		csv.WriteField(machineSize.z * ConvMToIN);
		csv.WriteField(_backToWheel * ConvMToIN);
		csv.WriteField(_frontToWheel * ConvMToIN);
		csv.WriteField(_chassisCenterHeight * ConvMToIN);

		// csv.WriteField(machineSize.x * ConvMToFt);
		// csv.WriteField(machineSize.y * ConvMToFt);
		// csv.WriteField(machineSize.z * ConvMToFt);

		csv.WriteField(machineCenterOffset.x * ConvMToFt);
		csv.WriteField(machineCenterOffset.y * ConvMToFt);
		csv.WriteField(machineCenterOffset.z * ConvMToFt);

		csv.WriteField(yellowFrontDist * ConvMToFt);
		csv.WriteField(yellowFrontSize * ConvMToFt);
		csv.WriteField(redFrontDist * ConvMToFt);
		csv.WriteField(redFrontSize * ConvMToFt);

		csv.WriteField(_vehRigidbody.centerOfMass.x * ConvMToFt);
		csv.WriteField(_vehRigidbody.centerOfMass.y * ConvMToFt);
		csv.WriteField(_vehRigidbody.centerOfMass.z * ConvMToFt);

		csv.WriteField(_vehRigidbody.inertiaTensor.x * ConvMToFt);
		csv.WriteField(_vehRigidbody.inertiaTensor.y * ConvMToFt);
		csv.WriteField(_vehRigidbody.inertiaTensor.z * ConvMToFt);

		csv.WriteField(_vehRigidbody.inertiaTensorRotation.x);
		csv.WriteField(_vehRigidbody.inertiaTensorRotation.y);
		csv.WriteField(_vehRigidbody.inertiaTensorRotation.z);
		csv.WriteField(_vehRigidbody.inertiaTensorRotation.w);

		csv.WriteField(_collisionLog.EnterCount + _collisionLog.ExitCount + _collisionLog.StayCount);

		csv.WriteField(_wheelWeightLb);
		csv.WriteField(_wheelDampingRate);

		csv.WriteField(_trialSettings.ExperimentUpdateMethod.ToString());
		csv.WriteField(_trialSettings.ManualUpdateTimestep);
		csv.WriteField(_trialSettings.WheelColliderSubsteps);
		csv.WriteField(_trialSettings.ManualUpdatesPerFrame);


		csv.WriteField(_error);
		csv.NextRecord();
	}

	private void SetMWCLocation()
	{
		ImpactProxEffect proxEffect = null;

		_scene.MWCCenter.gameObject.SetActive(false);
		_scene.MWCLeft.gameObject.SetActive(false);
		_scene.MWCRight.gameObject.SetActive(false);

		switch (_trialSettings.MWCLocation.ToLower())
		{
			case "center":
				_miner = _scene.MWCCenter;
				break;
			case "left":
				_miner = _scene.MWCLeft;
				break;
			case "right":
				_miner = _scene.MWCRight;
				break;
			default:
				throw new System.Exception("Invalid MWC Location in config");
		}

		_miner.gameObject.SetActive(true);
		proxEffect = _miner.GetComponent<ImpactProxEffect>();

		if (proxEffect != null)
			proxEffect.Reset();
	}

	private void SpawnShuttleCar()
	{
		_brakingStarted = false;
		SetMWCLocation();

		var prefab = FindMachinePrefab(_trialSettings.MachinePrefabName);
		if (prefab == null)
			throw new System.Exception($"Couldn't find machine prefab named {_trialSettings.MachinePrefabName}");

		_carReference = Instantiate(prefab, _scene.ShuttleCarSpawn.transform);


		BoxCollider[] boxes = _carReference.GetComponentsInChildren<BoxCollider>();

		if (boxes == null || boxes.Length != 1)
			throw new System.Exception($"Too many or too few box colliders on prefab {_trialSettings.MachinePrefabName}");

		_bodyCollider = boxes[0];

		_carReference.transform.localPosition = Vector3.zero;
		_carReference.transform.localRotation = Quaternion.identity;
		_vehRigidbody = _carReference.GetComponent<Rigidbody>();
		_carReference.GetComponent<Rigidbody>().velocity = Vector3.zero;
		_frontOfShuttleCar = _carReference.transform.Find("FrontOfCarTransform");
		Vector3 arbitraryPointForward = _carReference.transform.position;
		arbitraryPointForward.z = arbitraryPointForward.z + 12;//Setting well forward of the machine based on center point
		_frontOfShuttleCar.transform.position = _bodyCollider.ClosestPoint(arbitraryPointForward);

		_vehRigidbody.velocity = Vector3.zero;
		_vehRigidbody.angularVelocity = Vector3.zero;
		_vehRigidbody.ResetCenterOfMass();
		//_VehRigidbody.constraints = RigidbodyConstraints.FreezeRotationY;

		_collisionLog = _carReference.GetComponent<CollisionLog>();
		if (_collisionLog == null)
			_collisionLog = _carReference.AddComponent<CollisionLog>();

		#region CodeOnlyForDemos
		if (_scene.ChaseCamera != null)
		{
			_scene.ChaseCamera.transform.localPosition = Vector3.zero;
			_scene.ChaseCamera.transform.localRotation = Quaternion.identity;
			_scene.ChaseCamera.transform.SetParent(_carReference.transform, false);

			var driverPosTransform = _carReference.transform.Find("DriverCamPos");
			if (driverPosTransform != null)
			{
				_scene.ChaseCamera.transform.position = driverPosTransform.position;
				_scene.ChaseCamera.transform.rotation = driverPosTransform.rotation;
			}
		}
		#endregion

		//_carController = _carReference.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();
		//_carController.BrakePercentOverride = 0;
		//_carController.SetMaxSpeed(0);
		//_carController.ResetDeltas();//The integral term and dt can get messed up here for some reason on a restart unless they get cleared

		//find machine constants
		//_machineConstants = FindMachineConstants(_carController.MachineType);

		_initialSpeedFPS = _machineConstants.InitialSpeed * _trialSettings.InitialSpeedMultiplier;
		_brakeTorqueNM = _machineConstants.BrakeTorqueFtLb * _trialSettings.BrakeTorqueMultiplier * ConvFTLBToNM;
		_accelTorqueNM = _machineConstants.AccelTorqueFtLb * ConvFTLBToNM;

		//if (_machineConstants == null)
		//	throw new System.Exception($"Couldn't find machine constants for machine type {_carController.MachineType}");


		_wheelWeightLb = _machineConstants.WheelWeight;
		if (_trialSettings.WheelWeightOverride > 0)
			_wheelWeightLb = _trialSettings.WheelWeightOverride;

		_wheelDampingRate = _machineConstants.WheelDampingRate;

		_Wheels = _carReference.GetComponentsInChildren<WheelCollider>();
		foreach (WheelCollider wheel in _Wheels)
		{
			//Debug.Log(wheel.name);
			switch (wheel.name)
			{
				case "FL_WheelCol":
					_FLWheel = wheel;
					break;
				case "FR_WheelCol":
					_FRWheel = wheel;
					break;
				case "RL_WheelCol":
					_RLWheel = wheel;
					break;
				case "RR_WheelCol":
					_RRWheel = wheel;
					break;
			}

			Vector3 wheelPos = wheel.transform.localPosition;
			wheelPos.y = wheel.radius;
			wheel.transform.localPosition = wheelPos;

			wheel.suspensionDistance = 0.1f;

			// float distToGround = Mathf.Abs(wheel.transform.position.y - wheel.radius);
			// if (distToGround > 0.0001)
			// 	throw new System.Exception($"Bad wheel pos/radius on {_trialSettings.MachinePrefabName} wheel {wheel.name} dist to ground {distToGround:F2}");

			WheelFrictionCurve friction = wheel.forwardFriction;
			friction.extremumValue = _trialSettings.ExtremumForce;
			friction.asymptoteValue = _trialSettings.AsymptoteForce;
			friction.extremumSlip = _trialSettings.ExtremumSlip;
			friction.asymptoteSlip = _trialSettings.AsymptoteSlip;
			friction.stiffness = _trialSettings.FrictionStiffness;

			wheel.forwardFriction = friction;
			wheel.mass = _wheelWeightLb * ConvLBToKG;
			wheel.wheelDampingRate = _wheelDampingRate;
			wheel.ConfigureVehicleSubsteps(100, _trialSettings.WheelColliderSubsteps, _trialSettings.WheelColliderSubsteps);
		}

		//compute machine dimensions in machine space
		Vector3 flWheel = _carReference.transform.InverseTransformPoint(_FLWheel.transform.position);
		Vector3 frWheel = _carReference.transform.InverseTransformPoint(_FRWheel.transform.position);
		Vector3 rlWheel = _carReference.transform.InverseTransformPoint(_RLWheel.transform.position);
		Vector3 rrWheel = _carReference.transform.InverseTransformPoint(_RRWheel.transform.position);

		//verify that the machine's origin is the ground plane, e.g. the plane where the wheels would touch the ground
		if (Mathf.Abs(flWheel.y - _FLWheel.radius) > 0.001f)
			throw new System.Exception($"{_trialSettings.MachinePrefabName}'s origin is not on the ground plane");

		Vector3 machineCenter = _carReference.transform.InverseTransformPoint(_bodyCollider.transform.TransformPoint(_bodyCollider.center));
		_chassisCenterHeight = machineCenter.y;
		_chassisGroundClearance = machineCenter.y - (_bodyCollider.size.y / 2);

		_localMachineFrontDist = (_bodyCollider.size.z / 2.0f) + machineCenter.z;
		float machineBack = machineCenter.z - (_bodyCollider.size.z / 2.0f);

		if (flWheel.z != frWheel.z || rlWheel.z != rrWheel.z)
		{
			throw new System.Exception($"Wheel positions aren't aligned on {_trialSettings.MachinePrefabName}");
		}

		_wheelBase = Mathf.Abs(flWheel.z - rlWheel.z);
		_wheelFrontTrack = Mathf.Abs(flWheel.x - frWheel.x);
		_wheelRearTrack = Mathf.Abs(rlWheel.x - rrWheel.x);
		_frontToWheel = Mathf.Abs(flWheel.z - _localMachineFrontDist);
		_backToWheel = Mathf.Abs(rlWheel.z - machineBack);


		if (_Wheels == null || _Wheels.Length != 4 || _FLWheel == null || _FRWheel == null || _RLWheel == null || _RRWheel == null)
		{
			throw new System.Exception($"Machine has the wrong number of wheels! {_carReference.name}");
		}

		//set the total brake torque (4 wheels) in NM
		//_carController.SetBrakeTorque(_brakeTorqueNM * 4);
		//_carController.SetFullMotorTorque(_accelTorqueNM * 4);
		//_carController.SetTractionControl(0.4f, (_accelTorqueNM * 4) * 0.02f);

		_machineWeightLb = _machineConstants.EmptyWeight + _machineConstants.LoadCapacity * _trialSettings.LoadLevel;
		//_carController.GetComponent<Rigidbody>().mass = _machineWeightLb * ConvLBToKG;

		//configure the prox system
		_proxSystem = _carReference.GetComponent<ProxSystemController>();
		if (_trialSettings.ExperimentUpdateMethod == Experiment.UpdateMethod.Manual)
			_proxSystem.StateUpdateMethod = ProxSystemController.UpdateMethod.Manual;
		_boxProx = _proxSystem.FallbackProxSystem as BoxProxSystem;

		if (_boxProx != null)
		{
			//throw new System.Exception($"Proximty system configured incorrectly on {_trialSettings.MachinePrefabName}");

			_boxProx.SetMachineOffsetsYellow(_trialSettings.ProxYFront, _trialSettings.ProxYBack, _trialSettings.ProxYLeft, _trialSettings.ProxYRight);
			_boxProx.SetMachineOffsetsRed(_trialSettings.ProxRFront, _trialSettings.ProxRBack, _trialSettings.ProxRLeft, _trialSettings.ProxRRight);
		}

		ProxSystem.VisOptions opts = new ProxSystem.VisOptions();
		opts.ShowRedShell = true;
		opts.ShowYellowShell = !_ignoreYellowZone;

		_proxSystem.EnableZoneVisualization(opts);

		if (_proxSystem.FallbackProxSystem != null)
		{
			_proxSystem.FallbackProxSystem.EnableZoneVisualization(opts);
		}

		_brakesEngaged = false;


		_vehState = VehicleState.BrakeHold;
		_MaxDeltaTime = 0;

		for (int i = 0; i < 2; i++)
		{
			_PriorVelocities[i] = -1;
		}
	}

	private void UpdateDistanceMeasurements()
	{
		Vector3 localShuttleCarPos = _scene.Mine.transform.InverseTransformPoint(_vehRigidbody.position);
		Vector3 localMinerPos = _scene.Mine.transform.InverseTransformPoint(_miner.position);

		_frontPlaneDist = localMinerPos.z - localShuttleCarPos.z - _localMachineFrontDist;
		_frontPlaneDist *= ConvMToFt;

		//Vector3 localSCFront = _scene.Mine.transform.InverseTransformPoint(_frontOfShuttleCar.transform.position);
		//dist2 = (localMinerPos.z - localSCFront.z) * ConvMToFt;

		Vector3 shuttleCarPos = _frontOfShuttleCar.position;
		Vector3 minerPos = _miner.position;

		_distanceToVehicle = Vector3.Distance(minerPos, _bodyCollider.ClosestPoint(minerPos)) * ConvMToFt;

		//_frontPlaneDist = (_carReference.transform.position - _vehRigidbody.position).magnitude;
		// shuttleCarPos.x = minerPos.x;
		// float DistanceToPerson = 0; // Vector3.Distance(shuttleCarPos, minerPos);

		// //compute distance to person as distance along the forward vector the the shuttle car spawner transform
		// Vector3 distanceToPersonVector = minerPos - shuttleCarPos;
		// DistanceToPerson = Vector3.Dot(distanceToPersonVector, _scene.ShuttleCarSpawn.transform.forward);
		// _forwardDistanceToPerson = (minerPos.z - _frontOfShuttleCar.position.z) / Mathf.Cos(_scene.Mine.transform.rotation.eulerAngles.x * Mathf.Deg2Rad);

		// DistanceToPerson = DistanceToPerson * 3.28084f;
		// _forwardDistanceToPerson = _forwardDistanceToPerson * 3.28084f;


		if (_frontPlaneDist < _minFrontPlaneDist)
		{
			_minFrontPlaneDist = _frontPlaneDist;
		}
		if (_distanceToVehicle < _minDistanceToVehicle)
		{
			_minDistanceToVehicle = _distanceToVehicle;
		}

		//float StopDistance = Vector3.Distance(_stopPosition, _carReference.transform.position);
		//StopDistance = StopDistance * 3.28084f;//meters to feet

		// string distanceToPersonText = null;
		// if (DistanceToPerson >= 0)
		// 	distanceToPersonText = string.Format("{0,6:F2}", DistanceToPerson);
		// else
		// 	distanceToPersonText = "<color=#ff0000ff>COLLISION</color>";

	}

	private void ChangeVehicleState(VehicleState toState)
	{
		VehicleState fromState = _vehState;

		if (fromState == toState)
			throw new System.Exception($"Tried to transition to the same state {fromState}");

		OnExitedVehicleState(fromState);
		OnEnteredVehicleState(toState);

		_vehState = toState;
	}

	private void OnEnteredVehicleState(VehicleState state)
	{
		switch (state)
		{
			case VehicleState.SpeedUp:
				break;

			case VehicleState.InitialSteady:
				_achievedSteadyState = true;
				//_steadyStateStartTime = GetElapsedTime();
				_timeEnteredInitialSteady = GetElapsedTime();
				break;

			case VehicleState.Yellow:
				_timeEnteredYellow = GetElapsedTime();
				break;

			case VehicleState.YellowBraking:
				//_yellowZoneBrakingStartTime = GetElapsedTime();
				_timeEnteredYellowBraking = GetElapsedTime();
				break;

			case VehicleState.YellowSteady:
				//_yellowZoneSteadyStartTime = GetElapsedTime();
				_timeEnteredYellowSteady = GetElapsedTime();
				break;

			case VehicleState.Red:
				_timeEnteredRed = GetElapsedTime();
				break;

			case VehicleState.RedBraking:
				_timeEnteredRedBraking = GetElapsedTime();
				//_redZoneBrakingStartTime = GetElapsedTime();
				break;

			case VehicleState.Stopped:
				_redZoneDistance = Vector3.Distance(_redTriggerPosition, _vehRigidbody.position);
				_timeStopped = GetElapsedTime();
				break;

		}
	}

	private void OnExitedVehicleState(VehicleState state)
	{
		switch (state)
		{
			case VehicleState.SpeedUp:
				break;

			case VehicleState.InitialSteady:
				_initialSteadyDuration = GetElapsedTime() - _timeEnteredInitialSteady;
				if (_initialSteadyDuration < SteadyStateTime)
				{
					_error += "Steady state less than two seconds";
				}
				break;

			case VehicleState.Yellow:
				_yellowDuration = GetElapsedTime() - _timeEnteredYellow;
				break;

			case VehicleState.YellowBraking:
				_yellowBrakingDuration = GetElapsedTime() - _timeEnteredYellowBraking;
				break;

			case VehicleState.YellowSteady:
				_yellowSteadyDuration = GetElapsedTime() - _timeEnteredYellowSteady;

				if (_brakingOcurredInYellowSteady)
					_error += "Yellow Braking After Steady State; ";

				break;

			case VehicleState.Red:
				_redDuration = GetElapsedTime() - _timeEnteredRed;
				break;

			case VehicleState.RedBraking:
				_redBrakingDuration = GetElapsedTime() - _timeEnteredRedBraking;
				break;
		}
	}

	private void OnEnteredYellowZone()
	{
		_yellowTriggerPosition = _vehRigidbody.position;
		_yellowTriggerTime = GetElapsedTime();

		if (_trialSettings.LagTime <= 0)
			ChangeVehicleState(VehicleState.YellowBraking);
		else
			ChangeVehicleState(VehicleState.Yellow);
	}

	private void OnEnteredRedZone()
	{
		_redTriggerPosition = _vehRigidbody.position;
		_yellowZoneDistance = Vector3.Distance(_yellowTriggerPosition, _redTriggerPosition);
		_redTriggerTime = GetElapsedTime();

		if (_trialSettings.LagTime <= 0)
			ChangeVehicleState(VehicleState.RedBraking);
		else
			ChangeVehicleState(VehicleState.Red);
	}

	// Check if the vehicle state needs to be changed (state transitions & state transition errors only)
	private bool UpdateVehicleState(float deltaTime, float elapsedTime)
	{
		switch (_vehState)
		{
			case VehicleState.BrakeHold:
				if (elapsedTime > 2.0f)
				{
					ChangeVehicleState(VehicleState.SpeedUp);
				}
				break;

			case VehicleState.SpeedUp:
				//check if we have achieved the target velocity
				if (_vehRigidbody.velocity.magnitude * ConvMSToFPS >= (_initialSpeedFPS - (_initialSpeedFPS * SteadyStateTolerance)))
				{
					ChangeVehicleState(VehicleState.InitialSteady);
				}
				else if (!_ignoreYellowZone && _proxSystem.ActiveProxZone == ProxZone.YellowZone)
				{
					_error += "Never Achieved Steady State; ";
					OnEnteredYellowZone();
				}
				else if (_proxSystem.ActiveProxZone == ProxZone.RedZone)
				{
					_error += "Never Achieved Steady State; ";
					OnEnteredRedZone();
				}

				break;

			case VehicleState.InitialSteady:
				if (!_ignoreYellowZone && _proxSystem.ActiveProxZone == ProxZone.YellowZone)
				{
					OnEnteredYellowZone();
				}
				else if (_proxSystem.ActiveProxZone == ProxZone.RedZone)
				{
					if (!_ignoreYellowZone)
						_error += "Never entered yellow zone; ";

					OnEnteredRedZone();
				}
				break;

			case VehicleState.Yellow:
				float timeInYellow = elapsedTime - _yellowTriggerTime;

				if (_proxSystem.ActiveProxZone == ProxZone.RedZone)
				{
					_error += "Never started Yellow Zone Braking; ";
					OnEnteredRedZone();
				}
				else if (timeInYellow >= _trialSettings.LagTime)
				{
					ChangeVehicleState(VehicleState.YellowBraking);
				}
				break;

			case VehicleState.YellowBraking:

				if (_proxSystem.ActiveProxZone == ProxZone.RedZone)
				{
					_error += "Never achieved Yellow Zone Steady State; ";
					OnEnteredRedZone();
				}
				else if (IsVelocitySteady(YellowZoneSpeedLimit, SteadyStateTolerance))
				{
					ChangeVehicleState(VehicleState.YellowSteady);
				}
				break;

			case VehicleState.YellowSteady:
				if (_proxSystem.ActiveProxZone == ProxZone.RedZone)
				{
					OnEnteredRedZone();
				}
				// if (_VehRigidbody.velocity.magnitude > (YellowZoneSpeedLimit + YellowZoneSpeedLimit * SteadyStateTolerance))
				// {
				// 	_error += "Yellow Braking After Steady State; ";
				// 	ChangeVehicleState(VehicleState.YellowBraking);
				// }
				break;

			case VehicleState.Red:
				float timeInRed = elapsedTime - _redTriggerTime;
				if (timeInRed >= _trialSettings.LagTime)
				{
					ChangeVehicleState(VehicleState.RedBraking);
				}
				break;

			case VehicleState.RedBraking:
				if (_vehRigidbody.velocity.magnitude < 0.008f) //Approximation (uncertainty in the unity model can potentially mean it's never *actually* zero)
				{
					ChangeVehicleState(VehicleState.Stopped);
				}
				break;

			case VehicleState.Stopped:
				float timeStopped = elapsedTime - _timeStopped;
				if (timeStopped >= 2.0f)
					_trialComplete = true;
				break;

		}

		return true;
	}

	// process anything that needs to happen during the state & update the motor control state
	private void ProcessVehicleState(float deltaTime, float elapsedTime)
	{
		switch (_vehState)
		{
			case VehicleState.BrakeHold:
				_motorState = MotorControlState.Stopped;
				break;

			case VehicleState.SpeedUp:
				_motorState = MotorControlState.NormalForward;
				ForceAlignMachine();
				break;

			case VehicleState.InitialSteady:
			case VehicleState.Yellow:
				_motorState = MotorControlState.NormalForward;
				break;

			case VehicleState.YellowBraking:
				_motorState = MotorControlState.YellowBraking;
				break;

			case VehicleState.YellowSteady:
				//this case is unusual as the system is in the lag time between hitting prox
				//red zone and engaging the red zone braks, motor control could either be in
				//yellow steady or yellow braking depending on vehicle speed
				if (IsVelocitySteady(YellowZoneSpeedLimit, SteadyStateTolerance))
				{
					_motorState = MotorControlState.YellowSteady;
				}
				else
				{
					_brakingOcurredInYellowSteady = true;
					_motorState = MotorControlState.YellowBraking;
				}
				break;

			case VehicleState.Red:
				if (_ignoreYellowZone)
				{
					//in one stage braking, the lag time behavior is the same as normal forward
					_motorState = MotorControlState.NormalForward;
				}
				else
				{
					//this case is unusual as the system is in the lag time between hitting prox
					//red zone and engaging the red zone braks, motor control could either be in
					//yellow steady or yellow braking depending on vehicle speed
					if (IsVelocitySteady(YellowZoneSpeedLimit, SteadyStateTolerance))
					{
						_motorState = MotorControlState.YellowSteady;
					}
					else
					{
						_brakingOcurredInYellowSteady = true;
						_motorState = MotorControlState.YellowBraking;
					}
				}
				break;

			case VehicleState.RedBraking:
				_motorState = MotorControlState.Red;
				break;

			case VehicleState.Stopped:
				_motorState = MotorControlState.Stopped;
				break;

		}
	}

	private bool IsVelocitySteady(float speedLimt, float tolerance)
	{
		for (int i = 0; i < _PriorVelocities.Length; i++)
		{
			if (_PriorVelocities[i] > (speedLimt + (speedLimt * tolerance)))
			{
				return false;
			}
		}

		return true;
	}

	//update the vehicle drive settings based on the current motor control state
	private void UpdateMotorControl(MotorControlState state, float deltaTime, float elapsedTime)
	{
		switch (state)
		{
			case MotorControlState.NormalForward:
				//_carController.BrakePercentOverride = 0;
				//_carController.SetMaxSpeed(_initialSpeedFPS * ConvFPSToMPH);//To MPH friendly for unity car controller
				//_carController.Move(0, 1, 0, 0, deltaTime);
				break;

			case MotorControlState.YellowBraking:
				//_carController.SetMaxSpeed(YellowZoneSpeedLimitMPH);
				//_carController.BrakePercentOverride = 0.5f;
				//_carController.Move(0, 0, 1, 0, deltaTime);

				_totalYellowSlip += Mathf.Abs(_currentForwardSlip);
				_totalYellowSlipSamples++;
				break;

			case MotorControlState.YellowSteady:
				//_carController.SetMaxSpeed(YellowZoneSpeedLimitMPH);
				//_carController.BrakePercentOverride = 0;
				//_carController.Move(0, 1, 0, 0, deltaTime);

				_totalYellowSlip += Mathf.Abs(_currentForwardSlip);
				_totalYellowSlipSamples++;
				break;

			case MotorControlState.Red:
				//_carController.SetMaxSpeed(0);
				//_carController.BrakePercentOverride = 1.0f;
				//_carController.Move(0, 0, 1, 0, deltaTime);

				_totalRedSlip += Mathf.Abs(_currentForwardSlip);
				_totalRedSlipSamples++;
				break;

			case MotorControlState.Stopped:
				//_carController.SetMaxSpeed(0);
				//_carController.BrakePercentOverride = 1.0f;
				//_carController.Move(0, 0, 1, 0, deltaTime);
				break;

			default:
				throw new System.Exception($"Invalid motor control state specified {state.ToString()}");
		}
	}

	public override bool UpdateExperiment(float deltaTime, float elapsedTime)
	{
		//when this is called, physics has been updated with a timstep of deltaTime, 
		//and elapsedTime has already elapsed (in the physics engine) from the trial start

		_proxSystem.UpdateProxState();

		_numSamples++;
		_elapsedTime = elapsedTime;

		if (deltaTime > _MaxDeltaTime)
		{
			_MaxDeltaTime = deltaTime;
		}

		if (!ComputeWheelSlip(out _currentForwardSlip, out _currentSideSlip))
		{
			_currentForwardSlip = -100;
			_currentSideSlip = -100;
		}

		UpdateDistanceMeasurements();

		float velocity = _carReference.transform.InverseTransformDirection(_carReference.GetComponent<Rigidbody>().velocity).z * ConvMSToFPS; //Convert vehicle velocity to feet per second rather than meters per second
		for (int i = _PriorVelocities.Length - 1; i > 0; i--)
		{
			_PriorVelocities[i] = _PriorVelocities[i - 1];
		}
		_PriorVelocities[0] = _vehRigidbody.velocity.magnitude;


		if (GetElapsedTime() >= 90) //Trial automatically restarts after 25 seconds have passed
		{
			return false;
		}

		if (!UpdateVehicleState(deltaTime, elapsedTime))
			return false;

		ProcessVehicleState(deltaTime, elapsedTime);
		UpdateMotorControl(_motorState, deltaTime, elapsedTime);

		// if (GetElapsedTime() <= 1.5f)
		// {
		// 	//_carReference.GetComponent<Rigidbody>().velocity = Vector3.zero;
		// 	//CarReference.GetComponent<Rigidbody>().velocity = (CarReference.transform.forward) * (CarRefController.MaxSpeed * 0.44704f);//Float value is mph to met/s

		// 	_carController.SetMaxSpeed(0);
		// 	_carController.BrakePercentOverride = 1.0f;
		// 	_carController.Move(0, 0, -1, 0, deltaTime);
		// }
		// else
		// {
		// 	if (!UpdateVehicleState(deltaTime, elapsedTime))
		// 		return false;

		// 	ProcessVehicleState(deltaTime, elapsedTime);
		// 	UpdateMotorControl(_motorState, deltaTime, elapsedTime);
		// }

		if (EnableDetailedLogs)
		{
			WriteLogEntry();
		}

		return !_trialComplete;
	}

	// public override bool UpdateExperiment(float deltaTime, float elapsedTime)
	// {
	// 	//when this is called, physics has been updated with a timstep of deltaTime, 
	// 	//and elapsedTime has already elapsed (in the physics engine) from the trial start

	// 	_proxSystem.UpdateProxState();

	// 	_numSamples++;
	// 	_elapsedTime = elapsedTime;

	// 	if (deltaTime > _MaxDeltaTime)
	// 	{
	// 		_MaxDeltaTime = deltaTime;
	// 	}

	// 	UpdateDistanceMeasurements();

	// 	float velocity = _carReference.transform.InverseTransformDirection(_carReference.GetComponent<Rigidbody>().velocity).z * ConvMSToFPS; //Convert vehicle velocity to feet per second rather than meters per second
	// 	for (int i = _PriorVelocities.Length - 1; i > 0; i--)
	// 	{
	// 		_PriorVelocities[i] = _PriorVelocities[i - 1];
	// 	}
	// 	_PriorVelocities[0] = _VehRigidbody.velocity.magnitude;

	// 	if (EnableDetailedLogs)
	// 	{
	// 		WriteLogEntry();
	// 	}

	// 	if (GetElapsedTime() >= 90) //Trial automatically restarts after 25 seconds have passed
	// 	{
	// 		return false;
	// 	}

	// 	if (GetElapsedTime() <= 2)
	// 	{
	// 		_carReference.GetComponent<Rigidbody>().velocity = Vector3.zero;
	// 		//CarReference.GetComponent<Rigidbody>().velocity = (CarReference.transform.forward) * (CarRefController.MaxSpeed * 0.44704f);//Float value is mph to met/s
	// 		_proxSystem.EnableZoneVisualization(new global::ProxSystem.VisOptions(true, true));
	// 		_carController.SetMaxSpeed(0);
	// 		_carController.Move(0, 0, -1, 0, deltaTime);
	// 	}
	// 	if (GetElapsedTime() > 2)
	// 	{
	// 		//Vector3 eulerAngles = _carController.transform.eulerAngles;
	// 		// eulerAngles.y = 0;
	// 		// _carController.transform.rotation = Quaternion.Euler(eulerAngles);

	// 		switch (_vehState)
	// 		{
	// 			case VehicleState.SpeedUp:
	// 				_carController.SetMaxSpeed(_initialSpeedFPS * ConvFPSToMPH);//To MPH friendly for unity car controller
	// 				_carController.Move(0, 1, 0, 0, deltaTime);

	// 				// eulerAngles = _carController.transform.eulerAngles;
	// 				// eulerAngles.y = 0;
	// 				// _carController.transform.rotation = Quaternion.Euler(eulerAngles);


	// 				bool isSteadyState = false;
	// 				if (_VehRigidbody.velocity.magnitude * ConvMSToFPS >= (_initialSpeedFPS - (_initialSpeedFPS * SteadyStateTolerance)))
	// 				{
	// 					isSteadyState = true;
	// 				}

	// 				if (isSteadyState)//Steady state if our PID output is nearing zero. Less than 1%
	// 				{
	// 					_vehState = VehicleState.InitialSteady;
	// 					_steadyStateStartTime = (float)GetElapsedTime();
	// 				}

	// 				if (_proxSystem.ActiveProxZone == ProxZone.YellowZone)
	// 				{
	// 					_vehState = VehicleState.Yellow;
	// 					_yellowTriggerPosition = _carReference.transform.position;

	// 					_delay = GetElapsedTime() + _delayTime;
	// 					_error += "Never Achieved Steady State; ";
	// 					//_InitialSpeedSSDuration = (float)GetElapsedTime() - _SteadyStateStartTime;                                                               
	// 				}
	// 				//_PriorVelocities[0] = _PriorVelocities[1];
	// 				//_PriorVelocities[1] = _VehRigidbody.velocity.magnitude;
	// 				break;
	// 			case VehicleState.InitialSteady:

	// 				// eulerAngles = _carController.transform.eulerAngles;
	// 				// eulerAngles.y = 0;
	// 				// _carController.transform.rotation = Quaternion.Euler(eulerAngles);

	// 				_carController.Move(0, 1, 0, 0, deltaTime);
	// 				_achievedSteadyState = true;
	// 				if (_proxSystem.ActiveProxZone == ProxZone.YellowZone)
	// 				{
	// 					_vehState = VehicleState.Yellow;
	// 					_yellowTriggerPosition = _carReference.transform.position;

	// 					_delay = GetElapsedTime() + _delayTime;
	// 					_initialSpeedSSDuration = (float)GetElapsedTime() - _steadyStateStartTime;
	// 					if (_initialSpeedSSDuration < SteadyStateTime)
	// 					{
	// 						_error += "Steady state less than two seconds";
	// 					}
	// 				}
	// 				break;
	// 			case VehicleState.Yellow:
	// 				if (!_brakingStarted)
	// 				{
	// 					_brakingStarted = true;
	// 					_brakingStartPos = _carReference.transform.position;
	// 					//_brakingStartTime = Time.time;
	// 				}

	// 				if (!_ignoreYellowZone && GetElapsedTime() >= (_delay - deltaTime))//Ensure that the state transition behavior happens exactly at our delay time, addressing Will's comment
	// 				{
	// 					_vehState = VehicleState.YellowBraking;
	// 					//Moving these to be only in the Yellow Braking state, see above note about ensuring behaviors occur in proper location
	// 					//_YellowZoneBrakingStartTime = (float)GetElapsedTime();
	// 					//CarRefController.SetMaxSpeed(YellowZoneSpeedLimitMPH);
	// 					//CarRefController.IsYellowZone = true;
	// 					_checkTimeFlag = true;
	// 				}
	// 				else
	// 				{
	// 					_carController.Move(0, 1, 0, 0, deltaTime);
	// 				}

	// 				if (_proxSystem.ActiveProxZone == ProxZone.RedZone)
	// 				{
	// 					_stopPosition = _carReference.transform.position;
	// 					_redTriggerPosition = _carReference.transform.position;
	// 					_yellowZoneDistance = Vector3.Distance(_carReference.transform.position, _yellowTriggerPosition);

	// 					_vehState = VehicleState.Red;
	// 					_carController.Move(0, 1, 0, 0, deltaTime);
	// 					_delay = GetElapsedTime() + _delayTime;
	// 					if (!_ignoreYellowZone)
	// 					{
	// 						_error += "Never started Yellow Zone Braking; ";
	// 					}
	// 				}

	// 				break;
	// 			case VehicleState.YellowBraking:
	// 				_yellowBrakingEngaged = true;
	// 				_carController.IsYellowZone = true;
	// 				_carController.SetMaxSpeed(YellowZoneSpeedLimitMPH);
	// 				if (_checkTimeFlag)//Prevent replicating behavior, ensure that the yellow braking start time reflects this
	// 				{
	// 					_yellowZoneBrakingStartTime = (float)GetElapsedTime();
	// 					_checkTimeFlag = false;
	// 				}

	// 				bool isYellowSteady = true;
	// 				for (int i = 0; i < _PriorVelocities.Length; i++)
	// 				{
	// 					if (_PriorVelocities[i] > (YellowZoneSpeedLimit + (YellowZoneSpeedLimit * SteadyStateTolerance)))
	// 					{
	// 						isYellowSteady = false;
	// 					}
	// 				}
	// 				if (isYellowSteady)//OLD DEBUGGING METHOD: _VehRigidbody.velocity.magnitude <= YellowZoneSpeedLimit
	// 				{
	// 					_vehState = VehicleState.YellowSteady;
	// 					//CarRefController.SetMaxSpeed(YellowZoneSpeedLimitMPH);
	// 					_carController.Move(0, 1, 0, 0, deltaTime);
	// 					_yellowZoneBrakingDuration += ((float)GetElapsedTime() - _yellowZoneBrakingStartTime);
	// 					_yellowZoneSteadyStartTime = (float)GetElapsedTime();
	// 				}
	// 				else
	// 				{
	// 					_carController.Move(0, 0, -0.25f, 0, deltaTime);
	// 				}

	// 				if (_proxSystem.ActiveProxZone == ProxZone.RedZone)
	// 				{
	// 					_stopPosition = _carReference.transform.position;
	// 					_redTriggerPosition = _carReference.transform.position;
	// 					_yellowZoneDistance = Vector3.Distance(_carReference.transform.position, _yellowTriggerPosition);

	// 					_vehState = VehicleState.Red;
	// 					_carController.IsYellowZone = false;
	// 					_carController.Move(0, 1, 0, 0, deltaTime);
	// 					_delay = GetElapsedTime() + _delayTime;
	// 					_error += "Never achieved Yellow Zone Steady State; ";
	// 				}
	// 				//_PriorVelocities[0] = _PriorVelocities[1];
	// 				//_PriorVelocities[1] = _VehRigidbody.velocity.magnitude;
	// 				break;
	// 			case VehicleState.YellowSteady:
	// 				if (_proxSystem.ActiveProxZone == ProxZone.RedZone)
	// 				{
	// 					_stopPosition = _carReference.transform.position;
	// 					_redTriggerPosition = _carReference.transform.position;
	// 					_yellowZoneDistance = Vector3.Distance(_carReference.transform.position, _yellowTriggerPosition);

	// 					_vehState = VehicleState.Red;
	// 					_carController.Move(0, 1, 0, 0, deltaTime);
	// 					_delay = GetElapsedTime() + _delayTime;
	// 					_yellowZoneSteadyStateDuration = (float)GetElapsedTime() - _yellowZoneSteadyStartTime;
	// 					if (_yellowZoneSteadyStateDuration < SteadyStateTime)
	// 					{
	// 						_error += "Yellow zone steady state under 2 seconds; ";
	// 					}
	// 				}
	// 				if (_VehRigidbody.velocity.magnitude > (YellowZoneSpeedLimit + YellowZoneSpeedLimit * SteadyStateTolerance))
	// 				{
	// 					_vehState = VehicleState.YellowBraking;
	// 					_error += "Yellow Braking After Steady State; ";
	// 					_yellowZoneBrakingStartTime = (float)GetElapsedTime();
	// 					_yellowZoneSteadyStateDuration = (float)GetElapsedTime() - _yellowZoneSteadyStartTime;
	// 				}
	// 				_carController.IsYellowZone = false;
	// 				_carController.Move(0, 1, 0, 0, deltaTime);
	// 				break;
	// 			case VehicleState.Red:

	// 				if (GetElapsedTime() >= (_delay - deltaTime))//See comments on Yellow zone braking above
	// 				{
	// 					_vehState = VehicleState.RedBraking;

	// 					if (_yellowBrakingEngaged)
	// 					{
	// 						_yellowZoneBrakingDuration += (float)GetElapsedTime() - _yellowZoneBrakingStartTime;
	// 						_yellowZoneSteadyStateDuration = 0;
	// 						_error += "Never achieved yellow zone steady state.";//Needed to add an error catch here for the case of yellow braking behavior occuring during red zone delay
	// 					}
	// 					_checkTimeFlag = true;
	// 				}
	// 				else
	// 				{
	// 					if (_yellowBrakingEngaged)//For the condition of still performing yellow braking when entering the red zone (yet still in the delay time) 
	// 					{
	// 						bool isYellowSteady2 = true;
	// 						for (int i = 0; i < _PriorVelocities.Length; i++)
	// 						{
	// 							if (_PriorVelocities[i] > (YellowZoneSpeedLimit + (YellowZoneSpeedLimit * SteadyStateTolerance)))
	// 							{
	// 								isYellowSteady2 = false;
	// 							}
	// 						}
	// 						if (isYellowSteady2)
	// 						{
	// 							_carController.SetMaxSpeed(YellowZoneSpeedLimitMPH);
	// 							_carController.Move(0, 0, -0.25f, 0, deltaTime);
	// 							_carController.IsYellowZone = true;
	// 							_yellowBrakingEngaged = false;
	// 							_yellowZoneSteadyStateDuration = 0;
	// 							_yellowZoneBrakingDuration += (float)GetElapsedTime() - _yellowZoneBrakingStartTime;
	// 						}
	// 						else
	// 						{
	// 							_carController.Move(0, 1, 0, 0, deltaTime);
	// 							_carController.IsYellowZone = true;
	// 						}
	// 					}
	// 					else
	// 					{
	// 						_carController.Move(0, 1, 0, 0, deltaTime);
	// 					}
	// 				}
	// 				break;
	// 			case VehicleState.RedBraking:
	// 				_carController.IsYellowZone = false;
	// 				_carController.RedZoneOverride = true;
	// 				_carController.SetMaxSpeed(0);

	// 				if (_checkTimeFlag)
	// 				{
	// 					_redZoneBrakingStartTime = (float)GetElapsedTime();
	// 					_checkTimeFlag = false;
	// 				}

	// 				if (_VehRigidbody.velocity.magnitude < 0.008f) //Approximation (uncertainty in the unity model can potentially mean it's never *actually* zero)
	// 				{
	// 					_vehState = VehicleState.Stopped;
	// 					_steadyStateStartTime = (float)GetElapsedTime();
	// 					_redZoneBrakingDuration = (float)GetElapsedTime() - _redZoneBrakingStartTime;
	// 					_redZoneDistance = Vector3.Distance(_carReference.transform.position, _redTriggerPosition);
	// 					_carController.RedZoneOverride = true;
	// 					_carController.Move(0, 0, -1, 0, deltaTime);
	// 				}
	// 				else
	// 				{
	// 					_carController.RedZoneOverride = true;
	// 					_carController.Move(0, 0, -1, 0, deltaTime);
	// 				}
	// 				break;
	// 			case VehicleState.Stopped:
	// 				_carController.RedZoneOverride = true;
	// 				_carController.SetMaxSpeed(0);
	// 				if (GetElapsedTime() > (_steadyStateStartTime + SteadyStateTime))
	// 				{

	// 					_trialComplete = true;
	// 				}
	// 				_carController.IsYellowZone = false;
	// 				_carController.RedZoneOverride = true;
	// 				_carController.Move(0, 0, -1, 0, deltaTime);
	// 				break;
	// 			default:
	// 				break;
	// 		}

	// 	}
	// 	else
	// 	{
	// 		_carController.SetMaxSpeed(0);
	// 		_carController.Move(0, 0, -1, 0, deltaTime);
	// 	}

	// 	return !_trialComplete;
	// }

	public override void FinalizeExperiment()
	{
		//_trialEndTime = Time.time;

		Debug.Log($"Experiment trial {_trialNum} complete, dist to person: {_frontPlaneDist}");

		CloseLogFile();

		//write session log
		string sessionLogFile = Path.Combine(_manager.GetLogFolder(), $"{_session}.csv");
		if (!File.Exists(sessionLogFile))
		{
			using (var file = new StreamWriter(sessionLogFile))
			using (var csv = new CsvWriter(file, CultureInfo.InvariantCulture))
			{
				WriteSessionLogHeader(csv);
				WriteSessionLogEntry(csv);
			}
		}
		else
		{
			using (var file = new StreamWriter(sessionLogFile, true))
			using (var csv = new CsvWriter(file, CultureInfo.InvariantCulture))
			{
				WriteSessionLogEntry(csv);
			}
		}

		if (EnableDetailedLogs)
		{
			//write out trial settings
			string settingsFile = Path.Combine(_manager.GetLogFolder(), $"{_session}-t{_trialNum}-settings.yaml");
			var yamlWriter = ExperimentConfig.BuildSerializer();

			using (StreamWriter settingsWriter = new StreamWriter(settingsFile, false, System.Text.Encoding.UTF8))
			{
				yamlWriter.Serialize(settingsWriter, _trialSettings);
			}
		}


		//cleanup scene
		if (_scene.ChaseCamera != null)
		{
			//make sure the chase camera doesn't get destroyed
			_scene.ChaseCamera.transform.SetParent(null);
		}
		Destroy(_carReference);

		//reset physics & timescale to normal
		//Time.timeScale = 1.0f;
		//Physics.autoSimulation = true;
	}
}
