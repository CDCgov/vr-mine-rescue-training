﻿/*************************************************************************
 *  Copyright © 2017-2018 Mogoson. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  RockerRivetEditor.cs
 *  Description  :  Custom editor for RockerRivet.
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
    [CustomEditor(typeof(RockerRivet), true)]
    [CanEditMultipleObjects]
    public class RockerRivetEditor : BaseEditor
    {
        #region Field and Property
        protected RockerRivet Target { get { return target as RockerRivet; } }
        #endregion

        #region Protected Method
        protected virtual void OnSceneGUI()
        {
            if (Target.joint == null)
                return;

            Handles.color = Blue;
            DrawSphereCap(Target.transform.position, Quaternion.identity, NodeSize);

            GUI.color = Blue;
            Handles.Label(Target.transform.position, "Rivet");
        }
        #endregion
    }
}