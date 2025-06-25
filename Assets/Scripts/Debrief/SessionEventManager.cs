using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Text;

public class SessionEventManager : SceneManagerBase
{
    private const float EventYStart = 20;
    private const float EventYMax = 55;
    private const float EventYIncrement = 0.001f;

    public class SessionEventInstanceData
    {
        public SessionEventData EventData;
        public DebriefEventItem DBItem;
        public DebriefMarkers.DebriefMarkerData CategoryData;
        public PlayerRepresentation Player;

        //references to the common player & role data structures for speed
        public EventVisibilityData PlayerVisibilityData;
        public EventVisibilityData RoleVisibilityData;
    }

    public class EventVisibilityData
    {
        public bool EventsVisible = true;
        public bool TrailVisible = true;
    }

    public static SessionEventManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<SessionEventManager>("SessionEventManager", false);
    }

    public PlayerColorManager PlayerColorManager;
    public PlayerManager PlayerManager;
    public SystemManager SystemManager;
    public TimelineController TimelineController;
    public MineMapSymbolManager MineMapSymbolManager;

    public DebriefMarkers DebriefMarkerCategories;

    public DebriefSceneLoader DebriefSceneLoader;
    public bool IsDebug = false;
    //public DebriefMarkers DebriefMarkers;
    public Transform DebriefItemCanvas;
    public RectTransform TeamstopContainer;
    public RectTransform TeamstopContainerScrn2;
    public RectTransform TimelineHorizontalBarTransform;
    public RectTransform TimelineHorizontalBarTransformScrn2;
    public GameObject TeamstopPrefab;
    public GameObject DebriefItemPrefab;
    public PlayerVisibiltyHandler PlayerHandler;
    public ActionsVisibilityHandler ActionsHandler;
    public GameObject PlayerTogglePrefab;
    public Transform PlayerToggleContainer;
    public GameObject ActionTogglePrefab;
    public Transform ActionToggleContainer;
    //public List<Transform> SpawnedItems;
    public EventTooltipHandler EventTooltip;
    public SessionPlaybackControl PlaybackControl;
    //public Dictionary<SessionEventData, DebriefEventItem> AllEvents;
    public float EventStartTime = 0;
    public float EventEndTime = -1;
    public SessionPlaybackControl SessionPlaybackControl;
    public DebriefTeamstopColors TSColors;
    public GameObject HighlightCircle;
    public GameObject HighlightCirclePrefab;

    public event Action CategoryVisibilityChanged;
    public event Action SegmentsPopulated;
    public event Action TrailVisibilityChanged;
    public event Action EventVisibilityChanged;

    /// <summary>
    /// Mapping from event ID to SessionEventInstanceData
    /// </summary>
    public Dictionary<int, SessionEventInstanceData> SessionEvents;

    [HideInInspector]
    public List<MineSegmentInfo> LoadedMineSegmentInfos;

    private Dictionary<int, EventVisibilityData> _playerVisibility;
    private Dictionary<VRNPlayerRole, EventVisibilityData> _roleVisibility;

    private HashSet<int> _activeEvents = new HashSet<int>();

    //private List<SessionEventData> _sessionEventDatas;

    //private DebriefMarkers _debriefMarkers;

    private void Awake()
    {
        SessionEvents = new Dictionary<int, SessionEventInstanceData>();
        _playerVisibility = new Dictionary<int, EventVisibilityData>();
        _roleVisibility = new Dictionary<VRNPlayerRole, EventVisibilityData>();

        foreach (var role in System.Enum.GetValues(typeof(VRNPlayerRole)))
        {
            _roleVisibility.Add((VRNPlayerRole)role, new EventVisibilityData());
        }
    }

    void Start()
    {
        if (PlayerColorManager == null)
        {
            PlayerColorManager = PlayerColorManager.GetDefault();
        }
        if (DebriefSceneLoader == null)
        {
            DebriefSceneLoader = FindObjectOfType<DebriefSceneLoader>();
        }
        if (PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }
        if (SessionPlaybackControl == null)
        {
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);
        }

        if(SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }

        if(TimelineController == null)
        {
            TimelineController = TimelineController.GetDefault(gameObject);
        }

        if(MineMapSymbolManager == null)
        {
            MineMapSymbolManager = MineMapSymbolManager.GetDefault(gameObject);
        }
        //_debriefMarkers = await Addressables.LoadAssetAsync<DebriefMarkers>(DebriefMarkerAsset).Task;

        //SpawnedItems = new List<Transform>();
        //_sessionEventDatas = new List<SessionEventData>();
        //Spawn player / action toggle buttons
        //AllEvents = new Dictionary<SessionEventData, DebriefEventItem>();

        DebriefSceneLoader.SceneLoaded += OnSceneLoad;

        
    }

    void OnSceneLoad()
    {
        //if(PlaybackControl == null)
        //{
        //    PlaybackControl = FindObjectOfType<SessionPlaybackControl>();
        //}
        //if (!IsDebug)
        //    SpawnControls();
        Debug.Log("Entered OnSceneLoad in Session Event Manager");
        _playerVisibility.Clear();
        SessionEvents.Clear();
        SessionEvents = new Dictionary<int, SessionEventInstanceData>();
        foreach (var player in PlayerManager.PlayerList.Values)
        {
            _playerVisibility[player.PlayerID] = new EventVisibilityData();
        }

        

        //Clear out old data, if there
        if(DebriefItemCanvas.childCount > 0)
        {
            foreach(Transform child in DebriefItemCanvas)
            {
                Destroy(child.gameObject);
            }
        }

        ClearTeamstops();

        GameObject go = Instantiate(HighlightCirclePrefab, DebriefItemCanvas);
        HighlightCircle = go;
        HighlightCircle.SetActive(false);
        //ActionsHandler.DebriefActionItems = new Dictionary<int, List<GameObject>>();
        //Populating the Mine Segments
        LoadedMineSegmentInfos = new List<MineSegmentInfo>();
        MineSegmentInfo[] mineSegmentInfos = FindObjectsOfType<MineSegmentInfo>();
        Debug.Log($"Found mine segment infos! {mineSegmentInfos.Length}");
        foreach (MineSegmentInfo msi in mineSegmentInfos)
        {
            LoadedMineSegmentInfos.Add(msi);
        }
        Debug.Log($"++++++++++Segments populated+++++++++++ {LoadedMineSegmentInfos.Count}");
        SegmentsPopulated?.Invoke();
        //PlayerHandler.PlayerDebriefItems = new Dictionary<int, List<GameObject>>();
        EventStartTime = SessionPlaybackControl.CurrentSessionLog.StartTime;
        List<Vector3> _spawnedPositions = new List<Vector3>();

        float eventY = EventYStart;

        Debug.Log($"Current session log: {SessionPlaybackControl.CurrentSessionLog.LogFileName}");

        var dmPlayer = new PlayerRepresentation
        {
            Name = "DM",
            PlayerRole = VRNPlayerRole.UnknownRole,
            ClientID = 0,
            PlayerID = 0,
        };

        StringBuilder eventSB = new StringBuilder();

        foreach (var eventData in SessionPlaybackControl.CurrentSessionLog.GetAllEvents())
        {
            try
            {
                //THIS WILL SPAWN ALL THE EVENTS AT LOAD. During runtime/scrub we can toggle visibilty
                //Debug.Log($"{eventData.EventID}: Event type - {eventData.EventData.EventType}: Obj Type - {eventData.EventData.ObjectType}, {eventData.EventData.ObjectName} @ {eventData.EventData.Timestamp}");
                var data = eventData.EventData;
                int player = eventData.EventData.SourcePlayerID;
                int action = -1;
                if (IsDebug)
                {
                    break;
                }
                //List<GameObject> playerDebriefItems = new List<GameObject>();
                //List<GameObject> actionDebriefItems = new List<GameObject>();


                var markerData = DebriefMarkerCategories.GetMarkerData(eventData.EventData.EventType);
                if (markerData == null)
                    continue;

                //PlayerRepresentation playerRep = PlayerManager.GetPlayer(eventData.EventData.SourcePlayerID);
                PlayerRepresentation playerRep = SessionPlaybackControl.CurrentSessionLog.GetPlayerRep(
                    eventData.EventData.SourcePlayerID, 
                    eventData.EventData.Timestamp);

                //if (!PlayerManager.PlayerList.TryGetValue(eventData.EventData.SourcePlayerID, out playerRep))
                if (playerRep == null && eventData.EventData.SourcePlayerID <= 0 && 
                    (eventData.EventData.EventType == VRNLogEventType.Dmaction || 
                    eventData.EventData.EventType == VRNLogEventType.DmspawnObject ||
                    eventData.EventData.EventType == VRNLogEventType.MineExplosion ||
                    eventData.EventData.EventType == VRNLogEventType.Npcunconscious ||
                    eventData.EventData.EventType == VRNLogEventType.Npcdeath))
                {
                    playerRep = dmPlayer;
                }
                else if (playerRep == null)
                {
                    Debug.LogWarning($"No player with that ID in the player list: {eventData.EventData.SourcePlayerID} event ({eventData.EventData.EventType}, {eventData.EventData.ObjectType})");
                    continue;
                }
                else if( eventData.EventData.SourcePlayerID > 0 && (eventData.EventData.EventType == VRNLogEventType.Npcunconscious ||
                    eventData.EventData.EventType == VRNLogEventType.Npcdeath))
                {
                    playerRep = dmPlayer;
                }

                if (string.IsNullOrEmpty(playerRep.Name) || eventData.EventData.Position == null || eventData.EventData.Timestamp <= 0)
                {
                    Debug.LogWarning($"Received bad event data: {eventData.EventData.EventType}, {eventData.EventData.ObjectType}");
                    continue;
                }

                //_sessionEventDatas.Add(eventData);
                GameObject dbItem = GameObject.Instantiate(DebriefItemPrefab, DebriefItemCanvas);

                DebriefEventItem dbEventItem = dbItem.GetComponent<DebriefEventItem>();
                dbEventItem.Icon.sprite = markerData.MarkerSprite;
                dbEventItem.SoundEffect = markerData.SoundEffect;
                dbEventItem.WorldSpacePosition = eventData.EventData.Position.ToVector3();

                string playerName = "";

                //dbEventItem.PlayerColorImage.color = PlayerColorManager.PlayerColors[player];
                
                var markerColor = PlayerColorManager.GetPlayerColor(playerRep.PlayerRole);
                //var markerColor = playerRep.PlayerColor;
                if (eventData.EventData.SourcePlayerID <= 0)
                   markerColor = Color.black;

                dbEventItem.PlayerColorImage.color = markerColor;
                playerName = playerRep.Name;

                VRNVector3 eventPosition = eventData.EventData.Position;
                TimeSpan timeSpan = TimeSpan.FromSeconds(eventData.EventData.Timestamp - SessionPlaybackControl.CurrentSessionLog.StartTime);
                string time = $"{timeSpan.Minutes.ToString("00")}:{timeSpan.Seconds.ToString("00")}";
                Vector3 pos = new Vector3(eventPosition.X, 20, eventPosition.Z);
                if (_spawnedPositions.Contains(pos))
                {
                    pos.x = pos.x + UnityEngine.Random.Range(-0.1f, 0.1f);
                    pos.z = pos.z + UnityEngine.Random.Range(-0.1f, 0.1f);
                }

                pos.y = eventY;
                eventY += EventYIncrement;
                if (eventY > EventYMax)
                    eventY = EventYMax;

                _spawnedPositions.Add(pos);
                //dbEventItem.PlayerColorImage.rectTransform.anchoredPosition3D = pos;//this seemed like a mistake
                dbEventItem.TryGetComponent<RectTransform>(out var rectTransform);
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition3D = pos;
                }
                MineSegmentInfo _associatedMineSegment = null;
                foreach (MineSegmentInfo inf in LoadedMineSegmentInfos)
                {
                    MineSegment ms = inf.GetComponent<MineSegment>();
                    Collider collider = inf.GetComponentInChildren<MeshCollider>();
                    if (ms == null)
                    {
                        continue;
                    }

                    if (collider.bounds.Contains(new Vector3(eventPosition.X, eventPosition.Y, eventPosition.Z)))
                    {
                        _associatedMineSegment = inf;
                        Debug.Log("Segment found");
                        break;
                    }
                }
                Vector2 dist = Vector2.zero;
                string entry = "";
                string crosscut = "";
                if (_associatedMineSegment != null)
                {
                    dist = _associatedMineSegment.GetDistanceFromNearestIntersection(new Vector3(eventPosition.X, eventPosition.Y, eventPosition.Z), out crosscut, out entry);
                }
                string unit = "(ft)";
                switch (SystemManager.SystemConfig.DistanceUnit)
                {
                    case 1:
                        unit = "(m)";
                        break;
                    case 1.09361f:
                        unit = "(yd)";
                        break;
                    case 3.28084f:
                        unit = "(ft)";
                        break;
                    default:
                        unit = "(ft)";
                        break;
                }
                
                //string meta = eventData.EventData.PositionMetadata;
                //if(eventData.EventData.EventType == VRNLogEventType.PickupObj || eventData.EventData.EventType == VRNLogEventType.DropObj)
                //{
                //    meta += eventData.EventData.ObjectName;
                //}

                //if(meta == "")
                //{
                //    meta = "-";
                //}

                

                //string eventText = "";
                eventSB.Clear();
                eventSB.AppendFormat("•<indent=8%>Player: {0}</indent>\n", playerName);
                eventSB.AppendFormat("•<indent=8%>Event: {0}</indent>\n", ParseEventType(data.EventType));
                eventSB.AppendFormat("•<indent=8%>Time: {0}</indent>\n", time);

                if (!string.IsNullOrEmpty(entry) && !string.IsNullOrEmpty(crosscut))
                {
                    eventSB.AppendFormat("•<indent=8%>Entry {0}, Crosscut {1}</indent>\n", entry, crosscut);
                    eventSB.AppendFormat("•<indent=8%>Distance{0}: {1:F2},{2:F2}</indent>\n", unit, dist.x, dist.y);
                }

                if (!string.IsNullOrEmpty(data.PositionMetadata))
                    eventSB.AppendFormat("•<indent=8%>Details: {0}</indent>\n", data.PositionMetadata);

                if (!string.IsNullOrEmpty(data.ObjectName))
                    eventSB.AppendFormat("•<indent=8%>Object: {0}</indent>\n", data.ObjectName);
                
                if (!string.IsNullOrEmpty(data.Message))
                    eventSB.AppendFormat("•<indent=8%>Message: {0}</indent>\n", data.Message);

                //if (crosscut == "" && entry == "")
                //{
                //    eventText = $"•<indent=8%>Player: {playerName}</indent>\n•<indent=8%>Event: {ParseEventType(eventData.EventData.EventType)}</indent>\n•<indent=8%>Time: {time}</indent>";
                //}
                //else
                //{
                //    eventText = $"•<indent=8%>Player: {playerName}</indent>\n•<indent=8%>Event: {ParseEventType(eventData.EventData.EventType)}</indent>\n•<indent=8%>Time: {time}</indent>\n•<indent=8%>Entry {entry}, Crosscut {crosscut}</indent>\n•<indent=8%>Distance{unit}: {dist.x:F2},{dist.y:F2}</indent>\n•<indent=8%>Details: {meta}</indent>";

                //}

                //if (eventData.EventData.Message != null && eventData.EventData.Message.Length > 0)
                //{
                //    eventText += $"\n•<indent=8%>Message: {eventData.EventData.Message}</indent>";
                //}

                var eventText = eventSB.ToString();

                Button button = dbItem.GetComponent<Button>();
                button.onClick.AddListener(ClickTest);
                if (button != null)
                {
                    button.onClick.AddListener(() => EventTooltip.ActivateTooltip(eventText, dbEventItem));
                }
                

                SessionEventInstanceData instData = new SessionEventInstanceData();
                instData.EventData = eventData;
                instData.DBItem = dbEventItem;
                instData.CategoryData = markerData;
                instData.Player = playerRep;

                SessionEvents[eventData.EventID] = instData;


            }
            catch (Exception ex)
            {
                Debug.LogError($"Error spawning event marker {ex.Message} {ex.StackTrace}");
            }
            
        }

        try
        {
            EventVisibilityChanged?.Invoke();
            TrailVisibilityChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error calling event/trail visibility callback: {ex.Message} {ex.StackTrace}");
        }

        Scrub(0);

        SpawnTeamstops();

    }

    string ParseEventType(VRNLogEventType vRNLogEventType)
    {
        string output = "";

        switch (vRNLogEventType)
        {
            case VRNLogEventType.Message:
                output = "Message";
                break;
            case VRNLogEventType.PickupObj:
                output = "Pickup Object";
                break;
            case VRNLogEventType.DropObj:
                output = "Drop Object";
                break;
            case VRNLogEventType.LinkLineAttach:
                output = "Linkline Attach";
                break;
            case VRNLogEventType.LinkLineDetach:
                output = "Linkline Detach";
                break;
            case VRNLogEventType.DateAndInitial:
                output = "Date and Initial";
                break;
            case VRNLogEventType.DoorOpen:
                output = "Door Open";
                break;
            case VRNLogEventType.DoorClose:
                output = "Door Close";
                break;
            case VRNLogEventType.DoorKnock:
                output = "Knock";
                break;
            case VRNLogEventType.RoofCheck:
                output = "Roof Check";
                break;
            case VRNLogEventType.RibCheck:
                output = "Rib Check";
                break;
            case VRNLogEventType.CurtainHalfHang:
                output = "Curtain Half-hanged";
                break;
            case VRNLogEventType.CurtainFullHang:
                output = "Curtain Fully Hanged";
                break;
            case VRNLogEventType.CurtainRemove:
                output = "Curtain Removed";
                break;
            case VRNLogEventType.RolledCurtainPickedUp:
                output = "Curtain Picked Up";
                break;
            case VRNLogEventType.RolledCurtainDropped:
                output = "Curtain Dropped";
                break;
            case VRNLogEventType.GasCheck:
                output = "Gas Check";
                break;
            case VRNLogEventType.SentinelInspect:
                output = "Sentinel Check";
                break;
            case VRNLogEventType.FireStarted:
                output = "Fire Started";
                break;
            case VRNLogEventType.FireExtinguished:
                output = "Fire Extinguished";
                break;
            case VRNLogEventType.FireExtinguisherDischarge:
                output = "Fire Extinguisher Discharged";
                break;
            case VRNLogEventType.FireExtinguisherPickedUp:
                output = "Fire Extinguisher Picked Up";
                break;
            case VRNLogEventType.FireExtinguisherDropped:
                output = "Fire Extinguisher Dropped";
                break;
            case VRNLogEventType.NpcstartFollow:
                output = "Patient Start Follow";
                break;
            case VRNLogEventType.NpcstopFollow:
                output = "Patient Stop Follow";
                break;
            case VRNLogEventType.NpcstatusUpdate:
                output = "Patient Status Update";
                break;
            case VRNLogEventType.SentinelLow:
                output = "Sentinel Alert";
                break;
            case VRNLogEventType.SentinelEmpty:
                output = "Sentinel Empty";
                break;
            case VRNLogEventType.DmspawnObject:
                output = "DM Spawn Object";
                break;
            case VRNLogEventType.Dmaction:
                output = "DM Action";
                break;
            case VRNLogEventType.EquipmentAdded:
                output = "Equipment Added";
                break;
            case VRNLogEventType.EquipmentRemoved:
                output = "Equipment Removed";
                break;
            case VRNLogEventType.NpcplacedOnStretcher:
                output = "Miner Placed on Stretcher";
                break;
            case VRNLogEventType.NpcremovedFromStretcher:
                output = "Patient Removed from Stretcher";
                break;
            case VRNLogEventType.PostInstall:
                output = "Post Installed";
                break;
            case VRNLogEventType.PostRemoved:
                output = "Post Removed";
                break;
            case VRNLogEventType.MineExplosion:
                output = "Mine Explosion";
                break;
            case VRNLogEventType.ZoneViolation:
                output = "Zone Violation";
                break;
            case VRNLogEventType.Npcdeath:
                output = "Patient Death";
                break;
            case VRNLogEventType.Npcunconscious:
                output = "Patient Unconscious";
                break;
            default:
                output = vRNLogEventType.ToString();
                break;
        }

        return output;
    }
    public bool IsCategoryDisplayed(int categoryID)
    {
        if (categoryID < 0 || categoryID >= DebriefMarkerCategories.EventCategoryData.Count)
            return false;

        var catData = DebriefMarkerCategories.EventCategoryData[categoryID];
        return catData.CategoryVisible;
    }

    public void ShowAllEventCategories(bool show)
    {
        foreach (var catData in DebriefMarkerCategories.EventCategoryData)
        {
            catData.CategoryVisible = show;
        }

        UpdateMarkerVisibility();

        CategoryVisibilityChanged?.Invoke();
    }

    public void ShowEventCategory(int categoryID, bool show)
    {
        Debug.Log($"SessionEventManager: Show event category: {categoryID} : {show}");

        if (categoryID < 0 || categoryID >= DebriefMarkerCategories.EventCategoryData.Count)
            return;

        var catData = DebriefMarkerCategories.EventCategoryData[categoryID];
        catData.CategoryVisible = show;

        UpdateMarkerVisibility();

        CategoryVisibilityChanged?.Invoke();

        //foreach (var data in SessionEvents.Values)
        //{
        //    if (data.CategoryData.EventCategoryIndex == categoryID)
        //    {
        //        if (show && _activeEvents.Contains(data.EventData.EventID))
        //            data.DBItem.EventActivate(true);
        //        else
        //            data.DBItem.EventActivate(false);
        //    }
        //}
    }

    public EventVisibilityData GetPlayerVisibilityData(int playerID)
    {
        EventVisibilityData visData;
        if (_playerVisibility.TryGetValue(playerID, out visData))
            return visData;

        visData = new EventVisibilityData();
        _playerVisibility[playerID] = visData;

        return visData;
    }

    /// <summary>
    /// Do any players have events turned on?
    /// Note: events could still not exist, or be hidden for other reasons
    /// </summary>
    public bool AreAnyPlayerEventsEnabled()
    {
        foreach (var visData in _playerVisibility.Values)
        {
            if (visData.EventsVisible)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Do any players have trails turned on?
    /// Note: trails could still not exist, or be hidden for other reasons
    /// </summary>
    public bool AreAnyPlayerTrailsEnabled()
    {
        foreach (var visData in _playerVisibility.Values)
        {
            if (visData.TrailVisible)
                return true;
        }

        return false;
    }

    public EventVisibilityData GetRoleVisibilityData(VRNPlayerRole role)
    {
        EventVisibilityData visData;
        if (_roleVisibility.TryGetValue(role, out visData))
            return visData;

        visData = new EventVisibilityData();
        _roleVisibility[role] = visData;

        return visData;
    }

    /// <summary>
    /// Do any roles have events turned on?
    /// Note: events could still not exist, or be hidden for other reasons
    /// </summary>
    public bool AreAnyRoleEventsEnabled()
    {
        foreach (var visData in _roleVisibility.Values)
        {
            if (visData.EventsVisible)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Do any roles have trails turned on?
    /// Note: trails could still not exist, or be hidden for other reasons
    /// </summary>
    public bool AreAnyRoleTrailsEnabled()
    {
        foreach (var visData in _roleVisibility.Values)
        {
            if (visData.TrailVisible)
                return true;
        }

        return false;
    }


    public void ShowPlayerEvents(int playerID, bool bShow)
    {
        var visData = GetPlayerVisibilityData(playerID);
        visData.EventsVisible = bShow;

        UpdateMarkerVisibility();
        EventVisibilityChanged?.Invoke();
    }

    public void ShowAllPlayerEvents(bool bShow)
    {
        foreach (var visData in _playerVisibility.Values)
        {
            visData.EventsVisible = bShow;
        }

        UpdateMarkerVisibility();
        EventVisibilityChanged?.Invoke();
    }

    public void ShowPlayerTrail(int playerID, bool bShow)
    {
        var visData = GetPlayerVisibilityData(playerID);
        visData.TrailVisible = bShow;

        TrailVisibilityChanged?.Invoke();
    }

    public void ShowAllPlayerTrails(bool bShow)
    {
        foreach (var visData in _playerVisibility.Values)
        {
            visData.TrailVisible = bShow;
        }

        TrailVisibilityChanged?.Invoke();
    }

    public void ShowRoleEvents(VRNPlayerRole role, bool bShow)
    {
        var visData = GetRoleVisibilityData(role);
        visData.EventsVisible = bShow;

        UpdateMarkerVisibility();
        EventVisibilityChanged?.Invoke();
    }

    public void ShowAllRoleEvents(bool bShow)
    {
        foreach (var visData in _roleVisibility.Values)
        {
            visData.EventsVisible = bShow;
        }

        UpdateMarkerVisibility();
        EventVisibilityChanged?.Invoke();
    }

    public void ShowRoleTrail(VRNPlayerRole role, bool bShow)
    {
        var visData = GetRoleVisibilityData(role);
        visData.TrailVisible = bShow;

        TrailVisibilityChanged?.Invoke();
    }

    public void ShowAllRoleTrails(bool bShow)
    {
        foreach (var visData in _roleVisibility.Values)
        {
            visData.TrailVisible = bShow;
        }

        TrailVisibilityChanged?.Invoke();
    }

    public void UpdateMarkerVisibility()
    {
        foreach (var data in SessionEvents.Values)
        {
            //turn on DM events for now
            if (data.EventData.EventData.SourcePlayerID == 0)
            {
                data.DBItem.EventActivate(true);
                continue;
            }

            if (data.PlayerVisibilityData == null)
                data.PlayerVisibilityData = GetPlayerVisibilityData(data.Player.PlayerID);
            if (data.RoleVisibilityData == null)
                data.RoleVisibilityData = GetRoleVisibilityData(data.Player.PlayerRole);

            if (data.PlayerVisibilityData.EventsVisible && data.RoleVisibilityData.EventsVisible && 
                data.CategoryData.CategoryVisible && _activeEvents.Contains(data.EventData.EventID))
            {
                data.DBItem.EventActivate(true);
            }
            else
            {
                data.DBItem.EventActivate(false);
            }
        }
    }

    public void UpdateMarkerColors()
    {
        foreach (var data in SessionEvents.Values)
        {
            if (data.Player == null)
                continue;

            data.DBItem.PlayerColorImage.color = PlayerColorManager.GetPlayerColor(data.Player.PlayerRole);
        }
    }

    //void SpawnControls()
    //{
    //    foreach (KeyValuePair<int, PlayerRepresentation> player in PlayerManager.PlayerList)
    //    {
    //        GameObject playerToggle = Instantiate(PlayerTogglePrefab, PlayerToggleContainer);
    //        playerToggle.name = player.Value.Name;


    //        //add event?
    //        //Toggle playerToggleBtn = playerToggle.GetComponent<Toggle>();
    //        //playerToggleBtn.onValueChanged.AddListener((value) => PlayerHandler.PlayerItemVisibilty(i, value));
    //        PlayerVisButton pVis = playerToggle.GetComponent<PlayerVisButton>();
    //        TextMeshProUGUI label = playerToggle.GetComponentInChildren<TextMeshProUGUI>();
    //        //label.color = PlayerColorManager.PlayerColors[i];

    //        var playerName = player.Value.Name;

    //        label.text = $"{player.Value.PlayerRole}: {playerName}";
    //        Debug.Log("Player Role: " + player.Value.PlayerRole);
    //        pVis.Index = player.Value.PlayerID;
    //        pVis.PlayerVisibiltyHandler = PlayerHandler;
    //        pVis.SetButtonColor(PlayerColorManager.GetPlayerColor(player.Value.PlayerRole));
    //        if (pVis.PlayerVisibiltyHandler.PlayerToggleBtns == null)
    //            pVis.PlayerVisibiltyHandler.PlayerToggleBtns = new List<Toggle>();
    //        if (pVis.PlayerVisibiltyHandler.PlayerDebriefItems == null)
    //        {
    //            pVis.PlayerVisibiltyHandler.PlayerDebriefItems = new Dictionary<int, List<GameObject>>();
    //        }
    //        pVis.PlayerVisibiltyHandler.PlayerToggleBtns.Add(pVis.EventToggle);
    //        pVis.PathToggle.onValueChanged.AddListener((value) => SessionPlaybackControl.ShowPlayerTrack(player.Value.PlayerID, pVis.PathToggle.isOn));

    //        //player.Value.PlayerColorChanged += (newColor) => {
    //        //    pVis.SetButtonColor(newColor);
    //        //};



    //        player.Value.PlayerRoleChanged += (newRole) => {
    //            pVis.SetButtonColor(PlayerColorManager.GetPlayerColor(newRole));
    //            label.text = $"{newRole}: {playerName}";
    //            UpdateMarkerColors();
    //        };
    //    }
    //}

    public void ClearTeamstops()
    {
        foreach (Transform child in TeamstopContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform ch2 in TeamstopContainerScrn2)
        {
            Destroy(ch2.gameObject);
        }
        Debug.Log("Cleared teamstops!");

        if (TimelineController.Teamstops == null)
        {
            TimelineController.Teamstops = new List<SessionTeamstopState>();
        }
        else
        {
            TimelineController.Teamstops.Clear();
        }
    }

    public void SpawnTeamstops()
    {
        float width = TeamstopContainer.rect.width;
        float startTime = SessionPlaybackControl.CurrentSessionLog.StartTime;
        float endTime = SessionPlaybackControl.CurrentSessionLog.EndTime;
        float logDuration = SessionPlaybackControl.CurrentSessionLog.Duration;
        float timeToSliderWidthRatio = width / logDuration;
        int colorIndex = 0;
        List<RectTransform> Labels = new List<RectTransform>();
        List<RectTransform> LabelsScrn2 = new List<RectTransform>();
        
        //foreach(Transform child in TeamstopContainer)
        //{
        //    Destroy(child.gameObject);
        //}
        //foreach(Transform ch2 in TeamstopContainerScrn2)
        //{
        //    Destroy(ch2.gameObject);
        //}
        //Debug.Log("Cleared teamstops!");
        
        //if(TimelineController.Teamstops == null)
        //{
        //    TimelineController.Teamstops = new List<SessionTeamstopState>();
        //}
        //else
        //{
        //    TimelineController.Teamstops.Clear();
        //}
        foreach (var teamstop in SessionPlaybackControl.CurrentSessionLog.GetTeamstops())
        {
            Debug.Log($"Teamstop {teamstop.TeamstopIndex} spawned at time {teamstop.TeamstopStartTime}");
            GameObject obj = Instantiate(TeamstopPrefab, TeamstopContainer);
            GameObject obj2 = Instantiate(TeamstopPrefab, TeamstopContainerScrn2);
            RectTransform tsTransform = obj.GetComponent<RectTransform>();
            RectTransform tsTransform2 = obj2.GetComponent<RectTransform>();
            TimelineController.Teamstops.Add(teamstop);
            Vector2 position;
            float sTime = 0;
            if (teamstop.TeamstopStartTime != 0)
            {
                position = new Vector2((teamstop.TeamstopStartTime - startTime) * timeToSliderWidthRatio, tsTransform.anchoredPosition.y);
                sTime = teamstop.TeamstopStartTime;
            }
            else
            {
                position = new Vector2(0, tsTransform.anchoredPosition.y);
                sTime = SessionPlaybackControl.CurrentSessionLog.StartTime;
            }
            tsTransform.anchoredPosition = position;
            tsTransform2.anchoredPosition = position;
            Vector2 sizeDelta = tsTransform.sizeDelta;
            if (teamstop.TeamstopEndTime != -1)
            {
                sizeDelta.x = (teamstop.TeamstopEndTime - sTime) * timeToSliderWidthRatio;
            }
            else
            {
                sizeDelta.x = width - tsTransform.anchoredPosition.x;
            }
            tsTransform.sizeDelta = sizeDelta;
            tsTransform2.sizeDelta = sizeDelta;
            TextMeshProUGUI label = obj.GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI label2 = obj2.GetComponentInChildren<TextMeshProUGUI>();
            label.text = (teamstop.TeamstopIndex + 1).ToString();
            label2.text = (teamstop.TeamstopIndex + 1).ToString();
            Debug.Log($"Teamstop: {teamstop.TeamstopIndex}, {teamstop.TeamstopStartTime} - {teamstop.TeamstopEndTime}");
            Image tsImage = obj.GetComponent<Image>();
            Image tsImage2 = obj2.GetComponent<Image>();
            Labels.Add(label.rectTransform);
            LabelsScrn2.Add(label2.rectTransform);
            if (colorIndex == TSColors.TeamstopColors.Length)
            {
                colorIndex = 0;
            }
            tsImage.color = TSColors.TeamstopColors[colorIndex];
            tsImage2.color = TSColors.TeamstopColors[colorIndex];
            colorIndex++;
        }
        foreach (RectTransform label in Labels)
        {
            label.SetParent(TimelineHorizontalBarTransform, true);
        }

        foreach(RectTransform label in LabelsScrn2)
        {
            label.SetParent(TimelineHorizontalBarTransformScrn2, true);
        }
    }

    public void UpdateScale(float normalizedScale)
    {
        //float scale = Mathf.Lerp(0.01f, 0.002f, normalizedScale);
        float scale = Mathf.Lerp(0.01f, 0.002f  * 3.0f, normalizedScale);
        //foreach (Transform item in SpawnedItems)
        foreach (var instData in SessionEvents.Values)
        {
            instData.DBItem.transform.localScale = new Vector3(scale, scale, scale);
            //item.localScale = new Vector3(scale, scale, scale);
        }

        if (HighlightCircle != null)
            HighlightCircle.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void ShowHighlightCircle()
    {
        HighlightCircle.SetActive(true);
    }

    public void HideHighlightCircle()
    {
        HighlightCircle.SetActive(false);
    }

    public void PositionHighlightCircle(Vector2 position)
    {
        HighlightCircle.transform.position = new Vector3(position.x, 30, position.y);
    }

    private void OnDestroy()
    {
        Debug.Log("Scene loader event removed in Session Event Manager");
        DebriefSceneLoader.SceneLoaded -= OnSceneLoad;
    }

    public void ClickTest()
    {
        Debug.Log("Item clicked");
    }

    public void Scrub(float time)
    {
        //List<SessionEventData> activeEvents = new List<SessionEventData>();
        //HashSet<int> activeEvents = new HashSet<int>();
        _activeEvents.Clear();

        foreach (var dbEvent in SessionPlaybackControl.CurrentSessionLog.GetActiveEvents(EventStartTime, EventEndTime))//var dbEvent in SessionPlaybackControl.CurrentSessionLog.GetEventsFromTime(EventStartTime)
        {
            //activeEvents.Add(dbEvent);
            _activeEvents.Add(dbEvent.EventID);
        }

        UpdateMarkerVisibility();

        //foreach (KeyValuePair<SessionEventData, DebriefEventItem> kvp in AllEvents)

        //foreach (var instData in SessionEvents.Values)
        //{
        //    if (!instData.CategoryData.CategoryVisible)
        //        continue;

        //    instData.DBItem.EventActivate(_activeEvents.Contains(instData.EventData.EventID));
        //}
    }
}
