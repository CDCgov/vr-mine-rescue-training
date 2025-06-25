using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float DestroyAfterSeconds = 5; 

    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(SelfDestruct), DestroyAfterSeconds);
    }

    void SelfDestruct()
    {
        Destroy(gameObject);
    }

}
