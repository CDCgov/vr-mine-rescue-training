using UnityEngine;

public class EmissiveLightControl : MonoBehaviour
{
    public Light SourceLight;
    public GameObject ControlledObject;
    public float IntensityMultiplier = 3.0f;
    public float FalloffStartDistance = 10.0f;
    public float ScaleMultiplier = 1.0f;

    public bool IgnoreCameraDistance = false;

    private Renderer _rend;
    private Material _mat;
    private float _initialScale = 1.0f;
    private float _initialIntensity;
    private LensFlare _lFlare;

    private void Start()
    {
        //_rend = ControlledObject.GetComponent<Renderer>();
        //_mat = _rend.material;

        //_initialScale = ControlledObject.transform.localScale.x;
        _initialIntensity = SourceLight.intensity;
    }

    private void Update()
    {
        Camera cam = Camera.main;
        float dist = 0;
        float intensityMultiplier = IntensityMultiplier;

        if (cam != null)
        {
            if (!IgnoreCameraDistance)
                dist = Vector3.Distance(transform.position, cam.transform.position);
            else
                dist = 5;

            float scaleMult = dist * dist;
            scaleMult = Mathf.Clamp(scaleMult, 1.0f, 5.0f);
            float scale = _initialScale * scaleMult * ScaleMultiplier;

            //ControlledObject.transform.localScale = new Vector3(scale,scale,scale);

            float intensityReduction = FalloffStartDistance / dist;
            intensityReduction = Mathf.Clamp(intensityReduction, 0.0f, 1.0f);

            intensityMultiplier *= intensityReduction;
            SourceLight.intensity = Mathf.Clamp(_initialIntensity * intensityMultiplier, 0, _initialIntensity);
        }


        //Color c = SourceLight.color;
        //if (c.r < 0.25f)
        //    c.r = 0;
        //if (c.g < 0.25f)
        //    c.g = 0;
        //if (c.b < 0.25f)
        //    c.b = 0;

        //c *= intensityMultiplier;
        //_mat.SetColor("_EmissionColor", c);

    }
}