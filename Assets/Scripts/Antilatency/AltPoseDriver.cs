using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;

public class AltPoseDriver : TrackedPoseDriver
{
    public enum TrackingMode
    {
        Mode1,
        Mode2,
        AltDirect,
    };

    public AntilatencyManager AntilatencyManager;
    public SystemManager SystemManager;
    public string AltTag = "HMD";

    public Transform RigTransform;
    public TrackingMode CurrentTrackingMode = TrackingMode.Mode1;

    [NonSerialized]
    public Vector3 Offset;
    [NonSerialized]
    public Quaternion RotOffset;
    [NonSerialized]
    public Quaternion NativeRotation;
    [NonSerialized]
    public Quaternion AltRotation;

    [NonSerialized]
    public Vector3 NativePosition;
    [NonSerialized]
    public Vector3 AltPosition;
    [NonSerialized]
    public Vector3 RawAltPosition;

    private TrackerNode _trackerNode;
    private Quaternion _targetRot;
    private float _targetRotDelta;
    private Vector3 _targetOffset;
    private float _targetOffsetDelta;

    private float _posError;
    private float _angleError;
    private float _lastAngleError = 0;
    private float _rotRecentChange = 0;
    private Quaternion _lastRotation = Quaternion.identity;

    private float[] _yawOffsetSamples;
    private int _currentYawSample;

    protected virtual void Start()
    {
        if (AntilatencyManager == null)
            AntilatencyManager = AntilatencyManager.GetDefault();
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        _yawOffsetSamples = new float[SystemManager.SystemConfig.TrackingNumSamples];
        _currentYawSample = 0;

        Offset = Vector3.zero;
        RotOffset = Quaternion.identity;

        SetTrackingMode(CurrentTrackingMode);

        _trackerNode = AntilatencyManager.GetTrackerByTag(AltTag);
    }

    protected override void PerformUpdate()
    {
        base.PerformUpdate();
    }

    public void SetTrackingMode(TrackingMode mode)
    {
        CurrentTrackingMode = mode;
        Offset = Vector3.zero;
        RotOffset = Quaternion.identity;
        _targetRot = Quaternion.identity;
        _targetOffset = Vector3.zero;

    }

    private System.Text.StringBuilder _sb;
    public string GetStatusText()
    {
        if (_sb == null)
            _sb = new System.Text.StringBuilder();

        _sb.Clear();

        // var correctionRotation = AltRotation * Quaternion.Inverse(NativeRotation);

        // float corAngle;
        // Vector3 corAxis;
        // correctionRotation.ToAngleAxis(out corAngle, out corAxis);
        // var euler = correctionRotation.eulerAngles;

        var correctionRotation = NativeRotation * Quaternion.Inverse(AltRotation);

        float corAngle;
        Vector3 corAxis;
        correctionRotation.ToAngleAxis(out corAngle, out corAxis);

        var euler = correctionRotation.eulerAngles;

        _sb.AppendLine($"CorrAngle: {corAngle:F1}");
        _sb.AppendLine($"CorrEuler: {euler.x:F1} {euler.y:F1} {euler.z:F1}");
        _sb.AppendLine($"RawAltPos: {RawAltPosition.ToString()}");
        _sb.AppendLine($"AltPos   : {AltPosition.ToString()}");
        _sb.AppendLine($"Offset   : {Offset.ToString()}");
        _sb.AppendLine($"PosErr   : {_posError.ToString()}");
        _sb.AppendLine($"AngleErr : {_angleError.ToString()}");
        _sb.AppendLine($"RotChange: {_rotRecentChange:F1}");
        //_sb.AppendLine($"ASDFASDF");

        if (_trackerNode != null)
        {
            _sb.AppendLine($"Stability: {_trackerNode.TrackingStability:F2}");
        }

        return _sb.ToString();
    }

    public void OculusToAlt(ref Vector3 pos, ref Quaternion rot)
    {
        switch (CurrentTrackingMode)
        {
            case TrackingMode.Mode1:
                //pos = RotOffset * pos;
                pos += Offset;

                

                break;

            case TrackingMode.Mode2:
                pos = RotOffset * pos;
                pos += Offset;

                rot = RotOffset * rot;
                break;
        }
   }



    protected override void Update()
    {
        _rotRecentChange -= Time.deltaTime * 60.0f;
        if (_rotRecentChange < 0)
            _rotRecentChange = 0;

        /*
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Offset += new Vector3(0, 0.2f, 0);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Offset -= new Vector3(0, 0.2f, 0);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //RotOffset *= Quaternion.AngleAxis(-10, Vector3.up);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //RotOffset *= Quaternion.AngleAxis(10, Vector3.up);
        }
        */
        base.Update();
    }
}
