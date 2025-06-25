using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using Google.Protobuf;
using UnityEngine.AddressableAssets;
using Stopwatch = System.Diagnostics.Stopwatch;

public class MultiSceneNetworkTests
{
    private class InstanceData
    {
        public Scene Scene;
        public PhysicsScene PhysicsScene;        
        public NetworkManager NetworkManager;
        //public NetworkedObjectManager NetworkedObjectManager;
        public PlayerManager PlayerManager;

        public void GetTestClientRig(out Transform rig, out CustomXRInteractor rightInteractor)
        {
            var rigObj = TestUtil.FindObjectInSceneRecursive(Scene, "XR Rig", "Couldn't find XR Rig in test scene");
            rig = rigObj.transform;

            var rightDirect = TestUtil.FindObjectInSceneRecursive(Scene, "RightDirect", "Couldn't find RightDirect in test scene");
            rightInteractor = TestUtil.GetComponent<CustomXRInteractor>(rightDirect);
            rightInteractor.SetPhysicsSceneOverride(PhysicsScene);
        }
    }

    private const string DMSceneName = "NetworkTestSceneDM";
    private const string ClientSceneName = "NetworkTestSceneClient";
    private const int NumClients = 3;

    private List<InstanceData> _clients;
    private InstanceData _dm;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        LoadableAssetManager.EnableExternalAssetLoading = false;
        Util.EnableDontDestroyOnLoad = false;

        _dm = CreateInstance("DMScene");
        yield return TestUtil.LoadAndMergeScene(DMSceneName, _dm.Scene);        

        _clients = new List<InstanceData>();
        for (int i = 0; i < NumClients; i++)
        {
            var client = CreateInstance($"Client{i}");
            yield return TestUtil.LoadAndMergeScene(ClientSceneName, client.Scene);
            yield return new WaitForSecondsRealtime(0.25f);

            var netManager = TestUtil.GetComponentInScene<NetworkManager>(client.Scene);
            netManager.ClientIDAssigned += (clientID) =>
            {
                var playerManager = PlayerManager.GetDefault(netManager.gameObject);
                Debug.Log($"PlayerManager {playerManager.gameObject.GetInstanceID()} Client {netManager.ClientID} Requesting player id");
                playerManager.RequestPlayerID();
            };

            if (netManager.ClientID > 0)
            {
                var playerManager = PlayerManager.GetDefault(netManager.gameObject);
                Debug.Log($"PlayerManager {playerManager.gameObject.GetInstanceID()} Client {netManager.ClientID} Requesting player id");
                playerManager.RequestPlayerID();
            }
                

            client.NetworkManager = netManager;

            _clients.Add(client);
        }

        _dm.NetworkManager = TestUtil.GetComponentInScene<NetworkManager>(_dm.Scene);
        _dm.PlayerManager = PlayerManager.GetDefault(_dm.NetworkManager.gameObject);
        Assert.That(_dm.PlayerManager, Is.Not.Null, "Couldn't get DM player manager");

        while (AreClientsReady() == false)
        {
            yield return new WaitForSecondsRealtime(1.0f);
            Debug.Log("Waiting for clients to connect...");
        }

        //Debug.Log($"NetworkManager Scene: {NetworkManager.GetDefault().gameObject.scene.name}");

        //wait for all objects to load and synchronize
        var netObjects = GameObject.FindObjectsOfType<NetworkedObject>();
        while (AreNetObjectsReady(netObjects) == false)
        {
            yield return new WaitForSecondsRealtime(1.0f);
            Debug.Log("Waiting for networked objects to initialize...");
        }

