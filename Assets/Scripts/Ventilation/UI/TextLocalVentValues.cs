using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextLocalVentValues : MonoBehaviour
{
    public VentilationManager VentilationManager;

    private StringBuilder _sb;
    private TextMeshProUGUI _textMesh;
    // Start is called before the first frame update
    void Start()
    {
        _sb = new StringBuilder();
        _textMesh = GetComponent<TextMeshProUGUI>();

        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        StartCoroutine(UpdateTextDisplay());
    }

    IEnumerator UpdateTextDisplay()
    {
        while (true)
        {
            _sb.Clear();

            UpdateVentText(_sb);

            if (_textMesh != null)
                _textMesh.text = _sb.ToString();

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdateVentText(StringBuilder sb)
    {
        var graph = VentilationManager.GetVentilationGraph();
        if (graph == null)
        {
            sb.AppendLine("No Ventilation Graph");
            return;
        }

        var airway = graph.FindClosestAirway(transform.position);
        if (airway != null)
            airway.AppendText(sb);
        else
            sb.AppendLine("No Airway Found");

        var junction = graph.FindClosestJunction(transform.position);
        if (junction != null)
            junction.AppendText(sb);
        else
            sb.AppendLine("No Junction Found");

    }
}
