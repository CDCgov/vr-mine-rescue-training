using g3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;


public class ScenarioEditorCamera : CameraLogic, ISceneCamera
{
    private enum CameraMoveMode
    {
        None,
        Moving,
        MouseDrag,
    }

    [UnityEngine.Serialization.FormerlySerializedAs("cameraSettings")]
    public List<ScenarioEditorCameraSettingsObject> CameraSettings;
    [UnityEngine.Serialization.FormerlySerializedAs("pivot")]
    public Transform PivotTransform;
    public float SpeedMultiplier = 3;
    [Tooltip("Distance of panning plane from the camera along the forward or down axis depending on panning type")]
    public float PanPlaneDistance = 5;

    [UnityEngine.Serialization.FormerlySerializedAs("inputTargetController")]
    public InputTargetController InputTargetController;
    
    public float mouseScrollModifier = 5;
    [SerializeField] LayerMask panningLayerMask;
    //private bool _topDownPerspective = false;

    private IEnumerator centerCam;
    public SystemManager SystemManager;

    public bool OverrideViewportRect = true;
    //public bool AllowOrthographic = true;

    public Vector3 CameraPosition
    {
        get
        {
            return transform.position;
        }
    }

    public bool IsOrbitEnabled
    {
        get { return _orbitRotate; }
    }

    //private Vector3 lastMousePosition;
    //private bool _moving;
    private CameraMoveMode _moveMode;
    private float _moveSpeed = 0;
    private bool _isRotating = false;


    private const float OrbitStopDistance = 0.5f;
    private bool _orbitRotate = false;
    private Vector3 _orbitStartPos = Vector3.zero;

    private Vector3 _motionVector = Vector3.zero;
    private Vector2 _lastMousePos;
    private Plane _mouseDragPlane;

    private ScenarioEditorCameraSettingsObject _currentCameraSettings;
    //private Ray _ray;
    //private Placer _activePlacer;

    private bool _inputLocked;
    private Vector3 _diff;
    private Vector3 _start;
    private Vector3 _cameraForward;
    private float _zoomDistance;
    private float _orbitStartZoom;
    private LayerMask _mask;
    private Transform _assetContainer;

    private Vector2 _mouseAxisDelta;
    private Camera _camera;
    private Transform _cameraTransform;

    private PointerEventData _pointerData = null;
    private List<RaycastResult> _pointerRaycastResults = new List<RaycastResult>();

    //private float _currentZoom = 0;
    //private float Zoom
    //{
    //    get
    //    {
    //        if (Input.GetKey(KeyCode.LeftShift))
    //            return Input.mouseScrollDelta.y * _currentCameraSettings.zoomSpeed * mouseScrollModifier;
    //        else
    //            return Input.mouseScrollDelta.y * _currentCameraSettings.zoomSpeed;
    //    }
    //}

    void Awake()
    {
        _camera = Camera.main;
        _cameraTransform = _camera.transform;
        
        _mask = LayerMask.GetMask("PanPlane");
        _mask += LayerMask.GetMask("SelectedObject");

        ChangeCameraSettings("MineLayer");

        //adjust camera rect depending on screen resolution (default is 1920) done to make screen scale function work.
        SystemManager = SystemManager.GetDefault();

        if (OverrideViewportRect)
        {
            //Rect camDefault = _camera.rect;
            ////Debug.Log($"Setting cam relative to UI Scale: {camDefault.ToString()}, UI - {SystemManager.SystemConfig.UIScale}");
            //camDefault.x = (camDefault.x * (1920.0f / (float)Screen.width));
            //camDefault.width = (camDefault.width * ((float)Screen.width / 1920.0f));
            //_camera.rect = camDefault;
        }
    }

