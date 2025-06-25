using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using NIOSH_EditorLayers;
using NIOSH_MineCreation;

[System.Serializable]
public struct PlacementOptions
{
    public bool AllowTranslate;
    public bool AllowRotate;
    public bool AllowScale;
}

[System.Serializable]
public struct ScenarioCursorData
{
    public Vector3 MousePos;
    public Ray SceneRay;
    public Vector3 SurfacePos;
    public Vector3 SurfaceNormal;
    public Vector3 GroundPos;
}

/// <summary>
/// A scene object that has a logic object attached to it to drive functionality
/// </summary>
public class Placer : LayerControlledClass
{
    // Sits in the world waiting for logic to drive it
    // Needs to:
    /*
     * - Communicate with selected placerlogic
     * - Store created gizmos and activate/deactivate them
     * - 
     */
    public static Placer GetDefault()
    {
        return Util.GetDefaultManager<Placer>(null, "Placer", false);
    }

    public MineLayerTileManager MineLayerTileManager;
    public LoadableAssetManager LoadableAssetManager;

    [SerializeField] private int activeLogicIndex;
    [SerializeField] public PlacerLogic activeLogic;          // Current placer logic that is running
    [SerializeField] PlacerLogic[] placerlogicChoices; // Available placer logic options that can be swapped between TODO cannot currently swap
    [SerializeField] List<PlacerGizmo> placerGizmos;   // GIzmos in the scene that could be loaded when a placer logic is selected

    public List<PlacerGizmo> resizeGizmos;
    public Transform assetContainer;

    //public LayerMask mask;
    public LayerMask objectLayers;
    public LayerMask surfaceLayers;
    public LayerMask gizmoLayers;
    //public float minimumGizmoSize = 5f;
    //public float maximumGizmoSize = 100f;
    //public float gizmoSizeMod = 1.5f;
    public float rotationSpeed = 1f;

    public GameObject SelectedObject
    {
        get { return _selectedObject; }
        private set
        {
            if (value != _selectedObject)
            {
                _selectedObject = value;
                SelectedObjectChanged?.Invoke(value);
            }
        }
    }

    public IEnumerable<PlacablePrefab> AllSceneObjects
    {
        get
        {
            if (assetContainer == null)
                yield break;

            _sceneObjects.Clear();
            assetContainer.GetComponentsInChildren<PlacablePrefab>(false, _sceneObjects);
            foreach (var obj in _sceneObjects)
            {
                if (obj == null || obj.gameObject == null)
                    continue;

                yield return obj;
            }
        }
    }

    public bool IsInputLocked
    {
        get => inputLocked;
    }

    public ObjectInfo SelectedObjectInfo
    {
        get { return _selObjInfo; }
    }

    public ScenarioCursorData CurrentCursor
    {
        get { return _currentCursor; }
    }

    public RaycastHit CurrentCursorHit
    {
        get { return _cursorHit; }
    }

    public LayerManager.EditorLayer CurrentLayer
    {
        get { return _currentLayer; }
    }

    public event Action<GameObject> SelectedObjectChanged;
    public event Action SceneObjectListChanged;

    //public delegate void OnObjectSelected( GameObject obj);
    //public OnObjectSelected onObjectSelected;

    //public delegate void OnObjectDeselected();
    //public OnObjectDeselected onObjectDeselected;

    InputTargetController inputTargetController;

    [SerializeField] GameObject gizmoUIPanel;
    [SerializeField] GameObject _ventUIPanel;
    public GameObject SceneControlsUIPanel;
    //public Action onGizmoReleased;

    public Color hoveredColor;
    public Color selectedColor;
    public Color gizmoColor;
    public Color ventNodeColor;
    public Color ventConnectionColor;

    private CameraManager camManager;

    private GameObject cachedSelectedObject; // need to store this to pass selected object between selection modes when changing
                                             //public LayerManager.EditorLayer currentEditorLayer;

    //private List<PlacerGizmo> enabledGizmos = new List<PlacerGizmo>();
    //private List<PlacerGizmo> activeGizmos = new List<PlacerGizmo>();
    private PlacerGizmo selectedGizmo;        // The gizmo currently selected/in use by the user

    private bool inputLocked;
    //private bool _usingPlacementOverride = false;

    //private RaycastHit[] _raycastHits;
    //private int _numHits;

    private RaycastHit[] _objectRayHits;
    private int _numObjectHits;

    private RaycastHit[] _gizmoRayHits;
    private int _numGizmoHits;

    private RaycastHit[] _groundRayHits;
    private int _numGroundHits;

    private bool _resetSnappyRotaionFlag = false;

    private int _raycastObjectsMask;
    private int _raycastSurfaceMask;
    private int _raycastGroundMask;
    private int _raycastMineTilesMask;
    private int _raycastSnapZoneMask;
    private int _gizmoLayer;
    private int _ventLayer;

    //private Collider[] _mineTileResults;
    //private int _numMineTileResults;
    //private bool _mineTileSnapped;

    private GameObject _selectedObject;
    private PlacablePrefab _selPlaceable;
    private ObjectInfo _selObjInfo;
    private MineLayerTile _selMineTile;
    //private RotatorSetAccess _selRotatorSet;
    private IScenarioEditorMouseDrag _selMouseDrag;
    private IScenarioEditorMouseClick _selMouseClick;
    private IScenarioEditorMouseMove _selMouseMove;
    private IScenarioEditorResizable _selResizable;
    private IScenarioEditorSelectable _selSelectable;
    private IScenarioEditorFocusTarget _selFocusTarget;
    private GameObject _cursorObject; //object under cursor on mouse down
    private RaycastHit _cursorHit;

    private IScenarioEditorMouseHover _hover;
    private GameObject _hoverObj;
    private RaycastHit _hoverHit;

    private GizmoKind _activeGizmoType;
    private PlacerGizmo _activeGizmo = null;
    private bool _allowGizmoTypeChange = true;
    private bool _cancelSelectionClick = false;

    private bool _objectUnderCursorAtDragStart;
    //private Ray _startDragRay;
    //private Vector3 _startDragPos;
    //private Vector3 _startDragMousePos;
    private ScenarioCursorData _startDragCursor;

    //private Ray _lastDragRay;
    //private Vector3 _lastDragPos;
    //private Vector3 _lastDragMousePos;
    private ScenarioCursorData _currentCursor;
    private ScenarioCursorData _lastDragCursor;

    private bool _dragStarting = false;
    private bool _dragInProcess = false;
    private bool _dragOffScreenStart = false;

    private Plane _defaultGroundPlane = new Plane(Vector3.up, Vector3.zero);
    private float _snappyDelay = 0;
    private Vector3 prior;

    private List<PlacablePrefab> _sceneObjects = new List<PlacablePrefab>();

    LayerManager.EditorLayer _currentLayer;
    GizmoOrder CreateGizmoRequest(GizmoKind kind, GizmoAxis axis)
    {
        GizmoOrder request;
        request.axis = axis;
        request.kind = kind;

        return request;
    }

