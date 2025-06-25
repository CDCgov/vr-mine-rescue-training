using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gas_Reading : MonoBehaviour {

    public GameObject GasMeter;
    public bool gasReading = false;

    private void Start()
    {
        GasMeter.SetActive(gasReading);
    }

    // Update is called once per frame
    void Update () {

        if (Input.GetButtonDown("GasReading"))
        {
            gasReading = !gasReading;
            GasMeter.SetActive(gasReading);
            
        }
        
    }
}
