using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using MFireProtocol;
using System.Runtime.Serialization;
using DataStructures.ViliWonka.KDTree;
using System;

public class VentGraph
{

    private List<VentAirway> _airways;
    private List<VentJunction> _junctions;
    private List<VentFire> _fires;
    private List<VentFan> _fans;
    private List<VentControl> _ventControls;

    private System.Diagnostics.Stopwatch _stopwatch;
    private MFireServerControl _serverControl;

    private KDTree _kdAirways;
    private KDTree _kdJunctions;
    private Dictionary<int, VentAirway> _kdAirwayMap;

    private PointOctree<VentAirway> _airwayTree;
    private PointOctree<VentJunction> _junctionTree;

    private KDQuery _kdAirwayQuery;
    private List<int> _kdAirwayResults;
    private List<float> _kdAirwayResultDists;

    private KDQuery _kdJunctionQuery;
    private List<int> _kdJunctionResults;
    private List<float> _kdJunctionResultDists;

    private List<VentJunction> _treeJuncResults;
    private List<VentAirway> _treeAirwayResults;

    private int _currentJunctionID;

    public Action OnGraphModified;

    public int NumJuncions
    {
        get
        {
            if (_junctions == null)
                return 0;
            else
                return _junctions.Count;
        }
    }
    
    public int NumAirways
    {
        get
        {
            if (_airways == null)
                return 0;
            else
                return _airways.Count;
        }
    }


    public VentGraph()
    {
        _stopwatch = new System.Diagnostics.Stopwatch();
        Reset();

        _currentJunctionID = 0;
    }

    public IEnumerable<VentAirway> GetAirways()
    {
        foreach (var airway in _airways)
            yield return airway;
    }

    public VentAirway GetRandomAirway()
    {
        if (_airways == null)
            return null;

        int index = UnityEngine.Random.Range(0, _airways.Count);
        return _airways[index];
    }

    public IEnumerable<VentJunction> GetJunctions()
    {
        foreach (var junction in _junctions)
            yield return junction;
    }

    /// <summary>
    /// change all fans to the specified fan data
    /// </summary>
    /// <param name="fanData"></param>
    public void SetAllFans(VentFanData fanData)
    {
        if (_fans == null || _fans.Count <= 0)
            return;

        foreach (var fan in _fans)
        {
            fan.SetFanData(fanData);
        }
    }

    public void SetAllFires(VentFireData fireData)
    {
        if (_fires == null || _fires.Count <= 0)
            return;

        foreach (var fire in _fires)
        {
            fire.SetFireData(fireData);
        }
    }

    public void Reset()
    {
        _airways = new List<VentAirway>();
        _junctions = new List<VentJunction>();
        _fires = new List<VentFire>();
        _fans = new List<VentFan>();
        _ventControls = new List<VentControl>();

        _kdAirways = null;
        _kdJunctions = null;

        _kdAirwayMap = new Dictionary<int, VentAirway>();
        _kdAirwayQuery = new KDQuery();
        _kdAirwayResults = new List<int>();
        _kdAirwayResultDists = new List<float>();

        _kdJunctionQuery = new KDQuery();
        _kdJunctionResults = new List<int>();
        _kdJunctionResultDists = new List<float>();

        _airwayTree = new PointOctree<VentAirway>(10, Vector3.zero, 0.5f);
        _junctionTree = new PointOctree<VentJunction>(10, Vector3.zero, 0.5f);

        _treeJuncResults = new List<VentJunction>();
        _treeAirwayResults = new List<VentAirway>();

        _currentJunctionID = 0;

        OnGraphModified?.Invoke();
    }

    public void RemoveAllFires()
    {
        _fires = new List<VentFire>();

        OnGraphModified?.Invoke();
    }

    public void RemoveAllFans()
    {
        _fans = new List<VentFan>();

        OnGraphModified?.Invoke();
    }

    public void ResetVentilationData()
    {
        foreach (var airway in _airways)
            airway.ResetVentilationData();

        foreach (var junction in _junctions)
            junction.ResetVentilationData();

        OnGraphModified?.Invoke();
    }

    public int AddJunction(VentJunction junc)
    {
        //junc.JunctionID = _junctions.Count;
        _currentJunctionID++;
        junc.JunctionID = _currentJunctionID;
        _junctions.Add(junc);

        _junctionTree.Add(junc, junc.WorldPosition);

        _kdJunctions = null;

        OnGraphModified?.Invoke();

        return junc.JunctionID;
    }

    public void RemoveJunction(int junctionID)
    {
        var junc = FindJunction(junctionID);
        if (junc == null)
            return;

        //remove any associated airways
        if (_airways != null)
        {
            for (int i = _airways.Count - 1; i >= 0; i--)
            {
                if (_airways[i].Start == junc ||
                    _airways[i].End == junc)
                {
                    //_airways.RemoveAt(i);
                    RemoveAirway(_airways[i].AirwayID);
                }
            }
        }

        if (!_junctions.Remove(junc))
            throw new System.Exception($"Couldn't remove junction ID {junctionID}");

        if (!_junctionTree.Remove(junc))
        {
            //throw new System.Exception($"Couldn't remove junction ID {junctionID}");
        }

        OnGraphModified?.Invoke();
    }

    public void UpdateAirway(VentAirway airway)
    {
        bool foundAirway = false;
        bool foundAirwayID = false;
        VentAirway existingAirway = null;

        foreach (var a in _airways)
        {
            if (a == airway)
            {
                foundAirway = true;
            }

            if (a.AirwayID == airway.AirwayID)
            {
                foundAirwayID = true;
                existingAirway = a;
            }
        }

        if (foundAirwayID && !foundAirway)
        {
            //remove old airway
            _airways.Remove(existingAirway);
            _kdAirways = null;
        }

        if (!foundAirwayID && foundAirway)
        {
            //this can't happen
            Debug.LogError($"VentGraph: Error updating airway {airway.AirwayID}");
        }

        if (!foundAirwayID && !foundAirway)
        {
            airway.Start.LinkedAirways.Add(airway);
            airway.End.LinkedAirways.Add(airway);
            _airways.Add(airway);
            _kdAirways = null;
        }

        OnGraphModified?.Invoke();
    }

