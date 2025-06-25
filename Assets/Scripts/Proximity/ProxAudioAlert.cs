using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxAudioAlert : MonoBehaviour
{

    [HideInInspector]
    public ProxAudioSet AudioSet;

    //private AudioClip _yellowSound;
    //private AudioClip _redSound;

    private AudioSource _audioYellow;
    private AudioSource _audioRed;

    ProxZone _proxZone = ProxZone.GreenZone;
    ProxZone _lastProxZone = ProxZone.GreenZone;

    public void SetProxZone(ProxZone zone)
    {
        //prox zone is set every frame we are in a field, possibly multiple times by multiple fields
        //record current zone as the highest zone we are in
        if ((int)zone > (int)_proxZone)
        {
            _proxZone = zone;
        }
    }

    private void LateUpdate()
    {

        if (_proxZone != _lastProxZone)
        {
            Debug.Log("Audio Prox Zone Changed " + _proxZone.ToString());
            switch (_proxZone)
            {
                case ProxZone.GreenZone:
                    _audioRed.Stop();
                    _audioYellow.Stop();
                    break;

                case ProxZone.YellowZone:
                    _audioRed.Stop();
                    _audioYellow.Play();
                    break;

                case ProxZone.RedZone:
                    _audioRed.Play();
                    _audioYellow.Stop();
                    break;
            }			
        }

        _lastProxZone = _proxZone;
        _proxZone = ProxZone.GreenZone;
    }

    // Use this for initialization
    void Start()
    {
        //_yellowSound = Resources.Load<AudioClip>("Sound/Prox/proxwarning_yellow");
        //_redSound = Resources.Load<AudioClip>("Sound/Prox/proxwarning_red");

        //_audioYellow = gameObject.AddComponent<AudioSource>();
        //_audioRed = gameObject.AddComponent<AudioSource>();

        if (AudioSet == null)
        {
            Debug.LogError("Prox audio alert missing audio set!");
            return;
        }

        SetupAudioSources();
    }

    public void SetupAudioSources()
    {
        _audioYellow = SetupAudioSource(AudioSet.YellowZoneAlert);
        _audioRed = SetupAudioSource(AudioSet.RedZoneAlert);
    }

    AudioSource SetupAudioSource(AudioClip clip)
    {
        GameObject obj = new GameObject("ProxAudioSource");
        obj.transform.SetParent(transform, false);

        var src = obj.AddComponent<AudioSource>();

        src.loop = true;
        src.clip = clip;
        src.minDistance = 5;
        src.maxDistance = 25;
        src.volume = 0.12f;
        src.spatialBlend = 1.0f;
        src.playOnAwake = false;
        src.Stop();

        return src;
    }
    
}
