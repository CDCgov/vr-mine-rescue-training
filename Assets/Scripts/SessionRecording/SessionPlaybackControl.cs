using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Events;
using Google.Protobuf;

public class PlayerTrackData
{
    public int PlayerID;
    public PlayerRepresentation Player;
    public GameObject PlayerTrackObj;
    public Dictionary<int, PlayerTrackSegmentData> SegmentData;

    public SessionEventManager.EventVisibilityData RoleVisibilityData;
    public SessionEventManager.EventVisibilityData PlayerVisibilityData;
}

public class PlayerTrackSegmentData
{
    public LineRenderer LineRenderer;
    public List<Vector3> PositionBuffer;
    public GameObject GameObject;
}

public class TeamstopArrowData
{
    public GameObject ArrowObj;
    public SessionTeamstopState StartTeamstop;
    public SessionTeamstopState EndTeamstop;
}

public class SessionPlaybackControl : SceneManagerBase
{
    public NetworkManager NetworkManager;
    public SceneLoadManager SceneLoadManager;
    public NetworkedObjectManager NetworkedObjectManager;
    public PlayerManager PlayerManager;
    public TeleportManager TeleportManager;
    public VentilationManager VentilationManager;
    public MineMapManager MineMapManager;
    public POIManager POIManager;
    public PlayerColorManager PlayerColorManager;
    public SocketManager SocketManager;
    public SessionEventManager SessionEventManager;
    public SystemManager SystemManager;

    public GameObject TeamstopArrowPrefab = null;

    //public bool RandomizePlayerNames = false;

    private bool _playerTracksNeedUpdated = false;


    public string LogFilePath;
    public SessionLog CurrentSessionLog
    {
        get { return _sessionLog; }
    }

    public float CurrentPlaybackSpeed
    {
        get { return _playbackSpeed; }
    }

    public bool IsPlaying
    {
        get
        {
            if (_playbackSpeed > 0)
                return true;
            else
                return false;
        }
    }

    public float CurrentTimestamp
    {
        get { return _currentTimestamp; }
    }

    public float EventStartTime
    {
        get { return _eventStartTime; }
        set
        {
            _eventStartTime = value;
            //UpdatePlayerTracks();
            _playerTracksNeedUpdated = true;
            EventTimeChanged?.Invoke();
        }
    }

    public float EventEndTime
    {
        get { return _eventEndTime; }
        set
        {
            _eventEndTime = value;
            //UpdatePlayerTracks();
            _playerTracksNeedUpdated = true;
            EventTimeChanged?.Invoke();
        }
    }

    public bool IsSessionLoaded
    {
        get { return _sessionLoaded; }
    }

    public event Action SessionLoaded;
    public event Action SessionScrubbed;
    public event Action PlaybackSpeedChanged;
    public event Action EventTimeChanged;


    private SessionLog _sessionLog;
    private float _playbackSpeed = 0;
    private float _lastScrubTime = 0;
    private int _activeVentState = -1;

    private CancellationTokenSource _cancelSource;
    private float _progress;
    private bool _loadInProgress = false;
    private bool _sessionLoaded = false;
    private float _currentTimestamp = 0;
    private float _eventStartTime = -1;
    private float _eventEndTime = -1;

    private bool _scrubInProgress = false;

    private Dictionary<int, PlayerTrackData> _playerTrackRenderers;
    private GameObject _playerTrackPrefab = null;

    private List<TeamstopArrowData> _teamstopArrows = null;
    private HashSet<Guid> _ignoredNetObjSet = new HashSet<Guid>();

    private IgnitionSource[] _ignitionSources = null;

    public static SessionPlaybackControl GetDefault(GameObject self)
    {
        return self.GetDefaultManager<SessionPlaybackControl>("SessionPlaybackControl");
    }

