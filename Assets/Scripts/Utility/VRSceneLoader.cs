using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSceneLoader : MonoBehaviour
{
    public SceneLoadManager SceneLoadManager;
    public NetworkManager NetworkManager;
    public float DelayTime = 2.0f;
    public string SceneName = "MineRescueScenario1";

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        yield return new WaitForSeconds(DelayTime);

        NetworkManager.SendLoadScene(SceneName, true);

        yield return null;
    }
}
