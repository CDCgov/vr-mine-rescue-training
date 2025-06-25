using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BAHDOL;

[CustomEditor(typeof(NPCController))]
public class NPCControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NPCController npc = (NPCController)target;
        NPC_Animator anim = npc.GetComponent<NPC_Animator>();
        ComponentInfo_NPC comp = npc.GetComponent<ComponentInfo_NPC>();

        if (Application.isPlaying)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Walking"))
            {
                anim.TransitionToAnimation(BAHDOL.AnimationState.Walking);
            }

            if (GUILayout.Button("Seated"))
            {
                anim.TransitionToAnimation(BAHDOL.AnimationState.Seated);
            }

            if (GUILayout.Button("Unconscious"))
            {
                anim.TransitionToAnimation(BAHDOL.AnimationState.Unconscious);
            }

            if (GUILayout.Button("Kill"))
            {
                npc.Kill();
            }

            if (GUILayout.Button("Revive"))
            {
                npc.Revive();
            }

            if (GUILayout.Button("BG4 On"))
            {
                npc.SetEquipment(MinerEquipmentFlags.BG4);
            }

            if (GUILayout.Button("BG4 Off"))
            {
                npc.SetEquipment(MinerEquipmentFlags.SafetyGlasses);
            }

            GUILayout.EndVertical();
        }

        DrawDefaultInspector();
    }
}
