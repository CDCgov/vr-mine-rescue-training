using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProxAudioAlert))]
public class ProxAudioAlertEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ProxAudioAlert proxAlert = (ProxAudioAlert)target;

        if (EditorUtil.ObjectSelector<ProxAudioSet>("Audio Set", "l:ProxAudioSet", ref proxAlert.AudioSet,
            target, "Changed Audio Set"))
        {
            if (Application.isPlaying)
            {
                proxAlert.SetupAudioSources();
            }
        }		
            

        DrawDefaultInspector();
    }
}