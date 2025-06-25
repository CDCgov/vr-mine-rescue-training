using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BH20RaceControl : MonoBehaviour
{
    public GameObject CinemachineCam;
    public GameObject FixedVCam;

    public GameObject Player1Vehicle;
    public GameObject Player2Vehicle;


    private Camera _player1Cam;
    private Camera _player2Cam;
    
    private GameObject _player1Marker;
    private GameObject _player2Marker;

    public GameObject FirePrefab;


    // Start is called before the first frame update
    void Start()
    {
        _player1Cam = Player1Vehicle.GetComponentInChildren<Camera>();
        _player2Cam = Player2Vehicle.GetComponentInChildren<Camera>();

        _player1Marker = Player1Vehicle.transform.Find("MarkerPlane").gameObject;
        _player2Marker = Player2Vehicle.transform.Find("MarkerPlane").gameObject;

        _player1Cam.gameObject.SetActive(false);
        _player2Cam.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("BH20Race");
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Player1Vehicle.GetComponent<BH20RaceData>().ResetPosition();
        }

        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Player2Vehicle.GetComponent<BH20RaceData>().ResetPosition();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            bool bThirdPerson = CinemachineCam.activeSelf;

            CinemachineCam.SetActive(!bThirdPerson);
            _player1Cam.gameObject.SetActive(bThirdPerson);
            _player2Cam.gameObject.SetActive(bThirdPerson);

            _player1Marker.SetActive(!bThirdPerson);
            _player2Marker.SetActive(!bThirdPerson);

        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            FixedVCam.SetActive(!FixedVCam.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            StopVehicle(Player1Vehicle);
            StopVehicle(Player2Vehicle);

            CreateFire(Player1Vehicle.transform.position);
            CreateFire(Player2Vehicle.transform.position);

        }
    }

    private void StopVehicle(GameObject vehicle)
    {
        vehicle.GetComponent<BH20GamepadControl>().enabled = false;
        vehicle.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private void CreateFire(Vector3 pos)
    {
        var obj = Instantiate<GameObject>(FirePrefab);
        obj.transform.position = pos;
    }
}
