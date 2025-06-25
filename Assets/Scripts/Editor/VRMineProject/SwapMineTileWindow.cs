using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class SwapMineTileWindow : EditorWindow {

    public GameObject Tile;
    public MineSegment CurrentSegment;

    public List<GameObject> SelectedTiles;


    private int comboboxSelection = 0;
    public string[] _tilesets;
    public string[] _tilesetsTruncated;

    public string[] paths;

    Vector2 _scrollPos;
    
    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        comboboxSelection = EditorGUILayout.Popup(comboboxSelection, _tilesetsTruncated);
        if (EditorGUI.EndChangeCheck())
        {
            paths = Directory.GetFiles(_tilesets[comboboxSelection]);
        }
        EditorGUILayout.Space();
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        for(int i = 0; i < paths.Length; i++)
        {
            if(Path.GetExtension(paths[i]) == ".meta")
            {
                continue;
            }
            if (GUILayout.Button(Path.GetFileNameWithoutExtension(paths[i])))
            {
                Debug.Log(paths[i]);
                GameObject source = AssetDatabase.LoadAssetAtPath(paths[i], typeof(GameObject)) as GameObject;
                foreach(GameObject tile in SelectedTiles)
                {
                    GameObject go = PrefabUtility.InstantiatePrefab(source) as GameObject;
                    go.name = tile.name;
                    go.transform.position = tile.transform.position;
                    go.transform.rotation = tile.transform.rotation;
                    DestroyImmediate(tile, false);
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }
}
