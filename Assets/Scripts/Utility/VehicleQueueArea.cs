using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

[RequireComponent(typeof(BoxCollider))]
public class VehicleQueueArea : MonoBehaviour
{
    public BoxCollider QueueArea;
    

    [SerializeField]
    private Queue<GameObject> _vehicleStack;
    [SerializeField]
    private bool _zoneIsOccupied = false;
    [SerializeField]
    private GameObject _gameObjectInZone;
    [SerializeField]
    private int _QueueCount = 0;

    private LayerMask m_LayerMask;
    private Vector3 Center;
    private Vector3 Extents;
    // Start is called before the first frame update
    void Start()
    {
        _vehicleStack = new Queue<GameObject>();
        if(QueueArea == null)
        {
            gameObject.GetComponent<BoxCollider>();
        }
        m_LayerMask = LayerMask.GetMask("Vehicle");
        Center = QueueArea.center;
        Extents = QueueArea.size / 2;
        QueueArea.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckForVehicles();
        //if (_zoneIsOccupied)
        //{

        //}
        //else
        //{
        //    _gameObjectInZone = null;
        //}
    }

    private void OnTriggerStay(Collider other)
    {
        //CarAIControl ai = other.transform.root.GetComponent<CarAIControl>();
        //if(ai != null)
        //{
        //    _zoneIsOccupied = true;
        //    if (!_vehicleStack.Contains(ai.gameObject))
        //    {
        //        _vehicleStack.Enqueue(ai.gameObject);
        //    }
        //    if(_gameObjectInZone == null)
        //    {

        //    }
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        //CarAIControl ai = other.transform.root.GetComponent<CarAIControl>();
        //if (ai != null)
        //{
        //    if(ai.gameObject == _gameObjectInZone)
        //    {
        //        return;
        //    }
        //    else if (_gameObjectInZone == null)
        //    {
        //        _gameObjectInZone = ai.gameObject;
        //        _zoneIsOccupied = true;
        //    }
        //    else
        //    {
        //        if (!_vehicleStack.Contains(ai.gameObject))
        //        {
        //            if (_zoneIsOccupied)
        //            {
        //                _vehicleStack.Enqueue(ai.gameObject);
        //                _QueueCount++;
        //                Debug.Log(ai.name + " entered the zone");
        //                ai.GetComponent<CarController>().Move(0, 0, 1, 1);
        //                ai.enabled = false;
        //            }
        //        }
        //    }
        //}        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.gameObject == _gameObjectInZone)
        {
            if (_vehicleStack.Count > 0)
            {
                GameObject go = _vehicleStack.Dequeue();
                _QueueCount--;
                //go.GetComponent<CarAIControl>().enabled = true;
                _gameObjectInZone = go;
                _zoneIsOccupied = true;
            }
            else
            {
                _zoneIsOccupied = false;
                _gameObjectInZone = null;
            }
        }
        //if (_vehicleStack.Contains(other.transform.root.gameObject))
        //{
        //    GameObject go = _vehicleStack.Dequeue();
        //    Debug.Log(go.name + " left the zone");
        //    if(_vehicleStack.Count > 0)
        //    {
        //        _vehicleStack.Peek().GetComponent<CarAIControl>().enabled = true;
        //    }
        //}
    }

    private void CheckForVehicles()
    {
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position + Center, Extents, gameObject.transform.rotation, m_LayerMask);
        int i = 0;
        bool foundOccupyingVehicle = false;
        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            //Output all of the collider names
            //Debug.Log("Hit : " + hitColliders[i].name + i);
            //Increase the number of Colliders in the array
            
            if(_gameObjectInZone == null)
            {
                _gameObjectInZone = hitColliders[i].transform.root.gameObject;
                //_gameObjectInZone.transform.root.GetComponent<CarAIControl>().enabled = true;
                foundOccupyingVehicle = true;
            }
            else
            {
                if(_gameObjectInZone != hitColliders[i].transform.root.gameObject)
                {
                    if (!_vehicleStack.Contains(hitColliders[i].transform.root.gameObject))
                    {
                        _vehicleStack.Enqueue(hitColliders[i].transform.root.gameObject);
                        //hitColliders[i].transform.root.GetComponent<CarAIControl>().enabled = false;
                        //hitColliders[i].transform.root.GetComponent<CarController>().Move(0, 0, 1, 1);
                        //Debug.Log(_vehicleStack.Count);
                    }
                }
                else
                {
                    foundOccupyingVehicle = true;
                }
            }
            if (!_vehicleStack.Contains(hitColliders[i].transform.root.gameObject))
            {
                _vehicleStack.Enqueue(hitColliders[i].transform.root.gameObject);
            }
            i++;
        }
        if (!foundOccupyingVehicle)
        {
            if (_vehicleStack.Count > 0)
            {
                _gameObjectInZone = _vehicleStack.Dequeue();
                if (_gameObjectInZone != null)
                {
                    //_gameObjectInZone.transform.root.GetComponent<CarAIControl>().enabled = true;
                }
            }
            else
            {
                _gameObjectInZone = null;
            }
        }        
    }

    public void ClearArea()
    {
        _vehicleStack.Clear();
        _gameObjectInZone = null;
    }

    
}
