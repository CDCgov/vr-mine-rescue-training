using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NetworkManagerTests
    {

        private NetworkManager _netManagerServer;
        private NetworkManager _netManagerClient;

        [SetUp]
        public void Setup()
        {
            var serverObj = new GameObject("NetManagerServer");
            var clientObj = new GameObject("NetManagerClient");

            _netManagerServer = serverObj.AddComponent<NetworkManager>();
            _netManagerClient = clientObj.AddComponent<NetworkManager>();

            Debug.Log("Setup Complete");
        }

        [TearDown]
        public void Teardown()
        {
            if (_netManagerClient != null)
                GameObject.DestroyImmediate(_netManagerClient.gameObject);
            if (_netManagerServer != null)
                GameObject.DestroyImmediate(_netManagerServer.gameObject);

            Debug.Log("Teardown Complete");
        }

        [Test]
        public void SimpleTest()
        {
            Assert.That(_netManagerClient, Is.Not.Null);
            Assert.That(_netManagerServer, Is.Not.Null);
        }

        [Test]
        public void ConnectTest()
        {
        }
    }

}