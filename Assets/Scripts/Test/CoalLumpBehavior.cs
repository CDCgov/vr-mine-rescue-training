using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoalLumpBehavior : MonoBehaviour
{
    private float _age = 0;    
    private float _priorTime = 0;

    public bool Active = true;
    public float Lifespan = 5;
    public float Value = 1;


    private void Start()
    {
        _priorTime = Time.time;
    }

    private void Update()
    {
        if (Active)
        {
            _age += (Time.time - _priorTime);            
            _priorTime = Time.time;
            if(_age > Lifespan)
            {
                Destroy(gameObject);
            }
        }
    }

    public void RestartLifespan()
    {
        _age = 0;
        _priorTime = Time.time;
    }
    
}