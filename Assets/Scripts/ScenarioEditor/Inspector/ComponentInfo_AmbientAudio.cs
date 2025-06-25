using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_AmbientAudio : ModularComponentInfo, ISaveableComponent
{
    public string componentName = "Ambient Audio";
    public AudioSource m_component;
    public Inspector.ExposureLevel volumeExposureLevel;
    public float volume = 1f;
    public float Pitch = 1f;
    public int ClipIndex = 0;
    public AmbientAudioCollection AudioCollection;
    public bool PlayOnStart = true;
    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
        {
            Debug.Log("Failed to load audio component info. Saved component is null for " + gameObject.name); return;
        }
        componentName = component.GetComponentName();
        float.TryParse(component.GetParamValueAsStringByName("Volume"), out volume);
        int.TryParse(component.GetParamValueAsStringByName("ClipIndex"), out ClipIndex);
        float.TryParse(component.GetParamValueAsStringByName("Pitch"), out Pitch);
        
        if (m_component) 
        { 
            m_component.volume = volume;
            m_component.clip = AudioCollection.AmbientAudios[ClipIndex].Clip;
            m_component.pitch = AudioCollection.AmbientAudios[ClipIndex].PitchRange.Clamp(Pitch);
            if(PlayOnStart)
                m_component.Play();
        }
    }

    public string[] SaveInfo()
    {
        return new string[] { "Volume|" + volume, "ClipIndex|" + ClipIndex, "Pitch|" + Pitch };
    }

    public string SaveName()
    {
        return componentName;
    }

    public void SetPitch(float pitch)
    {
        if(m_component == null)
        {
            return;
        }
        m_component.pitch = AudioCollection.AmbientAudios[ClipIndex].PitchRange.Clamp(pitch);
    }

    public void SetVolume(float volume)
    {
        if (m_component == null)
        {
            return;
        }
        m_component.volume = volume;
    }

    public void InitAudioClip()
    {
        if(m_component == null)
        {
            return;
        }
        m_component.clip = AudioCollection.AmbientAudios[ClipIndex].Clip;
    }

    public void PlaySource()
    {
        if(m_component == null)
        {
            return;
        }
        m_component.Play();
    }

    public void StopSource()
    {
        if (m_component == null)
        {
            return;
        }
        m_component.Stop();
    }
}