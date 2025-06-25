using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class RescaleMine : EditorWindow
{

    private Vector3 _scaleFactor;
    private string _outputText;

    private HashSet<int> _scaledObjects;

    [MenuItem("Create Mine/Rescale Mine...")]
    public static void OpenRescaleWindow()
    {
        var window = EditorWindow.GetWindow<RescaleMine>("Rescale Mine");
    }

    private void OnGUI()
    {

        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale X: ");
        _scaleFactor.x = EditorGUILayout.FloatField(_scaleFactor.x);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale Z: ");
        _scaleFactor.z = EditorGUILayout.FloatField(_scaleFactor.z);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale Y: ");
        _scaleFactor.y = EditorGUILayout.FloatField(_scaleFactor.y);
        GUILayout.EndHorizontal();



        if (GUILayout.Button("Rescale Mine"))
        {
            ScaleMine(_scaleFactor);
        }

        GUILayout.Label(_outputText);
    }

    public void ScaleMine(Vector3 scale)
    {
        _scaledObjects = new HashSet<int>();

        Vector3 inverse;
        Vector3 scaleFactor = Vector3.one;
        var mineSegments = GameObject.FindObjectsOfType<MineSegment>();


        foreach (var seg in mineSegments)
        {
            var t = seg.transform;
            scaleFactor = Vector3.one;

            foreach (Transform child in t.transform)
            {
                if (child.name == "Geometry")
                {
                    inverse.x = 1.0f / child.localScale.x;
                    inverse.y = 1.0f / child.localScale.y;
                    inverse.z = 1.0f / child.localScale.z;

                    child.localScale = Vector3.one;

                    //t.position = Vector3.Scale(t.position, inverse);
                    scaleFactor = Vector3.Scale(scaleFactor, inverse);
                }
                if (child.name == "Roofbolts")
                    child.localScale = Vector3.one;

            }

            if (t.localScale != Vector3.one)
            {
                inverse.x = 1.0f / t.localScale.x;
                inverse.y = 1.0f / t.localScale.y;
                inverse.z = 1.0f / t.localScale.z;
                //t.position = Vector3.Scale(t.position, inverse);
                scaleFactor = Vector3.Scale(scaleFactor, inverse);
            }

            t.localScale = scale;
            //t.position = Vector3.Scale(t.position, scale);
            scaleFactor = Vector3.Scale(scaleFactor, scale);
            t.position = Vector3.Scale(t.position, scaleFactor);
        }

        var rigids = GameObject.FindObjectsOfType<Rigidbody>();

        foreach (var rigid in rigids)
        {
            ScaleObjectPos(rigid.transform, scaleFactor);
        }

        var colliders = GameObject.FindObjectsOfType<Collider>();

        foreach (var collider in colliders)
        {
            if (collider.GetComponentInParent<MineSegment>() != null)
                continue;
            if (collider.GetComponent<Rigidbody>() != null)
                continue;
            if (collider.GetComponentInParent<Rigidbody>() != null)
                continue;
            
            if (collider.transform.parent != null && 
                collider.transform.parent.GetComponentInParent<Collider>() != null)
                continue;

            var obj = PrefabUtility.GetOutermostPrefabInstanceRoot(collider.gameObject);
            if (obj == null)
                ScaleObjectPos(collider.transform, scaleFactor);
            else
                ScaleObjectPos(obj.transform, scaleFactor);
        }

        var ventControl = GameObject.FindObjectOfType<VentilationControl>();
        if (ventControl != null)
        {
            ScaleVentilation(ventControl, scaleFactor);
        }

        var teamStops = GameObject.FindObjectsOfType<VRPointOfInterest>();
        foreach (var teamStop in teamStops)
        {
            ScaleObjectPos(teamStop.transform, scaleFactor);
        }

        var mineNetwork = FindObjectOfType<MineNetwork>();
        if (mineNetwork != null)
        {
            mineNetwork.SceneTileScale = scaleFactor;
        }

        _outputText = $"Rescaled {mineSegments.Length} segments scale factor {scaleFactor}";
    }

    void ScaleVentilation(VentilationControl ventControl, Vector3 scaleFactor)
    {
        if (ventControl.VentilationProvider != VentilationProvider.MFIRE)
            return;

        var junctions = ventControl.VentGraph.GetJunctions();

        foreach (var junc in junctions)
        {
            junc.WorldPosition = Vector3.Scale(junc.WorldPosition, scaleFactor);
        }

        ventControl.VFXBounds.center = Vector3.Scale(ventControl.VFXBounds.center, scaleFactor);
        ventControl.VFXBounds.extents = Vector3.Scale(ventControl.VFXBounds.extents, scaleFactor);
    }

    void ScaleObjectPos(Transform t, Vector3 scaleFactor)
    {
        if (_scaledObjects.Contains(t.gameObject.GetInstanceID()))
            return;

        _scaledObjects.Add(t.gameObject.GetInstanceID());
        //var rescaleData = t.gameObject.GetComponent<RescaleObjectData>();
        //if (rescaleData != null)
        //{
        //    t.position = rescaleData.OriginalPos;
        //}
        //else
        //{
        //    rescaleData = t.gameObject.AddComponent<RescaleObjectData>();
        //    rescaleData.OriginalPos = t.position;
        //}

        var pos = t.position;
        pos = Vector3.Scale(pos, scaleFactor);
        pos.y = t.position.y;
        t.position = pos;

        bool applyScale = false;

        var name = t.name.ToUpper();
        if (name.Contains("CURTAIN") || name.Contains("STOPPING"))
        {
            applyScale = true;
        }

        foreach (Transform child in t)
        {
            if (child.name == "GuardGridPillar")
                applyScale = true;
        }

        if (applyScale)
            t.localScale = Vector3.Scale(t.localScale, scaleFactor);
    }
}
