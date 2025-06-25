using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class XRObjectController : MonoBehaviour
{
    public abstract void GainedOwnership(CustomXRInteractable interactable);
    public abstract void LostOwnership(CustomXRInteractable interactable);
}
