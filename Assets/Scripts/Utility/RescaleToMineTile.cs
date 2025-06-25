using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RescaleToMineTile : MonoBehaviour
{
    public bool ScaleX = false;
    public bool ScaleY = true;
    public bool ScaleZ = false;

    
    public bool SpanX = false;
    public LayerMask RaycastLayerMask;

    // Start is called before the first frame update
    void Start()
    {
        var mineNetwork = FindObjectOfType<MineNetwork>();
        if (mineNetwork != null)
        {
            Vector3 scale = transform.localScale;
            if (ScaleX)
                scale.x = mineNetwork.SceneTileScale.x;
            if (ScaleY)
                scale.y = mineNetwork.SceneTileScale.y;
            if (ScaleZ)
                scale.z = mineNetwork.SceneTileScale.z;

            transform.localScale = scale;
        }

        if (SpanX)
            SpanXAxis();
    }

    void SpanXAxis()
    {
        Vector3 startPoint = Vector3.zero;
        Vector3 endPoint = Vector3.zero;

        var pos = transform.position + new Vector3(0, 0.5f, 0);
        var dir = transform.right;
        var layerMask = RaycastLayerMask.value;

        RaycastHit hit;
        if (!Physics.Raycast(pos, dir, out hit, 100, layerMask, QueryTriggerInteraction.Ignore))
            return;

        endPoint = hit.point;

        if (!Physics.Raycast(pos, dir * -1, out hit, 100, layerMask, QueryTriggerInteraction.Ignore))
            return;

        startPoint = hit.point;

        Vector3 midPoint = (startPoint + endPoint) * 0.5f;

        var rot = Quaternion.FromToRotation(Vector3.right, dir);

        //calculate length
        float length = Vector3.Distance(startPoint, endPoint);


        float objBaseLength = 1.0f;
        if (TryGetComponent<BoxCollider>(out var collider))
            objBaseLength = collider.size.x;

        var localScale = transform.localScale;
        transform.localScale = new Vector3(length / objBaseLength, localScale.y, localScale.z);

        midPoint.y = transform.position.y;
        transform.position = midPoint;

    }

    
}
