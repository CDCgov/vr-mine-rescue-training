using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
//using UnityEditor;
using System;
using System.Reflection;

/// <summary>
/// Class that manages the log within the Unity scene. It will handle the creation of the LogEntry instance and will await calls to add to the log from log handles within the unity scene.
/// </summary>
public class LogManager : MonoBehaviour {
    
    //Original approach was to utilize Unity Events. However, this caused conflicts with File I/O as the Event is considered another process.
    
    //Flag usable in editor to enable/disable logging    
    public bool LoggingActive = true;

    public static event UnityAction<LogPacket> LogMessageEntered;


    private Queue<LogPacket> _LogPacketQueue;
    private LogEntry _LogEntryRef;
    private StringMessageData _Messenger;
    private List<LogPacket> _PacketsToIgnore;//OBSOLETE, filtering of log packets moved to Log Viewer application
    private Dictionary<string, bool> _PacketActiveDictionary;//OBSOLETE, see above
    private bool _WaitingOnFrame = false;
    
    // Use this for initialization
    void Awake()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        _LogPacketQueue = new Queue<LogPacket>();
        _LogEntryRef = new LogEntry(sceneName, Application.dataPath);
        _Messenger = new StringMessageData();
        _Messenger.TimeStamp = (double)Time.time;
        _Messenger.Message = sceneName + " Project, Log created";
        _LogEntryRef.WriteToLog(_Messenger);

        _PacketActiveDictionary = new Dictionary<string, bool>();
        //_WaitingOnFrame = false;
        //_IsClosing = false;		
    }
    
    void OnApplicationQuit()
    {
        Debug.Log("Closing Log");
        _Messenger.Message = "Exiting program";
        _Messenger.TimeStamp = (double)Time.time;
        _LogEntryRef.WriteToLog(_Messenger);
        //_IsClosing = true;
        StopAllCoroutines();		
        _LogEntryRef.CloseLog(!LoggingActive);
    }
    
    public void AddPacketToQueue(LogPacket packet)
    {
        if (LoggingActive)
        {
            //Debug.Log(packet.ToString());
            
            _LogPacketQueue.Enqueue(packet);
            RaiseLogMessageEntered(packet);
            if (!_WaitingOnFrame)
            {
                _WaitingOnFrame = true;
                StartCoroutine(SendLogsToWriter());
            }
        }
        //if (LoggingActive)
        //{
        //    string ignoreCheck = packet.GetType().ToString();
        //    bool ignore;
        //    if (_PacketActiveDictionary.TryGetValue(ignoreCheck, out ignore))
        //    {
        //        if (!ignore)
        //        {
        //            _LogEntryRef.WriteToLog(packet);
        //            RaiseLogMessageEntered(packet);
        //        }
        //    }
        //    else
        //    {
        //        _LogEntryRef.WriteToLog(packet);
        //        RaiseLogMessageEntered(packet);
        //    }
        //}
    }
    

    //We want to wait until the scene is rendered to log updates. The only alternative using the Unity Engine would be to stick a helper class onto a Camera and do this code on OnPostRender().
    //After talking with Will, it seems like we just want to be careful about when the call to Log is made in the Update of objects we're tracking
    IEnumerator SendLogsToWriter()
    {
        //_WaitingOnFrame = true;		
        yield return new WaitForEndOfFrame();		
        LogPacket packet;
        //double time = (double)Time.time;
        double time = Time.time; //TODO: Create high precision Timer class
        //Debug.Log(_LogPacketQueue.Count);
        while (_LogPacketQueue.Count > 0)
        {
            packet = _LogPacketQueue.Dequeue();
            packet.TimeStamp = time; //Timestamp is when the visual information is rendered.           
            packet.Frame = Time.frameCount;
            if (_LogEntryRef != null)
            {
                _LogEntryRef.WriteToLog(packet);                
            }

        }		
        _WaitingOnFrame = false;
    }
    //Event created to stream in log message to a different UI system.
    private static void RaiseLogMessageEntered(LogPacket packet)
    {
        UnityAction<LogPacket> action = LogMessageEntered;

        if(action != null)
        {
            action(packet);
        }
    }


    /// <summary>
    /// Method to set logging being active during runtime. Not expected to get much use, practically.
    /// </summary>
    /// <param name="bLogActive"></param>
    public void GlobalLoggingToggle(bool bLogActive)
    {
        LoggingActive = bLogActive;
    }


    //Obsolete Method, type filtering to be performed in Log Viewer, opting to preserve data in the application
    public void IgnoreLogType(LogPacket logType, bool isIgnored)
    {
        string key = logType.GetType().ToString();
        if (isIgnored)
        {			
            if (_PacketActiveDictionary.ContainsKey(key))
            {
                return;
            }
            else
            {
                _PacketActiveDictionary.Add(key, isIgnored);
            }
        }
        else
        {
            _PacketActiveDictionary[key] = isIgnored;
        }
    }
}

//[CustomEditor(typeof(LogManager))]
//public class LogManagerEditor : Editor
//{
//	public override void OnInspectorGUI()
//	{
//		DrawDefaultInspector();
//		Assembly[] assemblies;
//		AppDomain currentDomain = AppDomain.CurrentDomain
//	}
//}