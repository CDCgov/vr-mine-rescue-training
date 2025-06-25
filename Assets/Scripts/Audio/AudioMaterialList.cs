using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioMaterialList", menuName = "VRMine/AudioMaterialList", order = 1)]
public class AudioMaterialList : ScriptableObject
{
    public List<AudioMaterial> AudioMaterials;
    public AudioMaterial FallbackMaterial;

    public bool TryGetMaterialByName(string name, out AudioMaterial audioMaterial)
    {
        try
        {
            foreach (AudioMaterial audioMat in AudioMaterials)
            {
                if (audioMat.name == name)
                {
                    audioMaterial = audioMat;
                    return true;
                }
            }
        }
        catch (System.Exception e)
        {
            //Debug.LogError(e.ToString());
        }
        audioMaterial = null;
        return false;
    }    

    public AudioClip GetClipByMaterialNameAndIndex(string matName, AudioMaterialType collisionType, int index)
    {
        AudioClip ac = null;
        AudioMaterial aMat;

        if(TryGetMaterialByName(matName, out aMat))
        {
            ac = aMat.GetCollisionAudio(collisionType, index).CollisionClip;
        }

        return ac;
    }

    public bool ContainsKey(string key)
    {
        foreach(AudioMaterial am in AudioMaterials)
        {
            if(am.AudioMaterialName == key)
            {
                return true;
            }
        }
        return false;
    }
}