    // Start is called before the first frame update
    void Start()
    {


        _assetContainer = GameObject.Find("Assets")?.transform;

        Activate();

        if (InputTargetController == null)
            InputTargetController = FindObjectOfType<InputTargetController>();

        if (InputTargetController != null)
        {
            InputTargetController.onNewInputTarget += OnNewInputTarget;
            InputTargetController.SetInputTargetToViewPort();
        }

        _lastMousePos = Input.mousePosition;

    }

    private void OnDestroy()
    {
        if (InputTargetController != null)
            InputTargetController.onNewInputTarget -= OnNewInputTarget;

       
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"Pointer over UI: {Util.IsPointerOverUI}");
        if (_inputLocked || (Util.IsPointerOverUI && _moveMode != CameraMoveMode.MouseDrag && !_isRotating))
        {
            _lastMousePos = Input.mousePosition;
            return;
        }

        UpdateInput();

        //apply mouse wheel zoom even if a UI element is focused
        ApplyMouseScrollZoom(Input.mouseScrollDelta.y);

        //check if a UI element has focus
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.TryGetComponent<Selectable>(out var inputComp))
            {
                UpdateCameraPosition();
                _lastMousePos = Input.mousePosition;
                return;
            }
        }

        if (_moveMode == CameraMoveMode.MouseDrag)
            ApplyMouseDragMovement();
        else
            ApplyMotion();


        UpdateCameraPosition();
        if (Input.GetKeyDown(KeyCode.V) && ScenarioSaveLoad.Instance != null)
        {
            ZoomToFit(new Vector3(-1, -1, 1));
            //var bounds = ScenarioSaveLoad.Instance.MineBounds;
            //var extents = bounds.extents;
            //var max = Mathf.Max(extents.x, extents.y, extents.z);

            //EnableOrbit(bounds.center, max * 1.25f);
        }

        _lastMousePos = Input.mousePosition;
    }



    private bool RaycastGroundPlane(Ray ray, out Vector3 pt)
    {
        pt = Vector3.zero;

        //use zero ground plane for now
        Plane ground = new Plane(Vector3.up, Vector3.zero);

        if (Mathf.Abs(Vector3.Dot(ray.direction, ground.normal)) < 0.1f)
        {
            return false; //parallel to plane
        }

        if (!ground.Raycast(ray, out float enter))
            return false;


        pt = ray.origin + ray.direction * enter;
        return true;
    }

    private bool Raycast(Ray ray, out Vector3 pt)
    {
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, panningLayerMask))
        {
            pt = hit.point;
            return true;
        }

        pt = Vector3.zero;
        return false;
    }

    private void StartDrag()
    {
        if (CursorImageController.instance != null)
            CursorImageController.instance.ChangeCursorImage(CursorImageController.CursorImage.HandClosed);

        _moveMode = CameraMoveMode.MouseDrag;
        _lastMousePos = Input.mousePosition;

        var camDir = _camera.transform.forward;
        var angle = Vector3.Angle(camDir, Vector3.down);

        //if the camera is looking down, use flat ground plane
        if (angle < 50)
        {
            _mouseDragPlane = new Plane(Vector3.up, Vector3.zero);
        }
        else //calculate orthogonal plane for panning
        {
            Vector3 planePt;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, panningLayerMask))
            {
                planePt = hit.point;
            }
            else
            {
                planePt = ray.origin + ray.direction * 10.0f;
            }

            //Debug.Log($"ScenarioEditorCamera: Panning plane distance {dist}");

            _mouseDragPlane = new Plane(camDir, planePt);
        }
    }

    private void EndDrag()
    {
        if (CursorImageController.instance != null)
            CursorImageController.instance.ChangeCursorImage(CursorImageController.CursorImage.Arrow);

        _moveMode = CameraMoveMode.Moving;
    }

    void ApplyMouseDragMovement()
    {
        Ray oldRay = _camera.ScreenPointToRay(_lastMousePos);
        if (Mathf.Abs(Vector3.Dot(oldRay.direction, _mouseDragPlane.normal)) < 0.1f)
            return;

        //RaycastGroundPlane(oldRay, out var oldGroundPos);
        _mouseDragPlane.Raycast(oldRay, out Vector3 oldGroundPos);

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Mathf.Abs(Vector3.Dot(ray.direction, _mouseDragPlane.normal)) < 0.1f)
            return;

        //RaycastGroundPlane(ray, out var groundPos);
        _mouseDragPlane.Raycast(ray, out Vector3 groundPos);

        var delta = oldGroundPos - groundPos;
        //delta.y = 0;

        //_mouseDragStartWorldPosition = groundPos;
        PivotTransform.position += delta;

        if (Vector3.Distance(PivotTransform.position, _orbitStartPos) > OrbitStopDistance)
            DisableOrbit();
    }

    //private void UpdatePivotDrag()
    //{

    //    //if (Input.GetMouseButtonDown(2)) { CursorImageController.instance.ChangeCursorImage(CursorImageController.CursorImage.HandClosed); }
    //    //if (Input.GetMouseButton(2))
    //    //{
    //    Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
    //    RaycastHit hit;
    //    Plane plane;
    //    Vector3 planePosition;

    //    //if detecting hit on click, use hitpoint as location of movement plane
    //    if (Physics.Raycast(ray, out hit, Mathf.Infinity, panningLayerMask))
    //    {

    //        if (_isPanning == false)
    //        {
    //            _isPanning = true;
    //            _start = hit.point;
    //        }
    //        planePosition = _start;

    //        // change panning plane orientation and position based on alt key
    //        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.F)) { plane = new Plane(_cameraTransform.forward, planePosition); }
    //        else { plane = new Plane(Vector3.down, planePosition); }
    //    }
    //    //no hit detected, so set plane relative to camera at fixed distance
    //    else
    //    {
    //        // change panning plane orientation and position based on alt key
    //        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.F))
    //        {
    //            planePosition = _cameraTransform.position + (_cameraTransform.forward * PanPlaneDistance);
    //            plane = new Plane(_cameraTransform.forward, planePosition);
    //        }
    //        else
    //        {
    //            planePosition = _cameraTransform.position + (-_cameraTransform.up * PanPlaneDistance);
    //            plane = new Plane(Vector3.down, planePosition);
    //        }
    //        Debug.Log("Plane panning");
    //    }

    //    if (plane.Raycast(ray, out float enter))
    //    {
    //        _diff = ray.GetPoint(enter) - PivotTransform.position;
    //    }
    //}

    private void UpdateInput()
    {
        if (_currentCameraSettings == null)
            return;

        //float mouseX, mouseY;

        //if (Input.GetKey(KeyCode.LeftShift))
        //    _currentZoom = Input.mouseScrollDelta.y * _currentCameraSettings.zoomSpeed * mouseScrollModifier;
        //else
        //    _currentZoom = Input.mouseScrollDelta.y * _currentCameraSettings.zoomSpeed;        
        //Debug.Log("LastMouse: " + _lastMousePos);
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (_currentCameraSettings.invertYaw)
                mouseX *= -1.0f;

            if (!_currentCameraSettings.invertPitch)
                mouseY *= -1.0f;

            _mouseAxisDelta = new Vector2(mouseX, mouseY);
            _isRotating = true;

            //Vector2 mousePos = Input.mousePosition;
            //_mouseAxisDelta = (mousePos - _lastMousePos) * 0.1f;

            //if (_currentCameraSettings.invertYaw)
            //    _mouseAxisDelta.x *= -1.0f;
            //if (!_currentCameraSettings.invertPitch)
            //    _mouseAxisDelta.y *= -1.0f;
        }
        else
        {
            _mouseAxisDelta = Vector2.zero;
            _isRotating = false;
        }


        if (_cameraTransform.forward.y >= 1)
            _cameraForward = -_cameraTransform.up;
        else if (_cameraTransform.forward.y <= -1)
            _cameraForward = _cameraTransform.up;
        else
            _cameraForward = _cameraTransform.forward;


        _motionVector = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            _motionVector += _cameraForward;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            _motionVector += -_cameraForward;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            _motionVector += -_cameraTransform.right;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            _motionVector += _cameraTransform.right;

        _motionVector.y = 0;

        //if (_topDownPerspective && AllowOrthographic)
        //{
        //    if (Input.GetKey(KeyCode.C))
        //        if(_camera.orthographicSize > _currentCameraSettings.minOrthoSize)
        //            _camera.orthographicSize--;
        //        else
        //            _camera.orthographicSize = _currentCameraSettings.minOrthoSize;

        //    if (Input.GetKey(KeyCode.Space))
        //        if (_camera.orthographicSize < _currentCameraSettings.maxOrthoSize)
        //            _camera.orthographicSize++;
        //        else 
        //            _camera.orthographicSize = _currentCameraSettings.maxOrthoSize;
        //}
        //else
        //{
        //    if (Input.GetKey(KeyCode.C))
        //        _motionVector += -PivotTransform.up;
        //    if (Input.GetKey(KeyCode.Space))
        //        _motionVector += PivotTransform.up;
        //}

        if (Input.GetKey(KeyCode.C))
            _motionVector += -PivotTransform.up;
        if (Input.GetKey(KeyCode.Space))
            _motionVector += PivotTransform.up;


        //if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.F))
        //    _orbitRotate = true;
        //else
        //{
        //    _orbitRotate = false;
        //    // if not in orbit mode, allow zoom to alter plane height along camera forward axis
        //    //_motionVector += _cameraTransform.forward * _currentZoom;
        //}

        _motionVector = _motionVector.normalized;

        if (Input.GetKey(KeyCode.LeftShift))
            _moveSpeed = _currentCameraSettings.moveSpeed * SpeedMultiplier;
        else
            _moveSpeed = _currentCameraSettings.moveSpeed;

        if (Input.GetMouseButtonDown(2))
            StartDrag();
        if (!Input.GetMouseButton(2) && _moveMode == CameraMoveMode.MouseDrag)
            EndDrag();



    }

    //private void UpdateHotkeys()
    //{
    //    if (Input.GetKeyDown(KeyCode.Z))
    //    {
    //        StartCenterCamera();
    //    }

    //    if (Input.GetKeyDown(KeyCode.P))
    //    {
    //        SetCameraPerspective(false);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.T))
    //    {
    //        SetCameraPerspective(true);
    //    }
    //}

    private void ApplyMouseScrollZoom(float zoom)
    {
        if (_camera == null)
            return;

        if (Mathf.Abs(zoom) <= 0.01f)
            return;

        var viewportPos = _camera.ScreenToViewportPoint(Input.mousePosition);
        //Debug.Log(viewportPos);
        if (viewportPos.x > 1 || viewportPos.x < 0 ||
            viewportPos.y > 1 || viewportPos.y < 0)
        {
            return;
        }

        //check if the mouse is over a scroll rect
        //TODO: find or create a state for this without performing a separate raycast
        if (EventSystem.current != null && _camera != null)
        {
            if (_pointerData == null)
                _pointerData = new PointerEventData(EventSystem.current);

            _pointerRaycastResults.Clear();
            _pointerData.position = Input.mousePosition;

            EventSystem.current.RaycastAll(_pointerData, _pointerRaycastResults);

            foreach (var result in _pointerRaycastResults)
            {
                if (result.gameObject == null)
                    continue;

                //Debug.Log($"MouseRaycast: {result.gameObject.name}");

                var scrollRect = result.gameObject.GetComponentInChildren<ScrollRect>();
                if (scrollRect != null)
                    return;

                if (result.gameObject.TryGetComponent<MoveableWindow>(out var _))
                    return;

                //if (result.gameObject.TryGetComponent<ScrollRect>(out var scrollRect))
                //    return;
            }
        }

        if (_orbitRotate)
        {
            var zoomOffset = _zoomDistance * zoom * -0.15f;
            zoomOffset = Mathf.Clamp(zoomOffset, -5, 5);
            _zoomDistance += zoomOffset;
            if (_zoomDistance < 0)
                _zoomDistance = 0;

            //Debug.Log($"ScenarioEditorCamera: Mouse zoom offset {zoomOffset:F2}");
        }
        else
        {
            var dir = _camera.transform.forward;
            var origin = PivotTransform.position;
            //var ray = new Ray(origin, dir);
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            Vector3 groundPos = Vector3.zero;

            if (!RaycastGroundPlane(ray, out groundPos))
            {
                if (!Raycast(ray, out groundPos))
                {
                    return;
                }
            }

            var dist = Vector3.Distance(origin, groundPos);

            dist = dist * zoom * 0.15f;
            dist = Mathf.Clamp(dist, -5, 5);
            //Debug.Log($"ScenarioEditorCamera: Mouse zoom distance {dist:F2}");
            PivotTransform.position += dir * dist;
        }
    }


    //move the pivot using drag and whatnot
    private void ApplyMotion()
    {
        //if (_isPanning)
        //{
        //    Debug.Log($"Am I panning upon scene load?");
        //    return; // TO DO: reconcile conflicts with panning and WASD positioning at the same time
        //}

        // if not in orbit mode, allow zoom to alter plane height along camera forward axis
        //if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt) && !Input.GetKey(KeyCode.F))
        //{
        //    _motionVector += _cameraTransform.forward * _currentZoom;
        //}
        //_motionVector = _motionVector.normalized;
        //_moving = (_motionVector.sqrMagnitude > 0);
        //if (!_moving) return;


        //Vector3 motion = _motionVector * Time.unscaledDeltaTime * _currentCameraSettings.moveSpeed;
        //if (Input.GetKey(KeyCode.LeftShift)) motion = motion * SpeedMultiplier;
        //PivotTransform.position += motion;

        Vector3 motion = _motionVector * Time.unscaledDeltaTime * _moveSpeed;
        PivotTransform.position += motion;

        if (Vector3.Distance(PivotTransform.position, _orbitStartPos) > OrbitStopDistance)
            DisableOrbit();
    }

    //private void UpdateZoomDistance()
    //{
    //    if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt) || Input.GetKeyDown(KeyCode.F))
    //    {
    //        //zoomDistance = lastZoom;
    //        _zoomDistance = _currentCameraSettings.startZoom;
    //        PivotTransform.position = _cameraTransform.position + (_cameraTransform.forward * _zoomDistance);
    //    }
    //    else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.F))
    //    {

    //        //Mathf.Clamp(_zoomDistance -= _currentZoom, _currentCameraSettings.minZoom, _currentCameraSettings.maxZoom);
    //    }
    //    else if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt) || Input.GetKeyUp(KeyCode.F))
    //    {
    //        //lastZoom = zoomDistance;
    //        _zoomDistance = 0;
    //        PivotTransform.position = _cameraTransform.position;
    //    }

    //}

    public void ZoomToFit(Vector3 viewDir)
    {
        _cameraTransform.forward = viewDir.normalized;

        ZoomToFit(_assetContainer?.transform);

        //var bounds = ScenarioSaveLoad.Instance.MineBounds;
        //var extents = bounds.extents;
        //var max = Mathf.Max(extents.x, extents.y, extents.z);
        //float zoomDist = max * 1.5f;

        //EnableOrbit(bounds.center, zoomDist);
    }

    public void RotateCamera(Vector3 viewDir)
    {
        if (!_orbitRotate)
        {
            //enable orbit on the camera position projected onto the ground plane
            var pt = _cameraTransform.position;
            var zoomDist = pt.y;
            pt.y = 0;

            EnableOrbit(pt, zoomDist);
        }

        _cameraTransform.forward = viewDir.normalized;

    }

    public void ZoomToFit()
    {
        ZoomToFit(_assetContainer?.transform);
    }

    private void EnableOrbit(Vector3 orbitPos)
    {
        EnableOrbit(orbitPos, _currentCameraSettings.focusZoom);
    }

    private void EnableOrbit(Vector3 orbitPos, float zoomDist)
    {
        if (zoomDist <= 0)
            zoomDist = _currentCameraSettings.focusZoom;

        PivotTransform.transform.position = orbitPos;
        _zoomDistance = zoomDist;
        _orbitRotate = true;
        _orbitStartPos = PivotTransform.position;
    }

    private void DisableOrbit()
    {
        if (!_orbitRotate)
            return;

        var camPosition = ComputeCameraPosition();

        _orbitRotate = false;
        PivotTransform.transform.position = camPosition;//_camera.transform.position;
        _zoomDistance = 0;
    }

    public void ChangeCameraSettings(string newSettings)
    {
        foreach (ScenarioEditorCameraSettingsObject settings in CameraSettings)
        {
            if (settings.settingsName == newSettings)
            {
                _currentCameraSettings = settings;

                //Attempting to force input target to Viewport
                //if (InputTargetController == null)
                //{
                //    InputTargetController = FindObjectOfType<InputTargetController>();
                //    if (InputTargetController != null)
                //        InputTargetController.onNewInputTarget += OnNewInputTarget;
                //}

                if (InputTargetController != null)
                {
                    InputTargetController.SetInputTargetToViewPort();
                }
                else
                {
                    //Debug.Log("input target controller not spawned in the scene yet");
                    _inputLocked = false;
                }
            }
        }
    }

    public void FocusObject(Transform transform, float distance = -1)
    {
        FocusObject(transform.gameObject);
    }

    public void FocusTarget(Vector3 pos, float distance = -1)
    {
        EnableOrbit(pos, distance);
        UpdateCameraPosition();
    }

    public void PositionCamera(Vector3 pos, Quaternion rot)
    {
        if (_cameraTransform == null)
        {
            _camera = Camera.main;
            _cameraTransform = _camera.transform;
        }

        if (PivotTransform == null || _cameraTransform == null)
            return;

        //Vector3 dir = Vector3.forward;
        //dir = rot * dir;

        PivotTransform.position = pos /*+ dir * 5.0f*/;
        _cameraTransform.rotation = rot;

        _zoomDistance = 0;

        UpdateCameraPosition();
    }

    public override void FocusObject(GameObject go)
    {
        Vector3 pos;
        if (go.TryGetComponent<IScenarioEditorFocusTarget>(out var focus))
            pos = focus.GetFocusTarget();
        else
            pos = go.transform.position;

        var zoomDist = _currentCameraSettings.focusZoom;
        if (IsOrbitEnabled)
            zoomDist = _zoomDistance;

        EnableOrbit(pos, zoomDist);

        UpdateCameraPosition();
        //Debug.Log("Focus");
        //pitch = currentCameraSettings.minZoom;
        //UpdatePositioning();
    }

    //override position, rotatation, zoom distance
    void OverridePositioning(Vector3 _pivotPosition, float _yaw, float _pitch, float _zoom, bool _offsetWithZoom = false)
    {
        if (_cameraTransform == null)
            return;

        //rotate
        _cameraTransform.eulerAngles = new Vector3(_pitch, _yaw, _cameraTransform.eulerAngles.z);
        //position pivot
        PivotTransform.position = _pivotPosition;
        //position camera 
        //lastZoom = _zoom;
        if (_offsetWithZoom) _cameraTransform.position = PivotTransform.position + (_cameraTransform.forward * _zoom);
        else _cameraTransform.position = PivotTransform.position;

    }


    void UpdateCameraPosition()
    {
        _cameraTransform.position = PivotTransform.position;

        //rotate camera yaw
        _cameraTransform.Rotate(new Vector3(0, _mouseAxisDelta.x * _currentCameraSettings.yawSpeed, 0), Space.World);


        //rotate camera pitch
        var newPitch = _cameraTransform.eulerAngles.x + _mouseAxisDelta.y * _currentCameraSettings.pitchSpeed;
        float roll = 0;
        float yaw = _cameraTransform.eulerAngles.y;
        //roll = _cameraTransform.eulerAngles.z;
        //Debug.Log("NewPitch :" + newPitch);
        //float newPitch = cameraXFM.eulerAngles.x;
        //if (_topDownPerspective)
        //{
        //    _cameraTransform.eulerAngles = new Vector3(90, yaw, roll);
        //}
        
        if (newPitch < 100)
        {
            //Debug.Log("lookingDown | pitch = " + _cameraTransform.eulerAngles.x + "| yaw = " + _cameraTransform.eulerAngles.y + " roll |" + _cameraTransform.eulerAngles.z);
            //cameraXFM.localEulerAngles = new Vector3(Mathf.Clamp(cameraXFM.localEulerAngles.x, 0, currentCameraSettings.maxPitch), cameraXFM.localEulerAngles.y, cameraXFM.localEulerAngles.z);
            if (newPitch > _currentCameraSettings.maxPitch)
                _cameraTransform.eulerAngles = new Vector3(_currentCameraSettings.maxPitch, yaw, roll);
            else if (newPitch < _currentCameraSettings.minPitch)
                _cameraTransform.eulerAngles = new Vector3(_currentCameraSettings.minPitch, yaw, roll);
            else
                _cameraTransform.Rotate(new Vector3(_mouseAxisDelta.y * _currentCameraSettings.pitchSpeed, 0, 0));
        }
        else //looking up
        {
            //Debug.Log("lookingUp | pitch = " + _cameraTransform.eulerAngles.x + "| yaw = "+ _cameraTransform.eulerAngles.y + " roll |" + _cameraTransform.eulerAngles.z);
            //cameraXFM.localEulerAngles = new Vector3(Mathf.Clamp(cameraXFM.localEulerAngles.x, 360 + currentCameraSettings.minPitch, 360), cameraXFM.localEulerAngles.y, cameraXFM.localEulerAngles.z);
            if (newPitch < (360 + _currentCameraSettings.minPitch))
                _cameraTransform.eulerAngles = new Vector3(360 + _currentCameraSettings.minPitch, yaw, roll);
            else
                _cameraTransform.Rotate(new Vector3(_mouseAxisDelta.y * _currentCameraSettings.pitchSpeed, 0, 0));
        }

        //position camera
        _cameraTransform.position = ComputeCameraPosition();
    }

    private Vector3 ComputeCameraPosition()
    {
        _zoomDistance = Mathf.Clamp(_zoomDistance, 0 /*_currentCameraSettings.minZoom*/, _currentCameraSettings.maxZoom);
        return PivotTransform.position + (_cameraTransform.forward * -_zoomDistance);
    }

    public override void Activate()
    {
        //pitch = Mathf.Clamp(currentCameraSettings.startPitch, currentCameraSettings.minPitch, currentCameraSettings.maxPitch);

        //OverridePositioning(_currentCameraSettings.pivotStartPoint, _currentCameraSettings.startYaw, _currentCameraSettings.startPitch, _currentCameraSettings.startZoom);
        //isActive = true;
    }

    public override void Deactivate()
    {
        //isActive = false;
    }
    void OnNewInputTarget(InputTargetController.InputTarget inputTarget)
    {
        _inputLocked = inputTarget != InputTargetController.InputTarget.Viewport && inputTarget != InputTargetController.InputTarget.PartialViewport;// && inputTarget != InputTargetController.InputTarget.PartialViewport;
    }
    // hook up to center camera UI button

    //public void StartCenterCamera()
    //{
    //    Debug.Log("Start Center Camera");
    //    //ZoomToFit(_assetContainer.transform);
    //    IEnumerator centerCam = CenterCamera();
    //    StartCoroutine(centerCam);
    //}
    //IEnumerator CenterCamera()
    //{
    //    if (_assetContainer == null)
    //        yield break;

    //    Debug.Log("CenterCamera");


    //    /// Calculate mine center from average of all asset pieces
    //    //Vector3 mineCenter = Vector3.zero;
    //    /*
    //    foreach (Transform child in _assetContainer)
    //    {
    //        mineCenter += child.position;
    //    }
    //    mineCenter /= _assetContainer.childCount;*/

    //    //calculate bounds and center

    //    //mineCenter = GetBounds(_assetContainer).center;

    //    /// Recenter camera pivot and 
    //    //PivotTransform.position = mineCenter;

    //    ///set zoom distance
    //    ZoomToFit(_assetContainer);



    //    //_zoomDistance = _currentCameraSettings.startZoom;// zoom needs to be set according to size of mine
    //    Debug.Log("CenterCameraYield");

    //    UpdateInput();
    //    ApplyMotion();

    //    UpdateCameraPosition();

    //    yield return new WaitForSeconds(0.0f);

    //    //reset camera
    //    _zoomDistance = 0;
    //    PivotTransform.position = _cameraTransform.position;

    //    Debug.Log("CenterCameraEnd");
    //}

    //public void SetCameraPerspective(bool topDown)
    //{
    //    _topDownPerspective = topDown;
    //    _camera.orthographic = topDown && AllowOrthographic;

    //}

    private Bounds GetBounds(Transform tfm)
    {
        if (tfm == null)
        {
            Bounds b = new Bounds();
            var segments = GameObject.FindObjectsByType<MineSegment>(FindObjectsSortMode.None);
            if (segments == null || segments.Length <= 0)
                return b;

            b = segments[0].SegmentBoundsWorldSpace;

            foreach (var seg in segments)
            {
                b.Encapsulate(seg.SegmentBoundsWorldSpace);
            }

            return b;
        }

        Bounds bounds = new Bounds(tfm.position, Vector3.zero);

        Renderer[] renderers = tfm.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        return bounds;
    }

    //public void OnDrawGizmosSelected()
    //{
    //    var bounds = GetBounds(_assetContainer.transform);
    //    Gizmos.DrawCube(bounds.center, bounds.size);
    //}

    private void ZoomToFit(Transform tfm)
    {
        Bounds bounds = GetBounds(tfm);
        Vector3 boundSize = bounds.size;
        float diagonal = Mathf.Sqrt((boundSize.x * boundSize.x) + (boundSize.y * boundSize.y) + (boundSize.z * boundSize.z));
        //if (_topDownPerspective && AllowOrthographic)
        //{
        //    _camera.orthographicSize = diagonal / 2.0f;
        //    _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _currentCameraSettings.minOrthoSize, _currentCameraSettings.maxOrthoSize);
        //    PivotTransform.position = bounds.center;
        //}
        //else//perspective camera method
        //{

        //float camDistanceToBoundCenter = diagonal / 2.0f / Mathf.Tan(_camera.fieldOfView / 2.0f * Mathf.Deg2Rad);
        //_zoomDistance = camDistanceToBoundCenter + diagonal / 2.0f - (_camera.transform.position - PivotTransform.position).magnitude;

        //_zoomDistance /= 2f;
        //PivotTransform.position = bounds.center;
        //PivotTransform.position = bounds.center + (-PivotTransform.forward * camDistanceToBoundWithOffset);

        //}
        //Debug.Log($"Camera aspect: {_camera.aspect}");

        //get smallest fov (horizontal or vertical)
        float fov = _camera.fieldOfView;
        if (_camera.aspect < 1.0f)
            fov *= _camera.aspect;

        float zoomDist = (diagonal * 0.5f) / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        EnableOrbit(bounds.center, zoomDist);
        UpdateCameraPosition();

    }


}
