using System;
using System.Collections;
/*
using System.IO.Ports;
using System.IO;

public class ArduinoSerial
{
	private SerialPort _port;
	private byte[] _buffer;

	public ArduinoSerial()
	{
		_buffer = new byte[10];
	}

	public void Open(string port)
	{
		_port = new SerialPort(port, 57600, Parity.None, 8, StopBits.One);
		_port.ReadTimeout = 10;
		_port.Open();
	}

	public void Close()
	{
		_port.Close();
		_port.Dispose();
	}

	public void WriteByte(byte data)
	{
		_buffer[0] = data;
		_port.Write(_buffer, 0, 1);
	}

	public void EnableOutput(int pin)
	{
		WriteByte((byte)pin);
	}

	public void ResetOutputs()
	{
		WriteByte(0);
	}

	public void EnableLED(bool bEnable)
	{
		if (bEnable)
			WriteByte(0xFF);
		else
			WriteByte(0xFE);
	}
}
*/