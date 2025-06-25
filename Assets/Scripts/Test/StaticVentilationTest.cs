using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticVentilationTest : MonoBehaviour
{
    public VentilationManager VentilationManager;
    private MineNetwork _mineNetwork;

    // Start is called before the first frame update
    void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        _mineNetwork = GameObject.FindObjectOfType<MineNetwork>();
        if (_mineNetwork == null)
            return;

        StartCoroutine(PrintVent());
    }

    IEnumerator PrintVent()
    {
        while (true)
        {
            MineAtmosphere mineAtmosphere;
            
            //if (_mineNetwork.GetMineAtmosphere(transform.position, out mineAtmosphere))
            if (VentilationManager.GetMineAtmosphere(transform.position, out mineAtmosphere))
            {
                Debug.Log(mineAtmosphere.ToString());
            }
            else
            {
                Debug.Log($"Failed to get mine atmosphere at {transform.position.ToString()}");
            }

            yield return new WaitForSeconds(1);
        }
    }
}

