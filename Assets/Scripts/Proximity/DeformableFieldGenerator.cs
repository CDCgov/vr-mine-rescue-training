using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

#pragma warning disable 0219

[System.Serializable]
public class DeformableFieldGeneratorState
{
	public DeformableFieldGeneratorState()
	{
		Shells = new DeformableShell[2];

		for (int i = 0; i < Shells.Length; i++)
		{
			Shells[i] = new DeformableShell(DeformableProxSystem.MAP_WIDTH, DeformableProxSystem.MAP_HEIGHT);
		}
	}

	public DeformableShell[] Shells;
}

[System.Serializable]
public class DeformableFieldGenerator
{
	public string GeneratorID;
	public Vector3 Position;
	//public Quaternion Rotation;
	public Transform ParentTransform;

	[System.NonSerialized]
	public List<DeformableFieldGeneratorState> States;
	[System.NonSerialized]
	public List<float> StateBlendWeights;

	[System.NonSerialized]
	public Color GizmoColorOverride;

	public Vector3 WorldSpacePosition
	{
		get
		{
			//return ParentTransform.TransformPoint(Position);
			return ParentSpaceToWorldSpace(Position);
		}
	}

	private bool _generatorsLoaded = false;

	//cached transformation from parent space to "system space" - the coordinate space of the DeformablePRoxSystem
	//These are to be used exclusively for the multi-threaded visualization calculations, which must be rendered
	//in the coordinate space of the prox system

	private object _lockObject;
	private Matrix4x4 _parentToSystem;
	private Matrix4x4 _systemToParent;
	//private Quaternion _parentToSystemRot;
	//private Quaternion _systemtoParentRot; //inverse of _parentToSystemRot
	//private Vector3 _parentToSystemOffset;	

	public DeformableFieldGenerator()
	{
		_lockObject = new object();
		GeneratorID = Guid.NewGuid().ToString();
		States = new List<DeformableFieldGeneratorState>(4);
		StateBlendWeights = new List<float>(4);
		//Rotation = Quaternion.identity;

		_generatorsLoaded = false;

		AddState();
	}

