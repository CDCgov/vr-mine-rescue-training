using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookGizmo : MonoBehaviour {

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.05f, 0.05f, 0.05f));
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
