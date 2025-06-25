using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowTransform : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset;
    public bool ResetToPOIOnTargetLost = true;

    // private IEnumerator Start()
    // {
    // 	while (true)
    // 	{
    // 		yield return new WaitForEndOfFrame();
    // 	}
    // }

    private void LateUpdate()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (Target == null && ResetToPOIOnTargetLost)
        {
            var pois = POIManager.GetDefault(gameObject).GetPOIs();
            if (pois != null && pois.Count > 0)
            {
                foreach (var poi in pois)
                {
                    if (poi as VRPointOfInterest == null)
                    {
                        Target = poi.transform;
                        break;
                    }
                }
                
            }
        }

        if (Target != null)
        {
            Vector3 offset = Target.TransformDirection(Offset);
            transform.position = Target.position + offset;
            transform.rotation = Target.rotation;
        }
    }
}