using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Transform = MathNet.Numerics.IntegralTransforms.Transform;
using DelaunatorSharp;
using MFireProtocol;

namespace NIOSH_MineCreation
{
    public class VentLayerManager : MonoBehaviour, ISaveableComponent
    {
        //public static VentLayerManager Instance {  get; private set; }

        [SerializeField] private GameObject junctionPrefab;
        [SerializeField] private GameObject airwayPrefab;
        public bool CreateUIObjects = false;

        private VentGraph _ventGraph;

        private bool _uiObjectsCreated = false;

        public Action VentGraphUpdated;

        //private void Awake() 
        //{
        //    if (Instance != null && Instance != this) 
        //    { 
        //        Destroy(gameObject); 
        //    } 
        //    else 
        //    { 
        //        Instance = this; 
        //    } 
        //}

        private void Start()
        {
            //if(_ventGraph == null)
            //    _ventGraph = GameObject.Find("VentilationControl").GetComponent<VentilationControl>().VentGraph;

            FindSceneVentGraph();

            //temporary fix to assign asset ID on pre-existing vent layer manager
            if (gameObject.TryGetComponent<ObjectInfo>(out var objInfo))
            {
                objInfo.AssetID = "VENT_MANAGER";
            }
        }

        private bool FindSceneVentGraph()
        {
            if (_ventGraph != null)
                return true;

            var ventControl = FindObjectOfType<VentilationControl>();
            if (ventControl == null)
                return false;

            if (ventControl.VentGraph == null)
                ventControl.VentGraph = new VentGraph();

            _ventGraph = ventControl.VentGraph;

            _ventGraph.OnGraphModified += RaiseVentGraphUpdated;

            return true;
        }

        public void RaiseVentGraphUpdated()
        {
            try
            {
                VentGraphUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in ScenarioChanged event handler: {ex.Message} {ex.StackTrace}");
            }
        }

        private void RecreateVentLayerFromVentGraph(VentGraph graph)
        {
            if (!CreateUIObjects)
                return;

            DestroyUIObjects();

            Dictionary<int, VentilationLayerNode> junctionNodeLookup = new Dictionary<int, VentilationLayerNode>();

            // Load junctions to nodes
            //List<VentJunction> junctions = graph.GetJunctions().ToList();
            //for (int i = 0; i < graph.NumJuncions; i++)
            foreach (var junction in graph.GetJunctions())
            {
                VentilationLayerNode newNode = Instantiate(junctionPrefab, junction.WorldPosition, transform.rotation).GetComponent<VentilationLayerNode>();
                junctionNodeLookup.Add(junction.JunctionID, newNode);

                newNode.nodeJunction = junction;
                newNode.ForcePlaced();

                //newNode.nodeJunction.IsStartJunction = junctions[i].IsStartJunction;
                //newNode.nodeJunction.IsInAtmosphere = junctions[i].IsInAtmosphere;
                //newNode.nodeJunction.MFJunction.CH4Concentration = junctions[i].MFJunction.CH4Concentration;
                //newNode.nodeJunction.MFJunction.ContamConcentration = junctions[i].MFJunction.ContamConcentration;
            }

            // Load airways to connections
            //List<VentAirway> airways = graph.GetAirways().ToList();
            //for (int j = 0; j < graph.NumAirways; j++)
            foreach (var airway in graph.GetAirways())
            {
                VentilationConnectionAirway newConneciton = Instantiate(airwayPrefab).GetComponent<VentilationConnectionAirway>();
                newConneciton.SetStartEndPoints(junctionNodeLookup[airway.Start.JunctionID].transform,
                                                junctionNodeLookup[airway.End.JunctionID].transform);

                //VentAirway newAirway = new VentAirway();

                //newAirway.Start = graph.FindJunction(junctionNodeLookup[airways[j].Start.JunctionID].nodeJunction.JunctionID);
                //newAirway.End = graph.FindJunction(junctionNodeLookup[airways[j].End.JunctionID].nodeJunction.JunctionID);

                //newAirway.AddedResistance = airways[j].AddedResistance;
                //newAirway.OverrideResistance = airways[j].OverrideResistance;
                //newAirway.MFAirway.CH4EmissionRateAirway = airways[j].MFAirway.CH4EmissionRateAirway;
                //newAirway.MFAirway.FrictionFactor = airways[j].MFAirway.FrictionFactor;

                //graph.AddAirway(newAirway);
                newConneciton.SetAirway(airway);

                newConneciton.GetComponent<PlacablePrefab>().SetPlaced();
                newConneciton.gameObject.layer = LayerMask.NameToLayer("VentVisualization");

            }

            _uiObjectsCreated = true;
        }

