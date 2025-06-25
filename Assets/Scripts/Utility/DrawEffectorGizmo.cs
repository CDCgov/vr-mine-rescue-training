using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawEffectorGizmo : MonoBehaviour
{
    public float Radius = 0.1f;
    void OnDrawGizmos()//May change this to OnDrawGizmosSelected?
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, Radius);
        //Gizmos.DrawIcon(transform.position, transform.name);//TODO: Create a waypoint icon?
    }
}
