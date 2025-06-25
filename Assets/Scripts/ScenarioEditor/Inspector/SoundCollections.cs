using System.Linq;
using UnityEngine;



[CreateAssetMenu(fileName = "SoundList", menuName = "VRMine/SoundList", order = 2)]
public class SoundCollections : ScriptableObject
{
    public SoundCollection[] collections;

    public SoundCollection GetCollectionByString(string name)
    {
        if(string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("NULL SOUND " + name + " REQUESTED. CHECK SAVE FILE. RETURNING DEFAULT SOUND COLLECTION"); // TODO add actual default collection
            return collections[0];
        }
        else
        {
            return collections.Where(sound => sound.name == name).First();
        }
    }
}
