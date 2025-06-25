using UnityEngine;
using System.Collections.Generic;

#pragma warning disable 0219
#pragma warning disable 0168
#pragma warning disable 0414

/// <summary>
/// This class is responsible for simulating a cable under tension from a takeup real
/// </summary>
public class TensionedCable : MonoBehaviour
{
	public const float SegmentLength = 0.45f;

	public enum CableMode
	{
		LoopClosing,
		MidpointSmoothing,
		FollowTheLeader,
		TurnRadius
	}

	private struct CornerHit
	{
		public Vector3 hitPoint;
		public Vector3 cornerPoint;
		//public LinkedList<Vector3> segmentPath;
		public LinkedListNode<Vector3> pathNode;
	}

	public bool DrawDebugInfo = false;

	public CableMode CableTakeupMode;
	public Transform CableAnchorPoint;
	public Transform CableTarget;
	public float CableRadius = 0.06f;
	public float MaxTakeupSpeed = 8.0f;
	public float MaxLoopCloseSpeed = 10.0f;
	public float FTLTakeupSpeed = 10.0f;
	public float MaxStraightenMultiplier = 1.0f;
	public float CableTakeupFalloffDistance = 15.0f;
	public float MaxSegmentAngle = 5.0f;
	public float CableSlope = 1.0f;
	public float MaxCornerSharpness = 8;
	public float InitialStiffnessMaxAngle = 1;
	public float InitialStiffnessDistance = 2;
	public float InitialSegmentLength = 25.0f;
	public int InitialSegmentExtraIterations = 20;
	public float CableRadiusBufferMultipler = 4.0f;
    public float MaxCableLength = -1;
	

	[Range(1, 50)]
	public int CableTakeupIterations = 1;
	//public int CableTakeupSearchDistance = 3;

	private LinkedList<CornerHit> _corners;
	private LinkedList<Vector3> _path;
	//private List<float> _pathSegLengths;
	private List<float> _floorPos;

	//private LinkedList<Vector3> _currentSegPath;

	private Mesh _mesh;

	private int _maskWalls;
	private int _maskFloor;

	public List<Vector3> PathDisplay;

	void Start()
	{
		_corners = new LinkedList<CornerHit>();
		_path = new LinkedList<Vector3>();
		_floorPos = new List<float>(300);

		MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
		if (mr == null)
		{
			mr = gameObject.AddComponent<MeshRenderer>();
		}

		MeshFilter mf = gameObject.GetComponent<MeshFilter>();
		if (mf == null)
		{
			mf = gameObject.AddComponent<MeshFilter>();
		}

		_mesh = mf.mesh;
		if (_mesh == null)
		{
			_mesh = new Mesh();
			mf.sharedMesh = _mesh;
		}

		//things are broken right now - just do walls + floor + roof always
		_maskFloor = LayerMask.GetMask("Walls", "Roof", "Floor");
		_maskWalls = _maskFloor;

		AddPathSegment(CableAnchorPoint.position);
		AddPathSegment(CableTarget.position);

        //account for scaling
        //var scale = transform.lossyScale;
        //Vector3 invScale = new Vector3(1.0f / scale.x, 1.0f / scale.y, 1.0f / scale.z);
        //transform.localScale = invScale;
        //CableRadius = CableRadius * scale.x;
	}

	public void ResetCable()
	{
        if (gameObject == null)
            return;

        if (_corners == null || _path == null || _floorPos == null ||
            CableAnchorPoint == null || CableTarget == null)
            return;
        
		//Debug.Log($"Resetting cable {gameObject.name}");
        if (_corners != null)
		    _corners.Clear();
        if (_path != null)
		    _path.Clear();
        if (_floorPos != null)
		    _floorPos.Clear();

		AddPathSegment(CableAnchorPoint.position);
		AddPathSegment(CableTarget.position);

		RegenerateCableMesh();
	}


