using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingNodeBtn : MonoBehaviour {

    private bool _showingDisplay = false;
    public TrackingNode nodeRef;
    public GameObject DisplayBox;
    public UnityEngine.UI.Text textBox;
    public UnityEngine.UI.Button nodeBtn;
    public void ToggleActorDisplay()
    {
        if (_showingDisplay)
        {
            DisplayBox.SetActive(false);
            _showingDisplay = false;
        }
        else
        {
            string actors = "";
            foreach(ActorHost host in nodeRef.ActorsClosestToThisNode)
            {
                actors += host.ActorName + "\n";
            }
            textBox.text = actors;            
            DisplayBox.SetActive(true);
            _showingDisplay = true;
        }
    }

    public void UpdateColor()
    {
        UnityEngine.UI.ColorBlock cb = nodeBtn.colors;
        if (nodeRef.ActorsClosestToThisNode.Count != 0)
        {
            cb.normalColor = Color.green;
            nodeBtn.colors = cb;
        }
        else
        {
            cb.normalColor = Color.blue;
            nodeBtn.colors = cb;
        }
    }
}
