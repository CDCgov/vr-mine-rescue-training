using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Valve.VR;


public class ViveWandDemo : MonoBehaviour
{
    /*
    public Transform HeadTransform; 
    public GameObject WandModel;
    public GameObject LaserPrefab;
    public GameObject DemoObjectPrefab;
    public float DemoObjectScale = 0.05f;
    public Vector3 DemoObjectOffset;
    public bool DisableLaser = false;

    private SteamVR_PlayArea _playArea;
    private SteamVR_TrackedController _wand;
    private LineRenderer _laser;
    private Vector3 _laserEndPoint;
    private bool _laserHit = false;

    private bool _laserOn = false;
    private GameObject _demoObj;

    // Use this for initialization
    void Start()
    {
        _playArea = transform.GetComponentInParent<SteamVR_PlayArea>();
        _wand = GetComponent<SteamVR_TrackedController>();

        var laserObj = GameObject.Instantiate<GameObject>(LaserPrefab);
        laserObj.transform.SetParent(transform, false);
        laserObj.transform.localPosition = Vector3.zero;
        laserObj.transform.localRotation = Quaternion.identity;
        _laser = laserObj.GetComponent<LineRenderer>();
        _laser.gameObject.SetActive(false);

        _wand.MenuButtonClicked += OnMenuButton;
        _wand.TriggerClicked += OnTriggerClicked;
        _wand.TriggerUnclicked += OnTriggerUnclicked;
        _wand.PadClicked += OnPadClicked;

        _demoObj = GameObject.Instantiate<GameObject>(DemoObjectPrefab);
        _demoObj.transform.SetParent(transform);
        _demoObj.transform.localPosition = Vector3.zero;
        _demoObj.transform.localRotation = Quaternion.identity;
        _demoObj.SetActive(false);
    }

    Vector3 ComputePlayAreaCenter()
    {
        return transform.parent.position + Vector3.up;
    }

    private void OnPadClicked(object sender, ClickedEventArgs e)
    {
        if (_laserOn)
        {
            RaycastHit hit;
            Vector3 endPoint;

            if (Physics.Raycast(transform.position, transform.forward, out hit))
            {
                endPoint = hit.point;
                Vector3 groundedHit = hit.point;
                groundedHit.y = 0;
                groundedHit.x = groundedHit.x - HeadTransform.localPosition.x;
                groundedHit.z = groundedHit.z - HeadTransform.localPosition.z;

                transform.parent.position = groundedHit;
            }
        }
    }

    private void OnTriggerUnclicked(object sender, ClickedEventArgs e)
    {
        //_laserOn = false;
    }

    private void OnTriggerClicked(object sender, ClickedEventArgs e)
    {
        if (!DisableLaser)
        {
            EnableLaser(!_laserOn);
        }
    }

    private void OnMenuButton(object sender, ClickedEventArgs e)
    {
        //var pos = ComputePlayAreaCenter();

        //var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //obj.transform.position = pos;

        ShowDemoObject(!_demoObj.activeSelf);
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

    private void EnableLaser(bool bEnable)
    {
        _laserOn = bEnable;
        _laser.gameObject.SetActive(bEnable);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ShowDemoObject(!_demoObj.activeSelf);
        }

        if (_laserOn)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit))
            {
                float dist = Vector3.Distance(hit.point, transform.position);
                _laser.SetPosition(1, new Vector3(0, 0, dist));
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
    }
    public bool GetLaserStatus()
    {
        return _laserOn;
    }

    public bool GetLaserHit()
    {
        return _laserHit;
    }
    public Vector3 GetLaserEndPoint()
    {
        return _laserEndPoint;
    }
    */
}
