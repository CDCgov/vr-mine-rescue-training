﻿/*************************************************************************
 *  Copyright © 2017-2018 Mogoson. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  FreeCrankEditor.cs
 *  Description  :  Custom editor for FreeCrank.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  0.1.0
 *  Date         :  4/11/2018
 *  Description  :  Initial development version.
 *************************************************************************/

using UnityEditor;
using UnityEngine;

namespace Mogoson.Machinery
{
    [CustomEditor(typeof(FreeCrank), true)]
    [CanEditMultipleObjects]
    public class FreeCrankEditor : BaseEditor
    {
        #region Field and Property
        protected FreeCrank Target { get { return target as FreeCrank; } }

        protected Vector3 Axis { get { return Target.transform.forward; } }

        protected Vector3 ZeroAxis
        {
            get
            {
                if (Application.isPlaying)
                {
                    var up = Quaternion.Euler(Target.StartAngles) * Vector3.up;
                    if (Target.transform.parent)
                        up = Target.transform.parent.rotation * up;
                    return up;
                }
                else
                    return Target.transform.up;
            }
        }
        #endregion

        #region Protected Method
        protected virtual void OnSceneGUI()
        {
            Handles.color = Blue;
            DrawSphereCap(Target.transform.position, Quaternion.identity, NodeSize);
            DrawCircleCap(Target.transform.position, Target.transform.rotation, AreaRadius);
            DrawSphereArrow(Target.transform.position, Axis, ArrowLength, NodeSize, Blue, "Axis");
            DrawSphereArrow(Target.transform.position, ZeroAxis, ArrowLength, NodeSize, Blue, "Zero");
            DrawSphereArrow(Target.transform.position, Target.transform.up, AreaRadius, NodeSize, Blue, string.Empty);
            DrawArea();
            DrawRockers(Target.rockers, Target.transform, Blue);
        }

        protected virtual void DrawArea()
        {
            Handles.color = TransparentBlue;
            Handles.DrawSolidDisc(Target.transform.position, Axis, AreaRadius);
        }
        #endregion
    }
}