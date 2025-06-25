using UnityEngine;
using System.Collections;
using UnityStandardAssets.Vehicles.Car;

public class ProximityOverride : MonoBehaviour {

    ProxSystemController _prox;
    //CarController _carControl;
    // Use this for initialization
    void Start () {
        _prox = gameObject.GetComponent<ProxSystemController>();
        //_carControl = gameObject.GetComponent<CarController>();

        _prox.ProxZoneChanged += OnProxEvent;
    }
    
    void OnProxEvent(ProxZone zone)
    {
        switch (zone)
        {
            case ProxZone.None:
                //_carControl.SetMaxSpeed(6);				
                break;
            case ProxZone.GreenZone:
                //_carControl.SetMaxSpeed(6);
                break;
            case ProxZone.YellowZone:
                //_carControl.SetMaxSpeed(1);
                break;
            case ProxZone.RedZone:
                //_carControl.SetMaxSpeed(0);
                break;
            default:
                break;
        }
    }
}
