using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineFireInfo : MonoBehaviour, IMineInformation
{
    public string ObjectName = "";
    public int Priority = 9;
    public SessionEventManager SessionEventManager;
    public SystemManager SystemManager;

    private MineSegmentInfo _associatedMineSegment;
    private FireInteraction _fireInteraction;
    public string GetMineInfo()
    {
        throw new System.NotImplementedException();
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
        string fireMetadata = "";
        if(_fireInteraction != null)
        {
            fireMetadata = $"{(_fireInteraction.GetFirePercentage() * 100).ToString("F2")}%";
        }
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
            return $"•<indent=8%>Object: {ObjectName}</indent>\n•<indent=8%>Entry {entry}, Crosscut {cross}</indent>\n•<indent=8%>Distance{unit}: {dist.x.ToString("F2")},{ dist.y.ToString("F2")}</indent>\n•<indent=8%>Intensity: {fireMetadata}</indent>";
        }
        else
        {
            return $"•<indent=8%>Object: {ObjectName}</indent>\n•<indent=8%>Intensity: {fireMetadata}</indent>";
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

        if (SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }

        try
        {
            _fireInteraction = GetComponent<FireInteraction>();
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }

    
}
