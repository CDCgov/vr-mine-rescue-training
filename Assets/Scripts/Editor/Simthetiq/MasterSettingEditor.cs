using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Editor Script 
/// </summary>


[CustomEditor(typeof(MasterSetting))]

public class MasterSettingEditor : Editor
{
    private new MasterSetting target;
   
    private bool curveButton = false;
    private bool massButton = false;
    private bool wheelButton = false;
    //Wheel Flag For Wheel Settings
    private bool wheelFleft = false;
    private bool wheelFright = false;
    private bool wheelBleft = false;
    private bool wheelBright = false;
    private bool wacb = false;
    //WheelAxisController END

    private readonly GUIStyle ErrorStyle = new GUIStyle();
    private GUIStyle TitleStyle = new GUIStyle();

    void OnEnable()
    {
        if(base.target.GetType() == typeof(MasterSetting)) target = (MasterSetting)base.target;
        SetGuiStyle();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label(AssetDatabase.LoadAssetAtPath<Texture>("Assets\\Editor\\BH20_Editor\\Ressources/Simthetiq.jpg"));
 
        if (GUILayout.Button("Curves Settings")) curveButton = !curveButton;
        if (curveButton)
        {
            GUILayout.Space(15f);
            EditorGUILayout.LabelField("Acceleration Curve", TitleStyle);
            target.acceleration =  EditorGUILayout.CurveField(target.acceleration, GUILayout.Height(100));

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Direction Curve", TitleStyle);
            
            target.directionCurve = EditorGUILayout.CurveField(target.directionCurve, GUILayout.Height(100));

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Breaking Curve", TitleStyle);
            
            target.breakingCurve = EditorGUILayout.CurveField(target.breakingCurve, GUILayout.Height(100));
            GUILayout.Space(15f);
        }

        if (GUILayout.Button("Mass Settings")) massButton = !massButton;
        if (massButton)
        {
            GUILayout.Space(15f);

            EditorGUILayout.LabelField("Mass Settings", TitleStyle);
            target.bH20Rigidbody = (Rigidbody)EditorGUILayout.ObjectField("Front RigidBody", target.bH20Rigidbody, typeof(Rigidbody), true);

            if (target.bH20Rigidbody != null)
            {
                target.bH20Mass = EditorGUILayout.FloatField("Mass",target.bH20Mass);
                target.bH20Drag =  EditorGUILayout.FloatField("Drag",target.bH20Drag);
                target.bH20AngularDrag = EditorGUILayout.FloatField("Angular Drag",target.bH20AngularDrag);

            }
            else
            {
                ShowRefError("RigidBody Missing, Fix it to see the option");
            }


            GUILayout.Space(10);

            if (Application.isPlaying)
            {
                if(GUILayout.Button("Apply")) target.RigidBodyInit();

            }
            GUILayout.Space(15f);
        }

        if (GUILayout.Button("Wheels Settings")) wheelButton = !wheelButton;
        if (wheelButton)
        {
            GUILayout.Space(15f);
            target.frontleft = (WheelCollider)EditorGUILayout.ObjectField("frontleft Wheel Collider", target.frontleft, typeof(WheelCollider), true);
            target.frontRight = (WheelCollider)EditorGUILayout.ObjectField("frontRight Wheel Collider", target.frontRight, typeof(WheelCollider), true);
            target.rearLeft = (WheelCollider)EditorGUILayout.ObjectField("Rear Left Wheel Collider", target.rearLeft, typeof(WheelCollider), true);
            target.rearRight = (WheelCollider)EditorGUILayout.ObjectField("Rear Right Wheel Collider", target.rearRight, typeof(WheelCollider), true);
            GUILayout.Space(15f);
            
            if (CheckWheelRef())
            {
                GUILayout.Space(15f);

                if (GUILayout.Button("Front Wheel Left")) wheelFleft = !wheelFleft;
                if (wheelFleft)
                {
                    ShowWheelParam(ref target.frontleft);
                }

                if (GUILayout.Button("Front Wheel Right")) wheelFright = !wheelFright;
                if (wheelFright)
                {
                    ShowWheelParam(ref target.frontRight);
                }

                if (GUILayout.Button("Rear Wheel Left")) wheelBleft = !wheelBleft;
                if (wheelBleft)
                {
                    ShowWheelParam(ref target.rearLeft);
                }

                if (GUILayout.Button("Rear Wheel Right")) wheelBright = !wheelBright;
                if (wheelBright)
                {
                    ShowWheelParam(ref target.rearRight);
                }

                GUILayout.Space(15f);


                
            }
            else
            {
                ShowRefError("Missing WheelCollider!! Fix it to see the option!");

            }


        }
        if (GUILayout.Button("WheelAxisController Setting")) wacb = !wacb;
        if (wacb)
        {
            GUILayout.Space(10);
            target.frontAxis = (WheelAxisController)EditorGUILayout.ObjectField("Front Axis Script", target.frontAxis, typeof(WheelAxisController), true);
            target.rearAxis = (WheelAxisController)EditorGUILayout.ObjectField("Rear Axis Script", target.rearAxis, typeof(WheelAxisController), true);
            if (CheckAxisRef())
            {
                float mxspeed = EditorGUILayout.FloatField("MaxSpeed (km/h)", target.frontAxis.maxSpeed);
                target.frontAxis.maxSpeed = mxspeed;
                target.rearAxis.maxSpeed = mxspeed;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Front Axis", TitleStyle);

                GUILayout.Space(10);
                target.frontSpdTorque = EditorGUILayout.FloatField("Torque", target.frontSpdTorque);
                target.frontBrkTorque = EditorGUILayout.FloatField("BreakForce", target.frontBrkTorque);

                EditorGUILayout.LabelField("Rear Axis", TitleStyle);
                GUILayout.Space(10);
                target.rearSpdTorque = EditorGUILayout.FloatField("Torque", target.rearSpdTorque);
                target.rearBrkTorque = EditorGUILayout.FloatField("BreakForce", target.rearBrkTorque);

                if (Application.isPlaying)
                {
                    if(GUILayout.Button("Apply")) target.InitWheelAxisController();
                }
            }
            else
            {
                ShowRefError("Missing WheelAxisController Script Reference!! Fix it to see the option!");
            }
        }



        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

    }

