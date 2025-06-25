using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NIOSH_EditorLayers;

public class ComponentInfo_Cable : ModularComponentInfo, ISaveableComponent
{
    public LoadableAssetManager LoadableAssetManager;

    public bool IsLifeline = false;
    public string ComponentName = "Cable";
    
    //public List<HangingGeometry.CableNode> CableNodes;
    public CableType CableType;
    
    public HangingGeometry Component;
    //public GameObject LastHeadNode;
    
    //public GameObject NodeGizmoPrefab;
    public bool GizmosHidden;
    public int CableHangerIndex;
    public string CableHangerID;

    public event System.Action<HangingGeometry.CableNode> NodeAdded;
    public event System.Action<HangingGeometry.CableNode> NodeRemoved;

    public List<HangingGeometry.CableNode> CableNodes
    {
        get
        {
            if (Component == null)
                return null;

            return Component.CableNodes;
        }
    }

    public float CableSlack
    {
        get
        {
            if (Component == null)
                return 0;

            return Component.DefaultCableSlope;

        }
        set
        {
            if (Component == null)
                return;

            Component.DefaultCableSlope = value;
        }
    }

    public float CableDiameter // water .05, // power .05// lifeline 0.01
    {
        get
        {
            if (Component == null)
                return 0.05f;

            return Component.CableDiameter;
        }
    }

    

    //public List<GameObject> NodeGizmos = new List<GameObject>();
    //private ObjectInfo _objectInfo;
    //private RuntimeCableEditor _cableEditor;
    //private ContextMenuController _contextMenu;
    //private AssetLoader _assetLoader;
    private int _cableNodeCount;
    private GlobalCableData _globalCableData;


    private void Awake()   // FIXME null ref in here in a build
    {
        _globalCableData = FindObjectOfType<GlobalCableData>();

        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        //_assetLoader = FindObjectOfType<AssetLoader>();

        //if(_assetLoader && _assetLoader.IsEditorMode)InstantiateNodeGizmos();

        //if (CableCanGenerate())
        //{
        //    SetHangerType(CableHangerIndex);
        //    //Component.RegenerateMesh();
        //}
        //else
        //{
        //    SetHangerTypeNoRegen(CableHangerIndex);
        //}

        SetHangerType(CableHangerID, CableCanGenerate());

        //assign cable editor
        //_cableEditor = FindObjectOfType<RuntimeCableEditor>();
        //if(_cableEditor)_cableEditor.data = this;
        // assign context Menu and select node
        //_contextMenu = FindObjectOfType<ContextMenuController>();

        // add this to object info
        //_objectInfo = GetComponent<ObjectInfo>();
        //if (_objectInfo == null) _objectInfo = GetComponent<ObjectInfo>();
        //if (!_objectInfo.componentInfo_Cables.Contains(this)) _objectInfo.componentInfo_Cables.Add(this);
        PlacablePrefab placable = GetComponentInParent<PlacablePrefab>();
        if (placable != null)
        {
            Placer placer = FindObjectOfType<Placer>();
            if(placer != null) {  placer.activeLogic.selectedObject = placable; }
        }
    }

    private void Start()
    {
        if (LayerManager.Instance != null) 
            LayerManager.Instance.layerChanged += OnNewLayer;

        //assign cable slack
        if (Component) 
            Component.DefaultCableSlope = CableSlack;

        //if (_contextMenu) 
        //    _contextMenu.SetTarget(NodeGizmos[0].transform);
    }

    private void OnDestroy()
    {
        if (LayerManager.Instance != null) LayerManager.Instance.layerChanged -= OnNewLayer;
    }

    public void RegenerateMesh(bool generateHangers = true)
    {
        if (Component != null && CableCanGenerate())
            Component.RegenerateMesh(generateHangers);
    }

    public string SaveName()
    {
        return ComponentName;
    }

    public string[] SaveInfo()
    {
        List<string> data = NodesDataList();
        
        data.Add("Material|" + CableType);
        data.Add("Thickness|" + CableDiameter);
        data.Add("CableNodeCount|" + Component.CableNodes.Count);
        data.Add("Slack|" + CableSlack);
        data.Add("HangerIndex|" + CableHangerIndex);
        data.Add("CableHangerID|" + CableHangerID);
        return data.ToArray();
    }

