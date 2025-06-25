using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VentVFXControl : MonoBehaviour
{
    public VentilationManager VentilationManager;

    public bool SmoothRotation = true;
    public string VFXParameterName = "Air Velocity";
    public float UpdateDelay = 0.1f;
    

    private VisualEffect _vfx;
    private Vector3 _lastVelocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        _vfx = GetComponent<VisualEffect>();
        

        InvokeRepeating(nameof(UpdateVFX), 0, UpdateDelay);
    }

    private void Update()
    {
        transform.rotation = Quaternion.identity;
    }

    void UpdateVFX()
    {
        var graph = VentilationManager.GetVentilationGraph();
        if (graph == null)
            return;

        //var airway = graph.FindClosestAirway(transform.position);
        //if (airway == null)
        //    return;

        //var velocity = airway.ComputeAirVelocity();

        //_vfx.SetVector3(VFXParameterName, velocity);

        var pos = transform.position;
        VentAirway a1, a2;
        if (graph.FindNearbyAirways(pos, out a1, out a2))
        {
            //Vector3 velocity = a1.ComputeAirVelocity() + a2.ComputeAirVelocity();
            //velocity = velocity * 0.5f;

            var a1Pos = (a1.Start.WorldPosition + a1.End.WorldPosition) * 0.5f;
            var a2Pos = (a2.Start.WorldPosition + a2.End.WorldPosition) * 0.5f;

            float d1 = Vector3.Distance(pos, a1Pos);
            float d2 = Vector3.Distance(pos, a2Pos);

            float r1 = d1 / (d1 + d2);
            float r2 = d2 / (d1 + d2);

            var velocity = (a1.ComputeAirVelocity() * r1) + (a2.ComputeAirVelocity() * r2);

            if (SmoothRotation)
            {
                _lastVelocity = Vector3.RotateTowards(_lastVelocity, velocity, 15.0f * Time.deltaTime, 5.0f * Time.deltaTime);

                _vfx.SetVector3(VFXParameterName, _lastVelocity);
            }
            else
            {
                _vfx.SetVector3(VFXParameterName, velocity);
            }
        }

    }
}
