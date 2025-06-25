using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestKBMap : MonoBehaviour
{
    public InputBindingManager InputBindingManager;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        if (InputBindingManager == null)
            InputBindingManager = InputBindingManager.GetDefault();

        InputBindingManager.RegisterAction("SpawnTestSphere", "Testing", SpawnSphere);
        InputBindingManager.RegisterAction("SpawnTestCube", "Testing", SpawnCube);
        InputBindingManager.RegisterAction("SpawnTestCapsule", "Testing", SpawnCapsule);
        InputBindingManager.RegisterAction("InputBindingSceneLoadTest", "Testing", TestSceneLoad);
    
    }

    private void OnDisable() 
    {
        InputBindingManager.UnregisterAction("SpawnTestSphere");
        InputBindingManager.UnregisterAction("SpawnTestCube");
        InputBindingManager.UnregisterAction("SpawnTestCapsule");
        InputBindingManager.UnregisterAction("InputBindingSceneLoadTest");
    }

    void SpawnSphere()
    {
        GameObject.CreatePrimitive(PrimitiveType.Sphere);
    }

    void SpawnCube()
    {
        GameObject.CreatePrimitive(PrimitiveType.Cube);
    }

    void SpawnCapsule()
    {
        GameObject.CreatePrimitive(PrimitiveType.Capsule);
    }

    void TestSceneLoad()
    {
        SceneManager.LoadScene("MainScene");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
