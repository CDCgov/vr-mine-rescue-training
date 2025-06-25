using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MFireProtocol;
//using Sirenix.OdinInspector;


public class MineNetwork : MonoBehaviour, ISerializationCallbackReceiver, IVentGraphBuilder
{
    [System.NonSerialized]
    public MineSegment[] MineSegments;

    public StaticVentilationManager StaticVentilationManager;
    public bool EnableGeometryDeform = false;

    [HideInInspector]
    public float XSlope = 0;
    [HideInInspector]
    public float ZSlope = 0;
    [HideInInspector]
    public float YScale = 1.0f;

    public Vector3 SceneTileScale = Vector3.one;


    public MineSegmentLink[] MineSegmentLinks;

    public event Action MineNetworkInitialized;

    public VentilationProvider VentilationProvider = VentilationProvider.StaticVentilation;

    private bool _initialized = false;

    /*
    //MFire State
    [NonSerialized]
    public Dictionary<int, MineSegment> _junctionMap;
    [NonSerialized]
    public Dictionary<int, MineSegmentLink> _airwayMap;

    private Dictionary<int, Vector3> _junctionPositions;
    private Dictionary<int, Vector3> _airwayPositions;
    */

    //private Transform _geometryParent;
    private MFCConfigureMFire _mfireConfigParameters;

    //private MFireServerControl _serverControl;

    public static void LinkSegments(MineSegment seg1, MineSegment seg2)
    {
        MineNetwork mineNetwork = FindSceneMineNetwork();
        GameObject linkParent = mineNetwork.gameObject;

        GameObject goLink = new GameObject(string.Format("SegLink"));
        goLink.transform.SetParent(linkParent.transform, false);
        MineSegmentLink link = goLink.AddComponent<MineSegmentLink>();
        seg1.CreateSegmentLink(seg2, ref link);

        seg1.AddLink(link);
        seg2.AddLink(link);
        mineNetwork.AddLink(link);
    }

    public static MineNetwork FindSceneMineNetwork()
    {
        GameObject linkParent = GameObject.Find("MineNetwork");
        //if (linkParent == null)
        //{
        //	linkParent = new GameObject("MineNetwork");
        //}
        if (linkParent == null)
            return null;

        MineNetwork mineNetwork = linkParent.GetComponent<MineNetwork>();
        //if (mineNetwork == null)
        //	mineNetwork = linkParent.AddComponent<MineNetwork>();

        return mineNetwork;
    }

    static MineNetwork()
    {

    }

    public MineSegment FindMineSegment(Vector3 worldPos)
    {
        if (MineSegments == null)
            return null;

        //note we may need to write a faster version of this method, or rely on triggers
        for (int i = 0; i < MineSegments.Length; i++)
        {
            MineSegment seg = MineSegments[i];
            if (seg.SegmentBoundsWorldSpace.Contains(worldPos))
            {
                return seg;
            }
        }

        return null;
    }


    public void AddLink(MineSegmentLink link)
    {
        int curCount = 0;
        if (MineSegmentLinks != null)
            curCount = MineSegmentLinks.Length;

        MineSegmentLink[] newarray = new MineSegmentLink[curCount + 1];
        if (MineSegmentLinks != null)
            MineSegmentLinks.CopyTo(newarray, 0);

        newarray[curCount] = link;

        MineSegmentLinks = newarray;
    }

    /*public Transform GetGeometryParent()
	{
		if (_geometryParent == null)
		{
			GameObject geomParent = new GameObject("Geometry");
			geomParent.transform.parent = transform;
			_geometryParent = geomParent.transform;
		}

		return _geometryParent;
	}*/

    void Awake()
    {
        MineSegments = GameObject.FindObjectsOfType<MineSegment>();

        InitializeNetworkLinks();

        //_serverControl = FindObjectOfType<MFireServerControl>();
        //if (_serverControl == null)
        //    Debug.LogWarning($"Mine Network couldn't find MFIRE server control");
    }

    void Start()
    {
    }

    public void OnEnable()
    {
        SceneControl.InitializeSegments += OnInitializeSegments;
        SceneControl.BeginSimulation += OnBeginSimulation;
    }

