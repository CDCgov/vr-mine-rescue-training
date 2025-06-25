using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAlt : MonoBehaviour
{
    public AntilatencyManager AntilatencyManager;
    public string TagName;


    // Start is called before the first frame update
    void Start()
    {
        if (AntilatencyManager == null)    
            AntilatencyManager = AntilatencyManager.GetDefault();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var tracker = AntilatencyManager.GetTrackerByTag(TagName);

        transform.position = tracker.Pose.position;
        transform.rotation = tracker.Pose.rotation;
    }
}
