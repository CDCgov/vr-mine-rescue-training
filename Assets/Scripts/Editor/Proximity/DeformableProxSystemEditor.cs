using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#pragma warning disable 0219

[CustomEditor(typeof(DeformableProxSystem))]
public class DeformableProxSystemEditor : Editor
{
    //private readonly string[] _visOptions = { "None", "Yellow Zone", "Red Zone" };

    private bool _enableDeformTool = false;
    private int _deformGeneratorIndex = 0;
    private int _deformShellIndex;
    private float _deformAmplitude = 0.2f;
    private float _deformRadius = 25;

    private string[] _generatorList;
    private string[] _shellNames;

    private class GeneratorEditorOptions
    {
        public bool TrackFieldEquation;
        public bool ApplyDeformBrush;

        public void ResetToDefaults()
        {
            TrackFieldEquation = false;
            ApplyDeformBrush = false;
        }
    }

    private List<GeneratorEditorOptions> _genEditOptions;

    [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
    //[DrawGizmo(GizmoType.NonSelected)]
    public static void OnDrawDeformableProxGizmos(DeformableProxSystem system, GizmoType gizmoType)
    {
        if (Application.isPlaying)
            return;

        if (UnityEditor.Selection.activeGameObject != system.gameObject && !system.ShowYellowZone && !system.ShowRedZone)
            return;

        system.DrawBoxGizmos();
    }

    public override void OnInspectorGUI()
    {
        DeformableProxSystem proxSystem = (DeformableProxSystem)target;
        serializedObject.Update();

        if (proxSystem.FieldGenerators == null)
        {
            proxSystem.FieldGenerators = new List<DeformableFieldGenerator>();
        }

        bool enableDeformTool = GUILayout.Toggle(_enableDeformTool, "Enable Deform Tool", "Button");

        if (enableDeformTool != _enableDeformTool)
        {
            _enableDeformTool = enableDeformTool;
            if (_enableDeformTool)
            {
                Tools.current = Tool.None;
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
            else
            {
                HandleUtility.Repaint();
            }
        }

        if (_generatorList == null || _generatorList.Length != proxSystem.FieldGenerators.Count)
        {
            _generatorList = new string[proxSystem.FieldGenerators.Count];

            for (int i = 0; i < _generatorList.Length; i++)
            {
                _generatorList[i] = string.Format("Generator {0}", i);
            }
        }

        if (_shellNames == null)
        {
            _shellNames = System.Enum.GetNames(typeof(ProxShell));
        }

        if (_enableDeformTool)
        {
            GUILayout.BeginHorizontal();
            _deformGeneratorIndex = EditorGUILayout.Popup(_deformGeneratorIndex, _generatorList);
            _deformShellIndex = EditorGUILayout.Popup(_deformShellIndex, _shellNames);
            GUILayout.EndHorizontal();

            _deformAmplitude = EditorGUILayout.Slider("Amplitude", _deformAmplitude, 0.01f, 0.8f);
            _deformRadius = EditorGUILayout.Slider("Radius", _deformRadius, 10, 80);
        }

        GUILayout.Space(5);


        if (GUILayout.Button("Save Generator State"))
        {
            proxSystem.SaveGenerators();
            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("Save Gens as Copy"))
        {
            proxSystem.SaveAsCopy();
            EditorUtility.SetDirty(target);
        }

        GUILayout.Space(5);


        if (GUILayout.Button("Add Generator"))
        {
            proxSystem.AddGenerator();			
            proxSystem.SaveGenerators();
            EditorUtility.SetDirty(target);
        }

        GUILayout.Space(5);
        
        
        //generate gui section for each generator
        if (proxSystem.FieldGenerators != null)
        {
            if (_genEditOptions == null || _genEditOptions.Count != proxSystem.FieldGenerators.Count)
            {
                _genEditOptions = new List<GeneratorEditorOptions>(proxSystem.FieldGenerators.Count);

                for (int i = 0; i < proxSystem.FieldGenerators.Count; i++)
                {
                    GeneratorEditorOptions opts = new GeneratorEditorOptions();
                    opts.ResetToDefaults();
                    _genEditOptions.Add(opts);
                }

                Debug.Assert(proxSystem.FieldGenerators.Count == _genEditOptions.Count);
            }

            for (int i = 0; i < proxSystem.FieldGenerators.Count; i++)
            {
                DrawGeneratorInspector(proxSystem, proxSystem.FieldGenerators[i], i);
            }
        }

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("No Plate"))
        {
            proxSystem.Ca_ShellBaseShapeConst = 138.2f;
            proxSystem.Da_ShellShapeChangeConst = 0.215f;
            proxSystem.Cb_ShellBaseSizeConst = 1690.4f;
            proxSystem.Db_ShellSizeChangeConst = 0.333f;
        }

        if (GUILayout.Button("With Plate"))
        {
            proxSystem.Ca_ShellBaseShapeConst = 204.87f;
            proxSystem.Da_ShellShapeChangeConst = 0.307f;
            proxSystem.Cb_ShellBaseSizeConst = 2201.0f;
            proxSystem.Db_ShellSizeChangeConst = 0.366f;
        }

        GUILayout.EndHorizontal();		

        GUILayout.Space(5);

        float B_Red = proxSystem.B_Red;
        float B_Yellow = proxSystem.B_Yellow;
        float Ca = proxSystem.Ca_ShellBaseShapeConst;
        float Da = proxSystem.Da_ShellShapeChangeConst;
        float Cb = proxSystem.Cb_ShellBaseSizeConst;
        float Db = proxSystem.Db_ShellSizeChangeConst;

        
        //float ysqrt = Mathf.Sqrt(proxSystem.B_Yellow);
        float yroot = Mathf.Pow(proxSystem.B_Yellow, 1.0f/4.0f);
        float yroot_new = GUILayout.HorizontalSlider(yroot, 0.01f, 3.0f);
        if (yroot != yroot_new)
        {
            proxSystem.B_Yellow = yroot_new * yroot_new * yroot_new * yroot_new;
            EditorUtility.SetDirty(target);
        }

        float rroot = Mathf.Pow(proxSystem.B_Red, 1.0f/4.0f);
        float rroot_new = GUILayout.HorizontalSlider(rroot, 0.01f, 3.0f);
        if (rroot != rroot_new)
        {
            proxSystem.B_Red = rroot_new * rroot_new * rroot_new * rroot_new;
            EditorUtility.SetDirty(target);
        }
        
        DrawDefaultInspector();

        if (B_Red != proxSystem.B_Red ||
            B_Yellow != proxSystem.B_Yellow ||
            Ca != proxSystem.Ca_ShellBaseShapeConst ||
            Da != proxSystem.Da_ShellShapeChangeConst ||
            Cb != proxSystem.Cb_ShellBaseSizeConst ||
            Db != proxSystem.Db_ShellSizeChangeConst)
        {
            if (proxSystem.ActiveState >= 0)
            {
                for (int i = 0; i < proxSystem.FieldGenerators.Count; i++)
                {
                    DeformableFieldGenerator gen = proxSystem.FieldGenerators[i];
                    GeneratorEditorOptions opts = _genEditOptions[i];

                    if (opts.TrackFieldEquation)
                    {
                        proxSystem.ResetGeneratorToFieldEquation(i, proxSystem.ActiveState);
                    }
                }
            }
        }
        
    }


    private void DrawGeneratorInspector(DeformableProxSystem proxSystem, DeformableFieldGenerator gen, int index)
    {
        //DeformableFieldGenerator gen = proxSystem.FieldGenerators[index];

        GeneratorEditorOptions opts = _genEditOptions[index];

        GUILayout.BeginVertical(EditorStyles.helpBox);
        //var generator = generatorArray.GetArrayElementAtIndex(index);
        
        EditorGUI.indentLevel++;

        //GUILayout.Label(string.Format("{0} states", gen.States.Count));
        GUILayout.Label(string.Format("Generator {0}", index));
        for (int i = 0; i < gen.States.Count; i++)
        {
            GUILayout.Label(string.Format("state {0}, yellow: {1}, red: {2}", i, gen.States[i].Shells[0].ShellMap.Length, gen.States[i].Shells[1].ShellMap.Length));
        }

        opts.TrackFieldEquation = GUILayout.Toggle(opts.TrackFieldEquation, "Track field equation changes");

        gen.Position = EditorGUILayout.Vector3Field("Position", gen.Position);
        gen.ParentTransform = (Transform)EditorGUILayout.ObjectField("Parent XForm", gen.ParentTransform, typeof(Transform), true);
        

        //EditorGUILayout.PropertyField(generator, true);
        EditorGUI.indentLevel--;

        if (GUILayout.Button("Delete"))
        {
            //generatorArray.DeleteArrayElementAtIndex(index);
            proxSystem.FieldGenerators.Remove(gen);
        }

        if (GUILayout.Button("Reset to Field Equation") && proxSystem.ActiveState >= 0)
        {			
            proxSystem.ResetGeneratorToFieldEquation(index, proxSystem.ActiveState);
        }

        for (int i = 0; i < gen.StateBlendWeights.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("State {0} Weight", i));
            float weight = GUILayout.HorizontalSlider(gen.GetBlendWeight(i), 0, 1);
            gen.SetBlendWeight(i, weight);
            GUILayout.EndHorizontal();
        }

        gen.NormalizeBlendWeights();

        GUILayout.EndVertical();
    }


