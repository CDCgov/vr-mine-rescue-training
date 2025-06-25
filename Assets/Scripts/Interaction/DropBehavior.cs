using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropBehavior : MonoBehaviour
{
    public Vector3 HandVelocity;
    public Vector3 HandAcceleration;
    public Rigidbody ItemRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void OnDrop()
    {
        ThrowBehavior();
    }

    //void ThrowBehavior(GameObject objectToThrow, Vector3 velocity)
    //{
    //    Rigidbody rb = objectToThrow.GetComponent<Rigidbody>();
    //    if (rb != null)
    //    {
    //        rb.velocity = velocity;
    //    }
    //}
    void ThrowBehavior()
    {
        if (ItemRigidbody == null)
        {
            Rigidbody ItemRigidbody = gameObject.GetComponent<Rigidbody>();
        }
        if (ItemRigidbody != null)
        {
            ItemRigidbody.velocity = HandVelocity;
            
        }
    }
}
