using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// OBSOLETE: Function to control a light source when the ideal curtain behavior was only 1-sided.
/// </summary>
public class CurtainLight : MonoBehaviour 
{
    public Transform Curtain;
    public Transform PlayerSpot;

    private Light ThisLight;
    private bool ActorFound = false;

    void Start()
    {
        ThisLight = gameObject.GetComponent<Light>();
        ThisLight.enabled = false;

        if(PlayerSpot == null)
        {
            if (MasterControl.SceneControl != null)
            {
                //Wait for actor to spawn?
            }
            else
            {
                gameObject.SetActive(false); //Placeholder until multiplayer instantiaion is figured out. See: CurtainColliderHelper for relevant curtain script
            }
        }
        else
        {
            ActorFound = true;
        }
    }

    void Update()
    {
        if (ActorFound)
        {
            ThisLight.enabled = (Vector3.Distance(Curtain.position, PlayerSpot.position) < 10);
        }
        else
        {
            ThisLight.enabled = false;
            if(MasterControl.SceneControl != null && MasterControl.SceneControl.ActiveActors != null)
            {
                Transform helper = MasterControl.SceneControl.ActiveActors[0].transform;
                PlayerSpot = helper;
                ActorFound = true;
            }
        }

        if (ThisLight.enabled)
        {

            transform.position = PlayerSpot.position;
            transform.rotation = PlayerSpot.rotation;

            Vector3 intersect;
            float ldotn = Vector3.Dot(PlayerSpot.transform.forward, Curtain.transform.forward);
            if (ldotn != 0)
            {
                //Linear plane integration equation: https://en.wikipedia.org/wiki/Line%E2%80%93plane_intersection
                //Huh, fun fact: You can put hyperlinks into visual studio comments. Who knew.
                float d = Vector3.Dot((Curtain.transform.position - PlayerSpot.transform.position), Curtain.transform.forward) / Vector3.Dot(PlayerSpot.transform.forward, Curtain.transform.forward);
                intersect = d * PlayerSpot.transform.forward + PlayerSpot.transform.position;

                transform.RotateAround(intersect, Vector3.up, 180);
            }
            else
            {
                ThisLight.enabled = false;
            }
        }
    }
}