    List<string> NodesDataList()
    {
        List<string> nodesDataList = new List<string>();

        //save to info
        for (int i = 0; i < Component.CableNodes.Count; i++)
        {
            HangingGeometry.CableNode node = Component.CableNodes[i];

            string _x = $"Position_X{i}|" + node.Position.x;
            string _y = $"Position_Y{i}|" + node.Position.y;
            string _z = $"Position_Z{i}|" + node.Position.z;
            string Slope = $"Slope_{i}|" + node.Slope;
            string ShowCableHanger = $"ShowCableHanger_{i}|" + node.ShowCableHanger;
            string FixedPosition = $"FixedPosition_{i}|" + node.FixedPosition;
            string data = _x + _y + _z + Slope + ShowCableHanger + FixedPosition;
            //nodesDataList.Insert(i, data);
            nodesDataList.Add(_x);
            nodesDataList.Add(_y);
            nodesDataList.Add(_z);
            nodesDataList.Add(Slope);
            nodesDataList.Add(ShowCableHanger);
            nodesDataList.Add(FixedPosition);
        }

        return nodesDataList;
    }


    public void LoadInfo(SavedComponent component)    // FIXME null ref in here
    {
        if (component == null) 
        { 
            Debug.Log("Failed to load cable component info. Saved component is null for " + gameObject.name); 
            return; 
        } 
        
        // get component
        ComponentName = component.GetComponentName();
        int.TryParse(component.GetParamValueAsStringByName("CableNodeCount"), out _cableNodeCount);

        //load materials
        //int mat = 0;
        //int.TryParse(component.GetParamValueAsStringByName("Material"), out mat);
        //CableType = (CableType)mat;
        var cableTypeString = component.GetParamValueAsStringByName("Material");
        if (System.Enum.TryParse<CableType>(cableTypeString, out var cableType))
        {
            CableType = cableType;
        }
        
        //load thickness
        float.TryParse(component.GetParamValueAsStringByName("Thickness"), out Component.CableDiameter);

        //load slack
        float.TryParse(component.GetParamValueAsStringByName("Slack"), out Component.DefaultCableSlope);

        //load slack
        int.TryParse(component.GetParamValueAsStringByName("HangerIndex"), out CableHangerIndex);

        CableHangerID = component.GetParamValueAsStringByName("CableHangerID");

        //load cable nodes
        LoadCableNodes(component);

        //load to component
        if (Component)
        {
            if (CableDiameter <= 0 && _globalCableData != null)
                Component.CableDiameter = _globalCableData.GetDiameter(CableType);

            //Component.DefaultCableSlope = CableSlack;
            //Component.CableNodes.Clear();
            //Component.CableNodes = CableNodes;
            //Component.CableDiameter = CableDiameter;
            Component.CableMaterial = _globalCableData.GetMaterial(CableType);
            Component.UpdateMaterial();
            SetHangerType(CableHangerID);
        }

        //To Do check to see if we are in editor
        //if (_assetLoader && _assetLoader.IsEditorMode) InstantiateNodeGizmos();
        Component.RegenerateMesh();
    }
    
    void LoadCableNodes(SavedComponent component)
    {
        //clear
        //CableNodes.Clear();
        //CableNodes = new List<HangingGeometry.CableNode>();
        Component.CableNodes.Clear();
        Component.CableNodes = new List<HangingGeometry.CableNode>();
        
        //load to info
        for (int i = 0; i < _cableNodeCount; i++)
        {
            HangingGeometry.CableNode node = new HangingGeometry.CableNode();

            //node.Position = new Vector3(StringToVector3(component.GetParamValueAsStringByName($"Position_X{i}")), StringToVector3(component.GetParamValueAsStringByName($"Position_X{i}")),StringToVector3(component.GetParamValueAsStringByName($"Position_X{i}")));

            float _x = 0;
            float _y = 0;
            float _z = 0;

            float.TryParse(component.GetParamValueAsStringByName($"Position_X{i}"), out _x);
            float.TryParse(component.GetParamValueAsStringByName($"Position_Y{i}"), out _y);
            float.TryParse(component.GetParamValueAsStringByName($"Position_Z{i}"), out _z);

            node.Position = new Vector3(_x, _y, _z);

            float.TryParse(component.GetParamValueAsStringByName($"Slope_{i}"), out node.Slope);
            bool.TryParse(component.GetParamValueAsStringByName($"ShowCableHanger_{i}"), out node.ShowCableHanger);
            bool.TryParse(component.GetParamValueAsStringByName($"FixedPosition_{i}"), out node.FixedPosition);

            Component.CableNodes.Insert(i,node);
        }

        Component.ReindexNodes();
    }

