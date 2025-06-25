using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoalFountain : MonoBehaviour
{
    public GameObject CoalLump;

    public float SpawnDelay = 1;
    private float _priorSpawnTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > _priorSpawnTime + SpawnDelay)
        {
            GameObject lump = Instantiate(CoalLump);
            lump.transform.position = transform.position;
            lump.transform.rotation = transform.rotation;
            //lump.transform.localScale = new Vector3(10, 10, 10);
            Rigidbody rb = lump.GetComponent<Rigidbody>();
            if(rb == null)
            {
                rb = lump.AddComponent<Rigidbody>();
            }
            rb.mass = 5;
            rb.AddRelativeForce(0, 3000, 0);
            _priorSpawnTime = Time.time;
        }
    }
}
