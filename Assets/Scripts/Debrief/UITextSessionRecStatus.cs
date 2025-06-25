using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class UITextSessionRecStatus : MonoBehaviour
{
    public NetworkManager NetworkManager;
    private TextMeshProUGUI _text;

    private bool _stopped = false;

    private WaitForEndOfFrame _waitForEndOfFrame;
    private WaitForSecondsRealtime _waitForSecondsRealtime;

    private Coroutine _coroutine = null;

    private void Awake()
    {
        _waitForEndOfFrame = new WaitForEndOfFrame();
        _waitForSecondsRealtime = new WaitForSecondsRealtime(0.2f);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        _text = GetComponent<TextMeshProUGUI>();

        //StartCoroutine(UpdateCoroutine());
    }

    private void OnEnable()
    {
        _coroutine = StartCoroutine(UpdateCoroutine());
    }

    private void OnDisable()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        _coroutine = null;
    }

    // Update is called once per frame
    IEnumerator UpdateCoroutine()
    {
        yield return _waitForEndOfFrame;
        while (true)
        {
            try
            {
                UpdateText();
            }
            catch (System.Exception) { }
            //yield return new WaitForSecondsRealtime(0.1f);
            yield return _waitForSecondsRealtime;
        }
    }

    void UpdateText()
    {
        var sessionLog = NetworkManager.GetActiveSessionRec();
        if (sessionLog == null && !_stopped)
        {
            _text.text = "Stopped";
            _stopped = true;
            return;
        }

        if (sessionLog == null)
            return;

        var dur = sessionLog.Duration;
        var timespan = new System.TimeSpan(0, 0, Mathf.RoundToInt(dur));

        _stopped = false;
        _text.text = timespan.ToString("c");
        
    }
}
