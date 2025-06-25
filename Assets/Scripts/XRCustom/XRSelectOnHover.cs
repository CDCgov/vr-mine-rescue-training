using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRSelectOnHover : MonoBehaviour
{
    XRSimpleInteractable _simpleInteractable;
    XRBaseInteractable _baseInteract;

    private void Start()
    {
        if(_simpleInteractable == null)
        {
            _simpleInteractable = gameObject.GetComponent<XRSimpleInteractable>();
        }
    }
}
