using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveableWindow : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler
{
    public int MinimizedHeight = 25;

    protected RectTransform _rt;
    protected bool _ignoreNextClick = false;

    protected bool _minimized = false;

    protected Vector2 _originalSize;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _rt = GetComponent<RectTransform>();
        _originalSize = _rt.sizeDelta;
    }

    public void ToggleMinimized()
    {
        Minimize(!_minimized);
    }

    public void Minimize(bool minimize)
    {
        if (_minimized != minimize)
        {
            _minimized = minimize;

            if (_minimized)
            {
                _rt.sizeDelta = new Vector2(_originalSize.x, MinimizedHeight);
            }
            else
            {
                _rt.sizeDelta = _originalSize;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_ignoreNextClick)
        {
            _ignoreNextClick = false;
            return;
        }

        //ToggleMinimized();

        //_rt.sizeDelta = _rt.sizeDelta - new Vector2(0, 20);

        //Debug.Log(eventData.ToString());
        //Debug.Log("CLICKED");
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        
        //don't process click if there is any drag
        _ignoreNextClick = true;

        //Debug.Log(eventData.ToString());

        var delta = eventData.delta;
        delta = Vector2.Scale(delta, new Vector2(1.0f / transform.lossyScale.x, 
            1.0f / transform.lossyScale.y));
        _rt.anchoredPosition += delta;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }
}
