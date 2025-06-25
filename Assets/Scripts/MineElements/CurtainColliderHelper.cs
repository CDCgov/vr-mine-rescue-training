using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class provides an efficient way to have our curtains account for dynamically spawned players in Multiplayer.
/// </summary>
[RequireComponent(typeof(Cloth))]
public class CurtainColliderHelper : MonoBehaviour 
{
    public List<CapsuleCollider> CollidersToObserve;//Set the initial colliders for the curtain to observe in editor.

    private Cloth _clothRenderer;
    private int _actorCount = 0;
    private int _actorCollidersAdded = 0;

    void Start () 
    {
        if (CollidersToObserve == null)
            CollidersToObserve = new List<CapsuleCollider>();

        _clothRenderer = gameObject.GetComponent<Cloth>();		
        _actorCount = 0;

        int curtainLayer = LayerMask.NameToLayer("Curtain");

        var capsules = GameObject.FindObjectsOfType<CapsuleCollider>();
        foreach (var col in capsules)
        {
            if (col.gameObject.layer == curtainLayer)
            {
                CollidersToObserve.Add(col);
                if (CollidersToObserve.Count > 20)
                    break;
            }
        }

        _clothRenderer.capsuleColliders = CollidersToObserve.ToArray();		
    }
    
    void Update () 
    {		
        if (MasterControl.SceneControl != null && MasterControl.SceneControl.ActiveActors != null)//Check if the scene control and Active Actors are initialized yet.
        {
            if(MasterControl.SceneControl.ActiveActors.Count != _actorCount)//Check for a change in the # of actors. Update the colliders to observe if there is a difference.
            {
                //remove old actors
                for (int i = 0; i < _actorCollidersAdded; i++)
                {
                    CollidersToObserve.RemoveAt(CollidersToObserve.Count - 1);
                }

                int newActorCount = MasterControl.SceneControl.ActiveActors.Count;
                int collidersAdded = 0;

                for (int i = 0; i < newActorCount; i++)
                {
                    if (CollidersToObserve.Count >= 32)
                        break;

                    CapsuleCollider collider = MasterControl.SceneControl.ActiveActors[i].GetComponent<CapsuleCollider>();
                    if (collider != null)
                    {
                        CollidersToObserve.Add(collider);
                        collidersAdded++;
                    }
                }

                _actorCollidersAdded = collidersAdded;
                _actorCount = newActorCount;
                _clothRenderer.capsuleColliders = CollidersToObserve.ToArray();

                /*
                int finalLength = CollidersToObserve.Length + MasterControl.SceneControl.ActiveActors.Count;
                CapsuleCollider[] ColliderUpdate = new CapsuleCollider[finalLength];
                int colliderCount = 0;
                if(finalLength > 32)//Unity can only handle 32 colliders for the cloth renderer
                {					
                    Debug.LogError("Exceeded collider count! Exiting...");
                    Application.Quit();
                }
                else
                {
                    colliderCount = finalLength;
                }
                for(int i = 0; i < CollidersToObserve.Length; i++)
                {
                    ColliderUpdate[i] = CollidersToObserve[i];
                }
                int j = 0;
                for(int i = CollidersToObserve.Length; i < colliderCount; i++)
                {
                    ColliderUpdate[i] = MasterControl.SceneControl.ActiveActors[j].GetComponent<CapsuleCollider>();
                    j++;
                }
                _ClothRenderer.capsuleColliders = ColliderUpdate;

                _ActorCount = MasterControl.SceneControl.ActiveActors.Count; */
            }
        }
    }
    
}