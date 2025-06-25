using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#pragma warning disable 219

//(0)N-N, (1)N-M, (2)N-F, (3)M-M, (4)M-F, (5)F-F, (6)F-M, (7)F-N, (8)M-N, Connection Table replaces this usage entirely
public enum DustConnections
{
    None_None,
    None_Medium,
    None_Full,
    Medium_Medium,
    Medium_Full,
    Full_Full,
    Full_Medium,
    Full_None,
    Medium_None,
    Full,
    Medium,
    None
}


public class CreateMineWindow : EditorWindow
{	
    int _entries = 3;
    int _crosscuts = 3;
    string _sectionName = "Mains";
    int _tileSelection = 0;
    int _rockDustSelection = 0;
    private static Vector3 _sectionStartPosition = Vector3.zero;
    private static Vector3 _spawningSectionNormals = Vector3.zero;
    private static bool _isBranchSection = false;

    static string[] _tilesets;

    private static string[] _tilesetsTruncated;
    private static string[] _rockDustLevel;
    GameObject Select;
    Rect buttonRect;

    [MenuItem("Create Mine/Load From File (demo)", priority = 100)]
    public static void LoadFromFile()
    {
        Debug.Log(EditorUtility.OpenFilePanel("Load Mine Information", Application.dataPath, "txt"));
    }

    [MenuItem("Create Mine/Test Asset Database", priority = 100)]
    public static void AssetDataBaseTester()
    {        
        DirectoryInfo dirInfo = new DirectoryInfo("Assets/Tilesets/Adv2");
        //DirectoryInfo[] directs = dirInfo.GetDirectories();
        
        FileInfo[] fileInfo = dirInfo.GetFiles("*.prefab");
        Debug.Log(fileInfo.Length);


        //Debug.Log(AssetDatabase.GetAssetPath(Selection.activeGameObject));
        //Add tiles to memory for instantiation
        List<GameObject> prefabs = new List<GameObject>();
        foreach(FileInfo fileInf in fileInfo)
        {
            string fullPath = fileInf.FullName.Replace(@"\", "/");
            string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
            GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
            if (prefab != null)
            {
                GameObject load = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                load.name = load.name + " SUCCESS";
            }
        }		
    }

    [MenuItem("Create Mine/Create Mains", priority = 100)]
    public static void CoarseParamWindow()
    {
        _sectionStartPosition = Vector3.zero;
        _isBranchSection = false;
        string path = Path.Combine(Application.dataPath, "/Tilesets/");
        DirectoryInfo dirInfo = new DirectoryInfo("Assets/Tilesets");
        DirectoryInfo[] directs = dirInfo.GetDirectories();
        string[] paths = new string[directs.Length];
        for(int i=0; i< paths.Length; i++)
        {
            paths[i] = directs[i].ToString();
            Debug.Log(paths[i]);
        }
        //CreateMineWindow.Tilesets = Directory.GetDirectories(path);
        CreateMineWindow._tilesets = paths;
        _tilesetsTruncated = new string[CreateMineWindow._tilesets.Length];
     

        for (int i = 0; i < _tilesetsTruncated.Length; i++)
        {
            string stopAt = "\\";
            int lastIndex = CreateMineWindow._tilesets[i].Length-1;
            while (!CreateMineWindow._tilesets[i].Substring(lastIndex,1).Equals(stopAt))
            {
                lastIndex--;
            }
            int location = lastIndex;
            _tilesetsTruncated[i] = CreateMineWindow._tilesets[i].Substring(location+1);
        }
        
        EditorWindow window = EditorWindow.CreateInstance<CreateMineWindow>();
        window.minSize = new Vector2(450, 250);
        window.Show();
    }

    //This needs to be updated to reflect Connection Table changes and the new tile sets.
    public static void BranchWindow(Vector3 position, Quaternion rotation, Vector3 normal)
    {
        _sectionStartPosition = position;
        _isBranchSection = true;
        _spawningSectionNormals = normal;
        string path = Path.Combine(Application.dataPath, "/Tilesets/");
        DirectoryInfo dirInfo = new DirectoryInfo("Assets/Tilesets");
        DirectoryInfo[] directs = dirInfo.GetDirectories();
        string[] paths = new string[directs.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i] = directs[i].ToString();
            Debug.Log(paths[i]);
        }
        //CreateMineWindow.Tilesets = Directory.GetDirectories(path);
        CreateMineWindow._tilesets = paths;
        _tilesetsTruncated = new string[CreateMineWindow._tilesets.Length];


        for (int i = 0; i < _tilesetsTruncated.Length; i++)
        {
            string stopAt = "\\";
            int lastIndex = CreateMineWindow._tilesets[i].Length - 1;
            while (!CreateMineWindow._tilesets[i].Substring(lastIndex, 1).Equals(stopAt))
            {
                lastIndex--;
            }
            int location = lastIndex;
            _tilesetsTruncated[i] = CreateMineWindow._tilesets[i].Substring(location + 1);
        }

        EditorWindow window = EditorWindow.CreateInstance<CreateMineWindow>();
        window.Show();
    }