	void FixedUpdate()
	{
        if (CableTarget == null || CableAnchorPoint == null)
            return;

		RaycastHit hit;
		Vector3 targetPos = CableTarget.position;
		Vector3 pos = CableAnchorPoint.position;
		Vector3 prevCorner = pos;

		switch (CableTakeupMode)
		{
			case CableMode.LoopClosing:
				for (int i = 0; i < CableTakeupIterations; i++)
				{
					TakeupCableSlack();
				}

				ApplyGravity();
				break;

			case CableMode.MidpointSmoothing:
				

				for (int i = 0; i < CableTakeupIterations; i++)
				{
					TakeupCableSlackMidpoint();
				}
				//ApplyInitialCableStiffness();
				ApplyGravity();
				break;

			case CableMode.FollowTheLeader:
				TakeupCableSlackFTL();
				break;

			case CableMode.TurnRadius:
				TakeupCableTurnRadius();
				break;
		}


		_path.RemoveLast();
		Vector3 lastPos = _path.Last.Value;

		Vector3 dir = targetPos - lastPos;
		float dist = dir.magnitude;

		if (_path.Last.Previous != null)
		{
			Vector3 test = _path.Last.Previous.Value;
			Debug.DrawLine(test, test + Vector3.up * 0.1f, Color.green);
		}

		if (dist > SegmentLength)
		{
			Vector3 newPos = lastPos + targetPos;
			newPos *= 0.5f;

			_path.AddLast(newPos);
		}
		_path.AddLast(targetPos);

		if (PathDisplay == null)
			PathDisplay = new List<Vector3>();

		RegenerateCableMesh();
	}

	/// <summary>
	/// Apply gravity to the 2D-simulated cable causing it to droop down as permitted by the allowed slope
	/// </summary>
	void ApplyGravity()
	{
		RaycastHit hit;
		Vector3 prev = Vector3.zero, next = Vector3.zero;

		LinkedListNode<Vector3> node = _path.Last;
		if (node != null)
		{
			next = node.Value;
			node = node.Previous;
		}

		//for (int i = 1; i < _currentSegPath.Count - 1; i++)
		while (node != null && node.Previous != null)
		{
			Vector3 cur = node.Value;
			next = node.Next.Value;
			prev = node.Previous.Value;
		
			float floorYPos = FloorPos(cur);
			float distToFloor = cur.y - floorYPos;

			float y_max = 10; // Mathf.Max(next.y, prev.y);
			float y_min = floorYPos + (CableRadius * 1.5f);

			float slope = CableSlope;
			float slopeMult = distToFloor;
			slopeMult = Mathf.Clamp(slopeMult, 0.1f, 1.0f);
			slope *= slopeMult;

			ComputeAllowedHeight(cur, prev, slope, ref y_max, ref y_min);
			ComputeAllowedHeight(cur, next, slope, ref y_max, ref y_min);

			cur.y -= Time.deltaTime * 0.5f;

			if (cur.y > y_max)
			{
				cur.y -= Time.deltaTime * 1.0f;
				if (cur.y < y_max)
					cur.y = y_max;
			}
			if (cur.y < y_min)
			{
				cur.y += Time.deltaTime * 1.0f;
				if (cur.y > y_min)
					cur.y = y_min;
			}

			node.Value = cur;
			node = node.Previous;
		}
	}

	/// <summary>
	/// compute the min and max height allowed for the provided position, given the slope and adjacent nodes position
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="adjPos"></param>
	/// <param name="slope"></param>
	/// <param name="y_max"></param>
	/// <param name="y_min"></param>
	void ComputeAllowedHeight(Vector3 pos, Vector3 adjPos, float slope, ref float y_max, ref float y_min)
	{
		float dist = (adjPos - pos).magnitude;

		if (adjPos.y > pos.y)
		{
			y_min = Mathf.Max(y_min, adjPos.y - slope * dist);
		}
		else
		{
			y_max = Mathf.Min(y_max, adjPos.y + slope * dist);
		}
	}

