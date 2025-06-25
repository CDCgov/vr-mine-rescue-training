using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public enum LayoutType
{
    Standard,
    Column,
    Quad,
    Split_A,
    Split_B,
}
public enum Windows
{
    Hierarchy,
    Viewport,
    Library,
    Inspector,
}
public class WindowLayoutController : MonoBehaviour
{
    public LayoutType Layout;

    public TMP_Dropdown LayoutDropdown;
    public RectTransform WindowSpace;
    public List<RectTransform> WindowList;
    public List<bool> WindowMinimizedList;
    
    // public List<RectTransform> WindowList = new List<RectTransform>();
    public GameObject SeamHandle_Horizontal;
    public GameObject SeamHandle_HorizontalLeft;
    public GameObject SeamHandle_HorizontalRight;
    public GameObject SeamHandle_Vertical;
    public GameObject SeamHandle_VerticalLeft;
    public GameObject SeamHandle_VerticalRight;

    // values for controlling link lines
    [Tooltip("thickness of the collider for grab zones")]
    
    public float HandleWidth = 30;
    public float VericalSeamGap_Quad = 300;
    public float VericalSeamGap_Column = 100;
    public float HorizontalSeamValue;
    public float HorizontalLeftSeamValue;
    public float HorizontalRightSeamValue;
    public float VerticalSeamValue;
    public float VerticalLeftSeamValue;
    public float VerticalRightSeamValue;
    public Vector2 HorizontalBounds;
    public Vector2 VerticalBounds;
    public Vector2 VerticalLeftBounds;
    public Vector2 VerticalRightBounds;
    public event Action onLayoutChanged;

    private LayoutType _lastLayout;
    private LayoutSeam _seamHandle_Horizontal;
    private LayoutSeam _seamHandle_HorizontalLeft;
    private LayoutSeam _seamHandle_HorizontalRight;
    private LayoutSeam _seamHandle_Vertical;
    private LayoutSeam _seamHandle_VerticalLeft;
    private LayoutSeam _seamHandle_VerticalRight;

    const float _standardStartHorz = 0.3333333f;
    const float _standardStartVert = 0.5f;
    const float _standardStartVertRight = 0.77f;
    const float _standardStartVertLeft = 0.142f;

    const float _columnStartVert = 0.3333333f;
    const float _columnStartVertRight = 0.77f;
    const float _columnStartVertLeft = 0.142f;

    const float _quadStartVert = 0.25f;
    const float _quadStartHorzRight = 0.325f;
    const float _quadStartHorzLeft = 0.5f;

    const float _splitAStartVert = 0.25f;
    const float _splitAStartHorzLeft = 0.5f;
    const float _splitAStartVertRight = 0.77f;

    const float _splitBStartVert = 0.3333333f;
    const float _splitBStartVertLeft = 0.142f;
    const float _splitBStartHorzRight = 0.325f;



    public void Awake()
    {
        LayoutDropdown.onValueChanged.AddListener(ChangeLayout);
        SetComponents();
        NewLayout();
    }

    public void Start()
    {
        foreach(RectTransform window in WindowList)
        {
            var min = window.GetComponent<MinimizableWindowController>();
            min.StateChanged += GetWindowState;
        }
    }

    public void LateUpdate()
    {
        if(Layout != _lastLayout)
        {
            NewLayout();
        }
        SetWindowLayout();
    }
    private void SetComponents()
    {
        _seamHandle_Horizontal = SeamHandle_Horizontal.GetComponent<LayoutSeam>();
        _seamHandle_HorizontalLeft = SeamHandle_HorizontalLeft.GetComponent<LayoutSeam>();
        _seamHandle_HorizontalRight = SeamHandle_HorizontalRight.GetComponent<LayoutSeam>();
        _seamHandle_Vertical = SeamHandle_Vertical.GetComponent<LayoutSeam>();
        _seamHandle_VerticalLeft = SeamHandle_VerticalLeft.GetComponent<LayoutSeam>();
        _seamHandle_VerticalRight = SeamHandle_VerticalRight.GetComponent<LayoutSeam>();
    }


