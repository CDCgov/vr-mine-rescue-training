using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class FlyingCameraController : MonoBehaviour
{
    public SystemManager SystemManager;

    public float MoveSpeed = 5.0f;
    public float FastMoveSpeed = 15.0f;
    public Vector3 InputLookEuler;
    public Vector3 InputMoveVector;
    public LayerMask SelectionRaycastMask;
    public bool SmoothingEnabled = false;

    public string SelectedObjectName;
    public ISelectableObject SelectedObject;

    private Transform _trackedTransform;
    private Camera _camera;

    private Vector3 _velocity = Vector3.zero;
    private Quaternion _targetLook = Quaternion.identity;
    private Quaternion _lookAdjustRot = Quaternion.identity;
    private Vector3 _lookAdjustment = Vector3.zero;


    public FlyingCameraController()
    {
    }


    public void FollowTransform(Transform trackedTransform)
    {
        if (trackedTransform == null && _trackedTransform != null)
        {
            //match input euler to the previously tracked transform's orientation
            InputLookEuler = _trackedTransform.rotation.eulerAngles;

            //verify we have no "roll" applied
            InputLookEuler.z = 0;
        }
        _trackedTransform = trackedTransform;
    }

    private void Awake()
    {
        
    }

    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        SystemManager.MainCameraChanged += () =>
        {
            _camera = SystemManager.MainCamera;
        };

        //_camera = GetComponent<Camera>();
        //_camera = Camera.main;
        _camera = SystemManager.MainCamera;
        if (_camera == null)
            _camera = GetComponent<Camera>();

        InputLookEuler = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        ProcessMotion();
    }

    private float _currentSpeed = 0;
    private Vector3 _currentDirection = Vector3.zero;

    void ProcessMotion()
    {
        if (_trackedTransform != null)
        {
            transform.position = _trackedTransform.position;
            transform.rotation = _trackedTransform.rotation;
            return;
        }
        Vector3 motion;
        float deltaTime = Time.unscaledDeltaTime;

        float maxSpeed = MoveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            maxSpeed = FastMoveSpeed;
        }

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            maxSpeed *= 0.33f;
        }

        motion = InputMoveVector * maxSpeed * deltaTime;

        Vector3 euler = transform.rotation.eulerAngles;

        //motion = transform.rotation * motion;
        motion = Quaternion.AngleAxis(euler.y, Vector3.up) * motion;

        if (SmoothingEnabled)
        {
            transform.position += motion;

            var lookEuler = InputLookEuler + _lookAdjustment;

            _targetLook = Quaternion.Euler(lookEuler);

            transform.rotation = Quaternion.Slerp(transform.rotation, _targetLook, deltaTime * 3.0f);

        }
        else
        {
            transform.position += motion;
            transform.rotation = Quaternion.Euler(InputLookEuler) * _lookAdjustRot;
        }
    }

    private void SaveCameraPosition(int index)
    {
        SavedPosition pos = new SavedPosition(transform.position, transform.rotation, MasterControl.GetResearcherLightTargetIntensity());
        SystemManager.SystemConfig.SavedPointsOfInterest[index] = pos;
    }

    private void LoadCameraPosition(int index)
    {
        SavedPosition pos;
        if (SystemManager.SystemConfig.SavedPointsOfInterest.TryGetValue(index, out pos))
        {

            var lookEuler = pos.rot.ToQuaternion().eulerAngles;
            _lookAdjustment = lookEuler - InputLookEuler;
            _lookAdjustment.z = 0;

            transform.position = pos.pos.ToVector3();
            transform.rotation = pos.rot.ToQuaternion();

            MasterControl.SetResearcherLightIntensity(pos.researcherLightLevel);
        }
    }

    public void SetLookRotation(Quaternion rot)
    {
        var lookEuler = rot.eulerAngles;
        _lookAdjustment = lookEuler - InputLookEuler;
        _lookAdjustment.z = 0;

        transform.rotation = rot;
    }


    public void ProcessCustomInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = _camera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(mouseRay, out hit, 100.0f, SelectionRaycastMask.value, QueryTriggerInteraction.Ignore))
            {
                Debug.LogFormat("Researcher Clicked {0}", hit.collider.name);
                ISelectableObject selObj;
                selObj = hit.collider.GetComponentInChildren<ISelectableObject>();
                if (selObj == null)
                {
                    selObj = hit.collider.GetComponentInParent<ISelectableObject>();
                }

                if (selObj != null)
                {
                    SelectedObject = selObj;
                    SelectedObjectName = ((Component)selObj).gameObject.name;

                    Interactable interact = hit.collider.GetComponent<Interactable>();
                    if (interact != null)
                    {
                        interact.Interact();
                    }
                }
            }
        }

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            return;				

        //if (Input.GetKey(KeyCode.C))
        if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
        {
            int startKey = (int)KeyCode.Alpha0;
            int endKey = (int)KeyCode.Alpha9;

            for (int i = startKey; i <= endKey; i++)
            {
                if (Input.GetKeyDown((KeyCode)i))
                {
                    int camNumber = i - startKey;

                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        Debug.LogFormat("Saving camera position {0}", camNumber);
                        SaveCameraPosition(camNumber);
                    }
                    else
                    {
                        Debug.LogFormat("Switching to camera position {0}", camNumber);
                        LoadCameraPosition(camNumber);
                    }
                }
            }
        }
    }
}