using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFireProtocol;
using System.Threading.Tasks;

public class MFireServerControl : MonoBehaviour
{
	public MFireConnection ServerConnection;

	/// <summary>
	/// Elapsed MFire simulation time in milliseconds
	/// </summary>
	public double MFireElapsedTime;

    /// <summary>
    /// Event triggered just prior to advancing the MFire simulation
    /// </summary>
    public event Action<MFireServerControl> MFireSimulationWillUpdate;

    /// <summary>
    /// Event triggered immediately after receiving the results of a simulation step
    /// </summary>
    public event Action MFireSimulationUpdated;

    //public event Action<MFAirway> AirwayUpdated;
    //public event Action<MFJunction> JunctionUpdated;

    public bool AutoAdvanceEnabled
    {
        get
        {
            return _autoAdvance;
        }
        set
        {
            _autoAdvance = value;
        }
    }

	/*public static MFCUpdateMineState GetMineState()
	{
		lock (_stateLockObj)
		{
			return _mineState;
		}
	}*/

	private object _stateLockObj;
	private MFCUpdateMineState _mineState;

	private Dictionary<int, MFAirway> _mfAirways;
	private Dictionary<int, MFJunction> _mfJunctions;
	private Dictionary<int, MFFan> _mfFans;
	private Dictionary<int, MFFire> _mfFires;

	public bool MFireRunning = false;

	private bool _autoAdvance = false;
	private bool _mfireUpdateInProgress = false;
	private float _lastMFireUpdateRequestTime = 0;
    private MFCConfigureMFire _mfireConfigParameters;

	public void ResetMFireState()
	{
		lock (_stateLockObj)
		{
			_mfAirways = new Dictionary<int, MFAirway>();
			_mfJunctions = new Dictionary<int, MFJunction>();
			_mfFans = new Dictionary<int, MFFan>();
			_mfFires = new Dictionary<int, MFFire>();
		}
	}

	public void UpdateJunction(MFJunction junction)
	{
		lock (_stateLockObj)
		{
			_mfJunctions[junction.Number] = junction;
		}
	}

	public MFJunction GetJunction(int number)
	{
		if (_mfJunctions == null)
			return null;

		MFJunction j = null;
		lock (_stateLockObj)
		{
			j = _mfJunctions[number];
		}
		return j;
	}

	public void UpdateAirway(MFAirway airway)
	{
		lock (_stateLockObj)
		{
			_mfAirways[airway.Number] = airway;

		}
	}

	public void ChangeAirway(MFAirway airway)
	{
		lock (_stateLockObj)
		{
			_mfAirways[airway.Number] = airway;

			if (MFireRunning)
				ServerConnection.SendUpdateAirway(airway.Number, airway);
		}
	}

	public MFAirway GetAirway(int number)
	{
		if (_mfAirways == null)
			return null;

		MFAirway a = null;
		lock (_stateLockObj)
		{
			a = _mfAirways[number];
		}
		return a;
	}

	public MFAirway FindAirwayWithStartJunction(int startJunction)
	{
		MFAirway a = null;
		lock (_stateLockObj)
		{
			foreach (MFAirway airway in _mfAirways.Values)
			{
				if (airway.StartJunction == startJunction)
				{
					a = airway;
					break;
				}
			}
		}
		return a;
	}

	public void UpdateFan(MFFan fan)
	{
		lock (_stateLockObj)
		{
			_mfFans[fan.Number] = fan;
		}
	}

    public void ChangeFan(MFFan fan)
    {
        lock (_stateLockObj)
        {
            _mfFans[fan.Number] = fan;

            if (MFireRunning)
                ServerConnection.SendUpdateFan(fan.Number, fan);
        }
    }

    public void UpdateFire(MFFire fire)
	{
		lock (_stateLockObj)
		{
			_mfFires[fire.Number] = fire;
		}
	}

    public void ChangeFire(MFFire fire)
    {
        lock (_stateLockObj)
        {
            _mfFires[fire.Number] = fire;

            if (MFireRunning)
                ServerConnection.SendUpdateFire(fire.Number, fire);
        }
    }

