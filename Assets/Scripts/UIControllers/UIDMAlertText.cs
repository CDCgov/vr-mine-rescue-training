using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class UIDMAlertText : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public SceneLoadManager SceneLoadManager;
    public VentilationManager VentilationManager;
    private TextMeshProUGUI _text;

    private System.Text.StringBuilder _sb = new System.Text.StringBuilder();

    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();

        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        StartCoroutine(UpdateTextRoutine());
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene arg0, Scene arg1)
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);
    }

    IEnumerator UpdateTextRoutine()
    {
        while (true)
        {
            _sb.Clear();

            if (VentilationManager.GetVentilationProvider() == VentilationProvider.MFIRE)
            {
                if (!VentilationManager.AutoAdvanceEnabled)
                {
                    _sb.AppendLine("Ventilation simulation paused");
                }
            }


            if (SceneLoadManager.InSimulationScene && 
                !SceneLoadManager.LoadInProgress &&
                !SceneLoadManager.InWaitingRoom && !NetworkManager.IsSessionRecording())
            {
                _sb.AppendLine("Session not recording");
            }

            _text.text = _sb.ToString();

            yield return new WaitForSecondsRealtime(1.0f);
        }
    }
}
