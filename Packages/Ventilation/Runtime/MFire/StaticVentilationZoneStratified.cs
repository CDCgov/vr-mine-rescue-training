using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticVentilationZoneStratified : StaticVentilationZone
{
    [System.Serializable]
    public struct AtmosphereStratumData
    {
        public float UpperBoundHeight;
        public StaticMineAtmosphere StrataAtmosphere;

        [System.NonSerialized]
        public float LocalYUpperBounds;
    }

    public Bounds ZoneBounds;
    public List<AtmosphereStratumData> AtmosphereStrata;

    private void Start()
    {
        if (AtmosphereStrata == null)
            AtmosphereStrata = new List<AtmosphereStratumData>();

        if (StaticMineAtmosphere == null)
            StaticMineAtmosphere = new StaticMineAtmosphere();

        for (int i = AtmosphereStrata.Count - 1; i >= 0; i--)
        {
            if (AtmosphereStrata[i].StrataAtmosphere == null)
            {
                AtmosphereStrata.RemoveAt(i);
            }
        }

        SortStrata();
        RecalculateBounds();
    }

    public void RecalculateBounds()
    {
        var halfSize = ZoneBounds.size * 0.5f;

        for (int i = 0; i < AtmosphereStrata.Count; i++)
        {
            var data = AtmosphereStrata[i];
            data.LocalYUpperBounds = data.UpperBoundHeight + ZoneBounds.center.y - halfSize.y;
            AtmosphereStrata[i] = data;
        }
    }

    private void SortStrata()
    {
        AtmosphereStrata.Sort((a, b) =>
        {
            return a.UpperBoundHeight.CompareTo(b.UpperBoundHeight);
        });
    }

    void OnBeforeSerialize()
    {
        SortStrata();
    }

    public override void GetMineAtmosphere(Vector3 worldPos, out float zoneStrength, out MineAtmosphere atmosphere)
    {
        var localPos = transform.InverseTransformPoint(worldPos);
        var closestPoint = ZoneBounds.ClosestPoint(localPos);
        var diff = localPos - closestPoint;
        float dist = diff.magnitude;
        float localHeight = localPos.y;

        //Debug.Log($"StaticVentRect Dist: {dist:F1}");

        //get local atmosphere with variation applied
        //atmosphere = GetLocalAtmosphere();

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

        if (AtmosphereStrata == null || AtmosphereStrata.Count <= 0)
        {
            atmosphere = GetLocalAtmosphere();
            return;
        }

        for (int i = 0; i < AtmosphereStrata.Count; i++)
        {
            var data = AtmosphereStrata[i];
            if (data.LocalYUpperBounds >= localHeight)
            {
                atmosphere = data.StrataAtmosphere.GetAtmosphere();
                return;
            }
        }

        //default to static mine atmosphere
        atmosphere = StaticMineAtmosphere.GetAtmosphere();
    }

    private void DrawPlane(float height)
    {
        var halfSize = ZoneBounds.size * 0.5f;
        float y = height + ZoneBounds.center.y - halfSize.y;

        var maxHeight = ZoneBounds.center.y + halfSize.y;
        var minHeight = ZoneBounds.center.y - halfSize.y;
        y = Mathf.Clamp(y, minHeight, maxHeight);

        var p1 = ZoneBounds.center + halfSize;
        var p2 = ZoneBounds.center - halfSize;

        halfSize.x *= -1;

        var p3 = ZoneBounds.center + halfSize;
        var p4 = ZoneBounds.center - halfSize;

        p1.y = p2.y = p3.y = p4.y = y;

        Gizmos.color = Color.cyan;
        
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p1, p3);
        Gizmos.DrawLine(p1, p4);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p2, p4);
        Gizmos.DrawLine(p3, p4);

    }

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(ZoneBounds.center, ZoneBounds.size);
        //Gizmos.DrawWireSphere(transform.position, Radius);

        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, Radius+FalloffSize);
        Gizmos.DrawWireCube(ZoneBounds.center, ZoneBounds.size + new Vector3(FalloffSize * 2.0f, FalloffSize * 2.0f, FalloffSize * 2.0f));

        if (AtmosphereStrata != null)
        {
            foreach (var data in AtmosphereStrata)
            {
                DrawPlane(data.UpperBoundHeight);
            }
        }

        Gizmos.matrix = Matrix4x4.identity;
    }
}

