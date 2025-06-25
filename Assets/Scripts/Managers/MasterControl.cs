using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public enum LogMsgType
{
	Error = 0,
	Warning = 1,
	Information = 2
}

public enum ClientRole
{
	Unknown,
	Player,
	Researcher,
	MultiUser,
}

[System.Serializable]
public struct MapData
{
	public string SceneName;
	public string DisplayName;
}

[HasCommandConsoleCommands]
public class MasterControl : MonoBehaviour 
{
	public static MasterControl Instance;
	public static NetworkManager NetworkManager;
	public static InputManager InputManager;
	public static SceneControl SceneControl;
	public static ConfigSettings Settings;
	//public static SystemConfig SystemConfig;
	public static ResearcherCamController ResearcherCamera;

	public SystemManager SystemManager;

	public Transform MainCanvas;
	public Transform EventSystem;

	public Transform LogPanel;
	public Transform MiniMap;
	public GameObject NodeButton;
	public GameObject EmptyMapSprite;
	public GameObject StraightDotSprite;

	public MainMenuController MainMenu;	


	public string BackgroundSceneName;
	public MapData[] MineMaps;

	public Gradient VentContaminantGradient;
	public Gradient VentMethaneGradient;

	public static event UnityAction SceneChanged;


	public static ClientRole ActiveClientRole = ClientRole.Unknown;

	//private static Scene _backgroundScene;
	//private static GameObject _currentPlayerObj;

	private static GameObject _researchControlsUI;
	private static GameObject _chooseMineUI;
	private static OptionsMenuController _optionsUI;
	private static Light _researcherLight;
	private static float _researcherLightTargetIntensity;


	private static Text _logText;
	private static System.Text.StringBuilder _logStringBuilder;
	private static LinkedList<string> _logMessageList;

	private bool _initialized = false;


	void Awake()
	{
		if (SystemManager == null)
			SystemManager = SystemManager.GetDefault();

		SystemManager.MainCameraChanged += OnMainCameraChanged;

		Instance = this;
		Settings = new ConfigSettings();
		//SystemConfig = YAMLConfig.LoadConfig<SystemConfig>("system_config.yaml");

		string configFile = GetConfigFilePath();
		//Debug.Log(configFile);
		if (File.Exists(configFile))
		{
			Settings.LoadSettings(configFile);
		}
		else
		{
			Settings.LoadDefaultSettings();
			Settings.SaveSettings(configFile);
		}

		_logMessageList = new LinkedList<string>();
		_logStringBuilder = new System.Text.StringBuilder(10000);
		
	}

	private void OnMainCameraChanged()
	{
		var cam = SystemManager.MainCamera;
		if (cam == null)
			return;


		if (SystemManager.SystemConfig.SystemType == SystemType.CAVE)
		{
			//move the main canvas to world space in front of the main camera
			var matchTransform = MainCanvas.GetComponent<FollowTransform>();
			var canvas = MainCanvas.GetComponent<Canvas>();

			if (matchTransform == null)
			{
				matchTransform = MainCanvas.gameObject.AddComponent<FollowTransform>();
			}

			float uiDist = 2.0f;
			float uiHFov = 110.0f * Mathf.Deg2Rad;
			float uiVFov = cam.fieldOfView * cam.aspect * Mathf.Deg2Rad;

			canvas.renderMode = RenderMode.WorldSpace;
			matchTransform.Target = cam.transform;
			matchTransform.Offset = new Vector3(0,0,uiDist);

			var rt = canvas.GetComponent<RectTransform>();
			rt.sizeDelta = new Vector2(1600,900);
			rt.anchorMin = rt.anchorMax = Vector2.zero;
			rt.pivot = new Vector2(0.5f, 0.5f);

			//compute target canvas height to hit the camera vfov
			float height = Mathf.Tan(uiVFov / 2.0f) * 2 * uiDist;

			//compute scale to make the fixed 900 pixel height canvas that width
			float scale =  height / 900.0f;
			scale *= SystemManager.SystemConfig.CAVEUIScale;

			rt.localScale = new Vector3(scale,scale,scale);

		}
		else if (UnityEngine.XR.XRSettings.enabled)
		{
		//move the main canvas to world space in front of the main camera
			var matchTransform = MainCanvas.GetComponent<FollowTransform>();
			var canvas = MainCanvas.GetComponent<Canvas>();

			if (matchTransform == null)
			{
				matchTransform = MainCanvas.gameObject.AddComponent<FollowTransform>();
			}

			float uiDist = 2.0f;
			//float uiHFov = 110.0f * Mathf.Deg2Rad;
			//float uiVFov = cam.fieldOfView * cam.aspect * Mathf.Deg2Rad;
			float uiVFov = cam.fieldOfView * Mathf.Deg2Rad;

			canvas.renderMode = RenderMode.WorldSpace;
			matchTransform.Target = cam.transform;
			matchTransform.Offset = new Vector3(0,0,uiDist);

			var rt = canvas.GetComponent<RectTransform>();
			rt.sizeDelta = new Vector2(1600,900);
			rt.anchorMin = rt.anchorMax = Vector2.zero;
			rt.pivot = new Vector2(0.5f, 0.5f);

			//compute target canvas height to hit the camera vfov
			float height = Mathf.Tan(uiVFov / 2.0f) * 2 * uiDist;

			//compute scale to make the fixed 900 pixel height canvas that width
			float scale =  height / 900.0f;
			//scale *= SystemManager.SystemConfig.CAVEUIScale;

			rt.localScale = new Vector3(scale,scale,scale);
		}
	}

