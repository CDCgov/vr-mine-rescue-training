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

public class NetworkManagerPlayModeTests
{
    private const string TestPrefabAddress = "Cinderblock_MP";
    public const string TestSceneName = "NetworkTestScene";
    private const int NumClients = 3;

    private GameObject _clientPrefab;
    private GameObject _serverPrefab;
    //private GameObject _lateClientPrefab;

    private NetworkManager _netManagerServer;
    private NetworkManager[] _netManagerClients;

    private AudioListener _audioListener;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        if (_audioListener == null)
        {
            var go = new GameObject("AudioListener");
            _audioListener = go.AddComponent<AudioListener>();
        }

        //yield return TestUtil.LoadScene(TestSceneName, LoadSceneMode.Additive);

        yield return new WaitForSecondsRealtime(0.5f); 
        _clientPrefab = Resources.Load<GameObject>("UnitTesting/NetworkManagerClient");
        _serverPrefab = Resources.Load<GameObject>("UnitTesting/NetworkManagerTestServer");
        //_lateClientPrefab = Resources.Load<GameObject>("UnitTesting/NetworkManagerLateClient");

        /*
        var serverObj = new GameObject("NetworkManagerTestServer");
        var clientObj = new GameObject("NetworkManager");

        _netManagerServer = serverObj.AddComponent<NetworkManager>();
        _netManagerClient = clientObj.AddComponent<NetworkManager>(); 

        _netManagerServer.OverrideConfiguration = true;
        _netManagerServer.StartAsServer = true;

        _netManagerClient.OverrideConfiguration = true;
        _netManagerClient.StartAsServer = false;
        _netManagerClient.DefaultMultiplayerServer = "127.0.0.1";
         */

        var serverObj = GameObject.Instantiate<GameObject>(_serverPrefab);
        serverObj.name = "NetworkManager";       
        _netManagerServer = serverObj.GetComponent<NetworkManager>();

        yield return new WaitForSecondsRealtime(1.0f);

        _netManagerClients = new NetworkManager[NumClients];
        for (int i = 0; i < NumClients; i++)
        {
            _netManagerClients[i] = CreateClient(i);
            yield return new WaitForSecondsRealtime(0.25f);
        }


