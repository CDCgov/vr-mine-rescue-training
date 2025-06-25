using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleProxExperimentGUI : MonoBehaviour
{
	public const float PointAddMinDelay = 0.05f;

	public ExperimentManager ExperimentManager;

	public TextMeshProUGUI TitleText;
	//public WMG_Axis_Graph Graph;

	private VehicleProxExperiment _experiment;
	private bool _initialized = false;
	//private WMG_Series _mainSeries;
	//private WMG_Series _pidSeries;
	//private WMG_Series _accelSeries;
	private float _timeLastPointAdded;

	private float _lastTime;
	private float _lastVel;

	// Use this for initialization
	void Start()
	{
	}

	private void OnGUI()
	{
		//GUILayout.TextArea("One\nTwo\nThree");

		if (ExperimentManager == null)
			return;

		if (_experiment == null)
			return;

		var trialSettings = _experiment.CurrentTrialSettings;
		if (trialSettings == null)
			return;

		GUILayout.TextArea(trialSettings.ToString());
	}

	private void OnEnable()
	{
		if (ExperimentManager == null)
			ExperimentManager = ExperimentManager.GetDefault();

		//ResetGUI();
		_initialized = false;
		ExperimentManager.ExperimentStarting += OnExperimentStart;
		_experiment = ExperimentManager.CurrentExperiment as VehicleProxExperiment;
	}

	private void OnDisable()
	{
		if (ExperimentManager != null)
		{
			ExperimentManager.ExperimentStarting -= OnExperimentStart;
		}
	}

	private void OnExperimentStart(Experiment experiment)
	{
		_experiment = experiment as VehicleProxExperiment;
		//ResetGUI();
		_initialized = false;
	}

	private void ResetGUI()
	{
		_initialized = false;
		TitleText.text = "";

		_lastTime = 0;
		_lastVel = 0;

		//Graph.deleteSeries();
		//Graph.deleteSeries();
		//Graph.deleteSeries();

		//_mainSeries = Graph.addSeries();
		//_mainSeries.pointColor = Color.black;
		//_mainSeries.lineColor = Color.black;

		//_accelSeries = Graph.addSeries();
		//_accelSeries.pointColor = Color.yellow;
		//_accelSeries.lineColor = Color.yellow;

		////-10 to 10 for PID
		//Graph.yAxis.AxisMinValue = 0;
		//Graph.yAxis.AxisMaxValue = 10;
		//Graph.yAxis.AxisNumTicks = 5;
		//Graph.yAxis.hideGrid = true;
		//Graph.yAxis.MaxAutoGrow = false;
		//Graph.yAxis.SetLabelsUsingMaxMin = true;

		//Graph.xAxis.AxisMinValue = 0;
		//Graph.xAxis.AxisMaxValue = 26;
		//Graph.xAxis.AxisNumTicks = 13;
		//Graph.xAxis.hideGrid = true;
		//Graph.xAxis.MaxAutoGrow = true;
		//Graph.xAxis.SetLabelsUsingMaxMin = true;		

		if (_experiment != null && _experiment.ShowPIDOutput)
		{
			//_pidSeries = Graph.addSeries();
			//_pidSeries.pointColor = Color.blue;
			//_pidSeries.lineColor = Color.blue;
			//_pidSeries.seriesName = "PID Output";

			////-10 to 10 for PID
			//Graph.yAxis.AxisMinValue = -10;
			//Graph.yAxis.AxisMaxValue = 10;
			//Graph.yAxis.AxisNumTicks = 10;
		}
		else
		{
			//_pidSeries = null;
		}
	}

	// Update is called once per frame
	void Update()
	{
		//if (_experiment == null || TitleText == null || Graph == null)
		//	return;

		if (!_experiment.Initialized)
			return;

		var trialSettings = _experiment.CurrentTrialSettings;
		if (_experiment.Initialized && !_initialized && trialSettings != null)
		{
			ResetGUI();
			TitleText.text = $"{_experiment.SessionName} - {_experiment.BlockName} - Trial {_experiment.TrialNum + 1} of {ExperimentManager.CurrentExperimentTrialCount}  Machine: {trialSettings.MachinePrefabName}";
			_initialized = true;
		}

		float elapsed = Time.unscaledTime - _timeLastPointAdded;
		if (elapsed > PointAddMinDelay)
		{
			float curTime = _experiment.ElapsedTime;
			float curVelocity = _experiment.CurrentVelocity.magnitude * VehicleProxExperiment.ConvMSToFPS;
			float curAccel = (curVelocity - _lastVel) / (curTime - _lastTime);			

			//_mainSeries.pointValues.Add(new Vector2(curTime, curVelocity));

			//if (_accelSeries != null && !float.IsInfinity(curAccel) && !float.IsNaN(curAccel))
			//{
			//	//_accelSeries.pointValues.Add(new Vector2(curTime, curAccel));
			//}

			//var carController = _experiment.CarController;
			//if (carController != null && _experiment.ShowPIDOutput && _pidSeries != null)
			//{
			//	_pidSeries.pointValues.Add(new Vector2(_experiment.ElapsedTime, carController.GetOutput()));
			//}

			_lastTime = curTime;
			_lastVel = curVelocity;
			_timeLastPointAdded = Time.unscaledTime;
		}
	}
}