	public void Initialize()
	{
		if (_initialized)
			return;

		if (LogPanel != null)
			_logText = LogPanel.GetComponentInChildren<Text>();

        Util.DontDestroyOnLoad(gameObject);
		//DontDestroyOnLoad(MainCanvas.gameObject);
		//DontDestroyOnLoad(EventSystem.gameObject);

		if (SceneControl == null)
			SceneControl = GetSceneControl();

		if (SceneControl == null)
			SceneManager.LoadScene(BackgroundSceneName, LoadSceneMode.Single);

		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		NetworkManager = GetComponent<NetworkManager>();
		//NetworkManager.InitializeNetwork();

		InputManager = GetComponent<InputManager>();

		LogManager.LogMessageEntered += OnLogMessageEntered;

		if (SceneControl != null)
		{
			//we hvae loaded into an existing scene, create the camera, etc.
			SetClientRole(ClientRole.Researcher);
			ShowChooseMine(false, null);
			InitializePlayerPrefab();
		}

		_initialized = true;
	}

	void Start () 
	{
		if (!_initialized)
			Initialize();
	}

	private void OnLogMessageEntered(LogPacket log)
	{
		if (LogPanel.gameObject.activeInHierarchy)
		{
			//_logStringBuilder.AppendLine(log.ToString());
			_logStringBuilder.Length = 0;

			_logMessageList.AddFirst(log.ToString());
			if (_logMessageList.Count > 100)
				_logMessageList.RemoveLast();

			foreach (string str in _logMessageList)
				_logStringBuilder.AppendLine(str);

			_logText.text = _logStringBuilder.ToString();
		}
	}

	void OnDestroy()
	{
		//Settings.SaveSettings(GetConfigFilePath());		
	}

	static string GetConfigFilePath()
	{
		return Application.persistentDataPath + "/" + "VRMineConfig.json";
	}

	public static void SaveSettings()
	{
		Settings.SaveSettings(GetConfigFilePath());
	}

	public static SceneControl GetSceneControl()
	{
		GameObject sceneControlObj = GameObject.Find("SceneController");
		if (sceneControlObj == null)
			return null;

		return sceneControlObj.GetComponent<SceneControl>();
	}

	private void OnActiveSceneChanged(Scene arg0, Scene arg1)
	{
		InitializePlayerPrefab();
		RaiseSceneChanged();
	}

