using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIMousePanAndZoom : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, IPointerClickHandler
{
    public RectTransform Target;
    public bool ResetTransformOnEnable = false;
    public float DragDeltaMultiplier = 1.0f;

    private Vector3 _initialPosition;
    private bool _initialized = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        var delta = eventData.delta;

        delta *= DragDeltaMultiplier;
        //Debug.Log($"UIDrag: {delta}");
        Target.position += new Vector3(delta.x, delta.y, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Vector2 pt;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Target, eventData.position, Camera.main, out pt);
            Debug.Log($"Local Point: {pt}");

            //Target.localPosition = _initialPosition - new Vector3(pt.x, pt.y, 0);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ResetTransform();
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        var delta = eventData.scrollDelta;
        //Debug.Log($"UIDrag scroll: {delta}");

        var oldScale = Target.localScale;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Target.parent.GetComponent<RectTransform>(), 
            Input.mousePosition, null /*null for screen space overlay*/, out var bp);
        //basePos = Target.parent.InverseTransformPoint(basePos);
        var basePos = new Vector3(bp.x, bp.y, 0);
        //Debug.Log($"BasePos: {basePos}");

        basePos.z = 0;
        var bpScaled = Target.localPosition - basePos;
        bpScaled.Scale(new Vector3(1.0f / oldScale.x, 1.0f / oldScale.y, 1.0f / oldScale.z));

        Target.localScale *= (1.0f + delta.y * 0.2f);
        Target.localPosition = Vector3.Scale(Target.localScale, bpScaled) + basePos;

        //move target position so the point under the cursor doesn't move
        

    }

    public void SetZoom(float zoom)
    {
        Target.localScale = new Vector3(zoom, zoom, zoom);
    }

    public void ResetTransform()
    {
        if (!_initialized || Target == null)
            return;

        Target.localPosition = _initialPosition;
        Target.localScale = Vector3.one;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Target == null)
            Target = GetComponent<RectTransform>();

        _initialPosition = Target.localPosition;
        //_rt = GetComponent<RectTransform>();

        SceneManager.sceneLoaded += OnSceneLoaded;

        _initialized = true;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnEnable()
    {
        if (!_initialized || !ResetTransformOnEnable) 
            return;


        ResetTransform();
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        ResetTransform();
    }
}
