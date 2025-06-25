using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Networking.Transport;
using Google.Protobuf;

public class RelayTests
{

    [UnityTearDown]
    private IEnumerator UnityTearDown()
    {
        Debug.Log("UnityTearDown");

        var gameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (var obj in gameObjects)
        {
            GameObject.Destroy(obj);
        }

        yield return new WaitForSecondsRealtime(1.0f);
    }

    private void StartPipelineTestServer()
    {
        var go = new GameObject("RelayServer");
        go.AddComponent<VRMinePipelineTestRelay>();
    }

    private VRMineRelay StartRelayServer(bool simulateLatency = false)
    {

        Application.targetFrameRate = 200;

        var go = new GameObject("RelayServer");
        var relay = go.AddComponent<VRMineRelay>();
        relay.SimulateLatency = simulateLatency;

        return relay;
    }

    [UnityTest]
    public IEnumerator TestConnect()
    {

        StartPipelineTestServer();
        yield return null;

        var go = new GameObject("TestClient");
        var testClient = go.AddComponent<VRMineTestClient1>();
        yield return null;


        testClient.Connect("localhost", VRMineRelay.RelayPort);

        yield return new WaitForSeconds(0.3f);

        if (!testClient.IsConnected)
            throw new System.Exception("Failed to connect to server");

        testClient.Disconnect();
        yield return null;
        if (testClient.IsConnected)
            throw new System.Exception("Failed to disconnect from server");


    }


    [UnityTest]
    public IEnumerator TestPipelines()
    {
        StartPipelineTestServer();
        yield return null;

        var go = new GameObject("TestClient");
        var testClient = go.AddComponent<VRMineTestClient1>();
        yield return null;

        testClient.Connect("localhost", VRMineRelay.RelayPort);

        yield return new WaitForSeconds(0.3f);

        if (!testClient.IsConnected)
            throw new System.Exception("Failed to connect to server");

        testClient.SendTestStringReliable("ReliableTest");
        testClient.SendTestStringUnreliable("UnreliableTest");
        testClient.SendTestStringBase("BaseString");
        testClient.SendTestStringCritical("CriticalString");

        yield return new WaitForSeconds(0.3f);
    }

    [UnityTest]
    public IEnumerator RelayTest()
    {
        StartRelayServer();
        yield return null;

        var go = new GameObject("TestClient1");
        var testClient1 = go.AddComponent<VRMineClientSim>();

        go = new GameObject("TestClient2");
        var testClient2 = go.AddComponent<VRMineClientSim>();

        go = new GameObject("TestClient3");
        var testClient3 = go.AddComponent<VRMineClientSim>();
        yield return null;

        testClient1.Connect("localhost", VRMineRelay.RelayPort);
        testClient2.Connect("localhost", VRMineRelay.RelayPort);
        testClient3.Connect("localhost", VRMineRelay.RelayPort);

        yield return new WaitForSeconds(0.3f);

        VRNTextMessage txtMessage = new VRNTextMessage();
        txtMessage.Message = "Reliable Message 1";
        testClient1.SendNetMessage(VRNPacketType.TextMessage, txtMessage, reliable: true);

        txtMessage.Message = "Unreliable Message 1";
        testClient1.SendNetMessage(VRNPacketType.TextMessage, txtMessage, reliable: false);

        txtMessage.Message = "Unreliable Message 2";
        testClient1.SendNetMessage(VRNPacketType.TextMessage, txtMessage, reliable: false);

        //testClient1.SendTestStringReliable("Reliable Message 1");
        //testClient1.SendTestStringBase("Unreliable Message 1");
        //testClient1.SendTestStringBase("Unreliable Message 2");

        yield return new WaitForSeconds(0.3f);

    }

    [UnityTest]
    public IEnumerator TestDisconnect()
    {
        StartRelayServer();
        yield return null;

        var go = new GameObject("TestClient1");
        var testClient1 = go.AddComponent<VRMineClientSim>();

        go = new GameObject("TestClient2");
        var testClient2 = go.AddComponent<VRMineClientSim>();

        go = new GameObject("TestClient3");
        var testClient3 = go.AddComponent<VRMineClientSim>();

        yield return null;

        testClient1.Connect("localhost", VRMineRelay.RelayPort);
        testClient2.Connect("localhost", VRMineRelay.RelayPort);
        testClient3.Connect("localhost", VRMineRelay.RelayPort);
        yield return new WaitForSeconds(0.3f);

        Assert.That(testClient2.NumClientJoinedMessages, Is.EqualTo(2));

        testClient1.Disconnect();
        yield return new WaitForSeconds(0.5f);

        Assert.That(testClient2.NumClientLeftMessages, Is.EqualTo(1));

        testClient1.Connect("localhost", VRMineRelay.RelayPort);
        yield return new WaitForSeconds(0.5f);

        testClient1.Disconnect();
        yield return new WaitForSeconds(0.5f);

        testClient1.Connect("localhost", VRMineRelay.RelayPort);
        yield return new WaitForSeconds(0.5f);

        Assert.That(testClient1.ClientID, Is.GreaterThan(0));

        VRNTextMessage textMessage = new VRNTextMessage { Message = "Hello World" };
        testClient3.SendNetMessage(VRNPacketType.TextMessage,
            textMessage,
            reliable: true, broadcast: false,
            clientID: testClient1.ClientID);

        yield return new WaitForSeconds(0.5f);
        Assert.That(testClient1.LastTextMessage.Message, Is.EqualTo(textMessage.Message));
    }

