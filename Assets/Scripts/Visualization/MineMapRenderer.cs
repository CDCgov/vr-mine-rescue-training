using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.SceneManagement;
using Unity.VectorGraphics;

public class MineMapRenderer : MonoBehaviour, IPointerClickHandler
{
    public GameObject ResearcherCam;

    private GameObject _camObj;
    private Camera _cam;
    private RectTransform _rect;
    private MineNetwork _mineNet;

    private LineSpriteGenerator _lineSprite;

    // Start is called before the first frame update
    void Start()
    {
        //g3.Vector3d v3d = transform.position; 

        _rect = GetComponent<RectTransform>();
        RenderTexture rt = new RenderTexture(640, 480, 32);

        _camObj = new GameObject("MineMapCam");
        _cam = _camObj.AddComponent<Camera>();
        _cam.targetTexture = rt;

        _cam.cullingMask= LayerMask.GetMask("Floor", "MineMap");

        var img = GetComponent<RawImage>();
        img.texture = rt;
        
        //UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;

        //PositionCamera();

        GameObject obj = new GameObject();
        obj.transform.SetParent(transform);
        
        var rxt = obj.AddComponent<RectTransform>();
        rxt.anchorMin = Vector2.zero;
        rxt.anchorMax = Vector2.one;
        rxt.offsetMin = Vector3.zero;
        rxt.offsetMax = Vector3.zero;

        obj.AddComponent<CanvasRenderer>();
        obj.AddComponent<SVGImage>();
        _lineSprite = new LineSpriteGenerator();//obj.AddComponent<LineSpriteGenerator>();
        
        /*
        _lineSprite.AddPath(new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(100,0),
            new Vector2(100,100),
            new Vector2(0,100),
        }, Color.red, 1.0f); */

        _lineSprite.UpdateSprite(gameObject);
    }


    //private void OnSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    //{
        
    //    PositionCamera();
    //}

    //void PositionCamera()
    //{
    //    Bounds b = new Bounds();
    //    var mineSegs = GameObject.FindObjectsOfTypeAll(typeof(MineSegment));

    //    bool bFirst = true;

    //    foreach (var obj in mineSegs)
    //    {
    //        var seg = (MineSegment)obj;

    //        var segb = seg.SegmentBounds;
    //        segb.center += seg.transform.position;

    //        if (bFirst)
    //        {
    //            bFirst = false;
    //            //b = segb;
    //            b = new Bounds(seg.transform.position, Vector3.zero);
    //        }
    //        else
    //        {
    //            //b.Encapsulate(segb);
    //            b.Encapsulate(seg.transform.position);
    //        }

    //    }

    //    _cam.orthographic = true;
    //    _cam.transform.position = b.center + Vector3.up * 15;
    //    _cam.transform.rotation = Quaternion.Euler(90, 0, 0);

    //    _cam.orthographicSize = Mathf.Max(b.extents.x, b.extents.y) + 5;
    //    _cam.aspect = _rect.rect.width / _rect.rect.height;
    //}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 pos = eventData.position;


        Vector2 size = Vector2.Scale(_rect.rect.size, transform.lossyScale);
        var r = new Rect((Vector2)transform.position - (size * 0.5f), size);

        pos -= r.center;
        //pos /= Mathf.Max(r.width, r.height);
        pos.x /= r.width;
        pos.y /= r.height;
        pos *= _cam.orthographicSize;
        Vector3 worldPos = _cam.transform.position + new Vector3(pos.x, 0, pos.y);
        Debug.Log(pos.ToString());

        if (eventData.button == PointerEventData.InputButton.Left)
        {
           
            if (ResearcherCam != null)
            {
                ResearcherCam.transform.position = new Vector3(worldPos.x, ResearcherCam.transform.position.y, worldPos.z);
                //ResearcherCam.transform.position = new Vector3(wpos.x, ResearcherCam.transform.position.y, wpos.z);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {

        }
    }
}
