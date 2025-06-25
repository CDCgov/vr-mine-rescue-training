using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DebriefOverviewUIController : MonoBehaviour
{
    public SessionEventManager SessionEventManager;
    public TeleportManager TeleportManager;

    public Camera OverviewCamera;
    public Slider HorizontalPositionSlider;
    public Slider VerticalPositionSlider;
    public Slider ZoomSlider;
    public DebriefSceneLoader DebriefSceneLoader;

    public RectTransform OverviewCameraArea;
    

    private Vector3 _CamStartPos;
    private float _camOrthoScale;
    // Start is called before the first frame update
    void Start()
    {
        //if(OverviewCamera == null)
        //{
        //    OverviewCamera = GameObject.Find("OverviewCamera").GetComponent<Camera>();
        //    if(OverviewCamera == null)
        //    {
        //        Debug.LogError("Couldn't find overview camera");
        //        return;
        //    }
        //}
        //_camOrthoScale = OverviewCamera.orthographicSize;
        DebriefSceneLoader.SceneLoaded += ConfigureCamera;
        

        if(SessionEventManager == null)
        {
            //SessionEventManager = FindObjectOfType<SessionEventManager>();
            SessionEventManager = SessionEventManager.GetDefault(gameObject);
        }

        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);

        TeleportManager.AfterTeleport += OnTeleport;

        OnZoomValueChanged();
    }

    private void OnTeleport(Transform teleportPoint)
    {
        if (OverviewCameraArea != null)
        {
            var pos = OverviewCamera.WorldToScreenPoint(teleportPoint.position);
            if (!RectTransformUtility.RectangleContainsScreenPoint(OverviewCameraArea, pos))
            {
                MoveOverviewCamera(teleportPoint);
            }
            
        }
        else
        {
            MoveOverviewCamera(teleportPoint);
        }
    }

    private void MoveOverviewCamera(Transform dest)
    {
        var pos = OverviewCamera.transform.position;
        pos.x = dest.position.x;
        pos.z = dest.position.z;

        OverviewCamera.transform.position = pos;
    }

    //private void Update()
    //{
    //    bool overUIObject = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    //    if (overUIObject)
    //    {
    //        //var inputModule = EventSystem.current.currentInputModule as StandaloneInputModule;
    //        var inputModule = EventSystem.current.currentInputModule as DebriefInputModule;
    //        if (inputModule != null)
    //        {
    //            inputModule.GetHoveredObject();
    //        }
    //        return;
    //    }

    //    float wheel = Input.GetAxis("Mouse ScrollWheel");
    //    if(Mathf.Abs(wheel) > 0)
    //        ZoomSlider.value += wheel;
    //}

    public void OnHorizontalValueChanged()
    {
        Vector3 newPos = OverviewCamera.transform.position;
        newPos.x = Mathf.Lerp(_CamStartPos.x - 20, _CamStartPos.x + 20, HorizontalPositionSlider.value);
        OverviewCamera.transform.position = newPos;
    }

    public void OnVerticalValueChanged()
    {
        Vector3 newPos = OverviewCamera.transform.position;
        newPos.z = Mathf.Lerp(_CamStartPos.z - 20, _CamStartPos.z + 20, VerticalPositionSlider.value);
        OverviewCamera.transform.position = newPos;
    }

    public void Zoom(float value)
    {
        ZoomSlider.value += value * 0.5f;
    }

    public void OnZoomValueChanged()
    {
        //OverviewCamera.orthographicSize = Mathf.Lerp(_camOrthoScale, 1, ZoomSlider.value);
        //OverviewCamera.orthographicSize = Mathf.Lerp(32, 1, ZoomSlider.value);
        OverviewCamera.orthographicSize = Mathf.Lerp(32.0f * 3.0f, 1, ZoomSlider.value);
        SessionEventManager.UpdateScale(ZoomSlider.value);
    }

    public void ScrollViewChanged(Vector2 pos)
    {
        OverviewCamera.transform.position = new Vector3(_CamStartPos.x - pos.x, _CamStartPos.y, _CamStartPos.z - pos.y);
    }

    public void ConfigureCamera()
    {
        //Debug.Log("+++++++++++++++++++++++++IN CONFIG CAMERA+++++++++++++++++++++");
        if (OverviewCamera == null)
        {
            OverviewCamera = GameObject.Find("OverviewCamera").GetComponent<Camera>();
            if (OverviewCamera == null)
            {
                Debug.LogError("Couldn't find overview camera");
                return;
            }
        }
        _camOrthoScale = OverviewCamera.orthographicSize;
        _CamStartPos = OverviewCamera.transform.position;
        //Debug.Log("Camera start pos " + _CamStartPos);
    }

    public void ConfigureCamera(Camera cam)
    {
        //Debug.Log("+++++++++++++++++++++++++IN CONFIG CAMERA+++++++++++++++++++++");
        if (OverviewCamera == null)
        {
            OverviewCamera = cam;
        }
        _camOrthoScale = OverviewCamera.orthographicSize;
        _CamStartPos = OverviewCamera.transform.position;
        //Debug.Log("Camera start pos " + _CamStartPos);
    }

    private void OnDestroy()
    {
        DebriefSceneLoader.SceneLoaded -= ConfigureCamera;
        TeleportManager.AfterTeleport -= OnTeleport;
    }
}
