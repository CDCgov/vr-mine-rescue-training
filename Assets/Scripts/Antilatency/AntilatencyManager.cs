using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TrackerNode
{
    public Pose Pose;
    public string Path;
    public string Tag;


    public bool IsActive
    {
        get
        {
            return false;
        }
    }

    public float TrackingStability
    {
        get
        {
            return 0;
        }
    }
}

public class AntilatencyManager : MonoBehaviour
{

    public const string GameObjectName = "AntilatencyManager";

    public static AntilatencyManager GetDefault()
    {
        var obj = GameObject.Find(GameObjectName);
        if (obj == null)
        {
            obj = new GameObject(GameObjectName);

            obj.tag = "Manager";
        }

        var manager = obj.GetComponent<AntilatencyManager>();
        if (manager == null)
            manager = obj.AddComponent<AntilatencyManager>();

        return manager;
    }



    public List<TrackerNode> Trackers;
    public System.Action NetworkChanged;

    public TrackerNode GetTrackerByTag(string tagName)
    {


        return null;
    }

}