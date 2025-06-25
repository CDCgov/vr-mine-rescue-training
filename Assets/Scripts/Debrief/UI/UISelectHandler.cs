using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UISelectHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isSelected;

    public bool IsSelected
    {
        get { return isSelected; }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("Handle is selected");
        isSelected = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("Handle is released");
        isSelected = false;
    }
}