        public string[] SaveInfo()
        {
            if (!FindSceneVentGraph())
                return new string[0];

            //_ventGraph = GameObject.Find("VentilationControl").GetComponent<VentilationControl>().VentGraph;
            VRNVentGraph vrnGraph = new VRNVentGraph();
            _ventGraph.SaveTo(vrnGraph);

            int size = vrnGraph.CalculateSize();

            var _graphSerialized = new byte[size];

            CodedOutputStream memStream = new CodedOutputStream(_graphSerialized);
            vrnGraph.WriteTo(memStream);

            string base64data = Convert.ToBase64String(_graphSerialized);

            return new string[] { "VentGraph|" + base64data };
        }

        public string SaveName()
        {
            return "VentGraphData";
        }

        public void LoadInfo(SavedComponent component)
        {
            string loaded64data = component.GetParamValueAsStringByName("VentGraph");
            var data = Convert.FromBase64String(loaded64data);

            VRNVentGraph vrnGraph = VRNVentGraph.Parser.ParseFrom(data);

            if (!FindSceneVentGraph())
                return;

            _ventGraph.Reset();
            _ventGraph.LoadFrom(vrnGraph);


            if (CreateUIObjects)
            {
                //if (_uiObjectsCreated)
                //    return;

                RecreateVentLayerFromVentGraph(_ventGraph);

                //if (vrnGraph != null)
                //{
                //    VentGraph loadedGraph = new VentGraph();

                //    loadedGraph.LoadFrom(vrnGraph);

                //    RecreateVentLayerFromVentGraph(loadedGraph);
                //}
            }
        }

        public void ConnectNodes(VentilationLayerNode node1, VentilationLayerNode node2)
        {
            if (_ventGraph == null)
                return;


            if (_ventGraph.AreJunctionsConnected(node1.nodeJunction, node2.nodeJunction))
            {
                Debug.LogWarning($"VentLayerManager: Attempted to connect already connected junctions {node1.nodeJunction.JunctionID} {node2.nodeJunction.JunctionID}");
                return;
            }

            if (node1.nodeJunction.JunctionID == node2.nodeJunction.JunctionID)
            {
                Debug.LogWarning($"VentLayerManager: Attempted to connected junction to itself");
                return;
            }

            VentilationConnectionAirway newConneciton = Instantiate(airwayPrefab).GetComponent<VentilationConnectionAirway>();
            newConneciton.SetStartEndPoints(node1.transform,
                node2.transform);

            VentAirway newAirway = new VentAirway();

            newAirway.Start = node1.nodeJunction;
            newAirway.End = node2.nodeJunction;

            _ventGraph.AddAirway(newAirway);
            newConneciton.SetAirway(newAirway);

            newConneciton.GetComponent<PlacablePrefab>().SetPlaced();
            newConneciton.gameObject.layer = LayerMask.NameToLayer("VentVisualization");
        }

        public void DestroyUIObjects()
        {
            //List<VentilationLayerNode> allNodes = new List<VentilationLayerNode>(FindObjectsOfType<VentilationLayerNode>());
            var allNodes = FindObjectsOfType<VentilationLayerNode>();

            for (int i = 0; i < allNodes.Length; i++)
            {
                //detach from vent graph to prevent removing any newly loaded nodes in OnDestroy
                allNodes[i].DetachFromVentGraph();
                Destroy(allNodes[i].gameObject);
            }

            var allAirways = FindObjectsOfType<VentilationConnectionAirway>();

            for (int i = 0; i < allAirways.Length; i++)
            {
                //detach from vent graph to prevent removing any newly loaded airways in OnDestroy
                allAirways[i].DetachFromVentGraph();
                Destroy(allAirways[i].gameObject);
            }

            _uiObjectsCreated = false;
        }

