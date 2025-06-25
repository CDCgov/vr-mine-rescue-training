using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum VoiceMenuType
{
    MainMenu,
    LeftHandMenu,
    RightHandMenu,
    Off
}

public class VoiceMenu : MonoBehaviour
{
    /*
    public InputBindingManager InputBindingManager;

    public GameObject MenuObject;
    public GameObject LeftHandMenu;
    public GameObject RightHandMenu;
    public GameObject PlayerHead;
    public GameObject MyLight;
    public AudioSource AudioPlayer;
    public Transform OverheadPoint;
    public Transform HeadTransform;
    public Transform PlayerTransform;
    public Transform TeleportPoint;
    public Transform TeleportPointApproachingCars;

    public GameObject OnButton;
    public GameObject OffButton;
    public GameObject WorldUI;
    public GameObject SafeRoom;

    public HandControl LeftHandController;
    public HandControl RightHandController;
    public Text LeftHandModeLabel;
    public Text RightHandModeLabel;
    public Text OverheadViewLbl;
    public Text HintsLabel;
    public AudioSource TeleportSfx;
    public AudioSource VoiceMenuConfirm;

    public GameObject Wall;

    public VRPointOfInterest OverviewPOI;
    public GameObject OverviewLight;
    [SerializeField]
    private string[] m_Keywords;

    private KeywordRecognizer m_Recognizer;
    private bool bMenuActive = false;
    private bool bSafeRoomActive = false;

    private VoiceMenuType _menuType = VoiceMenuType.Off;
    private Vector3 _oldPosition;
    private Vector3 _startingPosition;
    private FollowTransform VRCameraFT;

    private float[] _startRightWidths;
    private float[] _startLeftWidths;

    void Awake()
    {
        _startLeftWidths = new float[LeftHandController.LRenderers.Length];
        _startRightWidths = new float[RightHandController.LRenderers.Length];
        for (int i = 0; i < _startRightWidths.Length; i++)
        {
            _startRightWidths[i] = RightHandController.LRenderers[i].startWidth;            
        }
        for (int i = 0; i < _startLeftWidths.Length; i++)
        {
            _startLeftWidths[i] = LeftHandController.LRenderers[i].startWidth;
        }
    }
    // Use this for initialization
    void Start()
    {
        m_Recognizer = new KeywordRecognizer(m_Keywords);
        m_Recognizer.OnPhraseRecognized += OnPhraseRecognized;
        m_Recognizer.Start();
        _menuType = VoiceMenuType.Off;
        LeftHandModeLabel.text = LeftHandController.Type.ToString();
        RightHandModeLabel.text = RightHandController.Type.ToString();
        _startingPosition = PlayerTransform.position;
        VRCameraFT = PlayerTransform.GetComponent<FollowTransform>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("RightVRMenu") || Input.GetButtonDown("LeftVRMenu"))
        {
            MoveToOverview();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayerTransform.localScale = Vector3.one;
            PlayerTransform.position = _startingPosition;
            PlayerTransform.localRotation = Quaternion.identity;            
            LeftHandController.SetLineRendererScales(1);
            RightHandController.SetLineRendererScales(1);
            VRCameraFT.Target = null;
            VRCameraFT.ResetToPOIOnTargetLost = false;
            if (TeleportSfx != null)
            {
                TeleportSfx.Play();
            }
            if(OverviewLight != null)
            {
                OverviewLight.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayerTransform.localScale = Vector3.one;
            PlayerTransform.position = TeleportPoint.position;
            Debug.Log("Went: " + PlayerTransform.position);
            PlayerTransform.localRotation = TeleportPoint.localRotation;            
            LeftHandController.SetLineRendererScales(1);
            RightHandController.SetLineRendererScales(1);
            VRCameraFT.Target = null;
            VRCameraFT.ResetToPOIOnTargetLost = false;
            if (TeleportSfx != null)
            {
                TeleportSfx.Play();
            }
            if (OverviewLight != null)
            {
                OverviewLight.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayerTransform.localScale = Vector3.one;
            PlayerTransform.position = TeleportPointApproachingCars.position;
            
            LeftHandController.SetLineRendererScales(1);
            RightHandController.SetLineRendererScales(1);
            VRCameraFT.Target = null;
            VRCameraFT.ResetToPOIOnTargetLost = false;
            if (TeleportSfx != null)
            {
                TeleportSfx.Play();
            }
            if (OverviewLight != null)
            {
                OverviewLight.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            ChangeHandMode(_menuType, HandType.Grabber);
            VoiceMenuConfirm.Play();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            ChangeHandMode(_menuType, HandType.Interactor);
            VoiceMenuConfirm.Play();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            VoiceMenuConfirm.Play();
            SceneManager.LoadScene("VRDemoScene");
        }

        // if (Input.GetKeyDown(KeyCode.X))
        // {
        // 	Debug.Log("Loading experiment VP_HMD");
        // 	ExperimentManager.CCLoadExperiment("VP_HMD");
        // }
    }

    private void OnEnable()
    {
        if (InputBindingManager == null)
            InputBindingManager = InputBindingManager.GetDefault();

        InputBindingManager.RegisterAction("VRNavigateMode", "VR", NavigateMode);
        InputBindingManager.RegisterAction("VRGrabMode", "VR", GrabMode);
        InputBindingManager.RegisterAction("VRActionMode", "VR", ActionMode);
        InputBindingManager.RegisterAction("VRMoveToOverview", "VR", MoveToOverview);
        InputBindingManager.RegisterAction("VRToggleHelp", "VR", ToggleHelp);
    }    

    private void OnDisable()
    {
        InputBindingManager.UnregisterAction("VRNavigateMode");
        InputBindingManager.UnregisterAction("VRGrabMode");
        InputBindingManager.UnregisterAction("VRActionMode");
        InputBindingManager.UnregisterAction("VRMoveToOverview");
        InputBindingManager.UnregisterAction("VRToggleHelp");
    }

    private void ToggleHelp()
    {
        //LeftHandController.ShowHelp = !LeftHandController.ShowHelp;
        //RightHandController.ShowHelp = !RightHandController.ShowHelp;

        LeftHandController.ToggleHelp();
        RightHandController.ToggleHelp();
    }

    private void MoveToOverview()
    {
        VRCameraFT.Target = OverviewPOI.transform;
        VRCameraFT.ResetToPOIOnTargetLost = true;

        VRCameraFT.transform.localScale = new Vector3(OverviewPOI.WorldScaleMultiplier, OverviewPOI.WorldScaleMultiplier, OverviewPOI.WorldScaleMultiplier);
        //UpscaleLineRends
        //for(int i = 0; i < RightHandController.LRenderers.Length; i++)
        //{
        //    RightHandController.LRenderers[i].startWidth = _startRightWidths[i] * OverviewPOI.WorldScaleMultiplier;
        //    RightHandController.LRenderers[i].endWidth = _startRightWidths[i] * OverviewPOI.WorldScaleMultiplier;
        //}
        //for (int i = 0; i < LeftHandController.LRenderers.Length; i++)
        //{
        //    LeftHandController.LRenderers[i].startWidth = _startLeftWidths[i] * OverviewPOI.WorldScaleMultiplier;
        //    LeftHandController.LRenderers[i].endWidth = _startLeftWidths[i] * OverviewPOI.WorldScaleMultiplier;
        //}
        LeftHandController.SetLineRendererScales(OverviewPOI.WorldScaleMultiplier);
        RightHandController.SetLineRendererScales(OverviewPOI.WorldScaleMultiplier);
        OverviewLight.SetActive(true);
        //for(int i=0; i<VRCameraFT.transform.childCount; i++)
        //{
        //    LineRenderer lren = VRCameraFT.transform.GetChild(i).GetComponentInChildren<LineRenderer>();
        //    lren.startWidth = 5f;
        //    lren.endWidth = 5f;
        //}
    }

    private void NavigateMode()
    {
        ChangeHandMode(_menuType, HandType.Navigator);
        VoiceMenuConfirm.Play();
    }

    private void GrabMode()
    {
        ChangeHandMode(_menuType, HandType.Grabber);
        VoiceMenuConfirm.Play();
    }

    private void ActionMode()
    {
        ChangeHandMode(_menuType, HandType.Interactor);
        VoiceMenuConfirm.Play();
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
        builder.AppendFormat("\tTimestamp: {0}{1}", args.phraseStartTime, Environment.NewLine);
        builder.AppendFormat("\tDuration: {0} seconds{1}", args.phraseDuration.TotalSeconds, Environment.NewLine);
        Debug.Log(builder.ToString());
        switch (args.text)
        {
            case "Voice Menu":
                MenuObject.SetActive(true);
                _menuType = VoiceMenuType.MainMenu;
                bMenuActive = true;
                VoiceMenuConfirm.Play();
                break;
            case "Close Menu":
                VoiceMenuConfirm.Play();
                CloseMenu();
                break;
            case "Mine Phone":
                AudioPlayer.Play();
                break;
            case "Lights On":

                MyLight.SetActive(true);
                OnButton.SetActive(false);
                OffButton.SetActive(true);
                VoiceMenuConfirm.Play();

                break;
            case "Lights Off":
                MyLight.SetActive(false);
                OffButton.SetActive(false);
                OnButton.SetActive(true);
                VoiceMenuConfirm.Play();
                break;
            case "Wall":
                Wall.SetActive(true);
                break;
            case "Left Hand Menu":
                if (_menuType == VoiceMenuType.MainMenu)
                {
                    LeftHandMenu.SetActive(true);
                    MenuObject.SetActive(false);
                    _menuType = VoiceMenuType.LeftHandMenu;
                    VoiceMenuConfirm.Play();
                }
                break;
            case "Right Hand Menu":
                if (_menuType == VoiceMenuType.MainMenu)
                {
                    RightHandMenu.SetActive(true);
                    MenuObject.SetActive(false);
                    _menuType = VoiceMenuType.RightHandMenu;
                    VoiceMenuConfirm.Play();
                }
                break;
            case "Fus Ro Da":
                Collider[] colliders = Physics.OverlapSphere(PlayerHead.transform.position, 12);
                foreach (Collider col in colliders)
                {
                    Debug.Log(col.name);
                    Rigidbody rb = col.GetComponentInChildren<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddExplosionForce(5000, PlayerHead.transform.position, 12);
                    }
                }
                break;
            case "Grab Mode":
                GrabMode();
                break;
            case "Action Mode":
                ActionMode();
                break;
            case "Navigate Mode":
                NavigateMode();
                break;
            case "Hello World":
                break;
            case "Overhead View":
                //_oldPosition = PlayerTransform.position;
                //TeleportAvatar(OverheadPoint.position);
                //Quaternion rot = new Quaternion();
                //rot.eulerAngles = new Vector3(0, 0, 0);
                //PlayerTransform.rotation = rot;
                //if (TeleportSfx != null)
                //{
                //    TeleportSfx.Play();
                //}
                MoveToOverview();
                
                break;
            case "Return":
                TeleportAvatar(_oldPosition);
                Quaternion rot2 = new Quaternion();
                rot2.eulerAngles = new Vector3(0, 0, 0);
                PlayerTransform.rotation = rot2;
                if (TeleportSfx != null)
                {
                    TeleportSfx.Play();
                }
                break;
            case "Reset":
                VoiceMenuConfirm.Play();
                SceneManager.LoadScene("VRDemoScene");
                break;
            case "Back":
                switch (_menuType)
                {
                    case VoiceMenuType.MainMenu:
                        break;
                    case VoiceMenuType.LeftHandMenu:
                        _menuType = VoiceMenuType.MainMenu;
                        LeftHandMenu.SetActive(false);
                        MenuObject.SetActive(true);
                        VoiceMenuConfirm.Play();
                        break;
                    case VoiceMenuType.RightHandMenu:
                        _menuType = VoiceMenuType.MainMenu;
                        RightHandMenu.SetActive(false);
                        MenuObject.SetActive(true);
                        VoiceMenuConfirm.Play();
                        break;
                    case VoiceMenuType.Off:
                        break;
                    default:
                        break;
                }
                break;
            case "Flamethrower":
                if (_menuType == VoiceMenuType.MainMenu)
                {
                    RightHandController.Type = HandType.Flamethrower;
                    LeftHandController.Type = HandType.Flamethrower;
                }
                break;
            case "Default Hands":
                ChangeHandMode(VoiceMenuType.LeftHandMenu, HandType.Navigator);
                ChangeHandMode(VoiceMenuType.RightHandMenu, HandType.Grabber);
                VoiceMenuConfirm.Play();
                break;
            case "Go Home":
                PlayerTransform.position = _startingPosition;
                if (TeleportSfx != null)
                {
                    TeleportSfx.Play();
                }
                //_oldPosition = _startingPosition;
                break;
            case "Hide Hints":
                WorldUI.SetActive(false);
                HintsLabel.text = "Show Hints";
                VoiceMenuConfirm.Play();
                break;
            case "Show Hints":
                WorldUI.SetActive(true);
                HintsLabel.text = "Hide Hints";
                VoiceMenuConfirm.Play();
                break;
            case "Exit Simulation":
                Application.Quit();
                break;
            case "Safe Room":
                bSafeRoomActive = !bSafeRoomActive;
                SafeRoom.SetActive(bSafeRoomActive);
                break;
            default:
                break;
        }
    }

    private void CloseMenu()
    {
        switch (_menuType)
        {
            case VoiceMenuType.MainMenu:
                MenuObject.SetActive(false);
                break;
            case VoiceMenuType.LeftHandMenu:
                LeftHandMenu.SetActive(false);
                break;
            case VoiceMenuType.RightHandMenu:
                RightHandMenu.SetActive(false);
                break;
            default:
                break;
        }
        bMenuActive = false;
        _menuType = VoiceMenuType.Off;
    }

    private void ChangeHandMode(VoiceMenuType menuType, HandType newMode)
    {
        switch (menuType)
        {
            case VoiceMenuType.LeftHandMenu:
                LeftHandController.Type = newMode;
                LeftHandModeLabel.text = LeftHandController.Type.ToString();
                LeftHandController.ChangeHandText(newMode);
                //ViveWandDemo vive = LeftHandController.GetComponent<ViveWandDemo>();
                //if (vive != null)
                //{
                //    if (newMode == HandType.Grabber)
                //    {
                //        vive.DisableLaser = true;
                //    }
                //    else
                //    {
                //        vive.DisableLaser = false;
                //    }
                //}
                break;
            case VoiceMenuType.RightHandMenu:
                RightHandController.Type = newMode;
                RightHandModeLabel.text = RightHandController.Type.ToString();
                RightHandController.ChangeHandText(newMode);
                break;
            default:
                switch (newMode)
                {
                    case HandType.Navigator:
                        LeftHandController.Type = newMode;
                        LeftHandController.ChangeHandText(HandType.Navigator);
                        LeftHandModeLabel.text = LeftHandController.Type.ToString();
                        break;
                    case HandType.Interactor:
                        RightHandController.Type = newMode;
                        RightHandController.ChangeHandText(HandType.Interactor);
                        RightHandModeLabel.text = RightHandController.Type.ToString();
                        break;
                    case HandType.Grabber:
                        RightHandController.Type = newMode;
                        RightHandController.ChangeHandText(HandType.Grabber);
                        RightHandModeLabel.text = RightHandController.Type.ToString();
                        break;
                    default:
                        break;
                }
                break;
        }
    }

    public void SetTeleportReturnPoint()
    {
        _oldPosition = PlayerTransform.position;
    }

    private void TeleportAvatar(Vector3 point)
    {
        Vector3 toTeleport = point;
        toTeleport.y = toTeleport.y - HeadTransform.localPosition.y;
        toTeleport.x = toTeleport.x - HeadTransform.localPosition.x;
        toTeleport.z = toTeleport.z - HeadTransform.localPosition.z;
        PlayerTransform.position = toTeleport;
    }

    public void PlayWhooshSfx()
    {
        if (TeleportSfx != null)
        {
            TeleportSfx.Play();
        }
    }

    */
}
