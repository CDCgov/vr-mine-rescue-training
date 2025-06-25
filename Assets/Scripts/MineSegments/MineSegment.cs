using UnityEngine;
using System.Collections.Generic;
using System.Text;
//using Sirenix.OdinInspector;
using MFireProtocol;

[SelectionBase()]
public class MineSegment : MonoBehaviour, ISelectableObject
{

	private struct SegmentMesh
	{
		public Mesh mesh;
		public Transform transform;
	}

	//public GameObject SegmentGeometryPrefab;
	public Bounds SegmentBounds;
    public Bounds IntersectionBounds;

	[System.NonSerialized]
	public Bounds SegmentBoundsWorldSpace;

    public bool IsIntersection = false;
	public bool IsBeltway = false;
	public bool IsMeshed = false;
	public bool IsStrapped = false;
	public bool IncludeInMap = true;
    public bool BuildNavMesh = true;

	//[InfoBox("Is this segment treated as being in outside atmosphere for ventilation computation?")]
	public bool IsInAtmosphere = false;

	public bool MFireStartJunction = false;
	public double CH4EmissionRateCFM = 0;
    public double InitialContam = 0;
    public double InitialContamConcentration = 0;
	public double AddedAirwayResistance = 0; 

	[System.NonSerialized]
	public List<MineSegmentLink> MineSegmentLinks;

	public SegmentGeometry SegmentGeometry;

	public int MFireJunction;
	[System.NonSerialized]
	public string DebugText = "";

	/// <summary>
	/// Parallel array witih SegmentGeometry.SegmentConnections for quickly accessing
	/// the adjoining mine segments. Null if the associated SegmentConnection has no connection
	/// </summary>
	[System.NonSerialized]
	public SegmentConnectionInfo[] SegmentConnections;

	//[HideInInspector]
	//public Quaternion _boundsMeshRotation;

	private Transform _geometryTransform;	
	private List<Mesh> _floorMeshes;
	private List<SegmentMesh> _segmentMeshes;
	private MineNetwork _mineNetwork;

	[System.NonSerialized]
	public LinkedList<MineElementHostBase> ContainedMineElements;

    private MFireServerControl _serverControl;
	//[System.NonSerialized]
	//public List<MFAirway> MFAirways;

	public MineSegment()
	{
		MineSegmentLinks = new List<MineSegmentLink>();
		//MFAirways = new List<MFAirway>();
	}

	//public void OnEnable()
	//{
	//	SceneControl.InitializeSegments += OnInitializeSegments;
	//	SceneControl.BeginSimulation += OnBeginSimulation;

 //       _serverControl = FindObjectOfType<MFireServerControl>();
	//}

	//public void OnDisable()
	//{
	//	SceneControl.InitializeSegments -= OnInitializeSegments;
	//	SceneControl.BeginSimulation -= OnBeginSimulation;
	//}
	//private void OnInitializeSegments()
	//{
	//	BindSegmentGeometry();
	//}

	private void OnBeginSimulation()
	{
		
	}
	
	public void UpdateVentilationResistance()
	{
        if (_serverControl == null)
            return;

		double resistance = 1.5f;
		bool hasVentControl = false;

		foreach (MineElementHostBase host in ContainedMineElements)
		{
            if (host == null || host.gameObject == null)
                continue;

			if (host is MineVentControlHost)
			{
				var ventHost = (MineVentControlHost)host;
				hasVentControl = true;
                if (host != null && host.enabled && host.gameObject.activeInHierarchy)
                {
                    resistance += ventHost.MineVentControl.AddedResistance;
                }
				ventHost.MineVentControl.AssociatedSegment = this;
			}
		}

		resistance += AddedAirwayResistance;

		if (hasVentControl || AddedAirwayResistance > 0)
		{
			foreach (MineSegmentLink link in MineSegmentLinks)
			{
				if (link.MFireAirway <= 0)
					continue;

				MFAirway airway = _serverControl.GetAirway(link.MFireAirway);

				double oldResist = airway.Resistance;
				airway.Resistance = resistance;

				if (oldResist != resistance)
                    _serverControl.ChangeAirway(airway);
			}
		}

		if (CH4EmissionRateCFM > 0)
		{
			foreach (MineSegmentLink link in MineSegmentLinks)
			{
				if (link.MFireAirway <= 0)
					continue;

				MFAirway airway = _serverControl.GetAirway(link.MFireAirway);

				airway.CH4EmissionRateAirway = CH4EmissionRateCFM / 2.0f;
			}
		}
	}

