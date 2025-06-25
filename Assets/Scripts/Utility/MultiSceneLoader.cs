using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiSceneLoader : MonoBehaviour
{
    public float DelayTime = 2.0f;
    public string[] ScenesToLoad;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (ScenesToLoad == null || ScenesToLoad.Length <= 0)
            yield return null;

        yield return new WaitForSeconds(DelayTime);

        foreach (string sceneName in ScenesToLoad)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }

}
