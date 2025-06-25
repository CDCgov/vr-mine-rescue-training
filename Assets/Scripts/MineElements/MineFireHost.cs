using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineFireHost : MineElementHostBase, IMineElementHost, ISelectableObject
{
    public MineFire MineFire;

    [System.NonSerialized]
    public VentFire VentFire;


    public MineElement GetMineElement()
    {
        return MineFire;
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