    [UnityTest]
    public IEnumerator ReliableLargeSendTest()
    {
        StartRelayServer(false);
        yield return null;

        var go = new GameObject("TestClient1");
        var testClient1 = go.AddComponent<VRMineClientSim>();

        go = new GameObject("TestClient2");
        var testClient2 = go.AddComponent<VRMineClientSim>();

        //testClient1.SimulateLatency = true;
        //testClient2.SimulateLatency = true;
        yield return null;

        testClient1.Connect("localhost", VRMineRelay.RelayPort);
        testClient2.Connect("localhost", VRMineRelay.RelayPort);

        yield return new WaitForSeconds(0.3f);

        Assert.That(testClient1.IsConnected);
        Assert.That(testClient2.IsConnected);

        testClient2.NumUnknownPackets = 0;

        const int maxPayload = 10000;
        byte[] buffer = new byte[maxPayload];
        System.Random rand = new System.Random();
        rand.NextBytes(buffer);

        CodedOutputStream stream = new CodedOutputStream(buffer);

        VRNHeader header = new VRNHeader();
        header.PacketType = VRNPacketType.Unknown;
        header.PacketDest = VRNPacketDestination.Broadcast;

        stream.WriteMessage(header);
        stream.Flush();

        if (!testClient1.IsConnected || !testClient2.IsConnected)
        {
            throw new System.Exception("Test clients couldn't connect to relay");
        }

        int packetsSent = 0;
        for (int size = 500; size <= maxPayload; size += 100)
        {
            Debug.Log($"Sending test size: {size}");
            testClient1.Send(buffer, size, reliable: true, sequenced: true);
            packetsSent++;

            yield return null;
            yield return null;
            yield return null;
            //yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSecondsRealtime(1.0f);

        Assert.That(testClient2.NumUnknownPackets, Is.EqualTo(packetsSent));

        //testClient1.Send(testC)
    }

    [UnityTest]
    public IEnumerator RapidSendTest()
    {
        StartRelayServer(true);
        yield return null;

        var go = new GameObject("TestClient1");
        var testClient1 = go.AddComponent<VRMineClientSim>();

        go = new GameObject("TestClient2");
        var testClient2 = go.AddComponent<VRMineClientSim>();

        testClient1.SimulateLatency = true;
        testClient2.SimulateLatency = true;
        yield return null;

        testClient1.Connect("localhost", VRMineRelay.RelayPort);
        testClient2.Connect("localhost", VRMineRelay.RelayPort);

        yield return new WaitForSeconds(0.5f);

        testClient2.NumPacketsReceived = 0;
        testClient2.NumTextMessagesReceived = 0;

        byte[] buffer = new byte[3000];
        CodedOutputStream stream = new CodedOutputStream(buffer);

        VRNHeader header = new VRNHeader();
        header.PacketType = VRNPacketType.TextMessage;
        header.PacketDest = VRNPacketDestination.Broadcast;

        VRNTextMessage msg = new VRNTextMessage();
        msg.Message = "Test";

        stream.WriteMessage(header);
        stream.WriteMessage(msg);
        stream.Flush();

        if (!testClient1.IsConnected || !testClient2.IsConnected)
        {
            throw new System.Exception("Test clients couldn't connect to relay");
        }

        //send rapid test messages for sequential frames
        int numMessages = 350;
        int numFrames = 30;
        for (int i = 0; i < numFrames; i++)
        {
            for (int j = 0; j < numMessages; j++)
            {
                testClient1.Send(buffer, (int)stream.Position, reliable: false, sequenced: false);
            }

            yield return null;
        }

        yield return new WaitForSecondsRealtime(2.0f);

        Assert.That(testClient2.NumTextMessagesReceived, Is.EqualTo(numMessages * numFrames));
    }    

    [UnityTest]
    public IEnumerator ReliableWindowTestNoLatency()
    {
        yield return ReliableWindowTest(false);
    }

    [UnityTest]
    public IEnumerator ReliableWindowTestWithLatency()
    {
        yield return ReliableWindowTest(true);
    }

    public IEnumerator ReliableWindowTest(bool simLatency)
    {
        StartRelayServer(true);
        yield return null;

        var go = new GameObject("TestClient1");
        var testClient1 = go.AddComponent<VRMineClientSim>();

        go = new GameObject("TestClient2");
        var testClient2 = go.AddComponent<VRMineClientSim>();

        testClient1.SimulateLatency = simLatency;
        testClient2.SimulateLatency = simLatency;
        yield return null;

        testClient1.Connect("localhost", VRMineRelay.RelayPort);
        testClient2.Connect("localhost", VRMineRelay.RelayPort);

        yield return new WaitForSeconds(0.3f);

        testClient2.NumTextMessagesReceived = 0;

        byte[] buffer = new byte[VRMineTransport.MaxPayloadSize];
        CodedOutputStream stream = new CodedOutputStream(buffer);

        VRNHeader header = new VRNHeader();
        header.PacketType = VRNPacketType.TextMessage;
        header.PacketDest = VRNPacketDestination.Broadcast;

        VRNTextMessage msg = new VRNTextMessage();
        msg.Message = "Test";

        stream.WriteMessage(header);
        stream.WriteMessage(msg);
        stream.Flush();

        if (!testClient1.IsConnected || !testClient2.IsConnected)
        {
            throw new System.Exception("Test clients couldn't connect to relay");
        }

        //send rapid test messages for sequential frames
        int numMessages = 200;
        int numFrames = 4;
        for (int i = 0; i < numFrames; i++)
        {
            for (int j = 0; j < numMessages; j++)
            {
                testClient1.Send(buffer, (int)stream.Position, reliable: true, sequenced: true);
            }

            yield return null;
        }

        yield return new WaitForSecondsRealtime(12.0f);

        Assert.That(testClient2.NumTextMessagesReceived, Is.EqualTo(numMessages * numFrames));
    }

    [UnityTest]
    public IEnumerator ClientIDAssignTest()
    {
        StartRelayServer(true);
        yield return null;

        var go = new GameObject("TestClient1");
        var testClient1 = go.AddComponent<VRMineClientSim>();

        go = new GameObject("TestClient2");
        var testClient2 = go.AddComponent<VRMineClientSim>();

        go = new GameObject("TestClientDM");
        var testClientDM = go.AddComponent<VRMineClientSim>();

        testClient1.SimulateLatency = true;
        testClient2.SimulateLatency = true;
        testClientDM.SimulateLatency = true;
        yield return null;

        testClient1.Connect("localhost", VRMineRelay.RelayPort);
        testClient2.Connect("localhost", VRMineRelay.RelayPort);
        testClientDM.Connect("localhost", VRMineRelay.RelayPort);

        yield return new WaitForSeconds(0.3f);

        //set DM to client ID 0
        var msg = new VRNAssignClientInfo { ClientID = 0 };
        testClientDM.SendNetMessage(VRNPacketType.AssignClientInfo, msg, reliable: true);


        yield return new WaitForSeconds(0.6f);

        Assert.That(testClient1.ClientID, Is.Not.Negative);
        Assert.That(testClient2.ClientID, Is.Not.Negative);
        Assert.That(testClient1.ClientID, Is.Not.EqualTo(testClient2.ClientID));

        Assert.That(testClientDM.ClientID, Is.Zero);

        //test directed send
        VRNTextMessage textMessage = new VRNTextMessage { Message = "Hello World" };
        testClient1.SendNetMessage(VRNPacketType.TextMessage, textMessage, reliable: true, broadcast: false,
            clientID: testClient2.ClientID);
        //msg = new VRNAssignClientInfo { ClientID = 0 };
        //testClient1.SendNetMessage(VRNPacketType.AssignClientInfo, msg, reliable: true);

        yield return new WaitForSeconds(0.6f);

        Assert.That(testClient2.LastTextMessage.Message, Is.EqualTo(textMessage.Message));
        Assert.That(testClientDM.LastTextMessage.Message, Is.Not.EqualTo(textMessage.Message));
    }

    [UnityTest]
    public IEnumerator MultipleClientPingTest()
    {
        const int NUM_CLIENTS = 65;
        StartRelayServer(true);
        yield return null;

        List<VRMineClientSim> testClients = new List<VRMineClientSim>();

        for (int i = 0; i < NUM_CLIENTS; i++)
        {
            var go = new GameObject($"TestClient{i}");
            var client = go.AddComponent<VRMineClientSim>();
            client.SimulateLatency = false;
            yield return null;

            client.Connect("localhost", VRMineRelay.RelayPort);
            testClients.Add(client);
        }

        yield return new WaitForSecondsRealtime(15);

        foreach (var client in testClients)
        {
            Assert.That(client.NumPingMessages, Is.GreaterThanOrEqualTo(3));
            Assert.That(client.ClientStats, Is.Not.Null);
            Assert.That(client.ClientStats.ClientStats.Count, Is.EqualTo(NUM_CLIENTS));

            foreach (var clientStat in client.ClientStats.ClientStats)
            {
                Assert.That(clientStat.AvgPingRTT, Is.GreaterThanOrEqualTo(0));
            }
        }

    }

    [UnityTest]
    public IEnumerator NetworkManagerTest()
    {
        StartRelayServer();
        yield return null;

        //var go = new GameObject("NetworkManager");
        var go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("NetworkManager"));
        go.name = "NetworkManager";
        var netManager = go.GetComponent<NetworkManager>();

        go = new GameObject("PlayerManager");
        var playerManager = go.AddComponent<PlayerManager>();

        //netManager.OverrideConfiguration = true;
        //netManager.StartAsServer = true;
        //netManager.PrimaryUDPPort = VRMineRelay.RelayPort;
        //netManager.DefaultMultiplayerServer = "localhost";

        go = new GameObject("TestClient1");
        var testClient1 = go.AddComponent<VRMineClientSim>();

        go = new GameObject("TestClient2");
        var testClient2 = go.AddComponent<VRMineClientSim>();
        yield return null;

        testClient1.Connect("localhost", VRMineRelay.RelayPort);
        testClient2.Connect("localhost", VRMineRelay.RelayPort);


        yield return new WaitForSeconds(1.3f);

        Assert.That(testClient1.IsConnected);
        Assert.That(testClient2.IsConnected);
        Assert.That(netManager.IsConnected);

        var msg = new VRNRequestNewPlayerID();

        msg.ClientID = testClient1.ClientID;
        msg.PlayerName = "TestClient1";
        testClient1.SendNetMessage(VRNPacketType.RequestNewPlayerId, msg,
            broadcast: false, reliable: true);

        msg.ClientID = testClient2.ClientID;
        msg.PlayerName = "TestClient2";
        testClient2.SendNetMessage(VRNPacketType.RequestNewPlayerId, msg,
            broadcast: false, reliable: true);

        yield return new WaitForSeconds(0.3f);

        Assert.That(testClient1.PlayerID, Is.GreaterThan(0));
        Assert.That(testClient2.PlayerID, Is.GreaterThan(0));

    }