    //set bounds according to window space size
    private void SetStandardBounds()
    {
        HorizontalBounds = new Vector2(WindowSpace.rect.height / 4, (WindowSpace.rect.height / 4) * 3);
        VerticalBounds = new Vector2((WindowSpace.rect.width / 8) * 3, (WindowSpace.rect.width / 8) * 5);
        VerticalLeftBounds = new Vector2((WindowSpace.rect.width / 8) * 1, (WindowSpace.rect.width / 8) * 3);
        VerticalRightBounds = new Vector2((WindowSpace.rect.width / 8) * 5, (WindowSpace.rect.width / 8) * 7);
    }

    public void GetWindowState(bool minimized, RectTransform rt)
    {
        int index = WindowList.IndexOf(rt);
        WindowMinimizedList[index] = minimized;
    }

    private void ChangeLayout(int i)
    {
        Layout = (LayoutType)i;
    }

    public void NewLayout()
    {
        switch (Layout)
        {
            case LayoutType.Standard:
                SeamHandle_Horizontal.SetActive(true);
                SeamHandle_HorizontalLeft.SetActive(false);
                SeamHandle_HorizontalRight.SetActive(false);
                SeamHandle_Vertical.SetActive(false);
                SeamHandle_VerticalLeft.SetActive(true);
                SeamHandle_VerticalRight.SetActive(true);
                SetStandardBounds();
                SetStandardValues();
                SetHandlePositions();
                break;

            case LayoutType.Column:
                SeamHandle_Horizontal.SetActive(false);
                SeamHandle_HorizontalLeft.SetActive(false);
                SeamHandle_HorizontalRight.SetActive(false);
                SeamHandle_Vertical.SetActive(true);
                SeamHandle_VerticalLeft.SetActive(true);
                SeamHandle_VerticalRight.SetActive(true);
                SetColumnBounds();
                SetColumnValues();
                SetHandlePositions();

                break;

            case LayoutType.Quad:
                SeamHandle_Horizontal.SetActive(false);
                SeamHandle_HorizontalLeft.SetActive(true);
                SeamHandle_HorizontalRight.SetActive(true);
                SeamHandle_Vertical.SetActive(true);
                SeamHandle_VerticalLeft.SetActive(false);
                SeamHandle_VerticalRight.SetActive(false);
                SetQuadBounds();
                SetQuadValues();
                SetHandlePositions();

                break;

            case LayoutType.Split_A:
                SeamHandle_Horizontal.SetActive(false);
                SeamHandle_HorizontalLeft.SetActive(true);
                SeamHandle_HorizontalRight.SetActive(false);
                SeamHandle_Vertical.SetActive(true);
                SeamHandle_VerticalLeft.SetActive(false);
                SeamHandle_VerticalRight.SetActive(true);
                SetSplitABounds();
                SetSplitAValues();
                SetHandlePositions();

                break;

            case LayoutType.Split_B:
                SeamHandle_Horizontal.SetActive(false);
                SeamHandle_HorizontalLeft.SetActive(false);
                SeamHandle_HorizontalRight.SetActive(true);
                SeamHandle_Vertical.SetActive(true);
                SeamHandle_VerticalLeft.SetActive(true);
                SeamHandle_VerticalRight.SetActive(false);
                SetSplitBBounds();
                SetSplitBValues();
                SetHandlePositions();

                break;

        }
        _lastLayout = Layout;
        //onLayoutChanged?.Invoke();
        StartCoroutine(InvokeLayoutChangeNextFrame());
    }

    IEnumerator InvokeLayoutChangeNextFrame()
    {
        yield return 0;
        onLayoutChanged?.Invoke();
    }

    public void SetWindowLayout()
    {
        switch (Layout)
        {
            case LayoutType.Standard:
                SetStandardBounds();
                StandardLayout();
                break;

            case LayoutType.Column:
                SetColumnBounds();
                ColumnLayout();
                break;

            case LayoutType.Quad:
                SetQuadBounds();
                QuadLayout();
                break;

            case LayoutType.Split_A:
                SetSplitABounds();
                SplitALayout();
                break;

            case LayoutType.Split_B:
                SetSplitBBounds();
                SplitBLayout();
                break;
        }
    }

    public void StandardLayout()
    {
        //left
        SetWindowEdges(WindowList[0], 0, WindowSpace.rect.width - VerticalLeftSeamValue, 0, 0);

        //bottom
        SetWindowEdges(WindowList[1], VerticalLeftSeamValue, WindowSpace.rect.width - VerticalRightSeamValue, 0, WindowSpace.rect.height - HorizontalSeamValue);
        
        //top
        SetWindowEdges(WindowList[2], VerticalLeftSeamValue, WindowSpace.rect.width - VerticalRightSeamValue, HorizontalSeamValue, 0);
        
        //right
        SetWindowEdges(WindowList[3], VerticalRightSeamValue, 0, 0, 0);


    }

