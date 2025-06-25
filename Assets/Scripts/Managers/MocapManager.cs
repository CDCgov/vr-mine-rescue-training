using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

[HasCommandConsoleCommands]
[CreateAssetMenu(fileName = "MocapManager", menuName = "VRMine/Managers/MocapManager", order = 0)]
public class MocapManager : Manager
{
    public const string DefaultResourcePath = "Managers/MocapManager";
    public static MocapManager GetDefault() { return Resources.Load<MocapManager>(DefaultResourcePath); }

    public SystemManager SystemManager;

    //private ViconInterface _vicon;
    private SystemManager _system;

    [CommandConsoleCommand("get_mocap_status", "get status of mocap connection")]
    public static void CCGetMocapStatus()
    {
        var mocap = MocapManager.GetDefault();
        var system = SystemManager.GetDefault();

        CommandConsole.Print($"Vicon IP: {system.SystemConfig.ViconIP}");

        if (!mocap.IsMocapConnected)
        {
            CommandConsole.Print("Mocap not connected");
            return;
        }

        float elapsedMs = (Time.time - mocap.GetLastPacketReceiveTime()) * 1000.0f;
        CommandConsole.Print($"Mocap Connected, last packet {elapsedMs}ms ago");
    }

    [CommandConsoleCommand("get_mocap_list", "get list of trackers")]
    public static void CCGetMocapList()
    {
        var mocap = MocapManager.GetDefault();

        CommandConsole.Print($"Vicon IP: {mocap.SystemManager.SystemConfig.ViconIP}");

        if (!mocap.IsMocapConnected)
        {
            CommandConsole.Print("Mocap not connected");
            return;
        }

        var segs = mocap.GetSegmentNames();
        foreach (string seg in segs)
        {
            CommandConsole.Print(seg);
        }
    }

    public bool IsMocapConnected
    {
        get { return false; }
        //get
        //{
        //    if (_vicon != null && _vicon.DataReceived)
        //        return true;
        //    else
        //        return false;
        //}
    }

    public float GetLastPacketReceiveTime()
    {
        //if (_vicon == null)
        //    return 0;

        //return _vicon.LastReceiveTime;
        return -1;
    }

    public string[] GetSegmentNames()
    {
        return null;
        //if (_vicon == null)
        //    return null;

        //return _vicon.GetSegmentNames();
    }

    public bool GetSegmentPos(string segName, out Vector3 position, out Quaternion rotation)
    {
     
        position = Vector3.zero;
        rotation = Quaternion.identity;
        return false;

        //if (_vicon == null)
        //    return false;

        //EyeTrackerNet.ViconSegmentData segData = new EyeTrackerNet.ViconSegmentData();
        //if (!_vicon.GetSegmentData(segName, out segData))
        //    return false;

        //position = segData.Position;
        //rotation = segData.Rotation;
        //return true;
    }


    private void OnEnable()
    {
        if (_system == null)
            _system = SystemManager.GetDefault();

        Debug.Log("MocapManager OnEnable");

        //SceneManager.sceneLoaded += OnSceneLoaded;		
        
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        //if (_vicon == null)
        //{
        //    Debug.Log($"Connecting to vicon server at {_system.SystemConfig.ViconIP}");
        //    _vicon = new ViconInterface();
        //    _vicon.Connect(_system.SystemConfig.ViconIP);
        //}
    }

    private void OnDisable()
    {

    }
}