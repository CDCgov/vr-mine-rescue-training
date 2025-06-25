using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scenario Editor/LoadableBasePrefabData")]
public class LoadableBasePrefabData : ScriptableObject
{
    public string PrefabID;
    public string DisplayName;
    public bool AllowDNI;
    public bool IgnitionSource;

    public GameObject EditorBaseObject;
    public GameObject SceneBaseObject;
}
