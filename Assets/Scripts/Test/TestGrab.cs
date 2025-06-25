using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrab : MonoBehaviour {

    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.forward, out hit))
            {
                SCSRCacheInteract scsr = hit.collider.GetComponent<SCSRCacheInteract>();
                if(scsr != null)
                {
                    scsr.Interact();
                }
            }
        }
    }
}