    public void UpdateJunction(VentJunction junction)
    {
        bool foundJunction = false;
        bool foundJunctionID = false;
        VentJunction existingJunction = null;

        foreach (var j in _junctions)
        {
            if (j == junction)
            {
                foundJunction = true;
            }

            if (j.JunctionID == junction.JunctionID)
            {
                foundJunctionID = true;
                existingJunction = j;
            }
        }

        if (foundJunctionID && !foundJunction)
        {
            //remove old airway
            _junctions.Remove(existingJunction);
            _kdJunctions = null;
        }

        if (!foundJunctionID && foundJunction)
        {
            //this can't happen
            Debug.LogError($"VentGraph: Error updating junction {junction.JunctionID}");
        }

        if (!foundJunctionID && !foundJunction)
        {

            _junctions.Add(junction);
            _kdJunctions = null;
        }

        OnGraphModified?.Invoke();
    }

    public bool AreJunctionsConnected(VentJunction j1, VentJunction j2)
    {
        foreach (var airway in _airways)
        {
            if ((airway.Start == j1 && airway.End == j2) ||
                (airway.End == j1 && airway.Start == j2))
                return true;
        }

        return false;
    }

    public void AddAirway(VentAirway airway)
    {
        if (airway.Start == null || airway.End == null)
        {
            throw new System.ArgumentException("Invalid airway start/end");
        }

        //airway.AirwayID = _airways.Count;
        int airwayID = 0;
        foreach (var a in _airways)
        {
            if (airwayID < a.AirwayID)
                airwayID = a.AirwayID;
        }
        airwayID++;
        airway.AirwayID = airwayID;

        airway.Start.LinkedAirways.Add(airway);
        airway.End.LinkedAirways.Add(airway);
        _airways.Add(airway);

        //_airwayTree.Add(airway, airway.Start.WorldPosition);
        //_airwayTree.Add(airway, airway.End.WorldPosition);

        //var p1 = airway.Start.WorldPosition;
        //var p2 = airway.End.WorldPosition;

        ////move initial points slightly inward
        //var dir = (p2 - p1).normalized;
        //p1 = p1 + dir * 0.2f;
        //p2 = p2 - dir * 0.2f;
        //var midp = (p1 + p2) * 0.5f;

        //_airwayTree.Add(airway, p1);
        //_airwayTree.Add(airway, p2);
        //_airwayTree.Add(airway, midp);

        foreach (var pt in airway.GetSamplePoints())
        {
            _airwayTree.Add(airway, pt);
        }

        //force rebuild kd tree
        _kdAirways = null;

        OnGraphModified?.Invoke();
    }

    public void RemoveAirway(int airwayID)
    {
        for (int i = _airways.Count - 1; i >= 0; i--)
        {
            if (_airways[i].AirwayID == airwayID)
            {
                _airways[i].Start.LinkedAirways.Remove(_airways[i]);
                _airways[i].End.LinkedAirways.Remove(_airways[i]);

                _airways.RemoveAt(i);
                break;
            }
        }

        //force rebuild kd tree
        _kdAirways = null;

        OnGraphModified?.Invoke();
    }

    public void AddFan(VentFan fan, VentAirway airway)
    {
        fan.FanID = _fans.Count;
        fan.Airway = airway;
        fan.Airway.LinkedFans.Add(fan);
        _fans.Add(fan);

        OnGraphModified?.Invoke();
    }

    public void AddFire(VentFire fire)
    {
        fire.FireID = _fires.Count;
        _fires.Add(fire);

        OnGraphModified?.Invoke();
    }

    public void AddVentControl(VentControl control)
    {
        if (_airways == null || _ventControls == null)
            return;

        var airway = FindClosestAirway(control.WorldPosition);
        if (airway == null)
            return;

        airway.LinkedVentControls.Add(control);
        control.LinkedAirway = airway;

        _ventControls.Add(control);

        airway.CalculateResistance();
        //airway.UpdateAirway()

        OnGraphModified?.Invoke();
    }

    public VentJunction GetStartJunction()
    {
        foreach (var junction in _junctions)
        {
            if (junction.IsStartJunction)
                return junction;
        }

        return null;
    }


    /// <summary>
    /// Rebuild KD tree of airways
    /// store KD point index to airway map
    /// </summary>
    private void RebuildKDAirways()
    {
        if (_airways == null || _airways.Count <= 0)
            return;

        _kdAirwayMap.Clear();

        var airwayPts = new List<Vector3>(_airways.Count * 3);

        foreach (var airway in _airways)
        {
            foreach (var pt in airway.GetSamplePoints())
            {
                var index = airwayPts.Count;
                airwayPts.Add(pt);
                _kdAirwayMap.Add(index, airway);
            }
        }

        _kdAirways = new KDTree(airwayPts.ToArray(), 16);
    }


    /// <summary>
    /// Rebuild KD tree of Junctions
    /// 1-to-1 map of KD point index to junction array index
    /// </summary>
    private bool RebuildKDJunctions()
    {
        if (_junctions == null || _junctions.Count <= 0)
            return false;

        var junctionPts = new Vector3[_junctions.Count];

        for (int i = 0; i < _junctions.Count; i++)
        {
            junctionPts[i] = _junctions[i].WorldPosition;
        }

        _kdJunctions = new KDTree(junctionPts, 16);
        return true;
    }

    private string FormatSW(System.Diagnostics.Stopwatch sw)
    {
        return sw.Elapsed.TotalMilliseconds.ToString("F4");
        //double ticks = (double)sw.ElapsedTicks;
        //sw.
    }

    public VentJunction FindJunction(int junctionID)
    {
        foreach (var junc in _junctions)
        {
            if (junc.JunctionID == junctionID)
                return junc;
        }

        return null;
    }

    public VentAirway FindAirway(int airwayID)
    {
        foreach (var airway in _airways)
        {
            if (airway.AirwayID == airwayID)
                return airway;
        }

        return null;
    }

