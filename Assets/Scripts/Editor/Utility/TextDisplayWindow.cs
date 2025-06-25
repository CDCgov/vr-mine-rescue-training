using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextDisplayWindow : EditorWindow
{
    public string WindowText;

    private Vector2 _scrollPos = Vector2.zero;
    private string _filename = "DefaultFilename.txt";

    public static void ShowWindow(string text)
    {
        var window= (TextDisplayWindow)EditorWindow.GetWindow(typeof(TextDisplayWindow));
        window.WindowText = text;
        window.Show();
    }

    private void OnGUI()
    {
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);



        GUILayout.TextArea(WindowText);

        GUILayout.EndScrollView();


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
        {
            SaveToFile(_filename, WindowText);
        }

        _filename = GUILayout.TextField(_filename);

        GUILayout.EndHorizontal();
    }

    private void SaveToFile(string filename, string text)
    {
        if (text == null || text.Length <= 0)
            return;

        var path = Path.GetFullPath(filename);

        if (File.Exists(path))
        {
            Debug.LogError($"File {path} already exists!");
            return;
        }
        
        File.WriteAllText(path, text);

        Debug.Log($"Wrote text to {path}");
    }
}
