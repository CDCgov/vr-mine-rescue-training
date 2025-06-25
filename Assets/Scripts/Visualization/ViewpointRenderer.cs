using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ViewpointRenderer : MonoBehaviour
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
        _rect = GetComponent<RectTransform>();
        RenderTexture rt = new RenderTexture(640, 480, 32);

        _camObj = new GameObject("MineMapCam");
        _cam = _camObj.AddComponent<Camera>();
        _cam.targetTexture = rt;

        _cam.cullingMask = LayerMask.GetMask("Floor", "MineMap");

        var img = GetComponent<RawImage>();
        img.texture = rt;

        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;

        PositionCamera();

        
    }


    private void OnSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {

        PositionCamera();
    }

    void PositionCamera()
    {
        Bounds b = new Bounds();
        var mineSegs = GameObject.FindObjectsOfTypeAll(typeof(MineSegment));

        bool bFirst = true;

        foreach (var obj in mineSegs)
        {
            var seg = (MineSegment)obj;

            var segb = seg.SegmentBounds;
            segb.center += seg.transform.position;

            if (bFirst)
            {
                bFirst = false;
                //b = segb;
                b = new Bounds(seg.transform.position, Vector3.zero);
            }
            else
            {
                //b.Encapsulate(segb);
                b.Encapsulate(seg.transform.position);
            }

        }

        _cam.orthographic = true;
        _cam.transform.position = b.center + Vector3.up * 15;
        _cam.transform.rotation = Quaternion.Euler(90, 0, 0);

        _cam.orthographicSize = Mathf.Max(b.extents.x, b.extents.y) + 5;
        _cam.aspect = _rect.rect.width / _rect.rect.height;
    }

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

        //Vector3 wpos = _cam.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 0));
        Debug.Log(pos.ToString());

        if (ResearcherCam != null)
        {
            ResearcherCam.transform.position = new Vector3(worldPos.x, ResearcherCam.transform.position.y, worldPos.z);
            //ResearcherCam.transform.position = new Vector3(wpos.x, ResearcherCam.transform.position.y, wpos.z);
        }
    }
}