	public void AssociateMineElement(MineElementHostBase mineElementHost)
	{
		ContainedMineElements.AddLast(mineElementHost);

		DebugText += string.Format("Contains {0}\n", mineElementHost.GetType().ToString());
	}

    public void DissassociateMineElement(MineElementHostBase mineElementHost)
    {
        ContainedMineElements.Remove(mineElementHost);
    }

	public void RebuildGeometry()
	{
		//if (Application.isPlaying)
		//{
		//	Util.Log("ERROR: Attempted to rebuild MineSegment geometry while playing", VRMineLogType.Error);
		//	return;
		//}

		//_geometryTransform = transform.Find("Geometry");
		//if (_geometryTransform != null)
		//	DestroyImmediate(_geometryTransform.gameObject);

		//_geometryTransform = null;
		//BindSegmentGeometry();
	}

    public void InitializeSegmentConnections()
    {
        if (SegmentGeometry == null || SegmentGeometry.SegmentConnections == null)
        {
            Debug.LogError($"Mine Segment {name} missing or invalid segment geometry");
            return;
        }

        if (SegmentConnections == null)
            SegmentConnections = new SegmentConnectionInfo[SegmentGeometry.SegmentConnections.Length];
    }

	public void AddLink(MineSegmentLink link)
	{ 
		MineSegmentLinks.Add(link);

		if (SegmentGeometry == null)
			BindSegmentGeometry();

		if (SegmentGeometry == null || SegmentGeometry.SegmentConnections == null)
		{
			Debug.LogError($"Mine Segment {name} missing or invalid segment geometry");
			return;
		}

		if (SegmentConnections == null)
			SegmentConnections = new SegmentConnectionInfo[SegmentGeometry.SegmentConnections.Length];

		if (link.Segment1.SegmentGeometry == null || link.Segment2.SegmentGeometry == null)
		{
			Debug.LogError("MineSegment missing segment geometry");
			return;
		}

		//determine the connection index on this mine segment		
		SegmentConnectionInfo info = new SegmentConnectionInfo();

		if (link.Segment1 == this)
		{
			info.ConnIndex = link.Seg1ConnIndex;
			info.OppConnIndex = link.Seg2ConnIndex;
			info.OppMineSegment = link.Segment2;			
		}
		else if (link.Segment2 == this)
		{
			info.ConnIndex = link.Seg2ConnIndex;
			info.OppConnIndex = link.Seg1ConnIndex;
			info.OppMineSegment = link.Segment1;
		}
		else
		{
			Debug.LogErrorFormat("Invalid MineSegmentLink on {0}", gameObject.name);
			return;
		}

		if (info.OppMineSegment == null)
		{
			Debug.LogError($"Bad link detected!");
			return;
		}

		//cache connection ref
		info.Connection = GetSegmentConnection(link);
		info.OppConnection = info.OppMineSegment.GetSegmentConnection(link);

		if (info.Connection == null || info.OppConnection == null)
		{
			Debug.LogError($"Couldn't link segments! {link.Segment1.name} -> {link.Segment2.name}");
		}

		if (info.ConnIndex < 0 || info.ConnIndex > SegmentConnections.Length)
		{
			Debug.LogErrorFormat("Invalid connection index on {0}", gameObject.name);
			return;
		}

		SegmentConnections[info.ConnIndex] = info;

	}

	public void ClearLinks()
	{
		if (MineSegmentLinks != null)
			MineSegmentLinks.Clear();
	}

	public void CreateSegmentLink(MineSegment target, ref MineSegmentLink link)
	{
		if (SegmentGeometry == null)
			BindSegmentGeometry();

		float minDist = float.MaxValue;
		SegmentConnection[] seg1Connections = SegmentGeometry.SegmentConnections;
		SegmentConnection[] seg2Connections = target.GetSegmentConnections();

		link.Segment1 = this;
		link.Segment2 = target;

		//foreach (SegmentConnection conn in seg1Connections)
		for (int c1id = 0; c1id < seg1Connections.Length; c1id++)
		{
			SegmentConnection conn1 = seg1Connections[c1id];

			for (int c2id = 0; c2id < seg2Connections.Length; c2id++)
			{
				SegmentConnection conn2 = seg2Connections[c2id];

				Vector3 conn1WorldSpace = transform.TransformPoint(conn1.Centroid);
				Vector3 conn2WorldSpace = target.transform.TransformPoint(conn2.Centroid);

				float dist = Vector3.Distance(conn1WorldSpace, conn2WorldSpace);
				if (dist < minDist)
				{
					//link.Conn1 = conn1;
					//link.Conn2 = conn2;
					link.Seg1ConnIndex = c1id;
					link.Seg2ConnIndex = c2id;
					minDist = dist;
				}
			}
		}
	}

