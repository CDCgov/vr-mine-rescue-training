using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NIOSH_EditorLayers;
using DelaunatorSharp;

namespace NIOSH_MineCreation
{
    public class VentilationLayerNode : LayerControlledClass, IScenarioEditorMouseClick, IScenarioEditorMouseMove, IPoint
    {
        private const string ConnectionPreviewObjectName = "VentConnectionPreviewObj";

        public VentLayerManager VentLayerManager;
        public GameObject connectionPrefab;
        public VentJunction nodeJunction;
        public GameObject ConnectionPreviewPrefab;

        private PlacablePrefab _placeable;
        private RaycastHit hit;

        private Placer _placer;
        private MeshRenderer _renderer;        
        private VentGraph _ventGraph;
        private Color _startColor;
        
        //[SerializeField] private GameObject ventManagerPrefab;

        //private VentLayerManager _ventManager;
        private GameObject _connectionPreview;
        private Renderer _connectionPreviewRenderer;


        public bool IsSelectionLocked { get { return false; } }

        public double X { get => transform.position.x; set => throw new System.NotImplementedException(); }
        public double Y { get => transform.position.z; set => throw new System.NotImplementedException(); }

        protected override void OnLayerChanged(LayerManager.EditorLayer newLayer)
        {
            if(newLayer == LayerManager.EditorLayer.Ventilation)
            {
                _renderer.enabled = true;
            }
            else
            {
                _renderer.enabled = false;
            }
        }

        // Start is called before the first frame update
        new void Start()
        {
            base.Start();

            if (VentLayerManager == null)
                VentLayerManager = FindObjectOfType<VentLayerManager>();

            if(_placeable == null)
            {
                _placeable = GetComponent<PlacablePrefab>();
                _placeable.OnPlaced += PrefabPlaced;
            }
            
            GameObject placerGO = GameObject.Find("Placer");
            if(placerGO != null) { _placer = placerGO.GetComponent<Placer>(); }
            

            _renderer = GetComponent<MeshRenderer>();
            _startColor = _renderer.material.color;

            if(_ventGraph == null)
                _ventGraph = GameObject.Find("VentilationControl").GetComponent<VentilationControl>().VentGraph;
            
            //_ventManager = VentLayerManager.Instance;

            //if (_ventManager == null)
            //{
            //    _ventManager = Instantiate(ventManagerPrefab).GetComponent<VentLayerManager>();
            //}

            if (nodeJunction != null && nodeJunction.IsStartJunction)
            {
                StartJunctionRenderChange(true);
            }
            
            CheckInitalLayer();
        }

        public void DetachFromVentGraph()
        {
            _ventGraph = null;
        }


        private void OnDisable()
        {
            //Debug.Log($"VentLayerNode: OnDisable {gameObject.name}");

            if (nodeJunction != null && _ventGraph != null && _ventGraph.FindJunction(nodeJunction.JunctionID) != null)
                _ventGraph.RemoveJunction(nodeJunction.JunctionID);
        }

        new private void OnDestroy()
        {
            //Debug.Log($"VentLayerNode: OnDestroy {gameObject.name}");
            base.OnDestroy();
            
            
            //if(nodeJunction != null && _ventGraph != null && _ventGraph.FindJunction(nodeJunction.JunctionID) != null)
            //    _ventGraph.RemoveJunction(nodeJunction.JunctionID);
        }

        public void StartJunctionRenderChange(bool isStartJunction)
        {
            if (isStartJunction)
            {
                _renderer.material.color = Color.green;
            }
            else
            {
                _renderer.material.color = _startColor;
            }
        }

        // Update is called once per frame
        //void Update()
        //{
        //    if (Input.GetMouseButtonDown(1))
        //    {
        //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("VentVisualization", "SelectedObject")))
        //        {
        //            if (hit.collider.transform == transform && nodeJunction != null && !VentilationConnectionAirway.AirwayBeingConstructed)
        //            {
        //                GameObject firstNode = Instantiate(connectionPrefab, transform.position, transform.rotation);
        //                firstNode.transform.Translate(Vector3.up * 0.5f);
        //                //firstNode.transform.SetParent(transform);

        //                GameObject secondNode = Instantiate(connectionPrefab);
                        

        //                firstNode.GetComponent<VentilationNodeConnection>().MakeConnection(secondNode);

