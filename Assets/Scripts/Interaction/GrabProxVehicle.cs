using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabProxVehicle : GrabBehavior
{

    public NetworkManager NetworkManager;

    public ProxSystemController ProxControl;
    //public GameObject ProxSystem;

    private NetworkedObject _netObj;

    // Use this for initialization
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        _netObj = GetComponent<NetworkedObject>();

        ProxControl.EnableVisualization(false);
        //ProxSystem.SetActive(false);
    }

    public override void Grabbed()
    {
        if (_netObj != null)
        {
            _netObj.RequestOwnership();
        }
        base.Grabbed();
        //ProxSystem.SetActive(true);
        ProxControl.EnableVisualization(!ProxControl.ShowVisualization);
    }

    public override void Released()
    {
        base.Released();
        ProxControl.EnableVisualization(!ProxControl.ShowVisualization);
        //ProxSystem.SetActive(false);
    }
}
