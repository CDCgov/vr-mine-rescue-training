using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This is attached to curtains, stoppings, and other placables to detect AxisSnapZones
/// </summary>
public class AxisSnapTrigger : MonoBehaviour
{
    Placer _placer;
    Transform _snapZone;
    Renderer _snapZoneRenderer;
    bool _isNorthSouth;

    void Start()
    {
        _placer = FindObjectOfType<Placer>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Equals("AxisSnapZone_N") || other.gameObject.name.Equals("AxisSnapZone_S") || other.gameObject.name.Equals("AxisSnapZone_E") || other.gameObject.name.Equals("AxisSnapZone_W"))
        {
            if (other.transform != _snapZone)
            {
                _snapZone = other.transform;
                var dragSnap = _placer.activeLogic as SnapCurtainPlacerLogic;
                _isNorthSouth = other.gameObject.name.Equals("AxisSnapZone_N") || other.gameObject.name.Equals("AxisSnapZone_S");
                if(dragSnap!= null)dragSnap.SetSnapZone(_snapZone, _isNorthSouth);
                _snapZoneRenderer = _snapZone.GetComponent<Renderer>();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Equals("AxisSnapZone_N") || other.gameObject.name.Equals("AxisSnapZone_S") || other.gameObject.name.Equals("AxisSnapZone_E") || other.gameObject.name.Equals("AxisSnapZone_W"))
        {
            if(other.transform == _snapZone)
            {

                _snapZoneRenderer = null;
                _snapZone = null;
                var dragSnap = _placer.activeLogic as SnapCurtainPlacerLogic;
                if(dragSnap != null) dragSnap.SetSnapZone(null, _isNorthSouth);
            }
        }
    }



    
}
