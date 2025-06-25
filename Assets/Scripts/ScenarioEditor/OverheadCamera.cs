using g3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OverheadCamera : CameraLogic
{
    [SerializeField]
    private Transform pivot;

    [SerializeField]
    private CameraPivotMovement pivotMovement;

    [SerializeField]
    List<OverheadCameraSettingsObject> OverheadCameraSettings;
    OverheadCameraSettingsObject currentCameraSettings;

    private bool isPanning;
    private bool isActive;

    private Vector3 diff;
    private Vector3 start;
    LayerMask mask;
    EventSystem m_EventSystem;
    GraphicRaycaster m_Raycaster;
    float cameraCurrentHeight;
    [SerializeField]private float currentXPitch;

    [SerializeField] private float moveSpeed;



    private Ray ray;
    private RaycastHit hit;
    Placer activePlacer;
    private Vector3 previousPosition;
    float mouseScrollSign; // use this to track sign change in mouse scroll input to compare against pitch Eulers
    bool moving;
    private void Start()
    {
        mask = LayerMask.GetMask("PanPlane");
        mask += LayerMask.GetMask("SelectedObject");
        m_EventSystem = FindObjectOfType<EventSystem>();
        m_Raycaster = FindObjectOfType<GraphicRaycaster>();
       // currentXPitch = 45;
        currentXPitch = Camera.main.transform.rotation.eulerAngles.x;
        activePlacer = FindObjectOfType<Placer>();
        ChangeCameraSettings("MineLayer");
        Activate();
    }

    public void ChangeCameraSettings(string newSettings)
    {
        foreach (OverheadCameraSettingsObject settings in OverheadCameraSettings)
        {
            if (settings.settingsName == newSettings)
            {
                currentCameraSettings = settings;
                cameraCurrentHeight = settings.startHeight;
                ForceMovePivot(settings.pivotStartPoint);
                SetupPositioning();
            }
        }

    }

    public override void Activate()
    {
        cameraCurrentHeight = Mathf.Clamp(currentCameraSettings.startHeight, currentCameraSettings.minZoom, currentCameraSettings.maxZoom);
        
        SetupPositioning();
        isActive = true;
    }

    public override void Deactivate()
    {
        isActive = false;
    }

    public override void FocusObject(GameObject go)
    {
        pivot.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, go.transform.position.z);
        // cameraCurrentHeight = currentCameraSettings.minZoom;
        cameraCurrentHeight = currentCameraSettings.minZoom;
        SetupPositioning();
    }

    private void ToggleOrthographic()
    {
        cameraCurrentHeight = currentCameraSettings.maxZoom;
        SetupPositioning();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!isActive) { return; }
        if (activePlacer.IsLogicBusy()) { return; }
        
        //UpdateCameraZoom();
        UpdateCameraRotate();

        if (CheckIfOverUI()) { return; }

        UpdateCameraZoom();
        MoveCamera();
        if(!moving)UpdateCameraDrag();
        CheckKeyboardShortcuts();
    }

    void CheckKeyboardShortcuts()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleOrthographic();
        }
    }

    void ForceMovePivot(Vector3 position)
    {
        pivot.transform.position = position;
    }

    void SetupPositioning()
    {
        Vector3 previousPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        Vector3 newPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        Vector3 direction = previousPosition - newPosition;

        float rotationAroundYAxis = -direction.x * 180;
        float rotationAroundXAxis = 0;

        Camera.main.transform.position = pivot.position;

        Camera.main.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
        Camera.main.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World);

        Camera.main.transform.Translate(new Vector3(0, 0, -cameraCurrentHeight));
        Vector3 camPos = Camera.main.transform.position;
        Camera.main.transform.position = new Vector3(camPos.x, cameraCurrentHeight, camPos.z);
        previousPosition = newPosition;
        if (!float.IsNaN(currentXPitch))
        {
            Camera.main.transform.localEulerAngles = new Vector3(currentXPitch, Camera.main.transform.localEulerAngles.y, Camera.main.transform.localEulerAngles.z);
        }
            //if(!PitchOutOfRange())Camera.main.transform.localEulerAngles = new Vector3(currentXPitch, Camera.main.transform.localEulerAngles.y, Camera.main.transform.localEulerAngles.z);
    }

    PointerEventData m_PointerEventData;
    private bool CheckIfOverUI()
    {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();

        m_Raycaster.Raycast(m_PointerEventData, results);
        if (results.Count <= 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void UpdateCameraDrag()
    {
        pivotMovement.UpdatePivotDrag();
        if (Input.GetMouseButton(2))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                if (hit.transform.gameObject.layer == 3)
                {
                    diff = hit.point - Camera.main.transform.position;
                }
            }

            if (isPanning == false)
            {
                isPanning = true;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                {
                    if (hit.transform.gameObject.layer == 3)
                    {
                        start = hit.point;
                    }
                }
            }
        }
        else
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector3 movement = start - diff;
            Camera.main.transform.position = new Vector3(movement.x, cameraCurrentHeight, movement.z);
        }
    }

    void UpdateCameraZoom()
    {
        float oldHeight = cameraCurrentHeight;
        cameraCurrentHeight = Mathf.Clamp(cameraCurrentHeight + -Input.mouseScrollDelta.y * currentCameraSettings.zoomSpeed, currentCameraSettings.minZoom, currentCameraSettings.maxZoom);
        pivotMovement.UpdateZoomPosition(cameraCurrentHeight > currentCameraSettings.minZoom && cameraCurrentHeight < currentCameraSettings.maxZoom);
        //float newPitch = 0;
        if (Input.mouseScrollDelta.y != 0)
        {
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, cameraCurrentHeight, Camera.main.transform.position.z);

            float lastXPitch = currentXPitch;
            float newPitch = currentCameraSettings.maxPitchX - (currentCameraSettings.maxPitchX - currentCameraSettings.minPitchX) * Mathf.Log(4, cameraCurrentHeight);


            if (float.IsNaN(newPitch) || (newPitch < currentCameraSettings.minPitchX || (newPitch > lastXPitch && Input.mouseScrollDelta.y >= 0) ))
            {
                currentXPitch = currentCameraSettings.minPitchX;
            }
            else
            {
                currentXPitch = newPitch;
            }

            
        }


        SetupPositioning();
    }

    void UpdateCameraRotate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            previousPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(1))
        {
            Vector3 newPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Vector3 direction = previousPosition - newPosition;

            float rotationAroundYAxis = -direction.x * 180;
            float rotationAroundXAxis = 0;

            Camera.main.transform.position = pivot.position;

            Camera.main.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
            Camera.main.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World);

            Camera.main.transform.Translate(new Vector3(0, 0, -cameraCurrentHeight));
            Vector3 camPos = Camera.main.transform.position;
            Camera.main.transform.position = new Vector3(camPos.x, cameraCurrentHeight, camPos.z);
            previousPosition = newPosition;
        }
        if (!float.IsNaN(currentXPitch))
        {
            Camera.main.transform.localEulerAngles = new Vector3(currentXPitch, Camera.main.transform.localEulerAngles.y, Camera.main.transform.localEulerAngles.z);

        }
    }
    //if (!PitchOutOfRange()) Camera.main.transform.localEulerAngles = new Vector3(currentXPitch, Camera.main.transform.localEulerAngles.y, Camera.main.transform.localEulerAngles.z);

    void MoveCamera()
    {
        
        Vector3 motionVec = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            motionVec += transform.forward;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            motionVec += -transform.forward;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            motionVec += -transform.right;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            motionVec += transform.right;


        motionVec.y = 0;
        /*
        if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.RightArrow))
            motionVec += Vector3.down;

        if (Input.GetKey(KeyCode.Space))
            motionVec += Vector3.up;

        moving = (motionVec.sqrMagnitude > 0);
        if (!moving) return;
         */
        pivotMovement.MovePivot(moveSpeed, transform);
       
        Vector3 motion = motionVec * Time.unscaledDeltaTime * moveSpeed;
        motion.y = 0;
        
        transform.position += motion;

    }
    bool PitchOutOfRange()
    {
        return  currentXPitch <= 0;
    }
}