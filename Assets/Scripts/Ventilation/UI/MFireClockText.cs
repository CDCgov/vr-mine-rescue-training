using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class MFireClockText : MonoBehaviour
{
	private TextMeshProUGUI _clockText;

    private MFireServerControl _serverControl;

	// Use this for initialization
	void Start()
         
	{
		_clockText = GetComponent<TextMeshProUGUI>();
        _serverControl = FindObjectOfType<MFireServerControl>();
	}

	// Update is called once per frame
	void Update()
	{
		if (_serverControl == null || !_serverControl.MFireRunning)
		{
			_clockText.text = "";
			return;
		}

		int elapsedSeconds = (int)(Math.Round(_serverControl.MFireElapsedTime / 1000.0));
		int elapsedMinutes = elapsedSeconds / 60;
		elapsedSeconds -= elapsedMinutes * 60;
		int elapsedHours = elapsedMinutes / 60;
		elapsedMinutes -= elapsedHours * 60;

		if (_clockText != null)
			_clockText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", elapsedHours, elapsedMinutes, elapsedSeconds);
	}
}
