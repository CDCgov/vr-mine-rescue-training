using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ProtoBuf;
using ProtoBuf.Serializers;
using Google.Protobuf;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class SessionLog : IDisposable
{
    public const float TimestampBinSize = (1.0f / 30.0f);

    public struct SessionLogMetadata
    {
        public string Filename;
        public string SceneName;
        public string SessionName;
        public string TeleportTarget;
        public DateTime? LogStartTime;
        public float Duration;
        public int NumMessages;
    }

    public int MessageCount
    {
        get { return _messageCount; }
    }

    public string LogFileName
    {
        get { return _metadata.Filename; }
    }

    public string SessionName
    {
        get { return _metadata.SessionName; }
    }

    public string SceneName
    {
        get { return _metadata.SceneName; }
    }

    public DateTime? SessionDateTime
    {
        get { return _metadata.LogStartTime; }
    }

    public float CurrentTime;
    public float EventTimeWindow;

    public VRNLogHeader LogHeader;
    public VRNLogFooter LogFooter;
    public SessionNetObjState CurrentNetObjState;
    public SessionVentilationState CurrentVentilationState;
    //public string CurrentTeleportTarget;
    public SessionTeamstopState CurrentTeamstopState;
    public Dictionary<int, PlayerRepresentation> CurrentPlayerData;
    private SessionLogMetadata _metadata;

    private Dictionary<VRNPacketType, int> _packetStats;

    public float Duration
    {
        get
        {
            float duration = _lastTimestamp - _firstTimestamp;
            if (duration < 0)
                duration = 0;
            return duration;
        }
    }

    public float StartTime
    {
        get { return _firstTimestamp; }
    }

    public float EndTime
    {
        get { return _lastTimestamp; }
    }

    private FileStream _fileStream;
    private bool _openForWriting;
    private bool _openForReading;
    private MemoryStream _buffer;

    VRNHeader _vrnHeader;
    private bool _disposed;
    private int _messageCount = 0;
    private float _firstTimestamp = 0;
    private float _lastTimestamp = 0;
    private int _logEventID = 0;
    private int _ventSateIndex = 0;
    private string _curTeleportPoint;
    private int _curTeamstopIndex = 0;

    private bool _randomizePlayerNames = false;

    VRNLogHeader _logHeader;

    private Dictionary<int, SessionTimeSeries<PlayerRepresentation>> _playerData;
    private Dictionary<int, SessionTimeSeries<SessionPlayerTrackData>> _playerTrackData;
    private SessionTimeSeries<SessionTeamstopState> _sessionTeamstopState;
    private SessionTimeSeries<SessionNetObjState> _sessionNetObjState;
    private SessionTimeSeries<SessionEventData> _sessionEvents;
    private SessionTimeSeries<SessionVentilationState> _sessionVentState;
    //private SessionTimeSeries<MineMapData> _sessionMineMapState;

    private CancellationTokenSource _cancelLoad;

    private Transform _calcPOI;
    private Transform _calcRig;
    private Transform _calcOffset;
    private Transform _calcHead;

    private Dictionary<int, VRNCalibrationOffsetData> _calDataCache;

    public int NumPlayers
    {
        get
        {
            if (_playerData == null)
                return 0;

            return _playerData.Count;
        }
    }

    //public static List<SessionLogMetadata> ScanDefaultFolder()
    //{
    //    return SessionLog.ScanFolder(null);
    //}

    public static List<SessionLogMetadata> ScanFolder(string path)
    {
        if (path == null || path.Length <= 0)
        {
            //path = GetDefaultLogPath();
            //path = SystemConfig.GetDefaultSessionLogPath();

            Debug.LogError("SessionLog: Error, no path specefied for session log folder scan");
            return null;
        }

        var files = Directory.EnumerateFiles(path, "*.vrminelog");

        var logs = new List<SessionLogMetadata>();

        foreach (var file in files)
        {
            //Debug.Log($"Found log file {file}");

            try
            {
                var meta = ReadMetadata(file);
                logs.Add(meta);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"SessionLog: Couldn't read log file {file} {ex.Message}");
            }

        }

        //foreach (var log in logs.OrderBy(l => l.LogStartTime))
        //{

        //}

        return logs;
    }

    public static SessionLogMetadata ReadMetadata(string filename)
    {
        SessionLogMetadata meta = new SessionLogMetadata();

        using (FileStream fs = new FileStream(filename, FileMode.Open))
        {
            var header = VRNLogHeader.Parser.ParseDelimitedFrom(fs);

            meta.Filename = Path.GetFileName(filename);
            meta.SessionName = header.SessionName;
            meta.SceneName = header.ActiveScene;
            meta.TeleportTarget = header.TeleportTarget;
            meta.LogStartTime = null;
            if (header.LogStartTime != null)
            {
                var utcTime = header.LogStartTime.ToDateTime();
                //convert to local time                
                meta.LogStartTime = utcTime.ToLocalTime();
            }

            try
            {
                //var messageCountBytes = System.BitConverter.GetBytes(_messageCount);
                //_fileStream.Write(messageCountBytes, 0, messageCountBytes.Length);

                var footerPosBytes = new byte[sizeof(long)];
                //attempt to read log footer
                fs.Seek(sizeof(long) * -1, SeekOrigin.End);
                if (fs.Read(footerPosBytes, 0, sizeof(long)) != sizeof(long))
                    throw new Exception("Couldn't read footer position");

                var footerPos = BitConverter.ToInt64(footerPosBytes, 0);

                if (footerPos > fs.Length || footerPos < 0)
                    throw new Exception($"Footer outside valid range");

                int footerSize = (int)(fs.Length - footerPos);
                if (footerSize > 2048)
                    throw new Exception("Footer too big");

                fs.Seek(footerPos, SeekOrigin.Begin);

                var footerHeader = VRNHeader.Parser.ParseDelimitedFrom(fs);
                var footer = VRNLogFooter.Parser.ParseDelimitedFrom(fs);

                if (footerHeader.PacketType == VRNPacketType.LogFooter)
                {
                    meta.NumMessages = footer.NumMessages;
                    meta.Duration = footer.SessionDuration;
                }

            }
            catch (Exception) { }
        }

        return meta;
    }

    //public static string GetDefaultLogPath()
    //{
    //    var mydocs = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    //    var folder = Path.Combine(mydocs, "VRMineSessionLogs");
    //    Directory.CreateDirectory(folder);

    //    return folder;
    //}

    public SessionLog(bool randomizePlayerNames) : this()
    {
        _randomizePlayerNames = randomizePlayerNames;
    }


    public SessionLog()
    {
        _vrnHeader = new VRNHeader();
        _buffer = new MemoryStream(NetworkManager.MaxPayloadSize);
        _playerData = new Dictionary<int, SessionTimeSeries<PlayerRepresentation>>();
        _playerTrackData = new Dictionary<int, SessionTimeSeries<SessionPlayerTrackData>>();
        _sessionTeamstopState = new SessionTimeSeries<SessionTeamstopState>();
        _sessionNetObjState = new SessionTimeSeries<SessionNetObjState>();
        _sessionEvents = new SessionTimeSeries<SessionEventData>();
        _sessionVentState = new SessionTimeSeries<SessionVentilationState>();
        //_sessionMineMapState = new SessionTimeSeries<MineMapData>();

        CurrentPlayerData = new Dictionary<int, PlayerRepresentation>();

        _calDataCache = new Dictionary<int, VRNCalibrationOffsetData>();
    }

    public static string GenerateFilename(string sceneName, string sessionName, string sessionLogPath)
    {
        if (sceneName.Contains("CustomScenario:"))
        {
            Regex reg = new Regex("CustomScenario:([^\\.]+)\\.");
            var match = reg.Match(sceneName);
            if (!match.Success || match.Groups.Count < 2)
                sceneName = "Unknown";
            else
            {
                sceneName = match.Groups[1].Value;
            }
        }

        string filename;
        var date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        if (sessionName == null || sessionName.Length <= 0)
            filename = $"SessionLog-{date}-{sceneName}.vrminelog";
        else
            filename = $"{sessionName}-{date}-{sceneName}.vrminelog";

        //var folder = SystemConfig.GetDefaultSessionLogPath();//GetDefaultLogPath();

        filename = Path.Combine(sessionLogPath, filename);

        return filename;
    }

    public void CreateLog(string filename, VRNLogHeader logHeader)
    {
        _openForReading = _openForWriting = false;
        if (_fileStream != null)
        {
            _fileStream.Dispose();
            _fileStream = null;
        }

        _firstTimestamp = -1;
        _lastTimestamp = -1;

        _fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
        _openForWriting = true;

        logHeader.WriteDelimitedTo(_fileStream);
    }

    //public static async Task<SessionLog> LoadLogAsync(string filename)
    //{

    //}

    public static async Task<SessionLog> LoadLogAsync(string filename, Action<float> progressCallback, CancellationToken cancelToken, bool randomizePlayerNames)
    {
        SessionLog log = new SessionLog(randomizePlayerNames);

        await Task.Run(() => { log.LoadLog(filename, progressCallback, cancelToken); });

        return log;
    }

    //public async Task<bool> LoadLogAsync()
    //{ 
    //}

    public void LoadLog(string filename, Action<float> progressCallback = null, CancellationToken cancelToken = default)
    {
        try
        {
            _calDataCache.Clear();

            var metadata = ReadMetadata(filename);
            _metadata = metadata;
            float lastProgress = 0;

            _packetStats = new Dictionary<VRNPacketType, int>();

            _fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            _openForWriting = false;
            _openForReading = true;
            _messageCount = 0;
            _lastTimestamp = 0;
            _firstTimestamp = -1;
            _logEventID = 0;
            _ventSateIndex = 0;

            LogHeader = VRNLogHeader.Parser.ParseDelimitedFrom(_fileStream);

            Debug.Log($"Read log header scene {LogHeader.ActiveScene} teleport {LogHeader.TeleportTarget} session {LogHeader.SessionName}");

            //add starting teamstop entry
            SessionTeamstopState initialTeamstop = new SessionTeamstopState();
            initialTeamstop.TeamstopIndex = 0;
            initialTeamstop.TeamstopStartTime = 0;
            initialTeamstop.TeamstopEndTime = -1;
            initialTeamstop.TeleportTarget = LogHeader.TeleportTarget;
            _sessionTeamstopState.AddSequentialEntry(initialTeamstop, 0);

            _curTeleportPoint = initialTeamstop.TeleportTarget;
            _curTeamstopIndex = 0;

            while (ReadNextEntry())
            {
                _messageCount++;
                if (cancelToken.IsCancellationRequested)
                    throw new TaskCanceledException("SessionLog::LoadLog Cancelled");

                if (metadata.NumMessages <= 0)
                    continue;

                float progress = (float)_messageCount / (float)metadata.NumMessages;
                if (progressCallback != null && progress - lastProgress > 0.01f)
                {
                    lastProgress = progress;
                    progressCallback(progress);
                }
            }

            BuildPlayerTracks();

            Debug.Log($"SessionLog: loaded session packet stats:");
            foreach (var kvp in _packetStats)
            {
                Debug.Log($"{kvp.Key} : {kvp.Value}");
            }

            if (progressCallback != null)
                progressCallback(1.0f);
        }
        catch (Exception ex) when (!(ex is TaskCanceledException))
        {
            Debug.LogError($"Couldn't open log file: {ex.Message}");
        }
        finally
        {
            if (_fileStream != null)
                _fileStream.Dispose();
            _fileStream = null;
        }

        Debug.Log($"SessionLog: Read {_messageCount} messages from {filename}");

    }

    public void Scrub(float timestamp)
    {
        CurrentTime = timestamp;
        foreach (var kvp in _playerData)
        {
            PlayerRepresentation player;
            //if (!CurrentPlayerData.TryGetValue(kvp.Key, out player)) 
            //    player = new PlayerRepresentation();

            //kvp.Value.InterpolateData(timestamp, ref player);
            player = kvp.Value.GetClosestData(timestamp);

            if (player == null)
                player = new PlayerRepresentation();
            CurrentPlayerData[kvp.Key] = player;
        }

        //if (CurrentNetObjState == null)
        //    CurrentNetObjState = new SessionNetObjState();

        //if (!_sessionNetObjState.InterpolateData(timestamp, ref CurrentNetObjState))
        //    CurrentNetObjState = null;
        CurrentNetObjState = _sessionNetObjState.GetClosestData(timestamp);

        var tsState = _sessionTeamstopState.GetClosestData(timestamp);
        if (tsState != null)
        {
            //CurrentTeleportTarget = tsState.TeleportTarget;
            CurrentTeamstopState = tsState;
        }

        CurrentVentilationState = _sessionVentState.GetClosestData(timestamp);
    }

    public PlayerRepresentation GetPlayerRep(int playerID, float timestamp)
    {
        if (_playerData == null || _playerData.Count <= 0)
            return null;

        SessionTimeSeries<PlayerRepresentation> playerSeries;
        if (!_playerData.TryGetValue(playerID, out playerSeries))
            return null;

        return playerSeries.GetClosestData(timestamp);
    }

    public float GetWindowStart()
    {
        var windowStart = CurrentTime - EventTimeWindow;
        if (windowStart < StartTime)
            windowStart = StartTime;

        //testing
        windowStart = 0;

        return windowStart;
    }

    public float GetWindowEnd()
    {
        return CurrentTime;
    }

    public IEnumerable<SessionEventData> GetActiveEvents()
    {
        var windowStart = GetWindowStart();
        var windowEnd = GetWindowEnd();

        int startIndex = _sessionEvents.GetClosestIndex(windowStart);

        int index = startIndex;
        while (true)
        {
            var eventData = _sessionEvents.GetData(index);
            if (eventData == null)
                break;

            yield return eventData;

            index++;
            var timestamp = _sessionEvents.GetTimestamp(index);
            if (timestamp >= windowEnd)
                break;
        }
    }

    public IEnumerable<SessionPlayerTrackData> GetPlayerTrack(int playerID, float windowStart = -1, float windowEnd = -1)
    {
        SessionTimeSeries<SessionPlayerTrackData> playerTrack;
        if (!_playerTrackData.TryGetValue(playerID, out playerTrack))
            yield break;

        if (windowStart < 0)
            windowStart = GetWindowStart();
        if (windowEnd < 0)
            windowEnd = GetWindowEnd();

        int startIndex = playerTrack.GetClosestIndex(windowStart);
        int index = startIndex;
        while (true)
        {
            var trackData = playerTrack.GetData(index);
            if (trackData == null)
                break;

            yield return trackData;

            index++;
            var timestamp = playerTrack.GetTimestamp(index);
            if (timestamp >= windowEnd)
                break;
        }


    }

    public IEnumerable<SessionTeamstopState> GetTeamstopsInWindow()
    {
        if (_sessionTeamstopState == null)
            yield break;

        var windowStart = GetWindowStart();
        var windowEnd = GetWindowEnd();

        int index = 0;
        while (true)
        {
            var tsState = _sessionTeamstopState.GetData(index);
            if (tsState == null)
                break;

            index++;

            if (tsState.TeamstopStartTime > windowStart &&
                tsState.TeamstopEndTime < windowEnd)
            {
                yield return tsState;
            }
        }

    }

    public IEnumerable<SessionTeamstopState> GetTeamstops()
    {
        if (_sessionTeamstopState == null)
            yield break;

        int index = 0;
        while (true)
        {
            var tsState = _sessionTeamstopState.GetData(index);
            if (tsState == null)
                break;

            index++;
            yield return tsState;
        }
    }

    public IEnumerable<SessionEventData> GetEventsFromTime(float time)
    {
        var windowStart = time;
        if (windowStart < StartTime)
            windowStart = StartTime;

        int startIndex = _sessionEvents.GetClosestIndex(windowStart);

        int index = startIndex;
        while (true)
        {
            var eventData = _sessionEvents.GetData(index);
            if (eventData == null)
                break;

            yield return eventData;

            index++;
            var timestamp = _sessionEvents.GetTimestamp(index);
            if (timestamp >= CurrentTime)
                break;
        }
    }

    public IEnumerable<SessionEventData> GetActiveEvents(float start, float end)
    {
        var windowStart = start;
        if (windowStart < StartTime)
        {
            windowStart = StartTime;
        }
        var windowEnd = end;
        if (windowEnd > EndTime)
        {
            windowEnd = EndTime;
        }

        int startIndex = _sessionEvents.GetClosestIndex(windowStart);
        int index = startIndex;
        while (true)
        {
            var eventData = _sessionEvents.GetData(index);
            if (eventData == null)
                break;
            index++;

            //closest index might be (almost always is) before the window start, or after the end, check timestamp
            if (eventData.EventData.Timestamp < start || eventData.EventData.Timestamp > end)
                continue;

            yield return eventData;
            
            var timestamp = _sessionEvents.GetTimestamp(index);
            if (timestamp >= windowEnd)
                break;
        }

    }
    //Used to instantiate events at scene load
    public IEnumerable<SessionEventData> GetAllEvents()
    {
        var windowStart = StartTime;
        int startIndex = _sessionEvents.GetClosestIndex(windowStart);
        int index = startIndex;
        while (true)
        {
            var eventData = _sessionEvents.GetData(index);
            if (eventData == null)
                break;

            yield return eventData;

            index++;
            var timestamp = _sessionEvents.GetTimestamp(index);
            if (timestamp >= EndTime)
                break;
        }
    }

    private void AddPlayerData(VRNHeader header, VRNVRPlayerInfo info)
    {
        SessionTimeSeries<PlayerRepresentation> playerData;
        if (!_playerData.TryGetValue(info.PlayerID, out playerData))
        {
            playerData = new SessionTimeSeries<PlayerRepresentation>();
            _playerData.Add(info.PlayerID, playerData);
        }
        

        PlayerRepresentation pr = new PlayerRepresentation();
        pr.UpdateVRClientData(info);
        pr.PlayerID = info.PlayerID;
        pr.ClientID = info.ClientID;

        pr.CalibrationPos = Vector3.zero;
        pr.CalibrationRot = Quaternion.identity;

        if (_randomizePlayerNames)
            pr.Name = $"M{pr.PlayerID}";
        else
            pr.Name = info.Name;

        pr.PlayerRole = info.Role;
        pr.LastDataTimestamp = header.ServerTime;

        if (pr.Name == null)
            pr.Name = "Unknown";

        var lastPlayerData = playerData.GetLastData();
        if (lastPlayerData != null)
        {
            pr.CalibrationPos = lastPlayerData.CalibrationPos;
            pr.CalibrationRot = lastPlayerData.CalibrationRot;
        }
        else
        {
            //check for cached calibration data
            VRNCalibrationOffsetData calData;
            if (_calDataCache.TryGetValue(info.PlayerID, out calData))
            {
                //if (AddCalibrationData(calData))
                //    _calDataCache.Remove(info.PlayerID);
                pr.CalibrationPos = calData.OffsetPos.ToVector3();
                pr.CalibrationRot = calData.OffsetRot.ToQuaternion();
            }
        }

        playerData.AddSequentialEntry(pr, header.ServerTime);
    }

    private void BuildPlayerTracks()
    {
        foreach (var kvp in _playerData)
        {
            var playerID = kvp.Key;
            var playerTimeSeries = kvp.Value;

            SessionTimeSeries<SessionPlayerTrackData> playerTrack = new SessionTimeSeries<SessionPlayerTrackData>();
            _playerTrackData[playerID] = playerTrack;

            SessionPlayerTrackData lastTrackData = null;

            foreach (var prKVP in playerTimeSeries.GetAllData())
            {
                var pr = prKVP.Value;
                var timestamp = prKVP.Key;

                try
                {
                    //calculate position in the space of the POI anchor
                    //transform hierarchy is (POI Anchor-> Rig -> CalOffset-> HeadPos)
                    var offsetMat = Matrix4x4.TRS(pr.CalibrationPos, pr.CalibrationRot, Vector3.one);
                    var rigMat = Matrix4x4.TRS(pr.RigOffset.Position, pr.RigOffset.Rotation, Vector3.one);

                    var matToAnchor = rigMat * offsetMat;

                    //var newPos = info.Head.Position.ToVector3();
                    var newPos = pr.Head.Position;
                    newPos = matToAnchor.MultiplyPoint(newPos);

                    var ts = _sessionTeamstopState.GetClosestData(timestamp);

                    if (lastTrackData == null || Vector3.Distance(lastTrackData.Position, newPos) > 0.2f
                        || lastTrackData.TeleportPoint != ts.TeleportTarget)
                    {

                        //position has changed sufficiently to need a new entry
                        var trackData = new SessionPlayerTrackData();
                        trackData.Position = newPos;
                        //trackData.AnchorSpacePosition = matToAnchor.MultiplyPoint(newPos);
                        trackData.TeleportPoint = ts.TeleportTarget;
                        trackData.TeamstopIndex = ts.TeamstopIndex;
                        playerTrack.AddSequentialEntry(trackData, timestamp);

                        lastTrackData = trackData;
                    }
                }
                catch (Exception)
                {
                    //catch any calculation errors, e.g. invalid quaternion, and ignore that point
                }                
            }
        }
    }

    private void AddCalibrationData(VRNHeader header, VRNCalibrationOffsetData calData)
    {
        AddCalibrationData(header.ServerTime, calData);
    }

    private bool AddCalibrationData(float timestamp, VRNCalibrationOffsetData calData)
    {
        SessionTimeSeries<PlayerRepresentation> playerData;
        if (!_playerData.TryGetValue(calData.PlayerID, out playerData))
        {
            _calDataCache[calData.PlayerID] = calData;
            Debug.Log($"Adding cal data for unknown player {calData.PlayerID} to the cache");
            //Debug.LogError($"SessionLog: Parsed cal data for unknown player {calData.PlayerID}");
            return false;
        }

        var playerTimestamp = playerData.GetLastTimestamp();
        var player = playerData.GetLastData();
        if (player == null)
        {
            _calDataCache[calData.PlayerID] = calData;
            Debug.Log($"Adding cal data for player with missing data {calData.PlayerID} to the cache");
            return false;
        }

        player.CalibrationPos = calData.OffsetPos.ToVector3();
        player.CalibrationRot = calData.OffsetRot.ToQuaternion();
         
        foreach (var priorPlayerData in GetPriorPlayerData(calData.PlayerID, playerTimestamp))
        {
            if (priorPlayerData.CalibrationPos != Vector3.zero)
                break;

            priorPlayerData.CalibrationPos = player.CalibrationPos;
            priorPlayerData.CalibrationRot = player.CalibrationRot;
        }

        return true;
    }

    private void AddTeleportData(VRNHeader header, VRNTeleportAll tele)
    {
        var lastEntry = _sessionTeamstopState.GetLastData();

        //don't add a new entry if its a repeated teleport to the current location
        if (lastEntry.TeleportTarget == tele.TeleportTarget)
            return;

        //update previous teamstops end time. list should never be empty due to inital teamstop entry
        lastEntry.TeamstopEndTime = header.ServerTime;

        SessionTeamstopState state = new SessionTeamstopState();
        state.TeamstopIndex = lastEntry.TeamstopIndex + 1;
        state.TeleportTarget = tele.TeleportTarget;
        state.TeamstopStartTime = header.ServerTime;
        state.TeamstopEndTime = -1;

        _sessionTeamstopState.AddSequentialEntry(state, header.ServerTime);
        _curTeleportPoint = state.TeleportTarget;
        _curTeamstopIndex = state.TeamstopIndex;
    }

    private void ReadSyncObjData(VRNHeader header, Stream fileStream)
    {
        string currentObjID = null;

        try
        {
            var filepos = fileStream.Position;

            var netObjSync = VRNNetObjSync.Parser.ParseDelimitedFrom(fileStream);
            if (netObjSync.ObjID.Length != 16)
                netObjSync = VRNNetObjSync.Parser.ParseDelimitedFrom(fileStream);
            if (netObjSync.ObjID.Length != 16)
                return;

            currentObjID = netObjSync.ObjID.ToGuid().ToString();

            //compute size of sync data portion of message
            var syncSize = header.PacketSize - (int)(fileStream.Position - filepos);

            if (syncSize <= 0)
                return;

            //read sync data into buffer
            byte[] buffer = new byte[syncSize];
            int bytesRead = fileStream.Read(buffer, 0, syncSize);
            if (bytesRead != syncSize)
                return;

            MemoryStream syncData = new MemoryStream(buffer);

            AddSyncObjData(header.ServerTime, netObjSync.ObjID.ToGuid(), syncData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"SessionLog: Parse error reading sync data for {currentObjID} {ex.Message}");
        }
    }

    private void ReadNetObjMessage(VRNHeader header, Stream fileStream)
    {
        string objIDString = null;

        try
        {
            var filepos = fileStream.Position;

            var netObjMessage = VRNNetObjMessage.Parser.ParseDelimitedFrom(fileStream);
            if (netObjMessage.ObjID.Length != 16)
            {
                Debug.LogWarning($"SessionLog: Bad NetObjMessage header");
                return;
            }
            var objID = netObjMessage.ObjID.ToGuid();
            objIDString = objID.ToString();

            if (netObjMessage.MessageType != "TIMER_SYNC" && netObjMessage.MessageType != "EMIT")
            {
                //for now just try to parse timer sync messages
                return;
            }

            SessionNetObjState.NetObjState objState;
            SessionNetObjState sessionState = GetNetObjState(header.ServerTime);

            if (!sessionState.Objects.TryGetValue(objID, out objState))
            {
                objState = new SessionNetObjState.NetObjState();
                objState.ObjectAlive = true;
            }


            //keep last message
            //if (objState.NetObjMessageHeader != null || objState.NetObjMessageBuffer != null)
            //{
            //    //keep only 1 message per time window for now
            //    return;
            //}

            //compute size of sync data portion of message
            var msgSize = header.PacketSize - (int)(fileStream.Position - filepos);
            if (msgSize <= 0)
                return;

            //read sync data into buffer
            byte[] buffer = new byte[msgSize];
            int bytesRead = fileStream.Read(buffer, 0, msgSize);
            if (bytesRead != msgSize)
                return;

            //CodedInputStream msgStream = new CodedInputStream(buffer);

            objState.NetObjMessage = netObjMessage.Clone();
            objState.NetObjMessageHeader = header.Clone();
            objState.NetObjMessageBuffer = buffer;
            sessionState.Objects[objID] = objState;
        }
        catch (Exception ex)
        {
            Debug.LogError($"SessionLog: Error reading NetObjMessage for {objIDString} {ex.Message}");
        }
    }

    private SessionNetObjState GetNetObjState(float timestamp)
    {
        SessionNetObjState sessionState;

        float timestampBin = _sessionNetObjState.ComputeTimestampBin(timestamp, TimestampBinSize);

        float lastTimestamp = _sessionNetObjState.GetLastTimestamp();
        if (timestampBin > lastTimestamp)
        {
            //new timestamp bin
            sessionState = new SessionNetObjState();

            //copy latest state for each object forward
            var oldState = _sessionNetObjState.GetLastData();
            if (oldState != null)
            {
                oldState.CopyTo(sessionState, false);

                //clear net object messages
                //sessionState.ClearNetObjMessages();
            }

            _sessionNetObjState.AddSequentialEntry(sessionState, timestampBin);
        }
        else
        {
            //use existing latest bin
            sessionState = _sessionNetObjState.GetLastData();
        }

        return sessionState;
    }



    private void AddSyncObjData(float timestamp, Guid objID, MemoryStream syncData)
    {

        SessionNetObjState.NetObjState objState;
        SessionNetObjState sessionState = GetNetObjState(timestamp);

        if (!sessionState.Objects.TryGetValue(objID, out objState))
        {
            objState = new SessionNetObjState.NetObjState();
        }
        else if (!objState.ObjectAlive)
        {
            //ignore sync from destroyed objects
            return;
        }

        objState.SyncData = syncData;
        objState.ObjectAlive = true;
        sessionState.Objects[objID] = objState;
    }

    private void AddNetworkedObjectListData(VRNHeader header, VRNNetworkedObjectList netObjList)
    {
        foreach (VRNSpawnObject spawnObj in netObjList.ObjectList)
        {
            AddSpawnObjData(header, spawnObj);
        }

        foreach (var destroyedObj in netObjList.DestroyedSceneObjects)
        {
            AddDestroyObjData(header, destroyedObj.ToGuid());
        }
    }

    private IEnumerable<SessionNetObjState> GetPriorNetObjState(float timestamp)
    {
        float timestampBin = _sessionNetObjState.ComputeTimestampBin(timestamp, TimestampBinSize);
        int index = _sessionNetObjState.GetClosestIndex(timestampBin);

        for (int i = index - 1; i >= 0; i--)
        {
            var sessionState = _sessionNetObjState.GetData(i);
            if (sessionState == null)
                break;

            yield return sessionState;
        }
    }

    private IEnumerable<PlayerRepresentation> GetPriorPlayerData(int playerID, float timestamp)
    {
        SessionTimeSeries<PlayerRepresentation> playerTimeSeries;
        if (_playerData.TryGetValue(playerID, out playerTimeSeries))
        {
            //float timestampBin = playerTimeSeries.ComputeTimestampBin(timestamp, TimestampBinSize);
            int index = playerTimeSeries.GetClosestIndex(timestamp);

            for (int i = index - 1; i >= 0; i--)
            {
                var playerRep = playerTimeSeries.GetData(i);
                if (playerRep == null)
                    break;

                yield return playerRep;
            }
        }
    }

    private void AddSpawnObjData(VRNHeader header, VRNSpawnObject spawnObj)
    {
        float timestamp = header.ServerTime;
        var objID = spawnObj.ObjID.ToGuid();

        SessionNetObjState.NetObjState objState;
        SessionNetObjState sessionState = GetNetObjState(timestamp);

        spawnObj.SpawnActivated = true;

        if (!sessionState.Objects.TryGetValue(objID, out objState))
        {
            objState = new SessionNetObjState.NetObjState();
        }

        objState.SpawnData = spawnObj;
        //objState.ObjectAlive = spawnObj.SpawnActivated;
        objState.ObjectAlive = true;

        sessionState.Objects[objID] = objState;

        //propagate the fact that the object didn't exist before this entry
        //backward through time
        foreach (var priorSessionState in GetPriorNetObjState(timestamp))
        {
            //only change the value if the object didn't exist previously
            if (!priorSessionState.Objects.TryGetValue(objID, out objState))
            {
                objState = new SessionNetObjState.NetObjState();
                objState.SpawnData = spawnObj;
                objState.ObjectAlive = false;

                priorSessionState.Objects[objID] = objState;
            }
        }

        //propagate spawn data forward if necessary
        //while (true)
        //{
        //    sessionState = _sessionNetObjState.GetData(index);
        //    index++;
        //    if (sessionState == null)
        //        break;

        //    if (!sessionState.Objects.TryGetValue(objID, out objState))
        //    {
        //        objState = new SessionNetObjState.NetObjState();
        //    }

        //    objState.SpawnData = spawnObj;
        //    objState.ObjectAlive = true;

        //    sessionState.Objects[objID] = objState;
        //}

    }

    private void AddSocketData(VRNHeader header, VRNSocketData socketData)
    {
        float timestamp = header.ServerTime;
       

        SessionNetObjState.NetObjState objState;
        SessionNetObjState sessionState = GetNetObjState(timestamp);


        //try to retrieve object ID from the string field, to support old logs (for now)
        Guid objID;
        if (!Guid.TryParse(socketData.ObjectIDString, out objID))
        {

            if (socketData.ObjectID.Length != 16)
            {
                //if any object is in this socket, remove the object from the socket
                ClearSocket(sessionState, socketData.SocketID);
                return;
            }
            else
                objID = socketData.ObjectID.ToGuid();
        }

        if (!sessionState.Objects.TryGetValue(objID, out objState))
        {
            objState = new SessionNetObjState.NetObjState();
        }

        objState.SocketID = socketData.SocketID;
        sessionState.Objects[objID] = objState;
    }

    /// <summary>
    /// Search for any object in the state assigned to the socket ID and remove it
    /// from the socket
    /// </summary>
    /// <param name="state"></param>
    /// <param name="socketID"></param>
    private void ClearSocket(SessionNetObjState state,  string socketID)
    {
        foreach (var kvp in state.Objects)
        {
            var data = kvp.Value;
            if (data.SocketID == socketID)
            {
                data.SocketID = null;
                state.Objects[kvp.Key] = data;
                break;
            }
        }
    }

    private void AddSymbolManagerData(VRNHeader header, VRNSymbolManagerState data)
    {
        //Debug.Log($"Adding symbol data at {header.ServerTime:F2} data for: {data.SymbolManagerID.ToGuid()}");

        float timestamp = header.ServerTime;

        SessionNetObjState sessionState = GetNetObjState(timestamp);
        var symbolManagerID = data.SymbolManagerID.ToGuid();

        sessionState.MineMaps[symbolManagerID] = data;


        //propagate the fact that the object didn't exist before this entry
        //backward through time
        float timestampBin = _sessionNetObjState.ComputeTimestampBin(timestamp, TimestampBinSize);
        int index = _sessionNetObjState.GetClosestIndex(timestampBin);

        //use a blank map
        VRNSymbolManagerState blankMapData = new VRNSymbolManagerState();
        blankMapData.ClientID = data.ClientID;
        blankMapData.PlayerID = data.PlayerID;
        blankMapData.SymbolManagerID = data.SymbolManagerID;
        blankMapData.SymbolManagerName = data.SymbolManagerName;

        //Debug.Log($"SymbolManagerData: {symbolManagerID} : {data.SymbolManagerName} player {data.PlayerID}");


        for (int i = index - 1; i >= 0; i--)
        {
            sessionState = _sessionNetObjState.GetData(i);
            if (sessionState == null)
                break;

            //only change the value if the object didn't exist previously
            if (!sessionState.MineMaps.ContainsKey(symbolManagerID))
            {

                sessionState.MineMaps[symbolManagerID] = blankMapData;
            }

        }
    }

    private void AddVentGraphData(VRNHeader header, VRNVentGraph ventGraph)
    {
        float timestamp = header.ServerTime;

        SessionVentilationState ventState = new SessionVentilationState();
        ventState.VentGraph = ventGraph;
        ventState.VentStateIndex = ++_ventSateIndex;

        _sessionVentState.AddSequentialEntry(ventState, timestamp);
    }

    private void AddDestroyObjData(VRNHeader header, VRNDestroyObject destroyObj)
    {
        var objID = destroyObj.ObjID.ToGuid();
        AddDestroyObjData(header, objID);
    }

    private void AddDestroyObjData(VRNHeader header, Guid objID)
    {
        float timestamp = header.ServerTime;        

        SessionNetObjState.NetObjState objState;
        SessionNetObjState sessionState = GetNetObjState(timestamp);

        if (!sessionState.Objects.TryGetValue(objID, out objState))
        {
            //the first message for this object is destroy, propagate ISAlive backwards in time
            //this mostly affects scene objects that are later destroyed
            foreach (var priorSessionState in GetPriorNetObjState(timestamp))
            {
                //only change the value if the object didn't exist previously
                if (!priorSessionState.Objects.TryGetValue(objID, out objState))
                {
                    objState = new SessionNetObjState.NetObjState();
                    objState.ObjectAlive = true;

                    priorSessionState.Objects[objID] = objState;
                }
            }

            objState = new SessionNetObjState.NetObjState();
        }

        objState.ObjectAlive = false;
        sessionState.Objects[objID] = objState;
    }

    private void AddLogEventData(VRNHeader header, VRNLogEvent ev)
    {
        //ignore log events that have an invalid event ID - these are created 
        //when the server replicates packets to clients
        if (ev.EventID <= 0)
            return;

        //Debug.Log($"Session Event: {ev.Timestamp:F1} :: {ev.EventType.ToString()}");

        SessionEventData eventData = new SessionEventData();
        eventData.EventID = ++_logEventID;
        ev.EventID = eventData.EventID;
        eventData.EventData = ev;
        _sessionEvents.AddSequentialEntry(eventData, header.ServerTime);
    }

    private bool ReadNextEntry()
    {
        if (!_openForReading)
            return false;

        long filepos = 0;

        try
        {
            filepos = _fileStream.Position;

            _vrnHeader.PacketType = VRNPacketType.Unknown;
            _vrnHeader.PacketSize = 0;
            _vrnHeader.MergeDelimitedFrom(_fileStream);
            _lastTimestamp = _vrnHeader.ServerTime;

            //Debug.Log($"SessionLog: Packet {_vrnHeader.PacketType.ToString()}");

            int packetCount;
            if (_packetStats.TryGetValue(_vrnHeader.PacketType, out packetCount))
                _packetStats[_vrnHeader.PacketType] = packetCount + 1;
            else
                _packetStats[_vrnHeader.PacketType] = 1;

            if (_firstTimestamp < 0)
            {
                _firstTimestamp = _vrnHeader.ServerTime;
                VRNTeleportAll tele = new VRNTeleportAll();
                tele.TeleportTarget = LogHeader.TeleportTarget;
                AddTeleportData(_vrnHeader, tele);
            }

            filepos = _fileStream.Position;

            switch (_vrnHeader.PacketType)
            {
                case VRNPacketType.VrplayerInfo:
                    var vrpi = VRNVRPlayerInfo.Parser.ParseDelimitedFrom(_fileStream);
                    if (vrpi.ClientID > 1000)
                        vrpi = VRNVRPlayerInfo.Parser.ParseDelimitedFrom(_fileStream);
                    AddPlayerData(_vrnHeader, vrpi);
                    break;

                case VRNPacketType.SendCalibrationData:
                    var caldata = VRNCalibrationOffsetData.Parser.ParseDelimitedFrom(_fileStream);
                    AddCalibrationData(_vrnHeader, caldata);
                    break;

                case VRNPacketType.TeleportAll:
                    var tele = VRNTeleportAll.Parser.ParseDelimitedFrom(_fileStream);
                    AddTeleportData(_vrnHeader, tele);
                    break;

                case VRNPacketType.NetObjectSync:
                    ReadSyncObjData(_vrnHeader, _fileStream);
                    break;

                case VRNPacketType.NetObjectMessage:
                    ReadNetObjMessage(_vrnHeader, _fileStream);
                    break;

                case VRNPacketType.SpawnObject:
                    var spawnObj = VRNSpawnObject.Parser.ParseDelimitedFrom(_fileStream);
                    AddSpawnObjData(_vrnHeader, spawnObj);
                    break;

                case VRNPacketType.SendNetworkedObjectList:
                    var netObjList = VRNNetworkedObjectList.Parser.ParseDelimitedFrom(_fileStream);
                    AddNetworkedObjectListData(_vrnHeader, netObjList);
                    break;

                case VRNPacketType.DestroyObject:
                    var destroyObj = VRNDestroyObject.Parser.ParseDelimitedFrom(_fileStream);
                    AddDestroyObjData(_vrnHeader, destroyObj);
                    break;

                case VRNPacketType.LogEvent:
                    var logEvent = VRNLogEvent.Parser.ParseDelimitedFrom(_fileStream);
                    AddLogEventData(_vrnHeader, logEvent);
                    break;

                case VRNPacketType.SendVentGraph:
                    var ventGraph = VRNVentGraph.Parser.ParseDelimitedFrom(_fileStream);
                    AddVentGraphData(_vrnHeader, ventGraph);
                    break;

                case VRNPacketType.SendSymbolManagerState:
                    var symbolState = VRNSymbolManagerState.Parser.ParseDelimitedFrom(_fileStream);
                    AddSymbolManagerData(_vrnHeader, symbolState);
                    break;

                case VRNPacketType.Vrsocket:
                case VRNPacketType.VrsocketAssigned:
                case VRNPacketType.VrsocketVacate:
                    var socketData = VRNSocketData.Parser.ParseDelimitedFrom(_fileStream);
                    AddSocketData(_vrnHeader, socketData);
                    break;

                //case VRNPacketType.VrsocketVacate:
                //    var socketData = VRNSocketData.Parser.ParseDelimitedFrom(_fileStream);
                //    AddSocketData(_vrnHeader, socketData);
                //    break;

                case VRNPacketType.LogFooter:
                    LogFooter = VRNLogFooter.Parser.ParseDelimitedFrom(_fileStream);
                    break;
            }


            //Debug.Log($"SessionLog: Read packet {_vrnHeader.PacketType.ToString()}");
            _fileStream.Position = filepos;
            _fileStream.Seek(_vrnHeader.PacketSize, SeekOrigin.Current);
        }
        catch (Exception ex)
        {
            Debug.Log($"SessionLog: read error {ex.Message}");
            return false;

            //try
            //{
            //    _fileStream.Position = filepos;
            //    _fileStream.Seek(_vrnHeader.PacketSize, SeekOrigin.Current);
            //    return true;
            //}
            //catch (Exception ex2)
            //{
            //    Debug.Log($"SessionLog: seek error {ex2.Message}");
            //    return false;
            //}
            
        }

        if (_vrnHeader.PacketType == VRNPacketType.LogFooter)
            return false;

        return true;

    }

    public bool WriteEvent(VRNLogEvent ev)
    {
        if (_fileStream == null || !_openForWriting)
            return false;

        ev.EventID = ++_logEventID;

        WriteLog(VRNPacketType.LogEvent, ev, true);

        return true;
    }

    public bool WriteLog(VRNPacketType packetType, IMessage msg, bool broadcast)
    {
        if (_fileStream == null || !_openForWriting)
            return false;

        if (packetType == VRNPacketType.DissonancePacket)
            return false;

        //_vrnHeader.PacketType = packetType;
        //_vrnHeader.PacketDest = broadcast ? VRNPacketDestination.Broadcast : VRNPacketDestination.Direct;
        //_vrnHeader.DestClientID = 0;
        //_vrnHeader.ServerTime = Time.time;
        //_vrnHeader.TickCount = Time.frameCount;

        //_vrnHeader.WriteDelimitedTo(_fileStream);
        //msg.WriteDelimitedTo(_fileStream);

        _buffer.Position = 0;
        _buffer.SetLength(0);

        msg.WriteDelimitedTo(_buffer);
        _buffer.Position = 0;

        WriteLog(packetType, _buffer, broadcast);

        _messageCount++;

        return true;
    }

    public bool WriteLog(VRNNetObjMessage netObjMsg, IMessage msg, bool broadcast)
    {
        if (_fileStream == null || !_openForWriting)
            return false;

        //_vrnHeader.PacketType = VRNPacketType.NetObjectMessage;
        //_vrnHeader.PacketDest = broadcast ? VRNPacketDestination.Broadcast : VRNPacketDestination.Direct;
        //_vrnHeader.DestClientID = 0;
        //_vrnHeader.ServerTime = Time.time;
        //_vrnHeader.TickCount = Time.frameCount;

        //_vrnHeader.WriteDelimitedTo(_fileStream);
        //netObjMsg.WriteDelimitedTo(_fileStream);
        //msg.WriteDelimitedTo(_fileStream);

        _buffer.Position = 0;
        _buffer.SetLength(0);

        netObjMsg.WriteDelimitedTo(_buffer);
        msg.WriteDelimitedTo(_buffer);

        _buffer.Position = 0;
        WriteLog(VRNPacketType.NetObjectMessage, _buffer, broadcast);

        _messageCount++;

        return true;
    }

    public bool WriteLogNoHeader(MemoryStream data, bool broadcast)
    {
        if (_fileStream == null || !_openForWriting)
            return false;

        if (_firstTimestamp < 0)
            _firstTimestamp = Time.time;

        _lastTimestamp = Time.time;

        data.CopyTo(_fileStream, 100);

        _messageCount++;

        return true;

    }

    public bool WriteLog(VRNPacketType packetType, MemoryStream data, bool broadcast)
    {
        if (_fileStream == null || !_openForWriting)
            return false;

        _vrnHeader.PacketType = packetType;
        _vrnHeader.PacketDest = broadcast ? VRNPacketDestination.Broadcast : VRNPacketDestination.Direct;
        _vrnHeader.DestClientID = 0;
        _vrnHeader.ServerTime = Time.time;
        _vrnHeader.TickCount = Time.frameCount;
        _vrnHeader.PacketSize = (int)(data.Length - data.Position);

        _vrnHeader.WriteDelimitedTo(_fileStream);

        if (_firstTimestamp < 0)
            _firstTimestamp = Time.time;

        _lastTimestamp = Time.time;


        //data.WriteTo(_fileStream);
        data.CopyTo(_fileStream, 100);

        //_fileStream.Write(data.GetBuffer(), (int)data.Position, _vrnHeader.PacketSize);

        _messageCount++;

        return true;
    }

    public void CloseLog()
    {
        if (_fileStream != null && _openForWriting)
        {
            VRNLogFooter footer = new VRNLogFooter();
            footer.NumMessages = _messageCount;
            footer.SessionDuration = _lastTimestamp - _firstTimestamp;

            Debug.Log($"Closing Log FirstTS {_firstTimestamp:F1} LastTS {_lastTimestamp:F1}");

            long pos = _fileStream.Position;
            WriteLog(VRNPacketType.LogFooter, footer, false);

            var footerPosBytes = System.BitConverter.GetBytes(pos);
            Debug.Assert(footerPosBytes.Length == sizeof(long));

            _fileStream.Write(footerPosBytes, 0, footerPosBytes.Length);
        }

        if (_fileStream != null)
        {
            _fileStream.Dispose();
            _fileStream = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                //_fileStream.Dispose();
                CloseLog();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~SessionLog()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
