using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VentVFXVectorFieldControl : MonoBehaviour
{
    public VentilationManager VentilationManager;

    public bool SmoothRotation = true;
    public string VFXParameterName = "Air Velocity";
    public float UpdateDelay = 0.1f;
    public bool UpdatePreviousFramePosition = false;


    private VisualEffect _vfx;
    private Vector3 _lastVelocity = Vector3.zero;
    private Vector3 _lastPosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        _vfx = GetComponent<VisualEffect>();

        //VentilationManager.VentilationUpdated += OnVentilationUpdated;
        OnVentilationUpdated();
        
    }

    private void OnEnable()
    {

        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        VentilationManager.VentilationUpdated += OnVentilationUpdated;
    }

    private void OnDisable()
    {
        if (VentilationManager == null)
            return;

        VentilationManager.VentilationUpdated -= OnVentilationUpdated;
    }

    private void Update()
    {
        if (UpdatePreviousFramePosition)
        {
            _vfx.SetVector3("LastFramePosition", _lastPosition);
            _lastPosition = transform.position;
        }
    }

    private void OnVentilationUpdated()
    {
        var ventControl = VentilationManager.GetVentilationControl();
        if (ventControl == null)
            return;

        var field = ventControl.GetVectorField();
        _vfx.SetTexture("VentVectorField", field);

        var gasField = ventControl.GetGasField();
        _vfx.SetTexture("VentGasField", gasField);

        var bounds = ventControl.GetVectorFieldBounds();
        _vfx.SetVector3("FieldCenter", bounds.center);
        _vfx.SetVector3("FieldExtent", bounds.extents);
    }
}
