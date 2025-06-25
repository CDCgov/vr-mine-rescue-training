using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelResetTimer : MonoBehaviour
{
    public Text[] TimerLabels;
    public GameObject[] Trucks;

    private float _TimeLeft = 120.0f;
    private bool _Start = false;
    private bool _Reset = false;
    private List<Vector3> _StartPositions;
    private List<Quaternion> _StartRotations;
    private List<Rigidbody> _TruckRigidbodies;
    private List<CoalInTruck> _TruckLoads;
    // Start is called before the first frame update
    void Start()
    {
        _TimeLeft = 120;
        foreach (Text lbl in TimerLabels)
        {
            lbl.text = _TimeLeft.ToString("0.00") + "s";
        }
        _Start = false;
        _StartPositions = new List<Vector3>();
        _StartRotations = new List<Quaternion>();
        _TruckRigidbodies = new List<Rigidbody>();
        _TruckLoads = new List<CoalInTruck>();
        foreach (GameObject truck in Trucks)
        {
            _StartPositions.Add(truck.transform.position);
            _StartRotations.Add(truck.transform.rotation);
            _TruckRigidbodies.Add(truck.GetComponent<Rigidbody>());
            _TruckLoads.Add(truck.GetComponentInChildren<CoalInTruck>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        _TimeLeft = _TimeLeft - Time.deltaTime;
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _Start = true;
            _TimeLeft = 120;
            _Reset = false;
            for (int i = 0; i < Trucks.Length; i++)
            {
                Trucks[i].transform.position = _StartPositions[i];
                Trucks[i].transform.rotation = _StartRotations[i];
                _TruckRigidbodies[i].velocity = Vector3.zero;
                _TruckLoads[i].RestartScore();
            }
            
        }
        if (_Start)
        {
            if (_TimeLeft < 0)
            {
                CoalInTruck winner = null;
                int score = 0;
                foreach (CoalInTruck load in _TruckLoads)
                {
                    if(load.GetScore() > score)
                    {
                        winner = load;
                        score = load.GetScore();
                    }
                }
                if(winner != null)
                {
                    winner.ScoreLabel.text = "WINNER";
                }
                _TimeLeft = 0;
                _Start = false;
                _Reset = true;
                
            }
            foreach (Text lbl in TimerLabels)
            {
                lbl.text = _TimeLeft.ToString("0.00") + "s";
            }            
        }
    }
}