        //                //connection.MakeConnection(secondNode);

        //                _placer.SelectObject(secondNode);

        //                firstNode.transform.SetParent(transform);
        //            }
        //        }
        //    }
        //}

        private void PrefabPlaced(bool isPlaced)
        {
            if (_ventGraph == null)
                return;

            if (isPlaced)
            {
                if (nodeJunction == null)
                {
                    nodeJunction = new VentJunction();
                    nodeJunction.WorldPosition = transform.position;

                    _ventGraph.AddJunction(nodeJunction);
                }
                else
                {
                    nodeJunction.WorldPosition = transform.position;
                    _ventGraph.UpdateJunction(nodeJunction);
                }
            }
        }

        public void ForcePlaced()
        {
            if(_ventGraph == null)
                _ventGraph = GameObject.Find("VentilationControl").GetComponent<VentilationControl>().VentGraph;
            
            if(_placeable == null)
            {
                _placeable = GetComponent<PlacablePrefab>();
                _placeable.OnPlaced += PrefabPlaced;
            }
            
            _placeable.SetPlaced();
        }

        private void GetConnectionPreview()
        {
            if (_connectionPreview != null)
                return;

            if (ConnectionPreviewPrefab == null)
                return;

            _connectionPreview = GameObject.Find(ConnectionPreviewObjectName);
            if (_connectionPreview == null)
            {
                _connectionPreview = GameObject.Instantiate<GameObject>(ConnectionPreviewPrefab);
                _connectionPreview.name = ConnectionPreviewObjectName;                
            }

            _connectionPreviewRenderer = _connectionPreview.GetComponent<Renderer>();

        }

        private void ShowConnectionVisualization(VentilationLayerNode otherNode)
        {
            GetConnectionPreview();

            if (_connectionPreview == null || otherNode == null)
                return;

            VentilationConnectionAirway.PositionConnectionVisual(_connectionPreview.transform,
                transform.position, otherNode.transform.position);
            //_connectionPreview.SetActive(true);

            if (_connectionPreviewRenderer != null)
                _connectionPreviewRenderer.enabled = true;
        }

        private void HideConnectionVisualization()
        {
            if (_connectionPreview == null)
                return;

            //_connectionPreview.SetActive(false);
            if (_connectionPreviewRenderer != null)
                _connectionPreviewRenderer.enabled = false;
        }

        private VentilationLayerNode GetOtherNode(RaycastHit cursorHit)
        {
            if (_ventGraph == null)
                return null;

            VentilationLayerNode otherNode = null;

            if (cursorHit.collider != null && cursorHit.collider.TryGetComponent<VentilationLayerNode>(out otherNode))
            {
                if (_ventGraph.AreJunctionsConnected(nodeJunction, otherNode.nodeJunction))
                    return null;

                return otherNode;
            }

            return null;
        }

        

        public void OnScenarioEditorMouseDown(Placer placer, int button, RaycastHit cursorHit, ScenarioCursorData cursorData)
        {
            if (button != 1 || VentLayerManager == null)
                return;

            VentilationLayerNode otherNode = GetOtherNode(cursorHit);
            if (otherNode != null)
            {
                VentLayerManager.ConnectNodes(this, otherNode);
                placer.SelectObject(otherNode.gameObject);
            }
        }

        public void OnScenarioEditorMouseUp(Placer placer, int button, RaycastHit cursorHit, ScenarioCursorData cursorData)
        {
            
        }

        public void OnScenarioEditorMouseFocusLost(Placer placer)
        {
            HideConnectionVisualization();
        }

        public void OnScenarioEditorMouseMove(Placer placer, RaycastHit cursorHit, ScenarioCursorData cursorData)
        {
            VentilationLayerNode otherNode = GetOtherNode(cursorHit);

            if (otherNode == null)
            {
                HideConnectionVisualization();
                return;
            }

            ShowConnectionVisualization(otherNode);
        }

        public void SetJunctionIsStart(bool value)
        {
            nodeJunction.IsStartJunction = value;
            VentLayerManager.RaiseVentGraphUpdated();
        }

        public void SetJunctionInAtmosphere(bool value)
        {
            nodeJunction.IsInAtmosphere = value;
            VentLayerManager.RaiseVentGraphUpdated();
        }
    }
}