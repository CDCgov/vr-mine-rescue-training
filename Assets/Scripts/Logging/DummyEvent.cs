using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyEvent : MonoBehaviour {

    private EventLogHandle _EventLogger;
    public GameObject BumpInTheRoad;
    private bool _IsEnabled = false;
    // Use this for initialization
    void Start()
    {
        _EventLogger = gameObject.GetComponent<EventLogHandle>();
        BumpInTheRoad.SetActive(_IsEnabled);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            _IsEnabled = !_IsEnabled;
            BumpInTheRoad.SetActive(_IsEnabled);
            if (_EventLogger != null)
            {
                _EventLogger.LogEvent("Bump enabled: " + _IsEnabled);
            }
        }
    }
}