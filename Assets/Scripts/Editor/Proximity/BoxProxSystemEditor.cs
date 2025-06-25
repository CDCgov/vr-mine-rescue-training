using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityStandardAssets.Vehicles.Car;

[CustomEditor(typeof(BoxProxSystem))]
public class BoxProxSystemEditor : Editor
{
    public const float FEET_TO_METERS = 0.3048f;
    public const float METERS_TO_FEET = 1.0f / FEET_TO_METERS;

    private Vector3[] _gridLines;

    private Bounds _machineBounds;

    private bool _feetGroup = true;
    private bool _yellowOffsetGroup = true;
    private bool _redOffsetGroup = true;

    GUIStyle _labelStyle;
    private static Texture2D _bgTex;

    private void Awake()
    {

        if (_bgTex == null)
        {
            _bgTex = new Texture2D(2, 2);

            Color[] pixels = new Color[_bgTex.width * _bgTex.height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.black;
            }

            _bgTex.SetPixels(pixels);
        }

        //Debug.Log("Editor Awake");
        _labelStyle = new GUIStyle();
        _labelStyle.fontSize = 45;
        _labelStyle.fontStyle = FontStyle.Bold;
        _labelStyle.normal.textColor = Color.white;
    }

    private void OnSceneGUI()
    {
        BoxProxSystem proxSystem = (BoxProxSystem)target;
        if (_machineBounds.extents == Vector3.zero)
            _machineBounds = proxSystem.ComputeMachineBounds();


        GameObject rootObj = PrefabUtility.FindPrefabRoot(proxSystem.gameObject);

        Vector3 origin = proxSystem.transform.position;
        origin += proxSystem.GridOffset;

        EditorUtil.DrawGridLines(origin, proxSystem.GridLineCount, proxSystem.GridLineSpacing, proxSystem.GridColor, ref _gridLines);

        Handles.matrix = Matrix4x4.identity;

        if (rootObj != null)
        {
            Handles.color = Color.green;
            _labelStyle.fontSize = 17;
            _labelStyle.normal.textColor = Color.white;
            _labelStyle.fontStyle = FontStyle.Bold;
            _labelStyle.alignment = TextAnchor.MiddleLeft;
            //_labelStyle.normal.background = _bgTex;

            /*Vector2 guiPos = HandleUtility.WorldToGUIPoint(rootObj.transform.position);
            Handles.BeginGUI();
            //GUI.Box(new Rect(guiPos.x, guiPos.y, 100, 30), "");
            //GUILayout.BeginArea(new Rect(guiPos.x, guiPos.y, 100, 30), _bgTex);
            //GUILayout.Label("Hello Box World", _labelStyle);
            EditorGUI.DropShadowLabel(new Rect(guiPos.x, guiPos.y, 100, 30), "HELLO SHADOW", _labelStyle);
            //GUILayout.EndArea();
            Handles.EndGUI();*/


            //Handles.Label(rootObj.transform.position + new Vector3(0.25f, 0, 0), "Prefab Root Position", _labelStyle);

            //EditorUtil.DrawDropShadowHandlesLabel(rootObj.transform.position + new Vector3(0.25f, 0, 0), "Prefab Root Position", _labelStyle);
            //Handles.DrawSolidDisc(rootObj.transform.position, Vector3.up, 0.2f);

        }

        //Rigidbody rb = rootObj.GetComponent<Rigidbody>();
        Rigidbody rb = proxSystem.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {

            Handles.color = Color.red;
            //EditorUtil.DrawDropShadowHandlesLabel(rb.worldCenterOfMass + new Vector3(0.15f, 0, 0), "Center of Mass", _labelStyle);				
            //Handles.DrawSolidDisc(rb.worldCenterOfMass, Vector3.up, 0.1f);
        }

        //CarController carController = proxSystem.GetComponentInParent<CarController>();
        //if (carController != null)
        //{
        //    Handles.matrix = carController.transform.localToWorldMatrix;

        //    //Handles.color = Color.magenta;
        //    //Vector3 wheelCenterPos = carController.ComputeWheelbaseCenter();
        //    //EditorUtil.DrawDropShadowHandlesLabel(wheelCenterPos + new Vector3(0.15f, 0, 0), "Wheelbase Center", _labelStyle);
        //    //Handles.DrawSolidDisc(wheelCenterPos, Vector3.up, 0.15f);

        //    Handles.color = Color.cyan;
        //    Handles.DrawSolidDisc(carController.ComputeCenterOfMass(), Vector3.up, 0.1f);
        //    Handles.DrawSolidDisc(carController.ComputeCenterOfMass(), Vector3.right, 0.1f);
        //    EditorUtil.DrawDropShadowHandlesLabel(carController.ComputeCenterOfMass() + new Vector3(0.15f, 0, 0), "Center of Mass", _labelStyle);

        //    Handles.matrix = Matrix4x4.identity;
        //}

        //draw machine bounding box
        Handles.color = Color.green;
        Handles.matrix = proxSystem.transform.localToWorldMatrix;
        Handles.DrawWireCube(_machineBounds.center, _machineBounds.size);
        Handles.matrix = Matrix4x4.identity;
    }

