using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using MFireProtocol;


public class MFireTextUpdater : MonoBehaviour
{
	public bool DisplayJunctionNumber = true;
	public bool DisplayAirwayNumber = true;
	public bool DisplayAirwayAirflow = true;
	public bool DisplayJunctionAirflow = true;
	public bool DisplayContam = true;
	public bool DisplayCH4 = true;

	private TextMeshPro _text;
	private MineNetwork _network;

	private float _nextUpdate = 0;

	StringBuilder _sbText;
	private Transform _indicatorArrow;
	private Canvas _canvas;

	private bool _textVisible = true;
    private MFireServerControl _serverControl;

	// Use this for initialization
	void Start()
	{
		_text = GetComponentInChildren<TextMeshPro>();
		_network = MineNetwork.FindSceneMineNetwork();
		_sbText = new StringBuilder();
		_indicatorArrow = transform.Find("IndicatorArrow");
		_canvas = GetComponent<Canvas>();

        _serverControl = FindObjectOfType<MFireServerControl>();
	}

	void ShowText(bool bShow)
	{
		if (_textVisible == bShow)
			return;

		foreach (Transform child in transform)
		{
			child.gameObject.SetActive(bShow);
		}

		_textVisible = bShow;
	}

	void UpdateText()
	{
		if (_serverControl == null || !_serverControl.MFireRunning)
		{
			ShowText(false);
			return;
		}
		else
		{
			ShowText(true);
		}

		_sbText.Length = 0;

		//MFAirway airway = _network.FindClosestAirway(_indicatorArrow.position);
		//MFJunction junc = _network.FindClosestJunction(_indicatorArrow.position);

		//if (airway != null)
		//{
		//	if (DisplayAirwayNumber)
		//		_sbText.AppendFormat("Airway Number   : {0}\n", airway.Number);
		//	if (DisplayAirwayAirflow)
		//		_sbText.AppendFormat("Airflow Rate    : {0:n0} CFM\n", airway.FlowRate);
		//}

		//if (junc != null)
		//{
		//	if (DisplayJunctionNumber)
		//		_sbText.AppendFormat("Junction Number : {0}\n", junc.Number);
		//	if (DisplayContam)
		//		_sbText.AppendFormat("Contam. Conc.   : {0:F0}%\n", junc.ContamConcentration * 100.0f);
		//	if (DisplayCH4)
		//		_sbText.AppendFormat("Methane Conc.   : {0:F0}%\n", junc.CH4Concentration * 100.0f);
		//	if (DisplayJunctionAirflow)
		//		_sbText.AppendFormat("Total Air Flow  : {0:n0}\n", junc.TotalAirFlow);
		//}

		_text.text = _sbText.ToString();
	}

	// Update is called once per frame
	void Update()
	{
		if (Time.time > _nextUpdate)
		{
			UpdateText();
			_nextUpdate = Time.time + Random.Range(0.4f, 0.6f);
		}
	}
}
