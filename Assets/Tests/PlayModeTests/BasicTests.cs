using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class BasicTests
{

    [Test]
    public void BasicTestsSimplePasses()
    {
        // Use the Assert class to test conditions.
    }

    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator BasicTestsWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
    }

    [UnityTest]
    public IEnumerator TestInactiveGameobject()
    {
        var obj = new GameObject("TestObject");
        var obj2 = new GameObject("TestObject2");
        obj2.AddComponent<MeshFilter>();
        obj2.transform.SetParent(obj.transform);

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        var test1 = obj.GetComponentInChildren<MeshFilter>();
        Assert.That(test1, Is.Not.Null, "Couldn't get component from active object");

        obj.SetActive(false);

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        var boxCol = obj2.AddComponent<BoxCollider>();
        boxCol.center = Random.insideUnitSphere;
        boxCol.size = Random.insideUnitSphere * 3.0f;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        var test2 = obj.GetComponentInChildren<MeshFilter>();
        Assert.That(test2, Is.Not.Null, "Couldn't get component from inactive object");

        var test3 = obj.GetComponentInChildren<BoxCollider>();
        Assert.That(test3, Is.Not.Null, "Couldn't get new component from inactive object");

        Debug.Log($"Box collider center: {test3.center} size: {test3.size}");
    }

    [Test]
    public void TestSystemManager()
    {
        var res1 = Resources.Load<SystemManager>("Managers/SystemManager");
        var res2 = Resources.Load<SystemManager>("Managers/SystemManager");

        res1.TestString = "Res1";

        Assert.AreEqual(res1.TestString, res2.TestString);
    }

    [Test]
    public void TestSceneInBuild()
    {
#if UNITY_EDITOR
        return;
#else
        bool found = false;

        for (int i = 0; i < SceneManager.sceneCount; i++) 
        {
            var scene = SceneManager.GetSceneAt(i);
            Debug.Log($"Test Build Contains: {scene.name}");

            if (scene.name == NetworkManagerPlayModeTests.TestSceneName)
                found = true;
        }

        Assert.That(found, $"{NetworkManagerPlayModeTests.TestSceneName} not in test player build");
#endif
    }

    [Test]
    public void TestDefaultLODs()
    {
        ExternalAssetManager manager = ExternalAssetManager.GetDefault(null);

        var lod3 = manager.GetDefaultLODValues(3);
        Assert.That(lod3.Count, Is.EqualTo(3), "Wrong count for 3 LODs");

        for (int i = 0; i < lod3.Count; i++)
        {
            Debug.Log($"LOD3:{i} : {lod3[i].ScreenRelativeHeight}");
        }

        var lod5 = manager.GetDefaultLODValues(5);
        Assert.That(lod5.Count, Is.EqualTo(5), "Wrong count for 5 LODs");

        for (int i = 0; i < lod5.Count; i++)
        {
            Debug.Log($"LOD3:{i} : {lod5[i].ScreenRelativeHeight}");
        }
    }

    [Test]
    public void TestVRMineFolders()
    {
        SystemManager systemManager = SystemManager.GetDefault();

        systemManager.SystemConfig.RootDataFolderOverride = null;
        //test default root folder
        Debug.Log(systemManager.SystemConfig.ExternalAssetsFolder);
        Debug.Log(systemManager.SystemConfig.ScenariosFolder);
        Debug.Log(systemManager.SystemConfig.SessionLogsFolder);

        Assert.That(systemManager.SystemConfig.ExternalAssetsFolder.Contains("Users"));

        systemManager.SystemConfig.RootDataFolderOverride = "C:\\temp";
        System.IO.Directory.CreateDirectory(systemManager.SystemConfig.RootDataFolderOverride);

        Debug.Log(systemManager.SystemConfig.ExternalAssetsFolder);
        Debug.Log(systemManager.SystemConfig.ScenariosFolder);
        Debug.Log(systemManager.SystemConfig.SessionLogsFolder);

        Assert.That(systemManager.SystemConfig.ExternalAssetsFolder, Is.EqualTo("C:\\temp\\VRMine\\ExternalAssets"));
    }

    [UnityTest]
    public IEnumerator TestLoadScene()
    {
        var asyncOp = TestUtil.LoadScene(NetworkManagerPlayModeTests.TestSceneName, LoadSceneMode.Additive);
        yield return asyncOp;

        Assert.That(asyncOp.isDone, "Scene did not load");

        var obj = GameObject.Find("Ground");
        Assert.That(obj != null, "Couldn't find 'Ground' object");

        yield return new WaitForSecondsRealtime(2.0f);

        yield return TestUtil.UnloadScene(NetworkManagerPlayModeTests.TestSceneName);

        obj = GameObject.Find("Ground");
        Assert.That(obj == null, "Ground object not destroyed");
    }

    private int _testVal;
    [UnityTest]
    public IEnumerator NestedCoroutineTest()
    {
        _testVal = 0;
        yield return NestedCoroutine();

        Assert.That(_testVal == 3, $"testVal has wrong value: {_testVal}");
        Debug.Log($"_testVal: {_testVal}");
    }

    private IEnumerator NestedCoroutine()
    {
        _testVal = 0;
        yield return new WaitForEndOfFrame();
        _testVal++;
        yield return new WaitForEndOfFrame();
        _testVal++;
        yield return new WaitForEndOfFrame();
        _testVal++;
        yield return new WaitForEndOfFrame();
    }

    [Test]
    public void TestWrappedEnumerable()
    {
        int total = 0;
        foreach (int val in WrappedEnumerable())
        {
            total += val;
            Debug.Log(val);
        }

        Assert.That(total, Is.GreaterThan(500));
    }


    private IEnumerable<int> WrappedEnumerable()
    {
        return TestEnumerable(100);
    }


    private IEnumerable<int> TestEnumerable(int offset)
    {
        for (int i = 0; i < 5; i++)
        {
            yield return i + offset;
        }
    }



    [UnityTest]
    public IEnumerator TestMultiScenePhysics()
    {
        const string testScene = "SimpleTestScene";
        //Ray testRay = new Ray(new Vector3(0, 0, -10), new Vector3(0,0,1));
        //Ray testRayDown = new Ray(new Vector3(0, 10, 0), new Vector3(0, -1, 0));
        Ray testRay = new Ray(new Vector3(0, 10, 0), new Vector3(0, -1, 0));

        var scene1 = SceneManager.CreateScene("TestScene1", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        var scene2 = SceneManager.CreateScene("TestScene2", new CreateSceneParameters(LocalPhysicsMode.Physics3D));

        var phys1 = scene1.GetPhysicsScene();
        var phys2 = scene2.GetPhysicsScene();

        CreateTestSceneObject(scene1, "Sphere1");
        CreateTestSceneObject(scene2, "Sphere2");

        RaycastMatrixTest(testRay, phys1, "Sphere1", phys2, "Sphere2");

        yield return TestUtil.LoadScene(testScene, LoadSceneMode.Additive);
        var loadedScene1 = SceneManager.GetSceneByName(testScene);
        var testPhys = loadedScene1.GetPhysicsScene();
        Debug.Log("Loaded Scene Raycast Hits:");
        TestUtil.DebugLogAllRaycastHits(testPhys, testRay);

        SceneManager.MergeScenes(loadedScene1, scene1);
        GameObject.Find("Ground").name = "Ground1";

        Debug.Log("Scene 1 Raycast Hits:");
        TestUtil.DebugLogAllRaycastHits(phys1, testRay);

        yield return TestUtil.LoadScene(testScene, LoadSceneMode.Additive);
        var loadedScene2 = SceneManager.GetSceneByName(testScene);
        SceneManager.MergeScenes(loadedScene2, scene2);
        GameObject.Find("Ground").name = "Ground2";

        Debug.Log("Scene 2 Raycast Hits:");
        TestUtil.DebugLogAllRaycastHits(phys2, testRay);

        yield return new WaitForSecondsRealtime(3.0f);

        phys1.Simulate(Time.fixedDeltaTime);
        phys2.Simulate(Time.fixedDeltaTime);

        RaycastMatrixTest(testRay, phys1, "Sphere1", phys2, "Sphere2");
        RaycastMatrixTest(testRay, phys1, "Ground1", phys2, "Ground2");

        yield return new WaitForSecondsRealtime(3.0f);
    }

    private void RaycastMatrixTest(Ray testRay, PhysicsScene phys1, string obj1, PhysicsScene phys2, string obj2)
    {
        TestUtil.AssertRaycastHitsObject(phys1, testRay, obj1, "phys1");
        TestUtil.AssertRaycastHitsObject(phys2, testRay, obj2, "phys2");

        TestUtil.AssertRaycastDoesNotHitObject(phys1, testRay, obj2, "phys1");
        TestUtil.AssertRaycastDoesNotHitObject(phys2, testRay, obj1, "phys2");


        //Assert.That(TestUtil.DoesRaycastHitObject(phys1, testRay, obj2) == false, "phys1 hit Sphere2");

        //Assert.That(TestUtil.DoesRaycastHitObject(phys2, testRay, "Sphere1") == false, "phys2 hit Sphere1");
        //Assert.That(TestUtil.DoesRaycastHitObject(phys2, testRay, "Sphere2") == true, "phys2 didn't hit Sphere2");
    }

    private GameObject CreateTestSceneObject(Scene scene, string name)
    {
        SceneManager.SetActiveScene(scene);

        GameObject go = new GameObject(name);
        var collider = go.AddComponent<SphereCollider>();
        collider.radius = 2;

        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;

        return go;
    }

    [UnityTest]
    public IEnumerator TestTransformHierarchy()
    {
        int count = 0;
        int i = 0;

        var parent = new GameObject("Parent");
        yield return new WaitForSecondsRealtime(0.2f);

        for (i = 0; i < 3; i++)
        {
            var obj = new GameObject($"Object1-{i}");
            obj.transform.parent = parent.transform;
        }

        count = CountChildObjects(parent.transform);
        Assert.That(count, Is.EqualTo(3), "Transform child count didn't match - step 1");

        yield return new WaitForSecondsRealtime(0.2f);

        for (i = 0; i < 3; i++)
        {
            var obj = new GameObject($"Object1-{i}");
            obj.transform.SetParent(parent.transform);
        }

        count = CountChildObjects(parent.transform);
        Assert.That(count, Is.EqualTo(6), "Transform child count didn't match - step 2");

        yield return new WaitForSecondsRealtime(0.2f);
        count = CountChildObjects(parent.transform);
        Assert.That(count, Is.EqualTo(6), "Transform child count didn't match - step 3");
    }

    private int CountChildObjects(Transform xform)
    {
        int count = 0;
        foreach (var obj in xform)
        {
            count++;
        }

        return count;
    }

    [UnityTest]
    public IEnumerator TestTransformWorldPositionUpdate()
    {
        var pos1 = new Vector3(8, 9, 10);
        var pos2 = new Vector3(7, 7, 7);

        var parent = new GameObject("Parent");
        parent.transform.position = pos1;

        var obj1 = new GameObject("obj1");
        obj1.transform.localPosition = pos2;
        obj1.transform.SetParent(parent.transform, false);

        Assert.That(obj1.transform.position == pos1 + pos2, "obj1 position didn't match before delay");
        yield return new WaitForSecondsRealtime(0.2f);
        Assert.That(obj1.transform.position == pos1 + pos2, "obj1 position didn't match after delay");
    }

    [UnityTest]
    public IEnumerator TestLoadableAssetCategories()
    {
        var loadableManager = LoadableAssetManager.GetDefault(null);

        while (!loadableManager.LoadCompleted)
            yield return new WaitForEndOfFrame();

        Debug.Log($"Scanned Environments:");
        foreach (var asset in loadableManager.GetLoadableAssetsByCategory(LoadableAssetCategories.ScannedEnvironment))
        {
            Debug.Log(asset.AssetID);
        }

        Debug.Log($"Mine Tiles:");
        foreach (var asset in loadableManager.GetLoadableAssetsByCategory(LoadableAssetCategories.MineTile))
        {
            Debug.Log(asset.AssetID);
        }
    }

    [UnityTest]
    public IEnumerator TestAnimatorStateEvents()
    {
        GameObject netManagerObj = new GameObject("NetworkManager");
        var netManager = netManagerObj.AddComponent<NetworkManager>();

        netManager.StartAsServer = true;
        netManager.AcceptConnections = false;

        var asyncLoad = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Actors/DOL_Miner_RigFinal_NPC_Final.prefab");
        while (!asyncLoad.IsDone)
            yield return null;

        var prefab = asyncLoad.Result;
        var obj = GameObject.Instantiate<GameObject>(prefab);

        //obj.TryGetComponent<NetworkedObject>(out var netObj);
        //Object.Destroy(netObj);

        obj.TryGetComponent<Animator>(out var animator);

        int normalState = Animator.StringToHash("Idle");
        int deadState = Animator.StringToHash("Dead");
        int unconcsciousState = Animator.StringToHash("Unconscious");

        yield return new WaitForSecondsRealtime(1.0f);
        animator.Play(normalState, 0, 0.5f);

        yield return new WaitForSecondsRealtime(2.0f);
        animator.Play(unconcsciousState, 0, 0.5f);

        yield return new WaitForSecondsRealtime(2.0f);
        animator.Play(deadState, 0, 0.5f);

        yield return new WaitForSecondsRealtime(2.0f);
        animator.Play(normalState, 0, 0.5f);

        yield return new WaitForSecondsRealtime(2.0f);
        animator.Play(deadState, 0, 0.5f);

        yield return new WaitForSecondsRealtime(2.0f);
        animator.Play(normalState, 0, 0.5f);

        yield return new WaitForSecondsRealtime(5.0f);

        GameObject.Destroy(obj);
        Addressables.Release(asyncLoad);
    }

    [UnityTest]
    public IEnumerator TestStaticGasZones()
    {
        MineAtmosphere testAtm;

        GameObject netManagerObj = new GameObject("NetworkManager");
        var netManager = netManagerObj.AddComponent<NetworkManager>();

        netManager.StartAsServer = true;
        netManager.AcceptConnections = false;


        yield return new WaitForSecondsRealtime(0.1f);

        var ventControlObj = new GameObject("VentilationControl");
        var ventControl = ventControlObj.AddComponent<VentilationControl>();
        ventControl.VentilationProvider = VentilationProvider.StaticVentilation;


        VentilationManager ventManager = VentilationManager.GetDefault(null);
        //StaticVentilationManager staticVent = StaticVentilationManager.GetDefault(null);

        var staticVentObj = new GameObject("StaticVentilationManager");
        var staticVent = staticVentObj.AddComponent<StaticVentilationManager>();


        yield return new WaitForEndOfFrame();

        var obj1 = new GameObject("zone1");
        var obj2 = new GameObject("zone2");

        obj1.transform.position = Vector3.zero;
        obj2.transform.position = Vector3.zero;

        var zone1 = obj1.AddComponent<StaticVentilationZoneRect>();
        var zone2 = obj2.AddComponent<StaticVentilationZoneRect>();

        zone1.ZoneBounds = new Bounds(Vector3.zero, Vector3.one);
        zone2.ZoneBounds = new Bounds(Vector3.zero, Vector3.one);

        yield return new WaitForEndOfFrame();

        zone1.StaticMineAtmosphere.MineAtmosphere = new MineAtmosphere(0.1f, 0.1f, 0.1f, 0.1f);
        zone2.StaticMineAtmosphere.MineAtmosphere = new MineAtmosphere(0.2f, 0.2f, 0.2f, 0.2f);

        zone1.StaticMineAtmosphere.MineAtmosphere.SetStrength(0);
        zone2.StaticMineAtmosphere.MineAtmosphere.SetStrength(0);

        zone1.StaticMineAtmosphere.MineAtmosphere.OxygenStrength = 1.0f;
        zone2.StaticMineAtmosphere.MineAtmosphere.HydrogenSulfideStrength = 1.0f;

        staticVent.LoadStaticZones();

        ventManager.GetMineAtmosphere(Vector3.zero, out testAtm);
        Debug.Log(testAtm.ToString());

        Assert.That(testAtm.Oxygen, Is.EqualTo(0.1f));
        Assert.That(testAtm.HydrogenSulfide, Is.EqualTo(0.2f));

        yield return new WaitForEndOfFrame();

        obj2.transform.position = new Vector3(100, 100, 100);

        ventManager.GetMineAtmosphere(Vector3.zero, out testAtm);
        Debug.Log(testAtm.ToString());

        Assert.That(testAtm.Oxygen, Is.EqualTo(0.1f));
        Assert.That(testAtm.HydrogenSulfide, Is.EqualTo(0.0f));

    }


}
