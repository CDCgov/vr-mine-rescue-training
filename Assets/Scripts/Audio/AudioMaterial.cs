using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioMaterialType
{
    Hard,
    Soft,
    Wet,
    Metallic,
    Dirt,
    Wooden
}
[CreateAssetMenu(fileName = "AudioMaterial", menuName = "VRMine/AudioMaterial", order = 1)]
public class AudioMaterial : ScriptableObject
{
    public string AudioMaterialName;
    [Tooltip("What other materials colliding against this material will generate")]
    public AudioMaterialType MaterialType = AudioMaterialType.Dirt;
    [Tooltip("What this collision sound this material will make when colliding with something that doesn't have an audio material")]
    public AudioMaterialType FallbackCollisionType = AudioMaterialType.Dirt;
    public AnimationCurve VolumeCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float ImpactNormalBias = 0.1f;
    public float SizeBias = 1;
    public float MassVolumeBias = 0.15f;
    public float MassPitchBias = 0;
    public float VelocityPitchBias = 0.15f;
    public float PitchRandomness = 0.1f;
    public Range RelativeVelocityThreshold;
    public Bounds ReferenceSizeBounds;
    public float ReferenceImpactVelocity = 4.43f;//This is the velocity of an object falling 1m at standard gravity (9.8 m/s/s) assuming no initial velocity.
    public float ReferenceMass = 1;
    public int Priority = 0;
    
    public List<AudioSoundSet> AudioSounds;
    private Dictionary<AudioMaterialType, AudioSoundSet> _audioClipDictionary;

    //public AudioClip GetRandomClip()
    //{
    //    AudioClip output;
    //    int n = Random.Range(0, AudioMaterialClips.Count - 1);
    //    output = AudioMaterialClips[n];
    //    return output;
    //}

    private void OnEnable()
    {
        //Debug.Log("Configuring audio material!");
        if(AudioSounds.Count >= 0)
        {
            _audioClipDictionary = new Dictionary<AudioMaterialType, AudioSoundSet>();
            foreach (AudioSoundSet audioSound in AudioSounds)
            {
                if (_audioClipDictionary.ContainsKey(audioSound.Key))
                {
                    Debug.LogError("Audio Material " + name + " has duplicate audio set for Material Type \"" + audioSound.Key+ "\". It will not be used during runtime.");
                    continue;
                }
                _audioClipDictionary.Add(audioSound.Key, audioSound);
            }
        }
    }

    //public AudioClip GetCollisionAudio(AudioMaterialType key)
    //{
    //    AudioSoundSet audioSoundSet = GetSoundSet(key);
    //    if(_audioClipDictionary.TryGetValue(key, out audioSoundSet))
    //    {
    //        return audioSoundSet.GetSound();
    //    }
    //    return _audioClipDictionary[FallbackCollisionType].GetSound();
    //}

    public CollisionAudioClip GetCollisionAudio(AudioMaterialType key, int index = -1)
    {
        AudioSoundSet audioSoundSet = GetSoundSet(key);

        if (audioSoundSet == null)
            return null;

        return audioSoundSet.GetSound(index);
        //if (_audioClipDictionary.TryGetValue(key, out audioSoundSet))
        //{
        //    return audioSoundSet.GetSound(index);
        //}
        //return _audioClipDictionary[FallbackCollisionType].GetSound();
    }

    /// <summary>
    /// Gets the volume of the impact audio based on the velocity and normal of the collision.
    /// </summary>
    public float GetImpactVolume(Vector3 relativeVel, Vector3 norm, float mass = 0)
    {
        float impactAmt = norm == Vector3.zero ? 1 : Mathf.Abs(Vector3.Dot(norm.normalized, relativeVel.normalized));
        float impactVel = (impactAmt + (1 - impactAmt) * (1 - ImpactNormalBias)) * relativeVel.magnitude;
        float massVal = ReferenceMass;
        if(mass > 0)
        {
            massVal = mass;
        }
        impactVel = (impactVel - MassVolumeBias) + (massVal / ReferenceMass) * MassVolumeBias;
        if (impactVel < RelativeVelocityThreshold.Min)
            return VolumeCurve.Evaluate(RelativeVelocityThreshold.Min);

        return VolumeCurve.Evaluate(RelativeVelocityThreshold.Normalize(impactVel));
    }
    /// <summary>
    /// Gets the volume of impact audio based on a scalar velocity value.
    /// </summary>
    /// <param name="velocityMagnitude"></param>
    /// <returns></returns>
    public float GetImpactVolume(float velocityMagnitude)
    {
        float impactVel = RelativeVelocityThreshold.Clamp(velocityMagnitude);

        return VolumeCurve.Evaluate(impactVel);
    }

