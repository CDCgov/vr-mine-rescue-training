using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NIOSH_EditorLayers;

public enum NodeAddMode
{
    AddModeOff,
    AddHangingNode,
    AddFixedNode,
    AddFloorNode,
}

public class RuntimeCableEditor : SceneManagerBase
{
    public static RuntimeCableEditor GetDefault(GameObject self)
    {
        return Util.GetDefaultManager<RuntimeCableEditor>(self, "RuntimeCableEditor", false);
    }

    //public Placer _placer;

    /// Add Nodes
    public Button addRoofNodeButton;
   // public Button addFixedNodeButton;
    public Button addFloorNodeButton;

    /// Swap Nodes 
    public Button swapToRoofNodeButton;
    public Button swapToFixedNodeButton;
    public Button swapToFloorNodeButton;

    /// Edit Slope
    public Button resetSlopeButton;
    public Button increaseSlopeButton;
    public Button decreaseSlopeButton;
    
    [SerializeField] LayerMask layerMask;

    ///Targeting
    InputTargetController.InputTarget inputTarget;
    //public int targetIndex;
    //public int lastTargetIndex;

    [SerializeField] Vector3 defaultNodeSize = new Vector3(.15f, .15f, .15f);
    [SerializeField] Vector3 headNodeSize = new Vector3(.35f, .35f, .35f);

    ///States
    //public bool dragging;    
    //public bool hoveringOverNode;
    //[System.NonSerialized]
    public NodeAddMode NodeAddMode
    {
        get { return _nodeAddMode; }
        set
        {
            if (_nodeAddMode != value)
            {
                _nodeAddMode = value;
                if (_nodeAddMode == NodeAddMode.AddModeOff && AreRoofboltPreviewsActive)
                {
                    ClearBoltPreviews();
                }
            }
        }
    }
    //bool _startDragging;

    ///Components

    public float SlopeIncrement = 0.04f;
    
    ContextMenuController _contextMenu;
    GameObject contextMenuUI;
    LayerManager.EditorLayer editorLayer;
    InputTargetController inputTargetController;
    Camera sceneCamera;

    //public ComponentInfo_Cable data;

    public Color SelectedColor;
    public Color HoveredColor;
    public Color ActiveColor;
    public Color InactiveColor;
    //public float visibleAlpha;
    //public float hiddenAlpha;

    public GameObject NodeGizmoPrefab;
    public GameObject NodePreviewPrefab;
    public GameObject BoltPreviewPrefab;

    [System.NonSerialized]
    public PlaceableCable SelectedCable;
    [System.NonSerialized]
    public ComponentInfo_Lifeline SelectedLifeline;

    public bool AreRoofboltPreviewsActive { get; private set; }

    GameObject lastHoveredNode;
    GameObject currentNode;

    private bool _hidden;
    private GlobalCableData globalCableData;
    private RuntimeMarkerEditor markerEditor;
    private Transform _nodePreview;

    private Collider[] _colliderHits = new Collider[150];
    private List<GameObject> _boltPreviews = new List<GameObject>(150);
    private int _boltLayerMask;

    private NodeAddMode _nodeAddMode;

    #region UnityMethods

