using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class TestUtil 
{
    public static AsyncOperation LoadScene(string scene, LoadSceneMode mode)
    {
#if UNITY_EDITOR
        var scenePath = GetScenePath(scene);
        LoadSceneParameters par = new LoadSceneParameters(mode, LocalPhysicsMode.Physics3D);
        return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, par);
#else
        return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene, mode);
#endif

    }

    public static string GetScenePath(string scene)
    {
#if UNITY_EDITOR
        foreach (var sceneID in AssetDatabase.FindAssets("t:SceneAsset", new string[] { "Assets" }))
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(sceneID);
            var fileName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (fileName.Equals(scene, System.StringComparison.CurrentCultureIgnoreCase))
                return scenePath;
        }

        return scene;
#else
        return scene;
#endif
    }

    public static AsyncOperation UnloadScene(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            //Debug.Log(scene.name);

            if (scene.name == sceneName)
            {
                return SceneManager.UnloadSceneAsync(scene);
            }
        }

        return null;
    }

    public static IEnumerator LoadAndMergeScene(string sceneName, Scene destScene, bool moveManagers = true)
    {
        
        yield return TestUtil.LoadScene(sceneName, LoadSceneMode.Additive);
        var loadedScene1 = SceneManager.GetSceneByName(sceneName);
        var rootObjs = loadedScene1.GetRootGameObjects();

        //Debug.Log("Root Objects:");        
        //foreach (var obj in rootObjs)
        //{
        //    Debug.Log(obj.name);
        //}

        SceneManager.MergeScenes(loadedScene1, destScene);



        //wait for objects that set DoNotDestroyOnLoad
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (moveManagers)
        {
            //var managers = GameObject.FindObjectsOfType<SceneManagerBase>();
            //foreach (var manager in managers)
            //{
            //    if (manager.gameObject.scene.name != "DontDestroyOnLoad")
            //        continue;

            //    SceneManager.MoveGameObjectToScene(manager.gameObject, destScene);
            //}

            foreach (var obj in rootObjs)
            {
                if (obj.scene.name == "DontDestroyOnLoad")
                {
                    Debug.Log($"MergeScenes: Moving {obj.name} to {destScene.name}");
                    SceneManager.MoveGameObjectToScene(obj, destScene);
                }
                else if (obj.scene != destScene)
                {
                    Debug.LogError($"MergeScenes: Object {obj.name} in scene {obj.scene.name} instead of {destScene.name}");
                    //SceneManager.MoveGameObjectToScene(obj, destScene);
                }
            }
        }
    }

    public static void AssertRaycastHitsObject(PhysicsScene phys, Ray ray, string objectName, string physSceneName)
    {
        if (DoesRaycastHitObject(phys, ray, objectName) == true)
            Debug.Log($"{objectName} raycast in {physSceneName} hit");
        else
            Assert.Fail($"{objectName} not hit in {physSceneName}");
    }

    public static void AssertRaycastDoesNotHitObject(PhysicsScene phys, Ray ray, string objectName, string physSceneName)
    {
        if (DoesRaycastHitObject(phys, ray, objectName) == false)
            Debug.Log($"{objectName} raycast in {physSceneName} not hit");
        else
            Assert.Fail($"{objectName} hit in {physSceneName}");
    }

    public static bool DoesRaycastHitObject(PhysicsScene phys, Ray ray, string objectName)
    {
        return DoesRaycastHitObject(phys, ray.origin, ray.direction, objectName);
    }

    public static bool DoesRaycastHitObject(PhysicsScene phys, Vector3 origin, Vector3 dir, string objectName)
    {
        RaycastHit[] hits = new RaycastHit[10];
        int count = phys.Raycast(origin, dir, hits);

        for (int i = 0; i < count; i++)
        {
            if (hits[i].collider.gameObject.name == objectName)
                return true;
        }

        return false;
    }

    public static bool DebugLogAllRaycastHits(PhysicsScene phys, Ray ray)
    {
        return DebugLogAllRaycastHits(phys, ray.origin, ray.direction);
    }

    public static bool DebugLogAllRaycastHits(PhysicsScene phys, Vector3 origin, Vector3 dir)
    {
        RaycastHit[] hits = new RaycastHit[10];
        int count = phys.Raycast(origin, dir, hits);

        for (int i = 0; i < count; i++)
        {
            var col = hits[i].collider;
            var rbname = hits[i].rigidbody == null ? "None" : hits[i].rigidbody.name;

            Debug.Log($"Raycast Hit: {col.name} RB: {rbname}");
        }

        return false;
    }

    private static List<GameObject> _rootObjectCache = new List<GameObject>(100);

    public static T GetComponentInScene<T>(Scene scene, string assertMessage) where T : Component
    {
        var comp = GetComponentInScene<T>(scene);
        Assert.That(comp, Is.Not.Null, assertMessage);

        return comp;
    }

    public static T GetComponentInScene<T>(Scene scene) where T : Component
    {
        scene.GetRootGameObjects(_rootObjectCache);
        for (int i = 0; i < _rootObjectCache.Count; i++)
        {
            var comp = _rootObjectCache[i].GetComponentInChildren<T>();
            if (comp != null)
                return comp;
        }

        return null;
    }

    public static List<T> GetComponentsInScene<T>(Scene scene) where T : Component
    {
        List<T> results = new List<T>();

        scene.GetRootGameObjects(_rootObjectCache);
        for (int i = 0; i < _rootObjectCache.Count; i++)
        {
            var comps = _rootObjectCache[i].GetComponentsInChildren<T>();
            if (comps != null)
                results.AddRange(comps);
        }

        return results;
    }

    public static GameObject FindObjectInSceneRecursive(Scene scene, string objName)
    {
        scene.GetRootGameObjects(_rootObjectCache);
        for (int i = 0; i < _rootObjectCache.Count; i++)
        {
            var result = FindObjectInSceneRecursive(_rootObjectCache[i].transform, objName);
            if (result != null)
                return result;
        }

        return null;
    }

    public static GameObject FindObjectInSceneRecursive(Scene scene, string objName, string assertMessage)
    {
        var obj = FindObjectInSceneRecursive(scene, objName);
        Assert.That(obj, Is.Not.Null, assertMessage);

        return obj;
    }


    public static T GetComponent<T>(GameObject obj) where T : Component
    {
        return GetComponent<T>(obj, $"Couldn't get {typeof(T).ToString()} from object {obj.name}");
    }

    public static T GetComponent<T>(GameObject obj, string assertMessage) where T : Component
    {
        if (!obj.TryGetComponent<T>(out var comp))
            Debug.LogAssertion(assertMessage);

        return comp;
    }

    public static T GetComponent<T>(Component obj) where T : Component
    {
        return GetComponent<T>(obj, $"Couldn't get {typeof(T).ToString()} from object {obj.gameObject.name}");
    }

    public static T GetComponent<T>(Component obj, string assertMessage) where T : Component
    {
        if (!obj.TryGetComponent<T>(out var comp))
            Debug.LogAssertion(assertMessage);

        return comp;
    }

    private static GameObject FindObjectInSceneRecursive(Transform t, string objName)
    {
        if (t.gameObject.name == objName)
            return t.gameObject;

        foreach (Transform child in t)
        {
            var result = FindObjectInSceneRecursive(child, objName);
            if (result != null)
                return result;
        }

        return null;
    }

    public static GameObject FindObjectInSceneNonRecursive(Scene scene, string objName)
    {
        scene.GetRootGameObjects(_rootObjectCache);
        for (int i = 0; i < _rootObjectCache.Count; i++)
        {
            if (_rootObjectCache[i].name == objName)
                return _rootObjectCache[i];

            var child = _rootObjectCache[i].transform.Find(objName);
            if (child != null)
                return child.gameObject;
        }

        return null;
    }

    public static void AssertObjectNearPosition(Transform obj, Vector3 targetPos, string assertString, float maxDist = 0.1f)
    {
        var dist = Vector3.Distance(obj.position, targetPos);
        Assert.That(dist, Is.LessThanOrEqualTo(maxDist), assertString);
    }

}