	/// <summary>
	/// Takeup slack in the cable using the loop closing method
	/// </summary>
	void TakeupCableSlack()
	{
		RaycastHit hit;
		Vector3 prev = Vector3.zero, next = Vector3.zero;
		Vector3 lastCatchPoint = CableAnchorPoint.position;
		float distFromAnchor = 0;

		LinkedListNode<Vector3> dirChangePointNode = null;
		Vector3 loopStartPoint;
		Vector3 loopEndPoint;
		Vector3 loopDir = Vector3.zero ;
		Vector3 loopDirNorm = Vector3.zero;

		loopStartPoint = loopEndPoint = _path.Last.Value;

		LinkedListNode<Vector3> node = _path.Last;
		if (node != null)
		{
			next = node.Value;
			node = node.Previous;
		}

		while (node != null && node.Previous != null)
		{
			Vector3 cur = node.Value;
			float originalHeight = cur.y;

			next = node.Next.Value;
			prev = node.Previous.Value;

			next.y = prev.y = cur.y;

			//find the next point in the sequence that changes turn direction,
			//ending the local loop of cable
			if (dirChangePointNode == null || dirChangePointNode == node)
			{
				//we are at the beginning, or at the change point, search again
				LinkedListNode<Vector3> searchNode = node.Previous;

				Vector3 last_cross = Vector3.zero;

				Vector3 p1, p2, v, v_prev;
				p1 = searchNode.Next.Value;
				p2 = searchNode.Value;
				v_prev = p2 - p1;
				v_prev.y = 0;

				searchNode = searchNode.Previous;

				while (searchNode != null)
				{
					p1 = searchNode.Next.Value;
					p2 = searchNode.Value;
					v = p2 - p1;

					Vector3 crossp = Vector3.Cross(v_prev, v);
					//float dotp = Vector3.Dot(v_prev, v);
					float angle = Vector3.Angle(v_prev, v);

					if (last_cross.y == 0)
					{
						last_cross = crossp;
					}
					
					if (crossp.y * last_cross.y < 0 && angle > 2.0f)
					{
						//y component changed sign, so the rotation changed direction
						//I think this is right?	
						//we have found the point we are looking for					
						break;
					}

					if (DrawDebugInfo)
					{
						if (crossp.y > 0)
							Debug.DrawLine(p1, p1 + Vector3.up * (angle * 0.01f + 0.1f), Color.green);
						if (crossp.y < 0)
							Debug.DrawLine(p1, p1 + Vector3.down * (angle * 0.01f + 0.1f), Color.red);
					}

					v_prev = v;
					searchNode = searchNode.Previous;
				}

				if (searchNode == null || searchNode.Next == null)
				{
					//hit the beginning of the line
					dirChangePointNode = _path.First;
				}
				else
				{
					//take the last starting point from the last vector tested
					dirChangePointNode = searchNode.Next;
				}

				loopStartPoint = loopEndPoint;
				loopEndPoint = dirChangePointNode.Value;

				loopDir = loopEndPoint - loopStartPoint;
				loopDirNorm = loopDir.normalized;

				if (DrawDebugInfo)
					Debug.DrawLine(loopStartPoint, loopEndPoint, Color.red);
			}

			//find the line connecting the previous & next path vertices
			Vector3 targetLine = next - prev;
			float targetLineLen = targetLine.magnitude;
			float targetLineXZLen = targetLine.XZMagnitude();
			distFromAnchor += Vector3.Distance(cur, next);

			float speedMult = (CableTakeupFalloffDistance - distFromAnchor) / CableTakeupFalloffDistance;
			speedMult = Mathf.Clamp(speedMult, 0, 1);

			if (targetLineLen < SegmentLength)
			{
				//short segment, prune
				node = node.Previous;
				_path.Remove(node.Next);
				continue;
			}		

			if (speedMult > 0)
			{
				float maxDist;

				//first move towards the local-loop direct line
				//defined by the prior and next points that the curve changes direction
				//first project the current position onto the line
				Vector3 projectedPt = cur - loopStartPoint;
				float projectedLen = Vector3.Dot(projectedPt, loopDirNorm);
				projectedPt = projectedLen * loopDirNorm + loopStartPoint;

				//get the vector to the point
				Vector3 dir = projectedPt - cur;
				float dist = dir.magnitude;
				dir.Normalize();

				maxDist = dist;

				//don't move through walls
				if (Physics.Raycast(cur, dir, out hit, dir.magnitude, _maskWalls))
				{
					maxDist = Mathf.Min((hit.point - cur).magnitude - CableRadius * CableRadiusBufferMultipler, maxDist);
				}

				//move towards the point
				cur += dir * Mathf.Min(Time.deltaTime * MaxLoopCloseSpeed * speedMult * (maxDist + 0.5f), maxDist);

				//move node towards the interior of whatever curve it's on
				targetLine *= 0.5f;
				Vector3 midPt = prev + targetLine;

				Vector3 targetDir = midPt - cur;
				maxDist = targetDir.magnitude;
				targetDir.Normalize();

				//don't move through walls
				if (Physics.Raycast(cur, targetDir, out hit, targetDir.magnitude, _maskWalls))
				{
					maxDist = Mathf.Min((hit.point - cur).magnitude - CableRadius * CableRadiusBufferMultipler, maxDist);
				}

				cur += targetDir * Mathf.Min(maxDist * MaxStraightenMultiplier, Time.deltaTime * MaxTakeupSpeed * speedMult);
			}
			
			//reset y position and update
			cur.y = originalHeight;
			node.Value = cur;

			//prev = cur;
			next = cur;
			node = node.Previous;
		}
	}