    private void Awake()
    {
        globalCableData = FindObjectOfType<GlobalCableData>();
        //_contextMenu = FindObjectOfType<ContextMenuController>();
        _contextMenu = GetComponent<ContextMenuController>();
        //_placer = FindObjectOfType<Placer>();

        _boltLayerMask = LayerMask.GetMask("RoofBolts");

        AreRoofboltPreviewsActive = false;
    }
    void Start()
    {
        //Placer = FindObjectOfType<Placer>();
        markerEditor = GetComponent<RuntimeMarkerEditor>();
        contextMenuUI = _contextMenu.contextMenuUI;
        sceneCamera = Camera.main;
        inputTargetController = FindObjectOfType<InputTargetController>();


        LayerManager.Instance.layerChanged += OnNewLayer;
        if (inputTargetController) inputTargetController.onNewInputTarget += OnNewInputTarget;
        addRoofNodeButton.onClick.AddListener(StartAddRoofNode);
        addFloorNodeButton.onClick.AddListener(StartAddFloorNode);
        resetSlopeButton.onClick.AddListener(ResetCableSlack);
        increaseSlopeButton.onClick.AddListener(IncreaseCableSlack);
        decreaseSlopeButton.onClick.AddListener(DecreaseCableSlack);
        // if(placer)placer.onObjectDeselected += DeselectObject;


    }
    private void OnDestroy()
    {
        LayerManager.Instance.layerChanged -= OnNewLayer;
        if (inputTargetController) inputTargetController.onNewInputTarget -= OnNewInputTarget;
        addRoofNodeButton.onClick?.RemoveListener(StartAddRoofNode);
        addFloorNodeButton.onClick?.RemoveListener(StartAddFloorNode);
        resetSlopeButton.onClick?.RemoveListener(ResetCableSlack);
        increaseSlopeButton.onClick?.RemoveListener(IncreaseCableSlack);
        decreaseSlopeButton.onClick?.RemoveListener(DecreaseCableSlack);
        // if (placer) placer.onObjectDeselected -= DeselectObject;

        if (_boltPreviews != null)
        {
            foreach (var bolt in _boltPreviews)
            {
                if (bolt != null)
                    Destroy(bolt);
            }

            _boltPreviews.Clear();
        }

    }

    public void ShowBoltPreviews(Vector3 pos)
    {
        int count = Physics.OverlapSphereNonAlloc(pos, 4, _colliderHits, _boltLayerMask);
        AreRoofboltPreviewsActive = true;

        for (int i = 0; i < count; i++)
        {
            if (i >= _boltPreviews.Count)
            {
                _boltPreviews.Add(CreateBoltPreview());
            }

            _boltPreviews[i].SetActive(true);
            _boltPreviews[i].transform.position = _colliderHits[i].transform.position;
        }

        for (int i = count; i < _boltPreviews.Count; i++)
        {
            _boltPreviews[i].SetActive(false);
        }
    }

    private GameObject CreateBoltPreview()
    {        
        if (BoltPreviewPrefab == null)
            return GameObject.CreatePrimitive(PrimitiveType.Sphere);

        return Instantiate<GameObject>(BoltPreviewPrefab);
    }

    public void ClearBoltPreviews()
    {
        AreRoofboltPreviewsActive = false;

        if (_boltPreviews == null)
            return;

        foreach (var bolt in _boltPreviews)
        {
            if (bolt != null)
                bolt.SetActive(false);
        }

    }

    //void Update()
    //{
    //    if(editorLayer == LayerManager.EditorLayer.Cables)
    //    {
    //        if (dragging)
    //        {
    //            RepositionNode();
    //        }
    //        else
    //        {
    //            CheckForNodeSelection(LayerMask.GetMask("Gizmo", "Default"));
    //            if (!hoveringOverNode && contextMenu.target != null)
    //            {
    //                ///Determine the layer to look for and show hanger
    //                if (nodeAddMode == NodeAddMode.AddHangingNode) PreviewAddCableNode(layerMask, true, true);
    //                if (nodeAddMode == NodeAddMode.AddFixedNode) PreviewAddCableNode(layerMask, true, false);
    //                if (nodeAddMode == NodeAddMode.AddFloorNode) PreviewAddCableNode(layerMask, false, false);
    //            }
                
    //            else if (contextMenu.target == null && nodePreview.gameObject.activeInHierarchy)//To Do : Set this up to run on descrete event rather than in update
    //            {
    //                nodePreview.gameObject.SetActive(false);
    //            }
    //        }
    //    }
    //}

    #endregion
    

    ///// <summary>
    ///// Check to see if a node is being over and if clicked, select it
    ///// </summary>
    ///// <param name="mask"></param>
    //void CheckForNodeSelection(LayerMask mask)
    //{
    //    //if (EventSystem.current.IsPointerOverGameObject()) return;
        