    public void SaveTo(VRNVentGraph vrnGraph)
    {
        vrnGraph.Airways.Clear();
        vrnGraph.Junctions.Clear();
        vrnGraph.Fires.Clear();

        foreach (var junc in _junctions)
        {
            var vrnJunc = new VRNVentJunction();
            junc.CopyTo(vrnJunc);
            vrnGraph.Junctions.Add(vrnJunc);
        }

        foreach (var airway in _airways)
        {
            var vrnAirway = new VRNVentAirway();
            airway.CopyTo(vrnAirway);
            vrnGraph.Airways.Add(vrnAirway);
        }

    }

    public void LoadFrom(VRNVentGraph vrnGraph)
    {


        foreach (var vrnJunc in vrnGraph.Junctions)
        {
            var junc = FindJunction(vrnJunc.JunctionID);
            if (junc == null)
                junc = new VentJunction();

            junc.LoadFrom(vrnJunc, this);
        }

        foreach (var vrnAirway in vrnGraph.Airways)
        {
            var airway = FindAirway(vrnAirway.AirwayID);
            if (airway == null)
            {
                if (vrnAirway.StartJunction == vrnAirway.EndJunction)
                {
                    Debug.LogError($"Error: Airway {vrnAirway.AirwayID} connects junction {vrnAirway.StartJunction} to itself");
                    continue;
                }
                else
                    airway = new VentAirway();
            }

            airway.LoadFrom(vrnAirway, this);
        }

        //update current junction ID
        _currentJunctionID = 0;
        foreach (var junc in _junctions)
        {
            if (_currentJunctionID < junc.JunctionID)
                _currentJunctionID = junc.JunctionID;
        }

        OnGraphModified?.Invoke();
    }

    public Vector3 ComputeAirVelocity(Vector3 pos)
    {
        VentAirway a1, a2;
        float d1, d2;

        if (!FindNearbyAirways(pos, out a1, out a2, out d1, out d2))
            return Vector3.zero;

        float ratio;
        if (d1 > 3.0f || d2 > 3.0f)
        {
            if (d1 > d2)
                ratio = 1.0f;
            else
                ratio = 0.0f;
        }
        else
        {
            //ratio = (d1 / d2);
            //ratio = Mathf.Clamp(ratio, 0, 3) / 3.0f;
            ratio = 0.5f;
        }

        var vel1 = a1.ComputeAirVelocity();
        var vel2 = a2.ComputeAirVelocity();
        //var targetVel = (vel1 + vel2) * 0.5f;
        var targetVel = vel1 * (1 - ratio) + vel2 * ratio;

        return targetVel;
    }

    public MineAtmosphere ComputeLocalMineAtmosphere(Vector3 pos, bool useRaycast)
    {
        MineAtmosphere mineAtmosphere = new MineAtmosphere();
        mineAtmosphere.Oxygen = 0.195f;

        VentJunction j1, j2;
        float ratio;

        if (!FindNearbyJunctions(pos, out j1, out j2, out ratio))
            return mineAtmosphere;

        float contamConc = (float)(j1.MFJunction.ContamConcentration * (1.0f - ratio) +
            j2.MFJunction.ContamConcentration * ratio);
        
        mineAtmosphere.Methane = (float)(j1.MFJunction.CH4Concentration * (1.0f - ratio) +
            j2.MFJunction.CH4Concentration * ratio);

        mineAtmosphere.CarbonMonoxide = contamConc;

        return mineAtmosphere;
    }

    public float ComputeLocalMethane(Vector3 pos)
    {
        VentJunction j1, j2;
        float ratio;

        if (!FindNearbyJunctions(pos, out j1, out j2, out ratio))
            return -1;

        return (float)(j1.MFJunction.CH4Concentration * (1.0f - ratio) + 
            j2.MFJunction.CH4Concentration * ratio);
    }

    public float ComputeLocalTemperature(Vector3 pos)
    {
        VentJunction j1, j2;
        float ratio;

        if (!FindNearbyJunctions(pos, out j1, out j2, out ratio))
            return -1;

        return (float)(j1.MFJunction.Temperature * (1.0f - ratio) +
            j2.MFJunction.Temperature * ratio);
    }

    public float ComputeLocalContaminant(Vector3 pos)
    {
        VentJunction j1, j2;
        float ratio;

        if (!FindNearbyJunctions(pos, out j1, out j2, out ratio))
            return -1;

        return (float)(j1.MFJunction.ContamConcentration * (1.0f - ratio) +
            j2.MFJunction.ContamConcentration * ratio);
    }

    private void ProjectOnLine(Vector3 pos, Vector3 p1, Vector3 p2, out float ratio)
    {
        Vector3 dir;
        ProjectOnLine(pos, p1, p2, out dir, out ratio);
    }

    private void ProjectOnLine(Vector3 pos, Vector3 p1, Vector3 p2, out Vector3 dir, out float ratio)
    {
        dir = p2 - p1;
        var length = dir.magnitude;
        dir.Normalize();

        Vector3 v = pos - p1;

        float distOnLine = Vector3.Dot(v, dir);
        distOnLine = Mathf.Clamp(distOnLine, 0, length);

        ratio = distOnLine / length;
    }

    public bool FindNearbyJunctions(Vector3 pos, out VentJunction j1, out VentJunction j2, out float ratio)
    {
        ratio = -1;

        if (!FindNearbyJunctions(pos, out j1, out j2))
            return false;

        //project the position onto the line between j1 and j2 and compute the percent down the line
        Vector3 dir;
        ProjectOnLine(pos, j1.WorldPosition, j2.WorldPosition, out dir, out ratio);
        //Vector3 dir = j2.WorldPosition - j1.WorldPosition;
        //var length = dir.magnitude;
        //dir.Normalize();

        //Vector3 v = pos - j1.WorldPosition;

        //float distOnLine = Vector3.Dot(v, dir);
        //distOnLine = Mathf.Clamp(distOnLine, 0, length);

        Debug.Assert(ratio <= 1 && ratio >= 0, "VentGraph: Invalid junction ratio!");
        //ratio = Mathf.Clamp(distOnLine / length, 0, 1);
        //ratio = distOnLine / length;

        return true;
    }

