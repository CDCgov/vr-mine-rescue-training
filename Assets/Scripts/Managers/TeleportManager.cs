using Google.Protobuf;
//using LiteNetLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;


public class TeleportManager : SceneManagerBase
{
    public static TeleportManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<TeleportManager>("TeleportManager", false);
    }

    public NetworkManager NetworkManager;
    public POIManager POIManager;
    public SceneFadeManager SceneFadeManager;

    public float FadeDuration = 1.5f;
    public bool ResetToPOIOnTeleport = false;
    public bool ResetRotationOnTeleport = false;

    public Transform ActiveTeleportTarget
    {
        get 
        { 
            if (_activeTeleportTarget == null)
            {
                if (POIManager == null)
                    return null;

                _activePOI = POIManager.GetSpawnPoint();
                if (_activePOI != null)
                    _activeTeleportTarget = _activePOI.transform;
            }
            return _activeTeleportTarget; 
        }
        set
        {
            _activeTeleportTarget = value;
        }
    }

    public string ActivePOIName
    {
        get
        {
            if (_activePOI_ID == null)
                return "";
            return _activePOI_ID;
        }
    }

    public event Action<Transform> BeforeTeleport;
    public event Action<Transform> Teleporting;
    public event Action<Transform> AfterTeleport;

    private List<TeleportController> _teleportControllers;
    private Transform _activeTeleportTarget;
    private PointOfInterest _activePOI;
    private string _activePOI_ID;

    private void Awake()
    {
        _teleportControllers = new List<TeleportController>();
    }

    public void RegisterController(TeleportController controller)
    {
        _teleportControllers.Add(controller);
    }

    public void UnregisterController(TeleportController controller)
    {
        _teleportControllers.Remove(controller);
    }

    public void TeleportToPOI(PointOfInterest poi)
    {
        VRNTeleportAll msg = new VRNTeleportAll
        {
            TeleportTarget = poi.ID,
            TeleportTime = Time.time,
        };

        NetworkManager.SendNetMessage(VRNPacketType.TeleportAll, msg);
        ProcessTeleportAll(msg);
    }

    // Use this for initialization
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (POIManager == null)
            POIManager = POIManager.GetDefault(gameObject);
        if (SceneFadeManager == null)
            SceneFadeManager = SceneFadeManager.GetDefault(gameObject);

        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChanged;

        //NetworkManager.TeleportAllCommand += OnTeleportAll;
        NetworkManager.RegisterHandler(VRNPacketType.TeleportAll, VRNTeleportAllHandler);

        NetworkManager.ClientJoinedServer += OnClientJoinedServer;
        NetworkManager.ClientJoinedScene += OnClientJoinedScene;
        POIManager.POIAdded += OnPOIAdded;

        Util.DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (NetworkManager != null)
        {
            NetworkManager.UnregisterHandler(VRNPacketType.TeleportAll, VRNTeleportAllHandler);
            NetworkManager.ClientJoinedServer -= OnClientJoinedServer;
            NetworkManager.ClientJoinedScene -= OnClientJoinedScene;
        }

        if (POIManager != null)
            POIManager.POIAdded -= OnPOIAdded;


        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnActiveSceneChanged;

    }


    private void OnClientJoinedScene(int clientID)
    {
        if (!NetworkManager.IsServer)
            return;

        if (_activePOI_ID != null)
        {
            SendTeleportAll(_activePOI_ID, clientID);
        }
    }

    private void OnPOIAdded(PointOfInterest poi)
    {
        if (_activePOI_ID != null && (_activePOI == null || _activePOI.ID != _activePOI_ID))
        {
            //this means we received a command to teleport to a POI that wasn't loaded at the time
            //teleport to the new POI
            _activePOI = poi;
            ImmediateTeleport(_activePOI.transform);
        }
        else if (_activeTeleportTarget == null && poi.POIType == POIType.SpawnPoint)
        {
            //teleport to spawn point if no POI is active
            _activePOI = poi;
            ImmediateTeleport(_activePOI.transform);
        }
    }

    private void OnClientJoinedServer(int clientID)
    {
        //if (_activePOI != null && NetworkManager.IsServer)
        //{
           
        //}
    }

    private void SendTeleportAll(string targetID, int clientID)
    {
        VRNTeleportAll msg = new VRNTeleportAll
        {
            TeleportTarget = targetID,
            TeleportTime = Time.time,
        };

        Debug.Log($"Sending teleport command to new client, id: {clientID}, poi: {_activePOI.ID}");
        NetworkManager.SendNetMessage(VRNPacketType.TeleportAll, msg, clientID:clientID);
    }

    private void VRNTeleportAllHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        try
        {
            //var tele = VRNTeleportAll.Parser.ParseDelimitedFrom(recvStream);
            var tele = new VRNTeleportAll();
            reader.ReadMessage(tele);
            ProcessTeleportAll(tele);

        }
        catch (Exception ex)
        {
            Debug.LogError($"VRNTeleportAllHandler Error: {ex.Message} {ex.StackTrace}");
        }
    }

    private void ProcessTeleportAll(VRNTeleportAll obj)
    {
        //set active name even if the POI isn't currently loaded
        _activePOI_ID = obj.TeleportTarget;

        var pois = POIManager.GetPOIs();

        foreach (var poi in pois)
        {
            if (poi.ID == obj.TeleportTarget)
            {
                _activePOI = poi;
                TeleportToTransform(poi.transform);
                return;
            }
        }
    }

    private async void TeleportToTransform(Transform destination)
    {
        BeforeTeleport?.Invoke(destination);
        if (SceneFadeManager != null)
            await SceneFadeManager.FadeOut(FadeDuration);

        _activeTeleportTarget = destination;
        Teleporting?.Invoke(destination);
        await Task.Delay(250);

        if (SceneFadeManager != null)
            await SceneFadeManager.FadeIn(FadeDuration);

        AfterTeleport?.Invoke(destination);
    }

    public void ImmediateTeleport(Transform destination)
    {
        BeforeTeleport?.Invoke(destination);

        _activeTeleportTarget = destination;
        Teleporting?.Invoke(destination);

        AfterTeleport?.Invoke(destination);

    }

    public void ImmediateTeleport(string destination)
    {
        if (destination == null || destination.Length <= 0)
        {
            ImmediateTeleportToSpawnPoint();
            return;
        }

        var pois = POIManager.GetPOIs();

        foreach (var poi in pois)
        {
            if (poi.ID == destination)
            {
                _activePOI = poi;
                _activePOI_ID = poi.ID;
                ImmediateTeleport(poi.transform);
                return;
            }
        }
    }

    public void ImmediateTeleportToSpawnPoint()
    {
        _activePOI_ID = null;
        _activeTeleportTarget = null;
        _activePOI = POIManager.GetSpawnPoint();

        if (_activePOI != null)
        {
            ImmediateTeleport(_activePOI.transform);
        }
    }


    private void OnActiveSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (transform == null || gameObject == null)
            return; // ignore if we have been / are being destroyed

        Debug.Log("TeleportManager: Active Scene Changed");

        _teleportControllers.Clear();

        ImmediateTeleportToSpawnPoint();



        //if (NetworkManager.IsServer)
        //{
        //    var pois = POIManager.GetPOIs();

        //    foreach (var poi in pois)
        //    {
        //        if (poi.POIType == POIType.SpawnPoint)
        //        {
        //            //TeleportToTransform(poi.transform);
        //            TeleportToPOI(poi);
        //            break;
        //        }
        //    }
        //}

        //var pois = POIManager.GetPOIs();

        //foreach (var poi in pois)
        //{
        //    if (poi.POIType == POIType.SpawnPoint)
        //    {
        //        //transform.position = poi.transform.position;
        //        //transform.rotation = poi.transform.rotation;
        //        ImmediateTeleport(poi.transform);
        //    }
        //}
    }

}
