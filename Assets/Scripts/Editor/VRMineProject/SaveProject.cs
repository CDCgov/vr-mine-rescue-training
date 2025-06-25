using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.IO;


public class SaveProject : UnityEditor.AssetModificationProcessor {	

    [MenuItem("File/Save Project (hotkey) %&s")]
    public static void SaveProjectShortcut()
    {
        AssetDatabase.SaveAssets();
        Debug.Log("Assets saved.");
    }

    public static string[] OnWillSaveAssets(string[] paths) //This is called whenever you hit Ctrl + S, or use the Save Scene function
    {
        //Get the name of the scene to save
        //string scenePath = string.Empty;
        string sceneName = string.Empty;

        foreach(string path in paths)
        {
            if (path.Contains(".unity"))
            {				
                sceneName = Path.GetFileNameWithoutExtension(path);
            }			
        }
        
        if(sceneName.Length == 0)
        {
            Debug.Log("No scene to save. Project information is saved if this message appears.");
            return paths;
        }

        
        Debug.Log("Saved scene and project.");

        return paths;
    }	
}