    public bool FindNearbyJunctions(Vector3 pos, out VentJunction j1, out VentJunction j2)
    {
        float d1, d2;
        return FindNearbyJunctions(pos, out j1, out j2, out d1, out d2);
    }

    public bool FindNearbyJunctions(Vector3 pos, out VentJunction j1, out VentJunction j2,
        out float d1, out float d2)
    {
        lock (_kdJunctionQuery)
        {
            if (_kdJunctions == null)
            {
                if (!RebuildKDJunctions())
                {
                    j1 = null;
                    j2 = null;
                    d1 = -1;
                    d2 = -1;
                    return false;
                }
            }

            _kdJunctionResults.Clear();
            _kdJunctionResultDists.Clear();

            _kdJunctionQuery.KNearest(_kdJunctions, pos, 2, _kdJunctionResults, _kdJunctionResultDists);

            if (_kdJunctionResults.Count < 2)
            {
                j1 = null;
                j2 = null;
                d1 = -1;
                d2 = -1;
                return false;
            }

            j1 = _junctions[_kdJunctionResults[0]];
            j2 = _junctions[_kdJunctionResults[1]];

            d1 = _kdJunctionResults[0];
            d2 = _kdJunctionResults[1];
        }

        return true;
    }

    public VentJunction FindClosestJunction(Vector3 pos)
    {
        return FindClosestJunctionKD(pos);
    }

    public VentJunction FindClosestJunctionBench(Vector3 pos, StringBuilder sb)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        //System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sw.Reset();
        sw.Start();
        var kdClosest = FindClosestJunctionKD(pos);
        sw.Stop();
        sb.AppendLine($"Junction KDTree: {FormatSW(sw)}ms {kdClosest.JunctionID}");

        sw.Reset();
        sw.Start();
        var octClosest = FindClosestJunctionOct(pos);
        sw.Stop();
        if (octClosest != null)
            sb.AppendLine($"Junction OctTree: {FormatSW(sw)}ms {octClosest.JunctionID}");

        sw.Reset();
        sw.Start();
        var refClosest = FindClosestJunctionRef(pos);
        sw.Stop();
        sb.AppendLine($"Junction Ref: {FormatSW(sw)}ms {refClosest.JunctionID}");

        //Debug.Log(sb.ToString());

