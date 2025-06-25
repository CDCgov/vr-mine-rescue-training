using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinePlayerInfo : MonoBehaviour, IMineInformation
{
    public int Priority = 1;
    public int PlayerID = 0;
    public PlayerManager PlayerManager;
    public SessionEventManager SessionEventManager;
    public SystemManager SystemManager;

    private MineSegmentInfo _associatedMineSegment;


    public string GetMineInfo()
    {
        return $"Player: {PlayerManager.GetPlayer(PlayerID).Name}";
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
            if(entry == "-" && cross == "-")
            {
                return $"•<indent=8%>Player: {PlayerManager.GetPlayer(PlayerID).Name}</indent>\n•<indent=8%>Player Role: {ParsePlayerRole(PlayerManager.GetPlayer(PlayerID).PlayerRole)}</indent>";
            }
            return $"•<indent=8%>Player: {PlayerManager.GetPlayer(PlayerID).Name}</indent>\n•<indent=8%>Player Role: {ParsePlayerRole(PlayerManager.GetPlayer(PlayerID).PlayerRole)}</indent>\n•<indent=8%>Entry {entry}, Crosscut {cross}</indent>\n•<indent=8%>Distance{unit}: {dist.x.ToString("F2")},{ dist.y.ToString("F2")}</indent>";
        }
        else
        {
            return $"•<indent=8%>Player: {PlayerManager.GetPlayer(PlayerID).Name}</indent>\n•<indent=8%>Player Role: {ParsePlayerRole(PlayerManager.GetPlayer(PlayerID).PlayerRole)}</indent>";
        }
    }

    public int GetPriority()
    {
        return Priority;
    }

    public string ParsePlayerRole(VRNPlayerRole vRNPlayerRole)
    {
        string output = "";
        switch (vRNPlayerRole)
        {
            case VRNPlayerRole.UnknownRole:
                output = "Unknown Role";
                break;
            case VRNPlayerRole.Captain:
                output = "Captain";
                break;
            case VRNPlayerRole.GasMan:
                output = "Gas Man";
                break;
            case VRNPlayerRole.MapMan:
                output = "Map Man";
                break;
            case VRNPlayerRole.SecondGasMan:
                output = "Second Gas Man";
                break;
            case VRNPlayerRole.TailCaptain:
                output = "Tail Captain";
                break;
            case VRNPlayerRole.Vrtech:
                output = "VR Tech";
                break;
            default:
                output = vRNPlayerRole.ToString();
                break;
        }

        return output;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }

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


        if (SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }
    }

    private void SessionEventManager_SegmentsPopulated()
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