	/// <summary>
	/// Takeup cable slack using the turn radius method
	/// </summary>
	void TakeupCableTurnRadius()
	{
		RaycastHit hit;
		Vector3 prev = Vector3.zero, next = Vector3.zero;
		Vector3 lastCatchPoint = CableAnchorPoint.position;
		float distFromAnchor = 0;

		LinkedListNode<Vector3> node = _path.Last;
		if (node != null)
		{
			//distFromAnchor = (node.Value - node.Previous.Value).magnitude;
			node = node.Previous;
		}

		Vector3 lastDir = CableTarget.forward;

		//for (int i = 1; i < _currentSegPath.Count - 1; i++)
		while (node != null && node.Previous != null) 
		{
			Vector3 cur = node.Value;
			float originalHeight = cur.y;
			next = node.Next.Value;
			prev = node.Previous.Value;

			next.y = prev.y = cur.y;

			Vector3 dir = cur - next;
			float segLength = dir.magnitude;
			segLength = Mathf.Clamp(segLength, 0.1f, SegmentLength * 1.2f);

			distFromAnchor += segLength;
			dir.Normalize();



			float angle = Vector3.Angle(lastDir, dir);
			Vector3 originalDir = dir;

			if (angle > InitialStiffnessMaxAngle)
			{

				Vector3 axis = Vector3.Cross(lastDir, dir);
				Quaternion rot = Quaternion.AngleAxis(InitialStiffnessMaxAngle, axis);

				dir = rot * lastDir;
				Vector3 target = dir * segLength + next;

				Debug.DrawLine(next, lastDir * segLength + next, Color.magenta);
				Debug.DrawLine(next, target, Color.white);
				Debug.DrawLine(cur, cur + axis, Color.yellow);
				Debug.DrawLine(next, originalDir * segLength + next, Color.green);

				cur = target;
			}

			lastDir = originalDir;

			node.Value = cur;

			node = node.Previous;
		}
	}

	void ApplyInitialCableStiffness()
	{
		RaycastHit hit;
		Vector3 prev = Vector3.zero, next = Vector3.zero;
		Vector3 lastCatchPoint = CableAnchorPoint.position;
		float distFromAnchor = 0;

		LinkedListNode<Vector3> node = _path.Last;
		if (node != null)
		{
			//distFromAnchor = (node.Value - node.Previous.Value).magnitude;
			node = node.Previous;
		}

		Vector3 lastDir = CableTarget.forward;

		//for (int i = 1; i < _currentSegPath.Count - 1; i++)
		while (node != null && node.Previous != null && distFromAnchor < InitialStiffnessDistance)
		{
			Vector3 cur = node.Value;
			float originalHeight = cur.y;
			next = node.Next.Value;
			prev = node.Previous.Value;

			next.y = prev.y = cur.y;

			Vector3 dir = cur - next;
			float segLength = dir.magnitude;
			segLength = Mathf.Clamp(segLength, 0.1f, SegmentLength * 1.2f);

			distFromAnchor += segLength; 
			dir.Normalize();

			

			float angle = Vector3.Angle(lastDir, dir);
			Vector3 originalDir = dir;

			if (angle > InitialStiffnessMaxAngle)
			{
				
				Vector3 axis = Vector3.Cross(lastDir, dir);
				Quaternion rot = Quaternion.AngleAxis(InitialStiffnessMaxAngle, axis);

				dir = rot * lastDir;
				Vector3 target = dir * segLength + next;

				Debug.DrawLine(next, lastDir * segLength + next, Color.magenta);
				Debug.DrawLine(next, target, Color.white);
				Debug.DrawLine(cur, cur + axis, Color.yellow);
				Debug.DrawLine(next, originalDir * segLength + next, Color.green);
				
				cur = target;
			}

			lastDir = originalDir;

			node.Value = cur;

			node = node.Previous;
		}
	}

