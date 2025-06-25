using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearManagersOnLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DestroyAllManagers();
    }
    public void DestroyAllManagers()
    {
        GameObject[] managers = GameObject.FindGameObjectsWithTag("Manager");
        foreach (GameObject obj in managers)
        {
            if (obj != null)
                Destroy(obj);
        }
    }
}
