using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine.Events;

public static class ProjectStats
{
    [MenuItem("Project Stats/Object Interaction Event Log Details")]
    public static void ObjectInteractionEventLogDetails()
    {
        StringBuilder sb = new StringBuilder();

        var prefabGUIDs = AssetDatabase.FindAssets("t:prefab");
        //var prefabGUIDs = AssetDatabase.FindAssets("t:ObjectInteractionEventLogger");

        sb.AppendLine("| Name | ObjEV | EVType | DropEVType | Message | FOVReq | MinHoldTime " +
            "| MinTimeInFOV | RepeatDelay | MinDistanceFromPrevious | IgnoreMinDstanceAfterSeconds " +
            "| AllowRepeatsWhileHeld | LogDroppeEvent | CheckInBounds | BoundingBoxTag |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");

        foreach (var guid in prefabGUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null)
                continue;

            ObjectInteractionEventLogger eventLog;
            eventLog = go.GetComponent<ObjectInteractionEventLogger>();
            if (eventLog == null)
                continue;

            var allEL = go.GetComponents<ObjectInteractionEventLogger>();

            foreach (var el in allEL)
            {

                //sb.AppendLine($"Name: {go.name}"); ;
                sb.AppendLine($"|{go.name}|{el.ObjectEvent}|{el.EventType}|{el.DroppedEventType}|{el.EventMessage}|" +
                    $"{el.RequireInsideFOV}|{el.MinHoldTime:F2}|{el.MinTimeInFOV:F2}|{el.RepeatDelay:F2}|{el.MinDistanceFromPrevious:F2}|" +
                    $"{el.IgnoreMinDistanceAfterSeconds:F2}|{el.AllowRepeatsWhileHeld}|{el.LogDroppedEvent}|{el.CheckInBounds}|" +
                    $"{el.BoundingBoxTag}|");
            }
        }

        TextDisplayWindow.ShowWindow(sb.ToString());
    }

    [MenuItem("Project Stats/Custom XR Interactable Stats")]
    public static void CustomXRInteractableStats()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var interact in FindAllPrefabComponents<CustomXRInteractable>())
        {
            sb.AppendFormat("Interactable: {0}\n", interact.name);

            var fields = typeof(CustomXRInteractable).GetFields();
            foreach (var field in fields)
            {
                var val = field.GetValue(interact);

                if (val == null)
                    continue;

                sb.AppendFormat("{0,-35}: {1}\n", field.Name, val.ToString());
            }

            sb.Append("-------------------------\n\n");
            /*
            if (interact.onActivate != null)
            {
                foreach (var listener in GetAllListeners(interact.onActivate))
                {
                    sb.AppendFormat("onActivate: {0}\n", listener);
                }
            }

            if (interact.onDeactivate != null)
            {
                foreach (var listener in GetAllListeners(interact.onDeactivate))
                {
                    sb.AppendFormat("onDeactivate: {0}\n", listener);
                }
            }

            if (interact.onSelectEnter != null)
            {
                foreach (var listener in GetAllListeners(interact.onSelectEnter))
                {
                    sb.AppendFormat("onSelectEnter: {0}\n", listener);
                }
            }

            if (interact.onSelectExit != null)
            {
                foreach (var listener in GetAllListeners(interact.onSelectExit))
                {
                    sb.AppendFormat("onSelectExit: {0}\n", listener);
                }
            }*/
        }

        TextDisplayWindow.ShowWindow(sb.ToString());
    }

    public static IEnumerable<string> GetAllListeners(UnityEvent ev)
    {
        int listenerCount = ev.GetPersistentEventCount();

        for (int i = 0; i < listenerCount; i++)
        {
            var method = ev.GetPersistentMethodName(i);
            var target = ev.GetPersistentTarget(i);

            yield return $"{target.GetType().ToString()}::{method}";
        }
    }

    public static IEnumerable<T> FindAllPrefabComponents<T>()
    {

        var prefabGUIDs = AssetDatabase.FindAssets("t:prefab");

        foreach (var guid in prefabGUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null)
                continue;

            //T comp;
            //comp = go.GetComponent<T>();
            //if (comp == null)
            //    continue;

            var allComps = go.GetComponents<T>();

            foreach (var comp in allComps)
            {
                yield return comp;
            }
        }
    }
}
