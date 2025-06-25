using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRLODManualSwitch : MonoBehaviour
{
    public GameObject[] LODs;

    private Transform _camera;
    private Transform _parent;
    private int _priorLOD = -1;
    // Start is called before the first frame update
    void Start()
    {
        _parent = transform.root;
        _camera = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(_parent.position, _camera.position);
        if(dist < 5)
        {
            if (_priorLOD != 0)
            {
                SetActiveLOD(0);
                _priorLOD = 0;
            }
        }
        else if(dist >= 5 && dist < 100)
        {
            if (_priorLOD != 1)
            {
                SetActiveLOD(1);
                _priorLOD = 1;
            }
        }
        else if(dist >= 100 && dist <= 1000)
        {
            if (_priorLOD != 2)
            {
                SetActiveLOD(2);
                _priorLOD = 2;
            }
        }
        else
        {
            SetActiveLOD(-1);
        }
    }

    void SetActiveLOD(int index)
    {
        for(int i = 0; i < LODs.Length; i++)
        {
            if(i == index)
            {
                LODs[i].SetActive(true);
            }
            else
            {
                LODs[i].SetActive(false);
            }
        }
    }
}
