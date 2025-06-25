using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Explode()
    {
        Debug.Log("triggered");
    }
    //private void OnTriggerEnter(Collider other)
    //{
    //	Debug.Log("triggered");
    //	rb.AddExplosionForce(500000, new Vector3(-5, 0, 0), 5);
    //}
}