    // Start is called before the first frame update
    async void Start()
    {
        if (MineMapManager == null)
            MineMapManager = MineMapManager.GetDefault(gameObject);
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);
        if (POIManager == null)
            POIManager = POIManager.GetDefault(gameObject);
        if (PlayerColorManager == null)
            PlayerColorManager = PlayerColorManager.GetDefault();
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);
        if (SocketManager == null)
            SocketManager = SocketManager.GetDefault(gameObject);
        if (SessionEventManager == null)
            SessionEventManager = SessionEventManager.GetDefault(gameObject);
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        POIManager.POIAdded += OnPOIAdded;
        SessionEventManager.TrailVisibilityChanged += OnTrailVisibilityChanged;

        //yield return new WaitForSeconds(0.25f);
        await Task.Delay(250);

        if (LogFilePath != null && LogFilePath.Length > 0 && File.Exists(LogFilePath))
        {
            _progress = 0;
            _loadInProgress = true;
            StartCoroutine(ShowProgressCoroutine());

            var startTime = Time.realtimeSinceStartup;

            _cancelSource = new CancellationTokenSource();
            var loadSucceeded = await LoadSession(LogFilePath,
                (progress) =>
                {
                    _progress = progress;
                },
                _cancelSource.Token);

            _loadInProgress = false;

            var elapsed = Time.realtimeSinceStartup - startTime;


            if (loadSucceeded)
            {
                Debug.Log($"SessionPlaybackControl: Session Load Succeeded, elapsed: {elapsed:F3}s");
            }
            else
                Debug.Log($"SessionPlaybackControl: Session Load Failed! elapsed: {elapsed:F3}s");

        }
    }

    private void OnTrailVisibilityChanged()
    {
        if (SessionEventManager == null)
            return;

        foreach (var trackData in _playerTrackRenderers.Values)
        {
            if (trackData.PlayerVisibilityData == null)
                trackData.PlayerVisibilityData = SessionEventManager.GetPlayerVisibilityData(trackData.PlayerID);
            if (trackData.RoleVisibilityData == null)
                trackData.RoleVisibilityData = SessionEventManager.GetRoleVisibilityData(trackData.Player.PlayerRole);

            bool bShow = trackData.PlayerVisibilityData.TrailVisible && trackData.RoleVisibilityData.TrailVisible;

            ShowPlayerTrack(trackData, bShow);
        }
    }

    private void OnPOIAdded(PointOfInterest obj)
    {
        ClearTeamstopArrows();
    }

    private void OnDestroy()
    {
        if (_cancelSource != null)
        {
            _cancelSource.Cancel();
            _cancelSource.Dispose();
            _cancelSource = null;
        }
    }

    private IEnumerator ShowProgressCoroutine()
    {
        while (_loadInProgress)
        {
            Debug.Log($"Session Load Progress: {_progress:F1}");
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    public void Play(float speed)
    {
        Debug.Log($"SessionPlaybackControl: Setting playback speed to {speed:F1}");
        _lastScrubTime = Time.unscaledTime;
        _playbackSpeed = speed;

        PlaybackSpeedChanged?.Invoke();
    }

    public async Task<bool> LoadSession(string filename, System.Action<float> progressCallback, CancellationToken cancelToken)
    {
        //_sessionLog = new SessionLog();
        //_sessionLog.LoadLog(LogFilePath);

        var startTime = Time.realtimeSinceStartup;

        bool randomizePlayerNames = false;
        if (SystemManager.SystemConfig != null)
            randomizePlayerNames = SystemManager.SystemConfig.DebriefRandomizePlayerNames;

        var log = await SessionLog.LoadLogAsync(filename, progressCallback, cancelToken, randomizePlayerNames);

        var elapsed = Time.realtimeSinceStartup - startTime;
        Debug.Log($"SessionLog Loading elapsed time: {elapsed:F3}s");

        await LoadSession(log);

        return true;
    }

    /// <summary>
    /// Load a SessionLog instance that is already processed and ready
    /// </summary>
    /// <param name="log"></param>
    public async Task LoadSession(SessionLog log)
    {
        _ignoredNetObjSet.Clear();
        _sessionLog = log;
        NetworkManager.IsPlaybackMode = true;
        //NetworkManager.SendLoadScene(_sessionLog.LogHeader.ActiveScene, true);
        await SceneLoadManager.LoadScene(_sessionLog.LogHeader.ActiveScene);

        if (_playerTrackPrefab == null)
            _playerTrackPrefab = Resources.Load<GameObject>("PlayerTrackRenderer");

        _playerTrackRenderers = new Dictionary<int, PlayerTrackData>();

        //_sessionLog.Scrub(0);
        PlayerManager.RemoveAllPlayers();//Trying to remove all players if they currently exist...

        if (MineMapManager != null)
            MineMapManager.ClearAllMaps();

        _sessionLog.Scrub(_sessionLog.StartTime);
        foreach (var player in _sessionLog.CurrentPlayerData.Values)
        {
            //the playerRep returned by this call is the one that receives updates from Scrub()
            var playerRep = PlayerManager.AddNewPlayer(player.ClientID, player.PlayerID, player.Name, player.PlayerRole);
            PlayerManager.UpdatePlayer(player);

            //var playerTrack = GameObject.Instantiate<GameObject>(playerTrackPrefab);
            //var lineRenderer = playerTrack.GetComponent<LineRenderer>();
            //var buffer = new List<Vector3>(1000);
            var playerTrackData = new PlayerTrackData()
            {
                PlayerID = player.PlayerID,
                //LineRenderer = lineRenderer,
                //GameObject = playerTrack,
                //PositionBuffer = buffer,
                PlayerTrackObj = new GameObject($"PlayerTrack-{player.PlayerID}"),
                Player = playerRep,
                SegmentData = new Dictionary<int, PlayerTrackSegmentData>(),
            };

            playerTrackData.PlayerTrackObj.layer = LayerMask.NameToLayer("Overlay");

            //playerTrack.SetActive(false);
            //playerTrackData.PlayerTrackObj.SetActive(false);

            _playerTrackRenderers.Add(player.PlayerID, playerTrackData);
        }

        CreateTeamstopArrows();

        if (SocketManager != null)
            SocketManager.EnableObjectScan = false;

        _sessionLoaded = true;
        SessionLoaded?.Invoke();

        //reset any existing ventiliation graph, to force loading the graph from the log
        VentilationManager.ClearVentGraph();

        _ignitionSources = FindObjectsByType<IgnitionSource>(FindObjectsSortMode.None);

        Scrub(_sessionLog.StartTime);
    }

    void AddPointToSegment(PlayerTrackData data, Vector3 worldSpacePos, int segID)
    {
        PlayerTrackSegmentData seg;
        if (!data.SegmentData.TryGetValue(segID, out seg))
        {
            seg = new PlayerTrackSegmentData();
            seg.GameObject = GameObject.Instantiate<GameObject>(_playerTrackPrefab);
            seg.GameObject.transform.SetParent(data.PlayerTrackObj.transform);
            seg.LineRenderer = seg.GameObject.GetComponent<LineRenderer>();
            seg.PositionBuffer = new List<Vector3>(1000);

            data.SegmentData.Add(segID, seg);
        }

        seg.PositionBuffer.Add(worldSpacePos);
    }

    void ClearTrackBuffer(PlayerTrackData data)
    {
        foreach (var seg in data.SegmentData.Values)
        {
            seg.PositionBuffer.Clear();
        }
    }

    void UpdateTrackLineRenderers(PlayerTrackData data)
    {
        var color = PlayerColorManager.GetPlayerColor(data.Player.PlayerRole);

        foreach (var seg in data.SegmentData.Values)
        {
            if (seg.PositionBuffer.Count <= 1)
            {
                seg.GameObject.SetActive(false);
                continue;
            }

            seg.LineRenderer.positionCount = seg.PositionBuffer.Count;
            seg.LineRenderer.SetPositions(seg.PositionBuffer.ToArray());
            seg.LineRenderer.startColor = color;
            seg.LineRenderer.endColor = color;
            seg.GameObject.SetActive(true);
        }
    }

    public void ShowPlayerTrack(int playerID, bool bShow)
    {

        PlayerTrackData data;
        if (!_playerTrackRenderers.TryGetValue(playerID, out data))
            return;
        Debug.Log($"Toggling player track: {playerID}, {bShow}, {data.Player.Name}");
        ShowPlayerTrack(data, bShow);
    }

    public void ShowPlayerTrack(PlayerTrackData playerTrackData, bool bShow)
    {
        if (playerTrackData == null || playerTrackData.PlayerTrackObj == null)
            return;

        playerTrackData.PlayerTrackObj.SetActive(bShow);
    }

    void UpdatePlayerTrack(int playerID)
    {
        PlayerTrackData data;
        if (!_playerTrackRenderers.TryGetValue(playerID, out data))
            return;

        if (!data.PlayerTrackObj.activeInHierarchy)
            return;

        ClearTrackBuffer(data);

        PointOfInterest lastPoi = null;
        string lastPoiName = null;

        //data.PositionBuffer.Clear();
        foreach (var trackData in _sessionLog.GetPlayerTrack(playerID, EventStartTime, EventEndTime))
        {
            PointOfInterest poi = null;

            if (trackData.TeleportPoint == lastPoiName)
            {
                //use cached poi from last iteration
                poi = lastPoi;
            }
            else
            {
                poi = POIManager.GetPOI(trackData.TeleportPoint);
                if (poi == null)
                {
                    poi = POIManager.GetSpawnPoint();
                }
                else
                {
                    lastPoi = poi;
                    lastPoiName = trackData.TeleportPoint;
                }
            }

            if (poi == null)
                continue;

            var pos = poi.transform.TransformPoint(trackData.Position);

            AddPointToSegment(data, pos, trackData.TeamstopIndex);

            //data.PositionBuffer.Add(pos);
        }

        //if (data.PositionBuffer.Count <= 1)
        //{
        //    data.GameObject.SetActive(false);
        //    return;
        //}

        UpdateTrackLineRenderers(data);

        //var color = PlayerColorManager.GetPlayerColor(data.Player.PlayerRole);

        //data.LineRenderer.positionCount = data.PositionBuffer.Count;
        //data.LineRenderer.SetPositions(data.PositionBuffer.ToArray());
        //data.LineRenderer.startColor = color;
        //data.LineRenderer.endColor = color;
        //data.GameObject.SetActive(true);        
    }

    public void ClearTeamstopArrows()
    {
        if (_teamstopArrows != null)
        {
            foreach (var data in _teamstopArrows)
            {
                if (data.ArrowObj != null)
                    Destroy(data.ArrowObj);
            }
        }

        _teamstopArrows = null;
    }

    public void CreateTeamstopArrows()
    {
        if (_sessionLog == null)
            return;

        if (TeamstopArrowPrefab == null)
            return;

        ClearTeamstopArrows();

        _teamstopArrows = new List<TeamstopArrowData>();
        SessionTeamstopState lastTeamstop = null;
        PointOfInterest lastTeamstopPOI = null;

        foreach (var teamstop in _sessionLog.GetTeamstops())
        {
            var poi = POIManager.GetPOI(teamstop.TeleportTarget);
            if (poi == null && teamstop.TeleportTarget == "")
                poi = POIManager.GetSpawnPoint();

            if (poi == null)
                continue;

            if (lastTeamstop == null)
            {
                lastTeamstop = teamstop;
                lastTeamstopPOI = poi;
                continue;
            }

            TeamstopArrowData data = new TeamstopArrowData();
            data.ArrowObj = CreateTeamstopArrow(lastTeamstopPOI.transform.position,
                poi.transform.position);
            data.StartTeamstop = lastTeamstop;
            data.EndTeamstop = teamstop;

            _teamstopArrows.Add(data);

            lastTeamstop = teamstop;
            lastTeamstopPOI = poi;

        }
    }

    private GameObject CreateTeamstopArrow(Vector3 start, Vector3 end)
    {
        var obj = Instantiate<GameObject>(TeamstopArrowPrefab);

        var dir = end - start;
        //var length = dir.magnitude;

        dir.Normalize();

        //offset start & end
        start = start + dir * 2.0f;
        end = end - dir * 2.0f;
        var length = Vector3.Distance(start, end);

        obj.transform.position = (start + end) * 0.5f + new Vector3(0, 18, 0);
        obj.transform.rotation = Quaternion.FromToRotation(Vector3.right, dir);
        obj.transform.localScale = new Vector3(length, 1, 4);

        return obj;
    }

    private void UpdateTeamstopArrows()
    {
        if (_teamstopArrows == null)
            return;

        var windowStart = _sessionLog.GetWindowStart();
        var windowEnd = _sessionLog.GetWindowEnd();

        foreach (var arrow in _teamstopArrows)
        {
            if (arrow.ArrowObj == null)
                continue;

            if (arrow.EndTeamstop.TeamstopStartTime > windowStart &&
                arrow.EndTeamstop.TeamstopStartTime < windowEnd)
            {
                arrow.ArrowObj.SetActive(true);
            }
            else
                arrow.ArrowObj.SetActive(false);
        }
    }

    private void UpdatePlayerTracks()
    {
        foreach (var player in _sessionLog.CurrentPlayerData.Values)
        {
            UpdatePlayerTrack(player.PlayerID);
        }

        _playerTracksNeedUpdated = false;
    }

    public async void Scrub(float timestamp)
    {
        if (_sessionLog == null)
            return;

        if (_scrubInProgress)
            return;

        if (_teamstopArrows == null)
            CreateTeamstopArrows();

        try
        {
            _scrubInProgress = true; //allow awaits to finish before scrubbing again

            _sessionLog.Scrub(timestamp);
            _lastScrubTime = Time.unscaledTime;
            _currentTimestamp = timestamp;

            UpdateTeamstopArrows();

            foreach (var player in _sessionLog.CurrentPlayerData.Values)
            {
                bool showPlayer = true;
                var delta = Mathf.Abs(timestamp - player.LastDataTimestamp);
                if (delta > 0.5f)
                {
                    showPlayer = false;
                }

                PlayerManager.UpdatePlayer(player, showPlayer);
            }

            //UpdatePlayerTracks();
            _playerTracksNeedUpdated = true;

            if (_sessionLog.CurrentNetObjState != null)
            {
                await UpdateNetObjState(_sessionLog.CurrentNetObjState);
            }


            if (_sessionLog.CurrentTeamstopState != null && TeleportManager.ActivePOIName != _sessionLog.CurrentTeamstopState.TeleportTarget)
            {
                TeleportManager.ImmediateTeleport(_sessionLog.CurrentTeamstopState.TeleportTarget);
            }

            foreach (var eventData in _sessionLog.GetActiveEvents())
            {
                //Debug.Log($"Event {eventData.EventID} active ({eventData.EventData.Position.ToVector3().ToString()})");
            }

            if (_sessionLog.CurrentVentilationState != null && _sessionLog.CurrentVentilationState.VentStateIndex != _activeVentState)
            {
                _activeVentState = _sessionLog.CurrentVentilationState.VentStateIndex;

                VentilationManager.LoadVentilationState(_sessionLog.CurrentVentilationState);
            }


        }
        finally
        {
            _scrubInProgress = false;
        }
    }

    //Until everything is switched to CodedInputStream, temporarily copy to a buffer
    //to avoid instantiating CodedInputStream on a Stream
    private byte[] _syncBuffer;

    private async Task UpdateNetObjState(SessionNetObjState sessionObjState)
    {

        if (_syncBuffer == null)
        {
            _syncBuffer = new byte[NetworkManager.MaxPayloadSize];
        }

        foreach (var kvp in sessionObjState.Objects)
        {
            var objID = kvp.Key;
            var objState = kvp.Value;

            if (_ignoredNetObjSet.Contains(objID))
                continue;

            var netObj = NetworkManager.GetNetworkObject(objID);

            if (netObj == null && objState.SpawnData != null && objState.SpawnData.AssetID != null) //objState.ObjectAlive && 
            {
                objState.SpawnData.OwnerID = 9999;
                objState.SpawnData.SceneID = -1;

                objState.SpawnData.SpawnActivated = true;
                netObj = await NetworkedObjectManager.HandleSpawnObject(objState.SpawnData);
                if (netObj == null)
                {
                    Debug.LogError($"Error spawning object {objState.SpawnData.AssetID}");
                    _ignoredNetObjSet.Add(objID);
                }
            }

            if (netObj == null)
                continue;

            try
            {
                var xrInteract = netObj.GetComponent<CustomXRInteractable>();
                var rb = netObj.GetComponent<Rigidbody>();
                var animator = netObj.GetComponent<Animator>();

                netObj.SetOwner(9999);
                netObj.gameObject.SetActive(objState.ObjectAlive);
                netObj.OwnershipState = NetworkedObject.NetOwnershipState.OwnedBySelf;

                if (xrInteract != null && objState.SocketID != null && objState.SocketID.Length > 0)
                {
                    AttachToSocket(xrInteract, objState.SocketID);
                }
                else if (xrInteract != null)
                {
                    CustomXRSocket socket = xrInteract.CurrentOwner as CustomXRSocket;
                    if (socket != null)
                    {
                        socket.RequestRemoveSocketedItem();
                    }

                    netObj.OwnershipState = NetworkedObject.NetOwnershipState.OwnedByOther;
                    xrInteract.ResetParent();
                    if (xrInteract.CurrentOwner != null)
                        xrInteract.ChangeOwnership(null);
                }

                if (objState.SyncData != null)
                {
                    var pos = objState.SyncData.Position;

                    int count = objState.SyncData.Read(_syncBuffer, 0, _syncBuffer.Length);

                    if (count > 0)
                    {
                        var codedStream = new CodedInputStream(_syncBuffer, 0, count);
                        netObj.ForceSyncObjState(codedStream);

                        //set OwnedByOther to allow synced position updates
                        netObj.OwnershipState = NetworkedObject.NetOwnershipState.OwnedByOther;
                    }

                    if (xrInteract != null && xrInteract.CurrentOwner != null)
                    {
                        //clear local transform if socketed
                        netObj.OwnershipState = NetworkedObject.NetOwnershipState.OwnedBySelf;
                        netObj.transform.localPosition = Vector3.zero;
                        netObj.transform.localRotation = Quaternion.identity;
                    }

                    objState.SyncData.Position = pos;
                }

                if (objState.NetObjMessage != null && objState.NetObjMessageBuffer != null & objState.NetObjMessageHeader != null)
                {
                    var codedStream = new CodedInputStream(objState.NetObjMessageBuffer);
                    NetworkedObjectManager.HandleNetObjMessage(objState.NetObjMessage, objState.NetObjMessageHeader, codedStream);
                }

                if (animator != null)
                {
                    animator.speed = _playbackSpeed;
                }

                if (rb != null)
                    rb.isKinematic = true;

            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"NetObjState SyncError {netObj.gameObject.name} : {ex.Message} {ex.StackTrace}");
            }


        }

        foreach (var kvp in sessionObjState.MineMaps)
        {
            MineMapManager.LoadSymbolManagerState(kvp.Value);
        }

        SessionScrubbed?.Invoke();
    }

    private void AttachToSocket(CustomXRInteractable xrInteract, string socketID)
    {
        var socketData = SocketManager.GetSocketData(socketID);
        if (socketData == null || socketData.Socket == null)
            return;


        socketData.Socket.InternalSocketItem(xrInteract);
    }



    // Update is called once per frame
    void Update()
    {
        if (_sessionLog == null)
            return;
        if (_sessionLog.CurrentTime >= _sessionLog.EndTime)
        {
            _playbackSpeed = 0;
        }
        if (_playbackSpeed > 0)
        {
            float elapsed = Time.unscaledTime - _lastScrubTime;
            if (elapsed > 0.033f)
            {
                Scrub(_sessionLog.CurrentTime + elapsed * _playbackSpeed);
            }
        }

        if (_playerTracksNeedUpdated)
        {
            UpdatePlayerTracks();
        }
    }
}
