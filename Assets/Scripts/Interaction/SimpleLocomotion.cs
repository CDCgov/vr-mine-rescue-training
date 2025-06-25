using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;



public class SimpleLocomotion : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public SystemManager SystemManager;
    public XRInputManager XRInputManager;

    public Transform Camera;
    //public float MovementFactor = 0.5f;
    //public float RotationFactor = 0.5f;
    public float MovementSpeed = 3;
    public float RotationSpeed = 90;
    public float JoystickDeadzone = 0.05f;
    public float MaxTeleportDist = 5;

    public bool AllowUserTeleport = true;
    public GameObject TeleportTargetReticle;
    public Transform ReticleTransform;

    public LocomotionSystem Loco;
    public SceneFadeManager SceneFadeManager;
    public TeleportationProvider TeleProvider;
    //[UnityEngine.Serialization.FormerlySerializedAs("watcher")]
    //public XRJoystickWatcher Watcher;

    //private InputDevice _LeftHandDevice;
    private Vector2 _axisVal;
    //private bool _useJoystick;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private bool _showReticle = false;
    private bool _isTeleporting = false;
    private bool _validDestination = false;
    private int _teleportMask;
    private int _teleportMaskObstructions;

    private XRInputManager.InputDeviceState _leftController;
    private XRInputManager.InputDeviceState _rightController;

    public List<RootMotion.FinalIK.RotationLimit> rotationLimits;

    private void Awake()
    {
        _teleportMask = LayerMask.GetMask("ValidTeleport", "Floor");
        _teleportMaskObstructions = LayerMask.GetMask("ValidTeleport", "Floor", "Roof", "Walls", "Default");
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (SceneFadeManager == null)
            SceneFadeManager = SceneFadeManager.GetDefault(gameObject);
        if (XRInputManager == null)
            XRInputManager = XRInputManager.GetDefault(gameObject);

        _leftController = XRInputManager.GetInputDevice(XRNode.LeftHand);
        _rightController = XRInputManager.GetInputDevice(XRNode.RightHand);

        Debug.Log("In Simple Loco");
        //watcher.primaryJoystickPress.AddListener(onPrimaryJoystickEvent);
        //watcher.secondaryJoystickPress.AddListener(onSecondaryJoystickEvent);
        //var leftHandDevices = new List<InputDevice>();

        //InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
        //if (leftHandDevices.Count == 1)
        //{
        //    _LeftHandDevice = leftHandDevices[0];
        //    //Debug.Log(string.Format("Device name '{0}' with role '{1}'", device.name, device.role.ToString()));
        //    Debug.Log("Found left hand.");
        //}
        //else if (leftHandDevices.Count > 1)
        //{
        //    Debug.Log("Found more than one left hand!");
        //}

        if (SystemManager != null && SystemManager.SystemConfig != null)
        {
            MovementSpeed = SystemManager.SystemConfig.VRMovementSpeed;
        }
        _startPos = transform.position;

        //Watcher.primaryButtonPress.AddListener(TeleportButtonPressed);
        _leftController.PrimaryButtonClicked += TeleportButtonPressed;
        _rightController.PrimaryButtonClicked += TeleportButtonPressed;
    }

    private void OnDestroy()
    {
        if (_leftController != null)
            _leftController.PrimaryButtonClicked -= TeleportButtonPressed;
        if (_rightController != null)
            _rightController.PrimaryButtonClicked -= TeleportButtonPressed;
    }

    //public bool UseJoystick
    //{
    //    get => _useJoystick;
    //    set
    //    {
    //        _useJoystick = value;
    //    }
    //}

    public void ApplyTranslation(Vector2 axis)
    {
        //deadzone
        if (axis.magnitude < JoystickDeadzone)
            return;


        //transform from camera space the VR rig's space
        Vector3 direction = new Vector3(axis.x, 0, axis.y);
        //direction = Camera.TransformDirection(direction);

        //var camRot = Camera.localRotation;
        var camRot = Camera.localRotation;
        if (Camera.parent != null)
            camRot *= Camera.parent.localRotation;

        Vector3 camEuler = camRot.eulerAngles;
        direction = Quaternion.AngleAxis(camEuler.y, Vector3.up) * direction;

        direction = transform.TransformDirection(direction);
        if (transform.parent != null)
            direction = transform.parent.InverseTransformDirection(direction);

        direction *= Time.deltaTime * MovementSpeed;
        transform.localPosition += direction;


        //if (axis.y != 0)
        //{
        //    //_axisVal.y != 0 || _axisVal.x != 0
        //    //transform.Translate(_axisVal.x* MovementFactor, 0, _axisVal.y * MovementFactor);
        //    Vector3 direction = Camera.forward;
        //    direction = transform.InverseTransformDirection(direction);

        //    direction = Vector3.ProjectOnPlane(direction, Vector3.up);
        //    //direction *= (axis.y * MovementFactor);
        //    direction *= Time.deltaTime * MovementSpeed;

        //    //transform.Translate(direction, Space.Self);
        //    transform.localPosition += direction;
        //}
        //if(axis.x != 0)
        //{
        //    Vector3 direction = Camera.right;
        //    direction = transform.InverseTransformDirection(direction);

        //    direction = Vector3.ProjectOnPlane(direction, Vector3.up);
        //    direction *= (axis.x * MovementFactor);

        //    //transform.Translate(direction, Space.Self);
        //    transform.localPosition += direction;
        //}

    }

    public void ApplyRotation(Vector2 axis)
    {
        if (Camera == null)
            return;



        if (Mathf.Abs(axis.x) >= JoystickDeadzone)
        {
            //transform.Rotate(0, axis.x * RotationFactor, 0);

            var degrees = Time.deltaTime * RotationSpeed * axis.x;
            Vector3 rotAxis = Vector3.up;
            if (transform.parent != null)
            {
                //put rotation axis in space of parent
                Debug.DrawLine(transform.position, transform.position + rotAxis * 3, Color.magenta);
                rotAxis = transform.parent.TransformDirection(rotAxis);
            }
            transform.RotateAround(Camera.transform.position, rotAxis, degrees);
        }
    }

    void Teleport(Vector3 dest)
    {
        TeleportRequest teleReq = new TeleportRequest
        {
            destinationPosition = dest,
            destinationRotation = transform.rotation,
            //destinationUpVector = Vector3.up,
            //destinationForwardVector = transform.forward
        };

        TeleProvider.QueueTeleportRequest(teleReq);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerManager == null || PlayerManager.CurrentPlayer == null || 
            _leftController == null || _rightController == null)
            return;


        if (PlayerManager.CurrentPlayer.TranslationEnabled)
            ApplyTranslation(_rightController.PrimaryAxis);
        if (PlayerManager.CurrentPlayer.RotationEnabled)
            ApplyRotation(_leftController.PrimaryAxis);

        //UPDATE PLAYER MANAGER IF THIS WORKS
        if (_showReticle)
        {
            UpdateTeleportDestination();
        }


        if (Input.GetKeyUp(KeyCode.Home))
        {
            Teleport(_startPos);
        }
    }

    void TeleportButtonPressed(bool buttonPressed)
    {
        if (_isTeleporting)
            return;

        if (buttonPressed)
        {
            if (PlayerManager.CurrentPlayer.UserTeleportEnabled)
                ShowTeleportMarker(true);
        }
        else
        {
            ShowTeleportMarker(false);
            if (_validDestination && PlayerManager.CurrentPlayer.UserTeleportEnabled)
                TeleportToTarget();
            _validDestination = false;
        }
        //TeleportCheckRoutineAsync(buttonPressed);
    }

    bool CheckSurfaceNormal(Vector3 n)
    {
        var angle = Vector3.Angle(Vector3.up, n);
        if (angle < 30.0f)
            return true;
        else
            return false;
    }

    void UpdateTeleportDestination()
    {
        RaycastHit hit;
        _validDestination = false;

        var teleportRay = new Ray(TeleportTargetReticle.transform.position, TeleportTargetReticle.transform.forward);

        if (Physics.Raycast(teleportRay, out hit, MaxTeleportDist, _teleportMask) && CheckSurfaceNormal(hit.normal))
        {
            _targetPos = hit.point;
            _validDestination = true;
            Debug.Log("Valid cast: " + _targetPos);
        }
        else
        {
            Vector3 pos;
            //attempt to move forward by the max teleport distance

            //calculate distance to obstruction if any
            var dir = teleportRay.direction;
            dir.y = 0;
            dir.Normalize();

            float obstructDist = MaxTeleportDist;
            //if (Physics.Raycast(teleportRay.origin, dir, out hit, MaxTeleportDist, _teleportMaskObstructions))
            //{
            //    //obstruction hit
            //    obstructDist = Vector3.Distance(teleportRay.origin, hit.point);
            //    if (obstructDist < 0.6f) //too close, no valid destination
            //        return;

            //    obstructDist -= 0.5f;
            //    pos = teleportRay.origin + teleportRay.direction * obstructDist;
            //}

            //search for a valid teleport target starting at the max allowed distance


            for (float dist = obstructDist; dist > 0.25f; dist -= 0.15f)
            {
                pos = teleportRay.origin + dir * dist;
                pos.y = 50;

                if (Physics.Raycast(pos, Vector3.down, out hit, 500, _teleportMask) && CheckSurfaceNormal(hit.normal))
                {
                    _targetPos = hit.point;
                    _validDestination = true;
                    break;
                }
            }

            /*
            //check if there is a raycast hit, then find the closest valid teleport point?
            if (Physics.Raycast(TeleportTargetReticle.transform.position, TeleportTargetReticle.transform.forward, out hit, 5))
            {
                Collider[] cols = Physics.OverlapSphere(hit.point, 1);
                Vector3 closePoint = Vector3.positiveInfinity;
                foreach (Collider col in cols)
                {
                    if (col.gameObject.layer == LayerMask.GetMask("ValidTeleport"))
                    {
                        closePoint = col.ClosestPoint(hit.point);
                        if (Vector3.Distance(transform.position, closePoint) < Vector3.Distance(transform.position, _targetPos))
                        {
                            _targetPos = closePoint;
                        }
                    }
                }
            }
            else if (Physics.Raycast(TeleportTargetReticle.transform.position + (TeleportTargetReticle.transform.forward * 5), -TeleportTargetReticle.transform.up, out hit, layerMask))//cast forward then look down for valid location
            {
                _targetPos = hit.point;
                _validDestination = true;
                Debug.Log("Down cast: " + _targetPos);
            }
            */
        }
        //Vector3 yCorrection = _targetPos + new Vector3(0, 0.01f, 0);

        if (_validDestination)
        {
            ReticleTransform.position = _targetPos;
            ReticleTransform.rotation = Quaternion.identity;
            TeleportTargetReticle.SetActive(true);
        }
        else
        {
            TeleportTargetReticle.SetActive(false);
        }
    }

    void ShowTeleportMarker(bool bShow)
    {
        _showReticle = bShow;
        if (!bShow)
            TeleportTargetReticle.SetActive(false);
    }

    private async void TeleportToTarget()
    {
        try
        {
            _isTeleporting = true;
            await SceneFadeManager.FadeOut(0.5f);
            //await Task.Delay(250);
            Teleport(_targetPos);
            //await Task.Delay(250);
            await SceneFadeManager.FadeIn(0.5f);
            _isTeleporting = false;
        }
        finally
        {
            _isTeleporting = false;
        }
    }

    private async void TeleportCheckRoutineAsync(bool val)
    {
        if (_isTeleporting)
        {
            return;
        }
        if (PlayerManager.CurrentPlayer.UserTeleportEnabled)
        {
            if (val)
            {
                if (!_showReticle)
                {
                    _showReticle = true;
                    TeleportTargetReticle.SetActive(true);
                }
            }
            else
            {
                if (_showReticle)
                {
                    _showReticle = false;
                    TeleportTargetReticle.SetActive(false);
                    Debug.Log("TELEPORT!");
                    _isTeleporting = true;
                    await SceneFadeManager.FadeOut(0.5f);
                    //await Task.Delay(250);
                    Teleport(_targetPos);
                    //await Task.Delay(250);
                    await SceneFadeManager.FadeIn(0.5f);
                    _isTeleporting = false;
                }
            }
        }
        return;
    }
}