    //    //re
    //    if (markerEditor.HoveringOverMarker) return; 
        
    //    if (inputTarget == InputTargetController.InputTarget.Viewport)
    //    {
    //        Ray mouseRay = sceneCamera.ScreenPointToRay(Input.mousePosition);
    //        RaycastHit hit;

    //        ///Check for selectable Node
    //        if (Physics.Raycast(mouseRay, out hit, 100, mask, QueryTriggerInteraction.Ignore))
    //        //if (Physics.Raycast(mouseRay, out hit, 100, mask))
    //            {
    //            ///detect node
    //            if (hit.collider.gameObject.TryGetComponent(out NodeGizmo nodeGizmo))
    //            {
    //                hoveringOverNode = true;
                    
    //                ///turn preview off when hovering
    //                if (nodeAddMode != NodeAddMode.AddModeOff) { nodePreview.gameObject.SetActive(false); }
                    
    //                ///Set hovered state
    //                CursorImageController.instance.cursorState = CursorImageController.CursorImage.HandOpen;
                    
    //                if (lastHoveredNode == null || lastHoveredNode != nodeGizmo.gameObject)
    //                {
    //                    lastHoveredNode = nodeGizmo.gameObject;

    //                    if (lastHoveredNode.transform != contextMenu.target)
    //                    {
    //                        lastHoveredNode.GetComponent<Renderer>().material.color = hoveredColor;
    //                        AdjustNodeTransparency(lastHoveredNode);
    //                    }
    //                }

                    
    //                ///Select node
    //                if (Input.GetMouseButtonDown(0))
    //                {
    //                    //assign targeting data
    //                    SetNewTargetData(nodeGizmo);
    //                    contextMenu.SetTarget(nodeGizmo.transform);
    //                    CursorImageController.instance.cursorState = CursorImageController.CursorImage.HandClosed;
    //                    dragging = true;
    //                }
    //                //else
    //                {
    //                    ///Set hovered state
    //                    CursorImageController.instance.cursorState = CursorImageController.CursorImage.HandOpen;
    //                    if(lastHoveredNode == null || lastHoveredNode != nodeGizmo.gameObject)
    //                    {
    //                        lastHoveredNode = nodeGizmo.gameObject;

    //                        if (lastHoveredNode.transform != contextMenu.target)
    //                        {
    //                            lastHoveredNode.GetComponent<Renderer>().material.color = hoveredColor;
    //                            AdjustNodeTransparency(lastHoveredNode);
    //                        }
    //                    }

    //                }
    //            }
    //            /*
    //            ///Clear hovered state
    //            if (lastHoveredNode != null && lastHoveredNode.transform != contextMenu.target)
    //            {
    //                lastHoveredNode.GetComponent<Renderer>().material.color = defaultColor;
    //                AdjustNodeTransparency(lastHoveredNode);
    //                lastHoveredNode = null;
    //            }*/
    //        }
    //        else
    //        {
    //            hoveringOverNode = false;
    //            ///Clear hovered state
    //            if (lastHoveredNode != null && lastHoveredNode.transform != contextMenu.target)
    //            {
    //                lastHoveredNode.GetComponent<Renderer>().material.color = defaultColor;
    //                AdjustNodeTransparency(lastHoveredNode);
    //                lastHoveredNode = null;
    //            }
    //            ///turn preview back on
    //            if (nodeAddMode != NodeAddMode.AddModeOff) { nodePreview.gameObject.SetActive(true); }
    //        }
    //    }
    //    else
    //    {
    //        hoveringOverNode = false;
    //        ///turn preview back on
    //        if (nodeAddMode != NodeAddMode.AddModeOff) { nodePreview.gameObject.SetActive(true); }
    //    }
    //}
    