    public MFFire GetFire(int number)
	{
		MFFire f = null;
		lock (_stateLockObj)
		{
			f = _mfFires[number];
		}
		return f;
	}
	
	public MFFan GetFan(int number)
	{
		MFFan fan = null;
		lock (_stateLockObj)
		{
			fan = _mfFans[number];
		}
		return fan;
	}

	public int GetNumAirways()
	{
		lock (_stateLockObj)
			return _mfAirways.Count;
	}

	public int GetNumJunctions()
	{
		lock (_stateLockObj)
			return _mfJunctions.Count;
	}

	public int GetNumFans()
	{
		lock (_stateLockObj)
			return _mfFans.Count;
	}

	public int GetNumFires()
	{
		lock (_stateLockObj)
			return _mfFires.Count;
	}

	public bool ValidateConfig()
	{
		lock (_stateLockObj)
		{
			foreach (KeyValuePair<int, MFJunction> kvp in _mfJunctions)
			{
				if (kvp.Value.Number != kvp.Key)
					return false;

			}

			foreach (KeyValuePair<int, MFAirway> kvp in _mfAirways)
			{
				if (kvp.Value.Number != kvp.Key)
					return false;

				if (!_mfJunctions.ContainsKey(kvp.Value.StartJunction))
					return false;
				if (!_mfJunctions.ContainsKey(kvp.Value.EndJunction))
					return false;
			}

			foreach (KeyValuePair<int, MFFan> kvp in _mfFans)
			{
				if (kvp.Value.Number != kvp.Key)
					return false;

				if (!_mfAirways.ContainsKey(kvp.Value.AirwayNo))
					return false;
			}

			foreach (KeyValuePair<int, MFFire> kvp in _mfFires)
			{
				if (kvp.Value.Number != kvp.Key)
					return false;

				if (!_mfAirways.ContainsKey(kvp.Value.AirwayNo))
					return false;
			}
		}

		return true;
	}

    public async Task<MFireProtocol.EngineState?> GetEngineState()
    {
        if (ServerConnection == null || !ServerConnection.IsConnected)
            return null;

        var task = ServerConnection.SendMFireCmdWithResult<MFRGetEngineState>(new MFCGetEngineState());
        if (await Task.WhenAny(task, Task.Delay(5000)) == task)
        {
            return ((MFRGetEngineState)task.Result).EngineState;
        }
        else
            return null;
    }

    public void SetMFIREConfigParameters(MFCConfigureMFire config)
    {
        _mfireConfigParameters = config;
        ServerConnection.SendMFireCmd(_mfireConfigParameters);
    }

	public void SendMFireConfig()
	{
		lock (_stateLockObj)
		{
			foreach (MFJunction junction in _mfJunctions.Values)
			{
				ServerConnection.SendUpdateJunction(junction.Number, junction);
			}

			foreach (MFAirway airway in _mfAirways.Values)
			{
				ServerConnection.SendUpdateAirway(airway.Number, airway);
			}

			foreach (MFFan fan in _mfFans.Values)
			{
				ServerConnection.SendUpdateFan(fan.Number, fan);
			}

			foreach (MFFire fire in _mfFires.Values)
			{
				ServerConnection.SendUpdateFire(fire.Number, fire);
			}

			MFireRunning = true;
		}
	}

	MFireServerControl()
	{
		_stateLockObj = new object();
	}

	private void OnEnable()
	{
		if (ServerConnection == null)
		{
			Debug.Log("Connecting to MFire");
			ServerConnection = new MFireConnection();
			MFireRunning = false;
			ServerConnection.Connect("127.0.0.1");
			ServerConnection.MFireCmdReceived += OnMFireCmdReceived;
            //ServerConnection.PacketDecodeError += OnPacketDecodeError;
		}
	}

    private void OnPacketDecodeError(string message)
    {
        Debug.LogError(message);
    }

