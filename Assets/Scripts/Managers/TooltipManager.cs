using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

public class TooltipManager : MonoBehaviour
{
    public GameObject TooltipInstance;
    public TextMeshProUGUI TooltipText;
    public RectTransform TooltipTarget;
    public Vector3 TooltipOffset = Vector3.zero;

    //public Canvas GUICanvas;
    public bool TooltipActive = false;

    private RectTransform _tooltipTransform;
    private RectTransform _guiRectTransform;
    private TextMeshAutoSize _tmAutoSize;
    private RectTransform _cachedSource;
    private Vector3 _cachedOffset = Vector3.zero;
    private bool _updateTooltipFlag = false;
    private bool _performReparent = true;

    public static TooltipManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<TooltipManager>("TooltipManager");
    }

    private void Awake()
    {
        //The tooltip will not display if it's nto a child of a canvas, if one is not set in the manager, find fisrt available screenspace camera. Placement is done via world coordinates later
        //if (GUICanvas == null)
        //{
        //    //GUICanvas = FindObjectOfType<Canvas>();
        //    Canvas[] canvi = FindObjectsOfType<Canvas>();
        //    foreach (Canvas c in canvi)
        //    {
        //        if(c.renderMode == RenderMode.ScreenSpaceOverlay)
        //        {
        //            GUICanvas = c;
        //            break;
        //        }
        //    }
        //}

        //GUICanvas.TryGetComponent<RectTransform>(out _guiRectTransform);

        var tooltip = Resources.Load<GameObject>("GUI\\TooltipWCanvas");
        TooltipInstance = GameObject.Instantiate(tooltip).transform.GetChild(0).gameObject;//The canvas + tooltip resource only has 1 child the tooltip label
        TooltipInstance.name = "SpawnedTooltip";
        TooltipText = TooltipInstance.GetComponentInChildren<TextMeshProUGUI>();
        _tmAutoSize = TooltipInstance.GetComponentInChildren<TextMeshAutoSize>();
        //if(GUICanvas == null)
        //{
        //    GUICanvas = FindObjectOfType<Canvas>();
        //}

        //GUICanvas.TryGetComponent<RectTransform>(out _guiRectTransform);

        TooltipInstance.TryGetComponent<RectTransform>(out _tooltipTransform);
        TooltipOffset = new Vector3(_tooltipTransform.anchoredPosition.x / 2, _tooltipTransform.anchoredPosition.y / 2, 0);

        _tmAutoSize.OnAutoSizeComplete += _tmAutoSize_OnAutoSizeComplete;

        //TooltipInstance.transform.SetParent(GUICanvas.transform, false);
        TooltipInstance.SetActive(false);
    }

    private void _tmAutoSize_OnAutoSizeComplete()
    {
        //TooltipPlacer(_cachedSource, _tooltipTransform);
        //Had to change this to a flag because Unity throws errors when trying to do events while a UI update loop is in progress
        _updateTooltipFlag = true;
    }

    private void Update()
    {
        if (_tmAutoSize != null && _updateTooltipFlag)
        {
            TooltipPlacer(_cachedSource, _tooltipTransform, _cachedOffset);
            _updateTooltipFlag = false;
        }
    }
    //Adding "ignore auto size property as it seems to be breaking tooltip in the cable editor
    public void EnableTooltip(RectTransform sourceTransform, string tooltipText, Vector3 offset, bool performReparent = true)
    {
        if (sourceTransform == null)
            return;
        
        _performReparent = performReparent;

        var guiCanvas = sourceTransform.GetComponentInParent<Canvas>();
        if (guiCanvas == null)
            return;

        if (!guiCanvas.TryGetComponent<RectTransform>(out _guiRectTransform))
            return;

        TooltipText.text = tooltipText;

        if (TooltipInstance != null)
        {
            TooltipInstance.SetActive(true);
        }
        
        TooltipTarget = sourceTransform;
        //_tooltipTransform.SetParent(sourceTransform, false);
        //_tooltipTransform.anchoredPosition3D = TooltipOffset;

        //_tooltipTransform.SetParent(GUICanvas.transform, true);
        TooltipPlacer(sourceTransform, _tooltipTransform, offset);
        //LayoutRebuilder.MarkLayoutForRebuild(_tooltipTransform);
    }

    public void DisableTooltip()
    {
        TooltipTarget = null;
        TooltipActive = false;
        if (TooltipInstance != null)
            TooltipInstance.SetActive(false);
    }

    void TooltipPlacer(RectTransform sourceTransform, RectTransform tooltipTransform, Vector3 offset)
    {
        if (sourceTransform == null || tooltipTransform == null || _guiRectTransform == null)
            return;

        tooltipTransform.pivot = new Vector2(1, 1);
        //if(_performReparent)
        //    tooltipTransform.SetParent(sourceTransform, false);
        //tooltipTransform.anchoredPosition3D = Vector3.zero;
        Vector3[] tooltipWorldCorners = new Vector3[4];
        Vector3[] sourceWorldCorners = new Vector3[4];
        Vector3[] canvasWorldCorners = new Vector3[4];
        //Each corner provides its world space value. The returned array of 4 vertices is clockwise. It starts bottom left and rotates
        //to top left, then top right, and finally bottom right. Note that bottom left, for example, is an (x, y, z) vector with x being
        //left and y being bottom.
        
        sourceTransform.GetWorldCorners(sourceWorldCorners);
        _guiRectTransform.GetWorldCorners(canvasWorldCorners);
        tooltipTransform.position = sourceWorldCorners[0];
        tooltipTransform.GetWorldCorners(tooltipWorldCorners);
        
        Vector2 pivot = tooltipTransform.pivot;
        if (tooltipWorldCorners[0].x - canvasWorldCorners[0].x < 0)
        {
            pivot.x = 0;
        }
        
        //If passed, next we check if it's off the bottom of the screen        
        if(tooltipWorldCorners[0].y - canvasWorldCorners[0].y < 0)
        {
            pivot.y = 0;
        }
        
        int index = 0;
        switch (pivot)
        {
            case { x: 0, y: 0 }:
                offset.x *= -1;
                offset.y *= -1;
                index = 2;
                break;
            case { x: 0, y: 1 }:
                offset.x *= -1;
                index = 3;
                break;
            case { x: 1, y: 1 }:
                index = 0;
                break;
            case { x: 1, y: 0 }:
                offset.y *= -1;
                index = 1;
                break;
            default:
                break;
        }

        

        tooltipTransform.pivot = pivot;
        tooltipTransform.position = sourceWorldCorners[index] + offset;
        //tooltipTransform.anchoredPosition3D = Vector3.zero;
        //if(_performReparent)
        //tooltipTransform.SetParent(_guiRectTransform, true);

        _cachedSource = sourceTransform;
        _cachedOffset = offset;
    }
}
