using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LeakyFeederCable))]
public class LeakyFeederNodeEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LeakyFeederCable cable = (LeakyFeederCable)target;
        
        //GameObject[] newSelection = new GameObject[1];
                
       
        if(!cable.bNodesSpawned)
        {
            if(GUILayout.Button("Add Hanging Cable Nodes"))
            {

                
                GameObject lfParent = new GameObject();

                lfParent.name = "LeakyFeederNodes";
                lfParent.transform.parent = cable.transform;
                lfParent.transform.localPosition = Vector3.zero;
                //initialNode.transform.parent = lfParent.transform;
                HangingCable hangingCable = cable.GetComponent<HangingCable>();
                
                if (cable != null)
                {
                    List<HangingGeometry.CableNode> nodes = hangingCable.CableNodes;
                    List<LeakyFeederNode> lfNodes = new List<LeakyFeederNode>();                

                    GameObject firstNodeObj = new GameObject();
                    firstNodeObj.transform.position = nodes[0].Position;
                    firstNodeObj.name = "Leaky Node 1";
                    firstNodeObj.transform.parent = lfParent.transform;
                    LeakyFeederNode firstNode = firstNodeObj.AddComponent<LeakyFeederNode>();
                    lfNodes.Add(firstNode);

                    for (int i = 1; i < nodes.Count; i++)
                    {                        
                        GameObject spawn = lfNodes[i-1].CreateNode(ref lfNodes, i-1, lfParent.transform);
                        spawn.transform.position = nodes[i].Position;                        
                    }
                    cable.bNodesSpawned = true;
                    cable.FirstNode = lfNodes[0];
                }
            }
        }
        
        if (GUILayout.Button("Add all radios"))
        {

        }
    }
}
