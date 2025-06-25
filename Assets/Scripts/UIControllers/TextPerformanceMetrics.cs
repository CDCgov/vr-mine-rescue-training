using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextPerformanceMetrics : MonoBehaviour
{
    public ProfilerManager ProfilerManager;

    private TMP_Text _text;
    private StringBuilder _sb = new StringBuilder();
    
    void Start()
    {
        if (ProfilerManager == null)
            ProfilerManager = ProfilerManager.GetDefault(gameObject);

        _text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        InvokeRepeating("UpdateText", 1.0f, 1.0f);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    void UpdateText()
    {
        _sb.Clear();
        ProfilerManager.GetLastData(_sb);

        _text.text = _sb.ToString();
    }
}
