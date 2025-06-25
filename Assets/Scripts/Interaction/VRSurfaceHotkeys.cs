using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSurfaceHotkeys : MonoBehaviour
{
    /*
    public InputBindingManager InputBindingManager;
    public FollowTransform VRCameraFT;
    public VRPointOfInterest OverviewPOI;
    public VRPointOfInterest HaulTruckPOI;
    public HandControl LeftController;
    public HandControl RightController;
    public LineRenderer[] LineRenderersToUpscale;

    private float[] _lineRendererStartingScales;
    private bool _StartOverview = true;
    private GameObject[] _mobileEquipmentGO;
    private List<Vector3> _mobileEquipmentStartPos;
    private List<Quaternion> _mobileEquipmentStartRot;
    private List<int> _circuitIndex;
    private VehicleQueueArea[] _vehicleQueueAreas;
    private AudioSource[] _audioSources;

    // Start is called before the first frame update
    void Awake()
    {
        //_lineRendererStartingScales = new float[LineRenderersToUpscale.Length];
        //for (int i = 0; i < LineRenderersToUpscale.Length; i++)
        //{
        //    _lineRendererStartingScales[i] = LineRenderersToUpscale[i].startWidth;
        //}
        
    }

    void Start()
    {
        //MoveToOverview();
        _mobileEquipmentGO = GameObject.FindGameObjectsWithTag("MobileEquipment");
        _mobileEquipmentStartPos = new List<Vector3>();
        _mobileEquipmentStartRot = new List<Quaternion>();
        _circuitIndex = new List<int>();
        if (_mobileEquipmentGO != null)
        {
            for (int i = 0; i < _mobileEquipmentGO.Length; i++)
            {
                _mobileEquipmentStartPos.Add(_mobileEquipmentGO[i].transform.position);
                _mobileEquipmentStartRot.Add(_mobileEquipmentGO[i].transform.rotation);
            }
        }
        _vehicleQueueAreas = GameObject.FindObjectsOfType<VehicleQueueArea>();
        _audioSources = GameManager.FindObjectsOfType<AudioSource>();
    }

    private void OnEnable()
    {
        if (InputBindingManager == null)
            InputBindingManager = InputBindingManager.GetDefault();

        InputBindingManager.RegisterAction("VRMoveToOverview", "VR", MoveToOverview);
        InputBindingManager.RegisterAction("VRMoveToHaulTruck", "VR", MoveToHaulTruck);
        InputBindingManager.RegisterAction("VRResetHaulTrucks", "VR", ResetHaulTrucks);
        InputBindingManager.RegisterAction("VRToggleHelp", "VR", ToggleHelp);
    }

    private void OnDisable()
    {
        InputBindingManager.UnregisterAction("VRMoveToOverview");
        InputBindingManager.UnregisterAction("VRMoveToHaulTruck");
        InputBindingManager.UnregisterAction("VRResetHaulTrucks");
        InputBindingManager.UnregisterAction("VRToggleHelp");
    }

    private void MoveToOverview()
    {
        VRCameraFT.Target = OverviewPOI.transform;
        VRCameraFT.ResetToPOIOnTargetLost = true;

        VRCameraFT.transform.localScale = new Vector3(OverviewPOI.WorldScaleMultiplier, OverviewPOI.WorldScaleMultiplier, OverviewPOI.WorldScaleMultiplier);
        //UpscaleLineRends
        //for (int i = 0; i < LineRenderersToUpscale.Length; i++)
        //{
        //    LineRenderersToUpscale[i].startWidth = _lineRendererStartingScales[i] * OverviewPOI.WorldScaleMultiplier;
        //    LineRenderersToUpscale[i].endWidth = _lineRendererStartingScales[i] * OverviewPOI.WorldScaleMultiplier;
        //}
        
        LeftController.SetLineRendererScales(OverviewPOI.WorldScaleMultiplier);
        RightController.SetLineRendererScales(OverviewPOI.WorldScaleMultiplier);
    }

    private void Update()
    {
        if (Input.GetButtonDown("RightVRMenu") || Input.GetButtonDown("LeftVRMenu"))
        {
            MoveToOverview();
        }
        if(Time.frameCount > 1 && _StartOverview)
        {
            MoveToOverview();
            _StartOverview = false;
            //StartAudioSources();
        }
    }

    private void ResetHaulTrucks()
    {
        if (_mobileEquipmentGO != null)
        {
            for (int i = 0; i < _mobileEquipmentGO.Length; i++)
            {
                Rigidbody rb = _mobileEquipmentGO[i].GetComponent<Rigidbody>();
                if(rb != null)
                {
                    rb.velocity = new Vector3(0, 0, 0);
                    rb.angularVelocity = new Vector3(0, 0, 0);
                }
                _mobileEquipmentGO[i].transform.position = _mobileEquipmentStartPos[i];
                _mobileEquipmentGO[i].transform.rotation = _mobileEquipmentStartRot[i];
                UnityStandardAssets.Vehicles.Car.CarAIControl ai = _mobileEquipmentGO[i].GetComponent<UnityStandardAssets.Vehicles.Car.CarAIControl>();
                UnityStandardAssets.Utility.WaypointProgressTracker progress = _mobileEquipmentGO[i].GetComponent<UnityStandardAssets.Utility.WaypointProgressTracker>();
                if(ai != null && progress != null)
                {
                    //progress.SetProgress(ai.StartWaypointIndex)d
                    ai.enabled = true;
                    progress.Reset();
                }
            }

            for(int i = 0; i < _vehicleQueueAreas.Length; i++)
            {
                _vehicleQueueAreas[i].ClearArea();
            }
        }
    }

    private void MoveToHaulTruck()
    {
        VRCameraFT.Target = HaulTruckPOI.transform;
        VRCameraFT.ResetToPOIOnTargetLost = true;

        VRCameraFT.transform.localScale = new Vector3(HaulTruckPOI.WorldScaleMultiplier, HaulTruckPOI.WorldScaleMultiplier, HaulTruckPOI.WorldScaleMultiplier);
        //UpscaleLineRends
        //for (int i = 0; i < LineRenderersToUpscale.Length; i++)
        //{
        //    LineRenderersToUpscale[i].startWidth = _lineRendererStartingScales[i] * OverviewPOI.WorldScaleMultiplier;
        //    LineRenderersToUpscale[i].endWidth = _lineRendererStartingScales[i] * OverviewPOI.WorldScaleMultiplier;
        //}
        VRActivateRearViews rearViews = HaulTruckPOI.GetComponent<VRActivateRearViews>();
        if(rearViews != null)
        {
            rearViews.ActivateRearViews(true);
        }

        LeftController.SetLineRendererScales(HaulTruckPOI.WorldScaleMultiplier);
        RightController.SetLineRendererScales(HaulTruckPOI.WorldScaleMultiplier);
    }
    public void ToggleHelp()
    {
        LeftController.ToggleHelp();
        RightController.ToggleHelp();
    }

    public void StartAudioSources()
    {
        foreach(AudioSource source in _audioSources)
        {
            source.Play();
        }
    }
    */
}
