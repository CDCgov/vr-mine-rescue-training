using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NIOSH_EditorLayers;
using System;


public class Inspector : MonoBehaviour
{
    //public static Inspector instance { get; private set; }


    //private Inspector _instance => instance;
    public enum ExposureLevel
    {
        Editable,
        Visible,
        Hidden,
        Fixed,
    }

    Placer scenePlacer;

    public RectTransform ContentParent;

    private GameObject _target;
    private ObjectInfo _targetInfo;
    private Transform _targetTransform;
    //[SerializeField] RectTransform _contentRt;
    //public VerticalLayoutGroup _layout;
    //[SerializeField] RectTransform _subContentRt;

    public ComponentInspector_Transform TransformInspectorInstance;

    public GameObject ComponentInspector_Name;
    public GameObject ComponentInspector_Transform;
    //public List<ComponentInspector> modularComponentInspectors = new List<ComponentInspector>();

    public GameObject GenericComponentInspectorPrefab;

    public GameObject componentInspectorPrefab_Light;
    public GameObject componentInspectorPrefab_AudioSource;
    public GameObject componentInspectorPrefab_Rigidbody;
    public GameObject componentInspectorPrefab_FireExtinguisher;
    public GameObject componentInspectorPrefab_Cable;
    public GameObject componentInspectorPrefab_Curtain;
    public GameObject componentInspectorPrefab_Lifeline;
    public GameObject componentInspectorPrefab_StaticGasZone;
    public GameObject componentInspectorPrefab_SubGasZone;
    public GameObject componentInspectorPrefab_JunctionProperties;
    public GameObject ComponentInspectorPrefab_Airway;
    public GameObject componentInspectorPrefab_NPC;
    public GameObject componentInspectorPrefab_EntryLabel;
    public GameObject componentInspectorPrefab_MineSegment;
    public GameObject componentInspectorPrefab_Interactable;
    public GameObject componentInspectorPrefab_CollisionAudio;
    public GameObject componentInspectorPrefab_AmbientAudio;
    public GameObject componentInspectorPrefab_VentFire;
    public GameObject componentInspectorPrefab_DynamicVentControl;
    public GameObject componentInspectorPrefab_Color;
    public GameObject componentInspectorPrefab_Decal;
    public GameObject componentInspectorPrefab_ShowOnMapMan;

    public delegate void OnNewTarget();
    public OnNewTarget onNewTarget;

    public delegate void OnClearTarget();
    public OnClearTarget onClearTarget;

    //public Transform modularItemContainer;

    private Vector3 _lastPos;
    private Quaternion _lastRot;
    private Vector3 _lastScale;

    private List<Selectable> _selectables;

    private List<GameObject> _modularInspectors = new List<GameObject>(20);
    private List<IComponentInspector> _compInspectors = new List<IComponentInspector>();

    private void Awake()
    {
        
        // If there is an instance, and it's not me, delete myself.
        //if (instance != null && instance != this)
        //{
        //    Destroy(this);
        //}
        //else
        //{
        //    instance = this;
        //}

        scenePlacer = FindObjectOfType<Placer>();
        _selectables = new List<Selectable>();

    }

    private void Start()
    {
        //scenePlacer.onObjectSelected += SetTarget;
        //scenePlacer.onObjectDeselected += DeselectObject;

        scenePlacer.SelectedObjectChanged += OnSelectedObjectChanged;

        //_transformInspector = GetComponentInChildren<ComponentInspector_Transform>();
    }

    private void OnDestroy()
    {
        //scenePlacer.onObjectSelected -= SetTarget;
        //scenePlacer.onObjectDeselected -= DeselectObject;
        scenePlacer.SelectedObjectChanged -= OnSelectedObjectChanged;
    }

    private void OnSelectedObjectChanged(GameObject obj)
    {
        if (obj == null)
            DeselectObject();
        else
            SetTarget(obj);
    }


    private void Update()
    {
        if (_targetTransform == null || TransformInspectorInstance == null)
            return;

        //if (target_XFM && target_XFM.hasChanged)
        if (_targetTransform.position != _lastPos ||
            _targetTransform.localScale != _lastScale ||
            _targetTransform.localRotation != _lastRot)
        {
            //IEnumerator coroutine = ComponentInspector_Transform.GetTransformInfo();
            //StartCoroutine(coroutine);
            //ComponentInspector_Transform.GetTransformInfo();
            TransformInspectorInstance.GetTransformInfo();

            _lastPos = _targetTransform.position;
            _lastScale = _targetTransform.localScale;
            _lastRot = _targetTransform.localRotation;
        }
    }


    #region Object Selection

