using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMapper : MonoBehaviour
{
    
    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit))
        {
            MappingMappableItem toMap = hit.collider.GetComponent<MappingMappableItem>();
            if(toMap != null)
            {
                toMap.OnMap();
            }
        } 
    }
}
