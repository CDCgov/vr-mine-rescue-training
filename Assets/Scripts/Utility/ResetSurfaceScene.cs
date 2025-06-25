using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetSurfaceScene : MonoBehaviour
{
    public Transform MainTruckSpawner;
    public Transform SecondaryTruckSpawner;
    public GameObject PrimaryTruckGO;
    public GameObject SecondaryTruckGO;

    Scene _activeScene;
    // Start is called before the first frame update
    void Start()
    {
        //_activeScene = SceneManager.GetActiveScene();
    }

    private void OnTriggerEnter(Collider other)
    {
        //SceneManager.LoadScene(_activeScene.name);
        GameObject truckOne = GameObject.Instantiate(PrimaryTruckGO, MainTruckSpawner.position, MainTruckSpawner.rotation);
        GameObject truckTwo = GameObject.Instantiate(SecondaryTruckGO, SecondaryTruckSpawner.position, SecondaryTruckSpawner.rotation);
        truckOne.name = "Primary";
        truckTwo.name = "Secondary";
        

        Destroy(PrimaryTruckGO);
        Destroy(SecondaryTruckGO);

        PrimaryTruckGO = truckOne;
        SecondaryTruckGO = truckTwo;
    }
}