    //void RepositionNode()
    //{
    //    //if (EventSystem.current.IsPointerOverGameObject()) return;
    //    if (Input.GetMouseButtonUp(0))
    //    {
    //        dragging = false;
    //        _startDragging = false;
    //        contextMenuUI.gameObject.SetActive(true);
    //        return;
    //    }

    //    if (!_startDragging)
    //    {
    //        contextMenuUI.gameObject.SetActive(false);
    //        _startDragging = true;
    //    }

    //    if (inputTarget == InputTargetController.InputTarget.Viewport)
    //    {
    //        Ray mouseRay = sceneCamera.ScreenPointToRay(Input.mousePosition);
    //        RaycastHit hit;

    //        if (Physics.Raycast(mouseRay, out hit, 100, layerMask, QueryTriggerInteraction.Ignore))
    //        {
    //            /// Reposition Node
    //            Vector3 nodePos = hit.point;
    //            nodePos += (data.Component.CableDiameter * 0.7f) * hit.normal;
    //            CurrentNode().Position = nodePos;
    //            CurrentGizmo().transform.position = nodePos;
    //            data.Component.RegenerateMesh();
    //        }
    //    }
    //}

    /// <summary>
    ///// Creates and places a node preview that follows the mouse ray hit position
    ///// </summary>
    //void PreviewAddCableNode(LayerMask mask, bool showHanger, bool snapToBolt)
    //{
    //    if(inputTarget == InputTargetController.InputTarget.Viewport)
    //    {
    //        if (EventSystem.current.IsPointerOverGameObject()) return;
    //        Ray mouseRay = sceneCamera.ScreenPointToRay(Input.mousePosition);

    //        Vector3 nodePos = Vector3.zero;
    //        RaycastHit hit;

    //        ///Check for pladata.m_component surface
    //        if (Physics.Raycast(mouseRay, out hit, 100, mask, QueryTriggerInteraction.Ignore))
    //        {
                
    //            nodePos = hit.point;
    //            nodePos += (data.Component.CableDiameter * 0.7f) * hit.normal;
    //            _nodePreview.position = nodePos;
    //            CursorImageController.instance.cursorState = CursorImageController.CursorImage.HandPointing;

    //            if (Input.GetMouseButtonDown(0))
    //            {
    //                AddNode(nodePos, showHanger);
    //            }
    //        }
    //        else
    //        {
    //            CursorImageController.instance.cursorState = CursorImageController.CursorImage.Arrow;
    //        }
    //    }

    //}

   
    /*
    /// <summary>
    /// Create a node Gizmo for each pre-existing node in a data.m_component on startup
    /// </summary>
    void InstantiateNodeGizmos()
    {
        foreach (HangingGeometry.CableNode node in data.m_component.CableNodes)
        {

            var newNode = Instantiate(data.nodePrefab, data.transform);
            newNode.transform.position = node.Position;
            data.nodeGizmos.Add(newNode);
            //newNode.GetComponent<NodeGizmo>().index = node.Get;
        }
        data.lastHeadNode = data.nodeGizmos[data.nodeGizmos.Count - 1];
        ResizeLastNode();
    }*/

    #region NodeHelpers
    //public void SettleFirstNode()
    //{
    //    data.Component.CableNodes[0].Position = data.NodeGizmos[0].transform.position;
    //}
    HangingGeometry.CableNode CurrentNode()
    {
        if (SelectedCable == null || SelectedCable.SelectedNode == null)
            return null;
        //return SelectedCable.CableInfo.CableNodes[targetIndex];
        return SelectedCable.SelectedNode.CableNode;
    }

    public GameObject CurrentGizmo()
    {
        if (SelectedCable == null || SelectedCable.SelectedNode == null)
            return null;

        //return SelectedCable.NodeGizmos[targetIndex];
        return SelectedCable.SelectedNode.gameObject;
    }

    //HangingGeometry.CableNode HeadNode()
    //{
    //    return SelectedCable.CableInfo.CableNodes[SelectedCable.CableInfo.CableNodes.Count - 1];
    //}