    private void ShowWheelParam(ref WheelCollider wc)
    {

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Mass", TitleStyle);
        wc.mass = EditorGUILayout.FloatField("Mass", wc.mass);
        GUILayout.Space(10);
        wc.wheelDampingRate = EditorGUILayout.FloatField("Wheel Damping Rate",wc.wheelDampingRate);
        wc.suspensionDistance = EditorGUILayout.FloatField("Suspension Distance", wc.suspensionDistance);
        wc.forceAppPointDistance = EditorGUILayout.FloatField("Force App Point Distance", wc.forceAppPointDistance);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Suspension", TitleStyle);
        JointSpring tempSpring = wc.suspensionSpring;
        tempSpring.spring = EditorGUILayout.FloatField("Spring", tempSpring.spring);
        tempSpring.damper = EditorGUILayout.FloatField("Damper", tempSpring.damper);
        tempSpring.targetPosition = EditorGUILayout.FloatField("Target Position", tempSpring.targetPosition);
        wc.suspensionSpring = tempSpring;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Forward Friction", TitleStyle);
        WheelFrictionCurve forwardCurve = wc.forwardFriction;
        forwardCurve.extremumSlip = EditorGUILayout.FloatField("Extremum Slip", forwardCurve.extremumSlip);
        forwardCurve.extremumValue = EditorGUILayout.FloatField("Extremum Value", forwardCurve.extremumValue);
        forwardCurve.asymptoteSlip = EditorGUILayout.FloatField("Asymptote Slip", forwardCurve.asymptoteSlip);
        forwardCurve.asymptoteValue = EditorGUILayout.FloatField("Asymptote Value", forwardCurve.asymptoteValue);
        forwardCurve.stiffness = EditorGUILayout.FloatField("Stiffness", forwardCurve.stiffness);
        wc.forwardFriction = forwardCurve;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Sideway Friction", TitleStyle);
        WheelFrictionCurve sidewayCurve = wc.sidewaysFriction;
        sidewayCurve.extremumSlip = EditorGUILayout.FloatField("Extremum Slip", sidewayCurve.extremumSlip);
        sidewayCurve.extremumValue = EditorGUILayout.FloatField("Extremum Value", sidewayCurve.extremumValue);
        sidewayCurve.asymptoteSlip = EditorGUILayout.FloatField("Asymptote Slip", sidewayCurve.asymptoteSlip);
        sidewayCurve.asymptoteValue = EditorGUILayout.FloatField("Asymptote Value", sidewayCurve.asymptoteValue);
        sidewayCurve.stiffness = EditorGUILayout.FloatField("Stiffness", sidewayCurve.stiffness);
        wc.sidewaysFriction = sidewayCurve;
        GUILayout.Space(15);
        if (GUILayout.Button("Apply To All"))
        {
            UnityEditorInternal.ComponentUtility.CopyComponent(wc);
            UnityEditorInternal.ComponentUtility.PasteComponentValues(target.frontleft);
            UnityEditorInternal.ComponentUtility.PasteComponentValues(target.frontRight);
            UnityEditorInternal.ComponentUtility.PasteComponentValues(target.rearLeft);
            UnityEditorInternal.ComponentUtility.PasteComponentValues(target.rearRight);

        }
        GUILayout.Space(15);
    }

