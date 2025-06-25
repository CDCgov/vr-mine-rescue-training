using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class MenuTooltipClick : MenuTooltip, IPointerClickHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    [Tooltip("Useful for when the tooltip click event interferes with the parent interactable. This will parent it to the grandparent")]
    public bool ReparentOnStart = false;

    public Transform ReparentTarget;

    private RectTransform _rect;
    private LayoutElement _layoutElement;
    private VerticalLayoutGroup _verticalLayoutGroup;
    private Texture2D _texture;

    
    protected override void Start()
    {
        base.Start();
        TryGetComponent<RectTransform>(out  _rect);
        TryGetComponent<LayoutElement>(out _layoutElement);
        _verticalLayoutGroup = GetComponentInParent<VerticalLayoutGroup>();
        if (ReparentOnStart)
        {
            //if (ReparentTarget == null)
            //{
            //    this.transform.SetParent(transform.parent.parent, true);
            //    //this.transform.parent = transform.parent.parent;
            //}
            //else
            //{
            //    //this.transform.parent = ReparentTarget;
            //    this.transform.SetParent(ReparentTarget, true);
            //}
            //StartCoroutine(ReparentCoroutine());
        }

        //disable tooltip object if it has no content
        if (TooltipText == null)
        {
            //if (Tooltip == null)
            //{
            //    this.gameObject.SetActive(false);
            //}
            //else
            //{
            //    var textObj = Tooltip.GetComponentInChildren<TMP_Text>();
            //    if (textObj == null)
            //    {
            //        this.gameObject.SetActive(false);
            //    }
            //}
            if(TooltipManager == null)
            {
                TooltipManager = TooltipManager.GetDefault(gameObject);
            }
            TooltipManager.DisableTooltip();
        }

        _texture = (Texture2D)Resources.Load("Cursor_hand_point");

    }

    //IEnumerator ReparentCoroutine()
    //{
    //    yield return new WaitForEndOfFrame();
    //    if (ReparentTarget == null)
    //    {
    //        this.transform.SetParent(transform.parent.parent, true);
    //        //this.transform.parent = transform.parent.parent;
    //    }
    //    else
    //    {
    //        //this.transform.parent = ReparentTarget;
    //        this.transform.SetParent(ReparentTarget, true);

    //    }

    //}

    
    public override void OnPointerEnter(PointerEventData eventData)
    {
        _pointerOverObject = true;
        Cursor.SetCursor(_texture, new Vector2(200, 100), CursorMode.Auto);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("PointerClick");
        ShowTooltip();
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
