using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingZone : MonoBehaviour {

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
    //public TrackingManager TrackingManager;
    public TrackingNode TrackingNode;

    
    public List<ActorHost> ActorsInZone;

    public bool ShowZone = true;
    // Use this for initialization
    protected void Start()
    {
        ActorsInZone = new List<ActorHost>();
        trCorner = xPosPosition + zPosPosition;
        tlCorner = xNegPosition + zPosPosition;
        blCorner = xNegPosition + zNegPosition;
        brCorner = xNegPosition + zNegPosition;
        transform.localScale = new Vector3(1, 1, 1);

        BoxCollider bc = gameObject.AddComponent<BoxCollider>();
        Vector3 center = transform.position;
        float zDist = Vector3.Distance(zNegPosition, zPosPosition) / 2;
        float xDist = Vector3.Distance(xNegPosition, xPosPosition) / 2;
        center.z = zNegPosition.z;
        center = center + zDist * Vector3.forward;
        center.x = xNegPosition.x;
        center = center + xDist * Vector3.right;
        center = transform.position + center;
        center.y = 1;
        bc.center = center - transform.position;
        bc.size = new Vector3(Mathf.Abs(xDist * 2), 2, Mathf.Abs(zDist * 2));
        bc.isTrigger = true;
        //Gizmos.DrawCube(center, new Vector3(Mathf.Abs(xDist * 2), 2, Mathf.Abs(zDist * 2)));
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        //Debug.Log(gameObject.name + ", " + Time.time);
        if(other.gameObject.layer == 20)
        {
            ActorHost actor = other.gameObject.GetComponent<ActorHost>();
            if(actor != null)
            {
                ActorsInZone.Add(actor);
                if (!TrackingNode.ActorsInNode.Contains(actor))
                {
                    TrackingNode.ActorsInNode.Add(actor);
                }
                TrackingNode.UpdateActorList();
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == 20)
        {
            ActorHost actor = other.gameObject.GetComponent<ActorHost>();
            if(actor != null)
            {
                ActorsInZone.Remove(actor);
                TrackingNode.UpdateActorList();
            }
        }
    }

    // Update is called once per frame
    protected void Update()
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
        center.y = 1;

        Gizmos.DrawCube(center, new Vector3(Mathf.Abs(xDist * 2), 2, Mathf.Abs(zDist * 2)));
        //trCorner = transform.position + (xPosPosition + zPosPosition);
        //tlCorner = transform.position + (xNegPosition + zPosPosition);
        //blCorner = transform.position + (xNegPosition + zNegPosition);
        //brCorner = transform.position + (xPosPosition + zNegPosition);
        ////Mesh coolMesh = new Mesh();
        ////coolMesh.Clear();
        //Vector3[] verts = new Vector3[4];
        //verts[0] = brCorner;
        //verts[1] = trCorner;
        //verts[2] = tlCorner;
        //verts[3] = blCorner;
        //int[] tris = new int[]{0,2,1,1,3,2};
        //Gizmos.DrawLine(brCorner, trCorner);
        //Gizmos.DrawLine(trCorner,tlCorner);
        //Gizmos.DrawLine(tlCorner, blCorner);
        //Gizmos.DrawLine(blCorner, brCorner);
        //coolMesh.RecalculateNormals();
        //Gizmos.DrawMesh(coolMesh);
        //Graphics.DrawMeshNow(coolMesh, Vector3.zero, Quaternion.identity);

        //DestroyImmediate(coolMesh);
    }

    protected Color ZoneColor(float strength)
    {
        float hue = Mathf.Lerp(0, 0.333f, strength);
        Color output = Color.HSVToRGB(hue, 1, 1);
        output.a = 0.1f;
        return output;
    }

    protected virtual void Reset()
    {
        TrackingNode = transform.parent.GetComponent<TrackingNode>();
        if(TrackingNode != null)
        {
            //trackNode.
            if (!TrackingNode.Zones.Contains(this))
            {
                TrackingNode.Zones.Add(this);
            }
        }
        else
        {
            if(transform.parent != null)
            {
                TrackingNode node = transform.parent.gameObject.AddComponent<TrackingNode>();
                node.Zones.Add(this);
            }
        }
    }

    protected void OnDisable()
    {
        //TrackingNode.Zones.Remove(this);
        //TrackingNode.ActiveZones.Remove(this);
    }
}
