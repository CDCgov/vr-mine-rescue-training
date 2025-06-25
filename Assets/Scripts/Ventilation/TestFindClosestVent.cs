using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class TestFindClosestVent : MonoBehaviour
{
    public VentilationManager VentilationManager;
    // Start is called before the first frame update
    void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        StartCoroutine(FindClosestVent());
    }

    public IEnumerator FindClosestVent()
    {
        StringBuilder sb = new StringBuilder();

        while (true)
        {
            var ventGraph = VentilationManager.GetVentilationGraph();
            sb.Clear();
            var junction = ventGraph.FindClosestJunctionBench(transform.position, sb);
            var airway = ventGraph.FindClosestAirwayBench(transform.position, sb);

            Debug.Log(sb.ToString());

            yield return new WaitForSeconds(2.0f);
        }
    }

}
