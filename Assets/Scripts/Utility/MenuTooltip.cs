using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public const float TooltipDelay = 0.5f;
    public TextMeshProUGUI TooltipText;
    [TextArea(2,4)]
    public string TooltipTextString;
    public string TooltipPrefabResource = "GUI\\DefaultTooltip";
    public Vector3 positionOffset = Vector3.zero;
    public bool HideTooltip = false; // used to prevent tooltip from showing
    public bool TooltipOverrideKeepInRect = false;
    public TooltipManager TooltipManager;
    public RectTransform TooltipRectOverride;
    public bool PerformReparent = true;
    protected float _startTime = 0;
    protected bool _tooltipEnabled = false; 
    protected Transform _origParent;
    protected bool _pointerOverObject = false;
    protected Canvas _canvas;
    protected RectTransform _parentRect;
    protected GameObject _tooltipInstance;


    private RectTransform _instanceTransform;
   
    

    //private void Start()
    //{
    //    _origParent = Tooltip.transform.parent;
    //}
    

    //private void Update()
    //{
    //    if (_tooltipEnabled && Time.time > _startTime)
    //    {
    //        if (!Tooltip.activeSelf)
    //        {
    //            //Tooltip.transform.parent = transform.parent;
    //            //Tooltip.transform.SetAsLastSibling();
    //            Tooltip.SetActive(true);
    //        }
    //    }
    //}
    //public void OnStartTooltip()
    //{
    //    _startTime = Time.time + TooltipDelay;
    //    _tooltipEnabled = true;
    //}

    //public void OnExitTooltip()
    //{
    //    Tooltip.SetActive(false);
    //    _tooltipEnabled = false;
    //}

    //public void OnDisable()
    //{
    //    Tooltip.SetActive(false);
    //    _tooltipEnabled = false;
    //}

    //public void OnEnable()
    //{
    //    _tooltipEnabled = false;
    //}

    public void SetTooltipText(string ttText)
    {
        TooltipTextString = ttText;

        if (TooltipText != null)
        {
            TooltipText.text = ttText;
        }

        if (_tooltipInstance != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipInstance.GetComponent<RectTransform>());

        
    }

    protected virtual void Start()
    {
        _canvas = GetComponentInParent<Canvas>();
        _parentRect = _canvas.GetComponent<RectTransform>();

        if (TooltipRectOverride != null)
        {
            _instanceTransform = TooltipRectOverride;
        }
        else
        {
            TryGetComponent<RectTransform>(out _instanceTransform);
        }
        
        //CreateTooltip();
        if (TooltipManager == null)
            TooltipManager = TooltipManager.GetDefault(gameObject);
    }

    protected void Update()
    {
        if (_tooltipInstance != null && _tooltipInstance.activeSelf)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Input.mousePosition))
            {
                //_tooltipInstance.SetActive(false);
                TooltipManager.DisableTooltip();
            }
        }
    }

    protected void OnDisable()
    {
        if (_tooltipInstance != null)
            _tooltipInstance.SetActive(false);
        _pointerOverObject = false;
        CancelInvoke();
    }

    protected void OnDestroy()
    {
        _pointerOverObject = false;
        if (_tooltipInstance != null)
        {
            Destroy(_tooltipInstance);
        }

        CancelInvoke();
    }

    protected void CreateTooltip()
    {
        //var prefab = Resources.Load<GameObject>(TooltipPrefabResource);
        //_tooltipInstance = GameObject.Instantiate(prefab);
        //TooltipText = _tooltipInstance.GetComponentInChildren<TextMeshProUGUI>();
        //TooltipText.text = TooltipTextString;

        
        //_tooltipInstance.transform.SetParent(_canvas.transform, false);
        ////LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipInstance.GetComponent<RectTransform>());
        //LayoutRebuilder.MarkLayoutForRebuild(_tooltipInstance.GetComponent<RectTransform>());
        ////LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipInstance.GetComponent<RectTransform>());
        ////LayoutRebuilder.ForceRebuildLayoutImmediate(_parentRect);
        //_tooltipInstance.SetActive(false);

        
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log($"Moused over: {transform.name}");
        _pointerOverObject = true;
        Invoke(nameof(ShowTooltip), TooltipDelay);
    }

    protected void KeepRectInBounds(RectTransform bounds, RectTransform child, float border)
    {
        if (TooltipOverrideKeepInRect)
        {
            return;
        }
        float minX = bounds.rect.xMin - child.rect.xMin + border;
        float maxX = bounds.rect.xMax - child.rect.xMax - border;

        float minY = bounds.rect.yMin - child.rect.yMin + border;
        float maxY = bounds.rect.yMax - child.rect.yMax - border;

        var pos = child.localPosition;

        if (pos.x > maxX)
            pos.x = maxX;
        else if (pos.x < minX)
            pos.x = minX;

        if (pos.y > maxY)
            pos.y = maxY;
        else if (pos.y < minY)
            pos.y = minY;

        child.localPosition = pos;
    }

    protected void ShowTooltip()
    {
        if (!_pointerOverObject || HideTooltip)
            return;

        //if (_tooltipInstance == null || _tooltipInstance.activeSelf)
        //    return;

        if (TooltipTextString == null || TooltipTextString.Length <= 0)
        {
            //if (Tooltip != null)
            //{
            //    var textObj = Tooltip.GetComponentInChildren<TMP_Text>();
            //    if (textObj != null)
            //        SetTooltipText(textObj.text);
            //}
            return;
        }

        //Vector2 pt;
        //if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, Input.mousePosition + positionOffset, null, out pt))
        //{
        //    _tooltipInstance.transform.localPosition = (pt + new Vector2(8,8));
        //    _tooltipInstance.SetActive(true);
        //    LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipInstance.GetComponent<RectTransform>());
        //    KeepRectInBounds(_parentRect, _tooltipInstance.GetComponent<RectTransform>(), 8);

        //    var rt = _tooltipInstance.GetComponent<RectTransform>();
        //    Debug.Log($"Showing tooltip width: {rt.rect.width} {rt.sizeDelta}");
        //}
        if (TooltipManager == null)
        {
            TooltipManager = TooltipManager.GetDefault(gameObject);
        }
        TooltipManager.EnableTooltip(_instanceTransform, TooltipTextString, positionOffset, PerformReparent);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log($"PointerExit: {gameObject.name}");
        _pointerOverObject = false;
        //_tooltipInstance.SetActive(false);
        if(TooltipManager != null)
            TooltipManager.DisableTooltip();
    }
    
    public void ForceCloseTooltip()
    {
        _pointerOverObject = false;
        if (TooltipManager != null)
        {
            TooltipManager.DisableTooltip();
        }
    }
}