    public void AddNode(Vector3 nodePos, bool showHanger, int index)
    {
        //SettleFirstNode();
        //CursorImageController.instance.cursorState = CursorImageController.CursorImage.HandPointingPressed;
        var newNode = Component.InsertNode(nodePos, showHanger, index);
        Component.RegenerateMesh();

        NodeAdded?.Invoke(newNode);
        ///// create node prefab clone track it, and select it
        //var newNode = Instantiate(data.NodeGizmoPrefab, data.transform);
        //newNode.transform.position = nodePos;
        //data.NodeGizmos.Insert(targetIndex + 1, newNode);
        //ResizeLastNode();


        //targetIndex = targetIndex + 1;
        //CurrentNode().Slope = data.CableSlack;

        //contextMenu.SetTarget(data.NodeGizmos[targetIndex].transform);
    }

    public void RemoveNode(int index)
    {
        if (CableNodes == null || index < 0 || index >= CableNodes.Count)
            return;

        //don't remove last node
        if (CableNodes.Count <= 1)
            return;


        var nodeToRemove = CableNodes[index];        
        Component.RemoveNode(index);

        NodeRemoved(nodeToRemove);
    }


    public bool CableCanGenerate()
    {
        return Component.CableNodes.Count > 1;
    }

    void OnNewLayer(LayerManager.EditorLayer _editorLayer)
    {

        //ControlNodeActiveState(_editorLayer == LayerManager.EditorLayer.Cables);
    }

    //public void ControlNodeActiveState(bool state)
    //{
    //    foreach (GameObject g in NodeGizmos)
    //    {
    //        g.SetActive(state);
    //    }
    //}

    public Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    public void SetHangerType(string cableHangerID, bool regenerateMesh = true)
    {
        var cableHanger = LoadableAssetManager.FindCableHanger(cableHangerID);

        Component.CableHangerPrefab = cableHanger.Prefab;
        Component.CableHangerWidth = cableHanger.CableHangerWidth;
        CableHangerID = cableHangerID;

        if (regenerateMesh)
            Component.RegenerateMesh();
    }

    //public void SetHangerType(int index)
    //{
    //    CableHangerIndex = index;
    //    if (Component == null)
    //        return;

    //    //assign to component
    //    if (_globalCableData == null) _globalCableData = FindObjectOfType<GlobalCableData>();
    //    switch (index)
    //    {
    //        case 0:
    //            Component.CableHangerPrefab = _globalCableData.cableHangerPrefab;
    //            break;
    //        case 1:
    //            Component.CableHangerPrefab = _globalCableData.bratticeHookPrefab;
    //            break;
    //        case 2:
    //            Component.CableHangerPrefab = _globalCableData.SingleHookPrefab;
    //            break;
    //        case 3:
    //            Component.CableHangerPrefab = _globalCableData.DoubleHookPrefab;
    //            break;
    //        case 4:
    //            Component.CableHangerPrefab = _globalCableData.QuickReleasePrefab;
    //            break;
    //    }
    //    Component.RegenerateMesh();
    //}

    //public void SetHangerTypeNoRegen(int index)
    //{
    //    if (_globalCableData == null) _globalCableData = FindObjectOfType<GlobalCableData>();
    //    switch (index)
    //    {
    //        case 0:
    //            Component.CableHangerPrefab = _globalCableData.cableHangerPrefab;
    //            break;
    //        case 1:
    //            Component.CableHangerPrefab = _globalCableData.bratticeHookPrefab;
    //            break;
    //        case 2:
    //            Component.CableHangerPrefab = _globalCableData.SingleHookPrefab;
    //            break;
    //        case 3:
    //            Component.CableHangerPrefab = _globalCableData.DoubleHookPrefab;
    //            break;
    //        case 4:
    //            Component.CableHangerPrefab = _globalCableData.QuickReleasePrefab;
    //            break;
    //    }
    //}

    public void SetCableType(int index)
    {
        CableType = (CableType)index;

        //set material and thickness according to type
        if (Component)
        {
            Component.CableDiameter = _globalCableData.GetDiameter(CableType);
            Component.CableMaterial = _globalCableData.GetMaterial(CableType);
            Component.UpdateMaterial();
            Component.RegenerateMesh();
        }


        if (CableType == CableType.Lifeline)
        {
            IsLifeline = true;
        }
        else
        {
            IsLifeline = false;
        }
        
    }

}

