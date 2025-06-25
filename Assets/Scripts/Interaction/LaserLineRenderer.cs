using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserLineRenderer : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public NetworkedObject NetworkedObject;

    public delegate bool LaserRaycastCheck(RaycastHit hit);

    public LineRenderer LineRenderer;
    public LayerMask RaycastLayers;
    public bool ShowLaserWithoutRayHit = true;
    public float VFXActivationThreshold = 0.5f;

    [System.NonSerialized]
    public float ActivationLevel = 0;
    [System.NonSerialized]
    public float RaycastDistance = 0.4f;

    public LaserRaycastCheck RaycastCheckFunction
    {
        set { _checkFunction = value; }
    }

    private VRNLaserPointerState _vrnLaserState;

    //private float _laserActivationLevel;
    private LaserRaycastCheck _checkFunction;

    private void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        if (NetworkedObject != null || TryGetComponent<NetworkedObject>(out NetworkedObject))
        {
            NetworkedObject.RegisterMessageHandler(OnNetObjMessage);
        }
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if (messageType != "LASER")
            return;

        if (_vrnLaserState == null)
        {
            _vrnLaserState = new VRNLaserPointerState();
        }

        _vrnLaserState.IsLaserEnabled = false;
        _vrnLaserState.LaserActivationLevel = 0;
        reader.ReadMessage(_vrnLaserState);

        Debug.Log($"LaserLineRenderer: Network set laser activation level to {_vrnLaserState.LaserActivationLevel:F2}");

        ActivationLevel = _vrnLaserState.LaserActivationLevel;
    }

    private void Update()
    {
        if (ActivationLevel < 0.05f)
        {
            EnableLaser(false);
            return;
        }

        RaycastLaser(ActivationLevel, RaycastDistance, _checkFunction);
    }

    public void EnableLaser(bool enabled)
    {
        if (LineRenderer == null)
            return;

        if (LineRenderer.enabled == enabled)
            return; //no change

        LineRenderer.enabled = enabled;

        if (!enabled)
        {
            DisableVFX();
        }

        if (NetworkManager != null && NetworkedObject != null && NetworkedObject.HasAuthority)
        {
            if (_vrnLaserState == null)
            {
                _vrnLaserState = new VRNLaserPointerState();
                _vrnLaserState.LaserMode = 0;
            }

            _vrnLaserState.IsLaserEnabled = enabled;
            _vrnLaserState.LaserActivationLevel = enabled ? 1.0f : 0.0f;

            NetworkManager.SendNetObjMessage(NetworkedObject.uniqueID, "LASER", _vrnLaserState);
        }
    }

    public void RaycastLaser(float activationLevel, float raycastDistance, LaserRaycastCheck checkFunction)
    {
        if (LineRenderer == null || checkFunction == null)
            return;

        if (activationLevel <= 0)
        {
            EnableLaser(false);
            return;
        }

        var emitter = LineRenderer.transform;
        LineRenderer.widthMultiplier = activationLevel;

        if (Physics.Raycast(emitter.position, emitter.forward, out var hit, raycastDistance, RaycastLayers.value, QueryTriggerInteraction.Ignore) &&
            checkFunction(hit))
        {
            EnableLaser(true);

            Vector3 localPos = emitter.InverseTransformPoint(hit.point);
            LineRenderer.SetPosition(1, localPos);

            if (activationLevel > VFXActivationThreshold)
            {
                float vfxActivation = (activationLevel - 0.5f) * 2.0f;
                UpdateVFX(vfxActivation, hit.point, hit.normal);
            }
            else
            {
                DisableVFX();
            }
        }
        else if (ShowLaserWithoutRayHit)
        {
            EnableLaser(true);

            LineRenderer.SetPosition(1, new Vector3(0, 0, raycastDistance));
            DisableVFX();
        }
        else
        {
            EnableLaser(false);
        }

    }

    private void DisableVFX()
    {
        UpdateVFX(0, Vector3.zero, Vector3.zero);
    }

    void UpdateVFX(float activationLevel, Vector3 pos, Vector3 normal)
    {
        if (activationLevel <= 0)
        {
            //_hitVFX.SetFloat(_spawnRateID, 0);
            return;
        }

        //_hitEffectObj.SetActive(true);

        //_hitVFX.SetFloat(_spawnRateID, activationLevel * _maxSpawnRate);

        //_hitEffectObj.transform.position = pos;
        //_hitEffectObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
    }
}