    //GameObject HeadGizmo()
    //{
    //    return data.NodeGizmos[data.NodeGizmos.Count - 1];
    //}

    //void ResizeLastNode()
    //{
    //    data.LastHeadNode.transform.localScale = defaultNodeSize;
    //    data.LastHeadNode = HeadGizmo();
    //    data.LastHeadNode.transform.localScale = headNodeSize;
    //}

    //public void ToggleHideGizmos()
    //{
    //    data.GizmosHidden = !data.GizmosHidden;
    //    foreach (GameObject g in data.NodeGizmos)
    //    {
    //        g.SetActive(!data.GizmosHidden);
    //    }
    //}

    #endregion

    #region Events/Actions

    void OnNewInputTarget(InputTargetController.InputTarget _inputTarget)
    {
        inputTarget = _inputTarget;

       
    }

    void OnNewLayer(LayerManager.EditorLayer _editorLayer)
    {
        //editorLayer = _editorLayer;
        //nodeAddMode = NodeAddMode.AddModeOff;
        //if (contextMenu.target != null )
        //{
        //    NodeGizmo nodeGizmo = contextMenu.target.GetComponent<NodeGizmo>();
        //    if (editorLayer == LayerManager.EditorLayer.Cables && nodeGizmo != null) SetNewTargetData(nodeGizmo);
        //}
    }

    public void SetSelectedCable(PlaceableCable cable)
    {
        if (cable == null)
        {
            SelectedCable = null;
            SelectedLifeline = null;
            return;
        }

        SelectedCable = cable;
        cable.TryGetComponent<ComponentInfo_Lifeline>(out SelectedLifeline);

        //if (cable.TryGetComponent<ComponentInfo_Lifeline>(out SelectedLifeline))
        //{
        //    markerEditor._info = SelectedLifeline;
        //}
        //else
        //{
        //    markerEditor._info = null;
        //}


    }

    //void SetNewTargetData(NodeGizmo nodeGizmo)
    //{
    //    //data = nodeGizmo.GetComponentInParent<ComponentInfo_Cable>();
    //    if (markerEditor._info != nodeGizmo.GetComponentInParent<ComponentInfo_Lifeline>() || data == null)
    //    {
    //        markerEditor._info = nodeGizmo.GetComponentInParent<ComponentInfo_Lifeline>();
    //    }
    //    var placable = data.GetComponentInParent<PlacablePrefab>();
    //    if(placable!= null) 
    //    {
    //        //_placer.activeLogic.selectedObject = placable;
    //        _placer.SelectObject(placable.gameObject);
    //    }
        
    //}

    #endregion
    
    #region Buttons/UI

    /// <summary>
    /// Position UI at current selected node position in viewport
    /// </summary>
    public void RemoveCurrentNode()
    {
        if (SelectedCable == null || SelectedCable.SelectedNode == null || 
            SelectedCable.SelectedNode.CableNode == null || SelectedCable.CableInfo == null)
            return;

        //don't remove last node
        if (SelectedCable.CableInfo.CableNodes == null || SelectedCable.CableInfo.CableNodes.Count <= 1)
            return;

        SelectedCable.CableInfo.RemoveNode(SelectedCable.SelectedNode.CableNode.NodeIndex);
        SelectedCable.CableInfo.RegenerateMesh();

        //if(targetIndex != 0)
        //{
        //    GameObject doomedGizmo = data.NodeGizmos[targetIndex];
        //    data.NodeGizmos.Remove(doomedGizmo);
        //    Destroy(doomedGizmo);

        //    data.Component.CableNodes.Remove(CurrentNode());

        //    data.Component.RegenerateMesh();

        //    /// if index is out of range, set to range max
        //    if (targetIndex > data.NodeGizmos.Count - 1)
        //    {
        //        targetIndex = data.NodeGizmos.Count - 1;
        //    }
        //}
        //else
        //{
        //    ///To Do: delete data.m_component system object if last node
        //}
        


    }

