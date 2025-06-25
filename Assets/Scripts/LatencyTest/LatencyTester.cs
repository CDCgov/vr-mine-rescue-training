using UnityEngine;
using System.Collections;
using System.IO;
/*
public class LatencyTester : MonoBehaviour
{
	private string _portName = "COM1";

	public float TestInterval = 1.0f;
	public float WhiteDuration = 0.3f;
	public MeshRenderer LatencyCameraBlockObject;
	public Color ColorWhite = Color.white;
	public Color ColorShaded = new Color(0, 0, 0, 0.9f);

	private double _fps;
	private float _lastTest;
	private bool _whiteEnabled = false;
	private bool _runTest = false;

	private ArduinoSerial _serial;

	void OnEnable()
	{
		if (File.Exists("comport.txt"))
			_portName = File.ReadAllText("comport.txt");
		_serial = new ArduinoSerial();
		_serial.Open(_portName);
	}

	void OnDisable()
	{
		_serial.Close();
	}
	
	void OnGUI()
	{
		Vector3 spawnPos = Camera.main.transform.position + Vector3.forward * 2;
		GUILayout.Label(string.Format("FPS: {0:F3}", _fps));

		if (GUILayout.Button("Spawn Spheres"))
		{
			for (int i = 0; i < 50; i++)
			{
				GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				obj.transform.position = spawnPos;
				obj.AddComponent<Rigidbody>();
			}
		}
	}

	// Use this for initialization
	void Start()
	{
		LatencyCameraBlockObject.material.color = ColorShaded;
	}

	//fixed update is called first, but might be called more than once or not at all
	void FixedUpdate()
	{
		if (Time.time - _lastTest > TestInterval)
		{
			_runTest = true;
			ShowWhite(true);
			_serial.EnableOutput(1);
			_lastTest = Time.time;
		}
	}


	// Update is called once per frame
	void Update()
	{
		if (_runTest)
			_serial.EnableOutput(2);

		if (_whiteEnabled && Time.time - _lastTest > WhiteDuration)
		{
			ShowWhite(false);
		}

		double fps = 1.0 / (double)Time.deltaTime;

		_fps = 0.1 * fps + 0.9 * _fps;


		StartCoroutine(EndOfFrame());

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			_serial.EnableLED(true);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			_serial.EnableLED(false);
		}
	}

	void LateUpdate()
	{
		if (_runTest)
			_serial.EnableOutput(3);
	}

	void ShowWhite(bool bShow)
	{
		_whiteEnabled = bShow;

		if (bShow)
		{
			LatencyCameraBlockObject.material.color = ColorWhite;
		}
		else
		{
			LatencyCameraBlockObject.material.color = ColorShaded;
			_serial.ResetOutputs();
		}
	}

	IEnumerator EndOfFrame()
	{
		yield return new WaitForEndOfFrame();

		if (_runTest)
			_serial.EnableOutput(4);

		_runTest = false;
	}
}
*/