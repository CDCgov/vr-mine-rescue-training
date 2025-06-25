using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public enum HandType
{
    Navigator,
    Interactor,
    Grabber,
    Spawner,
    Flamethrower,
    Action
}

public enum ControllerHand
{
    LeftHand,
    RightHand
}

/*

//[RequireComponent(typeof(SteamVR_TrackedController))]
public class HandControl : MonoBehaviour
{

    public ControllerHand Hand = ControllerHand.LeftHand;
    public SteamVR_TrackedController VrController;
    public SteamVR_Controller.Device ControllerDevice;
    public SteamVR_TrackedObject TrackedObj;
    //public LineRenderer L_Ren;
    public Transform PlayerTransform;
    public Transform HeadTransform;
    public GameObject ObjectSpawnPrefab;

    //public GameObject TeleportAvatar;
    //public GameObject RenderSphere;
    //public GameObject HandUIObj;
    //public GameObject GasMeterDemo;
    //public Text UiLabel;

    //public Light GlowHand;  

    public HandType Type = HandType.Interactor;
    //public ViveWandDemo Wand;
    public GameObject LaserPrefab;
    public GameObject DemoObjectPrefab;
    public float DemoObjectScale = 0.05f;
    public Vector3 DemoObjectOffset;
    public GameObject WandModel;
    public GameObject Flamethrower;
    public AudioSource TeleportSoundPlay;
    public VoiceMenu VoxMenu;
    public bool ForceNavigationToGroundPlane = false;
    public bool ShowHelp = true;
    public GameObject HelpInfo;
    public TMPro.TextMeshPro OverviewText;
    public TMPro.TextMeshPro TriggerText;
    public GameObject OverviewLight;
    public LineRenderer[] LRenderers;

    public HandControl OtherHand;

    private float[] _lineRendererStartingScales;


    private bool _triggerPressedLastFrame = false;
    private bool _menuPressedLastFrame = false;
    private bool _gasMeterEnabled = false;
    private Rigidbody pickedUpObjRb;
    private Pickupable _grabbedItem;
    private bool _throwing = false;
    private Rigidbody _controllerRB;
    private SteamVR_PlayArea _playArea;
    private LineRenderer _laser;
    private GameObject _demoObj;
    private bool _laserOn = false;
    private bool DisableLaser = false;
    private Vector3 _laserEndPoint;
    private bool _laserHit = false;
    private RaycastHit hit;
    private bool _gripped = false;
    private bool _triggerPressed = false;
    private int _groundLayer = -1;
    private int _navLayerMask = 0;
    private bool _helpIsShown = false;

    [SerializeField]
    private FixedJoint fJoint;
    [SerializeField]
    private GameObject grabbedObject;
    [SerializeField]
    private GameObject highlightedObject;

    //private void Awake()
    //{
    //    _lineRendererStartingScales = new float[LRenderers.Length];
    //    for (int i = 0; i < LRenderers.Length; i++)
    //    {
    //        _lineRendererStartingScales[i] = LRenderers[i].startWidth;
    //    }
    //}

    // Use this for initialization
    void Start()
    {
        _lineRendererStartingScales = new float[LRenderers.Length];
        for (int i = 0; i < LRenderers.Length; i++)
        {
            _lineRendererStartingScales[i] = LRenderers[i].startWidth;
        }
        if (VrController == null)
        {
            VrController = GetComponent<SteamVR_TrackedController>();
        }
        if (TrackedObj == null)
        {
            TrackedObj = GetComponent<SteamVR_TrackedObject>();
        }
        //if(L_Ren == null)
        //{
        //    L_Ren = GetComponent<LineRenderer>();
        //}
        Debug.Log("index: " + (int)TrackedObj.index);
        if ((int)TrackedObj.index > -1)
        {
            ControllerDevice = SteamVR_Controller.Input((int)TrackedObj.index);
        }
        
        //if (TeleportAvatar == null)
        //{
        //    TeleportAvatar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    var collider = TeleportAvatar.GetComponentInChildren<Collider>();
        //    if (collider != null)
        //        collider.enabled = false;
        //}
        fJoint = GetComponent<FixedJoint>();
        _controllerRB = GetComponent<Rigidbody>();


        _playArea = transform.GetComponentInParent<SteamVR_PlayArea>();
        VrController = GetComponent<SteamVR_TrackedController>();

        var laserObj = GameObject.Instantiate<GameObject>(LaserPrefab);
        laserObj.transform.SetParent(transform, false);
        laserObj.transform.localPosition = Vector3.zero;
        laserObj.transform.localRotation = Quaternion.identity;
        _laser = laserObj.GetComponent<LineRenderer>();
        _laser.gameObject.SetActive(false);

        LRenderers[0] = _laser;
        _lineRendererStartingScales[0] = _laser.startWidth;

        //_wand.MenuButtonClicked += OnMenuButton;
        //_wand.TriggerClicked += OnTriggerClicked;
        //_wand.TriggerUnclicked += OnTriggerUnclicked;
        //_wand.PadClicked += OnPadClicked;

        _demoObj = GameObject.Instantiate<GameObject>(DemoObjectPrefab);
        _demoObj.transform.SetParent(transform);
        _demoObj.transform.localPosition = Vector3.zero;
        _demoObj.transform.localRotation = Quaternion.identity;
        _demoObj.SetActive(false);

        _groundLayer = LayerMask.NameToLayer("Floor");
        _navLayerMask = LayerMask.GetMask("Floor", "LabelsAndInformation");

        
        

        EnableLaser(true);
    }

    void NavigateToTarget(Vector3 dir)
    {
        //RaycastHit hit;
        Vector3 endPoint;

        FollowTransform ft = transform.parent.GetComponent<FollowTransform>();

        if (_laserHit)
        {
            endPoint = hit.point;
            Vector3 hitPos = hit.point;


            RaycastHit groundHit;
            Vector3 groundedHitPos = hit.point;
            groundedHitPos += hit.normal * 1.5f;
            if (Physics.Raycast(groundedHitPos, Vector3.down, out groundHit, 20.0f, _navLayerMask))
            {
                //groundedHitPt.y = 0;
                hitPos = groundHit.point;
            }

            //offset by play space position
            hitPos.x = hitPos.x - HeadTransform.localPosition.x;
            hitPos.z = hitPos.z - HeadTransform.localPosition.z;

            if (ForceNavigationToGroundPlane)
                hitPos.y = 0;

            if (VoxMenu != null)
                VoxMenu.SetTeleportReturnPoint();

            var poi = hit.collider.GetComponent<PointOfInterest>();
            if (poi != null && ft != null)
            {
                ft.Target = poi.transform;
                ft.ResetToPOIOnTargetLost = true;

                var vrpoi = poi as VRPointOfInterest;
                if (vrpoi != null)
                {   
                    transform.parent.localScale = new Vector3(vrpoi.WorldScaleMultiplier, vrpoi.WorldScaleMultiplier, vrpoi.WorldScaleMultiplier);
                    //for (int i = 0; i < LRenderers.Length; i++)
                    //{
                    //    LRenderers[i].startWidth = _lineRendererStartingScales[i] * vrpoi.WorldScaleMultiplier;
                    //    LRenderers[i].endWidth = _lineRendererStartingScales[i] * vrpoi.WorldScaleMultiplier;
                    //}
                    SetLineRendererScales(vrpoi.WorldScaleMultiplier);
                    OtherHand.SetLineRendererScales(vrpoi.WorldScaleMultiplier);
                    if (OverviewLight != null)
                    {
                        OverviewLight.SetActive(false);
                    }
                }
                else
                {
                    if (transform.parent.localScale != Vector3.one)
                    {
                        //for (int i = 0; i < LRenderers.Length; i++)
                        //{
                        //    LRenderers[i].startWidth = _lineRendererStartingScales[i];
                        //    LRenderers[i].endWidth = _lineRendererStartingScales[i];
                        //}
                        SetLineRendererScales(vrpoi.WorldScaleMultiplier);
                        OtherHand.SetLineRendererScales(vrpoi.WorldScaleMultiplier);
                    }
                    transform.parent.localScale = Vector3.one;
                    if(OverviewLight != null)
                    {
                        OverviewLight.SetActive(false);
                    }
                }
            }
            else
            {
                if (ft != null)
                {
                    ft.ResetToPOIOnTargetLost = false;
                    ft.Target = null;
                }

                // if (hit.transform.gameObject.layer == _groundLayer)
                // 	transform.parent.position = hitPos;
                // else
                // 	transform.parent.position = hitPos;
                transform.parent.position = hitPos;


                //if (transform.parent.localScale != Vector3.one)
                //{
                //    for (int i = 0; i < PlayerTransform.transform.childCount; i++)
                //    {
                //        LineRenderer lren = PlayerTransform.transform.GetChild(i).GetComponentInChildren<LineRenderer>();
                //        lren.startWidth = 0.05f;
                //        lren.endWidth = 0.05f;
                //    }
                //}
                if (transform.parent.localScale != Vector3.one)
                {
                    //for (int i = 0; i < LRenderers.Length; i++)
                    //{
                    //    LRenderers[i].startWidth = _lineRendererStartingScales[i];
                    //    LRenderers[i].endWidth = _lineRendererStartingScales[i];
                    //}
                    SetLineRendererScales(1);
                    OtherHand.SetLineRendererScales(1);
                }
                transform.parent.localScale = Vector3.one;
                if (OverviewLight != null)
                {
                    OverviewLight.SetActive(false);
                }
            }

            if (TeleportSoundPlay != null)
            {
                TeleportSoundPlay.Play();
            }
        }
        else
        {
            //check if pointing up
            if (Vector3.Angle(Vector3.up, dir) < 30)
            {
                transform.parent.position += new Vector3(0, 6, 0);
                transform.parent.localScale = Vector3.one;
                if (transform.parent.localScale != Vector3.one)
                {
                    //for (int i = 0; i < LRenderers.Length; i++)
                    //{
                    //    LRenderers[i].startWidth = _lineRendererStartingScales[i];
                    //    LRenderers[i].endWidth = _lineRendererStartingScales[i];
                    //}
                    SetLineRendererScales(1);
                    OtherHand.SetLineRendererScales(1);
                }
            }
        }
    }

    void RaycastForNav(Vector3 dir)
    {
        if (Physics.Raycast(transform.position, dir, out hit, 2000.0f, _navLayerMask))
        {
            float dist = Vector3.Distance(hit.point, transform.position);
            float scale = PlayerTransform.localScale.x;
            //_laser.SetPosition(1, new Vector3(0, 0, dist));
            _laser.SetPosition(1, _laser.transform.InverseTransformPoint(hit.point));
            _laserEndPoint = hit.point;
            _laserHit = true;
        }
        else
        {
            _laser.SetPosition(1, new Vector3(0, 0, 5));
            _laserEndPoint = transform.position + new Vector3(0, 0, 5);
            _laserHit = false;
        }
    }

    void RaycastForInteraction(Vector3 dir)
    {
        if (Physics.Raycast(transform.position, dir, out hit))
        {
            float dist = Vector3.Distance(hit.point, transform.position);
            _laser.SetPosition(1, _laser.transform.InverseTransformPoint(hit.point));
            _laserEndPoint = hit.point;
            _laserHit = true;
        }
        else
        {
            _laser.SetPosition(1, new Vector3(0, 0, 5));
            _laserEndPoint = transform.position + new Vector3(0, 0, 5);
            _laserHit = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Check if hand is close to your head, toggles on information
        if (ShowHelp && grabbedObject == null)
        {
            if(Vector3.Distance(HeadTransform.position, transform.position) < (0.3f * PlayerTransform.localScale.x))
            {
                if (!_helpIsShown)
                {
                    HelpInfo.SetActive(true);
                    _helpIsShown = true;
                }
            }
            else
            {
                if (_helpIsShown)
                {
                    HelpInfo.SetActive(false);
                    _helpIsShown = false;
                }
            }
        }
        else
        {
            if (_helpIsShown)
            {
                HelpInfo.SetActive(false);
                _helpIsShown = false;
            }
        }

        Vector3 dir = transform.forward;
        if (_laserOn)
        {
            if (Type == HandType.Navigator)
                RaycastForNav(dir);
            else
                RaycastForInteraction(dir);
        }

        if (Hand == ControllerHand.LeftHand)
        {
            if (Input.GetButtonDown("LeftVRTrackpad") && !DisableLaser)
            {
                //if (!_gripped)
                //{
                //    EnableLaser(!_laserOn);
                //    _gripped = true;
                //}
            }
            else
            {
                //_gripped = false;
            }

            if (Input.GetAxis("LeftVRTrigger") == 1)
            {
                if (!_triggerPressed)
                {
                    _triggerPressed = true;
                    switch (Type)
                    {
                        case HandType.Navigator:
                            if (_laserOn)
                            {
                                NavigateToTarget(dir);
                            }
                            break;
                        case HandType.Interactor:

                            if (_laserOn && _laserHit)
                            {
                                Interactable inter = hit.collider.GetComponent<Interactable>();
                                if (inter != null)
                                {
                                    inter.Interact();
                                    Debug.Log("Hit it!!");
                                }
                            }
                            break;
                        case HandType.Grabber:
                            if (highlightedObject != null)
                            {
                                grabbedObject = highlightedObject;
                            }                            
                            else
                            {
                                if (_laserOn && _laserHit)
                                {
                                    if (hit.collider.tag == "Pickupable")
                                    {
                                        grabbedObject = hit.collider.gameObject;
                                        grabbedObject.transform.position = transform.position + new Vector3(0, 0, 0.1f); //Potentially adjust or remove this
                                    }
                                }
                            }
                            if (grabbedObject != null)
                            {
                                fJoint.connectedBody = grabbedObject.GetComponent<Rigidbody>();
                                pickedUpObjRb = grabbedObject.GetComponent<Rigidbody>();
                                _throwing = false;
                                GrabBehavior gb = grabbedObject.GetComponent<GrabBehavior>();
                                if (gb != null)
                                {
                                    gb.Grabbed();
                                }
                                if (_laserOn)
                                {
                                    EnableLaser(false);
                                    //DisableLaser = true;
                                }
                            }
                            break;
                        case HandType.Action:
                            if (_laserOn && _laserHit)
                            {
                                Interactable inter = hit.collider.GetComponent<Interactable>();
                                if (inter != null)
                                {
                                    inter.Interact();
                                    Debug.Log("Hit it!!");
                                    return;
                                }
                            }
                            if (highlightedObject != null)
                            {
                                grabbedObject = highlightedObject;
                            }
                            else
                            {
                                if (_laserOn && _laserHit)
                                {
                                    if (hit.collider.tag == "Pickupable")
                                    {
                                        grabbedObject = hit.collider.gameObject;
                                        grabbedObject.transform.position = transform.position;
                                    }
                                }
                            }
                            if (grabbedObject != null)
                            {
                                fJoint.connectedBody = grabbedObject.GetComponent<Rigidbody>();
                                pickedUpObjRb = grabbedObject.GetComponent<Rigidbody>();
                                _throwing = false;
                                GrabBehavior gb = grabbedObject.GetComponent<GrabBehavior>();
                                if (gb != null)
                                {
                                    gb.Grabbed();
                                }
                                if (_laserOn)
                                {
                                    EnableLaser(false);
                                    DisableLaser = true;
                                }
                            }
                            break;
                        case HandType.Flamethrower:
                            Flamethrower.SetActive(true);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (Input.GetAxis("LeftVRTrigger") < 1)
            {
                _triggerPressed = false;
                if (fJoint.connectedBody != null)
                {
                    DropItem();
                }
                if (Type == HandType.Flamethrower)
                {
                    Flamethrower.SetActive(false);
                }
            }
        }

        if (Hand == ControllerHand.RightHand)
        {
            if (Input.GetButtonDown("RightVRTrackpad") && !DisableLaser)
            {
                //if (!_gripped)
                //{
                //    EnableLaser(!_laserOn);
                //    _gripped = true;
                //}
            }
            else
            {
                _gripped = false;
            }

            //if (Input.GetButtonDown("RightVRTrackpad"))
            //{
            //    ShowDemoObject(!_demoObj.GetActive());
            //}

            if (Input.GetAxis("RightVRTrigger") == 1)
            {
                if (!_triggerPressed)
                {
                    _triggerPressed = true;
                    switch (Type)
                    {
                        case HandType.Navigator:
                            if (_laserOn)
                            {
                                NavigateToTarget(dir);
                            }
                            break;
                        case HandType.Interactor:
                            if (_laserOn && _laserHit)
                            {
                                Interactable inter = hit.collider.GetComponent<Interactable>();
                                if (inter != null)
                                {
                                    inter.Interact();
                                    Debug.Log("Hit it!!");
                                }
                            }
                            break;
                        case HandType.Grabber:
                            if (highlightedObject != null)
                            {
                                grabbedObject = highlightedObject;
                            }                            
                            else
                            {
                                if (_laserOn && _laserHit)
                                {
                                    if (hit.collider.tag == "Pickupable")
                                    {
                                        grabbedObject = hit.collider.gameObject;
                                        grabbedObject.transform.position = transform.position;
                                    }
                                }
                            }
                            if (grabbedObject != null)
                            {
                                fJoint.connectedBody = grabbedObject.GetComponent<Rigidbody>();
                                pickedUpObjRb = grabbedObject.GetComponent<Rigidbody>();
                                _throwing = false;
                                GrabBehavior gb = grabbedObject.GetComponent<GrabBehavior>();
                                if (gb != null)
                                {
                                    gb.Grabbed();
                                }
                                if (_laserOn)
                                {
                                    EnableLaser(false);
                                    DisableLaser = true;
                                }
                            }
                            break;
                        case HandType.Action:
                            if (_laserOn && _laserHit)
                            {
                                Interactable inter = hit.collider.GetComponent<Interactable>();
                                if (inter != null)
                                {
                                    inter.Interact();
                                    Debug.Log("Hit it!!");
                                    return;
                                }
                            }
                            if (highlightedObject != null)
                            {
                                grabbedObject = highlightedObject;
                            }
                            else
                            {
                                if (_laserOn && _laserHit)
                                {
                                    if (hit.collider.tag == "Pickupable")
                                    {
                                        grabbedObject = hit.collider.gameObject;
                                        grabbedObject.transform.position = transform.position;
                                    }
                                }
                            }
                            if (grabbedObject != null)
                            {
                                fJoint.connectedBody = grabbedObject.GetComponent<Rigidbody>();
                                pickedUpObjRb = grabbedObject.GetComponent<Rigidbody>();
                                _throwing = false;
                                GrabBehavior gb = grabbedObject.GetComponent<GrabBehavior>();
                                if (gb != null)
                                {
                                    gb.Grabbed();
                                }
                                if (_laserOn)
                                {
                                    EnableLaser(false);
                                    DisableLaser = true;
                                }
                            }
                            break;
                        case HandType.Flamethrower:
                            Flamethrower.SetActive(true);
                            break;
                        case HandType.Spawner:
                            if (ObjectSpawnPrefab != null && _laserOn && _laserHit)
                            {
                                var newObj = Instantiate(ObjectSpawnPrefab);
                                newObj.transform.position = hit.point;
                                newObj.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            if (Input.GetAxis("RightVRTrigger") < 1)
            {
                _triggerPressed = false;
                if (fJoint.connectedBody != null)
                {
                    DropItem();
                }
                if (Type == HandType.Flamethrower)
                {
                    Flamethrower.SetActive(false);
                }
            }
        }

        if (Type != HandType.Flamethrower)
        {
            Flamethrower.SetActive(false);
        }

    }

    private void FixedUpdate()
    {
        if (_throwing)
        {
            if ((int)TrackedObj.index > -1)
            {
                if (SteamVR_Controller.Input((int)TrackedObj.index) != null)
                {
                    ControllerDevice = SteamVR_Controller.Input((int)TrackedObj.index);
                }
            }
            Transform origin;
            if (TrackedObj.origin != null)
            {
                origin = TrackedObj.origin;
            }
            else
            {
                origin = TrackedObj.transform.parent;
            }

            if (origin != null)//do the throw in local coordinate space if it exists
            {
                pickedUpObjRb.velocity = origin.TransformVector(ControllerDevice.velocity * 2);
                pickedUpObjRb.angularVelocity = origin.TransformVector(ControllerDevice.angularVelocity * 0.25f);//damping the angular velocity to reduce spin effects due to hand motion
            }
            else
            {
                pickedUpObjRb.velocity = ControllerDevice.velocity;
                pickedUpObjRb.angularVelocity = ControllerDevice.angularVelocity * 0.25f;//damping the angular velocity to reduce the spin effects due to hand motion
            }
            pickedUpObjRb.maxAngularVelocity = pickedUpObjRb.angularVelocity.magnitude;
            
            _throwing = false;
            FinishDropItem();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Pickupable")
        {
            highlightedObject = other.gameObject;
            //Debug.Log(highlightedObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        highlightedObject = null;
    }

    void DropItem()
    {
        fJoint.connectedBody = null;
        _throwing = true;
        EnableLaser(true);
    }
    void FinishDropItem()
    {
        //fJoint = null;
        GrabBehavior gb = grabbedObject.GetComponent<GrabBehavior>();
        if (gb != null)
        {
            gb.Released();
        }
        pickedUpObjRb = null;
        grabbedObject = null;
        DisableLaser = false;
    }

    private void EnableLaser(bool bEnable)
    {
        _laserOn = bEnable;
        _laser.gameObject.SetActive(bEnable);
    }

    private void ShowDemoObject(bool bShow)
    {
        if (bShow)
        {
            _demoObj.transform.localScale = new Vector3(DemoObjectScale, DemoObjectScale, DemoObjectScale);
            _demoObj.transform.localPosition = DemoObjectOffset;
        }

        WandModel.SetActive(!bShow);
        _demoObj.SetActive(bShow);
    }

    public void ChangeHandText(HandType hand)
    {
        switch (hand)
        {
            case HandType.Navigator:
                TriggerText.text = "Navigator";
                break;
            case HandType.Interactor:
                TriggerText.text = "Action";
                break;
            case HandType.Grabber:
                TriggerText.text = "Grab";
                break;
            case HandType.Spawner:
                break;
            case HandType.Flamethrower:
                TriggerText.text = "FIRE!";
                break;
            default:
                break;
        }
    }
    public void SetLineRendererScales(float multiplier)
    {
        if (_lineRendererStartingScales == null)
        {
            _lineRendererStartingScales = new float[LRenderers.Length];
            for (int i = 0; i < LRenderers.Length; i++)
            {
                _lineRendererStartingScales[i] = LRenderers[i].startWidth;
            }
        }
        for (int i = 0; i < LRenderers.Length; i++)
        {
            //Debug.Log("LREN: " + LRenderers[i].name);
            //Debug.Log("Starting Scales length: " + _lineRendererStartingScales.Length);
            //Debug.Log("StartScale: " + _lineRendererStartingScales[i]);
            //Debug.Log("Multiplier: " + multiplier);
            LRenderers[i].startWidth = _lineRendererStartingScales[i] * multiplier;
            LRenderers[i].endWidth = _lineRendererStartingScales[i] * multiplier;
        }
    }

    public void ToggleHelp()
    {
        ShowHelp = !ShowHelp;
        if (ShowHelp)
        {
            EnableLaser(true);
        }
        else
        {
            EnableLaser(false);
        }
    }
}
*/