        yield return new WaitForSecondsRealtime(5.0f);
        yield return AdvanceFrame();
        yield return new WaitForSecondsRealtime(1.0f);
        //yield return AdvanceFrame();
    }

    private void OverridePhysicsSceneAll()
    {
        OverridePhysicsScene(_dm);
        foreach (var client in _clients)
        {
            OverridePhysicsScene(client);
        }
    }

    private void OverridePhysicsScene(InstanceData inst)
    {
        var physScene = inst.Scene.GetPhysicsScene();

        var rootObjects = inst.Scene.GetRootGameObjects();

        List<CustomXRSocket> compList = new List<CustomXRSocket>();

        foreach (var obj in rootObjects)
        {
            compList.Clear();
            obj.GetComponentsInChildren<CustomXRSocket>(compList);

            foreach(var comp in compList)
            {
                comp.OverridePhysicsScene(physScene);
            }
        }
    }

    private void OnClientIDAssigned(int clientID)
    {

    }

    private bool AreNetObjectsReady(NetworkedObject[] netObjects)
    {
        foreach (var netObj in netObjects)
        {
            if (!netObj.Initialized)
                return false;
        }

        return true;
    }

    private bool AreClientsReady()
    {        
        foreach (var client in _clients)
        {
            if (client.NetworkManager == null)
                return false;

            if (client.NetworkManager.ClientID < 0)
                return false;

            client.PlayerManager = PlayerManager.GetDefault(client.NetworkManager.gameObject);
            if (client.PlayerManager == null)
                return false;

            //if (client.PlayerManager.CurrentPlayer.PlayerRole == VRNPlayerRole.UnknownRole)
            //    client.PlayerManager.CurrentPlayer.PlayerRole = VRNPlayerRole.Captain;

            if (client.PlayerManager.CurrentPlayer == null || client.PlayerManager.CurrentPlayer.PlayerID <= 0)
                return false;
        }

        return true;
    }


    private T CreateManagerObject<T>(string objName) where T : Component
    {
        var obj = new GameObject(objName);
        T manager = obj.AddComponent<T>();

        return manager;
    }

    private InstanceData CreateInstance(string sceneName)
    {
        InstanceData inst = new InstanceData();
        inst.Scene = SceneManager.CreateScene(sceneName, new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        inst.PhysicsScene = inst.Scene.GetPhysicsScene();

        GameObject physStep = new GameObject("ScenePhysicsStep");
        SceneManager.MoveGameObjectToScene(physStep, inst.Scene);

        physStep.AddComponent<ScenePhysicsStep>();

        return inst;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Debug.Log("Starting Multi Scene Network Teardown");
        yield return SceneManager.UnloadSceneAsync(_dm.Scene);
        foreach (var client in _clients)
        {
            yield return SceneManager.UnloadSceneAsync(client.Scene);
        }

        _dm = null;
        _clients = null;

        yield return new WaitForSecondsRealtime(1.0f);
    }

    [UnityTest]
    public IEnumerator SetupTest()
    {
        yield return new WaitForSecondsRealtime(1.0f);

        for (int i = 0; i < NumClients; i++)
        {
            var client = _clients[i];

            CheckObjectsInScene<CustomXRInteractor>(client.Scene);
        }
    }

    private void CheckObjectsInScene<T>(Scene scene) where T: Component
    {
        var comps = TestUtil.GetComponentsInScene<T>(scene);

        foreach (var comp in comps)
        {
            Assert.That(comp.gameObject.scene == scene, $"Object {comp.name} is not in scene {scene.name}");

            Debug.Log($"Component {typeof(T).Name} found on {comp.name} in scene {scene.name}");
        }
    }

    private void UpdatePhysics()
    {
        Physics.SyncTransforms();
        UpdatePhysics(_dm);

        foreach (var client in _clients)
        {
            UpdatePhysics(client);
        }
    }

    private void UpdatePhysics(InstanceData data)
    {
        data.PhysicsScene.Simulate(Time.fixedDeltaTime);
    }

    private IEnumerator AdvanceFrame()
    {
        UpdatePhysics();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        UpdatePhysics();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        yield return new WaitForSecondsRealtime(0.1f);
    }


    [UnityTest]
    public IEnumerator TestLinkLineInteraction()
    {        
        var testClient = _clients[1];

        var rightDirect = TestUtil.FindObjectInSceneRecursive(testClient.Scene, "RightDirect", "Couldn't find RightDirect in test scene");
        var xrInteractor = TestUtil.GetComponent<CustomXRInteractor>(rightDirect);
        xrInteractor.SetPhysicsSceneOverride(testClient.PhysicsScene);

        var socketManager = SocketManager.GetDefault(xrInteractor.gameObject);        

        var linkObj = TestUtil.GetComponentInScene<LinkLineObject>(testClient.Scene, "Couldn't find LinkLineObject in test scene");
        var linkStartPos = linkObj.transform.position;
        linkObj.name = "TESTLINKOBJ";
        var linkInteractable = TestUtil.GetComponent<CustomXRInteractable>(linkObj);

        var socketID = "RightBeltSmallerSocket";
        var socketObj = TestUtil.FindObjectInSceneRecursive(testClient.Scene, socketID, $"Couldn't find link line socket {socketID}");

        socketObj.transform.position = linkStartPos + Vector3.one;

        var socket = socketObj.GetComponent<CustomXRSocket>();

        Debug.Log($"Using link line object {linkObj.name}");
        Debug.Log($"Using link socket {socketID}");

        xrInteractor.transform.position = linkObj.transform.position;
        yield return AdvanceFrame();
        yield return AdvanceFrame();

        //pickup carabiner
        Assert.That(xrInteractor.ActiveInteractable != null, "Interactor found no link line interactable");
        xrInteractor.SimulateGripPress(true);
        yield return AdvanceFrame();

        var dist = Vector3.Distance(xrInteractor.transform.position, linkObj.transform.position);
        Assert.That(dist, Is.LessThan(1), "Interactor did not move link line carabiner");

        xrInteractor.transform.position = xrInteractor.transform.position + new Vector3(0, 1, 0);
        yield return AdvanceFrame();

        xrInteractor.transform.position = socketObj.transform.position;
        yield return AdvanceFrame();

        //link to link line
        xrInteractor.SimulateGripPress(false);
        yield return new WaitForSecondsRealtime(1.0f);

        //Debug.Break();
        Assert.That(testClient.PlayerManager.CurrentPlayer.OnLinkLine, Is.True, "Test client not on link line");
        CheckLinkLineState(testClient.PlayerManager.CurrentPlayer.PlayerID, true);

        //pickup carabiner disconnecting from link line
        xrInteractor.SimulateGripPress(true);
        yield return new WaitForSecondsRealtime(1.0f);

        var socketData = socketManager.GetSocketData(socket.SocketID);
        Assert.That(socketData.IsOccupied, Is.False, "Socket did not clear #1");

        //move and drop carabiner
        xrInteractor.transform.position += new Vector3(0, 10, 0);
        Debug.Log($"TEST: Dropping carabiner at {xrInteractor.transform.position}");
        yield return AdvanceFrame();
        xrInteractor.SimulateGripPress(false);

        socketData = socketManager.GetSocketData(socket.SocketID);
        Assert.That(socketData.IsOccupied, Is.False, "Socket did not clear #2");

        //Debug.Break();
        Assert.That(testClient.PlayerManager.CurrentPlayer.OnLinkLine, Is.False, "Test client still on line");
        CheckLinkLineState(testClient.PlayerManager.CurrentPlayer.PlayerID, false);

        //wait for carabiner to return
        yield return new WaitForSecondsRealtime(5.0f);
        Debug.Log($"TEST: Link obj now at {linkObj.transform.position}");
        dist = Vector3.Distance(linkStartPos, linkObj.transform.position);
        Assert.That(dist, Is.LessThan(0.25f), "Link line did not return to start pos");

        //move back to carabiner and pick up
        xrInteractor.transform.position = linkObj.transform.position;
        yield return AdvanceFrame();
        yield return AdvanceFrame();
        Debug.Log($"TEST: moving to carabiner at {xrInteractor.transform.position}");

        socketData = socketManager.GetSocketData(socket.SocketID);
        Assert.That(socketData.IsOccupied, Is.False, "Socket did not clear #3");
        dist = Vector3.Distance(linkObj.transform.position, xrInteractor.transform.position);
        Assert.That(dist, Is.LessThan(0.25f), "interactor not near carabiner");
        xrInteractor.SimulateGripPress(true);
        yield return new WaitForSecondsRealtime(1.0f);

        Assert.That(linkInteractable.CurrentOwner, Is.EqualTo(xrInteractor), "Interactor did not grab carabiner");


        //move to socket and link to link line again
        Debug.Log($"TEST: moving to socket at {socketObj.transform.position}");
        xrInteractor.transform.position = socketObj.transform.position;
        yield return new WaitForSecondsRealtime(1.0f);

        dist = Vector3.Distance(linkObj.transform.position, socketObj.transform.position);
        Assert.That(dist, Is.LessThan(0.25f), "Carabiner did not move to link line socket");
        xrInteractor.SimulateGripPress(false);
        yield return new WaitForSecondsRealtime(1.0f);


        Assert.That(testClient.PlayerManager.CurrentPlayer.OnLinkLine, Is.True, "Test client did not re-link to link line");
        CheckLinkLineState(testClient.PlayerManager.CurrentPlayer.PlayerID, true);

        Debug.Log($"LinkLineTest: Movement starting");
        //move away from link line
        for (int i = 0; i < 100; i++)
        {
            socketObj.transform.position += new Vector3(0, 0, 0.15f);
            yield return AdvanceFrame();

            if (!testClient.PlayerManager.CurrentPlayer.OnLinkLine)
                break;
            //yield return AdvanceFrame();
        }

        Debug.Log($"LinkLineTest: Movement complete");
        yield return new WaitForSecondsRealtime(1.0f);

        Assert.That(testClient.PlayerManager.CurrentPlayer.OnLinkLine, Is.False, "Test client did not drop from link line due to distance");
        CheckLinkLineState(testClient.PlayerManager.CurrentPlayer.PlayerID, false);


        yield return new WaitForSecondsRealtime(2.0f);
        //Debug.Break();

        //yield return new WaitForSecondsRealtime(8.0f);
    }

    [UnityTest]
    public IEnumerator TestPhysicsObjectHandoff()
    {
        var client1 = _clients[1];
        var client2 = _clients[2];

        client1.GetTestClientRig(out var rig1, out var inter1);
        client2.GetTestClientRig(out var rig2, out var inter2);        

        rig1.transform.position = new Vector3(-0.5f, 0, 1);
        rig2.transform.position = new Vector3(0.5f, 0, 1);

        yield return AdvanceFrame();      
        Assert.That(Vector3.Distance(inter1.transform.position, inter2.transform.position), Is.LessThan(2.0f), "Interactors did not move to test position");

        //var blockDMobj = TestUtil.FindObjectInSceneRecursive(_dm.Scene, "CinderBlock1");
        //blockDMobj.transform.position = new Vector3(0, 0, 3);
        //yield return new WaitForSecondsRealtime(1.0f);

        var block1obj = TestUtil.FindObjectInSceneRecursive(client1.Scene, "CinderBlock1");
        var block2obj = TestUtil.FindObjectInSceneRecursive(client2.Scene, "CinderBlock1");

        var block1 = TestUtil.GetComponent<NetworkedObject>(block1obj);
        var block2 = TestUtil.GetComponent<NetworkedObject>(block2obj);

        yield return TestObjectHandoff("Client1:", block1, block2, inter1, inter2, client1.PhysicsScene, client2.PhysicsScene);

        yield return new WaitForSecondsRealtime(1.0f);
    }

    [UnityTest]
    public IEnumerator TestFirstAidBagSocket()
    {
        OverridePhysicsSceneAll();
        yield return new WaitForSecondsRealtime(3.0f);

        var client = _clients[1];
        client.NetworkManager.SendRequestWorldState();
        yield return new WaitForSecondsRealtime(3.0f);

        yield return TestMoveObjectOutOfSocket(_dm, client, "FirstAidBag_MP(Clone)");

        yield return new WaitForSecondsRealtime(3.0f);
    }

    [UnityTest]
    public IEnumerator TestFireExtinguisherSocket()
    {
        const string testObjName = "FireExtinguisher_MineRescue-1";

        OverridePhysicsSceneAll();

        var client = _clients[1];
        var client2 = _clients[2];

        yield return TestMoveObjectOutOfSocket(_dm, client, testObjName);

        var testObj = TestUtil.FindObjectInSceneRecursive(client.Scene, testObjName);
        var testObjDm = TestUtil.FindObjectInSceneRecursive(_dm.Scene, testObjName);
        var testObjObserver = TestUtil.FindObjectInSceneRecursive(client2.Scene, testObjName);

        yield return new WaitForSecondsRealtime(3.0f);

        Debug.Log($"TestFireExtinguisherSocket: DM xform parent: {testObjDm.transform.parent}");
        Debug.Log($"TestFireExtinguisherSocket: Client xform parent: {testObj.transform.parent}");
        Debug.Log($"TestFireExtinguisherSocket: Observer xform parent: {testObjObserver.transform.parent}");

        Assert.IsNotNull(testObjDm.transform.parent, "dm object transform has no parent");
        Assert.IsNotNull(testObj.transform.parent, "client object transform has no parent");
        Assert.IsNotNull(testObjObserver.transform.parent, "observer object transform has no parent");

        Assert.That(testObjDm.transform.parent.name, Is.EqualTo(testObj.transform.parent.name), "Client object parent didn't match DM");
        Assert.That(testObjDm.transform.parent.name, Is.EqualTo(testObjObserver.transform.parent.name), "Observer object parent didn't match DM");
    }

    [UnityTest]
    public IEnumerator TestMoveBothFireExtinguishers()
    {
        const string obj1Name = "FireExtinguisher_MineRescue-1";
        const string obj2Name = "FireExtinguisher_MineRescue-2";

        OverridePhysicsSceneAll();

        yield return new WaitForSecondsRealtime(3.0f);


        var client = _clients[1];
        client.NetworkManager.SendRequestWorldState();
        yield return new WaitForSecondsRealtime(3.0f);

        var testObj1 = TestUtil.FindObjectInSceneRecursive(client.Scene, obj1Name);
        var testObj2 = TestUtil.FindObjectInSceneRecursive(client.Scene, obj2Name);

        var returnPos1 = testObj1.transform.position + new Vector3(0, 0.25f, 0);
        var returnPos2 = testObj2.transform.position + new Vector3(0, 0.25f, 0);

        yield return TestMoveObjectOutOfSocket(_dm, client, obj1Name, false, false);
        yield return new WaitForSecondsRealtime(2.0f);
        yield return TestMoveObjectOutOfSocket(_dm, client, obj2Name, false, false);
        yield return new WaitForSecondsRealtime(2.0f);

        yield return TestReturnToSocket(_dm, client, obj1Name, returnPos1);
        yield return new WaitForSecondsRealtime(2.0f);
        yield return TestReturnToSocket(_dm, client, obj2Name, returnPos2);
        yield return new WaitForSecondsRealtime(2.0f);
    }


    [UnityTest]
    public IEnumerator TestMultipleAvailableSockets()
    {

        OverridePhysicsSceneAll();

        const string testObjName = "FireExtinguisher_MineRescue-2";
        var client = _clients[1];
        var client2 = _clients[2];

        var testObj = TestUtil.FindObjectInSceneRecursive(client.Scene, testObjName);
        Assert.IsNotNull(testObj, $"Couldn't find {testObjName} in client scene");

        var testInteract = testObj.GetComponent<CustomXRInteractable>();
        Assert.IsNotNull(testInteract, "Couldn't get test object CustomXRInteractable component");

        var dmTestObj = TestUtil.FindObjectInSceneRecursive(_dm.Scene, testObjName);
        Assert.IsNotNull(dmTestObj, $"Couldn't find {testObjName} in dm scene");

        var dmTestInteract = dmTestObj.GetComponent<CustomXRInteractable>();
        Assert.IsNotNull(dmTestInteract, "Couldn't get test object CustomXRInteractable component");

        var testObjObserver = TestUtil.FindObjectInSceneRecursive(client2.Scene, testObjName);
        Assert.IsNotNull(testObjObserver, $"Couldn't find {testObjName} in client2 scene");

        var testInteractObserver = testObjObserver.GetComponent<CustomXRInteractable>();
        Assert.IsNotNull(testInteractObserver, "Couldn't get test object CustomXRInteractable component");

        var poiManager = POIManager.GetDefault(dmTestObj);
        var teleport = TeleportManager.GetDefault(dmTestObj);

        var ts = poiManager.GetPOI("TS2");
        Assert.IsNotNull(ts, "Couldn't find TS2 teamstop");
        teleport.TeleportToPOI(ts);

        yield return new WaitForSecondsRealtime(6.0f);

        Assert.That(teleport.ActivePOIName, Is.EqualTo("TS2"));

        testInteract.OwnerChanged += (newOwner) =>
        {
            if (newOwner == null)
                return;

            var newOwnerComp = (Component)newOwner;
            Debug.Log($"TestMultipleAvailableSockets: Test object owner changed to {newOwnerComp.name}");
        };

        testInteractObserver.OwnerChanged += (newOwner) =>
        {
            if (newOwner == null)
                return;

            var newOwnerComp = (Component)newOwner;
            Debug.Log($"TestMultipleAvailableSockets: Observer test object owner changed to {newOwnerComp.name}");
        };

        dmTestInteract.OwnerChanged += (newOwner) =>
        {
            if (newOwner == null)
                return;

            var newOwnerComp = (Component)newOwner;
            Debug.Log($"TestMultipleAvailableSockets: DM test object owner changed to {newOwnerComp.name}");
        };

        yield return TestMoveObjectOutOfSocket(_dm, client, "FireExtinguisher_MineRescue-1", false, false);

        yield return TestMoveObjectOutOfSocket(_dm, client, "FireExtinguisher_MineRescue-2", false, true);

        yield return new WaitForSecondsRealtime(3.0f);

        Debug.Log($"TestMultipleAvailableSockets: DM last owner {dmTestInteract.CurrentOwner} xform parent: {dmTestObj.transform.parent}");
        Debug.Log($"TestMultipleAvailableSockets: Client last owner {testInteract.CurrentOwner} xform parent: {testObj.transform.parent}");
        Debug.Log($"TestMultipleAvailableSockets: Observer client last owner {testInteractObserver.CurrentOwner} xform parent: {testObjObserver.transform.parent}");

        var dist = Vector3.Distance(dmTestObj.transform.position, testObj.transform.position);
        var distObserver = Vector3.Distance(dmTestObj.transform.position, testObjObserver.transform.position);

        Debug.Log($"TestMultipleAvailableSockets: DM Position {dmTestObj.transform.position} \n"+
            $"Client position: {testObj.transform.position} distance: {dist:F2} \n" + 
            $"Observer Position {testObjObserver.transform.position} distance: {distObserver:F2}");


        Assert.IsNotNull(dmTestObj.transform.parent, "dm object transform has no parent");
        Assert.IsNotNull(testObj.transform.parent, "client object transform has no parent");
        Assert.IsNotNull(testObjObserver.transform.parent, "observer object transform has no parent");

        Assert.That(dmTestObj.transform.parent.name, Is.EqualTo(testObj.transform.parent.name), "Client object parent didn't match DM");
        Assert.That(dmTestObj.transform.parent.name, Is.EqualTo(testObjObserver.transform.parent.name), "Observer object parent didn't match DM");
    }

    [UnityTest]
    public IEnumerator TestFireExtinguisherActivation()
    {
        OverridePhysicsSceneAll();

        var client = _clients[1];
        yield return TestMoveObjectOutOfSocket(_dm, client, "FireExtinguisher_MineRescue-1", activateBeforeDrop: true);

        var testObj = TestUtil.FindObjectInSceneRecursive(client.Scene, "FireExtinguisher_MineRescue-1");
        if (!testObj.TryGetComponent<FireExtinguisherInteraction>(out var fireExtinguisher))
        {
            Assert.Fail("Couldn't find FireExtinguisherInteraction component");
            yield break;
        }

        Assert.That(fireExtinguisher.IsActivated, Is.False, "Fire extinguisher active after drop");

        yield return new WaitForSecondsRealtime(3.0f);
    }

    private IEnumerator TestMoveObjectOutOfSocket(InstanceData dm, InstanceData client, string objName, bool activateBeforeDrop = false, bool returnToSocket = true)
    {
        client.GetTestClientRig(out var rig1, out var inter1);

        var testObj = TestUtil.FindObjectInSceneRecursive(client.Scene, objName);
        Assert.IsNotNull(testObj, $"Couldn't find {objName} in client scene");

        var dmObj = TestUtil.FindObjectInSceneRecursive(dm.Scene, objName);
        Assert.IsNotNull(dmObj, $"Couldn't find {objName} in dm scene");

        var socketManager = SocketManager.GetDefault(testObj);
        var interactable = testObj.GetComponent<CustomXRInteractable>();
        var dmInteractable = dmObj.GetComponent<CustomXRInteractable>();

        Assert.IsNotNull(socketManager, $"Couldn't find socket manager for {objName}");
        Assert.IsNotNull(interactable, $"Couldn't find interactable on {objName}");
        Assert.IsNotNull(dmInteractable, $"Couldn't find interactable on {objName} in dm scene");

        Vector3 returnPos = testObj.transform.position + new Vector3(0, 0.25f, 0);


        client.GetTestClientRig(out var rig, out var interactor);

        yield return MoveObject(interactor.transform, testObj.transform.position);

        bool isSocketed = dmInteractable.CurrentOwner as CustomXRSocket != null;
        Assert.That(isSocketed, Is.True, $"{objName} not initially socketed, owner: {interactable.CurrentOwner}");

        //pickup and move target object
        yield return PickupObject(interactor, interactable);
        yield return MoveObject(interactor.transform, interactor.transform.position + new Vector3(0,0,5));

        if (activateBeforeDrop)
        {
            interactor.SimulateTriggerPress(true);
            yield return AdvanceFrame();
            yield return new WaitForSecondsRealtime(1.0f);
        }

        Assert.That(Vector3.Distance(interactor.transform.position, testObj.transform.position), Is.LessThan(0.2f));

        //drop target object
        interactor.SimulateGripPress(false);
        yield return AdvanceFrame();

        if (activateBeforeDrop)
            interactor.SimulateTriggerPress(false);

        isSocketed = dmInteractable.CurrentOwner as CustomXRSocket != null;
        Assert.That(isSocketed, Is.False, $"{objName} still socketed after move");
        Assert.That(Vector3.Distance(interactor.transform.position, testObj.transform.position), Is.LessThan(2.0f));

        yield return new WaitForSecondsRealtime(1.0f);

        if (!returnToSocket)
            yield break;

        //pickup target object again
        yield return PickupObject(interactor, interactable);

        if (activateBeforeDrop)
        {
            interactor.SimulateTriggerPress(true);
            yield return AdvanceFrame();
        }

        //move back to retrn position
        yield return MoveObject(interactor.transform, returnPos);

        //drop object
        interactor.SimulateGripPress(false);

        yield return new WaitForSecondsRealtime(3.0f);

        if (activateBeforeDrop)
        {
            interactor.SimulateTriggerPress(false);
        }

        isSocketed = dmInteractable.CurrentOwner as CustomXRSocket != null;
        Assert.That(isSocketed, Is.True, $"{objName} did not re-socket");

        foreach (var otherClient in _clients)
        {
            var obj = TestUtil.FindObjectInSceneRecursive(otherClient.Scene, objName);
            Assert.IsNotNull(testObj, $"Couldn't find {objName} in other client scene");
            Assert.IsNotNull(testObj.transform.parent, $"{objName} has no transform parent in {otherClient.Scene}");
        }
    }

    private IEnumerator TestReturnToSocket(InstanceData dm, InstanceData client, string objName, Vector3 returnPos)
    {
        var testObj = TestUtil.FindObjectInSceneRecursive(client.Scene, objName);
        Assert.IsNotNull(testObj, $"Couldn't find {objName} in client scene");

        var dmObj = TestUtil.FindObjectInSceneRecursive(dm.Scene, objName);
        Assert.IsNotNull(dmObj, $"Couldn't find {objName} in dm scene");

        client.GetTestClientRig(out var rig, out var interactor);

        var socketManager = SocketManager.GetDefault(testObj);
        var interactable = testObj.GetComponent<CustomXRInteractable>();
        var dmInteractable = dmObj.GetComponent<CustomXRInteractable>();

        //pickup target object again
        yield return PickupObject(interactor, interactable);

        //move back to retrn position
        yield return MoveObject(interactor.transform, returnPos);

        //drop object
        interactor.SimulateGripPress(false);

        yield return new WaitForSecondsRealtime(3.0f);

        bool isSocketed = dmInteractable.CurrentOwner as CustomXRSocket != null;
        Assert.That(isSocketed, Is.True, $"{objName} did not re-socket");

        foreach (var otherClient in _clients)
        {
            var obj = TestUtil.FindObjectInSceneRecursive(otherClient.Scene, objName);
            Assert.IsNotNull(testObj, $"Couldn't find {objName} in other client scene");
            Assert.IsNotNull(testObj.transform.parent, $"{objName} has no transform parent in {otherClient.Scene}");
        }
    }

    private IEnumerator MoveObject(Transform obj, Vector3 destPos)
    {
        obj.position = destPos;
        yield return AdvanceFrame();

        var dist = Vector3.Distance(obj.position, destPos);
        Assert.That(dist, Is.LessThan(0.1f), $"Object {obj.name} did not move to position {destPos} (actual: {obj.position})");
    }

    private IEnumerator PickupObject(CustomXRInteractor interactor, CustomXRInteractable interactable)
    {
        yield return MoveObject(interactor.transform, interactable.transform.position);

        interactor.SimulateGripPress(true);
        yield return AdvanceFrame();
        //yield return new WaitForSecondsRealtime(0.5f);
        yield return AdvanceFrame();

        foreach (var inter in interactable.ActiveInteractors)
        {
            if (inter == interactor)
                yield break;
        }
        yield return new WaitForSecondsRealtime(50);
        Assert.Fail($"Couldn't pickup object {interactable.name} with {interactor.name}");
    }

    [UnityTest]
    public IEnumerator TestClientObjectHandoff()
    {
        var client1 = _clients[1];
        var client2 = _clients[2];

        client1.GetTestClientRig(out var rig1, out var inter1);
        client2.GetTestClientRig(out var rig2, out var inter2);

        rig1.transform.position = new Vector3(-0.5f, 0, 1);
        rig2.transform.position = new Vector3(0.5f, 0, 1);

        yield return AdvanceFrame();
        Assert.That(Vector3.Distance(inter1.transform.position, inter2.transform.position), Is.LessThan(2.0f), "Interactors did not move to test position");

        //var objDM = TestUtil.FindObjectInSceneRecursive(_dm.Scene, "FireExtinguisher_MineRescue-1");
        //objDM.transform.position = new Vector3(0, 1, 3);
        //yield return new WaitForSecondsRealtime(3.0f);

        var obj1 = TestUtil.FindObjectInSceneRecursive(client1.Scene, "FireExtinguisher_MineRescue-1");
        var obj2 = TestUtil.FindObjectInSceneRecursive(client2.Scene, "FireExtinguisher_MineRescue-1");

        var net1 = TestUtil.GetComponent<NetworkedObject>(obj1);
        var net2 = TestUtil.GetComponent<NetworkedObject>(obj2);

        yield return TestObjectHandoff("Client1:", net1, net2, inter1, inter2, client1.PhysicsScene, client2.PhysicsScene);

        yield return new WaitForSecondsRealtime(1.0f);
    }

    private IEnumerator TestObjectHandoff(string logHeader, NetworkedObject obj1, NetworkedObject obj2, CustomXRInteractor inter1, CustomXRInteractor inter2, PhysicsScene phys1, PhysicsScene phys2)
    {
        Physics.SyncTransforms();
        yield return AdvanceFrame();

        var rb1 = obj1.GetComponent<Rigidbody>();
        var rb2 = obj2.GetComponent<Rigidbody>();

        Vector3 pos1 = obj1.transform.position;
        //Vector3 pos1 = new Vector3(0, 1, 1);
        Vector3 pos2 = new Vector3(0, 1.5f, 0);
        Vector3 pos3 = pos2 + new Vector3(0, 0, 1.5f);

        //rb1.position = obj1.transform.position = pos1;
        inter1.transform.position = pos1;
        inter2.transform.position = pos1;
        //yield return new WaitForSecondsRealtime(2.0f);
        Debug.Log($"{logHeader} pos1: {pos1} pos2: {pos2}");

        //Debug.Break();
        //yield return null;

        yield return AdvanceFrame();

        //int numInteractables = inter1.CheckForInteractables();
        //Assert.That(numInteractables, Is.EqualTo(1), $"{logHeader} CheckForInteractables found wrong number of objects");


        var startFrame = Time.frameCount;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        //yield return new WaitUntil(() => { return inter1.ActiveInteractable != null; });
        while (inter1.ActiveInteractable == null)
        {
            //Debug.Break();
            //yield return null;
            //Physics.SyncTransforms();
            //phys1.Simulate(Time.fixedDeltaTime * 10.0f);
            //inter1.CheckForInteractables();

            //Debug.Break();
            //yield return null;
            Debug.Log($"{logHeader} inter:{inter1.transform.position} obj1:{obj1.transform.position} rb1:{rb1.position}");
            yield return AdvanceFrame();
        }    
        sw.Stop();

        Debug.Log($"{logHeader} interactor took {sw.ElapsedMilliseconds}ms ({Time.frameCount - startFrame} frames) to detect object");


        //initial pickup by inter1
        inter1.SimulateGripPress(true);
        yield return new WaitForSecondsRealtime(1.0f);
        inter1.transform.position = pos2;
        inter2.transform.position = pos2;
        yield return new WaitForSecondsRealtime(1.0f); 
        Assert.That(Vector3.Distance(obj1.transform.position, pos2), Is.LessThan(0.1f), $"{logHeader} Object did not move to first test position");

        //inter2 takes objects
        inter2.SimulateGripPress(true);
        yield return new WaitForSecondsRealtime(1.0f);
        inter2.transform.position = pos3;
        yield return new WaitForSecondsRealtime(2.0f);

        TestUtil.AssertObjectNearPosition(obj1.transform, pos3, $"{logHeader} obj1 did not move to pos3");
        TestUtil.AssertObjectNearPosition(obj2.transform, pos3, $"{logHeader} obj2 ({obj1.transform.position} did not move to pos3 ({pos3})");

        //inter1 releases grip
        inter1.SimulateGripPress(false);
        yield return new WaitForSecondsRealtime(2.0f); 

        TestUtil.AssertObjectNearPosition(obj1.transform, pos3, $"{logHeader} obj1 not at pos3 after handoff");
        TestUtil.AssertObjectNearPosition(obj2.transform, pos3, $"{logHeader} obj2 not at pos3 after handoff");

        //drop object
        inter2.SimulateGripPress(false);
        yield return new WaitForSecondsRealtime(2.0f);
    }


    private void CheckLinkLineState(int playerID, bool shouldBeLinked)
    {
        CheckLinkLineState(_dm, playerID, shouldBeLinked);

        foreach (var client in _clients)
        {
            CheckLinkLineState(client, playerID, shouldBeLinked);
        }
    }

    private void CheckLinkLineState(InstanceData data, int playerID, bool shouldBeLinked)
    {
        foreach (var kvp in data.PlayerManager.PlayerList)
        {
            if (kvp.Value.PlayerID == playerID)
                Assert.That(kvp.Value.OnLinkLine, Is.EqualTo(shouldBeLinked), $"Client {data.NetworkManager.ClientID} has incorrect link line status for playerID {playerID}");
            else
                Assert.That(kvp.Value.OnLinkLine, Is.False, $"Client {data.NetworkManager.ClientID} incorrectly shows playerID {playerID} on link line");

        }
    }

    [UnityTest]
    public IEnumerator TestMapBoardSync()
    {
        var client1 = _clients[1];
        var client2 = _clients[2];

        client2.NetworkManager.IsStatLogEnabled = true;

        var netObjManager1 = client1.NetworkManager.NetworkedObjectManager;
        var netObjManager2 = client2.NetworkManager.NetworkedObjectManager;

        var spawnObj = netObjManager1.SpawnObject("MineRescue/Map_MP", Guid.NewGuid(), Vector3.one, Quaternion.identity, false);
        while (!spawnObj.IsCompleted)
            yield return null;

        yield return new WaitForSecondsRealtime(1.5f);

        var mapObj = spawnObj.Result;
        var symbolManager = mapObj.GetComponentInChildren<MineMapSymbolManager>();
        Assert.That(symbolManager, Is.Not.Null);

        var mapObj2 = netObjManager2.GetNetworkObject(mapObj.uniqueID);
        var symbolManager2 = mapObj2.GetComponentInChildren<MineMapSymbolManager>();
        Assert.That(symbolManager2, Is.Not.Null);

        yield return InstantiateTestSymbols(symbolManager, 50);
        yield return InstantiateTestSymbols(symbolManager, 50);

        yield return InstantiateTestSymbols(symbolManager, 50);
        yield return InstantiateTestSymbols(symbolManager, 50);

        yield return InstantiateTestSymbols(symbolManager, 50);
        yield return InstantiateTestSymbols(symbolManager, 25);

        yield return new WaitForSecondsRealtime(1.5f);

        //foreach (var stats in client2.NetworkManager.PacketRecvStats)
        //{
        //    Debug.Log($"PacketReceivedStats: {stats.Key}: {stats.Value.PacketCount}:{stats.Value.MaxPacketSize}");
        //}

        if (client2.NetworkManager.PacketRecvStats.TryGetValue("SendSymbolManagerState", out var stats))
        {
            Debug.Log($"Test: PacketReceivedStats: {stats.PacketType}: {stats.PacketCount}:{stats.MaxPacketSize}");
        }

        Debug.Log($"Test: SymbolManager2 Count: {symbolManager2.ActiveSymbols.Count}");
        Assert.That(symbolManager2.ActiveSymbols.Count, Is.EqualTo(symbolManager.ActiveSymbols.Count));

    }

    private IEnumerator InstantiateTestSymbols(MineMapSymbolManager symbolManager, int count)
    {
        for (int i = 0; i < count; i++)
            yield return InstantiateTestSymbol(symbolManager);

        yield return new WaitForSecondsRealtime(1.5f);
    }

    private IEnumerator InstantiateTestSymbol(MineMapSymbolManager symbolManager)
    {
        var symbolName = "MineMapSymbols/TeleportPoint";
        var symbol = symbolManager.InstantiateSymbolAsync(symbolName, Vector3.zero, Quaternion.identity);

        while (!symbol.IsCompleted)
        {
            yield return null;
        }

        //Debug.Log($"Test: Symbol created, count: {symbolManager.ActiveSymbols.Count}");
    }
}