    private void Awake()
    {
        _startDragCursor = new ScenarioCursorData();
        _lastDragCursor = new ScenarioCursorData();

        //_raycastHits = new RaycastHit[50];
        //_numHits = 0;

        //_mineTileResults = new Collider[80];
        //_numMineTileResults = 0;

        _objectRayHits = new RaycastHit[80];
        _groundRayHits = new RaycastHit[80];
        _gizmoRayHits = new RaycastHit[80];

        _numObjectHits = 0;
        _numGroundHits = 0;
        _numGizmoHits = 0;

        _raycastObjectsMask = LayerMask.GetMask("Gizmo", "VentVisualization", "Default", "Floor", "MineSegments");
        _raycastSurfaceMask = LayerMask.GetMask("Gizmo", "Default", "Floor", "MineSegments");
        _raycastGroundMask = LayerMask.GetMask("Floor", "MineSegments");
        _raycastMineTilesMask = LayerMask.GetMask("MineSegments");
        _raycastSnapZoneMask = LayerMask.GetMask("Ignore Raycast");

        _gizmoLayer = LayerMask.NameToLayer("Gizmo");
        _ventLayer = LayerMask.NameToLayer("VentVisualization");
        //currentEditorLayer = LayerManager.EditorLayer.Mine;

        _hoverObj = null;
        _hover = null;
    }
    private new void Start()
    {
        base.Start();

        if (MineLayerTileManager == null)
            MineLayerTileManager = MineLayerTileManager.GetDefault();
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        camManager = FindObjectOfType<CameraManager>();

        placerlogicChoices = new PlacerLogic[]
        {
            new GizmoPlacerLogic(null,this,camManager),
            new DragDropPlacerLogic(null, this, camManager),
            new DragSnapPlacerLogic(null, this, camManager),
            new DragDropVentPlacerLogic(null, this, camManager),
            new CablePlacerLogic(null, this, camManager),
            new SnapCurtainPlacerLogic(null, this, camManager)

        };


        // Hide all scene gizmos
        foreach (PlacerGizmo gizmo in placerGizmos)
        {
            if (gizmo != null)
            {
                gizmo.gameObject.SetActive(false);
            }

        }

        inputTargetController = FindObjectOfType<InputTargetController>();

        //LayerManager.Instance.layerChanged += LayerChanged;
        inputTargetController.onNewInputTarget += OnNewInputTarget;

        //DeactivateResizeGizmos();
        DeactivateGizmos();
    }

    private void OnDestroy()
    {

        inputTargetController.onNewInputTarget -= OnNewInputTarget;
        //LayerManager.Instance.layerChanged -= LayerChanged;
    }

    /// <summary>
    /// Cancel a click/release selection change (if the left click has been handled externally)
    /// </summary>
    public void CancelSelectionClick()
    {
        _cancelSelectionClick = true;
    }

    private void Update()
    {
        ProcessMouseInput();
        ProcessKeyboardInput();

        UpdateActiveGizmos();

        //if (activeLogic != null)
        //{
        //    if (!inputLocked) { activeLogic.DoLogic(); }
        //    activeLogic.DoSupportLogic();
        //}

    }

    private void LateUpdate()
    {
        if (SelectedObject != null)
            SelectedObject.transform.hasChanged = false;
    }

    private void ProcessMouseInput()
    {
        //if (_currentLayer == LayerManager.EditorLayer.Cables)
        //    return;

        if (!_dragInProcess && inputLocked)
            return; //check for mouse button up if dragging, even if input is locked

        //perform object raycast
        RaycastScene(_objectRayHits, out _numObjectHits, QueryTriggerInteraction.Collide);
        _cursorObject = SearchForSelectableObjects(Camera.main.transform.position, _objectRayHits, _numObjectHits, out _cursorHit);
        
        _lastDragCursor = _currentCursor;
        _currentCursor = GetCurrentCursorData();

        if (!RaiseMouseHover())
            ClearMouseHover();

        if (_selMouseMove != null && SelectedObject != null)
        {
            _selMouseMove.OnScenarioEditorMouseMove(this, _cursorHit, _currentCursor);
        }

        if (Input.GetMouseButtonUp(0))
        {
            float mouseDist = Vector3.Distance(_startDragCursor.MousePos, Input.mousePosition);
            if (!_dragInProcess && mouseDist < 10.0f)
            {
                bool selectionLocked = false;

                if (_selMouseClick != null)
                    selectionLocked = _selMouseClick.IsSelectionLocked;

                if (!inputLocked && !selectionLocked && !_cancelSelectionClick)
                {
                    //scene clicked
                    SearchSceneObjects();
                    RaiseMouseButtonUp(0);
                }
            }
            else
            {
                CompleteMouseDrag();
            }

            _dragStarting = false;
            _cancelSelectionClick = false;
        }

        if (inputLocked)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            //perform gizmo raycast
            RaycastGizmos();
            StartMouseDrag();
            RaiseMouseButtonDown(0);
        }

