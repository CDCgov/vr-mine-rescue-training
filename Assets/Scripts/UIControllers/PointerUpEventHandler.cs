using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using NIOSH_MineCreation;

/// <summary>
/// A simplified event trigger that looks for when a pointer is unclicked from the target UI that this script is on. 
/// Useful for submitting slider values as descrete events rather than every time the value changes.
/// </summary>
public class PointerUpEventHandler : MonoBehaviour, IPointerUpHandler
{
    public UnityEvent onPointerUp;
    public virtual void OnPointerUp(PointerEventData eventData)
    {
       // Debug.Log("EventTest");
        onPointerUp?.Invoke();
    }
}