	public static float ComputeMinConnectionDist(MineSegment seg1, MineSegment seg2)
	{		
		float minDist = float.MaxValue;
		SegmentConnection[] seg1Connections = seg1.GetSegmentConnections();
		SegmentConnection[] seg2Connections = seg2.GetSegmentConnections();

		//foreach (SegmentConnection conn in seg1Connections)
		for (int c1id = 0; c1id < seg1Connections.Length; c1id++)
		{
			SegmentConnection conn1 = seg1Connections[c1id];

			for (int c2id = 0; c2id < seg2Connections.Length; c2id++)
			{
				SegmentConnection conn2 = seg2Connections[c2id];

				Vector3 conn1WorldSpace = seg1.transform.TransformPoint(conn1.Centroid);
				Vector3 conn2WorldSpace = seg2.transform.TransformPoint(conn2.Centroid);

				float dist = Vector3.Distance(conn1WorldSpace, conn2WorldSpace);
				if (dist < minDist)
				{
					minDist = dist;
				}
			}
		}

		return minDist;
	}

	public bool IsLinkedTo(MineSegment target)
	{
		foreach (MineSegmentLink link in MineSegmentLinks)
		{
			if (link.Segment1 == target || link.Segment2 == target)
				return true;
		}

		return false;
	}

	public SegmentConnection[] GetSegmentConnections()
	{
		if (SegmentGeometry == null)
			BindSegmentGeometry();

		return SegmentGeometry.SegmentConnections;
	}

	public SegmentConnection GetSegmentConnection(MineSegmentLink link)
	{
		if (SegmentGeometry == null || SegmentGeometry.SegmentConnections == null)
			return null;
		 
		if (link.Segment1 == this)
		{
			return SegmentGeometry.SegmentConnections[link.Seg1ConnIndex];
		}
		else if (link.Segment2 == this)
		{
			return SegmentGeometry.SegmentConnections[link.Seg2ConnIndex];
		}
		else
		{
			Debug.LogError("Error finding segment connection");
			return null;
		}
	}

	void OnDrawGizmos()
	{
		BindSegmentGeometry();
		//ComputeBounds();

		/*
		if (_floorMeshes != null && _floorMeshes.Count > 0)
		{
			foreach (Mesh mesh in _floorMeshes)
			{
				Gizmos.matrix = transform.localToWorldMatrix;
				Gizmos.color = new Color(0, 0, 0, 0);
				Gizmos.DrawMesh(mesh);
				Gizmos.matrix = Matrix4x4.identity;
			}
		}
		*/

		Gizmos.color = Color.yellow;
		foreach (MineSegmentLink link in MineSegmentLinks)
		{

			//Vector3 p1 = link.Segment1.transform.position + new Vector3(0, 0.5f, 0);
			//Vector3 p2 = link.Segment2.transform.position + new Vector3(0, 0.5f, 0);

			//Vector3 p1 = link.Segment1.SegmentBounds.center;
			//Vector3 p2 = link.Segment2.SegmentBounds.center;

			//Gizmos.DrawLine(p1, p2);
		}
	}

	void OnDrawGizmosSelected()
	{
		//ComputeBounds();
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(SegmentBounds.center + transform.position, SegmentBounds.size);

        if (IsIntersection)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(IntersectionBounds.center + transform.position, IntersectionBounds.size);
        }