        if (Input.GetMouseButton(0) && (_dragStarting || _dragInProcess))
        {
            ProcessMouseDrag();
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaiseMouseButtonDown(1);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            RaiseMouseButtonUp(1);
        }

    }

    private bool RaiseMouseHover()
    {
        if (_cursorObject != null)
        {
            IScenarioEditorMouseHover hover = null;
            if (_cursorHit.collider.gameObject.TryGetComponent<IScenarioEditorMouseHover>(out hover))
            {
                RaiseMouseHover(_cursorHit.collider.gameObject, hover);
                return true;
            }
            else if (_cursorObject.TryGetComponent<IScenarioEditorMouseHover>(out hover))
            {
                RaiseMouseHover(_cursorObject, hover);
                return true;
            }
        }

        return false;
    }

    public void RaiseSceneObjectListChanged()
    {
        SceneObjectListChanged?.Invoke();
    }

    private void RaiseMouseHover(GameObject obj, IScenarioEditorMouseHover hover)
    {
        if (_hoverObj == obj)
            return;

        if (_hover != null || _hoverObj != null)
            ClearMouseHover();

        if (obj == null || hover == null)
            return;


        hover.ScenarioEdtiorMouseHoverBegin(this, _cursorHit, _currentCursor);
        _hoverObj = obj;
        _hover = hover;
        _hoverHit = _cursorHit;
    }

    private void ClearMouseHover()
    {
        if (_hover != null)
        {
            _hover.ScenarioEdtiorMouseHoverEnd(this, _hoverHit, _currentCursor);
        }

        _hover = null;
        _hoverObj = null;
    }

    private void RaiseMouseButtonDown(int button)
    {
        if (_selMouseClick == null)
            return;

        _selMouseClick.OnScenarioEditorMouseDown(this, button, _cursorHit, _currentCursor);
    }

    private void RaiseMouseButtonUp(int button)
    {
        if (_selMouseClick == null)
            return;

        _selMouseClick.OnScenarioEditorMouseUp(this, button, _cursorHit, _currentCursor);
    }

    private void ProcessKeyboardInput()
    {
        if (inputLocked)
            return;

        if (_allowGizmoTypeChange)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SwitchActiveGizmos(GizmoKind.None);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SwitchActiveGizmos(GizmoKind.Pan);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SwitchActiveGizmos(GizmoKind.Rotate);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SwitchActiveGizmos(GizmoKind.Scale);
            }
        }

        if (SelectedObject != null)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                RotateToNextObject(false);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                RotateToNextObject(true);
            }

            if((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                if (Input.GetKeyDown(KeyCode.D))
                {                    
                    PerformDuplicate();
                }
            }
        }


        if (Input.GetKeyDown(KeyCode.F))
        {
            FocusSelectedObject();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectObject();
        }
        if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
        {
            if (_currentLayer != LayerManager.EditorLayer.Cables)
                DestroySelectedObject();
        }

    }

    private void PerformDuplicate()
    {
        if (_selObjInfo == null)
            return;

        LoadableAsset newLoadable = null;
        newLoadable = LoadableAssetManager.FindAsset(_selObjInfo.AssetID);
        var newObj = LoadableAssetManager.DuplicateObject(_selObjInfo, newLoadable);
        if(TryGetComponent<NetworkedObject>(out var networkedObject))
        {
            Guid guid = Guid.NewGuid();
            networkedObject.UniqueIDString = guid.ToString();
            networkedObject.uniqueID = guid;
        }
        SelectObject(newObj);

        RaiseSceneObjectListChanged();
    }

    private void RotateToNextObject(bool clockwise)
    {
        if (!_dragInProcess || _selObjInfo == null)
            return;

        //var newObj = _selRotatorSet.GetRotateToObject(clockwise, assetContainer);
        //if (newObj == null || newObj == SelectedObject)
        //    return;

        var variantSet = LoadableAssetManager.GetVariantSet(_selObjInfo.AssetID);
        if (variantSet == null)
            return;

        LoadableAsset newLoadable = null;
        if (clockwise)
            newLoadable = variantSet.GetNext(_selObjInfo.AssetID);
        else
            newLoadable = variantSet.GetPrev(_selObjInfo.AssetID);

        if (newLoadable == null)
            return;

        var newObj = LoadableAssetManager.InstantiateEditorAsset(newLoadable.AssetID, SelectedObject.transform.position,
            SelectedObject.transform.rotation, assetContainer);
        newObj.SetActive(true);

        DestroySelectedObject();
        SelectObject(newObj);

        RaiseSceneObjectListChanged();

        Physics.SyncTransforms();

        StartMouseDrag();
        _dragInProcess = true;
        _objectUnderCursorAtDragStart = true;
        ProcessMouseDrag();
    }

    private void UpdateActiveGizmos()
    {
        foreach (var gizmo in placerGizmos)
        {
            if (gizmo.gameObject.activeSelf)
            {
                gizmo.AlignGizmo(SelectedObject);
            }
        }

        foreach (var gizmo in resizeGizmos)
        {
            if (gizmo.gameObject.activeSelf)
            {
                gizmo.AlignGizmo(SelectedObject);
            }
        }
    }

    public void StartMouseDragFromOffScreen()
    {
        StartMouseDrag();
        inputLocked = false;
        _objectUnderCursorAtDragStart = true;
        _dragOffScreenStart = true;
    }

    private ScenarioCursorData GetCurrentCursorData()
    {
        var cursor = new ScenarioCursorData();

        cursor.MousePos = Input.mousePosition;
        //cursor.SceneRay = Camera.main.ScreenPointToRay(cursor.MousePos);
        if (GetSceneRay(out var ray))
            cursor.SceneRay = ray;
        else
            cursor.SceneRay = new Ray(Vector3.zero, Vector3.forward);

        cursor.SurfacePos = RaycastGround(true, out cursor.SurfaceNormal, allowRoof:true);
        cursor.GroundPos = RaycastGround(false, out var normal);

        return cursor;
    }

    private void StartMouseDrag()
    {        
        var cursor = GetCurrentCursorData();
        _objectUnderCursorAtDragStart = SearchForSelectedObject(_objectRayHits, _numObjectHits);

        _startDragCursor = cursor;
        _lastDragCursor = cursor;

        _dragInProcess = false;
        _dragStarting = true;
       
    }

    private void ProcessMouseDrag()
    {
        if (!_dragInProcess)
        {
            var mouseDist = Vector3.Distance(Input.mousePosition, _startDragCursor.MousePos);
            if (mouseDist > 5.0f)
            {
                _dragInProcess = true;
                _dragStarting = false;

                if (_selPlaceable != null)
                    _selPlaceable.UnPlace();

                if (_selMouseDrag != null)
                    _selMouseDrag.StartMouseDrag(this);
            }
            else
                return;
        }

        if (_selMouseDrag != null && _objectUnderCursorAtDragStart)
        {
            _selMouseDrag.ProcessMouseDrag(_lastDragCursor, _currentCursor);
        }
        else if (_activeGizmo != null && SelectedObject != null)
        {
            _activeGizmo.ProcessMouseDrag(SelectedObject.transform, _lastDragCursor, _currentCursor);
        }        
        else if (_activeGizmo == null && SelectedObject != null && _objectUnderCursorAtDragStart && (_activeGizmoType == GizmoKind.Resize || _activeGizmoType == GizmoKind.None || _dragOffScreenStart))
        {
            DragSelectedObject(_currentCursor.SurfacePos, _currentCursor.SurfaceNormal);
        }

        //if (SelectedObject != null && SelectedObject.transform.hasChanged)
        //{
        //    if (_selPlaceable != null && _selPlaceable.ParentToMineTile && _selMineTile == null)
        //    {
        //        _selPlaceable.SearchForNewParent();
        //    }

        //    //SelectedObject.transform.hasChanged = false;
        //}

    }

    private void CompleteMouseDrag()
    {
        _dragInProcess = false;
        _dragStarting = false;
        _dragOffScreenStart = false;

        if (_selMouseDrag != null)
            _selMouseDrag.CompleteMouseDrag();

        if (_selPlaceable != null)
            _selPlaceable.SetPlaced();

        SwitchActiveGizmos(_activeGizmoType);
    }

    //private float RotationAngle(Quaternion q, Vector3 axis)
    //{
    //    Vector3 v1;
    //    if (axis.z == 0)
    //        v1 = new Vector3(-axis.y, axis.x, 0);
    //    else if (axis.y == 0)
    //        v1 = new Vector3(-axis.z, 0, axis.x);
    //    else
    //        v1 = new Vector3(0, -axis.z, axis.y);

    //    v1.Normalize();

    //    var v2 = q * v1;

    //    return Vector3.Angle(v1, v2);
    //}

    private void DragSelectedObject(Vector3 pos, Vector3 normal)
    {

        if (SelectedObject == null || _selObjInfo == null || !_selObjInfo.IsTranslatable)
            return;

        if (_selResizable != null && !_dragOffScreenStart && _selObjInfo.PlacementTypeOverride != PlacementTypeOverride.SnapCurtain)
            return;

        bool rotateToSurface = true;
        if (_selPlaceable != null)
            rotateToSurface = _selPlaceable.RotateToSurface;

        if (_selObjInfo != null && _selObjInfo.PlacementAnchor != null)
        {
            var anchor = _selObjInfo.PlacementAnchor;
            SelectedObject.transform.position = pos - (anchor.position - SelectedObject.transform.position);
        }
        else
        {
            SelectedObject.transform.position = pos;
        }

        if (rotateToSurface)
        {
            //var newRot = Quaternion.FromToRotation(SelectedObject.transform.up, normal) * SelectedObject.transform.rotation;                        
            //var newRot = Quaternion.LookRotation(Vector3.forward, normal);

            
            var newRot = Quaternion.FromToRotation(Vector3.up, normal);
            
            
            var originalTwist = Util.TwistAngle(SelectedObject.transform.rotation, SelectedObject.transform.up);
            if (originalTwist < 0)
                originalTwist += 360.0f;

            var newTwist = Util.TwistAngle(newRot, normal);

            //newRot *= Quaternion.AngleAxis(originalTwist - newTwist, normal);
            newRot = Quaternion.AngleAxis(originalTwist - newTwist, normal) * newRot;
            SelectedObject.transform.rotation = newRot;

            //Debug.Log($"Original Twist: {originalTwist:F2} New Twist: {newTwist:F2}");
            

            /***
            var newRot = Quaternion.FromToRotation(Vector3.up, normal);

            Util.DecomposeSwingTwist(SelectedObject.transform.rotation, SelectedObject.transform.up, out var originalSwing, out var originalTwist);
            Util.DecomposeSwingTwist(newRot, normal, out var newSwing, out var newTwist);

            //SelectedObject.transform.rotation = originalTwist * newSwing;
            //SelectedObject.transform.rotation =  originalTwist * (Quaternion.Inverse(newTwist) * newRot);
            //SelectedObject.transform.rotation = originalTwist * originalSwing;
            SelectedObject.transform.rotation = originalTwist * newSwing;

            Debug.Log($"Original Twist: {originalTwist} New Twist: {newTwist}");
            ***/

            //var startRot = RotationAngle(SelectedObject.transform.rotation, SelectedObject.transform.up);
            //var newRot = Quaternion.FromToRotation(Vector3.up, normal);
            //var endRot = RotationAngle(newRot, normal);

            //SelectedObject.transform.rotation = newRot * Quaternion.AngleAxis(endRot - startRot, normal);


            //preserve local Y axis rotation
            //var euler = SelectedObject.transform.localRotation.eulerAngles;
            //SelectedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            //var newEuler = SelectedObject.transform.localRotation.eulerAngles;

            //SelectedObject.transform.localRotation = Quaternion.Euler(newEuler.x, euler.y, newEuler.z);

            //SelectedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);

        }

        if (_selObjInfo.PlacementTypeOverride == PlacementTypeOverride.SnapCurtain)
        {
            var snapZone = RaycastSnapZones();
            if (snapZone != null) 
            { 
                SnapObjectToSnapZone(SelectedObject.transform, snapZone.transform);
                return;
            }


            if (Time.time > _snappyDelay)
            {
                if(prior == pos)
                {
                    _snappyDelay = Time.time + 0.01f;
                    _resetSnappyRotaionFlag = true;
                    return;
                }
                prior = pos;
                //Vector3 scaleValues;

                if (_resetSnappyRotaionFlag)
                {
                    SelectedObject.transform.rotation = Quaternion.identity;
                    _resetSnappyRotaionFlag = false;
                }
                //Get the roof distance
                Ray ray = new Ray(SelectedObject.transform.position, Vector3.up);
                int numHits = 0;
                int selection = -1;
                float minDist = float.MaxValue;
                Vector3 roofHit = Vector3.zero;
                RaycastHit[] hits = new RaycastHit[80];
                numHits = Physics.RaycastNonAlloc(ray, hits, Mathf.Infinity, _raycastGroundMask, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < numHits; i++)
                {
                    var hit = hits[i];
                    var obj = hit.collider.gameObject;
                    if (IsSelectedObject(obj))
                    {
                        continue;
                    }
                    float dist = Vector3.Distance(ray.origin, hit.point);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        selection = i;
                    }
                }
                if (selection >= 0)
                {
                    roofHit = hits[selection].point;
                }
                else
                {
                    return;
                }
                //Get the point halfway to the ceiling
                Vector3 midPoint = Vector3.Lerp(SelectedObject.transform.position, roofHit, 0.5f);
                
                
                //Debug.Log($"First midpoint: {midPoint}");
                Vector3 rightDir = SelectedObject.transform.right;
                Vector3 leftDir = -SelectedObject.transform.right;
                //Debug.Log($"OG Directions: {rightDir}, {leftDir}");
                Vector3 forwardDir = SelectedObject.transform.forward;

                float width = float.MaxValue;
                Vector3 _cacheHit1 = Vector3.zero;
                Vector3 _cacheHit2 = Vector3.zero;

                RaycastHit[] wall1Hits = new RaycastHit[80];
                RaycastHit[] wall2Hits = new RaycastHit[80];
                int cachedIndex = 0;
                //Now comes scanning in a circle to find the closest wall
                for (int i = 0; i < 360; i += 5)
                {
                    Vector3 rotatedRightDir = Quaternion.AngleAxis(i, SelectedObject.transform.up) * rightDir;
                    Vector3 rotatedLeftDir = Quaternion.AngleAxis(i, SelectedObject.transform.up) * leftDir;
                    //Debug.Log($"Rotated Directions: {rotatedRightDir}, {rotatedLeftDir}");
                    Ray wallRay1 = new Ray(midPoint, rotatedRightDir);
                    Ray wallRay2 = new Ray(midPoint, rotatedLeftDir);
                    Debug.DrawRay(midPoint, rotatedRightDir);
                    Debug.DrawRay(midPoint, rotatedLeftDir);
                    Vector3 hit1 = new Vector3();
                    Vector3 hit2 = new Vector3();
                    minDist = float.MaxValue;
                    selection = -1;
                    numHits = Physics.RaycastNonAlloc(wallRay1, wall1Hits, Mathf.Infinity, _raycastGroundMask, QueryTriggerInteraction.Ignore);
                    
                    for (int j = 0; j < numHits; j++)
                    {
                        var hit = wall1Hits[j];
                        var obj = hit.collider.gameObject;
                        if (IsSelectedObject(obj))
                        {
                            continue;
                        }
                        float dist = Vector3.Distance(ray.origin, hit.point);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            selection = j;
                        }
                    }
                    if (selection >= 0)
                    {
                        //Debug.Log($"Selection index = {selection} and the wall hit length is: {wall1Hits.Length}");
                        hit1 = wall1Hits[selection].point;
                        //Debug.Log($"Hit1: {hit1}");
                    }
                    else
                    {
                        continue;
                    }

                    minDist = float.MaxValue;
                    selection = -1;
                    numHits = Physics.RaycastNonAlloc(wallRay2, wall2Hits, Mathf.Infinity, _raycastGroundMask, QueryTriggerInteraction.Ignore);
                    for (int j = 0; j < numHits; j++)
                    {
                        var hit = wall2Hits[j];
                        var obj = hit.collider.gameObject;
                        if (IsSelectedObject(obj))
                        {
                            continue;
                        }
                        float dist = Vector3.Distance(ray.origin, hit.point);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            selection = j;
                        }
                    }
                    if (selection >= 0)
                    {
                        hit2 = wall2Hits[selection].point;
                        //Debug.Log($"Hit2: {hit2}");
                    }
                    else
                    {
                        continue;
                    }
                    float newWidth = Vector3.Distance(midPoint, hit1) + Vector3.Distance(midPoint, hit2);
                    //Debug.Log($"Width compare: new - {newWidth} old - {width}, {hit1} and {hit2}");
                    //If the newest width is smaller than what came before, cache it
                    if (newWidth < width)
                    {
                        width = newWidth;
                        _cacheHit1 = hit1;
                        _cacheHit2 = hit2;
                        //Debug.Log($"Caching hits w:{width} at points {hit1} and {hit2}");
                        cachedIndex = i;
                    }
                    else
                    {
                        //Vector3 rotatedForwardDir = Quaternion.AngleAxis(i - 1, SelectedObject.transform.up) * forwardDir;
                        //Vector3 newMidpoint = Vector3.Lerp(_cacheHit1, _cacheHit2, 0.5f);
                        //Debug.Log($"New midpoint! {newMidpoint} from {_cacheHit1} and {_cacheHit2}");
                        //Ray groundRay = new Ray(newMidpoint, Vector3.down);
                        //RaycastHit hitInfo;
                        ////RaycastHit ceilingHit;
                        //Vector3 groundHitPoint = Vector3.zero;
                        //Vector3 ceilingHitPoint = Vector3.zero;
                        //RaycastHit[] groundRayHits = new RaycastHit[80];
                        //RaycastHit[] ceilingRayHits = new RaycastHit[80];

                        //minDist = float.MaxValue;
                        //selection = -1;
                        //numHits = Physics.RaycastNonAlloc(groundRay, groundRayHits, Mathf.Infinity, _raycastGroundMask, QueryTriggerInteraction.Ignore);
                        //for (int j = 0; j < numHits; j++)
                        //{
                        //    var hit = groundRayHits[j];
                        //    var obj = hit.collider.gameObject;
                        //    if (IsSelectedObject(obj))
                        //    {
                        //        continue;
                        //    }
                        //    float dist = Vector3.Distance(ray.origin, hit.point);
                        //    if (dist < minDist)
                        //    {
                        //        minDist = dist;
                        //        selection = j;
                        //    }
                        //}
                        //if (selection >= 0)
                        //{
                        //    groundHitPoint = groundRayHits[selection].point;
                        //    //groundHitPoint = hitInfo.point;
                        //}
                        //else
                        //{
                        //    return;
                        //}
                        //Ray ceilingRay = new Ray(newMidpoint, Vector3.up);
                        //RaycastHit hitInfo2;
                        //minDist = float.MaxValue;
                        //selection = -1;
                        //numHits = Physics.RaycastNonAlloc(ceilingRay, ceilingRayHits, Mathf.Infinity, _raycastGroundMask, QueryTriggerInteraction.Ignore);
                        //for (int j = 0; j < numHits; j++)
                        //{
                        //    var hit = ceilingRayHits[j];
                        //    var obj = hit.collider.gameObject;
                        //    if (IsSelectedObject(obj))
                        //    {
                        //        continue;
                        //    }
                        //    float dist = Vector3.Distance(ray.origin, hit.point);
                        //    if (dist < minDist)
                        //    {
                        //        minDist = dist;
                        //        selection = j;
                        //    }
                        //}
                        //if (selection >= 0)
                        //{
                        //    ceilingHitPoint = ceilingRayHits[selection].point;
                        //    Debug.Log($"Ceiling hit point? {ceilingRayHits[selection].point}");
                        //    //ceilingHitPoint = hitInfo2.point;
                        //}
                        //else
                        //{
                        //    return;
                        //}

                        //SelectedObject.transform.position = groundHitPoint;

                        //Debug.Log($"Distances: Width = {Vector3.Distance(_cacheHit1, _cacheHit2)} height = {Vector3.Distance(groundHitPoint, ceilingHitPoint)}");
                        //Debug.Log($"Ground Point = {groundHitPoint} Ceiling Point = {ceilingHitPoint}");
                        //Quaternion lookRot = Quaternion.LookRotation(rotatedForwardDir, SelectedObject.transform.up);
                        //Vector3 convertedRotation = lookRot.eulerAngles;
                        //convertedRotation.x = 0;
                        //convertedRotation.z = 0;
                        ////SelectedObject.transform.rotation = Quaternion.LookRotation(rotatedForwardDir, SelectedObject.transform.up);
                        //SelectedObject.transform.eulerAngles = convertedRotation;                      
                        //SelectedObject.transform.localScale = new Vector3(Vector3.Distance(_cacheHit1, _cacheHit2) / 6, Vector3.Distance(groundHitPoint, ceilingHitPoint) / 2.2f, 1);
                        //Debug.DrawRay(SelectedObject.transform.position, rotatedForwardDir);
                        //Debug.Log($"Snappy placement at {SelectedObject.transform.position}");
                        //_snappyDelay = Time.time + 0.1f;
                        //return;
                    }
                }


                Vector3 rotatedForwardDir = Quaternion.AngleAxis(cachedIndex, SelectedObject.transform.up) * forwardDir;
                Vector3 newMidpoint = Vector3.Lerp(_cacheHit1, _cacheHit2, 0.5f);
                //Debug.Log($"New midpoint! {newMidpoint} from {_cacheHit1} and {_cacheHit2}");
                Ray groundRay = new Ray(newMidpoint, Vector3.down);
                RaycastHit hitInfo;
                //RaycastHit ceilingHit;
                Vector3 groundHitPoint = Vector3.zero;
                Vector3 ceilingHitPoint = Vector3.zero;
                RaycastHit[] groundRayHits = new RaycastHit[80];
                RaycastHit[] ceilingRayHits = new RaycastHit[80];

                minDist = float.MaxValue;
                selection = -1;
                numHits = Physics.RaycastNonAlloc(groundRay, groundRayHits, Mathf.Infinity, _raycastGroundMask, QueryTriggerInteraction.Ignore);
                for (int j = 0; j < numHits; j++)
                {
                    var hit = groundRayHits[j];
                    var obj = hit.collider.gameObject;
                    if (IsSelectedObject(obj))
                    {
                        continue;
                    }
                    float dist = Vector3.Distance(ray.origin, hit.point);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        selection = j;
                    }
                }
                if (selection >= 0)
                {
                    groundHitPoint = groundRayHits[selection].point;
                    //groundHitPoint = hitInfo.point;
                }
                else
                {
                    return;
                }
                Ray ceilingRay = new Ray(newMidpoint, Vector3.up);
                RaycastHit hitInfo2;
                minDist = float.MaxValue;
                selection = -1;
                numHits = Physics.RaycastNonAlloc(ceilingRay, ceilingRayHits, Mathf.Infinity, _raycastGroundMask, QueryTriggerInteraction.Ignore);
                for (int j = 0; j < numHits; j++)
                {
                    var hit = ceilingRayHits[j];
                    var obj = hit.collider.gameObject;
                    if (IsSelectedObject(obj))
                    {
                        continue;
                    }
                    float dist = Vector3.Distance(ray.origin, hit.point);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        selection = j;
                    }
                }
                if (selection >= 0)
                {
                    ceilingHitPoint = ceilingRayHits[selection].point;
                    Debug.Log($"Ceiling hit point? {ceilingRayHits[selection].point}");
                    //ceilingHitPoint = hitInfo2.point;
                }
                else
                {
                    return;
                }

                SelectedObject.transform.position = groundHitPoint;

                //Debug.Log($"Distances: Width = {Vector3.Distance(_cacheHit1, _cacheHit2)} height = {Vector3.Distance(groundHitPoint, ceilingHitPoint)}");
                //Debug.Log($"Ground Point = {groundHitPoint} Ceiling Point = {ceilingHitPoint}");
                
                Quaternion lookRot = Quaternion.LookRotation(rotatedForwardDir, SelectedObject.transform.up);
                Vector3 convertedRotation = lookRot.eulerAngles;
                convertedRotation.x = 0;
                convertedRotation.z = 0;
                //SelectedObject.transform.rotation = Quaternion.LookRotation(rotatedForwardDir, SelectedObject.transform.up);
                SelectedObject.transform.eulerAngles = convertedRotation;
                SelectedObject.transform.localScale = new Vector3(Vector3.Distance(_cacheHit1, _cacheHit2) / 6, Vector3.Distance(groundHitPoint, ceilingHitPoint) / 1.9f, 1);
                if (SelectedObject.transform.TryGetComponent<IScenarioEditorResizable>(out var resize))
                {
                    
                    var size = resize.Size;
                    var snapZoneScale = SelectedObject.transform.lossyScale;
                    
                    if (snapZone != null)
                    {
                        if (!snapZone.TryGetComponent<BoxCollider>(out var snapCollider))
                        {
                            Debug.LogError($"Snap zone {snapZone.name} is missing a BoxCollider!");
                            return;
                        }
                        size.x = snapCollider.bounds.size.x;
                        size.z = snapCollider.bounds.size.z;
                    }
                    else
                    {
                        size.x = Vector3.Distance(_cacheHit1, _cacheHit2);
                        size.z = snapZoneScale.z;
                    }
                    size.y = snapZoneScale.y * 1.9f;                    
                    resize.SetSize(size, newMidpoint);
                }
                //Debug.DrawRay(SelectedObject.transform.position, rotatedForwardDir);
                Debug.Log($"Snappy placement at {SelectedObject.transform.position}");
                
                _snappyDelay = Time.time + 0.1f;
                return;
            }
        }
    }

    private void SnapObjectToSnapZone(Transform target, Transform snapZone)
    {
        if (target == null || snapZone == null)
            return;

        if (target.TryGetComponent<IScenarioEditorResizable>(out var resize))
        {
            SnapObjectToSnapZone(target, resize, snapZone);
            return;
        }

        if (snapZone.name.EndsWith("N") || snapZone.name.EndsWith("S"))
        {
            target.transform.position = new Vector3(snapZone.position.x, snapZone.position.y, target.position.z);
            target.transform.localScale = new Vector3(snapZone.parent.lossyScale.x, 
                snapZone.parent.lossyScale.y, target.transform.localScale.z);
        }
        else
        {
            target.transform.position = new Vector3(target.position.x, snapZone.position.y, snapZone.position.z);
            target.transform.localScale = new Vector3(snapZone.parent.lossyScale.z, snapZone.parent.lossyScale.y, 
                target.transform.localScale.z);
        }

        target.rotation = Quaternion.LookRotation(snapZone.forward);

    }

    private void SnapObjectToSnapZone(Transform target, IScenarioEditorResizable resize, Transform snapZone)
    {
        if (!snapZone.TryGetComponent<BoxCollider>(out var snapCollider))
        {
            Debug.LogError($"Snap zone {snapZone.name} is missing a BoxCollider!");
            return;
        }

        Vector3 pos = snapZone.position;

        target.rotation = Quaternion.LookRotation(snapZone.forward);

        var size = resize.Size;

        var snapZoneScale = snapZone.lossyScale;

        //if (snapZone.parent.TryGetComponent<Collider>(out var parentCollider))
        //{
        //    size.y = parentCollider.bounds.size.y;
        //}
        //else
        //{
        //    size.y = snapCollider.size.y * snapZoneScale.y;
        //}

        //size.y = ScenarioSaveLoad.Instance.Settings.MineSettings.seamHeight;

        //fix for inaccurate seam height
        size.y = snapZoneScale.y * 2.134f;

        if (snapZone.name.EndsWith("N") || snapZone.name.EndsWith("S"))
        {
            pos.z = target.position.z;
            //size.x = snapCollider.size.x * snapZoneScale.x;
            size.x = snapCollider.bounds.size.x;
        }
        else
        {
            pos.x = target.position.x;
            //size.x = snapCollider.size.x * snapZoneScale.z;
            size.x = snapCollider.bounds.size.z;
        }

        resize.SetSize(size, pos);
    }

    public void FocusSelectedObject()
    {
        if (SelectedObject == null || camManager == null)
            return;

        camManager.GetActiveCameraLogic()?.FocusObject(SelectedObject);
    }

    public void DestroySelectedObject()
    {
        if (SelectedObject == null || _selObjInfo == null)
            return;

        //bool isDestroyedMineTile = SelectedObject.gameObject.TryGetComponent<MineLayerTile>(out var mineTile);

        var obj = SelectedObject;
        DeselectObject();

        obj.SetActive(false);
        Destroy(obj);

        RaiseSceneObjectListChanged();
        //if (isDestroyedMineTile)
        //{
        //    MineLayerTileManager.RebuildTileConnections();
        //    //StartCoroutine(DelayedMineTileUpdate());
        //}

    }
    
    //private IEnumerator DelayedMineTileUpdate()
    //{
    //    yield return new WaitForEndOfFrame();
    //    MineLayerTileManager.RebuildTileConnections();
    //}


    private GameObject GetGameObject(RaycastHit hit)
    {
        if (hit.collider == null)
            return null;

        var objInfo = hit.collider.gameObject.transform.GetComponentInParent<ObjectInfo>();
        if (objInfo != null)
            return objInfo.gameObject;

        return hit.collider.gameObject;
    }

    private bool SearchForSelectedObject(RaycastHit[] hits, int numHits)
    {
        for (int i = 0; i < numHits; i++)
        {
            var hit = hits[i];
            var obj = GetGameObject(hit);
            if (obj == null)
                continue;

            if (obj == SelectedObject)
                return true;

            var objectInfo = obj.GetComponentInParent<ObjectInfo>();
            if (objectInfo == null)
                continue;

            if (objectInfo.gameObject == SelectedObject)
                return true;
        }

        return false;
    }

    private GameObject SearchForSelectableObjects(Vector3 origin, RaycastHit[] hits, int numHits, out RaycastHit hit, bool ignoreSelected = false, float filterMinDist = -1)
    {
        int maxPriority = -1;
        float minDist = float.MaxValue;
        int selIndex = -1;
        hit = new RaycastHit();

        for (int i = 0; i < numHits; i++)
        {
            var h = hits[i];
            var obj = GetGameObject(h);
            if (obj == null)
                continue;
            ObjectInfo objectInfo = null;

            if (ignoreSelected && IsSelectedObject(obj))
                continue;

            //if (obj.layer == _gizmoLayer)
            //    continue;

            obj.TryGetComponent<ObjectInfo>(out objectInfo);
            if (objectInfo == null)
                objectInfo = obj.GetComponentInParent<ObjectInfo>();

            if (objectInfo == null)
                continue;

            if (objectInfo.editorLayer != _currentLayer)
                continue;


            float dist = Vector3.Distance(origin, h.point);
            int priority = 0;

            if (filterMinDist > 0 && dist < filterMinDist)
            {
                continue;
            }

            if (obj.layer == _ventLayer)
            {
                priority += 1500;
            }

            if (priority < maxPriority)
                continue;

            if (priority > maxPriority || dist < minDist)
            {
                selIndex = i;
                minDist = dist;
                maxPriority = priority;
            }
        }

        if (selIndex < 0)
            return null;

        hit = hits[selIndex];

        return GetGameObject(hits[selIndex]);
    }

    private PlacerGizmo SearchForGizmos(Vector3 origin, RaycastHit[] hits, int numHits)
    {
        int maxPriority = -1;
        float minDist = float.MaxValue;
        PlacerGizmo closestGizmo = null;

        for (int i = 0; i < numHits; i++)
        {
            var hit = hits[i];
            var obj = hit.collider.gameObject;

            if (obj.layer != _gizmoLayer)
                continue;

            if (!obj.TryGetComponent<PlacerGizmo>(out var gizmo))
                continue;

            int priority = 0;

            //prefer plane gizmos
            if (gizmo.Axis == GizmoAxis.XZ)
            {
                priority = 5000;
            }
            else if (gizmo.Axis != GizmoAxis.X &&
                gizmo.Axis != GizmoAxis.Y &&
                gizmo.Axis != GizmoAxis.Z)
            {
                priority = 3000;
            }

            if (priority < maxPriority)
                continue;

            float dist = Vector3.Distance(origin, hit.point);
            if (dist < minDist || priority > maxPriority)
            {
                closestGizmo = gizmo;
                minDist = dist;
                maxPriority = priority;
            }
        }

        return closestGizmo;
    }

    private Vector3 RaycastGroundPlane(Ray ray)
    {
        if (_defaultGroundPlane.Raycast(ray, out float enter))
        {
            return ray.origin + ray.direction * enter;
        }
        else
        {
            return Vector3.zero;
        }
    }

    private unsafe GameObject RaycastSnapZones()
    {
        int numHits = 0;
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!GetSceneRay(out var ray))
        {
            return null;
        }

        numHits = Physics.RaycastNonAlloc(ray, _gizmoRayHits, Mathf.Infinity, _raycastSnapZoneMask, QueryTriggerInteraction.Collide);
        if (numHits <= 0)
            return null;

        for (int i = 0; i < numHits; i++)
        {
            var hit = _gizmoRayHits[i];
            if (hit.collider.gameObject.name.Contains("AxisSnapZone"))
                return hit.collider.gameObject;
        }

        return null;
    }

    private Vector3 RaycastGround(bool includeObjectSurface, out Vector3 hitNormal, bool allowRoof = false)
    {
        int numHits = 0;
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!GetSceneRay(out var ray))
        {
            hitNormal = Vector3.up;
            return Vector3.zero;
        }

        if (includeObjectSurface)
            numHits = Physics.RaycastNonAlloc(ray, _groundRayHits, Mathf.Infinity, _raycastSurfaceMask, QueryTriggerInteraction.Ignore);
        else
            numHits = Physics.RaycastNonAlloc(ray, _groundRayHits, Mathf.Infinity, _raycastGroundMask, QueryTriggerInteraction.Ignore);


        float minDist = float.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < numHits; i++)
        {
            var hit = _groundRayHits[i];
            //var obj = GetGameObject(hit);
            var obj = hit.collider.gameObject;

            if (!allowRoof && hit.normal.y <= -0.5f)
                continue; //ignore roof hits

            if (IsSelectedObject(obj))
                continue;

            float dist = Vector3.Distance(ray.origin, hit.point);
            if (dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }

        if (closestIndex >= 0)
        {
            hitNormal = _groundRayHits[closestIndex].normal;
            //Debug.Log($"Ground hit {_raycastHits[closestIndex].collider.name} n: {_raycastHits[closestIndex].normal}");
            return _groundRayHits[closestIndex].point;
        }
        else
        {
            hitNormal = Vector3.up;
            return RaycastGroundPlane(ray);
        }
    }

    private bool IsSelectedObject(GameObject obj)
    {
        if (obj == SelectedObject)
            return true;

        //check if this object is a child of the selected object
        //replace with rigidbody check if/when all placeable objects have a rigidbody
        //var objInfo = obj.transform.GetComponentInParent<ObjectInfo>();        
        //if (objInfo != null && objInfo.gameObject == SelectedObject)
        //    return true;

        Transform xform = obj.transform.parent;

        while (xform != null)
        {
            if (xform.gameObject == SelectedObject)
                return true;

            xform = xform.parent;
        }

        return false;
    }

    private bool GetSceneRay(out Ray ray)
    {
        ray = new Ray();

        try
        {
            var viewportPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            if (viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1)
                return false;

            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"GetSceneRay error: {ex.Message} {ex.StackTrace}");
            return false;
        }

        return true;
    }

    private int RaycastScene(RaycastHit[] hits, out int numHits,  QueryTriggerInteraction raycastTriggers = QueryTriggerInteraction.Ignore)
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        numHits = 0;

        if (GetSceneRay(out var ray))
            numHits = Physics.RaycastNonAlloc(ray, hits, Mathf.Infinity, _raycastObjectsMask, raycastTriggers);

        return numHits;
    }

    public void ClearActiveGizmo()
    {
        _activeGizmo = null;
    }

    private void RaycastGizmos()
    {
        RaycastScene(_gizmoRayHits, out _numGizmoHits, QueryTriggerInteraction.Collide);
        _activeGizmo = SearchForGizmos(Camera.main.transform.position, _gizmoRayHits, _numGizmoHits);
    }

    private void SearchSceneObjects()
    {
        //_numHits = RaycastScene();
        float filterMinDist = -1;
        if (SelectedObject != null)
        {
            filterMinDist = Vector3.Distance(Camera.main.transform.position, SelectedObject.transform.position);
        }

        var selectedObj = SearchForSelectableObjects(Camera.main.transform.position, _objectRayHits, _numObjectHits, out var hit, true, filterMinDist);
        if (selectedObj == null && filterMinDist > 0)
            selectedObj = SearchForSelectableObjects(Camera.main.transform.position, _objectRayHits, _numObjectHits, out hit, true);

        if (selectedObj != null)
        {
            SelectObject(selectedObj);
        }
        else if (_cursorObject != SelectedObject)
        {
            DeselectObject();
        }


    }

    public bool IsLogicBusy()
    {
        if (activeLogic != null)
        {
            return activeLogic.CheckIfActive();
        }
        return false;
    }

    public void SwitchActiveGizmos(GizmoKind kind, bool temporary = false)
    {
        if (kind == GizmoKind.None && (_currentLayer == LayerManager.EditorLayer.Ventilation || _selResizable != null))
            kind = GizmoKind.Resize;

        DeactivateGizmos();

        _activeGizmo = null;

        if (!temporary)
            _activeGizmoType = kind;      

        if (_selObjInfo == null)
            return;

        switch (kind)
        {
            case GizmoKind.Pan:
                if (!_selObjInfo.IsTranslatable)
                    return;
                break;

            case GizmoKind.Rotate:
                if (!_selObjInfo.IsRotatable)
                    return;
                break;

            case GizmoKind.Scale:
                if (!_selObjInfo.IsScalable)
                    return;
                break;

            case GizmoKind.Resize:
                if (_selResizable == null)
                    return;
                break;
        }

        foreach (PlacerGizmo gizmo in placerGizmos)
        {
            if (gizmo.GizmoType == kind)
            {
                gizmo.gameObject.SetActive(true);
            }
        }
    }

    public void SelectObject(GameObject obj)
    {
        if (obj == null || obj == SelectedObject)
            return;

        if (SelectedObject != null)
            DeselectObject();

        Debug.Log($"Selecting Object {obj.name}");
        obj.TryGetComponent<ObjectInfo>(out _selObjInfo);
        //obj.TryGetComponent<RotatorSetAccess>(out _selRotatorSet);
        obj.TryGetComponent<MineLayerTile>(out _selMineTile);
        obj.TryGetComponent<PlacablePrefab>(out _selPlaceable);
        obj.TryGetComponent<IScenarioEditorMouseDrag>(out _selMouseDrag);
        obj.TryGetComponent<IScenarioEditorMouseClick>(out _selMouseClick);
        obj.TryGetComponent<IScenarioEditorMouseMove>(out _selMouseMove);
        obj.TryGetComponent<IScenarioEditorResizable>(out _selResizable);
        obj.TryGetComponent<IScenarioEditorSelectable>(out _selSelectable);
        obj.TryGetComponent<IScenarioEditorFocusTarget>(out _selFocusTarget);

        SelectedObject = obj;

        if (_selSelectable != null)
            _selSelectable.ScenarioEditorSelectedObject(this, true);

        //ActivateGizmos(obj);
        if (!_dragInProcess)
        {
            if (_selResizable != null)
                SwitchActiveGizmos(GizmoKind.Resize);
            else
                SwitchActiveGizmos(_activeGizmoType);
        }
    }

    public void DeselectObject()
    {
        if (SelectedObject == null)
            return;

        if (_selMouseClick != null)
            _selMouseClick.OnScenarioEditorMouseFocusLost(this);

        if (_selSelectable != null)
            _selSelectable.ScenarioEditorSelectedObject(this, false);

        DeactivateGizmos();
        SelectedObject = null;
        _selObjInfo = null;
        _selMineTile = null;
        _selPlaceable = null;
        //_selRotatorSet = null;
        _selMouseDrag = null;
        _selMouseClick = null;
        _selMouseMove = null;
        _selResizable = null;
        _selSelectable = null;
        _selFocusTarget = null;
    }

    /// <summary>
    /// Turn the active gizmos off.
    /// </summary>
    public void DeactivateGizmos()
    {
        _activeGizmo = null;
        ////Debug.Log("Deactivate Gizmos");
        foreach (PlacerGizmo gizmo in placerGizmos)
        {
            if (gizmo == null || gizmo.gameObject == null)
                continue;

            gizmo.gameObject.SetActive(false);
        }
    }

    protected override void OnLayerChanged(LayerManager.EditorLayer newLayer)
    {
        //_usingPlacementOverride = false;
        _currentLayer = newLayer;
        //ForceDeselect();
        SetPlacerLogicFromLayer();
        //ForceDeselect();

        DeselectObject();

        //currentEditorLayer = newLayer;
    }

    void SetPlacerLogicFromLayer()
    {
        _allowGizmoTypeChange = false;

        switch (_currentLayer)
        {
            case LayerManager.EditorLayer.Mine:
                activeLogicIndex = 2;
                if (activeLogic != placerlogicChoices[activeLogicIndex])
                {
                    //ActivateManipLogic(placerlogicChoices[2]);
                    //DeactivateResizeGizmos();
                    SwitchActiveGizmos(GizmoKind.None);
                    //gizmoUIPanel.SetActive(false);
                    EnableGizmoTypeChanges(true);
                    _ventUIPanel.SetActive(false);
                    //SceneControlsUIPanel.SetActive(false);
                }
                break;

            case LayerManager.EditorLayer.Object:
                activeLogicIndex = 0;
                if (activeLogic != placerlogicChoices[activeLogicIndex])
                {
                    //ActivateManipLogic(placerlogicChoices[0]);
                    //DeactivateResizeGizmos();
                    SwitchActiveGizmos(GizmoKind.Pan);
                    //gizmoUIPanel.SetActive(true);
                    EnableGizmoTypeChanges(true);
                    _ventUIPanel.SetActive(false);
                    //SceneControlsUIPanel.SetActive(false);
                }
                break;

            case LayerManager.EditorLayer.Ventilation:
                activeLogicIndex = 3;
                if (activeLogic != placerlogicChoices[activeLogicIndex])
                {
                    DeactivateGizmos();
                    //ActivateResizeGizmos();
                    SwitchActiveGizmos(GizmoKind.Resize);
                    //ActivateManipLogic(placerlogicChoices[3]);
                    //gizmoUIPanel.SetActive(false);
                    EnableGizmoTypeChanges(true);
                    _ventUIPanel.SetActive(true);
                    //SceneControlsUIPanel.SetActive(false);
                }
                break;

            case LayerManager.EditorLayer.SceneControls:
                activeLogicIndex = 0;
                if (activeLogic != placerlogicChoices[activeLogicIndex])
                {
                    //DeactivateResizeGizmos();
                    SwitchActiveGizmos(GizmoKind.None);
                    //ActivateManipLogic(placerlogicChoices[0]);
                    //gizmoUIPanel.SetActive(true);
                    EnableGizmoTypeChanges(true);
                    _ventUIPanel.SetActive(false);
                    //SceneControlsUIPanel.SetActive(true);
                }

                break;

            case LayerManager.EditorLayer.Cables:
                activeLogicIndex = 4;
                if (activeLogic != placerlogicChoices[activeLogicIndex])
                {
                    //DeactivateResizeGizmos();
                    SwitchActiveGizmos(GizmoKind.None);
                    //ActivateManipLogic(placerlogicChoices[activeLogicIndex]);
                    //gizmoUIPanel.SetActive(false);
                    EnableGizmoTypeChanges(true);
                    _ventUIPanel.SetActive(false);
                    //SceneControlsUIPanel.SetActive(false);
                }
                break;

            default:
                //Debug.LogWarning("Editor layer \"" + _currentLayer.ToString() + "\" not recognized!");
                break;
        }
        //Debug.LogWarning("Set placer logic to layer default : " + activeLogic);
    }

    private void EnableGizmoTypeChanges(bool enable)
    {
        if (gizmoUIPanel != null)
            gizmoUIPanel.SetActive(enable);

        _allowGizmoTypeChange = enable;

    }

    protected virtual void InitializeLogic()
    {

    }

    void OnNewInputTarget(InputTargetController.InputTarget inputTarget)
    {
        inputLocked = inputTarget != InputTargetController.InputTarget.Viewport;
    }
}

public struct GizmoOrder
{
    public GizmoKind kind;
    public GizmoAxis axis;
}