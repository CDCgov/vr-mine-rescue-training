using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEffectCollection", menuName = "VRMine/SoundEffectCollection", order = 500)]
public class SoundEffectCollection : ScriptableObject
{
    public AudioClip[] SoundEffects;

    public AudioClip GetRandomSound()
    {
        if (SoundEffects == null || SoundEffects.Length <= 0)
            return null;

        int index = Random.Range(0, SoundEffects.Length);
        return SoundEffects[index];
    }

    public void PlaybackRandom(AudioSource source)
    {
        var sound = GetRandomSound();
        if (sound == null)
            return;

        source.PlayOneShot(sound);
    }

    public void PlaybackRandom(Vector3 pos)
    {
        PlaybackRandom(pos, 1.0f, 1.0f, 1.0f);
    }

    public void PlaybackRandomNonSpatial()
    {
        PlaybackRandom(Vector3.zero, 1.0f, 1.0f, 0.0f);
    }

    public void PlaybackRandomWithPitchVariation(Vector3 pos, float volume, float pitchMin, float pitchMax)
    {
        PlaybackRandom(pos, volume, Random.Range(pitchMin, pitchMax), 1.0f);
    }

    public void PlaybackRandom(Vector3 pos, float volume, float pitch, float spatialBlend)
    {
        var sound = GetRandomSound();
        if (sound == null)
            return;

        GameObject go = new GameObject("SoundEffectPlayback");
        go.transform.position = pos;

        var source = go.AddComponent<AudioSource>();
        source.volume = volume;
        source.pitch = pitch;
        source.clip = sound;
        source.spatialBlend = spatialBlend;
        source.Play();

        Destroy(go, sound.length);
    }
}