        foreach (MineSegmentLink link in MineSegmentLinks)
		{

			Vector3 p1 = link.Segment1.transform.position + new Vector3(0, 3.5f, 0);
			Vector3 p2 = link.Segment2.transform.position + new Vector3(0, 3.5f, 0);

			Gizmos.DrawLine(p1, p2);
		}
	}

	public void ClearSegmentGeometry()
	{
		//Transform oldGeometry = transform.Find("Geometry");
		//if (oldGeometry != null)
		//	DestroyImmediate(oldGeometry.gameObject);

		//SegmentGeometry = null;
		//_geometryTransform = null;
	}
	
	public void BindSegmentGeometry()
	{
		//if (SegmentGeometryPrefab == null)
		//{
		//	Debug.LogError("Missing Segment Geometry!!");
		//	return;
		//}

		//if (_geometryTransform == null || SegmentGeometry == null)
		//{
		//	_geometryTransform = transform.Find("Geometry");
		//	if (_geometryTransform == null)
		//	{
		//		if (Application.isPlaying)
		//		{
		//			Util.Log("Error: Segment Geometry Not Initialized", VRMineLogType.Error);
		//		}

		//		CreateSegmentGeometry();
		//	}

		//	SegmentGeometry = _geometryTransform.GetComponentInChildren<SegmentGeometry>();
		//	if (SegmentGeometry == null)
		//	{
		//		Debug.LogError("Segment geometry prefab is missing SegmentGeometry component!!");
		//	}

			
		//}

		//if (SegmentConnections == null)
		//	SegmentConnections = new SegmentConnectionInfo[SegmentGeometry.SegmentConnections.Length];

		//if (Application.isEditor)
		//{
		//	_floorMeshes = new List<Mesh>();

		//	MeshFilter[] meshfilters = _geometryTransform.GetComponentsInChildren<MeshFilter>();
		//	foreach (MeshFilter mf in meshfilters)
		//	{
		//		if (mf.gameObject.layer == LayerMask.NameToLayer("Floor"))
		//		{					
		//			_floorMeshes.Add(mf.sharedMesh);                    
		//		}
				
		//	}

		//	if (_floorMeshes.Count == 0)
		//	{
		//		//Util.Log("Warning: Segment geometry has no floor mesh!", VRMineLogType.Warning);                
		//	}			
		//}
	}

	void DeformSegment()
	{
		if (_segmentMeshes == null)
			return;

		//compute where the segment is after deform in world space
		Vector3 worldPos = transform.position;
		worldPos = DeformWorldSpacePoint(worldPos);

		//reset the bounds to a point in the middle of the segment
		SegmentBounds = new Bounds(worldPos, Vector3.zero);

		//deform each mesh
		foreach (SegmentMesh sm in _segmentMeshes)
		{
			DeformMesh(sm.mesh, sm.transform);
			MeshCollider col = sm.transform.gameObject.AddComponent<MeshCollider>();
			col.sharedMesh = sm.mesh;
		}

		//deform all child transforms
		foreach (Transform child in transform)
		{
			if (child.name == "Geometry")
				continue;

			DeformChildTransforms(child);
		}

		//deform all lightprobe positions
		LightProbeGroup[] lightProbeGroups = transform.GetComponentsInChildren<LightProbeGroup>();
		if (lightProbeGroups != null && lightProbeGroups.Length > 0)
		{
			foreach (LightProbeGroup lpGroup in lightProbeGroups)
			{
				Debug.Log("Deforming light probe group");

				Vector3[] probePositions = lpGroup.probePositions;
				for (int i = 0; i < probePositions.Length; i++)
				{
					Vector3 pos = probePositions[i];

					pos = DeformLocalPoint(pos, lpGroup.transform);

					probePositions[i] = pos;
				}

				#if UNITY_EDITOR
				lpGroup.probePositions = probePositions;
				#endif
			}
		}

		//SegmentBounds.center -= transform.position;
	}

	
	void DeformChildTransforms(Transform t)
	{
		Transform[] children = t.GetComponentsInChildren<Transform>();

		foreach (Transform child in children)
		{
			//only deform the position of leaf nodes in the hierarchy
			if (child.childCount > 0)
				continue;

			DeformedTransform df = child.GetComponent<DeformedTransform>();
			if (df == null)
			{
				df = child.gameObject.AddComponent<DeformedTransform>();
				df.OriginalWorldSpacePosition = child.transform.position;
				df.OriginalWorldSpaceOrientation = child.transform.rotation;
			}

			Vector3 worldPos = df.OriginalWorldSpacePosition;
			Vector3 deformPos = DeformWorldSpacePoint(worldPos);
			Vector3 right = DeformWorldSpacePoint(worldPos + Vector3.right);
			Vector3 forward = DeformWorldSpacePoint(worldPos + Vector3.forward);
			right = right - deformPos;
			forward = forward - deformPos;

			Vector3 up = Vector3.Cross(forward, right);
			Quaternion rotation = Quaternion.FromToRotation(Vector3.up, up);

			child.transform.rotation = rotation * df.OriginalWorldSpaceOrientation;

			child.transform.position = deformPos;
		}
	}

	void DeformMesh(Mesh m, Transform t)
	{
		Vector3[] vertices = m.vertices;

		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 v = vertices[i];

			//transform to world space for deformation
			v = t.TransformPoint(v);

			//apply the deform
			v = DeformWorldSpacePoint(v);
			SegmentBounds.Encapsulate(v);

			//move back to local space
			v = t.InverseTransformPoint(v);

			vertices[i] = v;
		}

		m.vertices = vertices;

		m.RecalculateBounds();
		//m.RecalculateNormals();
	}

	Vector3 DeformLocalPoint(Vector3 v, Transform t)
	{
		//transform to world space for deformation
		v = t.TransformPoint(v);

		//apply the deform
		v = DeformWorldSpacePoint(v);		

		//move back to local space
		v = t.InverseTransformPoint(v);

		

		return v;
	}

	Vector3 DeformWorldSpacePoint(Vector3 v)
	{
		//v.y = v.y + v.x * 0.1f;
		return _mineNetwork.DeformWorldSpacePoint(v);
	}

	//void CreateSegmentGeometry()
	//{
	//	_mineNetwork = MineNetwork.FindSceneMineNetwork();

	//	Util.Log("Creating MineSegment Geometry");
	//	//create parent to hold geometry
	//	GameObject geomParent = new GameObject();
	//	geomParent.transform.SetParent(transform, false);
	//	//geomParent.hideFlags = HideFlags.NotEditable;
	//	geomParent.name = "Geometry";

	//	//instantiate geometry
	//	GameObject segGeomObj = GameObject.Instantiate<GameObject>(SegmentGeometryPrefab);
	//	if (Application.isEditor)
	//		segGeomObj.isStatic = true;

	//	//segGeomObj.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy;
	//	_geometryTransform = segGeomObj.transform;
	//	_geometryTransform.SetParent(geomParent.transform, false);
	//	_geometryTransform.localPosition = Vector3.zero;
	//	_geometryTransform.localRotation = Quaternion.identity;

	//	_segmentMeshes = new List<SegmentMesh>();

	//	if (_mineNetwork.EnableGeometryDeform)
	//	{
	//		SegmentGeometry segGeometry = _geometryTransform.GetComponentInChildren<SegmentGeometry>();

	//		MeshFilter[] meshfilters = _geometryTransform.GetComponentsInChildren<MeshFilter>();
	//		foreach (MeshFilter mf in meshfilters)
	//		{
	//			SegmentMesh sm = new SegmentMesh();

	//			//copy the mesh for deforming
	//			sm.mesh = Instantiate<Mesh>(mf.sharedMesh);
	//			mf.sharedMesh = sm.mesh;
	//			sm.transform = mf.transform;

	//			_segmentMeshes.Add(sm);
	//		}

	//		//remove all old colliders
	//		Collider[] colliders = transform.GetComponentsInChildren<Collider>();
	//		foreach (Collider col in colliders)
	//		{
	//			DestroyImmediate(col);
	//		}

	//		DeformSegment();


	//		foreach (SegmentConnection conn in segGeometry.SegmentConnections)
	//		{
	//			conn.Centroid = DeformLocalPoint(conn.Centroid, segGeometry.transform);
	//		}

	//		/*Object[] children = geomParent.transform.GetComponentsInChildren<Component>();
	//		foreach (Object child in children)
	//		{
	//			child.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
	//		}*/
	//	}
	//	else
	//	{
	//		ComputeBounds();
	//	}

		

		

	//}

	void ComputeBounds()
	{
		SegmentBounds = new Bounds(transform.position, Vector3.zero);
		MeshFilter[] meshfilters = _geometryTransform.GetComponentsInChildren<MeshFilter>();
		foreach (MeshFilter mf in meshfilters)
		{
			Vector3[] vertices = mf.sharedMesh.vertices;
			foreach (Vector3 v in vertices)
			{
				Vector3 transPos = mf.transform.TransformPoint(v);
				SegmentBounds.Encapsulate(transPos);
			}
		}

		/*
		SegmentBounds = new Bounds(Vector3.zero, Vector3.zero);

		//MeshFilter[] meshfilters = _geometryTransform.GetComponentsInChildren<MeshFilter>();
		//foreach (MeshFilter mf in meshfilters)
		//{
		//	mf.sharedMesh.RecalculateBounds();
		//	SegmentBounds.Encapsulate(mf.sharedMesh.bounds);
		//}


		MeshRenderer[] meshRenderers = _geometryTransform.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer mr in meshRenderers)
		{
			Bounds b = mr.bounds;
			b.center -= transform.position;
			
			if (SegmentBounds.size == Vector3.zero)
				SegmentBounds = b;
			else
				SegmentBounds.Encapsulate(b);
		}*/
	}

	void Start()
	{
		BindSegmentGeometry();
	}

	private void Awake()
	{
		ContainedMineElements = new LinkedList<MineElementHostBase>();

		//TODO: Update this to account for segment rotation
		SegmentBoundsWorldSpace = SegmentBounds;
		SegmentBoundsWorldSpace.center += transform.position;
	}

	public void GetObjectInfo(StringBuilder sb)
	{
		//sb.AppendFormat("Type : {0}\n", SegmentGeometryPrefab.name);
		if (SegmentGeometry != null)
			sb.AppendFormat("Type : {0}\n", SegmentGeometry.name);
		sb.AppendFormat("Position     : {0}\n", transform.position.GetColoredText());
		sb.AppendFormat("Rotation     : {0}\n", transform.rotation.GetColoredText());
		sb.AppendFormat("Is Beltway   : {0}\n", IsBeltway ? "Yes" : "No");

        if (_serverControl == null)
            return;

		double count = 0;
		double avgFlowRate = 0;
		double avgResistance = 0;
		double avgFPM = 0;

		foreach (MineSegmentLink link in MineSegmentLinks)
		{
			if (link.MFireAirway <= 0)
				continue;

			count++;
			MFAirway airway = _serverControl.GetAirway(link.MFireAirway);

			if (airway != null)
			{
				avgFlowRate += airway.FlowRate;
				avgResistance += airway.Resistance;
				avgFPM += (airway.FlowRate / airway.CrossSectionalArea);
			}
		}
			
		avgFlowRate /= count;
		avgResistance /= count;
		avgFPM /= count;

		if (count > 0)
		{
			sb.AppendFormat("Avg Flow Rate: {0:F2}\n", avgFlowRate);
			sb.AppendFormat("Avg Resist   : {0:F2}\n", avgResistance);
			sb.AppendFormat("Avg FPM      : {0:F2}\n", avgFPM);
		}

		if (MFireJunction > 0)
		{
			MFJunction junc = _serverControl.GetJunction(MFireJunction);
			if (junc != null)
			{

				sb.AppendFormat("Total Airflow: {0:F2}\n", junc.TotalAirFlow);
				sb.AppendFormat("Contam Conc  : {0:F2}\n", junc.ContamConcentration);
				sb.AppendFormat("CH4 Conc     : {0:F2}\n", junc.CH4Concentration);
				sb.AppendFormat("Atmo Temp    : {0:F2}\n", junc.AtmosphereTemperature);
			}
		}
	}

	public string GetObjectDisplayName()
	{
		return gameObject.name;
	}

	//void Update()
	//{

	//}

	private void OnValidate()
	{        
		//Transform roofBolts = transform.Find("Roofbolts");
		//Transform geometry = transform.Find("Geometry");
		//if (geometry != null)
		//{
		//	//Debug.Log("Got in here 1");
		//	Transform tileGeo = geometry.GetChild(0);
		//	if (tileGeo != null)
		//	{
		//		SegmentGeometry segGeoRef = tileGeo.GetComponent<SegmentGeometry>();                
		//		if (segGeoRef.MeshRef != null)
		//		{
		//			//Debug.Log("Got in here 3");
		//			segGeoRef.MeshRef.SetActive(IsMeshed);
		//			segGeoRef.StrapRef.SetActive(IsStrapped);
		//			if(roofBolts != null)
		//			{
		//				roofBolts.gameObject.SetActive(!IsMeshed);
		//			}
		//		}
		//	}
		//}
	}
}
