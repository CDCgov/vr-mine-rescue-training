using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Camera))]
public class BlockCameraOutOfBounds : MonoBehaviour
{
    public LayerMask OutOfBoundsCullingMask;
    public LayerMask WallsAndFloorMask;
    public LayerMask OutOfBoundsMask;

    public float BoundsRadius = 0.5f;

    private Camera _camera;
    private HDAdditionalCameraData _hdData;

    private int _originalCullingMask;
    private int _outOfBoundsCullingMask;
    private int _wallsAndFloorMask;
    private int _outOfBoundsMask;

    private Collider[] _colliders;
    private bool _outOfBoundsEnabled = false;
    private HDAdditionalCameraData.ClearColorMode _originalClearColorMode = HDAdditionalCameraData.ClearColorMode.None;
    private Color _originalClearColor = Color.black;

    private void Awake()
    {
        _colliders = new Collider[50];
    }

    void Start()
    {
        if (!TryGetComponent<Camera>(out _camera))
        {
            Debug.LogError("BlockCameraOutOfBounds: Couldn't find camera component");
            this.enabled = false;
        }

        TryGetComponent<HDAdditionalCameraData>(out _hdData);

        _originalCullingMask = _camera.cullingMask;
        _outOfBoundsCullingMask = OutOfBoundsCullingMask.value;
        _wallsAndFloorMask = WallsAndFloorMask.value;
        _outOfBoundsMask = OutOfBoundsMask.value;
    }

    void LateUpdate()
    {
        if (!ScenarioSaveLoad.Settings.BlockCameraOutOfBounds)
            return;

        bool outOfBounds = false;

        if (CheckWallIntersection() ||
            CheckNotOverGround() || 
            CheckInOutOfBoundsZone())
        {
            outOfBounds = true;
        }

        if (outOfBounds != _outOfBoundsEnabled)
            SetCameraOutOfBounds(outOfBounds);
    }

    private void SetCameraOutOfBounds(bool outOfBounds)
    {
        if (outOfBounds)
        {
            _camera.cullingMask = _outOfBoundsCullingMask;
            
        }
        else
        {
            _camera.cullingMask = _originalCullingMask;
        }

        if (_hdData != null)
        {
            SetHDCameraOutOfBounds(outOfBounds);
        }

        _outOfBoundsEnabled = outOfBounds;
    }

    private void SetHDCameraOutOfBounds(bool outOfBounds)
    {
        _hdData.customRenderingSettings = true;

        var frameMask = _hdData.renderingPathCustomFrameSettingsOverrideMask;
        frameMask.mask[(uint)FrameSettingsField.Volumetrics] = true;
        _hdData.renderingPathCustomFrameSettingsOverrideMask = frameMask;

        _hdData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, !outOfBounds);

        if (outOfBounds)
        {
            _originalClearColorMode = _hdData.clearColorMode;
            _originalClearColor = _hdData.backgroundColorHDR;

            _hdData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            _hdData.backgroundColorHDR = Color.black;
        }
        else
        {
            _hdData.backgroundColorHDR = _originalClearColor;
            _hdData.clearColorMode = _originalClearColorMode;
        }
    }

    private bool CheckWallIntersection()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, BoundsRadius, _colliders, _wallsAndFloorMask, QueryTriggerInteraction.Ignore);

        if (count > 0)
            return true;
        else
            return false;
    }

    private bool CheckNotOverGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out var hitInfo, 1000, _wallsAndFloorMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool CheckInOutOfBoundsZone()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, BoundsRadius, _colliders, _outOfBoundsMask, QueryTriggerInteraction.Collide);

        if (count > 0)
            return true;
        else
            return false;

    }
}
