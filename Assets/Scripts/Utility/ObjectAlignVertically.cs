using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAlignVertically : MonoBehaviour
{    
    void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(Vector3.up);
    }
}
