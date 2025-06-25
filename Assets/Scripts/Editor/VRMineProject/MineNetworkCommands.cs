using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

#pragma warning disable 0219

public class MineNetworkCommands : ScriptableObject
{
	[MenuItem("Create Mine/Load Main Scene %#r", priority = 0)] //hokey ctrl+shift+r
	public static void LoadMainScene()
	{
		EditorSceneManager.OpenScene("Assets/Scenes/MasterScenes/MainScene.unity", OpenSceneMode.Single);
	}

	[MenuItem("Create Mine/Mine Segments/Link Mine Segments", priority = 200)]
	public static void LinkMineSegments()
	{

		MineNetwork mineNetwork = MineNetwork.FindSceneMineNetwork();
        UnityEditor.Undo.RecordObject(mineNetwork, "Link Segments");

		MineSegment[] mineSegments = GameObject.FindObjectsOfType<MineSegment>();

		Debug.LogFormat("Found {0} Mine Segments", mineSegments.Length);
		int linkCount = 0;

		for (int i = 0; i < mineSegments.Length; i++)
		{
			for (int j = i + 1; j < mineSegments.Length; j++)
			{
				MineSegment seg1 = mineSegments[i];
				MineSegment seg2 = mineSegments[j];

				float dist = MineSegment.ComputeMinConnectionDist(seg1, seg2);
				Debug.Log(dist);
				if (dist < 1.0f)
				{
					//close enough
					if (!seg1.IsLinkedTo(seg2))
					{
						MineNetwork.LinkSegments(seg1, seg2);
						linkCount++;
					}
				}
			} 
		}

		Debug.LogFormat("Created {0} links", linkCount);
		EditorSceneManager.MarkAllScenesDirty();
	}

	[MenuItem("Create Mine/Mine Segments/Rebuild All Geometry", priority = 200)]
	public static void RebuildAllGeometry()
	{

		MineNetwork mineNetwork = MineNetwork.FindSceneMineNetwork();

		//Transform geomParent = mineNetwork.GetGeometryParent();
		//Util.DestroyAllChildrenImmediate(geomParent);

		MineSegment[] mineSegments = GameObject.FindObjectsOfType<MineSegment>();
        UnityEditor.Undo.RecordObjects(mineSegments, "Rebuild Geometry");

		Debug.LogFormat("Found {0} Mine Segments", mineSegments.Length);

		for (int i = 0; i < mineSegments.Length; i++)
		{
			MineSegment seg = mineSegments[i];

			seg.ClearSegmentGeometry();
			seg.BindSegmentGeometry();
		}

		Resources.UnloadUnusedAssets();
		EditorSceneManager.MarkAllScenesDirty();
	}

	[MenuItem("Create Mine/Mine Segments/Clear All Geometry", priority = 200)]
	public static void ClearAllGeometry()
	{
		MineNetwork mineNetwork = MineNetwork.FindSceneMineNetwork();

		//Transform geomParent = mineNetwork.GetGeometryParent();
		//Util.DestroyAllChildrenImmediate(geomParent);

		MineSegment[] mineSegments = GameObject.FindObjectsOfType<MineSegment>();
		Debug.LogFormat("Found {0} Mine Segments", mineSegments.Length);

		for (int i = 0; i < mineSegments.Length; i++)
		{
			MineSegment seg = mineSegments[i];

			seg.ClearSegmentGeometry();
		}

		EditorSceneManager.MarkAllScenesDirty();
	}

    [MenuItem("Create Mine/Reset Networked Object IDs", priority = 200)]
    public static void ResetNetworkedObjectIDs()
    {

        var netObjects = GameObject.FindObjectsOfType<NetworkedObject>();
        Debug.LogFormat($"Found {netObjects.Length} Networked Objects");

        for (int i = 0; i < netObjects.Length; i++)
        {
            var obj = netObjects[i];
            obj.UniqueIDString = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(obj);

        }
    }

    [MenuItem("Create Mine/Mine Segments/Revert All Segments", priority = 200)]
	public static void RevertAllSegments()
	{		
		MineSegment[] mineSegments = GameObject.FindObjectsOfType<MineSegment>();
		Debug.LogFormat("Found {0} Mine Segments", mineSegments.Length);

		for (int i = 0; i < mineSegments.Length; i++)
		{
			MineSegment seg = mineSegments[i];

			//GameObject root = PrefabUtility.FindPrefabRoot(seg.gameObject);
			//PrefabUtility.ReconnectToLastPrefab(seg.gameObject);
			PrefabUtility.RevertPrefabInstance(seg.gameObject, InteractionMode.AutomatedAction);
		}
		EditorSceneManager.MarkAllScenesDirty();
	}


	[MenuItem("Create Mine/Mine Segments/Clear All Links", priority = 200)]
	public static void ClearAllLinks()
	{
		MineNetwork mineNetwork = MineNetwork.FindSceneMineNetwork();
		mineNetwork.MineSegmentLinks = null;
		Util.DestroyAllChildrenImmediate(mineNetwork.transform);

		MineSegment[] mineSegments = GameObject.FindObjectsOfType<MineSegment>();
		for (int i = 0; i < mineSegments.Length; i++)
		{
			mineSegments[i].ClearLinks();            
			mineSegments[i].SegmentConnections = new SegmentConnectionInfo[mineSegments[i].SegmentGeometry.SegmentConnections.Length];
		}

		EditorSceneManager.MarkAllScenesDirty();
	}

	[MenuItem("Test/Print Mine Network")]
	public static void PrintMineNetwork()
	{
		MineNetwork.InitializeLinks();

		MineSegment[] segments = GameObject.FindObjectsOfType<MineSegment>();

		for (int i = 0; i < segments.Length; i++)
		{
			MineSegment seg = segments[i];

			Debug.Log(seg.gameObject.name);
			for (int j = 0; j < seg.SegmentConnections.Length; j++)
			{
				SegmentConnectionInfo info = seg.SegmentConnections[j];
				if (info == null)
					Debug.LogFormat("{0}: Null", j);
				else
				{
					Debug.LogFormat("{0}: {1} ({2})", j, info.OppMineSegment.gameObject.name, info.OppConnIndex);
				}
			}
		}
	}

	
}