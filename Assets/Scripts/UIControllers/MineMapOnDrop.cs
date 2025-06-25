using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class MineMapOnDrop : MonoBehaviour//, IDropHandler
{
    public MappingVisualHandler MappingVisualHandler;
    public DragAndDropType Type = DragAndDropType.None;
    public bool OnDropEnabled = false;
    //public bool IsGasReading = false;
    public string DraggedItemLabel = "";
    public string DraggedItemAddress = "";

    //public void OnDrop(PointerEventData eventData)
    //{
    //    if (OnDropEnabled)
    //    {
    //        Vector2 pos = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
    //        Vector3 pos3d = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);

    //        switch (Type)
    //        {
    //            case DragAndDropType.Standard:
    //                MappingVisualHandler.AddMapItemDragAndDrop(DraggedItemLabel, DraggedItemAddress, pos);
    //                break;
    //            case DragAndDropType.GasReading:
    //                MappingVisualHandler.AddGasReadFromNetworkDragAndDrop("MFire Gas", pos);
    //                break;
    //            case DragAndDropType.Delete:
    //                MappingVisualHandler.DeleteItemDragAndDrop(pos);
    //                break;
    //            case DragAndDropType.Time:
    //                MappingVisualHandler.AddTimeDragAndDrop("", pos, false);
    //                break;
    //            case DragAndDropType.Door:
    //                MappingVisualHandler.AddDoorDragDrop("D", pos);
    //                break;
    //            case DragAndDropType.NumPad:
    //                MappingVisualHandler.SpawnRadialButtonDragAndDrop(pos, pos3d);
    //                break;
    //            default:
    //                break;
    //        }
    //        //if (IsGasReading)
    //        //{
    //        //    MappingVisualHandler.AddGasReadFromNetworkDragAndDrop("MFire Gas", pos);
    //        //}
    //        //else
    //        //{
    //        //    Debug.Log($"On Drop: {gameObject.name}, loc: {eventData.position}");
    //        //    MappingVisualHandler.AddMapItemDragAndDrop(DraggedItemLabel, DraggedItemAddress, pos);
    //        //}
    //        OnDropEnabled = false;
    //        //IsGasReading = false;
    //        Type = DragAndDropType.None;
    //    }
    //}
}