    public void ColumnLayout()
    {
        //left
        SetWindowEdges(WindowList[0], 0, WindowSpace.rect.width - VerticalLeftSeamValue, 0, 0);
        
        //middelLeft
        SetWindowEdges(WindowList[1], VerticalLeftSeamValue, WindowSpace.rect.width - VerticalSeamValue, 0, 0);

        //middleRight
        SetWindowEdges(WindowList[2], VerticalSeamValue, WindowSpace.rect.width - VerticalRightSeamValue, 0, 0);

        //right
        SetWindowEdges(WindowList[3], VerticalRightSeamValue, 0, 0, 0);

    }

    public void QuadLayout()
    {
        //topleft
        SetWindowEdges(WindowList[0], 0, WindowSpace.rect.width - VerticalSeamValue, HorizontalLeftSeamValue, 0);
        
        //bottomLeft
        SetWindowEdges(WindowList[1], 0, WindowSpace.rect.width - VerticalSeamValue, 0, WindowSpace.rect.height - HorizontalLeftSeamValue);
        
        //topRight
        SetWindowEdges(WindowList[2], VerticalSeamValue, 0, HorizontalRightSeamValue, 0);
        
        //bottomRight
        SetWindowEdges(WindowList[3], VerticalSeamValue, 0, 0, WindowSpace.rect.height - HorizontalRightSeamValue);
    }

    public void SplitALayout()
    {
        //topleft
        SetWindowEdges(WindowList[0], 0, WindowSpace.rect.width - VerticalSeamValue, HorizontalLeftSeamValue, 0);

        //bottomLeft
        SetWindowEdges(WindowList[1], 0, WindowSpace.rect.width - VerticalSeamValue, 0, WindowSpace.rect.height - HorizontalLeftSeamValue);

        //middleRight
        SetWindowEdges(WindowList[2], VerticalSeamValue, WindowSpace.rect.width - VerticalRightSeamValue, 0, 0);

        //right
        SetWindowEdges(WindowList[3], VerticalRightSeamValue, 0, 0, 0);
    }

    public void SplitBLayout()
    {
        //left
        SetWindowEdges(WindowList[0], 0, WindowSpace.rect.width - VerticalLeftSeamValue, 0, 0);

        //middelLeft
        SetWindowEdges(WindowList[1], VerticalLeftSeamValue, WindowSpace.rect.width - VerticalSeamValue, 0, 0);

        //topRight
        SetWindowEdges(WindowList[2], VerticalSeamValue, 0, HorizontalRightSeamValue, 0);

        //bottomRight
        SetWindowEdges(WindowList[3], VerticalSeamValue, 0, 0, WindowSpace.rect.height - HorizontalRightSeamValue);
    }

    private void SetWindowEdges(RectTransform layoutZone, float leftEdge, float rightEdge, float bottomEdge, float topEdge)
    {
        layoutZone.SetTopOffset(topEdge);
        layoutZone.SetBottomOffset(bottomEdge);
        layoutZone.SetRightOffset(rightEdge);
        layoutZone.SetLeftOffset(leftEdge);
    }

    public void SwapWindowZone(RectTransform sourceWindow, RectTransform targetWindow)
    {
        int targetIndex = WindowList.IndexOf(targetWindow);
        int sourceIndex = WindowList.IndexOf(sourceWindow);
        WindowList[targetIndex] = sourceWindow;
        WindowList[sourceIndex] = targetWindow;
    }

    private void SetColumnBounds()
    {
        HorizontalBounds = Vector2.zero;
        VerticalLeftBounds = new Vector2((WindowSpace.rect.width / 8) * 1, (WindowSpace.rect.width / 8) * 2);
        VerticalRightBounds = new Vector2((WindowSpace.rect.width / 8) * 5, (WindowSpace.rect.width / 8) * 7);
        VerticalBounds = new Vector2(VerticalLeftBounds.y + VericalSeamGap_Column, VerticalRightBounds.x - VericalSeamGap_Column);
        //VerticalBounds = new Vector2(VerticalLeftSeamValue + VericalSeamGap_Column, VerticalRightSeamValue - VericalSeamGap_Column);
    }

