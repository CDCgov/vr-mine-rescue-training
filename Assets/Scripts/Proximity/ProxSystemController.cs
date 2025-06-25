using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using Google.Protobuf;

#pragma warning disable 0219

/// <summary>
/// List of all possible proximity zone types
/// </summary>
public enum ProxZone
{
    None = -1,
    GreenZone = 0,
    YellowZone = 1,
    RedZone = 2
}

public enum ProxMWCUserType
{
    DefaultUser = 0,
    Miner = 1,
    Inspector = 2,
    MechSuit = 3
}

public class ProxMachineStateMap
{
    public string MachineState;

    /// <summary>
    /// List of prox systems for this machine state, one for each user type
    /// </summary>
    public ProxSystem[] SystemMap; 

    public ProxMachineStateMap()
    {
        int count = Enum.GetNames(typeof(ProxMWCUserType)).Length;
        SystemMap = new ProxSystem[count];
    }
}

[System.Serializable]
public struct ProxUserMachineStateMap
{
    public ProxMWCUserType UserType;
    public string MachineState;
    public ProxSystem ProxSystem;
}


public static class ProxUtil
{
    public static string GetColoredText(this ProxZone zone)
    {
        switch (zone)
        {
            case ProxZone.GreenZone:
                return "<color=#00ff00ff>Green Zone</color>";

            case ProxZone.YellowZone:
                return "<color=#ffff00ff>Yellow Zone</color>";

            case ProxZone.RedZone:
                return "<color=#ff0000ff>Red Zone</color>";

            default:
                return "Unknown";
        }
    }


}

/// <summary>
/// Proximty system main controller, responsible for interfacing with the various types of 
/// proximity detection systems, and reporting the proximity field and trigger state in a
/// system agnostic manner
/// </summary>
[HasCommandConsoleCommands]
public class ProxSystemController : MonoBehaviour, ISerializationCallbackReceiver
{
    public enum UpdateMethod
    {
        NormalUpdate,
        FixedUpdate,
        Manual
    }
    /// <summary>
    /// The ID of this prox system, only MWCs with matching IDs will interact
    /// </summary>
    public int ProxSystemID = 0;

    public ProxSystem FallbackProxSystem;

    public event UnityAction<ProxZone> ProxZoneChanged;

    /// <summary>
    /// The currently active proximity zone / alert state
    /// </summary>
    public ProxZone ActiveProxZone;

    public List<string> MachineStates;

    public Dictionary<string, ProxMachineStateMap> StateMap;

    public List<ProxUserMachineStateMap> SerializedStateMap;

    public ProxMWCUserType ProxVizualizationUserType = ProxMWCUserType.DefaultUser;
    public bool ShowVisualization;
    //public bool UseFixedUpdate = false;
    public UpdateMethod StateUpdateMethod = UpdateMethod.NormalUpdate;

    /// <summary>
    /// The currently selected proximity system used to determine the proximity state
    /// </summary>
    //private ProxSystem ActiveProxSystem;

    private ProxMachineStateMap _activeStateMap;

    //private ProxZone _activeVisualizationZone;
    private ProxSystem.VisOptions _visOpts;
    private Bounds _activeProxBounds;
    private int _mask;
    private int _playerLayer;

    [CommandConsoleCommand("show_proxvis", "Show or hide the prox system visualization")]
    public static void ShowProxVisCmd(bool? bShow)
    {
        var proxControllers = GameObject.FindObjectsOfType<ProxSystemController>();
        if (proxControllers == null || proxControllers.Length <= 0)
            return;

        if (bShow == null)
            bShow = !proxControllers[0].ShowVisualization;

        foreach (var proxController in proxControllers)
        {
            proxController.EnableVisualization((bool)bShow);
        }
    }

    public void EnableZoneVisualization(ProxSystem.VisOptions opts)
    {
        _visOpts = opts;
        ShowVisualization = true;

        /*if (ActiveProxSystem != null)
            ActiveProxSystem.EnableZoneVisualization(gameObject, zone); */
    }

    public void DisableZoneVisualization()
    {
        ShowVisualization = false;
        /*if (ActiveProxSystem != null)
            ActiveProxSystem.DisableZoneVisualization(); */
    }

    private IEnumerator<GameObject> GetObjectsInZone(ProxZone zone)
    {
        yield return null;
    }

    private void Awake()
    {
        _visOpts = new ProxSystem.VisOptions(true, true);
    }

    private void Start()
    {
        /*//for now use the first system
        if (ProximitySystems != null && ProximitySystems.Length > 0)
        {
            SetActiveProxSystem(ProximitySystems[0]);
        }*/

        _mask = LayerMask.GetMask("Player");
        _playerLayer = LayerMask.NameToLayer("Player");

        _activeProxBounds = new Bounds(Vector3.zero, Vector3.one * 50);

        SetMachineState("Default");
    }

    public ProxSystem GetActiveProxSystem(ProxMWCUserType userType)
    {
        ProxSystem proxSystem = null;

        if (_activeStateMap != null)
        {
            proxSystem = _activeStateMap.SystemMap[(int)userType];
            if (proxSystem == null)
                proxSystem = FallbackProxSystem;
        }
        else
            proxSystem = FallbackProxSystem;

        return proxSystem;
    }

    public void SetMachineState(string state)
    {
        ProxSystem proxSystem;

        //turn off old visualization
        proxSystem = GetActiveProxSystem(ProxVizualizationUserType);
        if (proxSystem != null)
        {
            proxSystem.DisableZoneVisualization();
        }

        StateMap.TryGetValue(state, out _activeStateMap);

        //turn on new visualization
        proxSystem = GetActiveProxSystem(ProxVizualizationUserType);
        if (proxSystem != null && ShowVisualization)
        { 
            proxSystem.EnableZoneVisualization(_visOpts);
        }
    }

