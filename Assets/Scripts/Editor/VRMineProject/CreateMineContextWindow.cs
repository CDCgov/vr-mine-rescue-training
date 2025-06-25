using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#pragma warning disable 414


public class CreateMineContextWindow : EditorWindow 
{
    public static GameObject Root;

    public Vector3 Position;
    public Vector3 Direction;

    string SectionName = "Branch Section";
    int Entries = 2;
    int Crosscuts = 2;
    int BranchDirection = 1;
    int _tileSelection = 0;
    int _rockDustSelection = 0;

    bool perpendicular = false;

    static string[] _tilesets;
    private static string[] _tilesetsTruncated;
    private static string[] _rockDustLevel;

    private void Awake()
    {
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
        CreateMineContextWindow._tilesets = paths;

        _tilesetsTruncated = new string[CreateMineContextWindow._tilesets.Length];
        for (int i = 0; i < _tilesetsTruncated.Length; i++)
        {
            string stopAt = "\\";
            int lastIndex = CreateMineContextWindow._tilesets[i].Length - 1;
            while (!CreateMineContextWindow._tilesets[i].Substring(lastIndex, 1).Equals(stopAt))
            {
                lastIndex--;
            }
            int location = lastIndex;
            _tilesetsTruncated[i] = CreateMineContextWindow._tilesets[i].Substring(location + 1);
        }
    }

