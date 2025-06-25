using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public enum DragAndDropType
{
    Standard,
    GasReading,
    Delete,
    Time,
    Door,
    NumPad,
    None
}

[RequireComponent(typeof(RectTransform))]
public class MineMapDragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    public MappingVisualHandler MappingVisualHandler;
    public MineMapOnDrop OnDropRef;
    public string MapItemLabel = "";
    public string MapItemAddress = "";
    public GameObject ButtonPrefab;
    public bool IsGasReading = false;
    public DragAndDropType Type = DragAndDropType.Standard;
    
    [SerializeField] private Canvas canvas;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Vector2 offset;
    private Vector2 _priorPos;
    private VectorMineMap _mineMap;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        

        if(OnDropRef == null)
        {
            OnDropRef = GameObject.FindObjectOfType<MineMapOnDrop>();
        }
    }

    public void Start()
    {
        if (OnDropRef != null)
        {
            OnDropRef.TryGetComponent<VectorMineMap>(out _mineMap);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //_canvasGroup.blocksRaycasts = false;
        //_canvasGroup.alpha = 0.6f;
        //offset = eventData.position;
        //_priorPos = eventData.position;
        //Debug.Log($"Rect Xform: {_rectTransform.anchoredPosition}");

        OnDropRef.OnDropEnabled = true;
        OnDropRef.DraggedItemLabel = MapItemLabel;
        OnDropRef.DraggedItemAddress = MapItemAddress;
        //OnDropRef.IsGasReading = IsGasReading;
        OnDropRef.Type = Type;

        if (_mineMap != null)
        {
            _mineMap.InstantiateTempSymbol(MapItemAddress, Vector3.zero, Quaternion.identity);
        }

        //GameObject newButton = Instantiate(ButtonPrefab, transform.parent);
        //MineMapDragAndDrop dragScript = newButton.GetComponent<MineMapDragAndDrop>();
        //dragScript.canvas = canvas;
        //dragScript.OnDropRef = OnDropRef;
        //CanvasGroup cg = newButton.GetComponent<CanvasGroup>();
        //cg.alpha = 1;
        //cg.blocksRaycasts = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Vector3 pos = transform.position;
        //Debug.Log($"Event delta? {eventData.delta}");
        //pos.x += (eventData.delta.x * canvas.transform.localScale.x);
        //pos.y += (eventData.delta.y * canvas.transform.localScale.y);
        //new Vector2(eventData.position.x, eventData.position.y)

        if(MappingVisualHandler != null)
        {
            MappingVisualHandler.DestroyRadialButton();
        }

        var worldPos = eventData.pointerCurrentRaycast.worldPosition;

        //_rectTransform.position = worldPos;
        //_priorPos = eventData.position;

        if (_mineMap != null)
        {
            //var rt = _mineMap.transform as RectTransform;

            //Vector2 canvPos = rt.InverseTransformPoint(worldPos);
            //canvPos.x -= rt.rect.x;
            //canvPos.y -= rt.rect.y;

            //var symbolPos = _mineMap.CanvasSpaceToWorld(canvPos);
            var symbolPos = _mineMap.WorldCanvasSpaceToWorld(worldPos);
            _mineMap.PositionTempSymbol(symbolPos);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //_canvasGroup.blocksRaycasts = true;

        //Destroy(gameObject);


        AddMapSymbol(eventData);

        if (_mineMap != null)
        {
            _mineMap.ClearTempSymbol();
        }

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("On pointer down dragdrop");
    }

    public void OnDrop(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    private void AddMapSymbol(PointerEventData eventData)
    {
        if (MappingVisualHandler == null || MappingVisualHandler.VecMineMap == null)
            return;

        if (_mineMap == null)
        {
            Debug.LogError($"MineMapDragAndDrop: Couldn't find VectorMineMap on {gameObject.name}");
            return;
        }

        //var rt = MappingVisualHandler.VecMineMap.transform.parent as RectTransform;
        //if (rt == null)
        //    rt = MappingVisualHandler.VecMineMap.transform as RectTransform;

        var worldPos = eventData.pointerCurrentRaycast.worldPosition;

        //Vector2 pos = MappingVisualHandler.VecMineMap.transform.InverseTransformPoint(worldPos);
        //Vector3 pos3d = MappingVisualHandler.VecMineMap.transform.InverseTransformPoint(worldPos);

        var symbolPos = _mineMap.WorldCanvasSpaceToWorld(worldPos);

        //if (rt == null || !rt.rect.Contains(pos))
        //    return;

        if (!MappingVisualHandler.PointInDragDropBounds(worldPos))
            return;


        switch (Type)
        {
            case DragAndDropType.Standard:
                MappingVisualHandler.AddMapItemDragAndDrop(MapItemLabel, MapItemAddress, symbolPos);
                break;
            case DragAndDropType.GasReading:
                MappingVisualHandler.AddGasReadFromNetworkDragAndDrop("MFire Gas", symbolPos);
                break;
            case DragAndDropType.Delete:
                MappingVisualHandler.DeleteItemDragAndDrop(symbolPos);
                break;
            case DragAndDropType.Time:
                MappingVisualHandler.AddTimeDragAndDrop("", symbolPos, false);
                break;
            case DragAndDropType.Door:
                MappingVisualHandler.AddDoorDragDrop("D", symbolPos);
                break;
            case DragAndDropType.NumPad:
                MappingVisualHandler.SpawnRadialButtonDragAndDrop(worldPos);
                break;
            default:
                break;
        }
    }

}