    private void SetQuadBounds()
    {
        HorizontalBounds = new Vector2(WindowSpace.rect.height / 4, (WindowSpace.rect.height / 4) * 3);
        VerticalLeftBounds = Vector2.zero;
        VerticalRightBounds = new Vector2(WindowSpace.rect.width, WindowSpace.rect.width);
        VerticalLeftSeamValue = 0;
        VerticalRightSeamValue = WindowSpace.rect.width;
        VerticalBounds = new Vector2(VerticalLeftSeamValue + VericalSeamGap_Quad, VerticalRightSeamValue - VericalSeamGap_Quad);
    }

    private void SetSplitABounds()
    {
        HorizontalBounds = new Vector2(WindowSpace.rect.height / 4, (WindowSpace.rect.height / 4) * 3);
        VerticalLeftBounds = Vector2.zero;
        VerticalRightBounds = new Vector2((WindowSpace.rect.width / 8) * 5, (WindowSpace.rect.width / 8) * 7);
        VerticalBounds = new Vector2(VerticalLeftBounds.y + VericalSeamGap_Quad, VerticalRightBounds.x - VericalSeamGap_Column);

    }
    
    private void SetSplitBBounds()
    {
        HorizontalBounds = new Vector2(WindowSpace.rect.height / 4, (WindowSpace.rect.height / 4) * 3);
        VerticalLeftBounds = new Vector2((WindowSpace.rect.width / 8) * 1, (WindowSpace.rect.width / 8) * 2);
        VerticalRightBounds = new Vector2(WindowSpace.rect.width, WindowSpace.rect.width);//
        VerticalBounds = new Vector2(VerticalLeftBounds.y + VericalSeamGap_Column, VerticalRightBounds.x - VericalSeamGap_Quad);
    }

    public void SetStandardValues()
    {
        HorizontalSeamValue = WindowSpace.rect.height * _standardStartHorz;
        VerticalSeamValue = WindowSpace.rect.width * _standardStartVert;
        VerticalLeftSeamValue = WindowSpace.rect.width * _standardStartVertLeft;
        VerticalRightSeamValue = WindowSpace.rect.width * _standardStartVertRight;
    }

    public void SetColumnValues()
    {
        VerticalSeamValue = WindowSpace.rect.width * _columnStartVert;
        VerticalLeftSeamValue = WindowSpace.rect.width * _columnStartVertLeft;
        VerticalRightSeamValue = WindowSpace.rect.width * _columnStartVertRight;
    }

    public void SetQuadValues()
    {
        VerticalSeamValue = WindowSpace.rect.width * _quadStartVert;
        HorizontalLeftSeamValue = WindowSpace.rect.height * _quadStartHorzLeft;
        HorizontalRightSeamValue = WindowSpace.rect.height * _quadStartHorzRight;
    }

    public void SetSplitAValues()
    {
        VerticalSeamValue = WindowSpace.rect.width * _splitAStartVert;
        VerticalRightSeamValue = WindowSpace.rect.width * _splitAStartVertRight;
        HorizontalLeftSeamValue = WindowSpace.rect.height * _splitAStartHorzLeft;
    }
    public void SetSplitBValues()
    {
        VerticalSeamValue = WindowSpace.rect.width * _splitBStartVert;
        VerticalLeftSeamValue = WindowSpace.rect.width * _splitBStartVertLeft;
        HorizontalRightSeamValue = WindowSpace.rect.height * _splitBStartHorzRight;
    }



    /// <summary>
    /// position handles after setting values to prevent values from shift
    /// </summary>
    public void SetHandlePositions()
    {
        _seamHandle_Horizontal.NewPosition = new Vector2(WindowSpace.rect.width / 2, HorizontalSeamValue);
        _seamHandle_HorizontalLeft.NewPosition = new Vector2(WindowSpace.rect.width / 2, HorizontalLeftSeamValue);
        _seamHandle_HorizontalRight.NewPosition = new Vector2(WindowSpace.rect.width / 2, HorizontalRightSeamValue);
        _seamHandle_Vertical.NewPosition = new Vector2(VerticalSeamValue, WindowSpace.rect.height / 2);
        _seamHandle_VerticalRight.NewPosition = new Vector2(VerticalRightSeamValue, WindowSpace.rect.height / 2);
        _seamHandle_VerticalLeft.NewPosition = new Vector2(VerticalLeftSeamValue, WindowSpace.rect.height / 2);
    }
}