    public void SetRootObject(string root)
    {
        Root = GameObject.Find(root);
        Debug.Log("Root set ");
        if (Root == null)
        {
            Debug.Log("...but it's null");

        }
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
        CreateMineContextWindow._tilesets = paths;

        _tilesetsTruncated = new string[CreateMineContextWindow._tilesets.Length];
        for (int i = 0; i < _tilesetsTruncated.Length; i++)
        {
            string stopAt = "\\";
            int lastIndex = CreateMineContextWindow._tilesets[i].Length - 1;
            while (!CreateMineContextWindow._tilesets[i].Substring(lastIndex, 1).Equals(stopAt))
            {
                lastIndex--;
            }
            int location = lastIndex;
            _tilesetsTruncated[i] = CreateMineContextWindow._tilesets[i].Substring(location + 1);
        }
    }
    void OnGUI()
    {
        //TestSectionInfo info = Root.transform.parent.GetComponent<TestSectionInfo>();

        GUILayout.Label("Enter Coarse Paramaters for the mine section");
        /*
        if (GUILayout.Button("Popup Options", GUILayout.Width(200)))
        {
            PopupWindow.Show(buttonRect, new MineWindowContent());
        }
        if(Event.current.type == EventType.Repaint)
        {
            buttonRect = GUILayoutUtility.GetLastRect();
        }
        */
        SectionName = EditorGUILayout.TextField("Section Name", SectionName);
        EditorGUILayout.Space();
        Entries = EditorGUILayout.IntField("Entries", Entries);
        EditorGUILayout.Space();
        Crosscuts = EditorGUILayout.IntField("Crosscuts", Crosscuts);
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Branch Direction: ");
        BranchDirection = EditorGUILayout.Popup(BranchDirection, new string[] { "Left", "Right" });
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tileset:");
        _tileSelection = EditorGUILayout.Popup(_tileSelection, _tilesetsTruncated);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Rockdust:");        
        _rockDustLevel = new string[4];
        _rockDustLevel[0] = "None";
        _rockDustLevel[1] = "Medium";
        _rockDustLevel[2] = "Full";
        _rockDustLevel[3] = "Auto";
        _rockDustSelection = EditorGUILayout.Popup(_rockDustSelection, _rockDustLevel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Create Section", GUILayout.Width(200)))
        {
            GameObject newSection = new GameObject(SectionName);
            newSection.transform.parent = Root.transform;

            if(Entries <= 1)
            {
                Debug.LogError("Make a branch with more than 1 entry");
                return;
            }

            if (_rockDustSelection == 3 && (Crosscuts <= 2 || Entries <= 2))
            {
                Debug.LogError("Cannot build a mine if crosscuts and entries are less than 2");
                return;
            }
            string[] files = Directory.GetFiles(CreateMineContextWindow._tilesets[_tileSelection]);

            Debug.Log("Assets/Tilesets/" + _tilesetsTruncated[_tileSelection]);
            DirectoryInfo dirInfo = new DirectoryInfo("Assets/Tilesets/" + _tilesetsTruncated[_tileSelection]);
            FileInfo[] fileInfArr = dirInfo.GetFiles("*.prefab");
            //Add tile references to memory for later instantiation
            List<GameObject> tilePrefabs = new List<GameObject>();
            foreach (FileInfo fileInf in fileInfArr)
            {
                string fullPath = fileInf.FullName.Replace(@"\", "/");
                string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
                tilePrefabs.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject);
                Debug.Log(assetPath);
            }
            List<Vector3> positions = new List<Vector3>();
            List<DustConnections> dCons = new List<DustConnections>();

            if (Direction.x > 0)
            {
                switch (BranchDirection)
                {
                    case 0:
                        //make a grid to the left
                        //TSouth
                        for(int j = 0; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3(j * 24 + 12 + Position.x, 0, Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TSouths(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //T Norths
                        for (int j = 1; j < Crosscuts; j++)
                        {                           
                            positions.Add(new Vector3(j * 24 + 12 + Position.x, 0, (Entries-1) * 24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }                            
                        }
                        Spawn_TNorths(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //Spawn T wests
                        for(int i = 1; i < Entries - 1; i++)
                        {
                            positions.Add(new Vector3(Position.x + 12, 0, i * 24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.Full);
                                    break;
                            }
                        }
                        Spawn_TWests(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn Corner
                        positions.Add(new Vector3(0 * 24 + 12 + Position.x, 0, (Entries - 1) * 24 + Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full);
                                break;
                        }
                        
                        Spawn_NW(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn 4 Ways
                        for (int i = 1; i < Entries-1; i++)
                        {
                            for (int j = 1; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(12 + j * 24 + Position.x, 0, i * 24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_FourWays(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EW straights
                        positions.Add(new Vector3(Position.x, 0, Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full);
                                break;
                        }
                        for (int i = 0; i < Entries; i++)
                        {
                            for (int j = 1; j < Crosscuts + 1; j++)
                            {
                                positions.Add(new Vector3(j * 24 + Position.x, 0, i * 24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Medium);
                                        }
                                        else if(j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.Medium_None);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_EW_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn NS Straights
                        for (int i = 0; i < Entries-1; i++)
                        {
                            for (int j = 0; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(j * 24 + Position.x+12, 0, i * 24 + Position.z+12));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium_Medium);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_NS_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EndCaps
                        for(int i = 0; i < Entries; i++)
                        {
                            positions.Add(new Vector3((Crosscuts) * 24 + Position.x, 0, i * 24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.None);
                                    break;
                            }
                        }
                        Spawn_EndEast(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        break;
                    case 1:
                        //TNorths
                        for (int j = 0; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3(j * 24 + 12 + Position.x, 0, Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TNorths(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //T Souths
                        for (int j = 1; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3(j * 24 + 12 + Position.x, 0, (Entries - 1) * -24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TSouths(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //Spawn T wests
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            positions.Add(new Vector3(Position.x + 12, 0, i * -24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.Full);
                                    break;
                            }
                        }
                        Spawn_TWests(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn Corner
                        positions.Add(new Vector3(0 * 24 + 12 + Position.x, 0, (Entries - 1) * -24 + Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full);
                                break;
                        }

                        Spawn_SW(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn 4 Ways
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            for (int j = 1; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(12 + j * 24 + Position.x, 0, i * -24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_FourWays(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EW straights
                        positions.Add(new Vector3(Position.x, 0, Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None_None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium_Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                        }
                        for (int i = 0; i < Entries; i++)
                        {
                            for (int j = 1; j < Crosscuts + 1; j++)
                            {
                                positions.Add(new Vector3(j * 24 + Position.x, 0, i * -24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Medium);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.Medium_None);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_EW_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn NS Straights
                        for (int i = 0; i < Entries - 1; i++)
                        {
                            for (int j = 0; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(j * 24 + Position.x + 12, 0, i * -24 + Position.z - 12));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium_Medium);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_NS_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EndCaps
                        for (int i = 0; i < Entries; i++)
                        {
                            positions.Add(new Vector3((Crosscuts) * 24 + Position.x, 0, i * -24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.None);
                                    break;
                            }
                        }
                        Spawn_EndEast(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        break;
                    default:
                        break;
                }
            }
            else if(Direction.x < 0)
            {
                switch (BranchDirection)
                {
                    case 1:
                        //make a grid to the left
                        //TSouth
                        for (int j = 0; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3(j * -24 -+ 12 + Position.x, 0, Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TSouths(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //T Norths
                        for (int j = 1; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3(j * -24 - 12 + Position.x, 0, (Entries - 1) * 24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TNorths(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //Spawn T wests
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            positions.Add(new Vector3(Position.x - 12, 0, i * 24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.Full);
                                    break;
                            }
                        }
                        Spawn_TEasts(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn Corner
                        positions.Add(new Vector3(0 * -24 - 12 + Position.x, 0, (Entries - 1) * 24 + Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full);
                                break;
                        }

                        Spawn_NE(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn 4 Ways
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            for (int j = 1; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(-12 + j * -24 + Position.x, 0, i * 24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_FourWays(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EW straights
                        positions.Add(new Vector3(Position.x, 0, Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None_None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium_Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                        }
                        for (int i = 0; i < Entries; i++)
                        {
                            for (int j = 1; j < Crosscuts + 1; j++)
                            {
                                positions.Add(new Vector3(j * -24 + Position.x, 0, i * 24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium_Full);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.None_Medium);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_EW_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn NS Straights
                        for (int i = 0; i < Entries - 1; i++)
                        {
                            for (int j = 0; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(j * -24 + Position.x - 12, 0, i * 24 + Position.z + 12));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium_Medium);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_NS_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EndCaps
                        for (int i = 0; i < Entries; i++)
                        {
                            positions.Add(new Vector3((Crosscuts) * -24 + Position.x, 0, i * 24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.None);
                                    break;
                            }
                        }
                        Spawn_EndWest(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        break;
                    case 0:
                        //make a grid to the left
                        //TNorth
                        for (int j = 0; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3(j * -24 - +12 + Position.x, 0, Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TNorths(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //T Souths
                        for (int j = 1; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3(j * -24 - 12 + Position.x, 0, (Entries - 1) * -24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TSouths(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //Spawn T wests
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            positions.Add(new Vector3(Position.x - 12, 0, i * -24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.Full);
                                    break;
                            }
                        }
                        Spawn_TEasts(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn Corner
                        positions.Add(new Vector3(0 * -24 - 12 + Position.x, 0, (Entries - 1) * -24 + Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full);
                                break;
                        }

                        Spawn_SE(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn 4 Ways
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            for (int j = 1; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(-12 + j * -24 + Position.x, 0, i * -24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_FourWays(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EW straights
                        positions.Add(new Vector3(Position.x, 0, Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None_None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium_Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                        }
                        for (int i = 0; i < Entries; i++)
                        {
                            for (int j = 1; j < Crosscuts + 1; j++)
                            {
                                positions.Add(new Vector3(j * -24 + Position.x, 0, i * -24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium_Full);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.None_Medium);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_EW_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn NS Straights
                        for (int i = 0; i < Entries - 1; i++)
                        {
                            for (int j = 0; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(j * -24 + Position.x - 12, 0, i * -24 + Position.z - 12));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium_Medium);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_NS_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EndCaps
                        for (int i = 0; i < Entries; i++)
                        {
                            positions.Add(new Vector3((Crosscuts) * -24 + Position.x, 0, i * -24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.None);
                                    break;
                            }
                        }
                        Spawn_EndWest(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        break;
                    default:
                        break;
                }
            }
            else if(Direction.z < 0)
            {
                switch (BranchDirection)
                {
                    case 0:
                        //TWests
                        for (int j = 0; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3(Position.x, 0, j * -24 + -12 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TWests(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //T Norths
                        for (int j = 1; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3((Entries - 1) * 24 + Position.x, 0, j * -24 + -12 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TEasts(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //Spawn T Norths
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            positions.Add(new Vector3(i * 24 + Position.x, 0,Position.z - 12));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.Full);
                                    break;
                            }
                        }
                        Spawn_TNorths(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn Corner
                        positions.Add(new Vector3((Entries - 1) * 24 + Position.x, 0, 0 * -24 + -12 + Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full);
                                break;
                        }

                        Spawn_NE(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn 4 Ways
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            for (int j = 1; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(i * 24 + Position.x, 0, -12 + j * -24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_FourWays(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn NS straights
                        positions.Add(new Vector3(Position.x, 0, Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None_None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium_Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                        }
                        for (int i = 0; i < Entries; i++)
                        {
                            for (int j = 1; j < Crosscuts + 1; j++)
                            {
                                positions.Add(new Vector3(i * 24 + Position.x, 0, j * -24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium_Full);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.None_Medium);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_NS_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EW Straights
                        for (int i = 0; i < Entries - 1; i++)
                        {
                            for (int j = 0; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(i * 24 + 12 + Position.x, 0, j * -24 + -12 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium_Medium);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_EW_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EndCaps
                        for (int i = 0; i < Entries; i++)
                        {
                            positions.Add(new Vector3(i * 24 + Position.x, 0, (Crosscuts) * -24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.None);
                                    break;
                            }
                        }
                        Spawn_EndSouth(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();                        
                        break;
                    case 1:
                        //TWests
                        for (int j = 0; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3(Position.x, 0, j * -24 + -12 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TEasts(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //T Wests
                        for (int j = 1; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3((Entries - 1) * -24 + Position.x, 0, j * -24 + -12 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TWests(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //Spawn T Norths
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            positions.Add(new Vector3(i * -24 + Position.x, 0, Position.z - 12));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.Full);
                                    break;
                            }
                        }
                        Spawn_TNorths(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn Corner
                        positions.Add(new Vector3((Entries - 1) * -24 + Position.x, 0, 0 * -24 + -12 + Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full);
                                break;
                        }

                        Spawn_NW(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn 4 Ways
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            for (int j = 1; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(i * -24 + Position.x, 0, -12 + j * -24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_FourWays(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn NS straights
                        positions.Add(new Vector3(Position.x, 0, Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None_None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium_Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                        }
                        for (int i = 0; i < Entries; i++)
                        {
                            for (int j = 1; j < Crosscuts + 1; j++)
                            {
                                positions.Add(new Vector3(i * -24 + Position.x, 0, j * -24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium_Full);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.None_Medium);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_NS_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EW Straights
                        for (int i = 0; i < Entries - 1; i++)
                        {
                            for (int j = 0; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(i * -24 + -12 + Position.x, 0, j * -24 + -12 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium_Medium);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_EW_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EndCaps
                        for (int i = 0; i < Entries; i++)
                        {
                            positions.Add(new Vector3(i * -24 + Position.x, 0, (Crosscuts) * -24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.None);
                                    break;
                            }
                        }
                        Spawn_EndSouth(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (BranchDirection)
                {
                    case 1:
                        //TWests
                        for (int j = 0; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3(Position.x, 0, j * 24 + 12 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TWests(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //T Easts
                        for (int j = 1; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3((Entries - 1) * 24 + Position.x, 0, j * 24 + 12 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TEasts(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //Spawn T Souths
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            positions.Add(new Vector3(i * 24 + Position.x, 0, Position.z + 12));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.Full);
                                    break;
                            }
                        }
                        Spawn_TSouths(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn Corner
                        positions.Add(new Vector3((Entries - 1) * 24 + Position.x, 0, 0 * 24 + 12 + Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full);
                                break;
                        }

                        Spawn_SE(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn 4 Ways
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            for (int j = 1; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(i * 24 + Position.x, 0, 12 + j * 24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_FourWays(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn NS straights
                        positions.Add(new Vector3(Position.x, 0, Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None_None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium_Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                        }
                        for (int i = 0; i < Entries; i++)
                        {
                            for (int j = 1; j < Crosscuts + 1; j++)
                            {
                                positions.Add(new Vector3(i * 24 + Position.x, 0, j * 24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Medium);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.Medium_None);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_NS_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EW Straights
                        for (int i = 0; i < Entries - 1; i++)
                        {
                            for (int j = 0; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(i * 24 + 12 + Position.x, 0, j * 24 + 12 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium_Medium);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_EW_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EndCaps
                        for (int i = 0; i < Entries; i++)
                        {
                            positions.Add(new Vector3(i * 24 + Position.x, 0, (Crosscuts) * 24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.None);
                                    break;
                            }
                        }
                        Spawn_EndNorth(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        break;
                    case 0:
                        //TWests
                        for (int j = 0; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3(Position.x, 0, j * 24 + 12 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TEasts(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //T Wests
                        for (int j = 1; j < Crosscuts; j++)
                        {
                            positions.Add(new Vector3((Entries - 1) * -24 + Position.x, 0, j * 24 + 12 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    if (j < Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Full);
                                    }
                                    else if (j == Crosscuts - 2)
                                    {
                                        dCons.Add(DustConnections.Medium);
                                    }
                                    else
                                    {
                                        dCons.Add(DustConnections.None);
                                    }
                                    break;
                            }
                        }
                        Spawn_TWests(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        //Spawn T Norths
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            positions.Add(new Vector3(i * -24 + Position.x, 0, Position.z + 12));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.Full);
                                    break;
                            }
                        }
                        Spawn_TSouths(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn Corner
                        positions.Add(new Vector3((Entries - 1) * -24 + Position.x, 0, 0 * 24 + 12 + Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full);
                                break;
                        }

                        Spawn_SW(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn 4 Ways
                        for (int i = 1; i < Entries - 1; i++)
                        {
                            for (int j = 1; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(i * -24 + Position.x, 0, 12 + j * 24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_FourWays(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn NS straights
                        positions.Add(new Vector3(Position.x, 0, Position.z));
                        switch (_rockDustSelection)
                        {
                            case 0:
                                dCons.Add(DustConnections.None_None);
                                break;
                            case 1:
                                dCons.Add(DustConnections.Medium_Medium);
                                break;
                            case 2:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                            default:
                                dCons.Add(DustConnections.Full_Full);
                                break;
                        }
                        for (int i = 0; i < Entries; i++)
                        {
                            for (int j = 1; j < Crosscuts + 1; j++)
                            {
                                positions.Add(new Vector3(i * -24 + Position.x, 0, j * 24 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Medium);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.Medium_None);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_NS_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EW Straights
                        for (int i = 0; i < Entries - 1; i++)
                        {
                            for (int j = 0; j < Crosscuts; j++)
                            {
                                positions.Add(new Vector3(i * -24 + -12 + Position.x, 0, j * 24 + 12 + Position.z));
                                switch (_rockDustSelection)
                                {
                                    case 0:
                                        dCons.Add(DustConnections.None_None);
                                        break;
                                    case 1:
                                        dCons.Add(DustConnections.Medium_Medium);
                                        break;
                                    case 2:
                                        dCons.Add(DustConnections.Full_Full);
                                        break;
                                    default:
                                        if (j < Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Full_Full);
                                        }
                                        else if (j == Crosscuts - 2)
                                        {
                                            dCons.Add(DustConnections.Medium_Medium);
                                        }
                                        else if (j == Crosscuts - 1)
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        else
                                        {
                                            dCons.Add(DustConnections.None_None);
                                        }
                                        break;
                                }
                            }
                        }
                        Spawn_EW_Straights(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();

                        //Spawn EndCaps
                        for (int i = 0; i < Entries; i++)
                        {
                            positions.Add(new Vector3(i * -24 + Position.x, 0, (Crosscuts) * 24 + Position.z));
                            switch (_rockDustSelection)
                            {
                                case 0:
                                    dCons.Add(DustConnections.None);
                                    break;
                                case 1:
                                    dCons.Add(DustConnections.Medium);
                                    break;
                                case 2:
                                    dCons.Add(DustConnections.Full);
                                    break;
                                default:
                                    dCons.Add(DustConnections.None);
                                    break;
                            }
                        }
                        Spawn_EndNorth(tilePrefabs, positions, dCons, newSection.transform);
                        positions.Clear();
                        dCons.Clear();
                        break;
                    default:
                        break;
                }
            }
            this.Close();
        }
        
    }

    string GetDustType(DustConnections dCon)
    {
        return dCon.ToString();
    }

    void Spawn_EW_Straights(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("EW") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }

    void Spawn_NS_Straights(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("NS") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }

    void Spawn_FourWays(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("4Way") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }

    void Spawn_TNorths(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for(int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("T_North") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }

    void Spawn_TEasts(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("T_East") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }

    void Spawn_TSouths(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("T_South") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }

    void Spawn_TWests(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("T_West") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }

    void Spawn_NW(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("NW") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }
    void Spawn_NE(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("NE") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }
    void Spawn_SW(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("SW") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }
    void Spawn_SE(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("SE") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }

    void Spawn_EndNorth(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("End_North") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }
    void Spawn_EndEast(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("End_East") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }
    void Spawn_EndSouth(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("End_South") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }
    void Spawn_EndWest(List<GameObject> tilePrefabs, List<Vector3> positions, List<DustConnections> dustLevel, Transform parentSection)
    {
        string type = "";
        for (int i = 0; i < positions.Count; i++)
        {
            type = GetDustType(dustLevel[i]);
            Object toLoad = null;
            foreach (GameObject go in tilePrefabs)
            {
                if (go.name.Contains("End_West") && go.name.Contains(type))
                {
                    toLoad = go;
                }
            }
            GameObject tile = PrefabUtility.InstantiatePrefab(toLoad) as GameObject;
            tile.transform.position = positions[i];
            tile.transform.parent = parentSection;
        }
    }
}