using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoopMover : MonoBehaviour 
{
    public Vector3 pos1 = new Vector3(0,0,20);
    public Vector3 pos2 = new Vector3(0, 0, -20);

    private bool move = true;
    private float speed = 0.1f;

    void Start () 
    {
    
    }
    
    void Update () 
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            move = !move;
        }

        if (move)
        {
            transform.position = Vector3.Lerp(pos1, pos2, (-Mathf.Cos(speed * Time.time) + 1.0f) / 2.0f);
        }
    }
}