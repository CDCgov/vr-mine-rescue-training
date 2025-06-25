using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicVentControl : VentResistancePlane
{
    public GameObject ControllerObject;
    public float MinResistance;
    public float MaxResistance;

    private NetSyncServerLinearConstraint _linearConstraint = null;
    private DoorInteraction _doorInteraction = null;

    private float _currentPosition = -1;
    
    void Start()
    {
        if (ControllerObject == null)
        {
            Debug.LogError($"DynamicVentilationControl {name} missing controller object");
            this.enabled = false;
            return;
        }

        if (MaxResistance < MinResistance)
        {
            Debug.LogError($"DynamicVentilationControl {name} max resistance less than min resistance");
            this.enabled = false;
            return;
        }

        ControllerObject.TryGetComponent<NetSyncServerLinearConstraint>(out _linearConstraint);
        ControllerObject.TryGetComponent<DoorInteraction>(out _doorInteraction);

        if (_currentPosition >= 0)
            SetCurrentPosition(_currentPosition);
    }

    public override void UpdateControlResistance()
    {
        if (ControllerObject == null)
        {
            AddedResistance = 0;
            return;
        }

        float ratio = GetCurrentPosition();
        AddedResistance = MinResistance + ((MaxResistance - MinResistance) * (1.0f - ratio));

        Debug.Log($"Changed {name} resistance to {AddedResistance:F2}");
    }

    public float GetCurrentPosition()
    {
        if (_linearConstraint != null)
        {
            _currentPosition = _linearConstraint.GetPercentAlongPath(ControllerObject.transform.position);            
        }
        else if (_doorInteraction != null)
        {
            _currentPosition = _doorInteraction.IsDoorOpen ? 1 : 0;
        }

        return _currentPosition;
    }

    public void SetCurrentPosition(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1);
        _currentPosition = ratio;

        if (_linearConstraint != null)
        {
            _linearConstraint.GetWorldSpacePoints(out var p1, out var p2);
            var length = Vector3.Distance(p1, p2);
            var dir = (p2 - p1).normalized;

            _linearConstraint.transform.position = p1 + dir * (ratio * length);
        }
        else if (_doorInteraction != null)
        {
            if (ratio > 0.5f && !_doorInteraction.IsDoorOpen)
                _doorInteraction.SetDoorState(true);
            else if (ratio <= 0.5f && _doorInteraction.IsDoorOpen)
                _doorInteraction.SetDoorState(false);
        }
    }
}
