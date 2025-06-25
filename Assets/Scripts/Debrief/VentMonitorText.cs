using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class VentMonitorText : MonoBehaviour
{
    public VentilationManager VentilationManager;
    public Transform SamplePoint;

    private StringBuilder _sb;


    private Vector3 _lastPosition = Vector3.zero;
    VentAirway _a1, _a2;
    float _d1, _d2;

    private TMP_Text _text;

    // Start is called before the first frame update
    void Start()
    {
        _sb = new StringBuilder();

        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        _text = GetComponent<TMP_Text>();

        if (SamplePoint == null || _text == null)
        {
            Debug.LogError($"No sample point and/or text for VentMonitorText {gameObject.name}");
            this.enabled = false;
        }

        InvokeRepeating(nameof(UpdateVentText), 0, 0.5f);
    }

    void UpdateVentText()
    {
        MineAtmosphere atm;
        var pos = SamplePoint.position;


        if (!VentilationManager.GetMineAtmosphere(pos, out atm))
            return;

        var graph = VentilationManager.GetVentilationGraph();
        if (graph == null)
            return;

        if (pos != _lastPosition || _a1 == null || _a2 == null)
        {
            _lastPosition = pos;
            graph.FindNearbyAirways(pos, out _a1, out _a2, out _d1, out _d2);
        }

        _sb.Clear();
        if (_a1 != null)
            _sb.AppendFormat("Airflow: {0:F0} CFM\n", _a1.MFAirway.FlowRate);

        _sb.AppendFormat("CO     : {0} PPM\n", atm.GetClampedCO_PPM(9999));
        _sb.AppendFormat("Methane: {0:F1}%\n", atm.Methane * 100.0f);
        _sb.AppendFormat("Oxygen : {0:F1}%", atm.Oxygen * 100.0f);


        _text.text = _sb.ToString();

    }

}