    public void EnableVisualization(bool enable)
    {
        ProxSystem proxSystem = GetActiveProxSystem(ProxVizualizationUserType);
        if (proxSystem == null)
            return;

        ShowVisualization = enable;

        if (enable)
        {
            proxSystem.EnableZoneVisualization(_visOpts);
        }
        else
        {
            proxSystem.DisableZoneVisualization();
        }
    }

    /*
    private void SetActiveProxSystem(ProxSystem system)
    {
        ActiveProxSystem = system;
        //_activeProxBounds = system.ComputeProxSystemBounds();
    } */

    private void Update()
    {
        if (StateUpdateMethod == UpdateMethod.NormalUpdate)
            UpdateProxState();
    }

    private void FixedUpdate()
    {
        if (StateUpdateMethod == UpdateMethod.FixedUpdate)
            UpdateProxState();
    }

    /// Summary: update the prox state based on nearby MWCs, etc. Normally called automatically in Update
    public void UpdateProxState()
    {
        if (_activeStateMap == null)
            return;

        ProxZone curZone = ProxZone.GreenZone;

        Vector3 boundsCenter = _activeProxBounds.center + transform.position;
        Vector3 boundsExtents = _activeProxBounds.extents;

        //Debug.DrawLine(boundsCenter, boundsCenter + boundsExtents);

        Collider[] closeObjects = Physics.OverlapBox(boundsCenter, boundsExtents, Quaternion.identity, _mask);

        //int colliderCount = closeObjects.Keys.Count;
        foreach (Collider col in closeObjects)
        //for (int i = 0; i < colliderCount; i++)
        {
            //Collider col = _closeObjects.Keys.;
            //if ((col.gameObject.layer & mask) == 0)
            //continue;
            if (col.gameObject.layer != _playerLayer)
                continue;

            //Vector3 testPos = col.transform.position;
            Vector3 testPos = col.bounds.center;

            //determine user type
            ProxMWC mwc = col.gameObject.GetComponent<ProxMWC>();

            ProxMWCUserType userType = ProxMWCUserType.DefaultUser;
            if (mwc != null)
            {
                userType = mwc.UserType;

                //skip if the system ID doesn't match
                if (mwc.ProxSystemID != ProxSystemID)
                    continue;
            }

            //check appropiate prox system
            ProxSystem proxSystem = GetActiveProxSystem(userType);
            ProxZone colZone = proxSystem.TestPoint(testPos);

            ProxAudioAlert alert = col.gameObject.GetComponent<ProxAudioAlert>();

            if (colZone > curZone)
            {
                curZone = colZone;
            }

            if (alert != null)
            {
                alert.SetProxZone(colZone);
            }

            //if (curZone == ProxZone.RedZone)
            //break; // don't need to keep testing
        }

        //_activeProxZone = curZone;
        if (curZone != ActiveProxZone)
        {
            ActiveProxZone = curZone;
            RaiseProxZoneChanged(curZone);
        }
    }

    private void LateUpdate()
    {
        /*
        if (ActiveProxSystem != null)
        {
            ProxZone activeProxZone = ActiveProxSystem.GetActiveProxZone();

            if (activeProxZone != ActiveProxZone)
            {
                ActiveProxZone = activeProxZone;
                RaiseProxZoneChanged(activeProxZone);
            }
        }
        */
    }

    private void RaiseProxZoneChanged(ProxZone zone)
    {
        var handler = ProxZoneChanged;
        if (handler != null)
        {
            handler(zone);
        }
    }

    public bool NeedsUpdate()
    {
        return true;
    }

    public void RebuildStateMap()
    {
        SaveStateMap();
        LoadStateMap();
    }

    private void SaveStateMap()
    {
        SerializedStateMap = new List<ProxUserMachineStateMap>();

        if (StateMap == null)
            return;

        foreach (KeyValuePair<string, ProxMachineStateMap> kvp in StateMap)
        {
            ProxMachineStateMap map = kvp.Value;
            for (int i = 0; i < map.SystemMap.Length; i++)
            {
                if (map.SystemMap[i] != null)
                {
                    ProxUserMachineStateMap newMapping = new ProxUserMachineStateMap();
                    newMapping.MachineState = map.MachineState;
                    newMapping.ProxSystem = map.SystemMap[i];
                    newMapping.UserType = (ProxMWCUserType)i;
                    SerializedStateMap.Add(newMapping);
                }
            }
        }
    }

    private void LoadStateMap()
    {	
        //create an entry in the map for each machine state
        StateMap = new Dictionary<string, ProxMachineStateMap>();
        foreach (string machineState in MachineStates)
        {
            ProxMachineStateMap machineMap = new ProxMachineStateMap();
            machineMap.MachineState = machineState;

            StateMap.Add(machineState, machineMap);
        }

        if (SerializedStateMap == null)
            return;

        //load state mapping from the serialized array
        for (int i = 0; i < SerializedStateMap.Count; i++)
        {
            var map = SerializedStateMap[i];

            ProxMachineStateMap machineMap;
            if (StateMap.TryGetValue(map.MachineState, out machineMap))
            {
                machineMap.SystemMap[(int)map.UserType] = map.ProxSystem;
            }
        }
    }

    public void WriteObjState(CodedOutputStream writer)
    {

    }

    public void SyncObjState(CodedInputStream reader)
    {

    }

    public void OnBeforeSerialize()
    {
        SaveStateMap();
    }

    public void OnAfterDeserialize()
    {
        //Debug.Log("Loading State Map");
        LoadStateMap();
    }
}