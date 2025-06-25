using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineNPCInfo : MonoBehaviour, IMineInformation
{
    public string NPCName = "";
    public string Status = "Idle";
    public int Priority = 5;
    public SessionEventManager SessionEventManager;
    public SystemManager SystemManager;
    private MineSegmentInfo _associatedMineSegment;
    //private NetSyncNPC _netSyncNPC;
    private NPCController _refugeBehaviors;
    public string GetMineInfo()
    {
        return null;
    }

    public string GetMineInfo(Vector3 location)
    {
        if (SessionEventManager != null)
        {
            foreach (MineSegmentInfo inf in SessionEventManager.LoadedMineSegmentInfos)
            {
                MineSegment ms = inf.GetComponent<MineSegment>();
                Collider collider = inf.GetComponentInChildren<MeshCollider>();
                if (ms == null)
                {
                    Debug.Log("MS was null");
                    continue;
                }
                if (collider.bounds.Contains(transform.position))
                {
                    _associatedMineSegment = inf;
                    break;
                }
            }
        }
        
        //Status = _netSyncNPC.FollowingStatus ? $"Following player {_netSyncNPC.PlayerToFollow}" : "Stopped";
        Status = _refugeBehaviors.GetStatus();
        
        if (_associatedMineSegment != null)
        {
            string entry = "";
            string cross = "";
            Vector2 dist = _associatedMineSegment.GetDistanceFromNearestIntersection(location, out cross, out entry);
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
            return $"•<indent=8%>Name: {NPCName}</indent>\n•<indent=8%>NPC Status: {Status}</indent>\n•<indent=8%>Entry {entry}, Crosscut {cross}</indent>\n•<indent=8%>Distance{unit}: {dist.x.ToString("F2")},{ dist.y.ToString("F2")}</indent>";
        }
        else
        {
            return $"•<indent=8%>Name: {NPCName}</indent>\n•<indent=8%>NPC Status: {Status}</indent>";
        }
    }

    public int GetPriority()
    {
        return Priority;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (SessionEventManager == null)
            SessionEventManager = SessionEventManager.GetDefault(gameObject);
        //{
        //    SessionEventManager = FindObjectOfType<SessionEventManager>();
        //}

        if (SessionEventManager != null)
        {
            if (SessionEventManager.LoadedMineSegmentInfos == null)
            {
                SessionEventManager.SegmentsPopulated += SessionEventManager_SegmentsPopulated;
            }
            //else
            //{
            //    SessionEventManager_SegmentsPopulated();
            //}
        }

        //_netSyncNPC = GetComponent<NetSyncNPC>();
        _refugeBehaviors = GetComponent<NPCController>();

        if (SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }
    }

    void SessionEventManager_SegmentsPopulated()
    {
        if (SessionEventManager != null)
        {
            foreach (MineSegmentInfo inf in SessionEventManager.LoadedMineSegmentInfos)
            {
                MineSegment ms = inf.GetComponent<MineSegment>();
                Collider collider = inf.GetComponentInChildren<MeshCollider>();
                if (ms == null)
                {
                    Debug.Log("MS was null");
                    continue;
                }
                if (collider.bounds.Contains(transform.position))
                {
                    _associatedMineSegment = inf;
                    break;
                }
            }
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
