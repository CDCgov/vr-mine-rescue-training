using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Obsolete]
public class ScaleSpawnedCurtain : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RaycastHit hit;
        Vector3 pos = transform.position;
        pos.y += 0.5f;
        int layerMask = LayerMask.GetMask("Floor");
        if(Physics.Raycast(pos, transform.right, out hit, 10, layerMask))
        {
            float scale = Vector3.Distance(pos, hit.point) / 3;
            if(scale > 1.2f)
            {
                Vector3 factor = Vector3.one;
                factor.x = scale + 0.05f;
                transform.localScale = factor;
            }
        }
    }
}