        public void AutoGenerateVentLayer()
        {
            if (!FindSceneVentGraph())
                return;

            DestroyUIObjects();

            _ventGraph.Reset();

            List<MineLayerTile> allTiles = new List<MineLayerTile>(FindObjectsOfType<MineLayerTile>());

            Dictionary<MineLayerTile, VentilationLayerNode> tileNodeLookup =
                new Dictionary<MineLayerTile, VentilationLayerNode>();

            for (int i = 0; i < allTiles.Count; i++)
            {
                VentilationLayerNode newNode = Instantiate(junctionPrefab, allTiles[i].transform.position + new Vector3(0, 1, 0), transform.rotation).GetComponent<VentilationLayerNode>();

                newNode.ForcePlaced();

                tileNodeLookup.Add(allTiles[i], newNode);

                for (int j = 0; j < allTiles[i].Connections.Length; j++)
                {
                    if (allTiles[i].Connections[j] != null && tileNodeLookup.ContainsKey(allTiles[i].Connections[j]))
                    {
                        VentilationLayerNode connectingNode = tileNodeLookup[allTiles[i].Connections[j]];

                        ConnectNodes(newNode, connectingNode);

                        //VentilationConnectionAirway newConneciton = Instantiate(airwayPrefab).GetComponent<VentilationConnectionAirway>();
                        //newConneciton.SetStartEndPoints(newNode.transform,
                        //    connectingNode.transform);

                        //VentAirway newAirway = new VentAirway();

                        //newAirway.Start = newNode.nodeJunction;
                        //newAirway.End = connectingNode.nodeJunction;

                        //_ventGraph.AddAirway(newAirway);
                        //newConneciton.SetAirway(newAirway);

                        //newConneciton.GetComponent<PlacablePrefab>().SetPlaced();
                        //newConneciton.gameObject.layer = LayerMask.NameToLayer("VentVisualization");
                    }
                }
            }
        }

        public IEnumerator AutoConnectVentNodes()
        {
            // Remove all airways
            foreach (VentilationConnectionAirway ventAirway in FindObjectsOfType<VentilationConnectionAirway>())
                Destroy(ventAirway.gameObject);

            while(FindObjectsOfType<VentilationConnectionAirway>().Length > 0 &&
                _ventGraph.GetAirways().Count() > 0)
            {
                yield return null;
            }

            if (!FindSceneVentGraph())
                yield break;

            // Build a mesh from vent nodes to make initial connections
            Delaunator delaunator = new Delaunator(FindObjectsOfType<VentilationLayerNode>().ToArray());

            delaunator.ForEachTriangleEdge(edge =>
            {
                RaycastHit raycastHit;
                int layerMask = LayerMask.GetMask("Floor");
                Vector3 startPos = ((VentilationLayerNode)edge.P).transform.position + Vector3.up;
                Vector3 direction = ((VentilationLayerNode)edge.Q).transform.position - 
                                    ((VentilationLayerNode)edge.P).transform.position;
                Ray ray = new Ray(startPos, direction);

                Physics.Raycast(ray,
                    out raycastHit,
                    Vector3.Distance(startPos, ((VentilationLayerNode)edge.Q).transform.position),
                    layerMask);

                // Connect nodes that dont connect through the "Floor" layer
                if(raycastHit.collider == null)
                {
                    ConnectNodes((VentilationLayerNode)edge.P, (VentilationLayerNode)edge.Q);
                }
            });

            yield return null;

            List<VentJunction> juncs = new List<VentJunction>(_ventGraph.GetJunctions());

            // Start to check junction connections using "Reverse A star" function
            foreach(VentJunction junction in juncs)
            {
                List<VentAirway> airways = new List<VentAirway>(junction.LinkedAirways);

                foreach(VentAirway airway in airways)
                {
                    VentJunction connection;

                    // Make sure we grab the other node in the connection
                    if (airway.Start == junction)
                        connection = airway.End;
                    else
                        connection = airway.Start;

                    if(ReverseAStar(junction, connection, airway, 0))
                    {
                        // Remove airway by destroying the viz object
                        foreach (VentilationConnectionAirway ventAirway in FindObjectsOfType<VentilationConnectionAirway>())
                        {
                            if (ventAirway.GetAirway().AirwayID == airway.AirwayID)
                            {
                                Destroy(ventAirway.gameObject);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool ReverseAStar(VentJunction start, VentJunction target, VentAirway query, int step)
        {
            if(start == target)
            {
                return true;
            }

            // Try not to step too far so we dont loop around pillars
            if (step >= 2)
                return false;

            List<VentAirway> airways = new List<VentAirway>(start.LinkedAirways);

            foreach (VentAirway airway in airways)
            {
                if(airway == query)
                    continue;

                // These arnt set when made so we need to force the airways to calculate their length now.
                airway.CalculateLength();
                query.CalculateLength();

                // Only continue if the length of the path is shorter than the connection we are testing against.
                if(airway.MFAirway.Length < query.MFAirway.Length)
                {
                    // Make sure we grab the other node in the connection.
                    if (airway.Start == start)
                    {
                        if (ReverseAStar(airway.End, target, query, step + 1))
                            return true;
                    }
                    else
                    {
                        if (ReverseAStar(airway.Start, target, query, step + 1))
                            return true;
                    }
                }
            }

            // Couldnt get to the target from here.
            return false;
        }
    }
}