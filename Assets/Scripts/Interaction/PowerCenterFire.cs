using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerCenterFire : MonoBehaviour
{
    public GameObject[] SparkEmitters;
    public GameObject[] FireEmitters;
    public AudioSource BoomAudioSource;

    private bool _notExploded = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return) && _notExploded)
        {
            foreach (GameObject sE in SparkEmitters)
            {
                sE.SetActive(false);
            }
            foreach (GameObject fE in FireEmitters)
            {
                fE.SetActive(true);
            }
            BoomAudioSource.Play();
            _notExploded = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            foreach(GameObject sE in SparkEmitters)
            {
                sE.SetActive(false);
            }
            foreach(GameObject fE in FireEmitters)
            {
                fE.SetActive(true);
            }
            BoomAudioSource.Play();
            _notExploded = false;
        }
    }
}