    private void SetTarget(GameObject obj)
    {
        _target = obj;
        _targetTransform = obj.transform;
        
        if (obj.TryGetComponent(out ObjectInfo info)) 
        { 
            _targetInfo = info;
           // //Debug.Log("New Target Sent: " + target);
            
            onNewTarget?.Invoke();
            DestroyModularItems();
            CreateModularItems();
            if (_targetInfo.SetLayerOnSelection) LayerManager.Instance.ChangeLayer(_targetInfo.editorLayer);
        }
        else 
        {
           // //Debug.Log("Selected Object Missing Object Info Script");
            //targetInfo = null;
        }
    }

    private void DeselectObject()
    {
        _target = null;
        _targetTransform = null;
        _targetInfo = null;
        onClearTarget?.Invoke();
        DestroyModularItems();

    }

    /// <summary>
    /// Destroy component items when item deselected
    /// </summary>
    /// <param name="container"></param>
    void DestroyModularItems()
    {
        //foreach (var insp in _modularInspectors)
        //{
        //    Destroy(insp);
        //}
        //_modularInspectors.Clear();
        foreach (Transform child in ContentParent)
        {
            if (child.TryGetComponent<ComponentInspector_Name>(out var _))
                continue;
            if (child.TryGetComponent<ComponentInspector_Transform>(out var _))
                continue;

            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        _modularInspectors.Clear();
        _selectables.Clear();
    }

    private GameObject GetComponentInspectorPrefab(ModularComponentInfo comp)
    {
        switch (comp)
        {
            case ComponentInfo_Airway _:
                return ComponentInspectorPrefab_Airway;

            case ComponentInfo_AudioSource _:
                return componentInspectorPrefab_AudioSource;

            case ComponentInfo_Cable _:
                return componentInspectorPrefab_Cable;

            //case ComponentInfo_EntryLabel _:
            //    return componentInspectorPrefab_EntryLabel;

            //case ComponentInfo_FireExtinguisher _:
            //    return componentInspectorPrefab_FireExtinguisher;

            case ComponentInfo_Interactable _:
                return componentInspectorPrefab_Interactable;

            case ComponentInfo_JunctionProperties _:
                return componentInspectorPrefab_JunctionProperties;

            case ComponentInfo_Lifeline _:
                return componentInspectorPrefab_Lifeline;

            case ComponentInfo_Light _:
                return componentInspectorPrefab_Light;

            case ComponentInfo_MineSegment _:
                return componentInspectorPrefab_MineSegment;

            case ComponentInfo_NPC _:
                return componentInspectorPrefab_NPC;

            case ComponentInfo_Rigidbody _:
                return componentInspectorPrefab_Rigidbody;

            case ComponentInfo_StaticGasZone _:
                return componentInspectorPrefab_StaticGasZone;

            case ComponentInfo_CollisionAudio _:
                return componentInspectorPrefab_CollisionAudio;
            
            case ComponentInfo_AmbientAudio _:
                return componentInspectorPrefab_AmbientAudio;

            case ComponentInfo_VentFire:
                return componentInspectorPrefab_VentFire;

            case ComponentInfo_DynamicVentControl:
                //return componentInspectorPrefab_DynamicVentControl;
                return GenericComponentInspectorPrefab;

            case ComponentInfo_Color:
                return componentInspectorPrefab_Color;

            case ComponentInfo_Decal:
                return componentInspectorPrefab_Decal;

            case ComponentInfo_MethaneGenerator:
                return GenericComponentInspectorPrefab;
            case ComponentInfo_ShowOnMapMan:
                return componentInspectorPrefab_ShowOnMapMan;
        }

        if (comp.TryGetComponent<IInspectableComponent>(out var _))
        {
            return GenericComponentInspectorPrefab;
        }


        return null;
    }

    /// <summary>
    /// create inspector items when selected
    /// </summary>
    /// <param name="container"></param>
    void CreateModularItems()
    {
        foreach (var comp in _targetInfo.GetModularComponentInfo())
        {
            if (comp is ComponentInfo_NetworkedObject)
                continue;

            //some components need special processing (for now)
            if (comp is ComponentInfo_Cable)
            {
                ComponentInfo_Cable cableInfo = comp as ComponentInfo_Cable;
                GameObject cableObj = new GameObject();
                if (cableInfo.CableType == CableType.Curtain || cableInfo.CableType == CableType.Line)
                {
                    cableObj = Instantiate(componentInspectorPrefab_Curtain, ContentParent);
                }
                else
                {
                    cableObj = Instantiate(componentInspectorPrefab_Cable, ContentParent);
                }

                _modularInspectors.Add(cableObj);

                ComponentInspector_Cable cableComponent = cableObj.GetComponent<ComponentInspector_Cable>();
                //cableComponent.Index = i;
                cableComponent.HeaderText.text = cableInfo.ComponentName;
                cableComponent.SetTargetComponent(cableInfo);
            }
            else if (comp is ComponentInfo_StaticGasZone)
            {
                ComponentInfo_StaticGasZone info = comp as ComponentInfo_StaticGasZone;
                GameObject obj = Instantiate(componentInspectorPrefab_StaticGasZone, ContentParent);
                _modularInspectors.Add(obj);

                ComponentInspector_StaticGasZone component = obj.GetComponent<ComponentInspector_StaticGasZone>();
                component.SetTargetComponent(info);
                component.HeaderText.text = info.componentName;

                for (int i = 0; i < info.SubGasZones.Count; i++)                
                {
                    //Debug.Log("Inspector make sub gas zones");
                    SubGasZoneData subInfo = info.SubGasZones[i];
                    GameObject subObj = Instantiate(componentInspectorPrefab_SubGasZone, ContentParent);
                    _modularInspectors.Add(subObj);

                    ComponentInspector_SubGasZone compInspector = subObj.GetComponent<ComponentInspector_SubGasZone>();

                    compInspector.Index = i;
                    subInfo.ZoneInspector = compInspector;
                    compInspector.Initialize(info, subInfo);
                }

            }
            else
            {
                try
                {
                    var prefab = GetComponentInspectorPrefab(comp);
                    if (prefab == null)
                        continue;

                    var inspectorObj = Instantiate(prefab, ContentParent);
                    if (inspectorObj == null)
                        continue;

                    _modularInspectors.Add(inspectorObj);

                    if (inspectorObj.TryGetComponent<ComponentInspector>(out var inspect))
                    {
                        inspect.SetTargetComponent(comp);
                    }

                    _compInspectors.Clear();
                    inspectorObj.GetComponentsInChildren<IComponentInspector>(_compInspectors);
                    foreach (var compInspector in _compInspectors)
                    {
                        compInspector.SetInspectedComponent(comp);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error creating modular component inspector: {ex.Message} {ex.StackTrace}");
                }
            }
        }

        ResetKeyboardNavigationOrder();
        
        SizeContainerContent(true);
    }

    private void ResetKeyboardNavigationOrder()
    {
        if (ContentParent == null)
            return;

        _selectables.Clear();

        ContentParent.GetComponentsInChildren<Selectable>(false, _selectables);

        Selectable prev = null;
        
        for (int i = 0; i < _selectables.Count; i++)
        {

         
            var current = _selectables[i];
            var next = FindNext(i); 

            Navigation nav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = prev,
                selectOnRight = next,
                selectOnDown = next,
                selectOnUp = prev,
            };

            //with the current input system the navigation takes precedence over
            //the slider - clear navigation in the slider direction for now
            var slider = current as Slider;
            if (slider != null)
            {
                if (slider.direction == Slider.Direction.LeftToRight || 
                    slider.direction == Slider.Direction.RightToLeft)
                {
                    nav.selectOnLeft = null;
                    nav.selectOnRight = null;
                }
                else
                {
                    nav.selectOnUp = null;
                    nav.selectOnDown = null;
                }
            }

            current.navigation = nav;
            prev = current;
        }
    }

    private Selectable FindNext(int index)
    {
        if (_selectables == null || index >= (_selectables.Count - 1))
            return null;

        for (int i = index + 1; index < _selectables.Count; i++)
        {
            if (_selectables[i] != null && _selectables[i].interactable)
                return _selectables[i];
        }

        return null;
    }

    public void SizeContainerContent(bool rebuildLayout = false)
    {

        //float height = 0;
        //Vector3 size = _contentRt.sizeDelta;
        //foreach (RectTransform child in _subContentRt)
        //{
        //    height = height + child.sizeDelta.y;
        //}
        //foreach (RectTransform child in _contentRt)
        //{
        //    height = height + child.sizeDelta.y ;
        //}

        //_contentRt.sizeDelta = new Vector3(size.x, height, size.z);

        ////Debug.Log("Resize Container");
        // if not root, also resize root

        if (rebuildLayout) RebuildLayout(false);
    }

    public void RebuildLayout(bool modularOnly = true)
    {
        // _layout.enabled = true;

        //if (modularOnly) { LayoutRebuilder.ForceRebuildLayoutImmediate(_subContentRt); }
        //else { LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRt); }

        LayoutRebuilder.ForceRebuildLayoutImmediate(ContentParent);
        
        //_layout.enabled = false;
    }
    
    #endregion

}
