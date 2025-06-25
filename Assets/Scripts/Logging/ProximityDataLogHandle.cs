using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof (ProxSystemController))]
public class ProximityDataLogHandle : LogHandle {

    private ProximityData _ProximityData;
    private ProxSystemController _ProxController;

    public int ProxID;
    public MobileEqupmentType EquipmentType;
    public LogManager LogManagerRef;

    public bool EnableContinuousLogging = true;
        
    
    // Use this for initialization
    void Start () {
        _ProximityData = new ProximityData();
        _ProximityData.ProxID = ProxID;
        _ProximityData.EquipmentType = EquipmentType;
        _ProxController = gameObject.GetComponent<ProxSystemController>();

        _ProxController.ProxZoneChanged += OnProximityEvent;

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
    
    // Update is called once per frame
    void Update () {
        if (EnableContinuousLogging)
        {
            _ProximityData.ZoneState = _ProxController.ActiveProxZone;
            _ProximityData.PlayerID = 0;
            _ProximityData.Position = transform.position;
            _ProximityData.TimeStamp = (double)Time.time;//Replace with whatever time keeper class we devise for the final product.
            LogManagerRef.AddPacketToQueue(_ProximityData);
        }
    }

    public void OnProximityEvent(ProxZone zone)
    {
        Debug.Log("Entered prox zone: " + zone.ToString());
        _ProximityData.IsEvent = true;
        _ProximityData.ZoneState = zone;
        _ProximityData.TimeStamp = (double)Time.time;			
        LogManagerRef.AddPacketToQueue(_ProximityData);	
    }

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

        int highestIndex = 0;
        ProximityDataLogHandle[] proxHandles = GameObject.FindObjectsOfType<ProximityDataLogHandle>();
        foreach(ProximityDataLogHandle item in proxHandles)
        {
            if(item.ProxID > highestIndex)
            {
                highestIndex = item.ProxID;
            }
            ProxID = highestIndex + 1;
        }
    }
}
