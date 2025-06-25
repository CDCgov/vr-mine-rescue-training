using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInMiniatureTeamstopDemonstration : MonoBehaviour
{
    public GameObject[] TeamstopDemos;
    public float Delay = 3;
    public float InitialDelay = 20;

    private int _currentTeamstop = 0;
    private float _delayTimer = Mathf.Infinity;

    private void Start()
    {
        _delayTimer = Time.time + InitialDelay;

        for(int i = 1; i < TeamstopDemos.Length; i++)
        {
            TeamstopDemos[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > _delayTimer)
        {
            _currentTeamstop++;
            if(_currentTeamstop >= TeamstopDemos.Length)
            {
                _currentTeamstop = 0;
            }
            for (int i = 0; i < TeamstopDemos.Length; i++)
            {
                TeamstopDemos[i].SetActive(i == _currentTeamstop);                
            }
            _delayTimer = Time.time + Delay;
        }
    }
}
