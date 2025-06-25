using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineObjectInfo : MonoBehaviour, IMineInformation, ISaveableComponent
{
    public string ObjectName = "";
    public int Priority = 9;
    public SessionEventManager SessionEventManager;
    public SystemManager SystemManager;

    private MineSegmentInfo _associatedMineSegment;

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
            return $"•<indent=8%>Object: {ObjectName}</indent>\n•<indent=8%>Entry {entry}, Crosscut {cross}</indent>\n•<indent=8%>Distance{unit}: {dist.x.ToString("F2")},{ dist.y.ToString("F2")}</indent>";
        }
        else
        {
            return $"•<indent=8%>Object: {ObjectName}</indent>";
        }
    }

    public int GetPriority()
    {
        return Priority;
    }

    // Start is called before the first frame update
    void Start()
    {
        var networkManager = NetworkManager.GetDefault(gameObject);
        if (networkManager != null && !networkManager.IsPlaybackMode)
        {
            return;
        }   
        
        if (SessionEventManager == null)
            SessionEventManager = SessionEventManager.GetDefault(gameObject);
        //{
        //    SessionEventManager = FindObjectOfType<SessionEventManager>();
        //}

        if(SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }
    }


    /// <summary>
    /// Delivers string formatted data to SavedComponent object to be stored in json format.
    /// Format is as follows:
    /// 
    /// ValueName|Value
    /// 
    /// So for example RotationSpeed|20 to store the RotationSpeed value of a component with the value of 20.
    /// </summary>
    /// <returns></returns>
    public string[] SaveInfo()
    {
        return new string[] { "ObjectName|" + ObjectName, "Priority|" + Priority };
    }

    public string SaveName()
    {
        return "MineObjectInfo";
    }
    /// <summary>
    /// Takes in a SavedComponent object and asks it for parameters it needs.
    /// </summary>
    /// <param name="savedComponent"></param>
    public void LoadInfo(SavedComponent savedComponent)
    {
        //ObjectName = savedComponent.GetParamValueAsStringByName("ObjectName");
        //int.TryParse(savedComponent.GetParamValueAsStringByName("Priority"), out Priority);
    }
}
