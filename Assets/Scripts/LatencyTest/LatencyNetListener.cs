using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class LatencyNetListener : MonoBehaviour
{
    private Thread _netThread;
    private ManualResetEvent _killThread;

    private LinkedList<string> _errorMessages;

    // Use this for initialization
    void Awake()
    {
        _killThread = new ManualResetEvent(false);
        _errorMessages = new LinkedList<string>();
    }

    void OnEnable()
    {
        _killThread.Reset();
        _netThread = new Thread(NetThreadEntry);

        _netThread.Start();

        
    }

    void OnDisable()
    {
        _killThread.Set();
        _netThread.Join(1000);
    }

    void NetThreadEntry()
    {
        WaitHandle[] handles = new WaitHandle[2];
        byte[] buffer = new byte[10];

        handles[0] = _killThread;

        UdpClient udpClient = new UdpClient(2345);

        lock(_errorMessages)
        {
            _errorMessages.AddLast("Latency Net Thread Running");
        }

        while (true)
        {
            try
            {
                System.IAsyncResult result = udpClient.BeginReceive(null, null);
                handles[1] = result.AsyncWaitHandle;

                int waitResult = WaitHandle.WaitAny(handles);
                if (waitResult == 0) //terminated
                    break;

                else if (result.IsCompleted)
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpClient.EndReceive(result, ref remoteEP);

                    if (data.Length <= 0)
                        continue;

                    buffer[0] = 0xff;
                    udpClient.Send(buffer, 1, remoteEP);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);

                lock (_errorMessages)
                {
                    _errorMessages.AddLast(ex.Message);
                }
            }
        }

        udpClient.Close();

        lock (_errorMessages)
        {
            _errorMessages.AddLast("Latency Net Thread Exiting");
        }
    }

    // Update is called once per frame
    void Update()
    {
        lock (_errorMessages)
        {
            while (_errorMessages.Count > 0)
            {
                Debug.Log(_errorMessages.First.Value);
                _errorMessages.RemoveFirst();
            }
        }
    }
}
