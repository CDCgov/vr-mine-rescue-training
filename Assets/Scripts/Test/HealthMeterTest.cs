using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthMeterTest : MonoBehaviour
{
    public int Health = 3;
    public GameObject[] HealthOrbs;

    private int _startingHealth;
    // Start is called before the first frame update
    void Start()
    {
        _startingHealth = Health;
        foreach(GameObject obj in HealthOrbs)
        {
            obj.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //foreach(GameObject orb in HealthOrbs)
        //{
        //    orb.transform.Rotate(transform.up, Time.deltaTime*(2* (1/Health)));
        //}
        //Debug.Log(Time.timeScale);
        transform.Rotate(0, 2, 0, Space.Self);
    }

    public void SubtractHealth()
    {
        Health--;
        for(int i = 0; i<_startingHealth; i++)
        {
            bool activate = i < Health;
            HealthOrbs[i].SetActive(activate);
        }
    }
}
