using UnityEngine;
using System.Collections;

public class StaticVentilationZoneRect : StaticVentilationZone
{
    public Bounds ZoneBounds;

    public override void GetMineAtmosphere(Vector3 worldPos, out float zoneStrength, out MineAtmosphere atmosphere)
    {
        var localPos = transform.InverseTransformPoint(worldPos);
        var closestPoint = ZoneBounds.ClosestPoint(localPos);
        var diff = localPos - closestPoint;
        float dist = diff.magnitude;

        //Debug.Log($"StaticVentRect Dist: {dist:F1}");

        //get local atmosphere with variation applied
        atmosphere = GetLocalAtmosphere();

        if (dist > FalloffSize)
        {
            zoneStrength = 0;
        }
        else if (dist > 0.01f && FalloffSize > 0)
        {
            zoneStrength = 1.0f - (dist / FalloffSize);
        }
        else
        {
            zoneStrength = 1;
        }
    }

    protected override void OnDrawGizmosSelected()
    {        
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(ZoneBounds.center, ZoneBounds.size);
        //Gizmos.DrawWireSphere(transform.position, Radius);

        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, Radius+FalloffSize);
        Gizmos.DrawWireCube(ZoneBounds.center, ZoneBounds.size + new Vector3(FalloffSize*2.0f, FalloffSize*2.0f, FalloffSize*2.0f));

        Gizmos.matrix = Matrix4x4.identity;
    }

}
