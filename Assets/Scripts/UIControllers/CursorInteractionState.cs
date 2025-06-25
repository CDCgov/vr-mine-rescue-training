
using UnityEngine.EventSystems;

using UnityEngine;

public class CursorInteractionState : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler
{


    [SerializeField] CursorImageController.CursorImage hoverState;
    [SerializeField] CursorImageController.CursorImage pressedState;
    private CursorImageController.CursorImage exitState = CursorImageController.CursorImage.Arrow;

    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorImageController.instance.ChangeCursorImage(hoverState);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorImageController.instance.ChangeCursorImage(exitState);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        CursorImageController.instance.ChangeCursorImage(pressedState);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        CursorImageController.instance.ChangeCursorImage(hoverState);
    }




}



