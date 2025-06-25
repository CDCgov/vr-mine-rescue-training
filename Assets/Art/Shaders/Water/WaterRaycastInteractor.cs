using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterRaycastInteractor : MonoBehaviour
{
    public Vector3 RaycastDir = new Vector3(0, -1, 0);
    public Vector3 Offset = Vector3.zero;
    public float Distance = 3.0f;
    public bool RaycastDirIsGlobalSpace = true;

    private int _layerMask;
    private Vector3 _lastPos;
    private RaycastHit[] _hits;

    private void Start()
    {
        _layerMask = LayerMask.GetMask("Water");
        _hits = new RaycastHit[10];
    }

    private void Update()
    {
        var moveDist = Vector3.Distance(transform.position, _lastPos);
        _lastPos = transform.position;

        if (moveDist < 0.01f)
            return;

        Vector3 dir;
        if (RaycastDirIsGlobalSpace)
            dir = RaycastDir;
        else
            dir = transform.TransformDirection(RaycastDir);


        WaterInteractionEffect water = null;
        int count = Physics.RaycastNonAlloc(transform.position + Offset, dir, _hits, Distance, _layerMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < count; i++)
        {
            water = _hits[i].collider.GetComponent<WaterInteractionEffect>();

            if (water != null)
            {
                water.SpawnMovmentEffect(_hits[i].point);
            }
        }

    }
}