	/// <summary>
	/// Takeup cable slack using the midpoint smoothing method
	/// </summary>
	void TakeupCableSlackMidpoint()
	{
		if (_path.Count < 3)
			return;

		RaycastHit hit;
		Vector3 prev = Vector3.zero, next = Vector3.zero;
		Vector3 lastCatchPoint = CableAnchorPoint.position;
		float distFromAnchor = 0;

		float initialSegmentLength = InitialSegmentLength;
		int initialSegmentExtraIterations = InitialSegmentExtraIterations;

		LinkedListNode<Vector3> node = _path.Last.Previous;
		Vector3 startPos = CableTarget.position;
		Vector3 startDir = CableTarget.forward;

		//for (int i = 1; i < _currentSegPath.Count - 1; i++)
		while (node != null && node.Previous != null)
		{
			Vector3 cur = node.Value;
			float originalHeight = cur.y;
			next = node.Next.Value;
			prev = node.Previous.Value;

			next.y = prev.y = cur.y;

			//find the line connecting the previous & next path vertices
			Vector3 targetLine = next - prev;
			float targetLineLen = targetLine.magnitude;
			float targetLineXZLen = targetLine.XZMagnitude();
			distFromAnchor += Vector3.Distance(cur, next);

			//compute how sharp a corner this is
			float angle = Vector3.Angle(cur - prev, next - cur);

			float speedMult = (CableTakeupFalloffDistance - distFromAnchor) / CableTakeupFalloffDistance;
			speedMult = Mathf.Clamp(speedMult, 0, 1);

			if (targetLineLen < SegmentLength && angle < 5.0f)
			{
				//short segment, prune
				//Debug.DrawLine(cur, cur + Vector3.up, Color.red, 1.0f, false);
				node = node.Previous;
				originalHeight = node.Value.y;
				_path.Remove(node.Next);
				
				continue;
			}

			if (angle > MaxCornerSharpness)
			{
				if (DrawDebugInfo)
					Debug.DrawLine(cur, cur + Vector3.up, Color.cyan);

				speedMult = Mathf.Max(0.5f, speedMult);
			}
			
			if (speedMult > 0)
			{
				float maxDist;

				//move node towards the interior of whatever curve it's on
				targetLine *= 0.5f;
				Vector3 midPt = prev + targetLine;

				Vector3 targetDir = midPt - cur;
				maxDist = targetDir.magnitude;
				targetDir.Normalize();

				//don't move through walls
				if (Physics.Raycast(cur, targetDir, out hit, targetDir.magnitude, _maskWalls))
				{
					maxDist = Mathf.Min((hit.point - cur).magnitude - CableRadius * CableRadiusBufferMultipler, maxDist);
				}

				cur += targetDir * Mathf.Min(maxDist * MaxStraightenMultiplier, Time.deltaTime * MaxTakeupSpeed * speedMult);
			}

			//reset y position and update
			cur.y = originalHeight;
			node.Value = cur;

			if (distFromAnchor > initialSegmentLength && initialSegmentLength > 0 && initialSegmentExtraIterations > 0)
			{
				initialSegmentExtraIterations--;
				initialSegmentLength -= 1.0f;

				//reset & repeat the first segment of the cable
				node = _path.Last.Previous;
				distFromAnchor = 0;
			}
			else
				node = node.Previous;
		}
	}

