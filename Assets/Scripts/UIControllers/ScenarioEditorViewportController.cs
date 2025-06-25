using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioEditorViewportController : MonoBehaviour
{

    public Camera SceneCamera;
    public Camera CanvasCamera;
    public RectTransform BottomLeftAnchor;
    public RectTransform TopRightAnchor;
    private RectTransform _rt;
    private Vector2 _bottomLeftViewportPosition;
    private Vector2 _topRightViewportPosition;
    private Rect _viewportRect;
    private float _width;
    private float _height;
    private bool _runScenarioEditor;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        MinimizableWindowController minController = GetComponent<MinimizableWindowController>();
        if (minController)
        {

            minController.StateChanged += OnChangeState;
        }
    }
    void Update()
    {
        if(_runScenarioEditor) 
        {
            _bottomLeftViewportPosition = CanvasCamera.ScreenToViewportPoint(BottomLeftAnchor.position);
            _topRightViewportPosition = CanvasCamera.ScreenToViewportPoint(TopRightAnchor.position);
            _width = _topRightViewportPosition.x - _bottomLeftViewportPosition.x;
            _height = _topRightViewportPosition.y - _bottomLeftViewportPosition.y;
            _viewportRect = new Rect(_bottomLeftViewportPosition.x, _bottomLeftViewportPosition.y, _width, _height);
            SceneCamera.rect = _viewportRect;
        }
    }

    public void SetRectValues( Rect rect, Vector2 anchorMin , Vector2 anchorMax)
    {
        _rt.anchorMax = anchorMax;
        _rt.anchorMin = anchorMin;
        _rt.anchoredPosition = new Vector2(rect.x, rect.y);
        _rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
        _rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);

    }
    public void Initialize()
    {

        _runScenarioEditor = true;

    }
    void OnChangeState(bool minimize, RectTransform tfm)
    {
        SceneCamera.gameObject.SetActive( !minimize);
       
    }
}