    void StartAddRoofNode()
    {
        NodeAddMode = NodeAddMode.AddHangingNode;

        if (inputTargetController != null)
            inputTargetController.SetInputTargetToViewPort();
    }

    void StartAddFloorNode()
    {
        NodeAddMode = NodeAddMode.AddFloorNode;

        if (inputTargetController != null)
            inputTargetController.SetInputTargetToViewPort();
    }

    void ResetCableSlack()
    {
        if (SelectedCable == null || SelectedCable.SelectedNode == null || SelectedCable.SelectedNode.CableNode == null)
            return;

        SelectedCable.SelectedNode.CableNode.Slope = SelectedCable.CableInfo.CableSlack;
        SelectedCable.CableInfo.RegenerateMesh();
        //CurrentNode().Slope = -1f;
        
        //CurrentNode().Slope = data.CableSlack;
        //data.Component.RegenerateMesh();
    }

    private RoofBolt GetClosestRoofbolt(Vector3 pos)
    {
        int count = Physics.OverlapSphereNonAlloc(pos, 5.0f, _colliderHits, _boltLayerMask);
        if (count <= 0)
            return null;

        return GetClosestRoofbolt(count, pos);
    }


    private RoofBolt GetClosestRoofbolt(int count, Vector3 pos)
    {
        //int count = Physics.OverlapSphereNonAlloc(pos, 5.0f, _colliderHits, _boltLayerMask);
        //if (count <= 0)
        //    return null;

        RoofBolt closest = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var hit = _colliderHits[i];

            if (!hit.transform.TryGetComponent<RoofBolt>(out var roofbolt))
                continue;

            var dist = Vector3.Distance(pos, roofbolt.transform.position);
            if (dist < minDist)
            {
                closest = roofbolt;
                minDist = dist;
            }
        }

