using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleColliderEnabler : MonoBehaviour
{
    public GameObject[] ObjectsToEnable;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Vehicle"))
        {
            foreach(GameObject obj in ObjectsToEnable)
            {
                obj.SetActive(true);
            }
        }
    }
}
