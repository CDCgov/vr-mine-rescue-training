using UnityEngine;
using System.Collections;

public class ProxTestIndicator : MonoBehaviour
{
    public ProxSystemController ProxController;

    private Light _light;
    private MeshRenderer _meshRenderer;

    // Use this for initialization
    void Start()
    {		
        _light = GetComponent<Light>();
        _meshRenderer = GetComponent<MeshRenderer>();

        ProxController.ProxZoneChanged += OnProxZoneChanged;

        OnProxZoneChanged(ProxZone.GreenZone);
    }

    private void OnProxZoneChanged(ProxZone zone)
    {
        switch (zone)
        {
            case ProxZone.RedZone:
                SetColor(Color.red);
                break;

            case ProxZone.YellowZone:
                SetColor(Color.yellow);
                break;

            default:
                SetColor(Color.green);
                break;
        }
    }

    private void SetColor(Color color)
    {
        if (_light != null)
            _light.color = color;

        if (_meshRenderer != null)
            _meshRenderer.material.color = color;
    }
}