	public void Save()
	{
		Directory.CreateDirectory("ProxData");
		string genFileName = string.Format("ProxData/{0}.dat", GeneratorID);

		//Debug.LogFormat("Saving Generator to {0}", genFileName);

		using (FileStream fs = new FileStream(genFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
		{
			//trying to fix strange error of git holding the file open?
			fs.Seek(0, SeekOrigin.Begin);			

			using (BinaryWriter writer = new BinaryWriter(fs))
			{
				Serialize(writer);
				 
				writer.Flush();
				fs.Flush();

				fs.SetLength(fs.Position); //truncate if needed
			}			
		}

	}

	public void Load()
	{
		//if (_generatorsLoaded)
		//			return; //only load generator data once per instantiation

		Directory.CreateDirectory("ProxData");
		string genFileName = string.Format("ProxData/{0}.dat", GeneratorID);

		if (!File.Exists(genFileName))
			return;

		//Debug.LogFormat("Loading Generator from {0}", genFileName);

		using (FileStream fs = new FileStream(genFileName, FileMode.Open))
		using (BinaryReader reader = new BinaryReader(fs))
		{
			Deserialize(reader);
		}

		_generatorsLoaded = true;
	}

	public void LoadFromCache(DeformableFieldGenerator cachedGen)
	{
		if (cachedGen == null || cachedGen.States == null)
			return;

		States = cachedGen.States;
		StateBlendWeights = new List<float>(cachedGen.StateBlendWeights);
		//StateBlendWeights = cachedGen.StateBlendWeights;

		NormalizeBlendWeights();
	}

	public void Serialize(BinaryWriter writer)
	{
		int stateCount = 0;
		if (States == null || States.Count == 0)
		{

			writer.Write(stateCount);
			return;
		}

		stateCount = States.Count;
		writer.Write(stateCount);

		for (int i = 0; i < stateCount; i++)
		{
			DeformableFieldGeneratorState state = States[i];
			state.Shells[0].Serialize(writer);
			state.Shells[1].Serialize(writer);
		}
	}

	public void Deserialize(BinaryReader reader)
	{
		int stateCount = reader.ReadInt32();
		if (stateCount <= 0)
			return;

		SetStateCount(stateCount);

		for (int i = 0; i < stateCount; i++)
		{
			DeformableFieldGeneratorState state = States[i];
			state.Shells[0].Deserialize(reader);
			state.Shells[1].Deserialize(reader);
		}
	}

	public int AddState()
	{
		DeformableFieldGeneratorState state = new DeformableFieldGeneratorState();
		States.Add(state);
		StateBlendWeights.Add(0);

		return States.Count - 1;
	}

	public void SetStateCount(int count)
	{
		//add or remove states as needed
		while (States.Count < count)
			AddState();

		while (States.Count > count)
			States.RemoveAt(States.Count - 1);

		//perform same process on the blend weight array
		while (StateBlendWeights.Count < count)
			StateBlendWeights.Add(0);

		while (StateBlendWeights.Count > count)
			StateBlendWeights.RemoveAt(StateBlendWeights.Count - 1);

		NormalizeBlendWeights();
	}

	public void SetBlendWeight(int state, float weight)
	{
		StateBlendWeights[state] = weight;
	}

	public float GetBlendWeight(int state)
	{
		return StateBlendWeights[state];
	}

	public void SetBlendWeights(List<float> weights)
	{
		if (weights == null || weights.Count > StateBlendWeights.Count)
			throw new Exception("attempted to set prox blend weights with invalid argument");

		for (int i = 0; i < weights.Count; i++)
		{
			StateBlendWeights[i] = weights[i];
		}
	}

	public void SetGeneratorPositionWorldSpace(Vector3 worldSpacePoint)
	{
		//Position = ParentTransform.InverseTransformPoint(worldSpacePoint);
		Position = WorldSpaceToParentSpace(worldSpacePoint);
	}

	/// <summary>
	/// normalize the blend weights to sum to 1
	/// </summary>
	public void NormalizeBlendWeights()
	{
		float sum = 0;

		for (int i = 0; i < StateBlendWeights.Count; i++)
			sum += StateBlendWeights[i];

		if (sum < 0.001)
		{
			StateBlendWeights[0] = 1;
			sum += 1.0f;
		}

		for (int i = 0; i < StateBlendWeights.Count; i++)
			StateBlendWeights[i] = StateBlendWeights[i] / sum;
	}


	/// <summary>
	/// Transform the provided world space point into the parent space of this generator
	/// </summary>
	/// <param name="worldSpacePoint"></param>
	/// <returns></returns>
	public Vector3 WorldSpaceToParentSpace(Vector3 worldSpacePoint)
	{
		return ParentTransform.InverseTransformPoint(worldSpacePoint);

		//Vector3 parentSpacePoint = worldSpacePoint - ParentTransform.position;

		//parentSpacePoint = ParentTransform.rotation * parentSpacePoint;

		//return parentSpacePoint;
	}

	public Vector3 ParentSpaceToWorldSpace(Vector3 parentSpacePoint)
	{
		return ParentTransform.TransformPoint(parentSpacePoint);

		//Vector3 worldSpacePoint;

		//worldSpacePoint = ParentTransform.rotation * parentSpacePoint;
		//worldSpacePoint += ParentTransform.position;

		//return worldSpacePoint;
	}
	
	//private Matrix4x4 _parentLocalToWorld;
	//private Matrix4x4 _parentWorldToLocal;
	//private Matrix4x4 _systemLocalToWorld;
	//private Matrix4x4 _systemWorldToLocal;
	

	public void CacheSystemSpaceTransform(Transform systemSpace)
	{
		//_parentLocalToWorld = ParentTransform.localToWorldMatrix;
		//_parentWorldToLocal = ParentTransform.worldToLocalMatrix;

		//_systemLocalToWorld = systemSpace.localToWorldMatrix;
		//_systemWorldToLocal = systemSpace.worldToLocalMatrix;

		//_parentToSystem = ParentTransform.localToWorldMatrix * systemSpace.worldToLocalMatrix;

		lock (_lockObject)
		{
			_parentToSystem = systemSpace.worldToLocalMatrix * ParentTransform.localToWorldMatrix;
			_systemToParent = Matrix4x4.Inverse(_parentToSystem);
		}
	}

	public Vector3 ParentSpaceToSystemSpace(Vector3 parentSpacePoint)
	{
		//Vector3 p = parentSpacePoint;
		//p = _parentLocalToWorld * p;
		//p = _systemWorldToLocal * p;
		//return p;

		lock (_lockObject)
		{
			parentSpacePoint = _parentToSystem.MultiplyPoint3x4(parentSpacePoint);
		}
		return parentSpacePoint;
	}

	public Vector3 SystemSpaceToParentSpace(Vector3 systemSpacePoint)
	{
		//Vector3 p = systemSpacePoint;
		//p = _systemLocalToWorld * p;
		//p = _parentWorldToLocal * p;
		//return p;

		lock (_lockObject)
		{
			systemSpacePoint = _systemToParent.MultiplyPoint3x4(systemSpacePoint);
		}

		return systemSpacePoint;
	}

	/// <summary>
	/// Get distance to the shell passing in a point in the coordinate space of the generators transform parent
	/// Uses the blended shell based on the provided weights
	/// </summary>
	/// <param name="shell"></param>
	/// <param name="parentSpacePoint"></param>
	/// <param name="weights"></param>
	/// <param name="distToGen"></param>
	/// <returns></returns>
	public float GetShellDistParentSpace(ProxShell shell, Vector3 parentSpacePoint, List<float> weights, out float distToGen)
	{
		//invert the generators transform to make the point relative to the generator
		parentSpacePoint -= Position;
		//localSpacePoint = Quaternion.Inverse(Rotation) * localSpacePoint;
		//localSpacePoint = rotInverse * localSpacePoint;

		distToGen = parentSpacePoint.magnitude;
		parentSpacePoint.Normalize();
		Vector2 coord = DeformableProxSystem.VectorToCoordinate(parentSpacePoint);

		return GetShellDistXY(shell, coord, weights);
	}

	/// <summary>
	/// Get distance to the shell passing in a point in the coordinate space of the generators transform parent
	/// Uses a single shell state determined by stateIndex
	/// </summary>
	/// <param name="shell"></param>
	/// <param name="parentSpacePoint"></param>
	/// <param name="stateIndex"></param>
	/// <param name="distToGen"></param>
	/// <returns></returns>
	public float GetShellDistParentSpace(ProxShell shell, Vector3 parentSpacePoint, int stateIndex, out float distToGen)
	{
		//invert the generators transform to make the point relative to the generator
		parentSpacePoint -= Position;
		//localSpacePoint = Quaternion.Inverse(Rotation) * localSpacePoint;

		distToGen = parentSpacePoint.magnitude;
		parentSpacePoint.Normalize();
		Vector2 coord = DeformableProxSystem.VectorToCoordinate(parentSpacePoint);

		return GetShellDistXY(shell, coord, stateIndex);
	}

	/// <summary>
	/// Get distance to the shell passing in a point in the coordinate space of the generator itself
	/// Uses the blended shell based on the provided weights
	/// </summary>
	/// <param name="shell"></param>
	/// <param name="dir"></param>
	/// <param name="weights"></param>
	/// <returns></returns>
	public float GetShellDistGenSpace(ProxShell shell, Vector3 dir, List<float> weights)
	{
		dir.Normalize();
		Vector2 coord = DeformableProxSystem.VectorToCoordinate(dir);

		return GetShellDistXY(shell, coord, weights);
	}

	/// <summary>
	/// Get distance to the shell passing in a point in the coordinate space of the generator itself
	/// Uses a single shell state determined by stateIndex
	/// </summary>
	/// <param name="shell"></param>
	/// <param name="dir"></param>
	/// <param name="stateIndex"></param>
	/// <returns></returns>
	public float GetShellDistGenSpace(ProxShell shell, Vector3 dir, int stateIndex)
	{
		dir.Normalize();
		Vector2 coord = DeformableProxSystem.VectorToCoordinate(dir);

		return GetShellDistXY(shell, coord, stateIndex);
	}

	/// <summary>
	/// Get the distance to the shell for the given x-y coordinate in the map
	/// Uses the blended shell based on the provided weights
	/// </summary>
	/// <param name="shell"></param>
	/// <param name="coord"></param>
	/// <param name="weights"></param>
	/// <returns></returns>
	public float GetShellDistXY(ProxShell shell, Vector2 coord, List<float> weights)
	{
		if (weights == null || weights.Count <= 0)
			weights = StateBlendWeights;

		int count = Mathf.Min(weights.Count, States.Count);
		float dist = 0;

		for (int i = 0; i < count; i++)
		{
			float val = States[i].Shells[(int)shell].GetValue((int)coord.x, (int)coord.y);
			dist += val * weights[i];
		}

		return dist;
	}

	/// <summary>
	/// Get the distance to the shell for the given x-y coordinate in the map
	/// Uses a single shell state determined by stateIndex
	/// </summary>
	/// <param name="shell"></param>
	/// <param name="coord"></param>
	/// <param name="stateIndex"></param>
	/// <returns></returns>
	public float GetShellDistXY(ProxShell shell, Vector2 coord, int stateIndex)
	{
		DeformableFieldGeneratorState state = States[stateIndex];

		return state.Shells[(int)shell].GetValue((int)coord.x, (int)coord.y);
	}

	/// <summary>
	/// Test if the provided point (in the coordinate space of the transform parent) is inside the shell
	/// </summary>
	/// <param name="parentSpacePoint"></param>
	/// <param name="shell"></param>
	/// <param name="weights"></param>
	/// <returns></returns>
	public bool TestPointParentSpace(Vector3 parentSpacePoint, ProxShell shell, List<float> weights)
	{
		//invert the generators transform to make the point relative to the generator
		parentSpacePoint -= Position;

		float dist = parentSpacePoint.magnitude;
		float testDist = GetShellDistGenSpace(shell, parentSpacePoint, weights);

		if (dist < testDist)
			return true;
		else
			return false;
	}

	/// <summary>
	/// Test if the provided point (in the coordinate space of the generator) is inside the shell
	/// </summary>
	/// <param name="genSpacePoint"></param>
	/// <param name="shell"></param>
	/// <param name="weights"></param>
	/// <returns></returns>
	public bool TestPointGeneratorSpace(Vector3 genSpacePoint, ProxShell shell, List<float> weights)
	{
		float dist = genSpacePoint.magnitude;
		float testDist = GetShellDistGenSpace(shell, genSpacePoint, weights);

		if (dist < testDist)
			return true;
		else
			return false;
	}

	public bool TestPointGeneratorSpace(Vector3 genSpacePoint, DeformableShell shell)
	{
		float dist = genSpacePoint.magnitude;

		genSpacePoint.Normalize();
		Vector2 coord = DeformableProxSystem.VectorToCoordinate(genSpacePoint);

		float testDist = shell.GetDist(coord);

		if (dist < testDist)
			return true;
		else
			return false;
		
	}


	public bool TestPointWorldSpace(Vector3 worldSpacePoint, ProxShell shell, List<float> weights)
	{
		//transform from world space to parent space
		Vector3 parentSpacePoint = WorldSpaceToParentSpace(worldSpacePoint);

		//invert the generators transform to make the point relative to the generator
		parentSpacePoint -= Position;

		float dist = parentSpacePoint.magnitude;
		float testDist = GetShellDistGenSpace(shell, parentSpacePoint, weights);

		if (dist < testDist)
			return true;
		else
			return false;
	}

	public bool TestPointSystemSpace(Vector3 systemSpacePoint, ProxShell shell, List<float> weights)
	{
		//transform from system space to parent space
		Vector3 parentSpacePoint = SystemSpaceToParentSpace(systemSpacePoint);

		//invert the generators transform to make the point relative to the generator
		parentSpacePoint -= Position;

		float dist = parentSpacePoint.magnitude;
		float testDist = GetShellDistGenSpace(shell, parentSpacePoint, weights);

		if (dist < testDist)
			return true;
		else
			return false;
	}

	/// <summary>
	/// Test a point in the coordinate space of the parent transform
	/// Uses the blended shell based on the provided weights
	/// </summary>
	/// <param name="parentSpacePoint"></param>
	/// <param name="weights"></param>
	/// <returns></returns>
	public ProxZone TestPointParentSpace(Vector3 parentSpacePoint, List<float> weights)
	{
		//invert the generators transform to make the point relative to the generator
		parentSpacePoint -= Position;
		//localSpacePoint = Quaternion.Inverse(Rotation) * localSpacePoint;

		float dist = parentSpacePoint.magnitude;
		float testDist = GetShellDistGenSpace(ProxShell.RedShell, parentSpacePoint, weights);

		if (dist < testDist)
			return ProxZone.RedZone;

		testDist = GetShellDistGenSpace(ProxShell.YellowShell, parentSpacePoint, weights);
		if (dist < testDist)
			return ProxZone.YellowZone;

		return ProxZone.GreenZone;
	}

	/// <summary>
	/// return the position of this generator in world space
	/// </summary>
	/// <returns></returns>
	public Vector3 GetPosition()
	{
		return ParentTransform.position + Position;
	}

	public void DeformSurface(Ray worldSpaceRay, int stateIndex, ProxShell shellID, float radius, float peakAmplitude)
	{
		DeformableFieldGeneratorState state = States[stateIndex];
		DeformableShell shell = state.Shells[(int)shellID];

		Vector3 hit;
		if (RaycastWorldSpace(worldSpaceRay, out hit, shell))
		{
			//Debug.LogFormat("Hit: {0}", hit);

			DeformSurface(hit, stateIndex, shellID, radius, peakAmplitude);
		}

	}

	public bool RaycastWorldSpace(Ray worldSpaceRay, out Vector3 hit, DeformableShell shell)
	{
		hit = Vector3.zero;

		//first transform the ray to generator space

		Vector3 origin = worldSpaceRay.origin;
		Vector3 target = worldSpaceRay.direction + worldSpaceRay.origin;

		origin = WorldSpaceToParentSpace(origin);
		target = WorldSpaceToParentSpace(target);

		//invert the generators transform to make the point relative to the generator
		origin -= Position;
		target -= Position;

		Vector3 dir = target - origin;
		dir.Normalize();


		//iterate along the ray until we hit the surface

		float dist = 0.1f;
		float increment = 0.01f;

		while (dist < 50)
		{
			Vector3 pt = origin + dir * dist;
			if (TestPointGeneratorSpace(pt, shell))
			{
				//hit the shell between here and the previous increment, use the midpoint
				hit = origin + dir * (dist - (increment * 0.5f));

				//transform the hit point from generator space back to world space
				hit += Position; //first to parent space
				hit = ParentSpaceToWorldSpace(hit);
				return true;
			}

			dist += increment;
		}

		return false;
	}

	public void DeformSurface(Vector3 worldSpacePoint, int stateIndex, ProxShell shellID, float radius, float peakAmplitude)
	{		
		Vector3 parentSpacePoint = WorldSpaceToParentSpace(worldSpacePoint);

		//invert the generators transform to make the point relative to the generator
		parentSpacePoint -= Position;

		parentSpacePoint.Normalize();

		//Debug.LogFormat("Deforming GenSpacePoint: {0}", parentSpacePoint);

		Vector2 coord = DeformableProxSystem.VectorToCoordinate(parentSpacePoint);

		DeformableFieldGeneratorState state = States[stateIndex];
		DeformableShell shell = state.Shells[(int)shellID];

		shell.DeformArea(coord, radius, peakAmplitude);
	}
}