        Debug.Log("Setup Complete");
    }

    private NetworkManager CreateClient(int index)
    {
        var clientObj = GameObject.Instantiate<GameObject>(_clientPrefab);
        clientObj.name = $"NetworkClient{index}";
        return clientObj.GetComponent<NetworkManager>();
    }

    [UnityTearDown]
    public IEnumerator Teardown()
    {
        foreach (var client in _netManagerClients)
        {
            if (client != null && client.gameObject != null)
            {
                client.Disconnect();
                GameObject.Destroy(client.gameObject);
            }
        }
        _netManagerClients = new NetworkManager[NumClients];

        //if (_netManagerClient != null)
        //    GameObject.Destroy(_netManagerClient.gameObject);
        if (_netManagerServer != null)
        {
            _netManagerServer.Disconnect();
            GameObject.Destroy(_netManagerServer.gameObject);
        }
        _netManagerServer = null;

        yield return TestUtil.UnloadScene(TestSceneName);

        Debug.Log("Teardown Complete");
        yield return new WaitForSecondsRealtime(0.1f);
    }

    [Test]
    public void SimpleTest()
    {
        //Assert.That(_netManagerClient, Is.Not.Null);
        foreach (var client in _netManagerClients)
            Assert.That(client, Is.Not.Null);

        Assert.That(_netManagerServer, Is.Not.Null);
    }

    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator BasicConnectionTest()
    {
        yield return new WaitForSeconds(3.0f);

        //Assert.That(_netManagerClient.IsConnected, Is.True);
        foreach (var client in _netManagerClients)
            Assert.That(client.IsConnected, Is.True);
    }

    private IEnumerator RunAsyncTest(Task task)
    {
        while (!task.IsCompleted && !task.IsFaulted)
            yield return null;

        if (task.IsFaulted)
            throw (task.Exception);
    }

    [UnityTest]
    public IEnumerator SpawnTest2() { return RunAsyncTest(SpawnTestInternal()); }

    [UnityTest]
    public IEnumerator SpawnTest()
    {
        var task = SpawnTestInternal();
        while (!task.IsCompleted && !task.IsFaulted)
            yield return null;

        if (task.IsFaulted)
            throw (task.Exception);
    }

    public async Task SpawnTestInternal()
    {
        Vector3 startPos = new Vector3(2, 3, 4);
        var objid = Guid.NewGuid();

        await Task.Delay(500);

        var netObj = await _netManagerClients[0].NetworkedObjectManager.SpawnObject(TestPrefabAddress,
            objid, startPos, Quaternion.identity, true);

        await Task.Delay(500);

        Assert.That(netObj, Is.Not.Null);
        Assert.That(netObj.gameObject, Is.Not.Null);
        Debug.Log($"Spawned {netObj.gameObject.name}");

        _netManagerClients[0].NetworkedObjectManager.DestroyObject(objid);

        await Task.Delay(1000);
        Assert.That(netObj == null);
        //yield return new WaitForSeconds(5.0f);
    }

    [UnityTest]
    public IEnumerator LateJoinTest() { return RunAsyncTest(LateJoinTestInternal()); }

    public async Task LateJoinTestInternal()
    {
        Vector3 startPos = new Vector3(2, 3, 4);
        var objid = new Guid(11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11);

        var netObj = await _netManagerClients[0].NetworkedObjectManager.SpawnObject(TestPrefabAddress,
            objid, startPos, Quaternion.identity, true);

        //create a fake "scene object"
        var prefab = await Addressables.LoadAssetAsync<GameObject>(TestPrefabAddress).Task;
        var sceneObj = GameObject.Instantiate<GameObject>(prefab);
        sceneObj.name = "TestSceneObject";        
        var sceneNetObj = sceneObj.GetComponent<NetworkedObject>();
        sceneNetObj.SceneID = -1;
        sceneNetObj.NetworkedObjectManager = _netManagerServer.NetworkedObjectManager;
        sceneNetObj.NetManager = _netManagerServer;
        sceneNetObj.OwnerClientID = (short)_netManagerServer.ClientID;

        await Task.Delay(300);
        sceneObj.SetActive(true);

        Assert.That(sceneObj != null, "Couldn't create scene object");

        var lateClientObj = GameObject.Instantiate<GameObject>(_clientPrefab);
        var lateManager = lateClientObj.GetComponent<NetworkManager>();
        var lateNetObjManager = lateClientObj.GetComponent<NetworkedObjectManager>();

        lateClientObj.name = "LateJoinClient";

        //bool gotObjectList = false;
        //int numObjectsReceived = 0;

        //lateManager.RegisterHandler(VRNPacketType.SendNetworkedObjectList, 
        //    (header, reader, clientID) => { 
        //        gotObjectList = true;
        //        //var msg = VRNNetworkedObjectList.Parser.ParseDelimitedFrom(recvStream);
        //        var msg = new VRNNetworkedObjectList();
        //        reader.ReadMessage(msg);

        //        if (msg != null && msg.ObjectList != null)
        //            numObjectsReceived = msg.ObjectList.Count;
        //    });

        //wait to be connected
        await Task.Delay(300);
        Assert.IsTrue(lateManager.ClientConnected, "Client did not connect");

        //request world state
        lateManager.SendRequestWorldState();
        await Task.Delay(600);

        //did we receive an object list
        //Assert.IsTrue(gotObjectList, "Did not receive object list");
        //Debug.Log($"Received object list with {numObjectsReceived} objects");

        //check that the spawned object was received on the late client
        var objdata = lateManager.NetworkedObjectManager.GetObjectData(objid);
        
        Assert.IsNotNull(objdata, $"Late client did not receive spawned object, count: {lateManager.NetworkedObjectManager.NetworkedObjectCount}");
        Assert.That(objdata.AssociatedObj != null, "Late join client did not create spawned object");

        Debug.Log($"Late client created object: {objdata.AssociatedObj.name}");

        //verify scene object is not destroyed
        Assert.That(sceneObj != null, "Scene object destroyed");

        //cleanup
        GameObject.Destroy(sceneObj);
        await Task.Delay(100);
    }

    [UnityTest]
    public IEnumerator TestLinkLine()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        foreach (var client in _netManagerClients)
        {
            var playerManager = client.GetComponent<PlayerManager>();
            Debug.Log($"{client.name}: {playerManager.CurrentPlayer.PlayerID}");
        }

        var player = _netManagerClients[0].GetComponent<PlayerManager>();
        player.CurrentPlayer.OnLinkLine = true;

        Assert.That(player.CurrentPlayer.OnLinkLine, "couldn't set player on link line!");

        yield return new WaitForSecondsRealtime(0.5f);

        Assert.That(player.CurrentPlayer.OnLinkLine, "player not on link line!");

        for (int i = 1; i < NumClients; i++)
        {
            var client = _netManagerClients[i];
            var manager = client.GetComponent<PlayerManager>();
            Assert.That(manager.CurrentPlayer.OnLinkLine == false, $"Player {client.name} incorrectly on link line");

            var remotePlayer = manager.GetPlayer(player.CurrentPlayer.PlayerID);
            Assert.That(remotePlayer.OnLinkLine == true, $"Remote client {client.name} doesn't show player on link line");
        }


        player.CurrentPlayer.OnLinkLine = false;

        yield return new WaitForSecondsRealtime(0.5f);

        Assert.That(player.CurrentPlayer.OnLinkLine == false, "player did not leave link line");

        for (int i = 1; i < NumClients; i++)
        {
            var client = _netManagerClients[i];
            var manager = client.GetComponent<PlayerManager>();
            Assert.That(manager.CurrentPlayer.OnLinkLine == false, $"Player {client.name} incorrectly on link line (#2)");

            var remotePlayer = manager.GetPlayer(player.CurrentPlayer.PlayerID);
            Assert.That(remotePlayer.OnLinkLine == false, $"Remote client {client.name} still shows player on link line");
        }

    }

    [UnityTest]
    public IEnumerator TestHeldStatus()
    {
        yield return RunAsyncTest(TestHeldStatusInternal());

        yield return new WaitForSecondsRealtime(1.0f);
    }

    public async Task TestHeldStatusInternal()
    {
        Vector3 startPos = new Vector3(2, 3, 4);
        var objid = new Guid(11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11);        

        var netObj = await _netManagerClients[0].NetworkedObjectManager.SpawnObject(TestPrefabAddress,
            objid, startPos, Quaternion.identity, true);

        int heldByClient = _netManagerClients[0].ClientID;
        _netManagerClients[0].NetworkedObjectManager.SetHeldStatus(objid, true, heldByClient);

        await Task.Delay(500);

        foreach (var client in _netManagerClients)
        {
            var objManager = client.NetworkedObjectManager;
            Assert.That(objManager.NetworkedObjectCount == 1, $"Wrong networked oobject count on {client.name}: {objManager.NetworkedObjectCount}");
            var objData = objManager.GetObjectData(objid);
            Assert.That(objData != null, $"no object data on {client.name}");
            Assert.That(objData.HeldState.HeldByClientID == heldByClient, $"object held by wrong client on {client.name}: {objData.HeldState.HeldByClientID}");
        }

        heldByClient = _netManagerClients[2].ClientID;
        _netManagerClients[2].NetworkedObjectManager.SetHeldStatus(objid, true, heldByClient);

        await Task.Delay(500);

        foreach (var client in _netManagerClients)
        {
            var objManager = client.NetworkedObjectManager;
            Assert.That(objManager.NetworkedObjectCount == 1, $"#2 Wrong networked oobject count on {client.name}: {objManager.NetworkedObjectCount}");
            var objData = objManager.GetObjectData(objid);
            Assert.That(objData != null, $"#2 no object data on {client.name}");
            Assert.That(objData.HeldState.HeldByClientID == heldByClient, $"#2 object held by wrong client on {client.name}: {objData.HeldState.HeldByClientID}");
        }

    }

    [UnityTest]
    public IEnumerator TestMapSync()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        var client = _netManagerClients[0];
        var client2 = _netManagerClients[1];

        var spawnObj = client.NetworkedObjectManager.SpawnObject("MineRescue/Map_MP", Guid.NewGuid(), Vector3.one, Quaternion.identity, false);
        while (!spawnObj.IsCompleted)
            yield return null;

        yield return new WaitForSecondsRealtime(1.5f);

        var mapObj = spawnObj.Result;
        var symbolManager = mapObj.GetComponentInChildren<MineMapSymbolManager>();
        Assert.That(symbolManager, Is.Not.Null);

        var mapObj2 = client2.NetworkedObjectManager.GetNetworkObject(mapObj.uniqueID);
        var symbolManager2 = mapObj2.GetComponentInChildren<MineMapSymbolManager>();
        Assert.That(symbolManager2, Is.Not.Null);


        for (int i = 0; i < 50; i++)
            yield return InstantiateTestSymbol(symbolManager);

        yield return new WaitForSecondsRealtime(1.5f);

        for (int i = 0; i < 50; i++)
            yield return InstantiateTestSymbol(symbolManager);

        yield return new WaitForSecondsRealtime(1.5f);

        for (int i = 0; i < 50; i++)
            yield return InstantiateTestSymbol(symbolManager);

        yield return new WaitForSecondsRealtime(1.5f);

        for (int i = 0; i < 50; i++)
            yield return InstantiateTestSymbol(symbolManager);

        yield return new WaitForSecondsRealtime(1.5f);

        for (int i = 0; i < 50; i++)
            yield return InstantiateTestSymbol(symbolManager);

        yield return new WaitForSecondsRealtime(1.5f);

        for (int i = 0; i < 25; i++)
            yield return InstantiateTestSymbol(symbolManager);

        yield return new WaitForSecondsRealtime(1.5f);

        yield return new WaitForSecondsRealtime(1.5f);
        Debug.Log($"Test: SymbolManager2 Count: {symbolManager2.ActiveSymbols.Count}");

        yield return new WaitForSecondsRealtime(5f);

    }

    private IEnumerator InstantiateTestSymbol(MineMapSymbolManager symbolManager)
    {
        var symbolName = "MineMapSymbols/TeleportPoint";
        var symbol = symbolManager.InstantiateSymbolAsync(symbolName, Vector3.zero, Quaternion.identity);

        while (!symbol.IsCompleted)
        {
            yield return null;
        }

        Debug.Log($"Test: Symbol created, count: {symbolManager.ActiveSymbols.Count}");
    }

    [UnityTest]
    public IEnumerator ZDelay()
    {
        yield return new WaitForSeconds(2.0f);
    }

}
