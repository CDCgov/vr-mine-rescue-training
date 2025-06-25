using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class UIColorWindow : EditorWindow
{
    public UIColorPalette Palette;

    void OnGUI()
    {
        Palette = EditorGUILayout.ObjectField(Palette, typeof(UIColorPalette), false) as UIColorPalette;
    }
}