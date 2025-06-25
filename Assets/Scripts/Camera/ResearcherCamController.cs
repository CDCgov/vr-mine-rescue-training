using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class ResearcherCamController : MonoBehaviour, IInputTarget
{
    public SystemManager SystemManager;
    public MocapManager MocapManager;

    public float MoveSpeed = 5.0f;
    public float FastMoveSpeed = 15.0f;
    public Vector3 InputLookEuler;
    public Vector3 InputMoveVector;
    public LayerMask SelectionRaycastMask;
    public bool SmoothingEnabled = true;
    public bool DebriefMode = false;
    public GameObject AxisOverlay;
    public GameObject ResearcherCapLamp;

    public string SelectedObjectName;
    public ISelectableObject SelectedObject;

    private InputTargetOptions _inputOptions;

    private Transform _trackedTransform;
    private Camera _camera;

    private Vector3 _velocity = Vector3.zero;
    private Quaternion _targetLook = Quaternion.identity;
    private Quaternion _lookAdjustRot = Quaternion.identity;
    private Vector3 _lookAdjustment = Vector3.zero;

    //private int _raycastMask;

    private Dictionary<string, Transform> _mocapLights;


    public ResearcherCamController()
    {
        _inputOptions = new InputTargetOptions();
        _inputOptions.ToggleMouseCapture = true;
    }

    public InputTargetOptions GetInputTargetOptions()
    {
        return _inputOptions;
    }

    public Vector3 GetLookEuler()
    {
        return InputLookEuler;
    }

    public Vector3 GetMovementVector()
    {
        return InputMoveVector;
    }

    public void SetLookEuler(Vector3 eulerAngles)
    {
        InputLookEuler = eulerAngles;
    }

    public void SetMovementVector(Vector3 moveVector)
    {
        InputMoveVector = moveVector;
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
        _mocapLights = new Dictionary<string, Transform>();
    }

    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (MocapManager == null)
            MocapManager = MocapManager.GetDefault();

        SystemManager.MainCameraChanged += () =>
        {
            _camera = SystemManager.MainCamera;
            if (DebriefMode)
            {
                _camera.targetDisplay = 1;
            }
        };

        //_camera = GetComponent<Camera>();
        //_camera = Camera.main;
        _camera = SystemManager.MainCamera;
        if (_camera == null)
            _camera = GetComponent<Camera>();
        if (_camera == null)
            _camera = Camera.main;

        if (DebriefMode && _camera != null)
        {
            _camera.targetDisplay = 1;
        }
        InputLookEuler = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        UpdateMocapLights();
        ProcessMotion();
    }

    private float _currentSpeed = 0;
    private Vector3 _currentDirection = Vector3.zero;

    void ProcessMotion()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            UnityEngine.UI.InputField inputField;
            TMPro.TMP_InputField tmpInputField;

            if (EventSystem.current.currentSelectedGameObject.TryGetComponent<UnityEngine.UI.InputField>(out inputField) ||
                EventSystem.current.currentSelectedGameObject.TryGetComponent<TMPro.TMP_InputField>(out tmpInputField))
            {
                //don't move camera when entering text in an input field
                return;
            }
        }

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
            /*
            float targetSpeed = motion.magnitude;
            float curSpeed = _velocity.magnitude;

            if (targetSpeed > 0 && curSpeed > 0)
            {
                
            }

            //_velocity = Vector3.RotateTowards(_velocity, motion, Time.deltaTime * 1.0f, Time.deltaTime * 1.0f);
            _velocity += motion * 0.150f;
            
            curSpeed = _velocity.magnitude;
            if (curSpeed > maxSpeed * Time.deltaTime)
            {
                _velocity = _velocity.normalized * maxSpeed * Time.deltaTime;
            }
            else
            {
                curSpeed = curSpeed * 0.99f;
                if (curSpeed < 0)
                    curSpeed = 0;

                _velocity = _velocity.normalized * curSpeed;
            }*/


            /*motion = motion.normalized * maxSpeed;
            _velocity = Vector3.RotateTowards(_velocity, motion, deltaTime * 1.0f, deltaTime * 10.0f);
            
            transform.position += _velocity * deltaTime;
            */

            // if (InputMoveVector.magnitude > 0.05)
            // {
            // 	_currentSpeed += 8 * Time.deltaTime;
            // 	_currentSpeed = Mathf.Clamp(_currentSpeed, 0, maxSpeed);

            // 	motion = motion.normalized;
            // 	transform.position += _currentSpeed * Time.deltaTime * motion;
            // 	_currentDirection = motion;
            // }
            // else
            // {
            // 	_currentSpeed -= 12 * Time.deltaTime;
            // 	_currentSpeed = Mathf.Clamp(_currentSpeed, 0, FastMoveSpeed);

            // 	transform.position += _currentSpeed * Time.deltaTime * _currentDirection;
            // }

            transform.position += motion;

            //transform.position += (_currentSpeed * Time.deltaTime) * transform.forward;

            //_targetLook = Quaternion.Euler(InputLookEuler) * _lookAdjustRot;
            ///_targetLook =  _lookAdjustRot * Quaternion.Euler(InputLookEuler);

            var lookEuler = InputLookEuler + _lookAdjustment;
            //lookEuler.x = Mathf.Clamp(lookEuler.x, -90, 90);
            
            _targetLook = Quaternion.Euler(lookEuler);

            transform.rotation = Quaternion.Slerp(transform.rotation, _targetLook, deltaTime * 3.0f);

        }
        else
        {
            transform.position += motion;
            transform.rotation = Quaternion.Euler(InputLookEuler) * _lookAdjustRot;
        }



        //transform.rotation = Quaternion.AngleAxis(InputLookEuler.y, Vector3.up);
        //HeadTransform.localRotation = Quaternion.AngleAxis(InputLookEuler.x, Vector3.left);
    }

    private void UpdateMocapLights()
    {
        if (SystemManager.SystemConfig.WandConfiguration == null ||
            SystemManager.SystemConfig.WandConfiguration.Count <= 0)
        {
            return;
        }

        foreach (var wandConfig in SystemManager.SystemConfig.WandConfiguration)
        {
            Vector3 pos;
            Quaternion rot;
            if (MocapManager.GetSegmentPos(wandConfig.SegmentName, out pos, out rot))
            {
                UpdateMocapLight(wandConfig.SegmentName, pos, rot);
            }
        }
    }

    private void UpdateMocapLight(string segName, Vector3 pos, Quaternion rot)
    {
        Transform mocapLight = null;
        _mocapLights.TryGetValue(segName, out mocapLight);

        if (mocapLight == null)
        {
            var mocapLightObj = Instantiate<GameObject>(ResearcherCapLamp);
            mocapLightObj.SetActive(true);
            mocapLightObj.name = segName;
            mocapLight = mocapLightObj.transform;	

            mocapLight.SetParent(transform, false);
            _mocapLights[segName] = mocapLight;
        }

        pos += SystemManager.SystemConfig.MocapPositionOffset.ToVector3();

        //pos = transform.TransformDirection(pos);
        
        //rot = rot * transform.rotation;

        mocapLight.localPosition = pos;
        mocapLight.localRotation = rot;

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
            // _lookAdjustRot = Quaternion.Inverse(Quaternion.Euler(InputLookEuler)) * pos.rot.ToQuaternion();
            // Vector3 adjustEuler = _lookAdjustRot.eulerAngles;
            // adjustEuler.z = 0; //eliminate any twisting
            // _lookAdjustRot = Quaternion.Euler(adjustEuler);
            
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


    private HangingCable _cable;

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

                if (hit.rigidbody != null)
                {
                    var ventObj = hit.rigidbody.gameObject.GetComponent<VentUIObj>();
                    if (ventObj != null)
                    {
                        VentilationManager.GetDefault(gameObject).SelectedVentUIObj = ventObj;
                    }
                }

                ISelectableObject selObj;
                selObj = hit.collider.GetComponentInChildren<ISelectableObject>();
                if (selObj == null)
                {
                    selObj = hit.collider.GetComponentInParent<ISelectableObject>();
                }
                if (selObj == null && hit.rigidbody != null)
                {
                    selObj = hit.rigidbody.GetComponentInChildren<ISelectableObject>();
                }

                if (selObj != null)
                {
                    Debug.Log($"Selected object {selObj.GetObjectDisplayName()}");
                    SelectedObject = selObj;
                    SelectedObjectName = ((Component)selObj).gameObject.name;

                    Interactable interact = hit.collider.GetComponent<Interactable>();
                    if(interact != null)
                    {
                        interact.Interact();
                    }
                }
            }
        }

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Ray mouseRay = _camera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            int mask = LayerMask.GetMask("Floor", "Roof", "Walls");

            if (Physics.Raycast(mouseRay, out hit, 100.0f, mask, QueryTriggerInteraction.Ignore))
            {
                Debug.LogFormat("Researcher Clicked {0}", hit.collider.name);

                if (_cable == null)
                {
                    GameObject go = new GameObject();
                    go.name = "HangingCableTest";

                    MeshFilter mf = go.AddComponent<MeshFilter>();
                    MeshRenderer mr = go.AddComponent<MeshRenderer>();

                    _cable = go.AddComponent<HangingCable>();
                    _cable.GeneratedMesh = new Mesh();
                    mf.sharedMesh = _cable.GeneratedMesh;

                }

                _cable.AppendNode(hit.point, true);
                _cable.RegenerateMesh();
            }
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            var audioAlert = GetComponent<ProxAudioAlert>();
            var proxMWC = GetComponent<ProxMWC>();

            if (audioAlert != null && proxMWC != null)
            {
                if (proxMWC.ProxSystemID == 0)
                {
                    proxMWC.ProxSystemID = 100;
                    audioAlert.SetProxZone(ProxZone.GreenZone);
                }
                else
                {
                    proxMWC.ProxSystemID = 0;
                }
            }

        }

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



                    // var camParent = GameObject.Find("SceneHighlightCams");
                    // if (camParent == null)
                    // 	break;

                    // Cinemachine.CinemachineVirtualCamera vcam;

                    // //deactivate all cams
                    // foreach (Transform child in camParent.transform)
                    // {
                    // 	vcam = child.GetComponent<Cinemachine.CinemachineVirtualCamera>();
                    // 	if (vcam.enabled)
                    // 	{
                    // 		//copy position to research cam position
                    // 		transform.position = Camera.main.transform.position;
                    // 		transform.rotation = Camera.main.transform.rotation;
                    // 	}
                    // 	vcam.enabled = false;
                    // }

                    // if (camNumber <= 0)
                    // 	break;
                    // camNumber--;

                    // if (camNumber >= camParent.transform.childCount)
                    // 	break;

                    // vcam = camParent.transform.GetChild(camNumber).GetComponent<Cinemachine.CinemachineVirtualCamera>();
                    // vcam.enabled = true;
                    // break;
                }
            }
        }
    }
}