	/// <summary>
	/// takeup cable slack using the follow-the-leader method
	/// </summary>
	void TakeupCableSlackFTL()
	{
		RaycastHit hit;
		Vector3 prev = Vector3.zero, next = Vector3.zero;
		Vector3 lastCatchPoint = CableAnchorPoint.position;
		float distFromAnchor = 0;

		LinkedListNode<Vector3> dirChangePointNode = null;
		Vector3 loopStartPoint;
		Vector3 loopEndPoint;
		Vector3 loopDir = Vector3.zero;
		Vector3 loopDirNorm = Vector3.zero;
		Color debugColor = Color.yellow;

		//LinkedListNode<Vector3> nextNode, prevNode;

		loopStartPoint = loopEndPoint = _path.Last.Value;

		LinkedListNode<Vector3> node = _path.Last;
		if (node != null)
		{
			next = node.Value;
			node = node.Previous;
		}

		Vector3 lastDir = node.Next.Value - node.Value;
		lastDir.Normalize();

		//for (int i = 1; i < _currentSegPath.Count - 1; i++)
		while (node != null && node.Previous != null)
		{
			Vector3 cur = node.Value;
			float originalHeight = cur.y;
			
			next = node.Next.Value;
			prev = node.Previous.Value;
			
			//find the line connecting the previous & next path vertices
			Vector3 targetLine = next - prev;
			float targetLineLen = targetLine.magnitude;
			float targetLineXZLen = targetLine.XZMagnitude();
			distFromAnchor += Vector3.Distance(cur, next);

			float speedMult = (CableTakeupFalloffDistance - distFromAnchor) / CableTakeupFalloffDistance;
			speedMult = Mathf.Clamp(speedMult, 0, 1);

			if (targetLineXZLen < SegmentLength)
			{
				//short segment, prune
				node = node.Previous;
				_path.Remove(node.Next);
				continue;
			}

			
			if (speedMult > 0)
			{
				float maxDist;
				const float maxSeparation = SegmentLength * 1.8f;

				//move towards the point in front of you, not exceeding SegmentLength distance behind you
				Vector3 vToNext = next - cur;
				Vector3 vFromPrev = cur - prev;
				float distToPrev = vFromPrev.magnitude;

				//if too far, introduce a new point
				if (distToPrev > maxSeparation)
				{
					Vector3 mid = (cur + prev) * 0.5f;

					//don't spawn in walls
					if (Physics.Raycast(cur, mid - cur, out hit, maxSeparation * 2, _maskWalls))
					{
						mid = hit.point + hit.normal * CableRadius;
					}

					_path.AddBefore(node, mid);
					prev = mid;
				}

				maxDist = vToNext.magnitude * 0.75f;
				Vector3 dir = vToNext.normalized;

				//don't move through walls				
				if (Physics.Raycast(cur, dir, out hit, maxDist, _maskWalls))
				{
					if (DrawDebugInfo)
					{
						Debug.DrawLine(cur, hit.point, Color.red);
						Debug.DrawLine(cur, cur + dir * maxDist, Color.magenta);

						Debug.DrawLine(hit.point, hit.point + Vector3.up * 0.5f, Color.red);
					}

					debugColor = Color.red;

					dir = (hit.point + hit.normal * CableRadius * 1.5f) - cur;
					dir.Normalize();
				}
				else
				{
					debugColor = Color.yellow;
					if (DrawDebugInfo)
					{
						Debug.DrawLine(cur, cur + dir * maxDist, Color.green);
					}
				}

				dir *= Mathf.Min(maxDist, FTLTakeupSpeed * speedMult * Time.deltaTime);
				//vToNext *= FTLTakeupSpeed * Time.deltaTime;
				cur += dir;
				
			}

			if (DrawDebugInfo)
				Debug.DrawLine(cur, cur + Vector3.up * 0.15f, debugColor);

			node.Value = cur;

			//prev = cur;
			next = cur;
			node = node.Previous;
		}
	}
		
	void AddPathSegment(Vector3 pt)
	{	
		_path.AddLast(pt);
	}

	void RegenerateCableMesh()
	{
        if (MaxCableLength > 0)
        {
            var length = Vector3.Distance(CableAnchorPoint.position, CableTarget.position);
            if (length > MaxCableLength)
            {
                _mesh.Clear();
                return;
            }
        }

        Vector3[] vertices = _mesh.vertices;
		int[] triangles = _mesh.triangles;
		Vector2[] uv = _mesh.uv;

		ProcGeometry.GenerateTube(_path, CableRadius, ref vertices, ref triangles, ref uv);

		_mesh.Clear();
		_mesh.vertices = vertices;
		_mesh.triangles = triangles;
		_mesh.uv = uv;

		_mesh.RecalculateNormals();
		_mesh.RecalculateBounds();

		//hack because the mesh is generated in world space
		transform.localPosition = Vector3.zero;
		transform.localPosition = transform.position * -1;
	}

	bool IsPathBlocked(Vector3 p1, Vector3 p2, out RaycastHit hit)
	{
		return Physics.Linecast(p1, p2, out hit, _maskWalls);
	}

	float DistToFloor(Vector3 pos)
	{
		RaycastHit hit;
		if (Physics.Raycast(pos, Vector3.down, out hit, 15.0f, _maskFloor))
		{
			return (hit.point - pos).magnitude;
		}
		else
			return 0;
	}

	float FloorPos(Vector3 pos)
	{
		RaycastHit hit;
		if (Physics.Raycast(pos, Vector3.down, out hit, 15.0f, _maskFloor))
		{
			return hit.point.y;
		}
		else
			return 0;
	}
}