    /// <summary>
    /// Gets the amount to multiply the pitch by based on the given scale and the ScaleMod property. NOTE: 1.73205 value is the length of a 1,1,1 vector i.e. sqrt(3)
    /// </summary>
    public float GetScaleModPitch(Vector3 scale, float priorPitch = 1)
    {
        float val = (priorPitch - SizeBias) + (1.7320508075688772f / scale.magnitude) * SizeBias;

        
        return val;
    }

    /// <summary>
    /// Gets the amount to multiply the volume by based on the given scale and the ScaleMod property.
    /// </summary>
    public float GetScaleModVolume(Vector3 scale)
    {
        float mod = Mathf.Clamp((1 - SizeBias) + (scale.magnitude / 1.7320508075688772f) * SizeBias, -3, 3);        
        return mod;
    }

    /// <summary>
    ///  Gets the amount to multiply the pitch by based on the given mass and the MassMod property.
    /// </summary>
    public float GetMassModPitch(float currentMass, float defaultMass, float priorPitch = 1)
    {
        return (priorPitch - MassVolumeBias) + (currentMass/defaultMass) * MassVolumeBias;
    }

    public float GetUnifiedModifiedPitch(Bounds objBounds, Vector3 velocity, float mass = -1, float defaultPitch = 1)
    {
        float pitch = defaultPitch;
        if(mass <= 0)
        {
            mass = ReferenceMass;
        }


        //original, found that it would behave mostly as expected when originally tested, but failed upon further examination
        //pitch = (defaultPitch + ScaleMod - VelocityPitchMod) - ((objBounds.size.magnitude / AbsoluteSizeBounds.size.magnitude) * ScaleMod) + ((velocity.magnitude / DefaultImpactVelocity) * VelocityPitchMod);

        pitch = pitch + SizeBias * ((ReferenceSizeBounds.size.magnitude / objBounds.size.magnitude) * pitch - pitch) + MassPitchBias * ((ReferenceMass / mass) * pitch - pitch) + VelocityPitchBias * ((velocity.magnitude / ReferenceImpactVelocity) * pitch - pitch);
        
        Debug.Log($"Modifying Pitch - default pitch: {defaultPitch}, size ratio: {(ReferenceSizeBounds.size.magnitude / objBounds.size.magnitude)}, mass ratio: {(ReferenceMass / mass)}, velocity ratio: {(velocity.magnitude / ReferenceImpactVelocity)}, (velocity magnitude of) {velocity.magnitude}");
        return pitch;
    }

    public float GetRandomPitch()
    {
        return Random.Range(-PitchRandomness, PitchRandomness);
    }

    public AudioSoundSet GetSoundSet(AudioMaterialType key)
    {
        AudioSoundSet audioSoundSet;
        _audioClipDictionary.TryGetValue(key, out audioSoundSet);

        if (audioSoundSet == null)
            _audioClipDictionary.TryGetValue(FallbackCollisionType, out audioSoundSet);

        if (audioSoundSet == null && AudioSounds != null && AudioSounds.Count > 0)
            audioSoundSet = AudioSounds[0];

        return audioSoundSet;
    }
}

[System.Serializable]
public class AudioSoundSet
{
    public AudioMaterialType Key;
    public List<CollisionAudioClip> Sounds = new List<CollisionAudioClip>();

    public CollisionAudioClip GetSound(int index = -1)
    {
        if (Sounds.Count == 0)
        {
            return null;
        }

        if (index < 0 || index >= Sounds.Count)
        {
            return Sounds[Random.Range(0, Sounds.Count)];
        }
        else
        {
            return Sounds[index];
        }
    }
}