    public void OnDisable()
    {
        SceneControl.InitializeSegments -= OnInitializeSegments;
        SceneControl.BeginSimulation -= OnBeginSimulation;
    }
    private void OnInitializeSegments()
    {
        int mineSegLayer = LayerMask.NameToLayer("MineSegments");
        foreach (MineSegment seg in MineSegments)
        {
            Vector3 localCenter = seg.SegmentBounds.center;
            localCenter = transform.InverseTransformPoint(localCenter);

            //seg.gameObject.layer = mineSegLayer;


            // BoxCollider zone = seg.gameObject.AddComponent<BoxCollider>();
            // zone.isTrigger = true;
            // zone.center = localCenter;
            // zone.size = seg.SegmentBounds.size;
        }
    }

    private void OnBeginSimulation()
    {
        //InitializeMFireNetwork();
    }


    void QueueSegmentLinks(Queue<MineSegmentLink> queue, MineSegment seg)
    {
        if (seg.MFireJunction <= 0)
            return;

        foreach (MineSegmentLink link in seg.MineSegmentLinks)
        {
            if (link.MFireAirway < 0)
                queue.Enqueue(link);
        }
    }


    void OnDrawGizmos()
    {
        //initialize the links in edit mode
        if (!_initialized)
        {
            InitializeNetworkLinks();
        }


    }

    public static void InitializeLinks()
    {
        MineNetwork network = FindSceneMineNetwork();
        if (network != null)
            network.InitializeNetworkLinks();
    }

    //rebuild the link association ref in each mine segment from the master list
    public void InitializeNetworkLinks()
    {
        if (_initialized || MineSegmentLinks == null)
            return;

        MineSegment[] mineSegments = GameObject.FindObjectsOfType<MineSegment>();
        foreach (MineSegment seg in mineSegments)
        {
            seg.ClearLinks();
        }

        foreach (MineSegmentLink link in MineSegmentLinks)
        {
            if (link.Segment1 != null && link.Segment2 != null)
            {
                link.Segment1.AddLink(link);
                link.Segment2.AddLink(link);
            }
        }

        _initialized = true;

        MineNetworkInitialized?.Invoke();
    }

