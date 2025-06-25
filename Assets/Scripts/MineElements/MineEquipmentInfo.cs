using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineEquipmentInfo : MonoBehaviour, IMineInformation
{
    public string EquipmentName = "";
    public string Status = "Idle";
    public int Priority = 10;
    public SessionEventManager SessionEventManager;
    public SystemManager SystemManager;
    public DebriefSceneLoader DebriefSceneLoader;

    private MineSegmentInfo _associatedMineSegment;
    private bool _loaded = false;

    private void Start()
    {
        if (SessionEventManager == null)
            SessionEventManager = SessionEventManager.GetDefault(gameObject);
        //{
        //    SessionEventManager = FindObjectOfType<SessionEventManager>();
        //}

        //if(SessionEventManager != null)
        //{
        //    if (SessionEventManager.LoadedMineSegmentInfos == null)
        //    {
        //        SessionEventManager.SegmentsPopulated += SessionEventManager_SegmentsPopulated;
        //    }
        //    else
        //    {
        //        SessionEventManager_SegmentsPopulated();
        //    }
        //}

        if(SessionEventManager != null)
            SessionEventManager.SegmentsPopulated += SessionEventManager_SegmentsPopulated;

        if(SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }

        
    }

    
    private void SessionEventManager_SegmentsPopulated()
    {
        //Debug.Log($"Segments populate script executed in {transform.name} at position {transform.position}");
        
        //if(SessionEventManager != null)
        //{
        //    Debug.Log($"Number of segments loaded: {SessionEventManager.LoadedMineSegmentInfos.Count}");
        //    foreach(MineSegmentInfo inf in SessionEventManager.LoadedMineSegmentInfos)
        //    {
        //        //Debug.Log($"Check MineSegmentInfo: {inf.name}");                
        //        MineSegment ms = inf.GetComponent<MineSegment>();
        //        Collider collider = inf.GetComponentInChildren<MeshCollider>();
        //        if(ms == null)
        //        {
        //            Debug.Log("MS was null");
        //            continue;
        //        }
        //        if (collider.bounds.Contains(transform.position))
        //        {
        //            _associatedMineSegment = inf;
        //            Debug.Log($"Segment found for {transform.name} within {ms.name}");
        //            break;
        //        }
        //    }
        //}
        //else
        //{
        //    Debug.Log("Session event manager is null");
        //}
    }

    public string GetMineInfo()
    {
        return $"•<indent=8%>Name: {EquipmentName}</indent>\n•<indent=8%>Item Status: {Status}</indent>\n•<indent=8%>Position: { transform.position.x.ToString("F2")},{ transform.position.z.ToString("F2")}</indent>";
    }

    public string GetMineInfo(Vector3 location)
    {
        if (!_loaded)
        {
            if (SessionEventManager != null)
            {
                Debug.Log($"Number of segments loaded: {SessionEventManager.LoadedMineSegmentInfos.Count}");
                foreach (MineSegmentInfo inf in SessionEventManager.LoadedMineSegmentInfos)
                {
                    //Debug.Log($"Check MineSegmentInfo: {inf.name}");                
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
                        Debug.Log($"Segment found for {transform.name} within {ms.name}");
                        break;
                    }
                }
                _loaded = true;
            }
            else
            {
                Debug.Log("Session event manager is null");
            }            
        }
        if (_associatedMineSegment != null)
        {
            string intersectionEntry = "";
            string intersectionCrosscut = "";
            Vector2 dist = _associatedMineSegment.GetDistanceFromNearestIntersection(location, out intersectionCrosscut, out intersectionEntry);
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
            return $"•<indent=8%>Name: {EquipmentName}</indent>\n•<indent=8%>Item Status: {Status}</indent>\n•<indent=8%>Entry {intersectionEntry}, Crosscut {intersectionCrosscut}</indent>\n•<indent=8%>Distance{unit}: {dist.x.ToString("F2")},{ dist.y.ToString("F2")}</indent>";
        }
        else
        {
            return $"•<indent=8%>Name: {EquipmentName}</indent>\n•<indent=8%>Item Status: {Status}</indent>";
        }
    }

    public int GetPriority()
    {
        return Priority;
    }

    private void OnDestroy()
    {
        if(DebriefSceneLoader != null)
            DebriefSceneLoader.SceneLoaded -= SessionEventManager_SegmentsPopulated;
    }
}
