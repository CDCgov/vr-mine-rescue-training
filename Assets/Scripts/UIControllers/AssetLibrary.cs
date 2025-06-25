using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NIOSH_EditorLayers;

public class AssetLibrary : MonoBehaviour
{
    [SerializeField] LayoutGroup layoutGroup;
    [SerializeField] bool autoAdjustSpacing = true;
    [SerializeField] float autoSpacingPercentage = 30;
    [SerializeField] RectTransform contentRt;
    [SerializeField] Vector2 padding;
    //ResizableWindow parentWindow;
    public RectTransform parentWindowRt;
    bool parentWindowAction;
    [SerializeField] ScrollRect scroll;
    [SerializeField] RectTransform viewportRt;
    [SerializeField] TextMeshProUGUI header;
    [SerializeField] TMP_InputField FilterField;
    LayerManager.EditorLayer curLayer = LayerManager.EditorLayer.Mine;

    public bool AdaptiveGrid;

    // Start is called before the first frame update
    void Start()
    {
        parentWindowAction= false;
        //parentWindow = GetComponent<ResizableWindow>();
        /*
        if (parentWindow)
        { 
            parentWindow.onChangePerformingAction.AddListener(ParentWindowAction);
            parentWindowRt = parentWindow.rt;
        }*/
        LayerManager.Instance.layerChanged += OnLayerChanged;
        if(FilterField != null)
        {
            FilterField.onValueChanged.AddListener(OnFilterChanged);
        }
    }
    void OnDestroy()
    {
        if(LayerManager.Instance != null) LayerManager.Instance.layerChanged -= OnLayerChanged;
        //if (parentWindow) parentWindow.onChangePerformingAction.RemoveListener(ParentWindowAction);
        if(FilterField != null)
        {
            FilterField.onValueChanged.RemoveListener(OnFilterChanged);
        }
    }
    private void LateUpdate()
    {
        if (parentWindowRt.hasChanged) { WindowChanged(); }
    }
    public void Initialize()
    {
        ChangeGridAlignment();
    }

    public void SetCellSize(float value)
    {
        if (layoutGroup is GridLayoutGroup)
        {
            var layoutGrid = (GridLayoutGroup)layoutGroup;
            layoutGrid.cellSize = new Vector2 (value,value);
            if (autoAdjustSpacing) AdjustSpacing(layoutGrid);
        }
        ChangeGridAlignment();

    }
    void AdjustSpacing(GridLayoutGroup layoutGrid)
    {
        layoutGrid.spacing = (autoSpacingPercentage / 100) * layoutGrid.cellSize;
    }
    void WindowChanged()
    {
        var layoutGrid = (GridLayoutGroup)layoutGroup;
        if (autoAdjustSpacing) AdjustSpacing(layoutGrid);

        if(!AdaptiveGrid) return;

        ChangeGridAlignment();
        scroll.horizontalScrollbar.gameObject.SetActive(false);
        scroll.verticalScrollbar.gameObject.SetActive(false);
        scroll.enabled = false;
        StartCoroutine(HandleScroll());
        parentWindowRt.hasChanged = false;
        

    }
    void ParentWindowAction(bool performingAction)
    {
        if (parentWindowAction == performingAction) return;
        
        ChangeGridAlignment();

        if (performingAction)
        {
            scroll.horizontalScrollbar.gameObject.SetActive(false);
            scroll.verticalScrollbar.gameObject.SetActive(false);
            scroll.enabled = false;
        }
        else
        {
            StartCoroutine(HandleScroll());
        }
            parentWindowAction = performingAction;
    }
    IEnumerator HandleScroll()
    {
        // we need to delay to resolve race conditions between layout componenets
        yield return new WaitForSeconds(.25f);

        //scroll.horizontalScrollbar.gameObject.SetActive(true);
        //scroll.verticalScrollbar.gameObject.SetActive(true);
        scroll.enabled = true;
        //scroll.CalculateLayoutInputHorizontal();
        //scroll.CalculateLayoutInputVertical();
    }


    void ChangeGridAlignment()
    {
        if (!AdaptiveGrid) return;

        if (layoutGroup is GridLayoutGroup)
        {
            
            var layoutGrid = (GridLayoutGroup)layoutGroup;

            if(viewportRt.rect.width < viewportRt.rect.height)
            {
                float cellWidth = layoutGrid.cellSize.x  + (layoutGrid.spacing.x);
                layoutGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
                layoutGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layoutGrid.constraintCount = ((int)((viewportRt.rect.width - layoutGrid.padding.left - layoutGrid.padding.right + layoutGrid.spacing.x) / cellWidth));
            }
            else 
            {
                float cellHeight = layoutGrid.cellSize.y + (layoutGrid.spacing.y);
                layoutGrid.startAxis = GridLayoutGroup.Axis.Vertical;
                layoutGrid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                layoutGrid.constraintCount = ((int)((viewportRt.rect.height - layoutGrid.padding.top - layoutGrid.padding.bottom + layoutGrid.spacing.y) / cellHeight));
            }
        
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRt);
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentWindowRt);

        }
    }

    void OnFilterChanged(string entry)
    {
        string lowerCase = entry.ToLower();
        foreach (RectTransform child in contentRt)
        {
            string childLowerCaseName = child.GetComponent<AssetUIObject>().GetName().ToLower();
            if (string.IsNullOrEmpty(entry))
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                if (childLowerCaseName.Contains(lowerCase))
                {
                    child.gameObject.SetActive(true);
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRt);
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentWindowRt);
    }


    void OnLayerChanged(LayerManager.EditorLayer newLayer)
    {
        curLayer = newLayer;

        if(FilterField != null)
        {
            FilterField.SetTextWithoutNotify("");
        }

        ChangeHeader();

    }
    void ChangeHeader()
    {
        string newHeader = "";

        switch (curLayer)
        {
            case LayerManager.EditorLayer.Mine:
                newHeader = "Environment Library";
                break;
            case LayerManager.EditorLayer.Object:
                newHeader = "Object Library";
                break;
            case LayerManager.EditorLayer.Ventilation:
                newHeader = "Ventilation Library";
                break;
            case LayerManager.EditorLayer.VentilationBlockers:
                newHeader = "Ventilation Blockers Library";
                break;
            case LayerManager.EditorLayer.Cables:
                newHeader = "Cable Library";
                break;
            case LayerManager.EditorLayer.Curtains:
                newHeader = "Curtain Library";
                break;
            case LayerManager.EditorLayer.SceneControls:
                newHeader = "Scene Controls Library";
                break;

        }
        header.text = newHeader;
    }
}
