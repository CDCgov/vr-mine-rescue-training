using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxDistanceStateBlend : MonoBehaviour
{

    public DeformableProxSystem ProxSystemTarget;
    public int BlendStateClose = 0;
    public int BlendStateDistant = 1;

    public float MinDist = 2.0f;
    public float MaxDist = 10.0f;

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 v = ProxSystemTarget.transform.position - transform.position;
        float dist = v.magnitude;

        dist -= MinDist;
        float ratio = dist / (MaxDist + MinDist);
        ratio = Mathf.Clamp(ratio, 0, 1);

        //ProxSystemTarget.StateBlendWeights[BlendStateDistant] = ratio;
        //ProxSystemTarget.StateBlendWeights[BlendStateClose] = 1.0f - ratio;
    }
}