	public static void TakeScreenshot()
	{
		Directory.CreateDirectory("Screenshots");
		string filename = string.Format("Screenshots/Screenshot_{0}.png", System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
		ScreenCapture.CaptureScreenshot(filename, 1);
	}

	private void InitializePlayerPrefab()
	{
		InputManager.BindKeyboardAndMouse(null);
		ResearcherCamera = null;

		//Debug.Log("OnActiveSceneChanged");
		GameObject sceneControlObj = GameObject.Find("SceneController");
		if (sceneControlObj == null)
			return;

		SceneControl = sceneControlObj.GetComponent<SceneControl>();

		//LogManager logManager = SceneControl.GetComponent<LogManager>();
		//if (logManager != null)
		//{
		//	logManager.
		//}

		MasterControl.ShowChooseMine(false, null);

		GameObject actorObj = null;
		Transform spawnPoint = null;

		switch (ActiveClientRole)
		{
			case ClientRole.Player:
				
				//actorObj = NetworkManager.Instance.InstantiateNetObj("Actor");
				//SceneControl.CurrentActorHost = actorObj.GetComponent<ActorHost>();
				//SceneControl.CurrentActorHost.ActorName = Settings.PlayerName;
				//InputManager.BindKeyboardAndMouse(SceneControl.CurrentActorHost);

				//spawnPoint = SceneControl.GetPlayerSpawn(NetworkManager.Instance.GetClientID());
				//SceneControl.CurrentActorHost.transform.position = spawnPoint.position;
				//SceneControl.CurrentActorHost.transform.rotation = spawnPoint.rotation;

				//SceneControl.CurrentActorHost.EnableCamera(true);
				break;

			case ClientRole.MultiUser:
				//actorObj = NetworkManager.Instance.InstantiateNetObj("MultiUserActor");
				//SceneControl.CurrentActorHost = actorObj.GetComponent<MultiUserActorHost>();
				//SceneControl.CurrentActorHost.ActorName = Settings.PlayerName;
				//InputManager.BindKeyboardAndMouse(SceneControl.CurrentActorHost);

				//spawnPoint = SceneControl.GetPlayerSpawn(NetworkManager.Instance.GetClientID());
				//SceneControl.CurrentActorHost.transform.position = spawnPoint.position;
				//SceneControl.CurrentActorHost.transform.rotation = spawnPoint.rotation;

				//SceneControl.CurrentActorHost.EnableCamera(true);
				break;

			case ClientRole.Researcher:
				EnableResearcherLight(true);
				//GameObject obj = NetworkManager.Instance.InstantiateNetObj("ResearcherCamera");

				// GameObject cinemachineCam = GameObject.Find("CinemachineCamera");
				// if (cinemachineCam == null)
				// {
				// 	cinemachineCam = Util.InstantiateResource("CinemachineCamera");
				// 	cinemachineCam.name = "CinemachineCam";
				// }

				GameObject obj = Util.InstantiateResource("ResearcherCamera");
				IInputTarget inputTarget = obj.GetComponent<IInputTarget>();
				InputManager.BindKeyboardAndMouse(inputTarget);
				ResearcherCamera = obj.GetComponent<ResearcherCamController>();

				Transform researcherSpawnPoint = SceneControl.GetResearcherSpawn();
				ResearcherCamera.transform.position = researcherSpawnPoint.position;
				ResearcherCamera.transform.rotation = researcherSpawnPoint.rotation;
				break;
		}

	}

	void Update () 
	{
		
		if (_researcherLight != null)
		{
			float intensity = _researcherLight.intensity;
			_researcherLight.intensity = Mathf.MoveTowards(intensity, _researcherLightTargetIntensity, 1.3f * Time.deltaTime);
		}

		if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Equals))
		{
			Time.timeScale = Mathf.Clamp(Time.timeScale + 1.0f, 0, 2.0f);

		}

		if (Input.GetKeyDown(KeyCode.Minus))
		{
			Time.timeScale = Mathf.Clamp(Time.timeScale - 1.0f, 0, 2.0f);
		}
		
		if (Input.GetKeyDown(KeyCode.KeypadDivide))
		{
			if (SystemManager.MainCamera != null)
				SystemManager.MainCamera.stereoSeparation = 0;
		}

		if (Input.GetKeyDown(KeyCode.KeypadMultiply))
		{
			if (SystemManager.MainCamera != null)
				SystemManager.MainCamera.stereoSeparation = 0.04f;
		}

