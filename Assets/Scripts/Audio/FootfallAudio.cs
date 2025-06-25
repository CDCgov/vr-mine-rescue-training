using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootfallAudio : MonoBehaviour
{
    public AudioClip[] FootfallClips;
    public AudioSource FootfallAudioSource;

    private bool _allowFootAudio = false;
    private float _cooldown = 0;
    

    // Update is called once per frame
    void Update()
    {
        if (Time.time < _cooldown)
        {
            return;
        }
        RaycastHit hit;
        int mask = 1 << LayerMask.NameToLayer("Floor");
        
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 1, mask))
        {
            if(Vector3.Distance(hit.point, transform.position) < 0.05f && _allowFootAudio && !FootfallAudioSource.isPlaying)
            {
                FootfallAudioSource.clip = FootfallClips[(int)Random.Range(0, FootfallClips.Length - 1)];
                FootfallAudioSource.Play();
                _cooldown = Time.time + 0.5f;
                _allowFootAudio = false;
            }
            else if(Vector3.Distance(hit.point, transform.position) > 0.1f)
            {
                _allowFootAudio = true;
            }
        }
    }
}
