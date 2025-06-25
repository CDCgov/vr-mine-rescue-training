using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextTexture))]
public class TextTextureInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Update Texture"))
        {
            var textTexture = target as TextTexture;
            if (textTexture != null)
            {
                textTexture.UpdateTexture();
            }
        }
    }
}
