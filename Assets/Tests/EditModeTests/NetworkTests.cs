using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NetworkTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void NetworkTestsSimplePasses()
    {
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NetworkTestsWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }

    [Test]
    public void TestSend()
    {

    }

    private void StartServer() 
    {
        var go = new GameObject();
        go.AddComponent<VRMineRelay>();
    }
}
