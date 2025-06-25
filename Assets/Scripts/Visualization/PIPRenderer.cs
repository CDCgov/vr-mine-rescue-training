using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.SceneManagement;
using Unity.VectorGraphics;


[RequireComponent(typeof(RawImage))]
public class PIPRenderer : MonoBehaviour, IPointerClickHandler
{
    public event System.Action Clicked;

    public float PIPMaxFPS = 15;
    public GameObject CameraPrefab;
    public int ResolutionX = 640;
    public int ResolutionY = 480;

    private GameObject _camObj;
    private Camera _cam;
    private RectTransform _rect;
    private RenderTexture _rt;

    // Start is called before the first frame update
    void Start()
    {
        if (CameraPrefab == null)
        {
            Debug.LogError($"No camera prefab on PIP Renderer {gameObject.name}");
            return;
        }
        
        
        _rect = GetComponent<RectTransform>();
        //_rt = new RenderTexture(ResolutionX, ResolutionY, 32);
        _rt = new RenderTexture((int)_rect.rect.width, (int)_rect.rect.height, 24, RenderTextureFormat.RGB111110Float);

        //_camObj = new GameObject("PIPCam");
        //_cam = _camObj.AddComponent<Camera>();
        _camObj = Instantiate<GameObject>(CameraPrefab);
        _cam = _camObj.GetComponent<Camera>();


        _cam.enabled = false;
        _cam.targetTexture = _rt;

        //make sure the gameobject ends up in the right scene
        _cam.transform.SetParent(transform);
        //_cam.transform.SetParent(null);

        //_cam.backgroundColor = Color.black;

        // _cam.cullingMask = LayerMask.GetMask("Floor", "MineMap");

        var img = GetComponent<RawImage>();
        img.texture = _rt;

        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;


        GameObject obj = new GameObject();
        obj.transform.SetParent(transform);

        var rxt = obj.AddComponent<RectTransform>();
        rxt.anchorMin = Vector2.zero;
        rxt.anchorMax = Vector2.one;
        rxt.offsetMin = Vector3.zero;
        rxt.offsetMax = Vector3.zero;


    }

    public void PositionCamera(Vector3 pos, Quaternion rot)
    {
        if (_camObj == null)
            return;


        _camObj.transform.position = pos;
        _camObj.transform.rotation = rot;
    }


    private void OnSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {

    }

    private void LateUpdate()
    {
        RenderTexture.active = null;
    }

    private float _lastRender = 0;

    // Update is called once per frame
    //void Update()
    //{
    //    var elapsed = Time.time - _lastRender;

    //    if (_cam != null && elapsed > 1.0f / PIPMaxFPS)
    //    {
    //        //RenderCamera();
    //    }
    //}

    public void RenderCamera()
    {
        if (_cam == null)
            return;

        _lastRender = Time.time;
        //_cam.forceIntoRenderTexture = true;
        //_cam.targetTexture = _rt;
        _cam.Render();

        //_cam.targetTexture = null;
        RenderTexture.active = null;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke();
        //RenderCamera();
    }
}
