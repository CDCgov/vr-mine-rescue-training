using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using NIOSH_EditorLayers;
using UnityEditor;
using System;
using UnityEngine.Serialization;

public class ObjectInfo : MonoBehaviour, ISaveableComponent
{
    public enum ComponentType
    {
        Rigidbody,
        AudioSource,
        Light,
        FireExtinguisher
    }

    public PlacementTypeOverride PlacementTypeOverride = PlacementTypeOverride.None;
    [System.NonSerialized]
    public string AssetID; //asset ID should be populated from the LoadableAsset when created/loaded
    [System.NonSerialized]
    public string BasePrefabID;

    public Guid InstanceID;

    [FormerlySerializedAs("displayName")]
    public string DisplayName;
    [FormerlySerializedAs("hierarchyItemPrefab")]
    public GameObject HierarchyItemPrefab; // instantiate when placed in hierarchy 
    
    //public GameObject hierarchyObjectInScene; //reference to UI instance so that it can be moved/destroyed when placing object in viewport
    //[System.NonSerialized]
    //public HierarchyItem HierarchyItem; //reference to UI instance so that it can be moved/destroyed when placing object in viewport

    //public HierarchyContainer hierarchyContainer; //reference to UI instance so that it can be moved/destroyed when placing object in viewport
    public LayerManager.EditorLayer editorLayer = LayerManager.EditorLayer.Object;

    [HideInInspector] public SimulatedParent simParent;
    [HideInInspector] public string UserSuppliedName;
    [HideInInspector] public string AssetWindowName;

    public ComponentInfo_Name componentInfo_Name;
    public ComponentInfo_Transform componentInfo_Transform;

    public bool IsResizable = false;
    public Material ScaledMaterial;
    public bool SetLayerOnSelection = false;

    public Transform PlacementAnchor;

    public Action ObjectInfoChanged;

    //public List<ComponentInfo_Rigidbody> componentInfo_Rigidbodies = new List<ComponentInfo_Rigidbody>();
    //public List<ComponentInfo_Light> componentInfo_Lights = new List<ComponentInfo_Light>();
    //public List<ComponentInfo_AudioSource> componentInfo_AudioSources = new List<ComponentInfo_AudioSource>();
    //public List<ComponentInfo_FireExtinguisher> componentInfo_FireExtinguishers = new List<ComponentInfo_FireExtinguisher>();
    //public List<ComponentInfo_Cable> componentInfo_Cables = new List<ComponentInfo_Cable>();
    //public List<ComponentInfo_Lifeline> componentInfo_Lifelines = new List<ComponentInfo_Lifeline>();
    //public ComponentInfo_StaticGasZone componentInfo_StaticGasZone;
    ////public List<ComponentInfo_SubGasZone> componentInfo_SubGasZones = new List<ComponentInfo_SubGasZone>();
    //public List<ComponentInfo_JunctionProperties> componentInfo_JunctionProperties = new List<ComponentInfo_JunctionProperties>();
    //public List<ComponentInfo_Airway> componentInfo_Airway = new List<ComponentInfo_Airway>();
    //public List<ComponentInfo_NPC> componentInfo_NPCs = new List<ComponentInfo_NPC>();
    //public List<ComponentInfo_EntryLabel> componentInfo_EntryLabels = new List<ComponentInfo_EntryLabel>();
    //public List<ComponentInfo_MineSegment> componentInfo_MineSegments = new List<ComponentInfo_MineSegment>();
    //public List<ComponentInfo_Interactable> componentInfo_Interactables = new List<ComponentInfo_Interactable>();
    //public List<ComponentInfo_BoltGraph> componentInfo_BoltGraphs = new List<ComponentInfo_BoltGraph>();

    //private LoadableAssetCollection _loadableAssetCollection;

    //private List<ModularComponentInfo> _modularComponents = new List<ModularComponentInfo>();

    public T GetFirstModularComponent<T>() where T : ModularComponentInfo
    {
        return null;
    }

    public IEnumerable<ModularComponentInfo> GetModularComponentInfo()
    {
        var components = GetComponentsInChildren<ModularComponentInfo>();

        foreach (var comp in components)
        {
            yield return comp;
        }
    }

    public IEnumerable<T> GetModularComponentInfo<T>() where T : ModularComponentInfo
    {
        var components = GetComponentsInChildren<T>();

        foreach (var comp in components)
        {
            yield return comp;
        }        
    }


    public bool IsTranslatable
    {
        get
        {
            if (componentInfo_Transform == null)
                return false;

            return componentInfo_Transform.positionExposureLevel == Inspector.ExposureLevel.Editable;
        }
    }

    public bool IsRotatable
    {
        get
        {
            if (componentInfo_Transform == null)
                return false;

            return componentInfo_Transform.rotationExposureLevel == Inspector.ExposureLevel.Editable;
        }
    }

    public bool IsScalable
    {
        get
        {
            if (componentInfo_Transform == null)
                return false;

            return componentInfo_Transform.scaleExposureLevel == Inspector.ExposureLevel.Editable;
        }
    }

