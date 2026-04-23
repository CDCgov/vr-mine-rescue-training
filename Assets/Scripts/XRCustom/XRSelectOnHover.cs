using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class XRSelectOnHover : MonoBehaviour
{
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable _simpleInteractable;
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable _baseInteract;

    private void Start()
    {
        if(_simpleInteractable == null)
        {
            _simpleInteractable = gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        }
    }
}
