using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Obsolete]
public class NPCInitialInteractionIndicator : MonoBehaviour
{
    public NPCController RefugeNPCBehaviors;

    private List<CustomXRInteractor> InteractorsInField;

    //private void Start()
    //{
    //    InteractorsInField = new List<CustomXRInteractor>();
    //}

    //private void Update()
    //{
    //    if (RefugeNPCBehaviors == null)
    //    {
    //        Debug.LogError($"RefugeNPCBehaviors not assigned on {gameObject.name}");
    //        this.enabled = false;
    //        return;
    //    }

    //    if(InteractorsInField.Count > 0)
    //    {
    //        RefugeNPCBehaviors.YellowIndicatorOn = true;
    //    }
    //    else
    //    {
    //        RefugeNPCBehaviors.YellowIndicatorOn = false;
    //    }
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    CustomXRInteractor customXRInteractor = other.GetComponent<CustomXRInteractor>();
    //    if(customXRInteractor != null)
    //    {
    //        //InteractorsInField.Add(customXRInteractor);
    //        if(customXRInteractor.IsTrackedController)
    //            RefugeNPCBehaviors.InteractorsInYellowZone.Add(customXRInteractor);
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    CustomXRInteractor customXRInteractor = other.GetComponent<CustomXRInteractor>();
    //    if (customXRInteractor != null)
    //    {
    //        //InteractorsInField.Remove(customXRInteractor);
    //        if (customXRInteractor.IsTrackedController)
    //            RefugeNPCBehaviors.InteractorsInYellowZone.Remove(customXRInteractor);
    //    }
    //}
}