        return closest;
    }


    public void ChangeBoltType(NodeAddMode nodeType)
    {
        if (SelectedCable == null ||
            SelectedCable.SelectedNode == null ||
            SelectedCable.SelectedNode.CableNode == null)
            return;

        var node = SelectedCable.SelectedNode.CableNode;

        if (nodeType == NodeAddMode.AddHangingNode)
        {
            node.ShowCableHanger = true;
            var bolt = GetClosestRoofbolt(node.Position);
            if (bolt != null)
            {
                var pos = bolt.GetHookPositionWorldSpace(0);
                SelectedCable.SelectedNode.transform.position = pos;
                node.Position = pos;
            }
        }
        else
        {
            node.ShowCableHanger = false;
        }

        SelectedCable.CableInfo.RegenerateMesh();
    }

    void ChangeCableSlack(float offset)
    {
        if (SelectedCable == null || SelectedCable.CableInfo == null || SelectedCable.SelectedNode == null || SelectedCable.SelectedNodeIndex < 0)
            return;

        HangingGeometry.CableNode node;
        if (SelectedCable.SelectedNodeIndex == SelectedCable.CableInfo.CableNodes.Count - 1 && SelectedCable.CableInfo.CableNodes.Count > 1)
            node = SelectedCable.CableInfo.CableNodes[SelectedCable.SelectedNodeIndex];
        else
            node = SelectedCable.SelectedNode.CableNode;

        node.Slope = Mathf.Clamp(node.Slope + offset, 0, 1.5f);

        SelectedCable.CableInfo.RegenerateMesh();

    }

    void IncreaseCableSlack()
    {
        ChangeCableSlack(Mathf.Abs(SlopeIncrement));

        //Debug.Log("IncreaseSlope");
        //if (SelectedCable == null || SelectedCable.CableInfo == null)
        //    return;

        //if (targetIndex +1 == SelectedCable.CableInfo.CableNodes.Count)
        //{
        //    var lastNode = SelectedCable.CableInfo.CableNodes[targetIndex - 1];
        //    Mathf.Clamp(lastNode.Slope -= SlopeIncrement, 0, 1.5f);
        //    if (lastNode.Slope < SlopeIncrement) { lastNode.Slope = SlopeIncrement; }
            
        //}
        //else
        //{
        //    Mathf.Clamp(CurrentNode().Slope -= SlopeIncrement, 0, 1.5f);
        //    if (CurrentNode().Slope < SlopeIncrement) { CurrentNode().Slope = SlopeIncrement; }
            
            
        //}
        //SelectedCable.CableInfo.RegenerateMesh();
        
    }
    void DecreaseCableSlack()
    {
        ChangeCableSlack(Mathf.Abs(SlopeIncrement) * -1.0f);

        //if (SelectedCable == null || SelectedCable.CableInfo == null)
        //    return;

        //Debug.Log("DecreaseSlope");
        //if (targetIndex + 1 == SelectedCable.CableInfo.CableNodes.Count)
        //{
        //    var lastNode = SelectedCable.CableInfo.CableNodes[targetIndex - 1];
        //    Mathf.Clamp(lastNode.Slope += SlopeIncrement, 0, 1.5f);
        //    if (lastNode.Slope < 0) { lastNode.Slope = 0; }
        //}
        //else 
        //{
        //    Mathf.Clamp(CurrentNode().Slope += SlopeIncrement, 0, 1.5f);
        //    if (CurrentNode().Slope < 0) CurrentNode().Slope = 0;
        //}
        //SelectedCable.CableInfo.RegenerateMesh();


    }
    public void SetAllCableSlack(float value, bool state)
    {
        if (SelectedCable == null || SelectedCable.CableInfo == null)
            return;

        SelectedCable.CableInfo.CableSlack = value;
        //data.Component.DefaultCableSlope = value;
        ResetAllCableSlack();
    }

    public void ResetAllCableSlack()
    {
        if (SelectedCable == null || SelectedCable.CableInfo == null || SelectedCable.CableInfo.CableNodes == null)
            return;

        foreach(HangingGeometry.CableNode node in SelectedCable.CableInfo.CableNodes)
        {
            node.Slope = SelectedCable.CableInfo.CableSlack;
        }
        SelectedCable.CableInfo.RegenerateMesh();
    }


    #endregion

    //public void ToggleVisibility(bool state)
    //{
    //    _hidden = state;
    //    foreach (GameObject node in data.NodeGizmos)
    //    {
    //        AdjustNodeTransparency(node);
    //    }

    //    //swap icon
    //}

    //public void OnDeselectNode(GameObject lastNode)
    //{
    //    //set the last material back to default
       
    //    lastNode.GetComponent<Renderer>().material.color = defaultColor;
    //    AdjustNodeTransparency(lastNode);

    //}

    //public void OnSelectNode(GameObject node)
    //{
    //    targetIndex = data.NodeGizmos.IndexOf(node);
    //    node.GetComponent<Renderer>().material.color = selectedColor;
    //    AdjustNodeTransparency(node);
    //}

    //void AdjustNodeTransparency(GameObject node)
    //{     
    //    var mat = node.GetComponent<Renderer>().material;
    //    var newColor = mat.color;
    //    if (_hidden) 
    //    { 
    //        newColor.a = hiddenAlpha; 
    //        defaultColor.a = hiddenAlpha;
    //        hoveredColor.a = hiddenAlpha;
    //        selectedColor.a = hiddenAlpha;

    //    }
    //    else
    //    {
    //        newColor.a = visibleAlpha; 
    //        defaultColor.a = visibleAlpha;
    //        hoveredColor.a = visibleAlpha;
    //        selectedColor.a = visibleAlpha;
    //    }
    //    node.GetComponent<Renderer>().material.color =  newColor;
    //}





    /*
    void DeselectObject() // not working. Node re appears after deselection. Could be race condition turns it back on?
    {
        contextMenu.target = null;
        if (nodePreview.gameObject.activeInHierarchy)
        {
            nodePreview.gameObject.SetActive(false);
        }
    }
    */
}
