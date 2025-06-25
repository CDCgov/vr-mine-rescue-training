using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleHandler : MonoBehaviour
{
    public AudioClip ToggleSfxClip;
    public AudioSource ToggleSource;
    public List<AudioSource> AudioToToggle;
    public List<GameObject> ObjectsToToggle;
    public List<Light> LightsToToggle;
    public bool IsOn = true;

    private ToggleObject _BasePrefabToggleBehavior;
    // Start is called before the first frame update
    void Start()
    {
        _BasePrefabToggleBehavior = GetComponentInParent<ToggleObject>();

        if(_BasePrefabToggleBehavior != null)
        {
            _BasePrefabToggleBehavior.ToggleSfxClip = ToggleSfxClip;
            _BasePrefabToggleBehavior.ToggleSource = ToggleSource;
            _BasePrefabToggleBehavior.AudioToToggle = new List<AudioSource>(AudioToToggle);
            _BasePrefabToggleBehavior.ObjectsToToggle = new List<GameObject>(ObjectsToToggle);
            _BasePrefabToggleBehavior.LightsToToggle = new List<Light>(LightsToToggle);
            _BasePrefabToggleBehavior.IsOn = IsOn;
        }
    }

    public void PopulateBehavior(ToggleObject obj)
    {
        if (obj != null)
        {
            obj.ToggleSfxClip = ToggleSfxClip;
            obj.ToggleSource = ToggleSource;
            obj.AudioToToggle = new List<AudioSource>(AudioToToggle);
            obj.ObjectsToToggle = new List<GameObject>(ObjectsToToggle);
            obj.LightsToToggle = new List<Light>(LightsToToggle);
            obj.IsOn = IsOn;
        }
    }
}