    private void OnMFireCmdReceived(MFireConnection obj)
	{		
		MFireCmd cmd = obj.DequeueReceivedCmd();

		while (cmd != null)
		{
			//Debug.LogFormat("Received MFire Cmd {0}", cmd.GetType().ToString());

			if (cmd is MFCUpdateMineState)
			{
				lock (_stateLockObj)
				{
					_mineState = (MFCUpdateMineState)cmd;
				}
			}

			else if (cmd is MFCUpdateJunction)
			{
                //Debug.Log("Updated Junction");
				var c = (MFCUpdateJunction)cmd;
				UpdateJunction(c.Junction);
			}

			else if (cmd is MFCUpdateAirway)
			{
                //Debug.Log("Updated Airway");
                var c = (MFCUpdateAirway)cmd;
				UpdateAirway(c.Airway);
			}

			else if (cmd is MFCSimulationUpdated)
			{
				var c = (MFCSimulationUpdated)cmd;
				Debug.LogFormat("MFire Simulation Updated - {0:F2} seconds elapsed", c.ElapsedTimeMs / 1000.0);
				MFireElapsedTime = c.ElapsedTimeMs;
				RaiseMFireSimulationUpdated();
				_mfireUpdateInProgress = false;
			}

			cmd = obj.DequeueReceivedCmd();
		}
	}

	private void OnDisable()
	{
		Debug.Log("Disconnecting from MFire");
		ServerConnection.Dispose();
		ServerConnection = null;
		MFireRunning = false;
	}

	private float _lastTest = 0;

	public void AdvanceMFireSimulation()
	{
        MFireSimulationWillUpdate?.Invoke(this);

		if (ServerConnection != null)
		{
			Debug.Log("Requesting MFire simulation to advance");
			_mfireUpdateInProgress = true;
			ServerConnection.SendRunSimulation();
			
		}

		_lastMFireUpdateRequestTime = Time.time;
	}

    public void ResetMFireSimulation()
    {
        //Debug.Log("Restarting MFire");
        //if (ServerConnection != null)
        //{
        //    ServerConnection.Dispose();
        //    ServerConnection = null;
        //    MFireRunning = false;
        //    System.Threading.Thread.Sleep(100);
        //}

        //ServerConnection = new MFireConnection();
        //ServerConnection.Connect("127.0.0.1");
        //ServerConnection.MFireCmdReceived += OnMFireCmdReceived;

        //var ventControl = FindObjectOfType<VentilationControl>();
        //if (ventControl != null)
        //    _ = ventControl.InitializeVentilation();

        if (ServerConnection == null || _mfireConfigParameters == null || !ServerConnection.IsConnected)
            return;

        ServerConnection.SendResetSimulation();
        //ServerConnection.SendMFireCmd(_mfireConfigParameters);
        //SendMFireConfig();
        //ServerConnection.SendRunSimulation();
        //ServerConnection.SendResetSimulation();

    }

	private void Update()
	{
        /*
		if (Input.GetKeyDown(KeyCode.Keypad7))
		{

            ResetMFireSimulation();
		}

		if (Input.GetKeyDown(KeyCode.Keypad8))
		{
			AdvanceMFireSimulation();	
		}

		if (Input.GetKeyDown(KeyCode.Keypad5))
		{
			_autoAdvance = !_autoAdvance;
			Debug.LogFormat("MFire AutoAdvance: {0}", _autoAdvance);
		}
        */

		if (_autoAdvance && !_mfireUpdateInProgress && Time.time > _lastMFireUpdateRequestTime + 2.0f)
		{
			AdvanceMFireSimulation();
		}
		/*
		if (ServerConnection == null)
			return;

		if (Time.time - _lastTest > 2.0f)
		{
			_lastTest = Time.time;

			ServerConnection.SendMFireCmdTest(41, 42, "Hello World");
		}
		*/
	}

	private void RaiseMFireSimulationUpdated()
	{
        MFireSimulationUpdated?.Invoke();
	}
}
