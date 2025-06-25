using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectHighlightController))]
public class ObjectHighlightControllerEditor : Editor
{
    private Color _highlightColor;
    private float _opacity;

    public override void OnInspectorGUI()
    {
        ObjectHighlightController highlight = (ObjectHighlightController)target;


        if (Application.isPlaying)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Highlight On"))
            {
                _highlightColor.a = _opacity;
                highlight.ShowHighlight(highlight, _highlightColor, true, 100);
            }

            if (GUILayout.Button("Highlight Off"))
            {
                highlight.ShowHighlight(highlight, Color.green, false, 100);
            }

            if (GUILayout.Button("Select Highlight"))
            {
                ObjectHighlightController.ShowHighlight(highlight.gameObject, highlight, 
                    ObjectHighlightColor.SelectHighlight);
            }

            if (GUILayout.Button("Error Highlight"))
            {
                ObjectHighlightController.ShowHighlight(highlight.gameObject, highlight,
                    ObjectHighlightColor.ErrorHighlight);
            }

            _highlightColor = EditorGUILayout.ColorField("Highlight Color", _highlightColor);            
            _opacity = EditorGUILayout.Slider("Opacity", _opacity, 0, 1);

            GUILayout.EndVertical();
        }

        DrawDefaultInspector();
    }
}
