using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LayoutSeam : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,  IDragHandler
{

    public SeamLineType SeamLine;
    public Vector2 NewPosition;
    RectTransform _rt;
    WindowLayoutController _controller;

    bool _dragging;

    // ~h -> v
    private bool IsHorizontal()
    {
        return SeamLine == SeamLineType.Horizontal || SeamLine == SeamLineType.HorizontalLeft || SeamLine == SeamLineType.HorizontalRight;
    }

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _controller = GetComponentInParent<WindowLayoutController>();
    }

    void Start()
    {
        SetInitialPosition();
    }

    void SetInitialPosition()
    {
        switch (SeamLine)
        {
            case SeamLineType.Horizontal:
                NewPosition = new Vector2(_controller.WindowSpace.rect.width / 2, _controller.HorizontalSeamValue);
                break;

            case SeamLineType.HorizontalLeft:
                NewPosition = new Vector2(_controller.WindowSpace.rect.width / 2, _controller.HorizontalLeftSeamValue);
                break;

            case SeamLineType.HorizontalRight:
                NewPosition = new Vector2(_controller.WindowSpace.rect.width / 2, _controller.HorizontalRightSeamValue);
                break;

            case SeamLineType.Vertical:
                NewPosition = new Vector2(_controller.VerticalSeamValue, _controller.WindowSpace.rect.height / 2);
                break;

            case SeamLineType.VerticalLeft:
                NewPosition = new Vector2(_controller.VerticalLeftSeamValue, _controller.WindowSpace.rect.height / 2);
                break;

            case SeamLineType.VerticalRight:
                NewPosition = new Vector2(_controller.VerticalRightSeamValue, _controller.WindowSpace.rect.height / 2);
                break;
        }

    }

    void Update()
    {
        if (_dragging == true) { CheckEndDrag(); }
        AdjustHandle();
    }
    void OnDisable()
    {
        _dragging = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsHorizontal())
        {
            CursorImageController.instance.ChangeCursorImage(CursorImageController.CursorImage.Resize_NS);
        }
        else
        {
            CursorImageController.instance.ChangeCursorImage(CursorImageController.CursorImage.Resize_EW);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CursorImageController.instance.ChangeCursorImage(CursorImageController.CursorImage.Arrow);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _dragging = true;
        Vector2 delta = eventData.delta;
        CursorImageController.instance.ChangeCursorImage(CursorImageController.CursorImage.HandClosed);
        delta = Vector2.Scale(delta, new Vector2(1.0f / transform.lossyScale.x, 1.0f / transform.lossyScale.y));
        NewPosition = _rt.anchoredPosition + new Vector2(delta.x, delta.y);
    }

    private void CheckEndDrag()
    {
        if (Input.GetMouseButtonUp(0)) 
        {
            CursorImageController.instance.ChangeCursorImage(CursorImageController.CursorImage.Arrow);
            _dragging = false;
        }
    }

    /// <summary>
    /// Adjust position and scale of seam handle
    /// </summary>
    private void AdjustHandle()
    {
        float width = _controller.WindowSpace.rect.width;
        float height = _controller.WindowSpace.rect.height;
        
        float horizontalSeamMidpoint = 0;
        float horizontalLeftSeamMidpoint = 0;
        float horizontalRightSeamMidpoint = 0;
        
        float horizontalHandWidth = 0;
        float horizontalLeftHandWidth = 0;
        float horizontalRightHandWidth = 0;
        
        bool bottomMinimized = true;
        bool topMinimized = true;
        bool leftMinimized = true;
        bool rightMinimized = true;

        bool middleLeftMinimized = true;
        bool middleRightMinimized = true;

        bool topLeftMinimized = true;
        bool topRightMinimized = true;

        bool bottomLeftMinimized = true;
        bool bottomRightMinimized = true;

        //Set local variables according to layout
        switch (_controller.Layout)
        {
            case LayoutType.Standard:
                {
                    horizontalSeamMidpoint = (_controller.VerticalLeftSeamValue + _controller.VerticalRightSeamValue) / 2;
                    horizontalHandWidth = (_controller.VerticalRightSeamValue - _controller.VerticalLeftSeamValue);
                    leftMinimized = _controller.WindowMinimizedList[0];
                    bottomMinimized = _controller.WindowMinimizedList[1];
                    topMinimized = _controller.WindowMinimizedList[2];
                    rightMinimized = _controller.WindowMinimizedList[3];
                    _controller.VerticalSeamValue = width / 2;
                }
                break;

            case LayoutType.Column:
                {
                    leftMinimized = _controller.WindowMinimizedList[0];
                    middleLeftMinimized = _controller.WindowMinimizedList[1];
                    middleRightMinimized = _controller.WindowMinimizedList[2];
                    rightMinimized = _controller.WindowMinimizedList[3];
                }
                break;

            case LayoutType.Quad:
                {       // x position and width of left horizontal handles
                    horizontalLeftSeamMidpoint = (0 + _controller.VerticalSeamValue) / 2;
                    horizontalLeftHandWidth = _controller.VerticalSeamValue;

                    // x position and width of left horizontal handles
                    horizontalRightSeamMidpoint = (_controller.VerticalSeamValue + _controller.WindowSpace.rect.width) / 2;
                    horizontalRightHandWidth = width - _controller.VerticalSeamValue;

                    //set flags
                    topLeftMinimized = _controller.WindowMinimizedList[0];
                    bottomLeftMinimized = _controller.WindowMinimizedList[1];
                    topRightMinimized = _controller.WindowMinimizedList[2];
                    bottomRightMinimized = _controller.WindowMinimizedList[3];


                    //set combination flags
                    leftMinimized = topLeftMinimized && bottomLeftMinimized;
                    rightMinimized = topRightMinimized && bottomRightMinimized;
                    topMinimized = topLeftMinimized && topRightMinimized;
                    bottomMinimized = bottomLeftMinimized && bottomRightMinimized;
                }
                break;

            case LayoutType.Split_A:
                {                
                    // x position and width of left horizontal handles
                    horizontalLeftSeamMidpoint = (0 + _controller.VerticalSeamValue) / 2;
                    horizontalLeftHandWidth = _controller.VerticalSeamValue;

                    //left quads flags
                    topLeftMinimized = _controller.WindowMinimizedList[0];
                    bottomLeftMinimized = _controller.WindowMinimizedList[1];

                    //right columns flags
                    middleRightMinimized = _controller.WindowMinimizedList[2];
                    rightMinimized = _controller.WindowMinimizedList[3];

                    //set combination flags
                    leftMinimized = topLeftMinimized && bottomLeftMinimized;
                }
                    break;

            case LayoutType.Split_B:
                {
                    // x position and width of left horizontal handles
                    horizontalRightSeamMidpoint = (_controller.VerticalSeamValue + _controller.WindowSpace.rect.width) / 2;
                    horizontalRightHandWidth = width - _controller.VerticalSeamValue;

                    //leftColomn flags
                    leftMinimized = _controller.WindowMinimizedList[0];
                    middleLeftMinimized = _controller.WindowMinimizedList[1];

                    //right quads flags
                    topRightMinimized = _controller.WindowMinimizedList[2];
                    bottomRightMinimized = _controller.WindowMinimizedList[3];

                    //set combination flags
                    rightMinimized = topRightMinimized && bottomRightMinimized;
                }
                break;
        }

        switch (SeamLine)
        {
            case SeamLineType.Horizontal:
                if (topMinimized && bottomMinimized)
                {
                        //disable selection/drag
                }
                else if (topMinimized && !bottomMinimized)
                {
                    NewPosition = new Vector3(horizontalSeamMidpoint, height, 0); // set to top

                }
                else if (bottomMinimized && !topMinimized)
                {
                    NewPosition = new Vector3(horizontalSeamMidpoint, 0, 0); //set to bottom
                }
                else // clamp to active bounds
                {
                    NewPosition = new Vector3(horizontalSeamMidpoint, Mathf.Clamp(NewPosition.y, _controller.HorizontalBounds.x, _controller.HorizontalBounds.y), 0);
                }

                _controller.HorizontalSeamValue = NewPosition.y;
                _rt.anchoredPosition = NewPosition;
                _rt.sizeDelta = new Vector2 (horizontalHandWidth, _controller.HandleWidth);
                break;

            case SeamLineType.HorizontalRight:

                if (topRightMinimized && !bottomRightMinimized)
                {
                    NewPosition = new Vector3(horizontalRightSeamMidpoint, height, 0); // set to top

                }
                else if (bottomRightMinimized && !topRightMinimized)
                {
                    NewPosition = new Vector3(horizontalRightSeamMidpoint, 0, 0); //set to bottom
                }
                else // clamp to active bounds
                {
                    NewPosition = new Vector3(horizontalRightSeamMidpoint, Mathf.Clamp(NewPosition.y, _controller.HorizontalBounds.x, _controller.HorizontalBounds.y), 0);
                }

                _controller.HorizontalRightSeamValue = NewPosition.y;
                _rt.anchoredPosition = NewPosition;
                _rt.sizeDelta = new Vector2(horizontalRightHandWidth, _controller.HandleWidth);
                break;

            case SeamLineType.HorizontalLeft:

                if (topLeftMinimized && !bottomLeftMinimized)
                {
                    NewPosition = new Vector3(horizontalLeftSeamMidpoint, height, 0); // set to top

                }
                else if (bottomLeftMinimized && !topLeftMinimized)
                {
                    NewPosition = new Vector3(horizontalLeftSeamMidpoint, 0, 0); //set to bottom
                }
                else // clamp to active bounds
                {
                    NewPosition = new Vector3(horizontalLeftSeamMidpoint, Mathf.Clamp(NewPosition.y, _controller.HorizontalBounds.x, _controller.HorizontalBounds.y), 0);
                }

                _controller.HorizontalLeftSeamValue = NewPosition.y;
                _rt.anchoredPosition = NewPosition;
                _rt.sizeDelta = new Vector2(horizontalLeftHandWidth, _controller.HandleWidth);
                break;
            
            case SeamLineType.Vertical:

                /*
                if (middleLeftMinimized && middleRightMinimized)
                {
                    //_newPosition = new Vector3(width/2, height / 2, 0); // set to middle
                    _newPosition = new Vector3(Mathf.Clamp(_newPosition.x, _controller.VerticalBounds.x, _controller.VerticalBounds.y), height / 2, 0);
                }*/

                if (leftMinimized && middleLeftMinimized)
                {
                    NewPosition = new Vector3(0, height / 2, 0); // set to far left
                    //Debug.Log("Left :" + leftMinimized);
                    //Debug.Log("midLeft :" + middleLeftMinimized);

                }
                else if (rightMinimized  && middleRightMinimized)
                {
                    NewPosition = new Vector3(width, height / 2, 0); // set to far right
                    //Debug.Log("Right :" + rightMinimized);
                    //Debug.Log("midRight :" + middleRightMinimized);
                }
                else
                {
                    NewPosition = new Vector3(Mathf.Clamp(NewPosition.x, _controller.VerticalBounds.x, _controller.VerticalBounds.y), height / 2, 0);
                }

                //Debug.Log("topLeft :" + topLeftMinimized);
                //Debug.Log("bottomLeft :" + bottomLeftMinimized);
                //Debug.Log("topRight :" + topRightMinimized);
                //Debug.Log("bottomRight :" + bottomRightMinimized);

                _controller.VerticalSeamValue = NewPosition.x;
                _rt.anchoredPosition = NewPosition;
                _rt.sizeDelta = new Vector2(_controller.HandleWidth,_controller.WindowSpace.rect.height);
                break;

            case SeamLineType.VerticalLeft:
                
                if (leftMinimized)
                {
                    NewPosition = new Vector3(0, height / 2 , 0); // set to top

                }
                else if (middleLeftMinimized && topMinimized && bottomMinimized && rightMinimized && middleRightMinimized) // if everything else is minimzed
                {
                    NewPosition = new Vector3(width, height / 2, 0); //set to vert center
                }
                else if (middleLeftMinimized && topMinimized && bottomMinimized) // if center is minimized
                {
                    NewPosition = new Vector3(_controller.VerticalSeamValue, height / 2, 0); //set to vert center
                }
                else // clamp to active bounds
                {
                    NewPosition = new Vector3(Mathf.Clamp(NewPosition.x, _controller.VerticalLeftBounds.x, _controller.VerticalLeftBounds.y), height / 2, 0);
                }

                _controller.VerticalLeftSeamValue = NewPosition.x;
                _rt.anchoredPosition = NewPosition;
                _rt.sizeDelta = new Vector2(_controller.HandleWidth, _controller.WindowSpace.rect.height);
                break;

            case SeamLineType.VerticalRight:

                if (rightMinimized)
                {
                    NewPosition = new Vector3(width, height / 2, 0); // set to top

                }
                else if (middleRightMinimized && topMinimized && bottomMinimized && leftMinimized && middleLeftMinimized) // if everything else is minimzed
                {
                    NewPosition = new Vector3(0, height / 2, 0); //set to vert center
                }
                else if (middleRightMinimized && topMinimized && bottomMinimized) // if center is minimized
                {
                    NewPosition = new Vector3(_controller.VerticalSeamValue, height / 2, 0); //set to vert center
                }
                else // clamp to active bounds
                {
                    NewPosition = new Vector3(Mathf.Clamp(NewPosition.x, _controller.VerticalRightBounds.x, _controller.VerticalRightBounds.y), height / 2, 0);
                }

                _controller.VerticalRightSeamValue = NewPosition.x;
                _rt.anchoredPosition = NewPosition;
                _rt.sizeDelta = new Vector2(_controller.HandleWidth, _controller.WindowSpace.rect.height);
                break;
        }
    }

}
public enum SeamLineType
{
    Horizontal,
    HorizontalLeft,
    HorizontalRight,
    Vertical,
    VerticalLeft,
    VerticalRight,
    VerticalTop,
    VerticalBottom,

}
