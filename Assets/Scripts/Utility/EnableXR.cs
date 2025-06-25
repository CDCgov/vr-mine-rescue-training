using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class EnableXR : MonoBehaviour
{
    public ConfigureMinerXRRig XRMinerConfiguration;

    private void Awake()
    {
        TextureXR.maxViews = 2;

        //Debug.Log($"GameViewRenderMode: {XRSettings.gameViewRenderMode.ToString()}");
        //Debug.Log($"XR Enabled: {XRSettings.enabled}");

    }

    private void OnDestroy()
    {
        StopXR();
    }

    //private IEnumerator Start()
    //{
    //       yield return new WaitForSeconds(1);
    //	yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
    //	XRGeneralSettings.Instance.Manager.StartSubsystems();
    //	if (XRMinerConfiguration == null)
    //	{
    //		XRMinerConfiguration = FindObjectOfType<ConfigureMinerXRRig>();
    //	}
    //	//4XRMinerConfiguration.DefaultConfigure();
    //}

    //private void OnDestroy()
    //{
    //	XRGeneralSettings.Instance.Manager.StopSubsystems();
    //	XRGeneralSettings.Instance.Manager.DeinitializeLoader();
    //}

    //private void OnEnable()
    //{
    //    XRGeneralSettings.Instance.Manager.InitializeLoader();
    //}

    //private void Start()
    //{
    //    XRGeneralSettings.Instance.Manager.StartSubsystems();
    //}

    //private void OnDisable()
    //{
    //    XRGeneralSettings.Instance.Manager.StopSubsystems();
    //}

    //private void OnDestroy()
    //{
    //    XRGeneralSettings.Instance.Manager.DeinitializeLoader();
    //}

    private IEnumerator Start()
    {
        yield return StartXR();
    }

    public IEnumerator StartXR()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            Debug.Log("Starting XR...");
            yield return new WaitForSecondsRealtime(0.25f);
            XRGeneralSettings.Instance.Manager.StartSubsystems();

            yield return null;

            //Debug.LogError($"GameViewRenderMode: {XRSettings.gameViewRenderMode.ToString()}");
            //Debug.Log($"XR Enabled: {XRSettings.enabled}");
        }
    }

    void StopXR()
    {
        Debug.Log("Stopping XR...");

        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped completely.");
    }



    //IEnumerator Start()
    //{
    //    yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
    //    XRGeneralSettings.Instance.Manager.StartSubsystems();
    //}
}
