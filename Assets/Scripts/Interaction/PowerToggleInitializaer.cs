using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerToggleInitializaer : MonoBehaviour
{
    public List<ComponentInfo_Light> Lights;
    public List<AudioSource> Audios;
    // Start is called before the first frame update
    void Start()
    {
        PowerToggleBehavior powerToggleBehavior = GetComponentInParent<PowerToggleBehavior>();
        if (powerToggleBehavior != null)
        {
            powerToggleBehavior.Lights = Lights;
            powerToggleBehavior.AudioSources = Audios;
        }
    }    
}