    private float _lastTime;

    private void OnSceneGUI()
    {
        DeformableProxSystem proxSystem = (DeformableProxSystem)target;
        if (proxSystem.FieldGenerators == null)
            return;

        int controlID = GUIUtility.GetControlID(FocusType.Passive);


        Event cur = Event.current;

        if (_enableDeformTool && proxSystem.ActiveState >= 0)
        {
            //block mouse interaction if the deform tool is active
            HandleUtility.AddDefaultControl(controlID);

            DeformableFieldGenerator gen = proxSystem.FieldGenerators[_deformGeneratorIndex];


            Vector2 mousePos = Event.current.mousePosition; //Input.mousePosition;
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(mousePos);
            Vector3 mouseWorldPos = mouseRay.origin;//Camera.main.ScreenToWorldPoint(mousePos);


            //Debug.Log(mouseWorldPos);
            //Handles.DrawLine(mouseWorldPos, gen.WorldSpacePosition);

            if (cur.type == EventType.MouseDown && cur.button == 0 && (cur.modifiers & EventModifiers.Alt) <= 0)
            {
            
                if ((cur.modifiers & EventModifiers.Shift) > 0)
                {
                    gen.DeformSurface(mouseRay, proxSystem.ActiveState, (ProxShell)_deformShellIndex, _deformRadius, _deformAmplitude);
                }
                else
                {
                    gen.DeformSurface(mouseRay, proxSystem.ActiveState, (ProxShell)_deformShellIndex, _deformRadius, _deformAmplitude * -1.0f);
                }

            }

            HandleUtility.Repaint();
        }
        else
        {
            //proxSystem.DrawBoxGizmos();

            //proxSystem.ComputeShellsSync();
            //proxSystem.DrawVisShell();

            //proxSystem.DrawVisShellMultithreaded();
            
        }

        

        //Undo.RecordObject(target, "gen position");


        if (!_enableDeformTool)
        {

            for (int i = 0; i < proxSystem.FieldGenerators.Count; i++)
            {
                DeformableFieldGenerator gen = proxSystem.FieldGenerators[i];
                if (gen.ParentTransform == null)
                    continue;

                //Vector3 genWorldPos = proxSystem.transform.TransformPoint(gen.Position);
                Vector3 genWorldPos = gen.WorldSpacePosition;

                //Handles.SelectionFrame(0, genWorldPos, Quaternion.identity, 2.0f);
                Handles.Label(genWorldPos, string.Format("Gen {0}", i));


                /*if (Tools.current == Tool.Rotate)
                {
                    Quaternion newRot = Handles.RotationHandle(gen.Rotation, genWorldPos);
                    if (gen.Rotation != newRot)
                    {
                        Undo.RecordObject(target, "Generator Rotation");
                        gen.Rotation = newRot;
                    }
                }
                else
                {

                    Vector3 newWorldPos = Handles.PositionHandle(genWorldPos, Quaternion.identity);
                    if (newWorldPos != genWorldPos)
                    {
                        Undo.RecordObject(target, "Generator Position");
                        gen.Position = proxSystem.transform.InverseTransformPoint(newWorldPos);
                    }
                }*/

                Vector3 newWorldPos = Handles.PositionHandle(genWorldPos, Quaternion.identity);
                if (newWorldPos != genWorldPos)
                {
                    UnityEditor.Undo.RecordObject(target, "Generator Position");
                    //gen.Position = proxSystem.transform.InverseTransformPoint(newWorldPos);
                    gen.SetGeneratorPositionWorldSpace(newWorldPos);
                }
            }
        }
    }

    private void OnEnable()
    {
        Tools.hidden = true;
    }

    private void OnDisable()
    {
        DeformableProxSystem proxSystem = (DeformableProxSystem)target;
        proxSystem.SaveGenerators();

        Tools.hidden = false;
    }
}
