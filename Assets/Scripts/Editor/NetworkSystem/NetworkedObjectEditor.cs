using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NetworkedObject))]
public class NetworkedObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NetworkedObject netObj = target as NetworkedObject;
        if (netObj == null)
            return;

        if (GUILayout.Button("Regenerate ID"))
        {
            netObj.UniqueIDString = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(netObj);
        }

        DrawDefaultInspector();
        //GUILayout.Label()
        var hasAuthority = netObj.HasAuthority ? "Yes" : "No";

        EditorGUILayout.LabelField($"Has Authority: {hasAuthority}");
        EditorGUILayout.LabelField($"Object ID: {netObj.uniqueID.ToString()}");
        EditorGUILayout.LabelField($"Owner ID: {netObj.OwnerClientID}");
        EditorGUILayout.LabelField($"LastSync: {netObj.LastSyncTick}");

    }
}