        return refClosest;
    }

    private VentJunction FindClosestJunctionKD(Vector3 pos)
    {
        lock (_kdJunctionQuery)
        {
            if (_kdJunctions == null)
                RebuildKDJunctions();

            _kdJunctionResults.Clear();
            _kdJunctionQuery.KNearest(_kdJunctions, pos, 1, _kdJunctionResults);

            if (_kdJunctionResults.Count <= 0)
                return null;

            return _junctions[_kdJunctionResults[0]];
        }
    }

    private VentJunction FindClosestJunctionOct(Vector3 pos)
    {
        if (_junctionTree == null)
            return FindClosestJunctionRef(pos);

        if (!_junctionTree.GetNearbyNonAlloc(pos, 12, _treeJuncResults))
            return null;

        float minDist = float.MaxValue;
        VentJunction closestJunction = null;

        foreach (var junc in _treeJuncResults)
        {
            float dist = Vector3.Distance(pos, junc.WorldPosition);
            if (dist < minDist)
            {
                closestJunction = junc;
                minDist = dist;
            }
        }

        return closestJunction;
    }

    private VentJunction FindClosestJunctionRef(Vector3 pos)
    {
        if (_junctions == null)
            return null;

        VentJunction closest = null;
        float minDist = float.MaxValue;

        foreach (var junction in _junctions)
        {
            float dist = Vector3.Distance(junction.WorldPosition, pos);
            if (dist < minDist)
            {
                minDist = dist;
                closest = junction;
            }
        }

        return closest;
    }

    public VentAirway FindClosestAirway(Vector3 pos)
    {
        return FindClosestAirwayKD(pos);
    }

    /// <summary>
    /// Find the two closest airways
    /// Note: will return the same airway for a1 and a2 when near the center
    /// actually finds the two closest airway sample points
    /// 
    /// Also computes the ratio between the two airways based on the position
    /// projected onto the airway paths
    /// </summary>
    /// <returns></returns>
    public bool FindNearbyAirways(Vector3 pos, out VentAirway a1, out VentAirway a2, out float ratio)
    {
        ratio = -1;

        if (!FindNearbyAirways(pos, out a1, out a2))
            return false;

        if (a1 == a2)
        {
            //not close to different airways
            ratio = 1.0f;
            return true;
        }

        //remap the airway points to follow a known order
        Vector3 start, mid, end;
        
        //find path points
        if (a1.End == a2.Start)
        {
            //normal sequence
            start = a1.Start.WorldPosition;
            mid = a1.End.WorldPosition;
            end = a2.End.WorldPosition;
        }
        else if (a1.Start == a2.End)
        {
            //reversed sequence
            //airway1 = a2;
            //airway2 = a1;
            //start = a2.Start.WorldPosition;
            //mid = a2.End.WorldPosition;
            //end = a1.End.WorldPosition;

            start = a1.End.WorldPosition;
            mid = a1.Start.WorldPosition;
            end = a2.Start.WorldPosition;
        }   
        else if (a1.Start == a2.Start)
        {
            //common start point
            start = a1.End.WorldPosition;
            mid = a1.Start.WorldPosition;
            end = a2.End.WorldPosition;

        }
        else if (a1.End == a2.End)
        {
            //common end point
            start = a1.Start.WorldPosition;
            mid = a1.End.WorldPosition;
            end = a2.Start.WorldPosition;

        }
        else
        {
            Debug.LogError($"VentGraph: Detected invalid path configuration on airways {a1.AirwayID} and {a2.AirwayID}");
            return false;
        }



        ////project the position onto the line between j1 and j2 and compute the percent down the line
        //Vector3 dir = j2.WorldPosition - j1.WorldPosition;
        //var length = dir.magnitude;
        //dir.Normalize();

        //Vector3 v = pos - j1.WorldPosition;

        //float distOnLine = Vector3.Dot(v, dir);
        //distOnLine = Mathf.Clamp(distOnLine, 0, length);

        //Debug.Assert(ratio <= 1 && ratio >= 0, "VentGraph: Invalid junction ratio!");
        ////ratio = Mathf.Clamp(distOnLine / length, 0, 1);
        //ratio = distOnLine / length;

        return true;
    }



    /// <summary>
    /// Find the two closest airways
    /// Note: will return the same airway for a1 and a2 when near the center
    /// actually finds the two closest airway sample points
    /// </summary>
    /// <returns></returns>
    public bool FindNearbyAirways(Vector3 pos, out VentAirway a1, out VentAirway a2)
    {
        float d1, d2;
        return FindNearbyAirways(pos, out a1, out a2, out d1, out d2);
    }

    /// <summary>
    /// Find the two closest airways
    /// Note: will return the same airway for a1 and a2 when near the center
    /// actually finds the two closest airway sample points
    /// </summary>
    /// <returns></returns>
    public bool FindNearbyAirways(Vector3 pos, out VentAirway a1, out VentAirway a2,
        out float d1, out float d2)
    {
        lock (_kdAirwayQuery)
        {
            if (_kdAirways == null)
            {
                RebuildKDAirways();

                if (_kdAirways == null)
                {
                    a1 = null;
                    a2 = null;
                    d1 = -1;
                    d2 = -1;
                    return false;
                }
            }

            _kdAirwayResults.Clear();
            _kdAirwayResultDists.Clear();

            _kdAirwayQuery.KNearest(_kdAirways, pos, 2, _kdAirwayResults, _kdAirwayResultDists);

            if (_kdAirwayResults.Count < 2)
            {
                a1 = null;
                a2 = null;
                d1 = -1;
                d2 = -1;
                return false;
            }

            a1 = _kdAirwayMap[_kdAirwayResults[0]];
            a2 = _kdAirwayMap[_kdAirwayResults[1]];

            d1 = _kdAirwayResultDists[0];
            d2 = _kdAirwayResultDists[1];
        }

        return true;
    }

    /// <summary>
    /// Find the n closest airways
    /// Note: will return the same airway multiple times
    /// actually finds the two closest airway sample points
    /// </summary>
    /// <returns></returns>
    public bool FindNearbyAirways(Vector3 pos, int n, List<int> airwayIndices, List<float> airwayDistances)
    {
        lock (_kdAirwayQuery)
        {
            if (_kdAirways == null)
            {
                RebuildKDAirways();

                if (_kdAirways == null)
                {                   
                    return false;
                }
            }

            _kdAirwayQuery.KNearest(_kdAirways, pos, n, airwayIndices, airwayDistances);

        }

        return true;
    }

    /// <summary>
    /// Find the n closest airways
    /// Note: will return the same airway multiple times
    /// actually finds the two closest airway sample points
    /// </summary>
    /// <returns></returns>
    public bool FindNearbyAirways(Vector3 pos, int n, HashSet<VentAirway> airways)
    {
        if (airways == null)
            return false;

        lock (_kdAirwayQuery)
        {
            if (_kdAirways == null)
            {
                RebuildKDAirways();

                if (_kdAirways == null)
                {
                    return false;
                }
            }

            _kdAirwayResults.Clear();
            //_kdAirwayResultDists.Clear();
            airways.Clear();

            _kdAirwayQuery.KNearest(_kdAirways, pos, n, _kdAirwayResults);

            foreach (int index in _kdAirwayResults)
            {
                var airway = _kdAirwayMap[index];
                airways.Add(airway);
            }

        }

        return true;
    }

    public VentAirway FindClosestAirwayBench(Vector3 pos, StringBuilder sb)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        //System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sw.Reset();
        sw.Start();
        var kdClosest = FindClosestAirwayKD(pos);
        sw.Stop();
        sb.AppendLine($"Airway KDTree: {FormatSW(sw)}ms {kdClosest.AirwayID}");

        sw.Reset();
        sw.Start();
        var octClosest = FindClosestAirwayOct(pos);
        sw.Stop();
        if (octClosest != null)
            sb.AppendLine($"Airway OctTree: {FormatSW(sw)}ms {octClosest.AirwayID}");

        sw.Reset();
        sw.Start();
        var refClosest = FindClosestAirwayRef(pos);
        sw.Stop();
        sb.AppendLine($"Airway Ref: {FormatSW(sw)}ms {refClosest.AirwayID}");

        //Debug.Log(sb.ToString());

        return refClosest;
    }

    private VentAirway FindClosestAirwayKD(Vector3 pos)
    {
        lock (_kdAirwayQuery)
        {
            if (_kdAirways == null)
                RebuildKDAirways();

            if (_kdAirways == null) //if rebuild failed
                return null;

            _kdAirwayResults.Clear();
            _kdAirwayQuery.KNearest(_kdAirways, pos, 1, _kdAirwayResults);

            if (_kdAirwayResults.Count <= 0)
                return null;

            return _kdAirwayMap[_kdAirwayResults[0]];
        }
    }

    private VentAirway FindClosestAirwayOct(Vector3 pos)
    {
        if (_airwayTree == null)
            return null;

        if (!_airwayTree.GetNearbyNonAlloc(pos, 12, _treeAirwayResults))
            return null;

        float minDist = float.MaxValue;
        VentAirway closestAirway = null;

        foreach (var airway in _treeAirwayResults)
        {
            var dist = DistToAirway(pos, airway);
            if (dist < minDist)
            {
                minDist = dist;
                closestAirway = airway;
            }
        }

        return closestAirway;
    }

    private VentAirway FindClosestAirwayRef(Vector3 pos)
    {
        if (_airways == null)
            return null;

        VentAirway closest = null;
        float minDist = float.MaxValue;

        foreach (var airway in _airways)
        {
            float dist = DistToAirway(pos, airway);
            if (dist < minDist)
            {
                minDist = dist;
                closest = airway;
            }
        }

        return closest;
    }

    private float DistToAirway(Vector3 pos, VentAirway airway)
    {
        Vector3 airwayDir = airway.End.WorldPosition - airway.Start.WorldPosition;
        var length = airwayDir.magnitude;
        airwayDir.Normalize();

        Vector3 v = pos - airway.Start.WorldPosition;

        float distOnLine = Vector3.Dot(v, airwayDir);
        if (distOnLine > length)
            distOnLine = length;
        if (distOnLine < 0)
            distOnLine = 0;

        var ptOnLine = airway.Start.WorldPosition + airwayDir * distOnLine;

        return Vector3.Distance(ptOnLine, pos);
    }

    public void CreateUIObjects(Transform parent, GameObject juncPrefab, GameObject airwayPrefab, GameObject firePrefab, float y_height)
    {
        foreach (var junction in _junctions)
        {
            var uiObj = CreateUIObj(parent, juncPrefab);

            uiObj.VentObj = junction;

            uiObj.UpdatePosition(y_height);
        }

        foreach (var airway in _airways)
        {
            if (airway.Start == null || airway.End == null)
                continue;

            var uiObj = CreateUIObj(parent, airwayPrefab);

            uiObj.VentObj = airway;

            uiObj.UpdatePosition(y_height);
            //var pos = (airway.Start.WorldPosition + airway.End.WorldPosition) * 0.5f;
            //var dir = airway.End.WorldPosition - airway.Start.WorldPosition;

            //pos.y = y_height;

            //uiObj.transform.position = pos;
            //uiObj.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        }

        foreach (var fire in _fires)
        {
            if (fire.Airway == null)
                continue;

            var uiObj = CreateUIObj(parent, firePrefab);

            uiObj.VentObj = fire;
            uiObj.UpdatePosition(y_height);
        }
    }

    public void UpdateUIObjects(Transform parent, float y_height)
    {
        foreach (Transform child in parent)
        {
            var uiObj = child.GetComponent<VentUIObj>();
            if (uiObj == null)
                continue;

            uiObj.UpdatePosition(y_height);

        }
    }

    private VentUIObj CreateUIObj(Transform parent, GameObject prefab)
    {
        var obj = GameObject.Instantiate<GameObject>(prefab);
        obj.transform.SetParent(parent, false);

        return obj.GetComponent<VentUIObj>();
    }

    /// <summary>
    /// Load the results of the simulation into the ventilation graph
    /// </summary>
    /// <param name="serverControl"></param>
    public void UpdateFromSimulation(MFireServerControl serverControl)
    {
        _stopwatch.Reset();
        _stopwatch.Start();

        foreach (var airway in _airways)
        {
            if (airway.MFireID >= 0)
            {
                var newData = serverControl.GetAirway(airway.MFireID);
                if (newData != null)
                    airway.CopyVentDataFrom(newData);
            }
        }

        foreach (var junction in _junctions)
        {
            if (junction.MFireID >= 0)
            {
                var newData = serverControl.GetJunction(junction.MFireID);
                if (newData != null)
                    junction.CopyVentDataFrom(newData);
            }
        }

        _stopwatch.Stop();
        Debug.Log($"Vent: MFire simulation data copy took {_stopwatch.ElapsedMilliseconds}ms");
    }

    public bool BuildMFireConfig(MFireServerControl serverControl)
    {
        _kdAirways = null;
        _kdJunctions = null;
        serverControl.ResetMFireState();

        ResetMFireIDs();
        MarkDeadEnds();

        var startJunction = GetStartJunction();
        if (startJunction == null)
        {
            //throw new System.Exception("VentGraph: no vent start junction set");
            Debug.LogError("VentGraph: no vent start junction set");
            return false;
        }

        CreateMFireJunction(startJunction, serverControl);

        int infiniteLoopPreventer = 50000;
        //create airways expanding outwards from the start segment, to hopefully fix fan flow direction....
        Queue<VentAirway> airwayQueue = new Queue<VentAirway>();

        QueueSegmentLinks(airwayQueue, startJunction);

        if (airwayQueue.Count <= 0)
        {
            throw new System.Exception("VentGraph: No airways linked to start junction");
        }

        while (airwayQueue.Count > 0)
        {
            var airway = airwayQueue.Dequeue();
            if (airway.MFireID < 0)
            {
                CreateMFireAirwayAndJunctions(airway, serverControl);
            }

            QueueSegmentLinks(airwayQueue, airway.Start);
            QueueSegmentLinks(airwayQueue, airway.End);

            infiniteLoopPreventer--;
            if (infiniteLoopPreventer <= 0)
            {
                Debug.LogError("VentGraph: Hit infinite loop prevention limit when processing vent graph");
                break;
            }
        }

        //create any remaining airways
        foreach (var airway in _airways)
        {
            if (airway.MFireID < 0)
            {
                if (airway.Start.MFireID < 0 || airway.End.MFireID < 0)
                    continue;

                Debug.LogWarning($"VentGraph: Creating disconnected airway from j{airway.Start.JunctionID} to j{airway.End.JunctionID}");
                CreateMFireAirway(airway, serverControl);
            }
        }

        //add fans
        foreach (var fan in _fans)
        {
            if (fan.Airway == null)
            {
                Debug.LogError($"Fan {fan.FanID} has no airway");
                continue;
            }

            if (fan.Airway.MFireID < 0)
            {
                Debug.LogError($"VentGraph: Fan {fan.FanID} is in inactive airway {fan.Airway.AirwayID}");
                continue;
            }

            var startPos = fan.Airway.Start.WorldPosition;
            var endPos = fan.Airway.End.WorldPosition;

            var dir = (endPos - startPos).normalized;
            var fanDir = fan.WorldRotation * Vector3.forward;

            var dot = Vector3.Dot(dir, fanDir);
            if (dot < 0)
            {
                Debug.Log($"VentGraph: Fan {fan.FanID} direction does not match airway, reversing airway");
                var temp = fan.Airway.Start;
                fan.Airway.Start = fan.Airway.End;
                fan.Airway.End = temp;
            }
            else
            {
                Debug.Log($"VentGraph: Fan {fan.FanID} direction matches airway");
            }

            CreateMFireFan(fan, serverControl);
        }

        UpdateAirwayResistance();

        //add fires
        foreach (var fire in _fires)
        {
            if (fire.Airway == null)
            {
                Debug.LogError($"VentGraph: Fire {fire.FireID} has no airway");
                continue;
            }

            if (fire.Airway.MFireID < 0)
            {
                Debug.LogError($"VentGraph: Fire {fire.FireID} is in inactive airway {fire.Airway.AirwayID}");
                continue;
            }

            CreateMFireFire(fire, serverControl);
        }


        //check for at least one fan
        if (serverControl.GetNumFans() <= 0)
        {
            Debug.Log("No fans in MFire Config");

            foreach (var airway in _airways)
            {
                if (airway.MFireID > 0)
                {
                    Debug.Log($"Adding default fan in airway {airway.AirwayID}");
                    var fan = new VentFan();
                    fan.Airway = airway;
                    CreateMFireFan(fan, serverControl);
                    break;
                }
            }
        }

        OnGraphModified?.Invoke();

        return true;
    }

    public void UpdateAirwayResistance()
    {
        if (_airways == null)
            return;

        foreach (var airway in _airways)
        {
            airway.CalculateResistance();
        }

        OnGraphModified?.Invoke();
    }

    public void ResetControlResistance()
    {
        if (_airways == null)
            return;

        foreach (var airway in _airways)
        {
            airway.ControlResistance = 0;
        }

        OnGraphModified?.Invoke();
    }

    void CreateMFireFire(VentFire fire, MFireServerControl serverControl)
    {
        //fire.FireID = serverControl.GetNumFires() + 1;
        //serverControl.UpdateFire(fire.MFFire);

        fire.MFireID = serverControl.GetNumFires();
        fire.UpdateFire(serverControl);

        OnGraphModified?.Invoke();
    }

    void CreateMFireFan(VentFan fan, MFireServerControl serverControl)
    {
        fan.MFireID = serverControl.GetNumFans() + 1;
        //serverControl.UpdateFan(fan.MFFan);
        fan.UpdateFan(serverControl);

        OnGraphModified?.Invoke();
    }

    void CreateMFireJunction(VentJunction junction, MFireServerControl serverControl)
    {
        if (junction.MFireID <= -5)
        {
            //don't add segments that are part of a dead end path
            return;
        }

        junction.MFireID = serverControl.GetNumJunctions() + 1;

        //serverControl.UpdateJunction(junction.MFJunction);
        junction.UpdateJunction(serverControl);

        OnGraphModified?.Invoke();
    }

    void QueueSegmentLinks(Queue<VentAirway> queue, VentJunction junc)
    {
        //if (seg.MFireJunction <= 0)
        //    return;
        if (junc.MFireID <= 0)
            return;

        //foreach (MineSegmentLink link in seg.MineSegmentLinks)
        foreach (var airway in junc.LinkedAirways)
        {
            //don't create dead-end airways
            if (airway.Start.MFireID <= -5 || airway.End.MFireID <= -5)
                continue;

            if (airway.MFireID < 0)
                queue.Enqueue(airway);
            //if (link.MFireAirway < 0)
            //    queue.Enqueue(link);
        }
    }

    void CreateMFireAirwayAndJunctions(VentAirway airway, MFireServerControl serverControl)
    {
        //don't add bad segments
        //if (link.Segment1.MFireJunction <= -5 || link.Segment2.MFireJunction <= -5)
        if (airway.Start.MFireID <= -5 || airway.End.MFireID <= -5)
            return;

        //if (link.Segment1.MFireJunction == -1)
        if (airway.Start.MFireID == -1)
        {
            CreateMFireJunction(airway.Start, serverControl);
            //var junc = CreateMFireJunction(link.Segment1, _serverControl);
            //_junctionSegmentMap.Add(junc.Number, link.Segment1);
        }
        //if (link.Segment2.MFireJunction == -1)
        if (airway.End.MFireID == -1)
        {
            CreateMFireJunction(airway.End, serverControl);
            //var junc = CreateMFireJunction(link.Segment2, _serverControl);
            //_junctionSegmentMap.Add(junc.Number, link.Segment2);
        }


        CreateMFireAirway(airway, serverControl);

        //var airway = CreateMFireAirway(link, _serverControl);

        //_airwaySegmentMap.Add(airway.Number, link);

        //return airway;

        OnGraphModified?.Invoke();
    }


    void CreateMFireAirway(VentAirway airway, MFireServerControl serverControl)
    {
        //MFAirway airway = new MFAirway();

        //MineSegment startSeg, endSeg;
        //VentJunction start, end;

        //if (link.Segment1.MFireJunction < 0 || link.Segment2.MFireJunction < 0)
        //    return null;
        if (airway.Start.MFireID < 0 || airway.End.MFireID < 0)
            return;

        //start at the lower-numbered junction
        //if (link.Segment1.MFireJunction < link.Segment2.MFireJunction)
        if (airway.Start.MFireID > airway.End.MFireID)
        {
            //swap start/end
            var tmp = airway.Start;
            airway.Start = airway.End;
            airway.End = tmp;
        }

        airway.MFireID = serverControl.GetNumAirways() + 1;
        airway.CalculateLength();
        airway.UpdateAirway(serverControl);
        //airway.UpdateLength();
        //serverControl.UpdateAirway(airway.MFAirway);

        //airway.StartJunction = startSeg.MFireJunction;
        //airway.EndJunction = endSeg.MFireJunction;

        //airwayCount++;
        //airway.Number = serverControl.GetNumAirways() + 1;

        //airway.Length = link.ComputeLength() * Constants.MetersToFeet;
        //airway.Perimeter = 100;
        //airway.CrossSectionalArea = 140;
        //airway.FlowRate = 0;
        //airway.Resistance = 1.5;
        //airway.FrictionFactor = 50;
        //airway.Type = 0;

        //link.MFireAirway = airway.Number;

        //serverControl.UpdateAirway(airway);
        //_airwayMap.Add(airway.Number, link);

        //return airway;

        OnGraphModified?.Invoke();
    }

    private void ResetMFireIDs()
    {
        foreach (var airway in _airways)
            airway.MFireID = -1;
        foreach (var junction in _junctions)
            junction.MFireID = -1;

    }

    void MarkDeadEnds()
    {
        //foreach (MineSegment seg in _mineNetwork.MineSegments)
        //{
        //    if (seg.MineSegmentLinks.Count <= 1)
        //    {
        //        MarkPathAsDeadEnd(seg);
        //    }
        //}

        foreach (var junc in _junctions)
        {
            if (junc.LinkedAirways.Count <= 1)
            {
                MarkPathAsDeadEnd(junc);
            }
        }
    }

    void MarkPathAsDeadEnd(VentJunction junc)
    {
        if (junc.MFireID <= -5)
            return; //don't infinitely recurse

        if (junc.LinkedAirways.Count <= 2)
        {
            junc.MFireID = -5;

            foreach (var airway in junc.LinkedAirways)
            {
                MarkPathAsDeadEnd(airway.Start);
                MarkPathAsDeadEnd(airway.End);
            }
        }
        else
        {
            //found a junction
        }

    }

    public static void SetFieldData(float[] data, int width, int x, int y, float r, float g)
    {
        var index = y * width * 2 + x * 2;
        data[index] = r;
        data[index + 1] = g;
    }

    public static void SetFieldAlpha(byte[] data, int width, int x, int y, byte a)
    {
        var index = y * width * 4 + x * 4;
        data[index + 3] = a;
    }

    public static void SetFieldRGB(byte[] data, int width, int x, int y, byte r, byte g, byte b)
    {
        var index = y * width * 4 + x * 4;
        data[index + 0] = r;
        data[index + 1] = g;
        data[index + 2] = b;
    }

    public static void SetFieldRGBA(byte[] data, int width, int x, int y, byte r, byte g, byte b, byte a)
    {
        var index = y * width * 4 + x * 4;
        data[index + 0] = r;
        data[index + 1] = g;
        data[index + 2] = b;
        data[index + 3] = a;
    }

    private Vector3 UVToWorld(int uvx, int uvy, int width, int height, Bounds worldBounds)
    {
        float x = (float)uvx / (float)(width - 1);
        float y = (float)uvy / (float)(height - 1);

        var vec = worldBounds.min + Vector3.Scale(worldBounds.size, new Vector3(x, 0, y));
        vec.y = 0;

        return vec;
    }

    public void UpdateFieldMineGeometry(byte[] data, int width, int height, Bounds worldbounds)
    {
        Vector3 upVector = new Vector3(0, 10, 0);
        Vector3 dir = new Vector3(0, -1, 0);

        int layerMask = LayerMask.GetMask("Floor");

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //border the geometry texture with 0 alpha so that outside the texture bounds is an invalid location
                if (y == 0 || x == 0 || y == height - 1 || x == width - 1)
                {
                    SetFieldAlpha(data, width, x, y, 0);
                    continue;
                }

                var pos = UVToWorld(x, y, width, height, worldbounds);

                //RaycastHit hit;
                if (Physics.Raycast(pos + upVector, dir, 50, layerMask, QueryTriggerInteraction.Ignore))
                {
                    SetFieldAlpha(data, width, x, y, 255);
                }
                else
                {
                    SetFieldAlpha(data, width, x, y, 0);
                }
            }
        }
    }

    public void UpdateFieldGasReadings(byte[] data, int width, int height, Bounds worldbounds)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pos = UVToWorld(x, y, width, height, worldbounds);

                var methane = ComputeLocalMethane(pos);
                var contam = ComputeLocalContaminant(pos);
                var temp = ComputeLocalTemperature(pos);

                //methane: scale using range 0->1
                methane = Mathf.Clamp(methane * 255, 0, 255);

                //contam: scale using range 0->0.001
                contam = Mathf.Clamp(contam * 255.0f * 1000.0f, 0, 255);

                //temp: scale using range 50->90
                temp = Mathf.Clamp(((temp - 50) / 90.0f) * 255.0f, 0, 255);

                SetFieldRGB(data, width, x, y, (byte)methane, (byte)contam, (byte)temp);
            }
        }
    }


    public void UpdateVectorField2D(float[] data, int width, int height, Bounds worldbounds)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pos = UVToWorld(x, y, width, height, worldbounds);
                var vel = ComputeAirVelocity(pos);
                SetFieldData(data, width, x, y, vel.x, vel.z);
            }
        }
    }


    void BuildVectorField2D()
    {
        //Texture3D field = new Texture3D(100, 100, 3, TextureFormat.RGB24, false);
        Texture2D field = new Texture2D(100, 100, TextureFormat.RGFloat, false);



        //field.SetPixels32()
    }

    //public string SaveToJSON()
    //{
    //    StringBuilder sb = new StringBuilder();
    //    sb.Append(JsonUtility.ToJson(_airways));
    //    sb.Append(JsonUtility.ToJson(_junctions));
    //    sb.Append(JsonUtility.ToJson(_fires));
    //    sb.Append(JsonUtility.ToJson(_fans));
    //    sb.Append(JsonUtility.ToJson(_ventControls));

    //    return sb.ToString();

    //}

    public string SaveToJSON()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var airway in _airways)
        {
            sb.AppendLine(JsonUtility.ToJson(airway.MFAirway));
        }

        foreach (var junction in _junctions)
        {
            sb.AppendLine(JsonUtility.ToJson(junction.MFJunction));
        }

        foreach (var fire in _fires)
        {
            sb.AppendLine(JsonUtility.ToJson(fire.MFFire));
        }

        foreach (var fan in _fans)
        {
            sb.AppendLine(JsonUtility.ToJson(fan.MFFan));
        }

        return sb.ToString();
    }
}