    public override void OnInspectorGUI()
    {
        BoxProxSystem proxSystem = (BoxProxSystem)target;

        //if (_machineBounds.extents == Vector3.zero)
        _machineBounds = proxSystem.ComputeMachineBounds();

        GameObject rootObj = PrefabUtility.FindPrefabRoot(proxSystem.gameObject);

        StringBuilder sbHelpText = new StringBuilder();
        if (rootObj != null)
            sbHelpText.AppendFormat("Prefab: {0}", rootObj.name);

        if (rootObj == null)
        {
            Rigidbody rb = proxSystem.GetComponentInParent<Rigidbody>();
            if (rb != null)
                rootObj = rb.gameObject;
        }

        sbHelpText.AppendFormat("Machine Width (ft): {0:F2}, Length (ft): {1:F2}\n", _machineBounds.size.x * METERS_TO_FEET, _machineBounds.size.z * METERS_TO_FEET);
        sbHelpText.AppendFormat("Machine Width (m): {0:F2}, Length (m): {1:F2}\n", _machineBounds.size.x, _machineBounds.size.z);

        bool wheelRadiusMismatch = false;

        //CarController carController = proxSystem.GetComponentInParent<CarController>();
        //if (carController != null && rootObj != null)
        //{
        //    WheelCollider[] wheelCols = carController.GetWheelColliders();
        //    if (wheelCols != null && wheelCols.Length >= 2)
        //    {
        //        //compute wheelbase
        //        List<float> wheelbaseX = new List<float>(10);
        //        List<float> wheelbaseZ = new List<float>(10);


        //        //find all distances in X and Z dimension
        //        for (int i = 0; i < wheelCols.Length - 1; i++)
        //        {
        //            Vector3 w1c = wheelCols[i].transform.position;

        //            for (int j = i + 1; j < wheelCols.Length; j++)
        //            {
        //                Vector3 w2c = wheelCols[j].transform.position;

        //                wheelbaseX.Add(Mathf.Abs(w1c.x - w2c.x));
        //                wheelbaseZ.Add(Mathf.Abs(w1c.z - w2c.z));
        //            }
        //        }

        //        for (int i = 0; i < wheelCols.Length - 1; i++)
        //        {
        //            if (wheelCols[i].radius != wheelCols[i + 1].radius)
        //                wheelRadiusMismatch = true;
        //        }

        //        if (wheelRadiusMismatch)
        //        {
        //            sbHelpText.AppendLine("Wheel Radii do not match!");
        //        }
        //        else
        //        {
        //            sbHelpText.AppendFormat("Wheel Radius (m): {0:F4}\n", wheelCols[0].radius);
        //        }

        //        sbHelpText.Append("Wheel Height (m): ");
        //        for (int i = 0; i < wheelCols.Length - 1; i++)
        //        {
        //            Vector3 localWheelPos = wheelCols[i].transform.TransformPoint(wheelCols[i].center);
        //            localWheelPos = rootObj.transform.InverseTransformPoint(localWheelPos);

        //            sbHelpText.AppendFormat(" {0:F4}", localWheelPos.y);
        //        }
        //        sbHelpText.AppendLine();


        //        wheelbaseX.Sort();
        //        wheelbaseZ.Sort();

        //        sbHelpText.Append("Wheelbase X (m): ");
        //        foreach (float x in wheelbaseX)
        //            sbHelpText.AppendFormat(" {0:F2}", x);
        //        sbHelpText.AppendLine();

        //        sbHelpText.Append("Wheelbase Z (m): ");
        //        foreach (float z in wheelbaseZ)
        //            sbHelpText.AppendFormat(" {0:F2}", z);
        //        sbHelpText.AppendLine();
        //    }
        //}

        EditorGUILayout.LabelField(sbHelpText.ToString(), EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Set Prox Offsets");
        if (GUILayout.Button("L"))
        {
            proxSystem.SetMachineOffsets(_machineBounds, 20, 18, 10, 10, ref proxSystem.YellowZoneBounds);
            proxSystem.SetMachineOffsets(_machineBounds, 15, 15, 7, 7, ref proxSystem.RedZoneBounds);
            proxSystem.CenterOffset = new Vector3(0, 5 * FEET_TO_METERS, 0);
            EditorUtility.SetDirty(target);
        }
        if (GUILayout.Button("S"))
        {
            proxSystem.SetMachineOffsets(_machineBounds, 20, 18, 8, 8, ref proxSystem.YellowZoneBounds);
            proxSystem.SetMachineOffsets(_machineBounds, 10, 10, 4, 4, ref proxSystem.RedZoneBounds);
            proxSystem.CenterOffset = new Vector3(0, 5 * FEET_TO_METERS, 0);
            EditorUtility.SetDirty(target);
        }
        if (GUILayout.Button("XS"))
        {
            proxSystem.SetMachineOffsets(_machineBounds, 10, 10, 6, 6, ref proxSystem.YellowZoneBounds);
            proxSystem.SetMachineOffsets(_machineBounds, 5, 5, 3, 3, ref proxSystem.RedZoneBounds);
            proxSystem.CenterOffset = new Vector3(0, 5 * FEET_TO_METERS, 0);
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();

        _feetGroup = EditorGUILayout.Foldout(_feetGroup, "Feet Extents");
        if (_feetGroup)
        {
            EditorGUI.indentLevel++;
            DrawFeetInspector("Yellow (ft) ", ref proxSystem.YellowZoneBounds);
            DrawFeetInspector("Red (ft) ", ref proxSystem.RedZoneBounds);
            EditorGUI.indentLevel--;
        }

        _yellowOffsetGroup = EditorGUILayout.Foldout(_yellowOffsetGroup, "Yellow Zone Machine Offsets");
        if (_yellowOffsetGroup)
        {
            EditorGUI.indentLevel++;
            DrawMachineOffsetInspector(ref proxSystem.YellowZoneBounds);
            EditorGUI.indentLevel--;
        }

        _redOffsetGroup = EditorGUILayout.Foldout(_redOffsetGroup, "Red Zone Machine Offsets");
        if (_redOffsetGroup)
        {
            EditorGUI.indentLevel++;
            DrawMachineOffsetInspector(ref proxSystem.RedZoneBounds);
            EditorGUI.indentLevel--;
        }


        DrawDefaultInspector();
    }



    private void DrawFeetInspector(string prefix, ref Bounds b)
    {
        float ftX = b.extents.x * METERS_TO_FEET;
        float ftY = b.extents.y * METERS_TO_FEET;
        float ftZ = b.extents.z * METERS_TO_FEET;

        float ftX_new = EditorGUILayout.FloatField(prefix + " X", ftX);
        float ftY_new = EditorGUILayout.FloatField(prefix + " Y", ftY);
        float ftZ_new = EditorGUILayout.FloatField(prefix + " Z", ftZ);

        Vector3 newExtents = Vector3.zero;
        newExtents.x = ftX_new * FEET_TO_METERS;
        newExtents.y = ftY_new * FEET_TO_METERS;
        newExtents.z = ftZ_new * FEET_TO_METERS;

        if (ftX != ftX_new || ftY != ftY_new || ftZ != ftZ_new)
        {
            b.extents = newExtents;
            EditorUtility.SetDirty(target);
        }


    }

    private void DrawMachineOffsetInspector(ref Bounds proxb)
    {
        BoxProxSystem proxSystem = (BoxProxSystem)target;

        float front, back, left, right;
        proxSystem.ComputeMachineOffsets(_machineBounds, proxb, out front, out back, out left, out right);

        EditorGUI.BeginChangeCheck();
        front = EditorGUILayout.FloatField("Front (ft): ", front);
        back = EditorGUILayout.FloatField("Back (ft): ", back);
        left = EditorGUILayout.FloatField("Left (ft): ", left);
        right = EditorGUILayout.FloatField("Right (ft): ", right);

        if (EditorGUI.EndChangeCheck())
        {
            proxSystem.SetMachineOffsets(_machineBounds, front, back, left, right, ref proxb);
            EditorUtility.SetDirty(target);
        }
    }

    // private void ComputeMachineBounds()
    // {
    // 	BoxProxSystem proxSystem = (BoxProxSystem)target;
    // 	Transform tparent = proxSystem.transform.parent;
    // 	if (tparent == null)
    // 		return;

    // 	GameObject parent = proxSystem.transform.parent.gameObject;

    // 	/*

    //     Bounds b = Util.ComputeBounds(parent);
    //     Debug.Log(b.extents);
    //     b.center -= tparent.position;

    //     _machineBounds = b;
    // 	*/

    // 	//compute bounds from attached box collider
    // 	BoxCollider col = parent.GetComponentInChildren<BoxCollider>();
    // 	if (col == null)
    // 		_machineBounds = new Bounds(Vector3.zero, Vector3.one);

    // 	Vector3 center = col.transform.TransformPoint(col.center);
    // 	center = proxSystem.transform.InverseTransformPoint(center);

    // 	Vector3 size = col.transform.TransformDirection(col.size);
    // 	size = proxSystem.transform.InverseTransformDirection(size);

    // 	size.x = Mathf.Abs(size.x);
    // 	size.y = Mathf.Abs(size.y);
    // 	size.z = Mathf.Abs(size.z);

    // 	_machineBounds = new Bounds(center, size);
    // 	//DrawBounds(b, Color.green);
    // }

    // /// <summary>
    // /// copute prox zone offsets off machine bounds in feet
    // /// </summary>
    // /// <param name="proxb"></param>
    // /// <param name="front"></param>
    // /// <param name="back"></param>
    // /// <param name="left"></param>
    // /// <param name="right"></param>
    // private void ComputeMachineOffsets(Bounds proxb, out float front, out float back, out float left, out float right)
    // {
    // 	BoxProxSystem proxSystem = (BoxProxSystem)target;

    // 	Vector3 center = proxSystem.CenterOffset + proxb.center;
    // 	Vector3 mcenter = _machineBounds.center;

    // 	front = (center.z + proxb.extents.z) - (mcenter.z + _machineBounds.extents.z);
    // 	back = (mcenter.z - _machineBounds.extents.z) - (center.z - proxb.extents.z);
    // 	left = (mcenter.x - _machineBounds.extents.x) - (center.x - proxb.extents.x);
    // 	right = (center.x + proxb.extents.x) - (mcenter.x + _machineBounds.extents.x);

    // 	front *= METERS_TO_FEET;
    // 	back *= METERS_TO_FEET;
    // 	left *= METERS_TO_FEET;
    // 	right *= METERS_TO_FEET;
    // }

    // /// <summary>
    // /// set prox zone offsets in feet from machine bounds
    // /// </summary>
    // /// <param name="proxb"></param>
    // /// <param name="front"></param>
    // /// <param name="back"></param>
    // /// <param name="left"></param>
    // /// <param name="right"></param>
    // private void SetMachineOffsets(ref Bounds proxb, float front, float back, float left, float right)
    // {
    // 	BoxProxSystem proxSystem = (BoxProxSystem)target;
    // 	//Vector3 center = proxSystem.CenterOffset + proxb.center;
    // 	Vector3 mcenter = _machineBounds.center;

    // 	Vector3 pext = proxb.extents;
    // 	Vector3 pct = proxb.center;

    // 	front *= FEET_TO_METERS;
    // 	back *= FEET_TO_METERS;
    // 	left *= FEET_TO_METERS;
    // 	right *= FEET_TO_METERS;

    // 	//compute extent by summing machine + front + back
    // 	pext.z = (front + back + _machineBounds.extents.z * 2) * 0.5f;
    // 	//compute center to match front/back alignment			
    // 	pct.z = mcenter.z + ((_machineBounds.extents.z + front) - pext.z);

    // 	//same for sides
    // 	pext.x = (left + right + _machineBounds.extents.x * 2) * 0.5f;
    // 	pct.x = mcenter.x + ((_machineBounds.extents.x + right) - pext.x);

    // 	//set height to 50 feet (extent is half size)
    // 	pext.y = 25 * FEET_TO_METERS;

    // 	proxb.extents = pext;
    // 	proxb.center = pct;
    // }
}
