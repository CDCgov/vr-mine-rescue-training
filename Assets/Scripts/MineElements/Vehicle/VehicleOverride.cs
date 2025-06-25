using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

/// <summary>
/// Class that handles vehicle behavior overrides upon entering proximity zones
/// </summary>
public class VehicleOverride : MonoBehaviour {

    public LogManager LogManagerRef;
    StringMessageData _TriggerMessage;

    private ProxSystemController[] _AllProxControllers;
    //private CarController _CarRef;
    //private CarAIControl _CarAI;
    
    void Start()
    {
        _TriggerMessage = new StringMessageData();
        _AllProxControllers = GameObject.FindObjectsOfType<ProxSystemController>();
        
        //_CarRef = GetComponent<CarController>();
        //_CarAI = GetComponent<CarAIControl>();
        foreach(ProxSystemController prox in _AllProxControllers)
        {
            prox.ProxZoneChanged += Prox_ProxZoneChanged;//It doesn't matter what prox zone it is, if it's in the red zone, we want the vehicle to stop!
            Debug.Log(prox.gameObject.name);
        }
    }
    /// <summary>
    /// Event method to override the car's controller to stop vehicle motion upon entering the red zone in the prox system.
    /// </summary>
    /// <param name="zone"></param>
    private void Prox_ProxZoneChanged(ProxZone zone)
    {
        Debug.Log("State changed " + zone.ToString());
        //if (_CarRef != null && zone == ProxZone.RedZone)
        //{
        //    _CarRef.ContinuousMove(0, 0, 0, 1, true);
        //    Debug.Log("Should have stopped!");
        //    if (LogManagerRef != null)
        //    {
        //        _TriggerMessage.Message = "Stop override";
        //        _TriggerMessage.IsEvent = true;
        //        LogManagerRef.AddPacketToQueue(_TriggerMessage);
        //    }          
        //    if (_CarAI != null)
        //    {
        //        _CarAI.enabled = false;
        //    }
        //}
    }
    /// <summary>
    /// Old, simple prox detection using Unity trigger. See the new event handle for interaction into Will's prox system
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        //CarController car = other.GetComponent<CarController>();
        //if(car != null)
        //{
        //    car.ContinuousMove(0, 0, 0, 1, true);
        //    _TriggerMessage.Message = "Stop override";
        //    _TriggerMessage.IsEvent = true;
        //    LogManagerRef.AddPacketToQueue(_TriggerMessage);
        //    CarAIControl ai = other.GetComponent<CarAIControl>();
        //    if(ai != null)
        //    {
        //        ai.enabled = false;
        //    }
        //}
    }
}