using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BH20RaceData))]
public class BH20RaceCoalLoadIndicator : MonoBehaviour
{
    public GameObject CoalLoadObject;

    private BH20RaceData _raceData;
    private Vector3 _maxScale;

    // Start is called before the first frame update
    void Start()
    {
        _raceData = GetComponent<BH20RaceData>();
        _maxScale = CoalLoadObject.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (_raceData.CoalLoad < 0.05)
        {
            CoalLoadObject.SetActive(false);
        }
        else
        {
            CoalLoadObject.transform.localScale = _maxScale * _raceData.CoalLoad;
            CoalLoadObject.SetActive(true);
        }
    }
}
