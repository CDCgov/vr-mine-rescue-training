using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResetVehicleCollider : MonoBehaviour 
{
    public Vector3 StartingPosition;
    public Vector3 StartingRotation;
    public LogManager LogManagerRef;
    public VehicleExperiment VehicleExperimentRef;

    private StringMessageData _RestartMessage;
    

    void Start()
    {
        _RestartMessage = new StringMessageData();
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<MobileEquipmentLogHandle>() != null)
        {
            //Rigidbody rb = other.GetComponent<Rigidbody>();
            //rb.velocity = new Vector3(0, 0, 0);
            //other.transform.position = StartingPosition;
            //other.transform.rotation = Quaternion.Euler(StartingRotation);
            //SimpleAICarInitialCondition ai = other.GetComponent<SimpleAICarInitialCondition>();
            //ai.Restart();
            VehicleExperimentRef.Restart();
            _RestartMessage.Message = "Trial Restart";
            LogManagerRef.AddPacketToQueue(_RestartMessage);
        }
    }
}