    private GameObject StartSimClient(string name)
    {
        var go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("SimClient"));
        go.name = name;

        return go;

    }

    private GameObject StartSimDM(string name)
    {
        var go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("SimDM"));
        go.name = name;

        return go;
    }

    [UnityTest]
    public IEnumerator SimClientTest()
    {
        const int NUM_CLIENTS = 15;

        List<GameObject> simClients = new List<GameObject>();

        StartRelayServer();
        yield return null;

        var dm = StartSimDM("SimDM");
        yield return new WaitForSecondsRealtime(0.3f);

        for (int i = 0; i < NUM_CLIENTS; i++)
        {
            var client = StartSimClient($"Client{i}");
            simClients.Add(client);
        }
        yield return new WaitForSecondsRealtime(0.5f);

        var dmPlayers = dm.GetComponentInChildren<PlayerManager>();
        foreach (var player in dmPlayers.PlayerList.Values)
        {
            Debug.Log($"Player: {player.Name} ClientID: {player.ClientID}");
        }

        Assert.That(dmPlayers.PlayerList.Count, Is.EqualTo(NUM_CLIENTS));
    }

    [UnityTest]
    public IEnumerator SimClientDMReconnect()
    {
        const int NUM_CLIENTS = 5;

        List<GameObject> simClients = new List<GameObject>();

        var relay = StartRelayServer();
        yield return null;

        var dm = StartSimDM("SimDM");
        yield return new WaitForSecondsRealtime(0.3f);

        for (int i = 0; i < NUM_CLIENTS; i++)
        {
            var client = StartSimClient($"Client{i}");
            simClients.Add(client);
        } 
        yield return new WaitForSecondsRealtime(1.5f);

        var dmPlayers = dm.GetComponentInChildren<PlayerManager>();
        foreach (var player in dmPlayers.PlayerList.Values)
        {
            Debug.Log($"Player: {player.Name} ClientID: {player.ClientID}");  
        }

        Assert.That(dmPlayers.PlayerList.Count, Is.EqualTo(NUM_CLIENTS)); 

        for (int i = 0; i < NUM_CLIENTS; i++)
        {
            var client = simClients[i];
            var clientPlayers = client.GetComponentInChildren<PlayerManager>();
            Assert.That(clientPlayers.PlayerList.Count, Is.EqualTo(NUM_CLIENTS - 1));
        }

        GameObject.Destroy(dm);
        yield return new WaitForSecondsRealtime(12.0f);

        //check that the clients have reset
        for (int i = 0; i < NUM_CLIENTS; i++)
        {
            var client = simClients[i];
            var clientPlayers = client.GetComponentInChildren<PlayerManager>();
            Assert.That(clientPlayers.PlayerList.Count, Is.EqualTo(0));
        }

        dm = StartSimDM("SimDM2");
        var dmNet = dm.GetComponentInChildren<NetworkManager>();
        yield return new WaitForSecondsRealtime(1.0f);

        dmNet.SendTextMessage("test");

        yield return new WaitForSecondsRealtime(5.0f);



        dmPlayers = dm.GetComponentInChildren<PlayerManager>();
        Assert.That(dmPlayers.PlayerList.Count, Is.EqualTo(NUM_CLIENTS));

        Assert.That(relay.SendErrors, Is.EqualTo(0));

    }

    [UnityTest]
    public IEnumerator SimClientRelayRestart()
    {
        const int NUM_CLIENTS = 5;

        List<GameObject> simClients = new List<GameObject>();

        var relay = StartRelayServer();
        yield return null;

        var dm = StartSimDM("SimDM");
        yield return new WaitForSecondsRealtime(0.3f);

        for (int i = 0; i < NUM_CLIENTS; i++)
        {
            var client = StartSimClient($"Client{i}");
            simClients.Add(client);
        }
        yield return new WaitForSecondsRealtime(0.5f);

        var dmNet = dm.GetComponentInChildren<NetworkManager>();
        var dmPlayers = dm.GetComponentInChildren<PlayerManager>();

        foreach (var player in dmPlayers.PlayerList.Values)
        {
            Debug.Log($"Player: {player.Name} ClientID: {player.ClientID}");
        }

        Assert.That(dmPlayers.PlayerList.Count, Is.EqualTo(NUM_CLIENTS));

        GameObject.Destroy(relay.gameObject);
        yield return new WaitForSecondsRealtime(15.0f);

        Assert.That(dmNet.IsConnected, Is.False);
        GameObject.Destroy(dm);

        relay = StartRelayServer();
        yield return new WaitForSecondsRealtime(5.0f);

        dm = StartSimDM("SimDM2");
        dmNet = dm.GetComponentInChildren<NetworkManager>();
        dmPlayers = dm.GetComponentInChildren<PlayerManager>();
        yield return new WaitForSecondsRealtime(2.0f);


        Assert.That(dmNet.IsConnected);
        Assert.That(dmPlayers.PlayerList.Count, Is.EqualTo(NUM_CLIENTS));

    }


    [UnityTest]
    public IEnumerator DMDisconnectTest()
    {
        var relay = StartRelayServer();
        yield return null;

        var dm = StartSimDM("SimDM");
        yield return new WaitForSecondsRealtime(2.0f);

        Assert.That(relay.ConnectedClients, Is.EqualTo(1));

        GameObject.Destroy(dm);
        yield return new WaitForSecondsRealtime(12.0f);
        
        Assert.That(relay.ConnectedClients, Is.EqualTo(0));
    }

    [Test]
    public void MovingAverageTest()
    {
        MovingAverage avg = new MovingAverage(20);

        avg.AddSample(5);
        avg.AddSample(5);
        avg.AddSample(5);
        avg.AddSample(5);

        Assert.That(avg.Average, Is.EqualTo(5).Within(0.05f));


        avg.AddSample(10);
        avg.AddSample(15);
        avg.AddSample(20);

        Assert.That(avg.Average, Is.EqualTo(9.286f).Within(0.05f));

        avg.Reset();
        Assert.That(avg.Average, Is.EqualTo(0));

        for (int i = 0; i < 100; i++)
        {
            var val = Random.value * 100.0f;
            avg.AddSample(val);
        }

        float sum = 0;
        for (int i = 0; i < 20; i++)
        {
            var val = Random.value * 100.0f;
            sum += val;
            avg.AddSample(val);
        }
        var testAvg = sum / 20.0f;

        Assert.That(avg.Average, Is.EqualTo(testAvg));
    }
}