    public string InstanceName
    {
        get
        {
            if (!string.IsNullOrEmpty(UserSuppliedName))
            {
                return UserSuppliedName;
            }
            else if (!string.IsNullOrEmpty(AssetWindowName))
            {
                return AssetWindowName;
            }
            else
            {
                return "Unknown";
            }
        }
        set
        {
            SetUserSuppliedName(value);
        }
    }

   
    //public bool isNetworked;
    

    //private Guid guid;
    private Placer _placer;

    
    //public Guid GUID
    //{
    //    get { return guid; }
    //    set { guid = value; guidString = guid.ToString(); }
    //}
    //public string guidString;
    private void Awake()
    {
        //componentInfo_Lifelines = new List<ComponentInfo_Lifeline>();
        //componentInfo_Cables = new List<ComponentInfo_Cable>();
        //componentInfo_FireExtinguishers = new List<ComponentInfo_FireExtinguisher>();
        //componentInfo_AudioSources = new List<ComponentInfo_AudioSource>();
        //componentInfo_Lights = new List<ComponentInfo_Light>();
        //componentInfo_Rigidbodies = new List<ComponentInfo_Rigidbody>();
        ////componentInfo_StaticGasZones = new List<ComponentInfo_StaticGasZone>();
        ////componentInfo_SubGasZones = new List<ComponentInfo_SubGasZone>();
        //componentInfo_NPCs = new List<ComponentInfo_NPC>();
        //componentInfo_MineSegments = new List<ComponentInfo_MineSegment>();

        if (componentInfo_Name == null)
        {
            componentInfo_Name = new ComponentInfo_Name();
            componentInfo_Name.objectInfo = this;
            componentInfo_Name.instanceDisplayName = AssetWindowName;
            componentInfo_Name.instanceUserSuppliedName = AssetWindowName;
        }

        if (componentInfo_Transform == null)
        {
            componentInfo_Transform = new ComponentInfo_Transform();
            componentInfo_Transform.transformComponent = transform;
        }
        componentInfo_Name.instanceDisplayName = AssetWindowName;
        componentInfo_Name.instanceUserSuppliedName = UserSuppliedName;

        //AssetLoader assetLoader = FindObjectOfType<AssetLoader>();
        //if (assetLoader != null)
        //{
        //    _loadableAssetCollection = assetLoader.Loadables;
        //    if(_loadableAssetCollection == null)
        //    {
        //        return;
        //    }
        //    if (_loadableAssetCollection.GetLoadableAsset(DisplayName) == null)
        //    {
        //        return;
        //    }
        //    AssetWindowName = _loadableAssetCollection.GetLoadableAsset(DisplayName).GetAssetWindowName();
        //    if (string.IsNullOrEmpty(UserSuppliedName))
        //    {
        //        UserSuppliedName = AssetWindowName;
        //    }
        //}
    }
    private void Start()
    {
        if (AssetLoader.Instance != null && AssetLoader.Instance.IsEditorMode)
        {
            _placer = Placer.GetDefault();
            if (editorLayer == LayerManager.EditorLayer.Mine)
            {
                
                GameObject _simParent = new GameObject();
                _simParent.name = DisplayName + "_SimParent: " + DisplayName;

                simParent = _simParent.AddComponent(typeof(SimulatedParent)) as SimulatedParent;
                if (_placer != null) simParent.transform.parent = _placer.assetContainer;
                simParent.transform.position = transform.position;
                simParent.objectInfo = this;

            }
        }
        

    }
    
    private void OnDestroy()
    {
        //if (HierarchyItem != null) HierarchyItem.StartDestroy();
        if (simParent != null) Destroy(simParent.gameObject);
        //Debug.Log("Prefab being Destroyed");
    }

    public void SetDisplayName(string value)
    {
        //yield return new WaitForSeconds(1);
        componentInfo_Name.instanceDisplayName = value;
        DisplayName = value;

        //set name in hierarchy
        if (string.IsNullOrEmpty(UserSuppliedName))
        {
            //if (HierarchyItem != null) 
            //{ 
            //    HierarchyItem.SetDisplayName(gameObject); 
            //}
        }
    }

    public string SaveName()
    {
        return componentInfo_Name.componentName;
    }
    public string[] SaveInfo()
    {

        return componentInfo_Name.SaveInfo();
    }

    public void LoadInfo(SavedComponent component)
    {
        componentInfo_Name.objectInfo = this;
        componentInfo_Name.LoadInfo(component);

    }

    private void SetUserSuppliedName(string name)
    {
        //yield return new WaitForSeconds(0.5f);
        componentInfo_Name.instanceUserSuppliedName = name;        
        UserSuppliedName = name;
        //if(HierarchyItem != null)
        //{
        //    HierarchyItem.SetDisplayName(gameObject);
        //}

        RaiseObjectInfoChanged();
        //yield return null;
    }

    private void RaiseObjectInfoChanged()
    {
        ObjectInfoChanged?.Invoke();
    }
}

public enum PlacementTypeOverride
{
    Gizmo = 0,
    DragDrop = 1,
    DragSnap = 2,

    Ventilation = 3,
    Cable = 4,
    SnapCurtain = 5,
    None = 100,
}
