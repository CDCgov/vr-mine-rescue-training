using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HMDShuttleSpawner : MonoBehaviour {

    public GameObject ShuttlePrefab;
    public float[] StopDistances;
    private GameObject _spawnedCar;
    ShuttleHMDDemoController _shuttleLogic;
    private int _index = 0;
    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_spawnedCar == null)
            {
                _spawnedCar = GameObject.Instantiate(ShuttlePrefab);
                _shuttleLogic = _spawnedCar.GetComponent<ShuttleHMDDemoController>();
                _shuttleLogic.StopDistance = StopDistances[_index];
                _shuttleLogic.StartMotion();
            }
            else
            {
                ResetTest();
            }
        }
    }

    private void ResetTest()
    {
        Destroy(_spawnedCar);
        _spawnedCar = null;
        _index++;
        if(_index >= StopDistances.Length)
        {
            _index = 0;
        }
    }
}
