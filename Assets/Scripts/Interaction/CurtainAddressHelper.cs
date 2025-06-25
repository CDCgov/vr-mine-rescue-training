using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurtainAddressHelper : MonoBehaviour
{
    public string HalfHungCurtainAddress = "Curtain_HalfHung_MP";

    private CurtainBehavior _curtainBehavior;
    // Start is called before the first frame update
    void Start()
    {
        _curtainBehavior = GetComponentInParent<CurtainBehavior>();
        if(_curtainBehavior != null)
        {
            _curtainBehavior.HalfHungCurtainAddress = HalfHungCurtainAddress;
        }
        else
        {
            Debug.Log($"Curtain behavior was null?");
        }
    }

}
