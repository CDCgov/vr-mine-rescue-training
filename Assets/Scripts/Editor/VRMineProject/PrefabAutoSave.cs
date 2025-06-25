using UnityEngine;
using UnityEditor;
using System;

///// <summary>
///// Automatically saves changes that are applied to prefabs. Only when you hit the "Apply" button when in scene.
///// </summary>
//[InitializeOnLoad]
//public class PrefabAutoSave
//{
//	private static bool _waiting;
//	private static DateTime _lastTime;

//	static PrefabAutoSave()
//	{
//		PrefabUtility.prefabInstanceUpdated -= PrefabInstanceUpdated;
//		PrefabUtility.prefabInstanceUpdated += PrefabInstanceUpdated;
//		EditorApplication.update -= Update;
//		EditorApplication.update += Update;
//	}

//	private static void PrefabInstanceUpdated(GameObject instance)
//	{
//		_waiting = true;
//		_lastTime = DateTime.Now;
//	}

//	private static void Update()
//	{
//		if(!_waiting || DateTime.Now - _lastTime < TimeSpan.FromSeconds(0.25))
//		{
//			return;
//		}
//		AssetDatabase.SaveAssets();
//		Debug.Log("Prefab change detected. Project Saved.");
//		_waiting = false;
//	}
//}