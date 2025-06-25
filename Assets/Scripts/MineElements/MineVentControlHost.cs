using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class MineVentControlHost : MineElementHostBase, IMineElementHost, ISelectableObject
{
    public VentilationManager VentilationManager;

	public MineVentControl MineVentControl;
	public KeyCode ToggleHotkey;

	private MeshRenderer[] _meshRenderers;
	private SkinnedMeshRenderer[] _skinnedMeshRenderers;
	private bool _bHighResist = true;

    private VentControl _ventControl;

    

	public MineElement GetMineElement()
	{
		return MineVentControl;
	}

	protected async override void Start()
	{
		base.Start();

        if (MineVentControl.AssociatedSegment == null)
            AssociateWithMineSegment();

        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        //wait for ventilation to initialize
        //await Task.Delay(1);

        //VentControl vc = new VentControl();
        //vc.ResistanceChange = MineVentControl.AddedResistance;
        //vc.WorldPosition = transform.position;

        //VentilationManager.AddVentControl(vc);

        VentilationManager.VentilationWillUpdate += OnVentilationWillUpdate;

    }

    void OnDestroy()
    {
        if (VentilationManager != null)
            VentilationManager.VentilationWillUpdate -= OnVentilationWillUpdate;

        SetResistance(0);
    }

    private void OnVentilationWillUpdate(VentGraph obj)
    {
        if (!gameObject.activeInHierarchy)
            return;

        var airway = obj.FindClosestAirway(transform.position);
        //airway.AddedResistance += MineVentControl.AddedResistance;
        airway.ControlResistance += MineVentControl.AddedResistance;

        Debug.Log($"VentControl: {gameObject.name} Adding {MineVentControl.AddedResistance:F0} resistance to airway {airway.AirwayID}");
    }


    public override void OnEnable()
    {
        base.OnEnable();

        AssociateWithMineSegment();
        UpdateVentilation();

        if (_associatedSegment != null)
            _associatedSegment.UpdateVentilationResistance();
    }

    public override void OnDisable()
    {
        base.OnDisable();

        UpdateVentilation();
    }

    [ResearcherAction("High Resist")]
	public void SetHighResistance()
	{
		Debug.Log("Changing vent control to high resistance");
		SetResistance(9000);
		ShowModel(true);
		_bHighResist = true;
	}

	[ResearcherAction("Med Resist")]
	public void SetMediumResistance()
	{
		Debug.Log("Changing vent control to medium resistance");
		SetResistance(50);
		ShowModel(true);
	}

	[ResearcherAction("Low Resist")]
	public void SetLowResistance()
	{
		Debug.Log("Changing vent control to low resistance");
		SetResistance(1.5);
		ShowModel(false);
		_bHighResist = false;
	}

	public void ShowModel(bool bShow)
	{
		if (_meshRenderers == null)
			_meshRenderers = GetComponentsInChildren<MeshRenderer>();

		if (_meshRenderers != null)
		{
			foreach (MeshRenderer mr in _meshRenderers)
			{
				mr.enabled = bShow;
			}
		}

		if (_skinnedMeshRenderers == null)
			_skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

		if (_skinnedMeshRenderers != null)
		{
			foreach (SkinnedMeshRenderer mr in _skinnedMeshRenderers)
			{
				mr.enabled = bShow;
			}
		}
	}

	public void SetResistance(double resistance)
	{
		if (MineVentControl.AssociatedSegment == null)
			return;

		MineVentControl.AddedResistance = resistance;
		MineVentControl.AssociatedSegment.UpdateVentilationResistance();
	}

    private void UpdateVentilation()
    {
        if (MineVentControl.AssociatedSegment == null)
            return;

        MineVentControl.AssociatedSegment.UpdateVentilationResistance();
    }

    protected override void OnInitializeSegments()
	{
		base.OnInitializeSegments();

        if (MineVentControl.AssociatedSegment == null)
		    AssociateWithMineSegment();
	}

	private void Update()
	{
		if (ToggleHotkey != KeyCode.None)
		{
			if (Input.GetKeyDown(ToggleHotkey))
			{
				if (_bHighResist)
					SetLowResistance();
				else
					SetHighResistance();
			}
		}
	}
}