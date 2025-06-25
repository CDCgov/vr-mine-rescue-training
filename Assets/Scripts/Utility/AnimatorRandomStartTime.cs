using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class AnimatorRandomStartTime : MonoBehaviour
{
    public Animator AnimatorToStart;
    // Start is called before the first frame update
    void Start()
    {
        if(AnimatorToStart == null)
        {
            AnimatorToStart = gameObject.GetComponent<Animator>();
            if(AnimatorToStart == null)
            {
                Debug.LogError("No animator attached to randomly start");
                return;
            }
        }

        //AnimatorToStart.StopPlayback();
        AnimatorToStart.Play("Idle", 0, Random.Range(0, 1f));
        //AnimatorToStart.playbackTime = Random.Range(0, 1f);
        //AnimatorToStart.StartPlayback();
    }
}
