using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VentMonitorController : MonoBehaviour
{
    public SessionPlaybackControl SessionPlaybackControl;
    public GameObject VentMonitorPrefab;

    private VentMonitoringLocation[] _monitorLocations;

    // Start is called before the first frame update
    void Start()
    {
        if (SessionPlaybackControl == null)
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);

        if (SessionPlaybackControl.IsSessionLoaded)
            SpawnMonitors();

        SessionPlaybackControl.SessionLoaded += OnSessionLoaded;
    }

    private void OnSessionLoaded()
    {
        SpawnMonitors();
    }

    void SpawnMonitors()
    {
        _monitorLocations = FindObjectsOfType<VentMonitoringLocation>();

        if (_monitorLocations == null || _monitorLocations.Length <= 0)
        {
            Debug.LogWarning("VentMonitorController: No monitoring locations");
            this.enabled = false;
        }

        foreach (var loc in _monitorLocations)
        {
            SpawnVentMonitor(loc);
        }
    }

    void SpawnVentMonitor(VentMonitoringLocation loc)
    {
        var monitorObj = GameObject.Instantiate<GameObject>(VentMonitorPrefab);
        var samplePoint = monitorObj.transform.Find("SamplePoint");

        monitorObj.transform.position = loc.transform.position + loc.LabelOffset;
        if (samplePoint != null)
            samplePoint.position = loc.transform.position;
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
