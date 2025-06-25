using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using Google.Protobuf;
using UnityEngine.AddressableAssets;

public class SimNetworkTests
{

    private GameObject _clientPrefab;
    private GameObject _serverPrefab;
    private GameObject _lateClientPrefab;

    private NetworkManager _netManagerServer;
    private NetworkManager _netManagerClient;

    [OneTimeSetUp]
    public void Setup()
    {
        
    }

    [OneTimeTearDown]
    public void Teardown()
    {
       
    }

   
    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator BasicTestsWithEnumeratorPasses()
    {
        yield return new WaitForSeconds(3.0f);
    }

    [UnityTest]
    public IEnumerator TestBG4Drain()
    {
        GameObject bg4test = new GameObject("BG4Test");
        var bg4sim = bg4test.AddComponent<BG4Sim>();
        var sentinel = bg4test.AddComponent<Sentinel>();

        for (int i = 0; i < 5; i++)
        {
            Debug.Log($"BG4 Pressure: {bg4sim.OxygenPressure}");
            yield return new WaitForSecondsRealtime(1.0f);
        }

        bg4sim.OxygenPressure = 700;

        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"BG4 Pressure: {bg4sim.OxygenPressure} Alarm: {bg4sim.GetAlarmState().ToString()}");
            yield return new WaitForSecondsRealtime(1.0f);
        }

        Assert.That(bg4sim.GetAlarmState(), Is.EqualTo(VRNBG4AlarmState.LowPressureAlarm));
        bg4sim.SilenceAlarm();
        yield return new WaitForSecondsRealtime(1.0f);
        Assert.That(bg4sim.GetAlarmState(), Is.EqualTo(VRNBG4AlarmState.Silenced));

        bg4sim.OxygenPressure = 3000;
        yield return new WaitForSecondsRealtime(1.0f);
        Assert.That(bg4sim.GetAlarmState(), Is.EqualTo(VRNBG4AlarmState.Off));

        yield return new WaitForSecondsRealtime(30.0f);

        bg4sim.OxygenPressure = 700;

        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"BG4 Pressure: {bg4sim.OxygenPressure} Alarm: {bg4sim.GetAlarmState().ToString()}");
            yield return new WaitForSecondsRealtime(1.0f);
        }
    }

}
