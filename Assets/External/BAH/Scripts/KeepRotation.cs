using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepRotation : MonoBehaviour
{
    private Quaternion StartRot;
    // Start is called before the first frame update
    void Awake()
    {
        StartRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.rotation != StartRot)
        {
            transform.rotation = StartRot;
        }
    }
}
