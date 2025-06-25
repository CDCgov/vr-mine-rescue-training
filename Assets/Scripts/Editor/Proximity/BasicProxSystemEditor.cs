using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(BasicProxSystem))]
public class BasicProxSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BasicProxSystem fieldSystem = (BasicProxSystem)target;
        serializedObject.Update();

        if (GUILayout.Button("Add Generator"))
        {
            BasicProxSystem.FieldGenerator gen = new BasicProxSystem.FieldGenerator();
            fieldSystem.FieldGenerators.Add(gen);
        }

        var fieldGens = serializedObject.FindProperty("FieldGenerators");
        for (int i = 0; i < fieldGens.arraySize; i++)
        {
            DrawGeneratorInspector(fieldGens, i);
        }

        //foreach (FieldGenerator gen in fieldSystem.FieldGenerators)
        //{

        //}

        //DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGeneratorInspector(SerializedProperty generatorArray, int index)
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);
        var generator = generatorArray.GetArrayElementAtIndex(index);

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(generator, true);
        EditorGUI.indentLevel--;
        if (GUILayout.Button("Delete"))
        {
            generatorArray.DeleteArrayElementAtIndex(index);
        }

        GUILayout.EndVertical();
    }
}
