using NIOSH_EditorLayers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NIOSH_MineCreation
{
    public class VentilationNodeConnection : LayerControlledClass
    {
        private PlacablePrefab _cachedPlaceable;

        private LineRenderer _lineRenderer;

        [SerializeField]
        private List<GameObject> connections;

        private VentGraph _ventGraph;

        public VentAirway nodeConnection;

        public UnityAction attachedToNode;

        public UnityAction OnNodeDestroyed;

        [SerializeField]
        private GameObject airwayPrefab;

        public VentilationConnectionAirway connectionAirway;

        protected override void OnLayerChanged(LayerManager.EditorLayer newLayer)
        {
            if (newLayer == LayerManager.EditorLayer.Ventilation)
            {
                _lineRenderer.enabled = true;
            }
            else
            {
                _lineRenderer.enabled = false;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            base.Start();

            if (connections == null)
                connections = new List<GameObject>();

            _cachedPlaceable = GetComponent<PlacablePrefab>();
            
            _lineRenderer = GetComponent<LineRenderer>();

            _ventGraph = GameObject.Find("VentilationControl").GetComponent<VentilationControl>().VentGraph;
        }

        // Update is called once per frame
        void Update()
        {
            if(connections.Count == 0 && connectionAirway != null)
            {
                Destroy(connectionAirway.gameObject);
            }
        }

        private void OnDestroy()
        {
            base.OnDestroy();
            OnNodeDestroyed?.Invoke();
        }

        public void ClearConnections()
        {
            connections.Clear();
            
            Destroy(gameObject);
        }

        public void AttachToNode(VentilationLayerNode nodeToAttach)
        {
            attachedToNode?.Invoke();
        }

        public void MakeConnection(GameObject newConnection)
        {
            if(connections == null)
                connections = new List<GameObject>();

            connections.Add(newConnection);
            newConnection.GetComponent<VentilationNodeConnection>().attachedToNode += ConnectionPlaced;
            newConnection.GetComponent<VentilationNodeConnection>().OnNodeDestroyed += ClearConnections;

            connectionAirway = Instantiate(airwayPrefab).GetComponent<VentilationConnectionAirway>();
            connectionAirway.SetStartEndPoints(transform, newConnection.transform);

            VentilationConnectionAirway.AirwayBeingConstructed = true;
        }

        private void ConnectionPlaced()
        {
            nodeConnection = new VentAirway();
            VentilationLayerNode startNode = transform.parent.GetComponent<VentilationLayerNode>();
            VentilationLayerNode endNode = connections[0].transform.parent.GetComponent<VentilationLayerNode>();
            nodeConnection.Start = startNode.nodeJunction;
            nodeConnection.End = endNode.nodeJunction;
            _ventGraph.AddAirway(nodeConnection);
            connectionAirway.GetComponent<VentilationConnectionAirway>().SetAirway(nodeConnection);
            connectionAirway.gameObject.layer = LayerMask.NameToLayer("VentVisualization");
            
            connectionAirway.SetStartEndPoints(startNode.transform, endNode.transform);
        }
    }
}