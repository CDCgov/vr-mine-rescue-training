using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 414

public enum MobileEqupmentType
{
    Roofbolter,
    ContinuousMiner,
    Mantrip,
    ShuttleCar,
    Scoop
}

public class MobileEquipmentLogHandle : LogHandle 
{
    private MobileEquipmentData _MobileEquipmentData;
    private Rigidbody _MERigidbody;
    
    public MobileEqupmentType MobileEquipmentTypeSelect;
    public int IDNumber;

    public LogManager LogManagerRef;

    private float _priorTime = 0;

    void Start () 
    {

        _MobileEquipmentData = new MobileEquipmentData();
        _MobileEquipmentData.MobileEquipmentType = (int)MobileEquipmentTypeSelect;
        _MobileEquipmentData.MobileEquipmentID = IDNumber;

        _MERigidbody = gameObject.GetComponent<Rigidbody>();
        if(_MERigidbody == null)
        {
            Debug.LogErrorFormat("No rigidbody attached to the mobile equipment game object: {0}", gameObject.name);
        }

        if (LogManagerRef == null)
        {
            LogManagerRef = FindObjectOfType(typeof(LogManager)) as LogManager;
            if (LogManagerRef == null)
            {
                Debug.LogError("You must have a LogManager attached to a GameObject in your scene. : " + gameObject.name);
                Application.Quit();
            }
        }
    }
    
    void FixedUpdate () 
    {		
        _MobileEquipmentData.Position = transform.position;
        _MobileEquipmentData.Rotation = transform.rotation;
        if (_MERigidbody != null)
        {
            _MobileEquipmentData.Velocity = _MERigidbody.velocity.magnitude;
        }
        else
        {
            _MobileEquipmentData.Velocity = 0;
        }
        //_MobileEquipmentData.TimeStamp = (double)Time.time;
        LogManagerRef.AddPacketToQueue(_MobileEquipmentData);        
    }

    //Only called in Editor mode when the Reset context menu item is selected on a component or when a script is first attached to a game object
    void Reset()
    {
        LogManagerRef = FindObjectOfType(typeof(LogManager)) as LogManager;
        if (LogManagerRef == null)
        {			
            GameObject logManager = new GameObject();
            logManager.transform.position = new Vector3(0, 0, 0);
            logManager.name = "Log Manager";
            LogManager lmClass = logManager.AddComponent<LogManager>();
            LogManagerRef = lmClass;
            Debug.LogWarning("You must have a LogManager attached to a GameObject in your scene. One has been generated into your scene.");
        }
        //This is a safeguard to ensure that there are no duplicate ID numbers, the ID numbers can be overridden by the user manuall in the editor
        int highestIndex = 0;
        MobileEquipmentLogHandle[] meHandles = GameObject.FindObjectsOfType<MobileEquipmentLogHandle>();
        foreach(MobileEquipmentLogHandle item in meHandles)
        {
            if(item.IDNumber > highestIndex)
            {
                highestIndex = item.IDNumber;
            }
        }
        IDNumber = highestIndex + 1;
    }
}