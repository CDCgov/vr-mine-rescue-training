using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class SceneInitializer
{

	public static MocapManager MocapManager;

	//static SceneInitializer()
	//{
	//	Debug.Log("SceneInitalizer Loaded");

	//	SceneManager.sceneLoaded += OnSceneLoaded;
	//	SceneManager.sceneUnloaded += OnSceneUnloaded;
	//	SceneManager.activeSceneChanged += OnActiveSceneChanged;

	//	if (MocapManager == null)
	//		MocapManager = MocapManager.GetDefault();

	//}

	private static void OnActiveSceneChanged(Scene arg0, Scene arg1)
	{
		//Debug.Log("SceneInitializer::OnActiveSceneChanged");
	}

	private static void OnSceneUnloaded(Scene arg0)
	{
		//Debug.Log("SceneInitializer::OnSceneUnloaded");
	}

	private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		//Debug.Log("SceneInitializer::OnSceneLoaded");

		var sceneControl = MasterControl.GetSceneControl();
		if (sceneControl == null)
		{
			//not a valid mine scene, do nothing
			return;
		}

		GameObject masterObj = GameObject.Find("MasterObject");
		if (masterObj == null && Camera.main == null)
		{
			Debug.Log("Couldn't find master object or main camera, loading master object prefab");
			masterObj = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("MasterPrefabs/MasterObject"));
			var masterControl = masterObj.GetComponent<MasterControl>();
			masterControl.Initialize();
			var eventSystem = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("MasterPrefabs/EventSystem"));
		}
	}
}