    private void ShowJointParam(ref HingeJoint hj, string title)
    {
        GUILayout.Space(15f);
        EditorGUILayout.LabelField(title, TitleStyle);
        hj.useSpring = GUILayout.Toggle(hj.useSpring, "Use Spring");
        if (hj.useSpring)
        {
            JointSpring rotationSpring = hj.spring;
            rotationSpring.spring = EditorGUILayout.FloatField("Spring", rotationSpring.spring);
            rotationSpring.damper = EditorGUILayout.FloatField("Damper", rotationSpring.damper);
            rotationSpring.targetPosition = EditorGUILayout.FloatField("Target Position", rotationSpring.targetPosition);
            hj.spring = rotationSpring;
        }

        hj.useLimits = GUILayout.Toggle(hj.useLimits, "Use Limits");
        if (hj.useLimits)
        {
            JointLimits rotJointLimits = hj.limits;
            rotJointLimits.min = EditorGUILayout.FloatField("Min", rotJointLimits.min);
            rotJointLimits.max = EditorGUILayout.FloatField("Max", rotJointLimits.max);
            rotJointLimits.bounciness = EditorGUILayout.FloatField("Bounciness", rotJointLimits.bounciness);
            rotJointLimits.bounceMinVelocity = EditorGUILayout.FloatField("Min Bounciness", rotJointLimits.bounceMinVelocity);
            hj.limits = rotJointLimits;
        }

        hj.breakForce = EditorGUILayout.FloatField("BreakForce", hj.breakForce);
        hj.breakTorque = EditorGUILayout.FloatField("Break Torque", hj.breakTorque);
    }



    private bool CheckWheelRef()
    {
        if (target.frontleft == null || target.frontRight == null || target.rearLeft == null || target.rearRight == null) return false;
        return true;
    }

    private bool CheckAxisRef()
    {
        if (target.frontAxis == null || target.rearAxis == null) return false;
        return true;
    }

    private void ShowRefError(string s)
    {
        GUILayout.Space(30f);
        EditorGUILayout.LabelField(s, ErrorStyle);
        GUILayout.Space(30f);
    }

    private void SetGuiStyle()
    {
        ErrorStyle.normal.textColor = Color.red;
        ErrorStyle.fontSize = 18;
        TitleStyle.fontSize = 16;
        TitleStyle.alignment = TextAnchor.MiddleCenter;
       
    }
}
