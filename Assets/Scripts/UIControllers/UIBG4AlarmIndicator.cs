using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

public class UIBG4AlarmIndicator : MonoBehaviour, ISelectedPlayerView
{
    public BG4SimManager BG4SimManager;
    public float FlashPeriod = 0.1f;

    private PlayerRepresentation _player;

    private bool _showIndicator;
    private bool _flashing;
    private float _lastFlashTime;

    public void SetPlayer(PlayerRepresentation player)
    {

        ClearPlayer();

        _player = player;

        Debug.Log($"BG4AlarmIndicator: Set Player - {_player.PlayerID}");
        if(_player == null)
        {
            gameObject.SetActive(false);
        }
    }

    void UpdatePlayerData()
    {
        if (BG4SimManager == null)
            BG4SimManager = BG4SimManager.GetDefault(gameObject);

        if (_player == null || BG4SimManager == null)
            return;

        //Debug.Log($"BG4AlarmIndicator: Update - {_player.PlayerID}");


        var simData = BG4SimManager.GetSimData(_player.PlayerID);
        if (simData == null)
            return;

        bool showIndicator;
        bool flashing;
        Color color = Color.yellow;

        if (simData.CriticalPressure)
            color = Color.red;

        switch (simData.AlarmState)
        {
            case VRNBG4AlarmState.CriticalPressureAlarm:
            case VRNBG4AlarmState.LowPressureAlarm:
                showIndicator = true;
                flashing = true;
                break;

            case VRNBG4AlarmState.Silenced:
                showIndicator = true;
                flashing = false;
                break;

            default:
                showIndicator = false;
                flashing = false;
                break;
        }

        ShowIndicator(showIndicator, flashing, color);
    }

    void ShowIndicator(bool show, bool flashing, Color color)
    {
        foreach (Transform child in transform)
        {

            _showIndicator = show;
            _flashing = flashing;

            if (!_flashing)
                child.gameObject.SetActive(show);

            var svg = child.GetComponent<SVGImage>();
            if (svg != null)
                svg.color = color;
        }
    }

    void ClearPlayer()
    {
        if (_player != null)
        {
            _player = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"BG4AlarmIndicator: Start");

        if (BG4SimManager == null)
            BG4SimManager = BG4SimManager.GetDefault(gameObject);

        ShowIndicator(false, false, Color.red);

        UpdatePlayerData();
    }

    void OnEnable()
    {
        StartCoroutine(UpdatePlayerLoop());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        ClearPlayer();
    }

    private IEnumerator UpdatePlayerLoop()
    {
        yield return new WaitForEndOfFrame();
        while (true)
        {
            try
            {
                UpdatePlayerData();
            }
            catch (System.Exception) { }
        
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        if (_showIndicator && _flashing)
        {
            if (Time.time - _lastFlashTime > FlashPeriod)
            {
                _lastFlashTime = Time.time;
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(!child.gameObject.activeSelf);
                }
            }

        }
    }

}
