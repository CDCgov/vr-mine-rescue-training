using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public Vector3 xPosPosition = Vector3.right;
    public Vector3 xNegPosition = Vector3.left;
    public Vector3 zPosPosition = Vector3.forward;
    public Vector3 zNegPosition = Vector3.back;

    public float CorridorHeight = 2;


    protected Vector3 trCorner;
    protected Vector3 tlCorner;
    protected Vector3 blCorner;
    protected Vector3 brCorner;

    [Range(0, 1)]
    public float ZoneStrength;
    public Vector3 NodePosition;

    public List<ActorHost> ActorsInZone;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = ZoneColor(ZoneStrength);
        Vector3 center = transform.position;
        float zDist = Vector3.Distance(zNegPosition, zPosPosition) / 2;
        float xDist = Vector3.Distance(xNegPosition, xPosPosition) / 2;
        center.z = zNegPosition.z;
        center = center + zDist * Vector3.forward;
        center.x = xNegPosition.x;
        center = center + xDist * Vector3.right;
        center = transform.position + center;
        center.y = transform.position.y;

        Gizmos.DrawCube(center, new Vector3(Mathf.Abs(xDist * 2), 2, Mathf.Abs(zDist * 2)));        
    }

    protected Color ZoneColor(float strength)
    {
        float hue = Mathf.Lerp(0, 0.333f, strength);
        Color output = Color.HSVToRGB(hue, 1, 1);
        output.a = 0.1f;
        return output;
    }
}