		if (Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			if (SystemManager.MainCamera != null)
				SystemManager.MainCamera.stereoSeparation = 0.07f;
		}

		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			if (Input.GetKeyDown(KeyCode.L))
			{
				_logStringBuilder.Length = 0;
				_logText.text = "";
				LogPanel.gameObject.SetActive(!LogPanel.gameObject.activeSelf);

			}            

			if (Input.GetKeyDown(KeyCode.H))
			{
				if (_researchControlsUI != null)
				{
					if (_researchControlsUI.activeSelf)
					{
						ShowResearcherControls(false);
						
						
					}
					else
					{
						ShowResearcherControls(true);
					}
				}
			}

			if (Input.GetKeyDown(KeyCode.A))
			{
				if (ResearcherCamera != null && ResearcherCamera.AxisOverlay != null)
				{
					ResearcherCamera.AxisOverlay.SetActive(!ResearcherCamera.AxisOverlay.activeSelf);
				}
			}

		}

		if (Input.GetKeyDown(KeyCode.F11))
		{
			Debug.Log("Taking Screenshot");
			TakeScreenshot();
		}


		if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
		{
			if (Input.GetKeyDown(KeyCode.Alpha0))
				SetResearcherLightIntensity(0);
			if (Input.GetKeyDown(KeyCode.Alpha1))
				SetResearcherLightIntensity(0.1f);
			if (Input.GetKeyDown(KeyCode.Alpha2))
				SetResearcherLightIntensity(0.2f);
			if (Input.GetKeyDown(KeyCode.Alpha3))
				SetResearcherLightIntensity(0.3f);
			if (Input.GetKeyDown(KeyCode.Alpha4))
				SetResearcherLightIntensity(0.4f);
			if (Input.GetKeyDown(KeyCode.Alpha5))
				SetResearcherLightIntensity(0.5f);
			if (Input.GetKeyDown(KeyCode.Alpha6))
				SetResearcherLightIntensity(0.6f);
			if (Input.GetKeyDown(KeyCode.Alpha7))
				SetResearcherLightIntensity(0.7f);
			if (Input.GetKeyDown(KeyCode.Alpha8))
				SetResearcherLightIntensity(0.8f);
			if (Input.GetKeyDown(KeyCode.Alpha9))
				SetResearcherLightIntensity(0.9f);
		}

		if (Input.GetKeyDown(KeyCode.P))
		{
			/*
			var defProxSystems = GameObject.FindObjectsOfType<DeformableProxSystem>();
			foreach (var proxSystem in defProxSystems)
			{
		
				proxSystem.ShowVisualization = !proxSystem.ShowVisualization;
			}
			*/

			var proxControllers = GameObject.FindObjectsOfType<ProxSystemController>();
			foreach (var proxController in proxControllers)
			{
				proxController.EnableVisualization(!proxController.ShowVisualization);
			}
		}

		if (Input.GetKeyDown(KeyCode.T))
		{
			Debug.Log("Toggling Researcher Cap Lamp");

			
			if (ResearcherCamera != null && ResearcherCamera.ResearcherCapLamp != null)
			{
				ResearcherCamera.ResearcherCapLamp.SetActive(!ResearcherCamera.ResearcherCapLamp.activeSelf);
			} 

			// var cinCam = GameObject.Find("CinemachineCam");
			// if (cinCam != null)
			// {
			// 	Transform capLamp = cinCam.transform.Find("CapLamp");
			// 	if (capLamp != null)
			// 		capLamp.gameObject.SetActive(!capLamp.gameObject.activeSelf);
			// }
		}
	}

	public static void SetClientRole(ClientRole newRole)
	{
		ActiveClientRole = newRole;

		if (newRole == ClientRole.Researcher)
		{
			//NetworkManager.SendLoadScene("TestMine1");
			ShowMainMenu(false);
			ShowResearcherControls(true);
			EnableResearcherLight(true);

			if (NetworkManager.IsConnected)
			{
				ShowChooseMine(true, (string mapName) =>
				{
					MasterControl.RequestSceneLoad(mapName);
					MasterControl.ShowChooseMine(false, null);
				});
			}
			else
			{
				ShowChooseMine(true, (string mapName) =>
				{
					MasterControl.LoadSceneSinglePlayerResearcher(mapName);
					MasterControl.ShowChooseMine(false, null);
				});
			}
		}
		else if (newRole == ClientRole.Player || newRole == ClientRole.MultiUser)
		{
			ShowMainMenu(false);
			EnableResearcherLight(false);
		}
		else
		{
			ShowMainMenu(true);
			EnableResearcherLight(false);
			ShowResearcherControls(false);
		}
	}

	public static void RequestSceneLoad(string name)
	{		
		//NetworkManager.SendLoadScene(name);
	}
	
	public static void RequestLoadMainMenu()
	{
		
		//NetworkManager.SendLoadMainMenu();
	}

	public static void LoadMainMenu()
	{
		LoadScene(Instance.BackgroundSceneName);
		//SetClientRole(ClientRole.Unknown);

		//reinitialzie client role based menus
		SetClientRole(ActiveClientRole);
	}

	public static void LoadScene(string name)
	{
		/*if (_backgroundScene.isLoaded)
		{
			Debug.Log("Unloading background scene");
			SceneManager.UnloadScene(_backgroundScene);
		}*/

		Time.timeScale = 1.0f;
		SceneManager.LoadScene(name, LoadSceneMode.Single);

		
	}

	public static void LoadScene(string name, ClientRole role)
	{
		SetClientRole(role);
		LoadScene(name);
	}


	public static void LoadSceneSinglePlayer(string name)
	{
		SetClientRole(ClientRole.Player);
		LoadScene(name);
	}

	public static void LoadSceneSinglePlayerResearcher(string name)
	{
		SetClientRole(ClientRole.Researcher);
		LoadScene(name);
	}

	public static void ShowMainMenu(bool bShow)
	{
		Instance.MainMenu.gameObject.SetActive(bShow);
	}

	public static void ShowChooseMine(bool bShow, UnityAction<string> LoadCallback)
	{
		if (bShow)
		{
			if (_chooseMineUI == null)
			{
				_chooseMineUI = InstantiateUIPanel("GUI/ChooseMine");
			}

			_chooseMineUI.GetComponent<ChooseMineController>().MapSelectedCallback = LoadCallback;
			_chooseMineUI.SetActive(true);
		}
		else
		{
			if (_chooseMineUI != null)
				_chooseMineUI.SetActive(false);
		}
	}

	[CommandConsoleCommand("show_researchercontrols", "Show or hide the researcher control window")]
	public static void ShowResearcherControls(bool? bShow)
	{
		if (_researchControlsUI == null)
			return;

		if (bShow == null)
			bShow = !_researchControlsUI.activeSelf;

		ShowResearcherControls((bool)bShow);
	}

	[CommandConsoleCommand("fov", "Chang fov of main camera")]
	public static void CCChangeFOV(float fov)
	{
		var systemManager = SystemManager.GetDefault();
		if (systemManager == null || systemManager.MainCamera == null)
			return;

		var cam = systemManager.MainCamera;

		cam.fieldOfView = fov;
	}

	[CommandConsoleCommand("stereoconv", "Chang convergence of main camera")]
	public static void CCChangeConvergence(float convDist)
	{
		var systemManager = SystemManager.GetDefault();
		if (systemManager == null || systemManager.MainCamera == null)
			return;

		var cam = systemManager.MainCamera;

		cam.stereoConvergence = convDist;
	}

	[CommandConsoleCommand("stereosep", "Chang stereo seperation of main camera")]
	public static void CCChangeStereoSep(float sep)
	{
		var systemManager = SystemManager.GetDefault();
		if (systemManager == null || systemManager.MainCamera == null)
			return;

		var cam = systemManager.MainCamera;

		cam.stereoSeparation = sep;
	}

	public static void ShowResearcherControls(bool bShow)
	{
		if (_researchControlsUI == null)
		{
			_researchControlsUI = InstantiateUIPanel("GUI/ResearcherControls");;
		}

		_researchControlsUI.gameObject.SetActive(bShow);
	}

	public static void ShowOptionsMenu(bool bShow)
	{
		if (_optionsUI == null)
		{
			_optionsUI = InstantiateUIPanel("GUI/OptionsMenu").GetComponent<OptionsMenuController>();
		}

		_optionsUI.LoadSettings();
		_optionsUI.gameObject.SetActive(bShow);
	}

	private static GameObject InstantiateUIPanel(string resourceName)
	{
		GameObject obj = Util.InstantiateResource(resourceName);
		obj.transform.SetParent(Instance.MainCanvas, false);

		return obj;
	}

	public static void EnableResearcherLight(bool bEnable)
	{
		if (bEnable)
		{
			if (_researcherLight == null)
			{
				GameObject objLight = Resources.Load<GameObject>("ResearcherLight");
				objLight = Instantiate<GameObject>(objLight);
                Util.DontDestroyOnLoad(objLight);

				_researcherLight = objLight.GetComponent<Light>();
				
			}
		}
		else
		{
			if (_researcherLight != null)
			{
				Destroy(_researcherLight.gameObject);
			}
		}
	}

	public static void SetResearcherLightIntensity(float intensity)
	{
		/*
		if (_researcherLight != null)
		{
			_researcherLight.intensity = intensity;
		}
		*/

		_researcherLightTargetIntensity = intensity;
	}

	public static float GetResearcherLightTargetIntensity()
	{
		return _researcherLightTargetIntensity;
	}
	

	public static void LogMessage(string msgFormat, LogMsgType type, params object[] parameters)
	{
		string msg = string.Format(msgFormat, parameters);

		if (type == LogMsgType.Error)
			Debug.LogError(msg);
		else
		{
			Debug.Log(msg);
		}

		if (Instance != null)
		{
			Instance.MainMenu.AddLogMessage(msg);
		}
	}

	private static void RaiseSceneChanged()
	{
		UnityAction action = SceneChanged;
		if (action != null)
			action();
	}

	public void CreateBasicMiniMap(List<MineSegment> mineSegments, List<TrackingNode> trackingNodes)
	{
		RectTransform miniMapRect = MiniMap.GetComponent<RectTransform>();
		float minXPos = 10000;
		float minZPos = 10000;
		foreach(MineSegment seg in mineSegments)
		{
			if(seg.transform.position.x < minXPos)
			{
				minXPos = seg.transform.position.x;
			}
			if (seg.transform.position.z < minZPos)
			{
				minZPos = seg.transform.position.z;
			}
		}
		foreach(MineSegment seg in mineSegments)
		{
			if(seg.SegmentConnections.Length < 3)
			{
				GameObject straight = Instantiate(StraightDotSprite);
				straight.transform.SetParent(MiniMap);
				RectTransform sRect = straight.GetComponent<RectTransform>();
				Vector3 pos = sRect.anchoredPosition3D;
				pos.x = (seg.transform.position.x / 1.2f) - minXPos;
				pos.y = (seg.transform.position.z / 1.2f) - minZPos;
				sRect.anchoredPosition3D = pos;
			}
			else
			{
				GameObject cross = Instantiate(StraightDotSprite);
				cross.transform.SetParent(MiniMap);
				RectTransform sRect = cross.GetComponent<RectTransform>();
				Vector3 pos = sRect.anchoredPosition3D;
				pos.x = (seg.transform.position.x / 1.2f) - minXPos;
				pos.y = (seg.transform.position.z / 1.2f) - minZPos;
				sRect.anchoredPosition3D = pos;
			}
		}
		foreach(TrackingNode node in trackingNodes)
		{
			GameObject nodeBtn = Instantiate(NodeButton);
			nodeBtn.transform.SetParent(MiniMap);
			RectTransform sRect = nodeBtn.GetComponent<RectTransform>();
			Vector3 pos = sRect.anchoredPosition3D;
			pos.x = (node.transform.position.x / 1.2f) - minXPos;
			pos.y = (node.transform.position.z / 1.2f) - minZPos;
			sRect.anchoredPosition3D = pos;
			TrackingNodeBtn btnCode = nodeBtn.GetComponent<TrackingNodeBtn>();
			btnCode.nodeRef = node;
			node.MiniMapButton = btnCode;
		}
	}

	public void ToggleMiniMap()
	{
		MiniMap.gameObject.SetActive(!MiniMap.gameObject.activeSelf);
	}

	public void UpdateMiniMap()
	{

	}
}