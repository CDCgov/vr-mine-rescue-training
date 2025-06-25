using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Code that handles behavior of wireless communication zones
/// </summary>
public class CommZone : Zone {
    //Put custom code related to CommZones here, if needed
    public MeshNode MeshNode;
    public List<HandheldRadio> RadiosInZone;


    private void Start()
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
        center.y = transform.position.y;
        bc.center = center - transform.position;
        bc.size = new Vector3(Mathf.Abs(xDist * 2), 2, Mathf.Abs(zDist * 2));
        bc.isTrigger = true;
    }
    private void OnValidate()
    {
        if(transform.parent == null)
        {
            GameObject meshNetwork = new GameObject();
            meshNetwork.transform.position = transform.position;
            meshNetwork.name = "Mesh Node";
            MeshNode mn = meshNetwork.AddComponent<MeshNode>();
            MeshNode = mn;
            transform.parent = meshNetwork.transform;
        }
        else
        {
            if(MeshNode == null)
            {
                MeshNode = transform.parent.GetComponent<MeshNode>();
                if(MeshNode == null)
                {
                    transform.parent.gameObject.AddComponent<MeshNode>();
                }
            }
        }
        
    }

    protected  void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 20)
        {
            ActorHost actor = other.gameObject.GetComponent<ActorHost>();
            if (actor != null)
            {
                ActorsInZone.Remove(actor);
                MeshNode.UpdateActorList();
            }
        }

        HandheldRadio rad = other.gameObject.GetComponent<HandheldRadio>();
        if (rad != null)
        {
            MeshNode.Radios.Remove(rad);
        }
    }

    protected void OnTriggerEnter(Collider other)
    {        
        if (other.gameObject.layer == 20)
        {
            ActorHost actor = other.gameObject.GetComponent<ActorHost>();
            if (actor != null)
            {
                ActorsInZone.Add(actor);
                if (!MeshNode.ActorsInNode.Contains(actor))
                {
                    MeshNode.ActorsInNode.Add(actor);
                }
                MeshNode.UpdateActorList();
            }
        }
        HandheldRadio rad = other.gameObject.GetComponent<HandheldRadio>();
        if (rad != null)
        {
            MeshNode.Radios.Add(rad);
        }
    }
    protected void Reset()
    {
        MeshNode = transform.parent.GetComponent<MeshNode>();
        if (MeshNode != null)
        {
            //trackNode.
            if (!MeshNode.CommZones.Contains(this))
            {
                MeshNode.CommZones.Add(this);
            }
        }
        else
        {
            if (transform.parent != null)
            {
                MeshNode node = transform.parent.gameObject.AddComponent<MeshNode>();
                node.CommZones.Add(this);
                MeshNode = node;
            }
            else
            {
                GameObject meshNetwork = new GameObject();
                meshNetwork.transform.position = transform.position;
                meshNetwork.name = "Mesh Node";
                MeshNode mn = meshNetwork.AddComponent<MeshNode>();
                MeshNode = mn;
                transform.parent = meshNetwork.transform;
            }
        }
    }
}
