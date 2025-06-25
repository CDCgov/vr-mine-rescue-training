using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Google.Protobuf;

public class MineElementHostBase : MonoBehaviour, INetSync, ISelectableObject
{	
	//private string _previewedResourceName;
	//private MeshPreviewData _previewData;

	protected MineElement _hostedMineElement;
	protected ProxSystemController _proxSystem = null;

	protected MineNetwork _network;
    protected MineSegment _associatedSegment;

	private MineElement HostedMineElement
	{
		get
		{
			if (_hostedMineElement == null)
			{
				IMineElementHost host = this as IMineElementHost;

				if (host != null)
				{
					_hostedMineElement = host.GetMineElement();
				}
			}

			return _hostedMineElement;
		}
	}

	protected virtual void Start()
	{
	
	}

	public virtual void OnEnable()
	{
		SceneControl.InitializeSegments += OnInitializeSegments;
		SceneControl.BeginSimulation += OnBeginSimulation;
	}

	public virtual void OnDisable()
	{
		SceneControl.InitializeSegments -= OnInitializeSegments;
		SceneControl.BeginSimulation -= OnBeginSimulation;
	}
	protected virtual void OnInitializeSegments()
	{
		_proxSystem = GetComponent<ProxSystemController>();
		_network = MineNetwork.FindSceneMineNetwork();
	}

	protected virtual void OnBeginSimulation()
	{
		
	}

	/// <summary>
	/// locate the mine segment that this mine element is in & notify it that
	/// it contains the mine element
	/// </summary>
	protected void AssociateWithMineSegment()
	{
        if (_network == null)
            _network = MineNetwork.FindSceneMineNetwork();

        if (_associatedSegment != null)
        {
            _associatedSegment.DissassociateMineElement(this);
        }

		MineSegment seg = _network.FindMineSegment(transform.position);
		if (seg != null)
		{
			seg.AssociateMineElement(this);
            _associatedSegment = seg;
		}
	}

	protected virtual string GetPreviewResource()
	{
		if (HostedMineElement == null)
			return null;

		return HostedMineElement.PrefabResource;
	}

	/*
	void OnDrawGizmos()
	{
		string resourceName = GetPreviewResource();

		if (_previewedResourceName == null || _previewedResourceName != resourceName)
		{
			_previewData = null;

			if (resourceName == null || resourceName.Length <= 0)
				return;

			GameObject obj = Resources.Load<GameObject>(resourceName);
			if (obj != null)
			{
				_previewData = Util.BuildPreviewData(obj);
			}

			if (_previewData != null)
			{
				Util.DrawPreview(transform, _previewData);
			}
		}
	}
	*/

	public virtual bool NeedsUpdate()
	{
		return HostedMineElement.NeedsUpdate();
	}

	public virtual void WriteObjState(CodedOutputStream writer)
	{
		HostedMineElement.WriteObjState(writer);
	}

	public virtual void SyncObjState(CodedInputStream reader)
	{
		HostedMineElement.SyncObjState(reader);
	}

	public virtual void GetObjectInfo(StringBuilder sb)
	{
		sb.AppendFormat("Position     : {0}\n", transform.position.GetColoredText());
		sb.AppendFormat("Rotation     : {0}\n", transform.rotation.GetColoredText());

		if (_proxSystem != null)
		{
			sb.AppendFormat("Prox Zone    : {0}\n", _proxSystem.ActiveProxZone.GetColoredText());
		}
	
		if (HostedMineElement != null)
			HostedMineElement.GetObjectInfo(sb);
	}

	public virtual string GetObjectDisplayName()
	{
		if (HostedMineElement != null)
			return HostedMineElement.DisplayName;
		else
			return gameObject.name;
	}
}