using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class CreateMineSectionContext : MonoBehaviour 
{

    //public static GameObject Selection;

    [MenuItem("CONTEXT/MineSegment/Create Branch Section")]
    static void OnContext(MenuCommand command)
    {
        //Debug.Log(command.context);
        //EditorWindow window = EditorWindow.CreateInstance<CreateMineContextWindow>();
        //window.Show();


        //CreateMineContextWindow window = EditorWindow.CreateInstance<CreateMineContextWindow>();		
        //window.Show();
        //Debug.Log(command.context.name);
        //window.SetRootObject(command.context.name);
        MineSegment ms = command.context as MineSegment;
        Debug.Log(ms.name);
        for(int i = 0; i < ms.SegmentConnections.Length; i++)
        {
            if (ms.SegmentConnections[i] != null)
            {
                Debug.Log(ms.SegmentConnections[i]);
            }
            else
            {
                Debug.Log("null! : " + i);
                Debug.Log(ms.SegmentGeometry.SegmentConnections[i].Centroid);
                Debug.Log(ms.transform.position + ms.SegmentGeometry.SegmentConnections[i].Centroid);

                CreateMineContextWindow window = EditorWindow.CreateInstance<CreateMineContextWindow>();
                Vector3 centroid = ms.SegmentGeometry.SegmentConnections[i].Centroid;
                window.Position = ms.transform.position + ms.SegmentGeometry.SegmentConnections[i].Centroid + ms.SegmentGeometry.SegmentConnections[i].Centroid;
                window.Direction = new Vector3(centroid.x / centroid.x, 0, centroid.z / centroid.z);
                //window.SetRootObject("Mains");
                window.Show();
            }
        }
    }

    [MenuItem("CONTEXT/MineSegment/Swap Tile")]
    static void OnSwap(MenuCommand command)
    {
        MineSegment ms = command.context as MineSegment;
        GameObject go = ms.gameObject;
        Debug.Log(go.name + ", " + go.transform.position);
        SwapMineTileWindow window = EditorWindow.CreateInstance<SwapMineTileWindow>();
        window.Show();
        window.CurrentSegment = ms;
        window.Tile = go;
    }
}