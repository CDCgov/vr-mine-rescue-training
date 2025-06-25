using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Helper script to suppress the curtain spawning sfx when not in scenario editor or upon the scene being loaded
/// </summary>
public class CurtainSpawnAudioPlayer : MonoBehaviour
{
    public AudioSource SpawnAudioSource;
    public bool PerformSpawnAudio = true;
    public float Delay = 2;
    // Start is called before the first frame update
    void Start()
    {
        
        if (ScenarioSaveLoad.IsScenarioEditor)
        {
            return;
        }
        if (SpawnAudioSource != null && PerformSpawnAudio)
        {
            //Debug.Log($"Curtain audio playing at {Time.timeSinceLevelLoad}");
            SpawnAudioSource.Play();
        }
    }
}
