using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoalScore : MonoBehaviour
{    
    private float _TriggerTime = 0;
    //private CoalInTruck _cit;
    private List<CoalInTruck> _trucksInZone;

    private void Start()
    {
        _trucksInZone = new List<CoalInTruck>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Container")
        {
            CoalInTruck _cit;
            _TriggerTime = Time.time;
            _cit = other.GetComponent<CoalInTruck>();
            _trucksInZone.Add(_cit);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Container")
        {
            CoalInTruck _cit;
            _TriggerTime = 0;
            _cit = other.GetComponent<CoalInTruck>();
            _trucksInZone.Remove(_cit);
        }
    }

    private void Update()
    {
        if(_trucksInZone.Count > 0)
        {
            if(Time.time - _TriggerTime > 0.25f)
            {
                foreach (CoalInTruck _cit in _trucksInZone)
                {                    
                    _cit.RemoveNextCoalObject();                    
                }
                _TriggerTime = Time.time;
            }
        }
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if(other.tag == "Container")
    //    {
    //        //Debug.Log("Vehicle in here!");
    //        if(Time.time - _TriggerTime % 1 == 0 || Time.time - _TriggerTime > 2)
    //        {
    //            _cit = other.GetComponent<CoalInTruck>();
    //            _cit.RemoveNextCoalObject();
    //            _TriggerTime = Time.time;
    //        }
    //    }
    //}
}
