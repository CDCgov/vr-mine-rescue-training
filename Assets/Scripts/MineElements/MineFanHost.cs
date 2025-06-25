using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineFanHost : MineElementHostBase, IMineElementHost, ISelectableObject
{
    public VentFanData FanData;
    public MineFan MineFan;

    public MineElement GetMineElement()
    {
        return MineFan;
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.DrawSphere(transform.position, 0.75f);
        //Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }

    protected override void OnInitializeSegments()
    {
        base.OnInitializeSegments();

        AssociateWithMineSegment();
    }
}