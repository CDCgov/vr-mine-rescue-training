using NIOSH_EditorLayers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ComponentInfo_Cable))]
public class PlaceableCable : NIOSH_EditorLayers.LayerControlledClass, IScenarioEditorMouseClick, IScenarioEditorMouseMove,
    IScenarioEditorMouseHover, IScenarioEditorMouseDrag, IScenarioEditorSelectable, IScenarioEditorFocusTarget
{
    public RuntimeCableEditor RuntimeCableEditor;
    public RuntimeMarkerEditor RuntimeMarkerEditor;

    [System.NonSerialized]
    public ComponentInfo_Cable CableInfo;

    private Placer _placer;
    private List<NodeGizmo> NodeGizmos = new List<NodeGizmo>();
    private NodeGizmo _selectedNode;
    private ContextMenuController _contextMenu = null;
    private Color _currentColor;

    private GameObject _addNodeGizmo;

    private int _boltLayerMask;
    private static Collider[] _colliderHits = new Collider[50];    

    public bool IsSelectionLocked
    {
        get
        {
            if (RuntimeCableEditor == null)
                return false;

            if (RuntimeCableEditor.NodeAddMode == NodeAddMode.AddModeOff)
                return false;

            return true;
        }
    }


    public NodeGizmo SelectedNode
    {
        get
        {
            return _selectedNode;
        }
    }

    public int SelectedNodeIndex
    {
        get
        {
            if (_selectedNode == null || _selectedNode.CableNode == null)
                return -1;

            return _selectedNode.CableNode.NodeIndex;
        }
    }

    void Awake()
    {
        if (RuntimeCableEditor == null)
            RuntimeCableEditor = RuntimeCableEditor.GetDefault(gameObject);

        if (RuntimeMarkerEditor == null && RuntimeCableEditor != null)
            RuntimeMarkerEditor = RuntimeCableEditor.GetComponent<RuntimeMarkerEditor>();

        if (RuntimeCableEditor != null)
            _contextMenu = RuntimeCableEditor.GetComponent<ContextMenuController>();

        CableInfo = GetComponent<ComponentInfo_Cable>();
        CableInfo.NodeAdded += OnCableNodeAdded;
        CableInfo.NodeRemoved += OnCableNodeRemoved;

        _boltLayerMask = LayerMask.GetMask("RoofBolts");
    }

    new void Start()
    {
        base.Start();


    }


    new void OnDestroy()
    {
        if (CableInfo != null)
        {
            CableInfo.NodeAdded -= OnCableNodeAdded;
            CableInfo.NodeRemoved -= OnCableNodeRemoved;
        }

        base.OnDestroy();
    }

    public void ShowNodeGizmos(bool show)
    {
        if (show)
            InstantiateNodeGizmos();
        else
            DestroyNodeGizmos();
    }

    private void ShowAddNodeGizmo(Vector3 pos)
    {
        if (_addNodeGizmo == null)
        {
            _addNodeGizmo = CreateAddNodeGizmo();
            if (_addNodeGizmo == null)
                return;
        }

        _addNodeGizmo.SetActive(true);
        _addNodeGizmo.transform.position = pos;
    }

    private void HideAddNodeGizmo()
    {
        if (_addNodeGizmo != null && _addNodeGizmo.activeSelf)
            _addNodeGizmo.SetActive(false);
    }

    private GameObject CreateAddNodeGizmo()
    {
        if (RuntimeCableEditor == null || RuntimeCableEditor.NodePreviewPrefab == null)
            return null;

        return Instantiate<GameObject>(RuntimeCableEditor.NodePreviewPrefab);
    }

    private void SelectNode(int nodeIndex)
    {
        if (NodeGizmos == null)
            return;

        foreach (var node in NodeGizmos)
        {
            if (node.CableNode != null && node.CableNode.NodeIndex == nodeIndex)
            {
                SelectNode(node);
                break;
            }    
        }
    }


    private void SelectNode(NodeGizmo nodeGizmo)
    {
        Debug.Log($"PlaceableCable: Selecting node {nodeGizmo.name}");

        if (_selectedNode != null)
        {
            _selectedNode.SetColor(_currentColor);
        }

        _selectedNode = nodeGizmo;
        _selectedNode.SetColor(RuntimeCableEditor.SelectedColor);

        if (_contextMenu != null)
        {
            _contextMenu.SetTarget(_selectedNode.transform);
        }
    }

    private void OnCableNodeAdded(HangingGeometry.CableNode obj)
    {
        InstantiateNodeGizmos();
        SelectNode(obj.NodeIndex);
    }

    private void OnCableNodeRemoved(HangingGeometry.CableNode obj)
    {
        InstantiateNodeGizmos();
    }

    private void UpdateGizmoColors(Color color)
    {
        _currentColor = color;

        if (NodeGizmos == null || NodeGizmos.Count <= 0)
            return;

        foreach (var gizmo in NodeGizmos)
        {
            gizmo.SetColor(color);
        }
    }

    /// <summary>
    /// Create a node Gizmo for each pre-existing node in a data.cable on startup
    /// </summary>
    private void InstantiateNodeGizmos()
    {
        if (RuntimeCableEditor == null || RuntimeCableEditor.NodeGizmoPrefab == null)
            return;

        DestroyNodeGizmos();

        foreach (HangingGeometry.CableNode node in CableInfo.Component.CableNodes)
        {

            var newNode = Instantiate(RuntimeCableEditor.NodeGizmoPrefab, transform);
            newNode.transform.position = node.Position;

            var gizmo = newNode.GetComponent<NodeGizmo>();
            gizmo.CableNode = node;
            NodeGizmos.Add(gizmo);

            //newNode.GetComponent<NodeGizmo>().index = node.Get;
        }

        UpdateGizmoColors(RuntimeCableEditor.InactiveColor);
        //LastHeadNode = NodeGizmos[NodeGizmos.Count - 1];
        //Debug.Log("instantiateCable");

        //if (CableCanGenerate()) Component.RegenerateMesh();
    }

    private RoofBolt GetClosestRoofbolt(Ray ray)
    {
        var count = Physics.OverlapCapsuleNonAlloc(ray.origin, ray.origin + ray.direction * 150.0f, 3.0f, _colliderHits, _boltLayerMask);
        if (count <= 0)
            return null;

        RoofBolt closest = null;
        float minAngle = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var hit = _colliderHits[i];

            if (!hit.transform.TryGetComponent<RoofBolt>(out var roofbolt))
                continue;

            var v = hit.transform.position - ray.origin;
            var angle = Vector3.Angle(ray.direction, v);
            if (angle < minAngle)
            {
                closest = roofbolt;
                minAngle = angle;
            }
        }

        return closest;
    }

    private void DestroyNodeGizmos()
    {
        if (gameObject == null || NodeGizmos == null || NodeGizmos.Count <= 0)
            return;

        foreach (var node in NodeGizmos)
        {
            Destroy(node.gameObject);
        }
        NodeGizmos.Clear();
    }

    public void OnScenarioEditorMouseDown(Placer placer, int button, RaycastHit cursorHit, ScenarioCursorData cursorData)
    {
        if (button != 0)
            return;

        if (RuntimeCableEditor != null && RuntimeCableEditor.NodeAddMode != NodeAddMode.AddModeOff && _addNodeGizmo != null && CableInfo != null)
        {
            bool addHanger = RuntimeCableEditor.NodeAddMode == NodeAddMode.AddHangingNode;
            int index = 0;
            if (_selectedNode != null)
            {
                index = _selectedNode.CableNode.NodeIndex + 1;
            }
            CableInfo.AddNode(_addNodeGizmo.transform.position, addHanger, index);
            return;
        }

        if (cursorHit.collider == null)
            return;

        if (!cursorHit.collider.TryGetComponent<NodeGizmo>(out var nodeGizmo))
            return;

        SelectNode(nodeGizmo);
    }

    public void OnScenarioEditorMouseUp(Placer placer, int button, RaycastHit cursorHit, ScenarioCursorData cursorData)
    {
        if (cursorHit.collider == null)
            return;

        if (!cursorHit.collider.TryGetComponent<NodeGizmo>(out var nodeGizmo))
            return;

        //SelectNode(nodeGizmo);
    }

    public void OnScenarioEditorMouseFocusLost(Placer placer)
    {
        _placer = placer;

        HideAddNodeGizmo();

        if (RuntimeCableEditor != null)
            RuntimeCableEditor.ClearBoltPreviews();
    }

    private Vector3 GetTargetPosition(bool snapToBolts, ScenarioCursorData cursorData)
    {
        var cableRadius = 0.05f;
        if (CableInfo != null)
            cableRadius = CableInfo.CableDiameter * 0.5f;

        if (!snapToBolts)
            return cursorData.SurfacePos + cursorData.SurfaceNormal * cableRadius;

        //var bolt = GetClosestRoofbolt(cursorData.SurfacePos + cursorData.SurfaceNormal * (CableInfo.CableDiameter * 0.5f));               
        var bolt = GetClosestRoofbolt(cursorData.SceneRay);
        Vector3 pos;

        if (bolt != null)
            pos = bolt.GetHookPositionWorldSpace(0);
        else
            pos = cursorData.SurfacePos + cursorData.SurfaceNormal * cableRadius;

        return pos;
    }

    public void OnScenarioEditorMouseMove(Placer placer, RaycastHit cursorHit, ScenarioCursorData cursorData)
    {
        _placer = placer;

        if (CableInfo == null || RuntimeCableEditor == null)
            return;

        switch (RuntimeCableEditor.NodeAddMode)
        {
            case NodeAddMode.AddFixedNode:
            case NodeAddMode.AddFloorNode:
                ShowAddNodeGizmo(GetTargetPosition(false, cursorData));
                if (RuntimeCableEditor.AreRoofboltPreviewsActive)
                    RuntimeCableEditor.ClearBoltPreviews();

                break;

            case NodeAddMode.AddHangingNode:

                var pos = GetTargetPosition(true, cursorData);
                ShowAddNodeGizmo(pos);                
                RuntimeCableEditor.ShowBoltPreviews(pos);

                break;

            default:
                if (RuntimeCableEditor.AreRoofboltPreviewsActive)
                    RuntimeCableEditor.ClearBoltPreviews();

                HideAddNodeGizmo();
                break;
        }
    }

    public void ScenarioEdtiorMouseHoverBegin(Placer placer, RaycastHit cursorHit, ScenarioCursorData cursorData)
    {
        if (!cursorHit.collider.TryGetComponent<NodeGizmo>(out var nodeGizmo))
            return;

        if (nodeGizmo == SelectedNode)
            return;

        nodeGizmo.SetColor(RuntimeCableEditor.HoveredColor);
    }

    public void ScenarioEdtiorMouseHoverEnd(Placer placer, RaycastHit cursorHit, ScenarioCursorData cursorData)
    {
        if (cursorHit.collider == null)
            return;

        if (!cursorHit.collider.TryGetComponent<NodeGizmo>(out var nodeGizmo))
            return;

        if (nodeGizmo == SelectedNode)
            return;

        nodeGizmo.SetColor(_currentColor);
    }

    public void StartMouseDrag(Placer placer)
    {
        _placer = placer;
    }

    public void ProcessMouseDrag(ScenarioCursorData prev, ScenarioCursorData current)
    {
        if (SelectedNode == null)
            return;

        if (RuntimeMarkerEditor != null && RuntimeMarkerEditor.target != null)
        {
            //marker is selected
            RuntimeMarkerEditor.ProcessMouseDrag(prev, current);
            return;
        }

        bool snapToBolts = false;
        if (SelectedNode.CableNode != null && SelectedNode.CableNode.ShowCableHanger)
            snapToBolts = true;

        //var pos = current.SurfacePos;
        var pos = GetTargetPosition(snapToBolts, current);

        if (snapToBolts)
            RuntimeCableEditor.ShowBoltPreviews(pos);

        SelectedNode.transform.position = pos;
        SelectedNode.CableNode.Position = pos;

        CableInfo.RegenerateMesh(false);
    }

    public void CompleteMouseDrag()
    {
        if (CableInfo == null)
            return;

        RuntimeCableEditor.ClearBoltPreviews();
        CableInfo.RegenerateMesh(true);
    }

    public void ScenarioEditorSelectedObject(Placer placer, bool selected)
    {
        _placer = placer;

        //ShowNodeGizmos(selected);
        if (selected)
        {
            RuntimeCableEditor.SetSelectedCable(this);
            UpdateGizmoColors(RuntimeCableEditor.ActiveColor);

            if (NodeGizmos == null || NodeGizmos.Count <= 0)
                InstantiateNodeGizmos();

            if (placer.CurrentCursorHit.collider != null &&
                placer.CurrentCursorHit.collider.TryGetComponent<NodeGizmo>(out var cursorNode))
                SelectNode(cursorNode);
            else
                SelectNode(NodeGizmos[0]);
        }
        else
        {
            UpdateGizmoColors(RuntimeCableEditor.InactiveColor);
        }

    }

    protected override void OnLayerChanged(LayerManager.EditorLayer newLayer)
    {
        if (newLayer == LayerManager.EditorLayer.Cables)
        {
            ShowNodeGizmos(true);
        }
        else
        {
            ShowNodeGizmos(false);
        }

    }

    public Vector3 GetFocusTarget()
    {
        if (RuntimeMarkerEditor != null && RuntimeMarkerEditor.target != null)
            return RuntimeMarkerEditor.target.transform.position;
        else if (SelectedNode != null)
            return SelectedNode.transform.position;

        return transform.position;
    }
}
