using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawnSphere : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "MapPencil")
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = collision.contacts[0].point;
            sphere.transform.parent = transform;
            sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }
}