    /// <summary>
    /// This OnGui populates the window to enter your mine parameters
    /// </summary>
    void OnGUI()
    {
        {
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 12;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(1,0.65f,0);
            titleStyle.alignment = TextAnchor.UpperLeft;

            EditorGUILayout.Space();
            GUILayout.Label("  Enter Mine Section Parameters", titleStyle);
            EditorGUILayout.Space();
            //These Gui Styles can be altered to tweak how the create mine window works
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = 15;
            labelStyle.normal.textColor = Color.white;

            GUIStyle textInputFieldStyle = new GUIStyle();
            textInputFieldStyle.fontSize = 15;
            textInputFieldStyle.normal.textColor = Color.white;

            GUIStyle popupStyle = new GUIStyle(EditorStyles.popup);
            popupStyle.fontSize = 15;
            popupStyle.normal.textColor = Color.white;
            popupStyle.fixedHeight = 40;

            GUIStyle buttonTextStyle = new GUIStyle(GUI.skin.button);            
            buttonTextStyle.fontSize = 15;
            buttonTextStyle.normal.textColor = Color.green;

            //GUIStyle entriesIntFieldStyle = new GUIStyle(); 

            //Constructs the interactable buttons, using the styles defined above
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Section Name:");            
            //_sectionName = EditorGUI.TextField(new Rect(100,60,300,20), _sectionName);
            _sectionName = EditorGUILayout.TextField(_sectionName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Entries:");            
            _entries = EditorGUILayout.IntField(_entries);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Crosscuts:");            
            _crosscuts = EditorGUILayout.IntField(_crosscuts);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tileset:");
            _tileSelection = EditorGUILayout.Popup(_tileSelection, _tilesetsTruncated);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rockdust:");

            //GUI.Label(new Rect(30, 60, 300, 30), "Section Name:", labelStyle);
            //GUI.Box(new Rect(300, 60, 300, 30), "");
            //_sectionName = EditorGUI.TextField(new Rect(300, 60, 300, 20), _sectionName, textInputFieldStyle);
            //GUI.Label(new Rect(30, 120, 300, 30), "Entries:", labelStyle);
            //GUI.Box(new Rect(300, 120, 100, 30), "");
            //_entries = EditorGUI.IntField(new Rect(300, 120, 100, 20), _entries, textInputFieldStyle);
            //GUI.Label(new Rect(30, 180, 300, 30), "Crosscuts:", labelStyle);
            //GUI.Box(new Rect(300, 180, 100, 30), "");
            //_crosscuts = EditorGUI.IntField(new Rect(300, 180, 100, 20), _crosscuts, textInputFieldStyle);
            //GUI.Label(new Rect(30, 240, 300, 30), "Tileset:", labelStyle);
            //_tileSelection = EditorGUI.Popup(new Rect(300, 240, 300, 20), _tileSelection, _tilesetsTruncated, popupStyle);
            //GUI.Label(new Rect(30, 300, 300, 30), "Rockdust:", labelStyle);

            _rockDustLevel = new string[4];
            _rockDustLevel[0] = "None";
            _rockDustLevel[1] = "Medium";
            _rockDustLevel[2] = "Full";
            _rockDustLevel[3] = "Auto";
            _rockDustSelection = EditorGUILayout.Popup(_rockDustSelection,_rockDustLevel);

            //EditorGUILayout.LabelField("Section Start");
            //Select = EditorGUILayout.ObjectField(Select, typeof(GameObject), true) as GameObject;
            EditorGUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create Section", buttonTextStyle))
            {
                //Can't build mine if crosscuts and entries <= 2
                if(_crosscuts <= 2 || _entries <= 2)
                {
                    Debug.LogError("Cannot build a mine if crosscuts and entries are less than 2");
                    return;
                }
                string[] files = Directory.GetFiles(CreateMineWindow._tilesets[_tileSelection]);

                Debug.Log("Assets/Tilesets/" + _tilesetsTruncated[_tileSelection]);
                DirectoryInfo dirInfo = new DirectoryInfo("Assets/Tilesets/" + _tilesetsTruncated[_tileSelection]);
                FileInfo[] fileInfArr = dirInfo.GetFiles("*.prefab");

                //Add tile references to memory for later instantiation
                List<GameObject> tilePrefabs = new List<GameObject>();
                foreach(FileInfo fileInf in fileInfArr)
                {
                    string fullPath = fileInf.FullName.Replace(@"\", "/");
                    string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
                    tilePrefabs.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject);
                    Debug.Log(assetPath);
                }

                
                if (GameObject.Find("Master Controller") == null)
                {
                    GameObject go = new GameObject();
                    go.name = "Master Controller";
                }
                GameObject mine = GameObject.Find("Mine");
                if (mine == null)
                {
                    mine = new GameObject();
                    mine.name = "Mine";
                }
                

                GameObject parent = new GameObject();
                parent.name = _sectionName;
                parent.transform.parent = mine.transform;
                parent.transform.position = _sectionStartPosition;                
                
                List<string> fourWays = new List<string>();
                List<string> threeWays = new List<string>();
                List<string> straightsEW = new List<string>();
                List<string> straightsNS = new List<string>();
                List<string> corners = new List<string>();
                List<string> endCaps = new List<string>();

                if (_isBranchSection)
                {

                }
                else
                {
                    //Creating a mine
                    if (_rockDustSelection == 0)
                    {
                        InstantiateMine(_entries, _crosscuts, tilePrefabs, parent.transform, 0);
                    }
                    else if (_rockDustSelection == 1)
                    {
                        InstantiateMine(_entries, _crosscuts, tilePrefabs, parent.transform, 1);
                    }
                    else if (_rockDustSelection == 2)
                    {
                        InstantiateMine(_entries, _crosscuts, tilePrefabs, parent.transform, 2);
                    }
                    else if (_rockDustSelection == 3)
                    {
                        InstantiateMine(_entries, _crosscuts, tilePrefabs, parent.transform, 3);
                    }
                }
                

                this.Close();
            }
            EditorGUILayout.Space();
        }
    }

    /// <summary>
    /// Takes input from the create mine window and will 
    /// </summary>
    /// <param name="entries"></param>
    /// <param name="crosscuts"></param>
    /// <param name="tilePrefabs"></param>
    /// <param name="parent"></param>
    /// <param name="dustLevel"></param>
    void InstantiateMine(int entries, int crosscuts, List<GameObject> tilePrefabs, Transform parent, int dustLevel) {
        string type = "None";
        string straightType = "None_None";
        if (dustLevel == 1) {
            type = "Medium";
            straightType = "Medium_Medium";
        }
        if (dustLevel == 2) {
            type = "Full";
            straightType = "Full_Full";
        }
        if(dustLevel == 3)
        {
            type = "Full";
            straightType = "Full_Full";
        }
        for (int i = 0; i < entries; i++) {
            if (i > 0 && i < (entries -1))
            {
                for (int j = 0; j < crosscuts; j++) 
                {
                    if(dustLevel == 3)
                    {
                        if (j < crosscuts - 2)
                        {
                            type = "Full";
                            straightType = "Full_Full";
                        }
                        else if(j == crosscuts - 2)
                        {
                            type = "Medium";
                        }
                        else
                        {
                            type = "None";
                        }
                    }
                    Object toLoad = null;
                    foreach (GameObject go in tilePrefabs)
                    {
                        if (go.name.Contains("4Way") && go.name.Contains(type))
                        {
                            toLoad = go;
                        }
                    }
                    GameObject section = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
                    section.transform.parent = parent.transform;
                    section.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_" + (j + 1);
                    section.transform.position = new Vector3((i * 24), 0, (j * 24));
                    AddCreateMinePlacementHelper (section, dustLevel);
                        //dust[i][j] = DustingHelper.Full;
                }
            }
            else
            {
                if (i == 0)
                {
                    for (int j = 0; j < crosscuts; j++)
                    {
                        if (dustLevel == 3)
                        {
                            if (j < crosscuts - 2)
                            {
                                type = "Full";
                                straightType = "Full_Full";
                            }
                            else if (j == crosscuts - 2)
                            {
                                type = "Medium";
                            }
                            else
                            {
                                type = "None";
                            }
                        }
                        Object toLoad = null;
                        foreach (GameObject go in tilePrefabs)
                        {
                            if (go.name.Contains("T_West") && go.name.Contains(type))
                            {
                                toLoad = go;
                            }
                        }
                        GameObject section = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
                        section.transform.parent = parent.transform;
                        section.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_" + (j + 1);
                        section.transform.position = new Vector3((i * 24), 0, (j * 24));
                        AddCreateMinePlacementHelper (section, dustLevel);
                    }
                }
                else
                {
                    for (int j = 0; j < crosscuts; j++)
                    {
                        if (dustLevel == 3)
                        {
                            if (j < crosscuts - 2)
                            {
                                type = "Full";
                                straightType = "Full_Full";
                            }
                            else if (j == crosscuts - 2)
                            {
                                type = "Medium";
                            }
                            else
                            {
                                type = "None";
                            }
                        }
                        Object toLoad = null;
                        foreach (GameObject go in tilePrefabs)
                        {
                            if (go.name.Contains("T_East") && go.name.Contains(type))
                            {
                                toLoad = go;
                            }
                        }
                        GameObject section = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
                        section.transform.parent = parent.transform;
                        section.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_" + (j + 1);
                        section.transform.position = new Vector3((i * 24), 0, (j * 24));
                        AddCreateMinePlacementHelper (section, dustLevel);
                    }
                }
            }
            Object toLoadEnd = null;
            foreach (GameObject go in tilePrefabs)
            {
                if(dustLevel == 3)
                {
                    type = "None";
                }
                if (go.name.Contains("End_North") && go.name.Contains(type))
                {
                    toLoadEnd = go;
                }
            }
            GameObject end = PrefabUtility.InstantiatePrefab(toLoadEnd) as GameObject;
            end.transform.parent = parent.transform;
            end.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_END";
            end.transform.position = new Vector3((i * 24), 0, ((crosscuts) * 24)-12);
            AddCreateMinePlacementHelper (end, dustLevel);
        //Spawning EW straights
        }
        for(int i = 0; i < entries-1; i++)
        {
            for(int j = 0; j < crosscuts; j++)
            {
                if (dustLevel == 3)
                {
                    if (j < crosscuts - 2)
                    {
                        straightType = "Full_Full";
                    }
                    else if (j == crosscuts - 2)
                    {
                        straightType = "Medium_Medium";
                    }
                    else
                    {
                        straightType = "None_None";
                    }
                    
                }
                Object toLoadEW = null;
                foreach (GameObject go in tilePrefabs)
                {
                    if (go.name.Contains("EW") && go.name.Contains(straightType))
                    {
                        toLoadEW = go;
                    }
                }
                GameObject crossCut = PrefabUtility.InstantiatePrefab(toLoadEW) as GameObject;
                crossCut.transform.parent = parent.transform;
                crossCut.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "-" + (i + 2) + "_Crosscut_" + (j + 1);
                crossCut.transform.position = new Vector3((i * 24)+12, 0, ((j) * 24));
                AddCreateMinePlacementHelper (crossCut, dustLevel);
            }
        }
        //Spawn NS straights
        for (int i = 0; i < entries; i++)
        {
            for (int j = -1; j < crosscuts; j++)
            {
                if (dustLevel == 3)
                {
                    if (j < crosscuts - 3)
                    {                       
                        straightType = "Full_Full";
                    }
                    else if (j == crosscuts - 3)
                    {
                        straightType = "Full_Medium";
                    }
                    else if (j == crosscuts -2)
                    {
                        straightType = "Medium_None";
                    }
                    else
                    {
                        straightType = "None_None";
                    }
                }
                Object toLoadNS = null;
                foreach (GameObject go in tilePrefabs)
                {
                    if (go.name.Contains("NS") && go.name.Contains(straightType))
                    {
                        toLoadNS = go;
                    }
                }
                GameObject entry = PrefabUtility.InstantiatePrefab(toLoadNS) as GameObject;
                entry.transform.parent = parent.transform;
                entry.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_" + (j + 1) + "-" + (j+2);
                entry.transform.position = new Vector3((i * 24), 0, ((j) * 24) + 12);
                AddCreateMinePlacementHelper (entry, dustLevel);
            }
        }
    }

    void InstantiateBranch(int entries, int crosscuts, List<GameObject> tilePrefabs, Transform parent, int dustLevel)
    {
        string type = "None";
        string straightType = "None_None";
        if (dustLevel == 1)
        {
            type = "Medium";
            straightType = "Medium_Medium";
        }
        if (dustLevel == 2)
        {
            type = "Full";
            straightType = "Full_Full";
        }

        if (_spawningSectionNormals.z > 0)
        {
            for (int i = 0; i < entries; i++)
            {
                if (i > 0 && i < (entries - 1))
                {
                    for (int j = 1; j < crosscuts; j++)
                    {
                        Object toLoad = null;
                        foreach (GameObject go in tilePrefabs)
                        {
                            if (go.name.Contains("4Way") && go.name.Contains(type))
                            {
                                toLoad = go;
                            }
                        }
                        GameObject section = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
                        section.transform.parent = parent.transform;
                        section.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_" + (j + 1);
                        Vector3 pos = new Vector3((i * 24), 0, (j * 24));
                        section.transform.position = _sectionStartPosition + pos;                        
                        AddCreateMinePlacementHelper(section, dustLevel);
                        //dust[i][j] = DustingHelper.Full;
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        for (int j = 0; j < crosscuts; j++)
                        {
                            Object toLoad = null;
                            foreach (GameObject go in tilePrefabs)
                            {
                                if (go.name.Contains("T_West") && go.name.Contains(type))
                                {
                                    toLoad = go;
                                }
                            }
                            GameObject section = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
                            section.transform.parent = parent.transform;
                            section.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_" + (j + 1);
                            section.transform.localPosition = new Vector3((i * 24), 0, (j * 24));
                            AddCreateMinePlacementHelper(section, dustLevel);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < crosscuts; j++)
                        {
                            Object toLoad = null;
                            foreach (GameObject go in tilePrefabs)
                            {
                                if (go.name.Contains("T_East") && go.name.Contains(type))
                                {
                                    toLoad = go;
                                }
                            }
                            GameObject section = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
                            section.transform.parent = parent.transform;
                            section.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_" + (j + 1);
                            section.transform.localPosition = new Vector3((i * 24), 0, (j * 24));
                            AddCreateMinePlacementHelper(section, dustLevel);
                        }
                    }
                }
                Object toLoadEnd = null;
                foreach (GameObject go in tilePrefabs)
                {
                    if (go.name.Contains("End_North") && go.name.Contains(type))
                    {
                        toLoadEnd = go;
                    }
                }
                GameObject end = PrefabUtility.InstantiatePrefab(toLoadEnd) as GameObject;
                end.transform.parent = parent.transform;
                end.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_END";
                end.transform.localPosition = new Vector3((i * 24), 0, ((crosscuts) * 24) - 12);
                AddCreateMinePlacementHelper(end, dustLevel);
                //Spawning EW straights
            }
            for (int i = 0; i < entries - 1; i++)
            {
                for (int j = 0; j < crosscuts; j++)
                {
                    Object toLoadEW = null;
                    foreach (GameObject go in tilePrefabs)
                    {
                        if (go.name.Contains("EW") && go.name.Contains(straightType))
                        {
                            toLoadEW = go;
                        }
                    }
                    GameObject crossCut = PrefabUtility.InstantiatePrefab(toLoadEW) as GameObject;
                    crossCut.transform.parent = parent.transform;
                    crossCut.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "-" + (i + 2) + "_Crosscut_" + (j + 1);
                    crossCut.transform.localPosition = new Vector3((i * 24) + 12, 0, ((j) * 24));
                    AddCreateMinePlacementHelper(crossCut, dustLevel);
                }
            }
            //Spawn NS straights
            for (int i = 0; i < entries; i++)
            {
                for (int j = -1; j < crosscuts; j++)
                {
                    Object toLoadNS = null;
                    foreach (GameObject go in tilePrefabs)
                    {
                        if (go.name.Contains("NS") && go.name.Contains(straightType))
                        {
                            toLoadNS = go;
                        }
                    }
                    GameObject entry = PrefabUtility.InstantiatePrefab(toLoadNS) as GameObject;
                    entry.transform.parent = parent.transform;
                    entry.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_" + (j + 1) + "-" + (j + 2);
                    entry.transform.localPosition = new Vector3((i * 24), 0, ((j) * 24) + 12);
                    AddCreateMinePlacementHelper(entry, dustLevel);
                }
            }
        }
        else if(_spawningSectionNormals.z < 0)
        {
            for (int i = 0; i < entries; i++)
            {
                if (i > 0 && i < (entries - 1))
                {
                    for (int j = 1; j < crosscuts; j++)
                    {
                        Object toLoad = null;
                        foreach (GameObject go in tilePrefabs)
                        {
                            if (go.name.Contains("4Way") && go.name.Contains(type))
                            {
                                toLoad = go;
                            }
                        }
                        GameObject section = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
                        section.transform.parent = parent.transform;
                        section.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_" + (j + 1);
                        Vector3 pos = new Vector3((i * 24), 0, (j * -24));
                        section.transform.position = _sectionStartPosition + pos;
                        AddCreateMinePlacementHelper(section, dustLevel);
                        //dust[i][j] = DustingHelper.Full;
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        for (int j = 0; j < crosscuts; j++)
                        {
                            Object toLoad = null;
                            foreach (GameObject go in tilePrefabs)
                            {
                                if (go.name.Contains("T_West") && go.name.Contains(type))
                                {
                                    toLoad = go;
                                }
                            }
                            GameObject section = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
                            section.transform.parent = parent.transform;
                            section.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_" + (j + 1);
                            section.transform.localPosition = new Vector3((i * 24), 0, (j * 24));
                            AddCreateMinePlacementHelper(section, dustLevel);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < crosscuts; j++)
                        {
                            Object toLoad = null;
                            foreach (GameObject go in tilePrefabs)
                            {
                                if (go.name.Contains("T_East") && go.name.Contains(type))
                                {
                                    toLoad = go;
                                }
                            }
                            GameObject section = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
                            section.transform.parent = parent.transform;
                            section.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_" + (j + 1);
                            section.transform.localPosition = new Vector3((i * 24), 0, (j * 24));
                            AddCreateMinePlacementHelper(section, dustLevel);
                        }
                    }
                }
                Object toLoadEnd = null;
                foreach (GameObject go in tilePrefabs)
                {
                    if (go.name.Contains("End_North") && go.name.Contains(type))
                    {
                        toLoadEnd = go;
                    }
                }
                GameObject end = PrefabUtility.InstantiatePrefab(toLoadEnd) as GameObject;
                end.transform.parent = parent.transform;
                end.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_END";
                end.transform.localPosition = new Vector3((i * 24), 0, ((crosscuts) * 24) - 12);
                AddCreateMinePlacementHelper(end, dustLevel);
                //Spawning EW straights
            }
            for (int i = 0; i < entries - 1; i++)
            {
                for (int j = 0; j < crosscuts; j++)
                {
                    Object toLoadEW = null;
                    foreach (GameObject go in tilePrefabs)
                    {
                        if (go.name.Contains("EW") && go.name.Contains(straightType))
                        {
                            toLoadEW = go;
                        }
                    }
                    GameObject crossCut = PrefabUtility.InstantiatePrefab(toLoadEW) as GameObject;
                    crossCut.transform.parent = parent.transform;
                    crossCut.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "-" + (i + 2) + "_Crosscut_" + (j + 1);
                    crossCut.transform.localPosition = new Vector3((i * 24) + 12, 0, ((j) * 24));
                    AddCreateMinePlacementHelper(crossCut, dustLevel);
                }
            }
            //Spawn NS straights
            for (int i = 0; i < entries; i++)
            {
                for (int j = -1; j < crosscuts; j++)
                {
                    Object toLoadNS = null;
                    foreach (GameObject go in tilePrefabs)
                    {
                        if (go.name.Contains("NS") && go.name.Contains(straightType))
                        {
                            toLoadNS = go;
                        }
                    }
                    GameObject entry = PrefabUtility.InstantiatePrefab(toLoadNS) as GameObject;
                    entry.transform.parent = parent.transform;
                    entry.name = "Section_" + _sectionName + "_Entry_" + (i + 1) + "_Crosscut_" + (j + 1) + "-" + (j + 2);
                    entry.transform.localPosition = new Vector3((i * 24), 0, ((j) * 24) + 12);
                    AddCreateMinePlacementHelper(entry, dustLevel);
                }
            }
        }
        else if(_spawningSectionNormals.x > 0)
        {

        }
        else
        {

        }
    }


    void AddCreateMinePlacementHelper (GameObject entry, int dustLevel) { 
        if (dustLevel == 0) {
            entry.AddComponent<CreateMinePlacementHelper> ().DustLevel = DustingHelper.None;
        } else if (dustLevel == 1) {
            entry.AddComponent<CreateMinePlacementHelper> ().DustLevel = DustingHelper.Medium;
        } else if (dustLevel == 2) {
            entry.AddComponent<CreateMinePlacementHelper> ().DustLevel = DustingHelper.Full;
        }
    }
}