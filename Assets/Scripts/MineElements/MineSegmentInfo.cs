using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineSegmentInfo : MonoBehaviour, IMineInformation
{
    public string SectionName = "Mains";
    public string EntryName = "-";
    public string CrosscutName = "-";
    public int Priority = 100;
    public bool IsIntersection = false;
    public SystemManager SystemManager;

    public VentilationManager VentilationManager;

    private void Start()
    {
        if(VentilationManager == null)
        {
            VentilationManager = VentilationManager.GetDefault(gameObject);
        }

        if(SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }
    }

    public string GetMineInfo()
    {
        return "";
    }
    public string GetMineInfo(Vector3 location)
    {
        MineAtmosphere mineAtmo;
        MineSegmentInfo closestIntersection = GetClosestMineSegmentIntersection(location);
        if(EntryName == "")
        {
            EntryName = "-";
        }
        if(CrosscutName == "")
        {
            CrosscutName = "-";
        }
        if(SectionName == "")
        {
            SectionName = "Mains";
        }
        string entry = "";
        string cross = "";
        Vector2 dist = GetDistanceFromNearestIntersection(location, out cross, out entry);

        //float xDist = location.x - closestIntersection.transform.position.x;
        //float zDist = location.z - closestIntersection.transform.position.z;
        float xDist = dist.x;
        float zDist = dist.y;
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
        var airway = VentilationManager.FindClosestAirway(location);
        double speed = 0;
        if(airway != null)
        {
            speed = airway.MFAirway.FlowRate;
        }
        if (VentilationManager.GetMineAtmosphere(location, out mineAtmo)){
            int co = (int)(mineAtmo.CarbonMonoxide * 1000000.0f);
            if(co > 9999)
            {
                co = 9999;
            }
            if (entry == "-" && cross == "-")
            {
                return $"•<indent=8%>Mine Segment: {SectionName}</indent>\n•<indent=8%>Methane: {(mineAtmo.Methane * 100).ToString("F2")}%</indent>\n•<indent=8%>CO: {co} ppm</indent>\n•<indent=8%>Oxygen: {(mineAtmo.Oxygen * 100).ToString("F2")}%</indent>\n•<indent=8%>Airspeed: {(speed).ToString("F2")}cfm</indent>";

            }
            else
            {
                return $"•<indent=8%>Mine Segment: {SectionName}</indent>\n•<indent=8%>Entry {entry}, Crosscut {cross}</indent>\n•<indent=8%>Methane: {(mineAtmo.Methane * 100).ToString("F2")}%</indent>\n•<indent=8%>CO: {co} ppm</indent>\n•<indent=8%>Oxygen: {(mineAtmo.Oxygen * 100).ToString("F2")}%</indent>\n•<indent=8%>Airspeed: {(speed).ToString("F2")}cfm</indent>\n•<indent=8%>Distance{unit}: {xDist.ToString("F2")},{zDist.ToString("F2")}</indent>";
            }
        }

        return $"•<indent=8%>Mine Segment: {SectionName}</indent>\n•<indent=8%>Entry {closestIntersection.EntryName}, Crosscut {closestIntersection.CrosscutName}</indent>\n•<indent=8%>Distance{unit}: {xDist.ToString("F2")},{zDist.ToString("F2")}</indent>";
    }

    public int GetPriority()
    {
        return Priority;
    }

    public MineSegmentInfo GetClosestMineSegmentIntersection(Vector3 location)
    {
        if (IsIntersection)
        {
            return this;
        }
        MineSegment currMineSeg = GetComponent<MineSegment>();
        float proximity = Mathf.Infinity;
        MineSegment closestMineSeg = null;
        if (currMineSeg == null || currMineSeg.SegmentConnections == null)
            return null;

        foreach(SegmentConnectionInfo segConInf in currMineSeg.SegmentConnections)
        {
            float dist = Vector3.Distance(location, transform.TransformPoint(segConInf.Connection.Centroid));
            if (dist < proximity && segConInf.OppMineSegment != null)
            {
                if (segConInf.OppMineSegment.GetComponent<MineSegmentInfo>() != null)
                {
                    closestMineSeg = segConInf.OppMineSegment;
                    proximity = dist;
                }
            }
        }
        if(closestMineSeg != null)
            return closestMineSeg.GetComponent<MineSegmentInfo>();
        return this;
    }

    public Vector2 GetDistanceFromNearestIntersection(Vector3 location)
    {   
        MineSegmentInfo closestIntersection = GetClosestMineSegmentIntersection(location);
        if(SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }
        Debug.Log($"Distance Unit: {SystemManager.SystemConfig.DistanceUnit}");
        if(SystemManager.SystemConfig.DistanceUnit == 0)
        {
            SystemManager.SystemConfig.DistanceUnit = 3.28084f;
            SystemManager.SystemConfig.SaveConfig();
        }
        if(closestIntersection == null)
        {
            Debug.LogError("No closest intersection?!");
            return Vector2.zero;
        }
        float xDist = (location.x - closestIntersection.transform.position.x) * SystemManager.SystemConfig.DistanceUnit;
        float zDist = (location.z - closestIntersection.transform.position.z) * SystemManager.SystemConfig.DistanceUnit;
        Vector2 dist = new Vector2(xDist, zDist);

        return dist;
    }

    public Vector2 GetDistanceFromNearestIntersection(Vector3 location, out string crosscut, out string entry)
    {
        MineSegmentInfo closestIntersection = GetClosestMineSegmentIntersection(location);
        if (SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }
        Debug.Log($"Distance Unit: {SystemManager.SystemConfig.DistanceUnit}");
        if (SystemManager.SystemConfig.DistanceUnit == 0)
        {
            SystemManager.SystemConfig.DistanceUnit = 3.28084f;
            SystemManager.SystemConfig.SaveConfig();
        }
        if (closestIntersection == null)
        {
            //Debug.LogError("No closest intersection?!");
            crosscut = this.CrosscutName;
            entry = this.EntryName;
            return Vector2.zero;
        }
        crosscut = closestIntersection.CrosscutName;
        entry = closestIntersection.EntryName;
        float xDist = (location.x - closestIntersection.transform.position.x) * SystemManager.SystemConfig.DistanceUnit;
        float zDist = (location.z - closestIntersection.transform.position.z) * SystemManager.SystemConfig.DistanceUnit;
        Vector2 dist = new Vector2(xDist, zDist);

        return dist;
    }

}
