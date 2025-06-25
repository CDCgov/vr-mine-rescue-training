using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineLayerManager : MonoBehaviour
{
    public static MineLayerManager Instance;

    [SerializeField]
    private GameObject doorPrefab;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetDoorPrefab()
    {
        return doorPrefab;
    }
}
