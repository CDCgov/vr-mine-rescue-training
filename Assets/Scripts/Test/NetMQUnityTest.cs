using UnityEngine;
using System.Threading;
using System.Collections;
using System.Timers;

/*
using NetMQ; // for NetMQConfig
using NetMQ.Sockets; 

public class NetMQUnityTest: MonoBehaviour
{

    Thread _clientThread;
    private Object thisLock_ = new Object();
    bool stop_thread_ = false;

    void Start()
    {
        Debug.Log("Start a request thread.");
        _clientThread = new Thread(NetMQClient);
        _clientThread.Start();
    }

    // Client thread which does not block Update()
    void NetMQClient()
    {
        AsyncIO.ForceDotNet.Force();
        NetMQConfig.ManualTerminationTakeOver();
        NetMQConfig.ContextCreate(true);

        string msg;
        var timeout = new System.TimeSpan(0, 0, 1); //1sec

        Debug.Log("Connect to the server.");
        var requestSocket = new RequestSocket(">tcp://127.0.0.1:50020");
        requestSocket.SendFrame("SUB_PORT");
        bool is_connected = requestSocket.TryReceiveFrameString(timeout, out msg);

        while (is_connected && stop_thread_ == false)
        {
            Debug.Log("Request a message.");
            requestSocket.SendFrame("msg");
            is_connected = requestSocket.TryReceiveFrameString(timeout, out msg);
            Debug.Log("Sleep");
            Thread.Sleep(1000);
        }

        requestSocket.Close();
        Debug.Log("ContextTerminate.");
        NetMQConfig.ContextTerminate();
        NetMQConfig.Cleanup(true);
    }

    void Update()
    {
        /// Do normal Unity stuff
    }

    void OnApplicationQuit()
    {
        lock (thisLock_)stop_thread_ = true;
        if (!_clientThread.Join(3000))
        {
            Debug.Log("Thread failed to terminate");
            _clientThread.Abort();
            if (!_clientThread.Join(2000))
            {
                Debug.Log("Thread failed to abort");
            }
        }
        Debug.Log("Thread Terminated");
    }

}*/