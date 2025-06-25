using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;

public class LogManagerEditor : MonoBehaviour {

    [MenuItem("CONTEXT/LogManager/Remove Player Log Handles")]
    public static void RemovePlayerLogHandles()
    {
        PlayerLogHandle[] playerLogs = GameObject.FindObjectsOfType<PlayerLogHandle>();
        foreach(PlayerLogHandle playLog in playerLogs)
        {
            DestroyImmediate(playLog, false);
        }
    }

    [MenuItem("CONTEXT/LogManager/Remove MobileEquipment Log Handles")]
    public static void RemoveMobileEquipmentLogHandles()
    {
        MobileEquipmentLogHandle[] mobLogs = GameObject.FindObjectsOfType<MobileEquipmentLogHandle>();
        foreach (MobileEquipmentLogHandle mobLog in mobLogs)
        {
            DestroyImmediate(mobLog, false);
        }
    }

    [MenuItem("CONTEXT/LogManager/Remove Proximity Log Handles")]
    public static void RemoveProximityLogHandles()
    {
        ProximityDataLogHandle[] proxLogs = GameObject.FindObjectsOfType<ProximityDataLogHandle>();
        foreach (ProximityDataLogHandle proxLog in proxLogs)
        {
            DestroyImmediate(proxLog, false);
        }
    }

    
}
/// <summary>
/// Special editor window for identifying logged game objects and then the option of removing said logs
/// </summary>
public class LogHandleWindow : EditorWindow
{
    private static LogHandle[] _LogHandles;
    private static List<LogHandle> _alphaSort;
    private static List<LogHandle> _filterSort;
    private bool _CloseButton;
    private Vector2 _ScrollPos;
    private List<LogHandle> _SelectedLogs;
    [NonSerialized]
    private List<GameObject> _SelectedGOs;
    private bool[] _CheckboxValues;
    private static List<Type> typeList;
    private static string[] typeListStrings;
    private int typeSelectedIndex;
    
    void Awake()
    {
        _SelectedLogs = new List<LogHandle>();
        _SelectedGOs = new List<GameObject>();
        _CheckboxValues = new bool[_LogHandles.Length];
    }

    [MenuItem("Logs/Identify Logged Game Objects")]
    public static void IdentifyLoggedGameObjects()
    {
        //This is the item that actually finds the log handles, the GUI window allows the user to remove logged objects
        _LogHandles = FindObjectsOfType<LogHandle>();

        _alphaSort = new List<LogHandle>();
        typeList = new List<Type>();
        foreach (LogHandle log in _LogHandles)
        {
            bool inserted = false;
            if (!typeList.Contains(log.GetType()))
            {
                typeList.Add(log.GetType());
            }
            for (int i = 0; i < _alphaSort.Count; i++)
            {
                int compareVal = log.CompareHandles(_alphaSort[i]);
                if (compareVal < 0)
                {
                    _alphaSort.Insert(i, log);
                    inserted = true;
                    break;
                }
            }
            if (!inserted)
            {
                _alphaSort.Add(log);
            }

            _filterSort = new List<LogHandle>();
            for(int i = 0; i < _alphaSort.Count; i++)
            {
                _filterSort.Add(_alphaSort[i]);
            }
        }
        typeListStrings = new string[typeList.Count];
        for(int i = 0; i < typeListStrings.Length; i++)
        {
            typeListStrings[i] = typeList[i].ToString();
        }
        LogHandleWindow window = (LogHandleWindow)GetWindow(typeof(LogHandleWindow), true, "Log Handles in Scene");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }

    void OnGUI()
    {
        
        if (_LogHandles != null)
        {
            

            EditorGUILayout.BeginVertical();
            EditorGUIUtility.labelWidth = 800;
            //EditorGUIUtility.
            _ScrollPos = EditorGUILayout.BeginScrollView(_ScrollPos,true, true, GUILayout.Width(400), GUILayout.Height(400));
            
            for (int i = 0; i < _filterSort.Count; i++)
            {
                LogHandle lh = _filterSort[i];
                string lhType = lh.ToString().Replace("LogHandle", "");
                _CheckboxValues[i] = EditorGUILayout.ToggleLeft(lhType, _CheckboxValues[i], GUILayout.Width(800),GUILayout.ExpandWidth(true));
                if (_CheckboxValues[i])
                {
                    _SelectedLogs.Add(_LogHandles[i]);
                    _SelectedGOs.Add(_LogHandles[i].gameObject);
                    
                }
                else
                {
                    _SelectedLogs.Remove(_LogHandles[i]);
                    _SelectedGOs.Remove(_LogHandles[i].gameObject);
                }
            }
            EditorGUILayout.EndScrollView();
            //EditorGUILayout.EndVertical();
            
           
            
            EditorGUILayout.LabelField("Log Type Filter");
            typeSelectedIndex = EditorGUILayout.Popup(typeSelectedIndex, typeListStrings);
            if (GUILayout.Button("Filter"))
            {
                _filterSort.Clear();
                for(int i=0; i < _alphaSort.Count; i++)
                {
                    if(_alphaSort[i].GetType() == typeList[typeSelectedIndex])
                    {
                        _filterSort.Add(_alphaSort[i]);
                    }
                }
            }
            if(GUILayout.Button("Remove Filter"))
            {
                _filterSort.Clear();
                for(int i = 0; i < _alphaSort.Count; i++)
                {
                    _filterSort.Add(_alphaSort[i]);
                }
            }         
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Log Handle Selection");
            if (GUILayout.Button("Select All"))
            {
                for (int i = 0; i < _CheckboxValues.Length; i++)
                {
                    _CheckboxValues[i] = true;
                    if (!_SelectedLogs.Contains(_LogHandles[i]))
                    {
                        _SelectedLogs.Add(_LogHandles[i]);
                        _SelectedGOs.Add(_LogHandles[i].gameObject);
                    }
                }
            }
            if (GUILayout.Button("Deselect All"))
            {
                for (int i = 0; i < _CheckboxValues.Length; i++)
                {
                    _CheckboxValues[i] = false;
                    _SelectedLogs.Clear();
                    _SelectedGOs.Clear();
                }
            }
            //if(GUILayout.Button("Filter Selected Types"))
            //{                
            //    foreach (LogHandle log in _SelectedLogs)
            //    {
            //        List<LogHandle> filterCopy = new List<LogHandle>();
            //        for(int i = 0; i < _filterSort.Count; i++)
            //        {
            //            filterCopy.Add(_filterSort[i]);
            //        }
            //        for(int i = 0; i < _filterSort.Count; i++)
            //        {
            //            if(_filterSort[i].GetType() == log.GetType())
            //            {
            //                filterCopy.Remove(_filterSort[i]);
            //            }
            //        }
            //        _filterSort.Clear();
            //        for(int i = 0; i < filterCopy.Count; i++)
            //        {
            //            _filterSort.Add(filterCopy[i]);
            //        }
            //    }
            //    _CheckboxValues = new bool[_filterSort.Count];
            //}


            //if(GUILayout.Button("Reset Filters"))
            //{
            //    _filterSort.Clear();
            //    for (int i = 0; i < _alphaSort.Count; i++)
            //    {
            //        _filterSort.Add(_alphaSort[i]);
            //    }
            //    for (int i = 0; i < _CheckboxValues.Length; i++)
            //    {
            //        _CheckboxValues[i] = false;
            //        _SelectedLogs.Clear();
            //    }
            //}
            if (GUILayout.Button("Remove Selected Log Handles"))
            {
                foreach(LogHandle lh in _SelectedLogs)
                {
                    DestroyImmediate(lh);
                }
                this.Close();
            }
            if (GUILayout.Button("Cancel"))
            {                
                this.Close();
            }            
        }
        else
        {
            this.Close();
            Debug.Log("Log Handles was null");
        }
    }
}