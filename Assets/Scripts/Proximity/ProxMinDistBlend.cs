using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxMinDistBlend : MonoBehaviour
{
    public enum FalloffType
    {
        Linear,
        Exponential
    }

    public List<Transform> MonitoredObjects;

    public int BlendStateClose = 1;
    public int BlendStateDistant = 0;

    public float MinDist = 2.0f;
    public float MaxDist = 10.0f;
    public float Exponent = 2.0f;
    public FalloffType Falloff = FalloffType.Exponential;

    private DeformableProxSystem _proxSystem;

    // Use this for initialization
    void Start()
    {
        _proxSystem = gameObject.GetComponent<DeformableProxSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (MonitoredObjects == null)
            return;

        foreach (DeformableFieldGenerator gen in _proxSystem.FieldGenerators)
        {
            UpdateBlendWeights(gen);
        }
        
    }

    /// <summary>
    /// update the blend weights for the generator based on distance to 
    /// other objects
    /// </summary>
    /// <param name="gen"></param>
    private void UpdateBlendWeights(DeformableFieldGenerator gen)
    {
        Vector3 pos = gen.GetPosition();
        float mindist = float.MaxValue;

        if (MonitoredObjects == null)
            return;

        foreach (Transform t in MonitoredObjects)
        {
            if (t == null)
                continue;

            float dist = (t.position - pos).magnitude;
            if (dist < mindist)
                mindist = dist;
        }

        float ratio = 1.0f;

        switch (Falloff)
        {
            case FalloffType.Linear:
                ratio = ComputeDistantRatio(mindist);
                break;

            case FalloffType.Exponential:
                ratio = ComputeDistantRatioExp(mindist);
                break;
        }		

        gen.SetBlendWeight(BlendStateDistant, ratio);
        gen.SetBlendWeight(BlendStateClose, 1.0f - ratio);

        //_proxSystem.StateBlendWeights[BlendStateDistant] = ratio;
        //_proxSystem.StateBlendWeights[BlendStateClose] = 1.0f - ratio;
    }

    /// <summary>
    /// Compute a value from 0 to 1 of how much to blend the distant state into the close state
    /// </summary>
    /// <param name="dist"></param>
    private float ComputeDistantRatio(float dist)
    {
        dist -= MinDist;
        float ratio = dist / (MaxDist);
        ratio = Mathf.Clamp(ratio, 0, 1);

        return ratio;
    }

    private float ComputeDistantRatioExp(float dist)
    {
        float pow = Exponent;
        float d = Mathf.Pow(dist, pow);
        float numerator = Mathf.Pow(MaxDist + MinDist, pow) * 0.4f;

        float ratio = numerator / d;
        ratio = Mathf.Clamp(ratio, 0, 1);

        return 1.0f - ratio;
    }
}
