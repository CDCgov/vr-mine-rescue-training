using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceMineDemoHotkeys : MonoBehaviour
{
    public GameObject[] CamObjs;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetCamActive(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SetCamActive(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SetCamActive(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            SetCamActive(3);
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            SetCamActive(4);
    }

    void SetCamActive(int index)
    {
        if (index < 0 || CamObjs == null || index >= CamObjs.Length)
            return;

        for (int i = 0; i < CamObjs.Length; i++)
        {
            CamObjs[i].SetActive(i == index);
        }
    }
}
