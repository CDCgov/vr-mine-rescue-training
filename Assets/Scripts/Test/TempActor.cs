using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempActor : MonoBehaviour {

    public Transform Hand;

    Pickupable _picked;
    
    // Update is called once per frame
    void Update () {
        if (Input.GetButtonUp("Fire1"))
        {
            RaycastHit hit;
            Debug.Log("Fire1 hit");
            if(Physics.Raycast(transform.position,transform.forward, out hit))
            {
                Pickupable pkUp = hit.collider.GetComponent<Pickupable>();
                if(pkUp != null)
                {
                    pkUp.Pickup(null, Hand);
                    _picked = pkUp;
                }
            }
        }

        if (Input.GetButtonUp("Fire2"))
        {
            if(_picked != null)
            {
                _picked.Drop();
                _picked = null;
            }
        }
    }
}
