using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventLogHandle : MonoBehaviour 
{
    private EventLogData _EventData;

    public int EventID;    
    public LogManager LogManagerRef;

    void Start ()
	{
        _EventData = new EventLogData();
        _EventData.EventID = EventID;

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
	
    /// <summary>
    /// Function to call during an event
    /// </summary>
    /// <param name="descriptor"></param>
    public void LogEvent(string descriptor)
    {
        _EventData.EventDescriptor = descriptor;
        _EventData.EventLocation = transform.position;
        LogManagerRef.AddPacketToQueue(_EventData);
    }	
}