    public Vector3 DeformWorldSpacePoint(Vector3 v)
    {
        v.y *= YScale;
        v.y += v.x * XSlope;
        v.y += v.z * ZSlope;

        return v;
    }

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
        _initialized = false;
    }

    //temporary map used when building a VentGraph from the mine network
    private Dictionary<MineSegment, VentJunction> _segmentJunctionMap;

    public bool BuildBaseVentGraph(VentGraph ventGraph)
    {
        if (MineSegments == null)
        {
            MineSegments = GameObject.FindObjectsOfType<MineSegment>();

            InitializeNetworkLinks();
        }

        AddJunctionsFromMineNetwork(ventGraph);
        AddAirwaysFromMineNetwork(ventGraph);
        return true;
    }

    public bool BuildVentGraph(VentGraph ventGraph)
    {
        //_airwayMap = new Dictionary<int, VentAirway>();
        //_junctionMap = new Dictionary<int, VentJunction>();
        //_fanMap = new Dictionary<int, VentFan>();
        //_fireMap = new Dictionary<int, VentFire>();

        AddJunctionsFromMineNetwork(ventGraph);
        AddAirwaysFromMineNetwork(ventGraph);
        AddFansFromMineNetwork(ventGraph);
        AddFiresFromMineNetwork(ventGraph);

        return true;
    }

    public bool UpdateVentGraph(VentGraph ventGraph)
    {
        ventGraph.RemoveAllFans();
        ventGraph.RemoveAllFires();

        AddFansFromMineNetwork(ventGraph);
        AddFiresFromMineNetwork(ventGraph);
        AddMethaneGeneratorsFromMineNetwork(ventGraph);

        return true;

    }

    public void AddMethaneGeneratorsFromMineNetwork(VentGraph ventGraph)
    {
        var generators = FindObjectsOfType<VentMethaneGenerator>();
        foreach (var gen in generators)
        {
            var ventAirway = ventGraph.FindClosestAirway(gen.transform.position);
            if (ventAirway != null)
            {
                Debug.Log($"Setting methane emission rate in airway {ventAirway.AirwayID} to {gen.CH4EmissionRate:F2}");

                ventAirway.CH4EmissionRate = gen.CH4EmissionRate;
            }

        }
    }

    public void AddJunctionsFromMineNetwork(VentGraph ventGraph)
    {
        //_junctions = new List<VentJunction>();
        //_junctionMap = new Dictionary<int, VentJunction>();

        _segmentJunctionMap = new Dictionary<MineSegment, VentJunction>();

        //foreach (MineSegment seg in _mineNetwork.MineSegments)
        for (int i = 0; i < MineSegments.Length; i++)
        {
            var seg = MineSegments[i];
            var ventJunction = new VentJunction();

            //MFJunction mfj = new MFJunction();
            //mfj.Number = seg.MFireJunction;
            //mfj.Temperature = 50;
            //mfj.AtmosphereJuncType = 0;//"start junction" is type 1, all others 0, per docs
            //mfj.IsInAtmosphere = seg.IsInAtmosphere;

            if (seg.MFireStartJunction)
            {
                //_mfireConfigParameters.StartJunction = mfj.Number;
                //mfj.AtmosphereJuncType = 1;
                ventJunction.IsStartJunction = true;
                //startSeg = seg;
            }

            //ventJunction.MFJunction = mfj;

            ventJunction.MFireID = -1;
            ventJunction.JunctionID = i;
            ventJunction.WorldPosition = seg.transform.position;
            ventJunction.MFJunction.TotalContaminant = seg.InitialContam;
            ventJunction.MFJunction.ContamConcentration = seg.InitialContamConcentration;
            ventJunction.MFJunction.ContamConcentrationBkp = seg.InitialContamConcentration;

            //int index = _junctions.Count;

            //temporarily set junction index in mine segment for mapping to links
            //seg.MFireJunction = index;

            //_junctions.Add(ventJunction);
            ventGraph.AddJunction(ventJunction);
            _segmentJunctionMap.Add(seg, ventJunction);

            //seg.MFireJunction = -1;
            //foreach (MineSegmentLink link in seg.MineSegmentLinks)
            //{
            //    link.MFireAirway = -1;
            //}


        }
    }

    public void AddAirwaysFromMineNetwork(VentGraph ventGraph)
    {
        //_airways = new List<VentAirway>();

        //foreach (MineSegmentLink link in _mineNetwork.MineSegmentLinks)
        for (int i = 0; i < MineSegmentLinks.Length; i++)
        {
            var link = MineSegmentLinks[i];

            //var startJunc = _junctions[link.Segment1.MFireJunction];
            //var endJunc = _junctions[link.Segment2.MFireJunction];
            try
            {
                var startJunc = _segmentJunctionMap[link.Segment1];
                var endJunc = _segmentJunctionMap[link.Segment2];

                if (link.Segment1 == link.Segment2 || startJunc == endJunc)
                    throw new Exception($"Airway start and end are the same junction! {startJunc.ToString()}");

                var ventAirway = new VentAirway();
                ventAirway.AirwayID = i;
                ventAirway.MFireID = -1;
                ventAirway.Start = startJunc;
                ventAirway.End = endJunc;
                ventAirway.CH4EmissionRate = link.Segment1.CH4EmissionRateCFM;


                //var airway = new MFAirway
                //{
                //    Length = link.ComputeLength() * Constants.MetersToFeet,
                //    Perimeter = 100,
                //    CrossSectionalArea = 140,
                //    FlowRate = 0,
                //    Resistance = 1.5,
                //    FrictionFactor = 50,
                //    Type = 0,
                //};

                //ventAirway.MFAirway = airway;

                //startJunc.LinkedAirways.Add(ventAirway);
                //endJunc.LinkedAirways.Add(ventAirway);
                //_airways.Add(ventAirway);

                ventGraph.AddAirway(ventAirway);

            }
            catch (Exception ex)
            {
                Debug.LogWarning($"VentilationControl: Couldn't add airway: {ex.Message}");
            }

        }
    }

    public void AddFansFromMineNetwork(VentGraph ventGraph)
    {
        var mineFans = FindObjectsOfType<MineFanHost>();
        foreach (var mineFan in mineFans)
        {
            var ventAirway = ventGraph.FindClosestAirway(mineFan.transform.position);
            if (ventAirway != null)
            {
                Debug.Log($"Found fan in airway {ventAirway.AirwayID}");
                VentFan fan = new VentFan();

                if (mineFan.FanData != null)
                    fan.SetFanData(mineFan.FanData);

                fan.WorldPosition = mineFan.transform.position;
                fan.WorldRotation = mineFan.transform.rotation;

                ventGraph.AddFan(fan, ventAirway);
            }

        }
        //_fans = new List<VentFan>();

        //foreach (MineSegment seg in MineSegments)
        //{
        //    foreach (var meHost in seg.ContainedMineElements)
        //    {
        //        if (seg.MFireJunction < 0)
        //            continue;

        //        if (meHost is MineFanHost)
        //        {

        //            if (!meHost.enabled)
        //                continue;

        //            //int airwayNo = -1;
        //            //MineSegmentLink fanLink = null;

        //            //foreach (MineSegmentLink link in seg.MineSegmentLinks)
        //            //{
        //            //    if (link.MFireAirway > 0)
        //            //    {
        //            //        airwayNo = link.MFireAirway;
        //            //        fanLink = link;
        //            //        break;
        //            //    }
        //            //}

        //            var mineFanHost = (MineFanHost)meHost;

        //            //var junction = _segmentJunctionMap[seg];
        //            //if (junction.LinkedAirways.Count <= 0)
        //            //    continue;

        //            var junction = ventGraph.FindClosestJunction(mineFanHost.transform.position);
        //            if (junction == null)
        //            {
        //                Debug.LogError($"Couldn't find junction for fan {mineFanHost.name}");
        //            }



        //            VentFan fan = new VentFan();

        //            if (mineFanHost.FanData != null)
        //                fan.SetFanData(mineFanHost.FanData);

        //            VentAirway airway = junction.LinkedAirways[0];
        //            //fan.Airway = junction.LinkedAirways[0];
        //            //fan.Airway.LinkedFans.Add(fan);
        //            //_fans.Add(fan);
        //            ventGraph.AddFan(fan, airway);
        //        }
        //    }
        //}
    }

    public void AddFiresFromMineNetwork(VentGraph ventGraph)
    {
        //add fires
        var mineFires = FindObjectsOfType<MineFireHost>();
        foreach (var fireHost in mineFires)
        {
            var ventAirway = ventGraph.FindClosestAirway(fireHost.transform.position);
            if (ventAirway == null)
                continue;

            Debug.Log($"Found fire in airway {ventAirway.AirwayID}");
            VentFire fire = new VentFire();
            fireHost.MineFire.CopyTo(fire.MFFire);
            fire.UpdateFire();
            fire.WorldPosition = fireHost.transform.position;
            fire.Airway = ventAirway;
            ventGraph.AddFire(fire);
            fireHost.VentFire = fire;
        }


        //foreach (MineSegment seg in MineSegments)
        //{
        //    foreach (var meHost in seg.ContainedMineElements)
        //    {
        //        if (seg.MFireJunction < 0)
        //            continue;

        //        if (meHost is MineFireHost)
        //        {

        //            var fireHost = (MineFireHost)meHost;

        //            VentFire fire = new VentFire();
        //            fireHost.MineFire.CopyTo(fire.MFFire);
        //            fire.UpdateFire();

        //            var airway = ventGraph.FindClosestAirway(fireHost.transform.position);
        //            if (airway == null)
        //            {
        //                Debug.LogError($"VentControl: Couldn't find airway for fire at {fireHost.transform.position.ToString()}");
        //                continue;
        //            }

        //            fire.WorldPosition = fireHost.transform.position;
        //            fire.Airway = airway;
        //            ventGraph.AddFire(fire);
        //            fireHost.VentFire = fire;

        //            //AddFire(airway.Number, fireHost.MineFire);
        //        }
        //    }
        //}
    }

    ///// <summary>
    ///// Retrieve mine atmosphere information at the specified world position
    ///// </summary>
    ///// <param name="worldPos"></param>
    ///// <returns></returns>
    //public bool GetMineAtmosphere(Vector3 worldPos, out MineAtmosphere mineAtmosphere)
    //{
    //    if (VentilationProvider == VentilationProvider.MFIRE)
    //    {
    //        var junction = FindClosestJunction(worldPos);

    //        mineAtmosphere.Oxygen = UnityEngine.Random.Range(0.16f, 0.21f);

    //        if (junction != null)
    //        {
    //            mineAtmosphere.Methane = (float)junction.CH4Concentration;
    //            mineAtmosphere.CarbonMonoxide = (float)junction.ContamConcentration * 0.5f;
    //        }
    //        else
    //        {
    //            mineAtmosphere.Methane = 0;
    //            mineAtmosphere.CarbonMonoxide = 0.01f;
    //        }
    //        mineAtmosphere.HydrogenSulfide = UnityEngine.Random.Range(0.0f, 0.02f);
    //    }
    //    else
    //    {
    //        if (StaticVentilationManager == null)
    //            StaticVentilationManager = StaticVentilationManager.GetDefault();

    //        StaticVentilationManager.GetMineAtmosphere(worldPos, out mineAtmosphere);
    //    }

    //    return true;

    //}


}