using UnityEngine;
using System.Collections;
/*
using System.IO.Ports;
using System.IO;

public class ArduinoTimingTest : MonoBehaviour
{
	public string PortName = "COM3";


	private SerialPort _port;
	private byte[] _buffer;

	StreamWriter _streamWriter;

	// Use this for initialization
	void Awake()
	{
		_buffer = new byte[10];

		
	}

	void OnEnable()
	{
		_streamWriter = new StreamWriter("arduino_timing.txt", false, System.Text.Encoding.UTF8, 4000);

		_port = new SerialPort(PortName, 57600, Parity.None, 8, StopBits.One);
		_port.ReadTimeout = 20;
		_port.Open();
	}

	void OnDisable()
	{
		_streamWriter.Close();
		_streamWriter.Dispose();
		_port.Close();
		_port.Dispose();
	}

	// Update is called once per frame
	void Update()
	{
		

		if (Input.GetKeyDown(KeyCode.Alpha1))
			WriteByte(0xFF);
		if (Input.GetKeyDown(KeyCode.Alpha2))
			WriteByte(0xFE);


		try
		{
			//_port.ReadExisting();
			int count = 0;
			int val = 0;

			try
			{
				//read until we get a timeout exception, because unity's serial support sucks
				while (val != -1 && count < 30)
				{
					count++;
					val = _port.ReadByte();
				}
			}
			catch (System.Exception) {  }

			if (count > 0)
			{
				Debug.LogFormat("Cleared {0} bytes", count - 1);
			}


			for (int i = 0; i < 10; i++)
			{
				System.Threading.Thread.Sleep(100);
				RunTimeTrial();
			}


		}
		catch (System.Exception ex)
		{
			Debug.Log(ex.Message);
		}
	}

	void RunTimeTrial()
	{
		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
		int val;

		sw.Start();

		WriteByte(1);
		val = _port.ReadByte();
		sw.Stop();

		Debug.Assert(val == '1');

		double elapsedMs = (double)sw.ElapsedTicks / ((double)System.Diagnostics.Stopwatch.Frequency / 1000.0);
		Debug.LogFormat("Round trip time: {0:F3}", elapsedMs);

		_streamWriter.WriteLine(elapsedMs.ToString("F5"));

		WriteByte(0);
		val = _port.ReadByte();
		Debug.Assert(val == '0');
	}

	void WriteByte(byte data)
	{
		_buffer[0] = data;
		_port.Write(_buffer, 0, 1);
	}
}
*/