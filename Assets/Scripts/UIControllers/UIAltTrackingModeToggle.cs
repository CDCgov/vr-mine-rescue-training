using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIAltTrackingModeToggle : MonoBehaviour
{
    public GameObject ButtonPrefab;

    // Start is called before the first frame update
    void Start()
    {
        AddButton("Mode 1", SetTrackingMode1);
        AddButton("Mode 2", SetTrackingMode2);
        AddButton("Alt Direct", SetTrackingAltDirect);
    }

    void AddButton(string name, UnityAction handler)
    {
        try
        {
            var obj = GameObject.Instantiate<GameObject>(ButtonPrefab);
            var button = obj.GetComponent<Button>();
            var text = obj.GetComponentInChildren<TextMeshProUGUI>();

            button.onClick.AddListener(handler);
            text.text = name;

            obj.transform.SetParent(transform, false);
            obj.SetActive(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating button {name} : {ex.Message}");
        }
    }

    void SetTrackingMode1()
    {
        Debug.Log("Tracking Mode 1");

        var poseDriver = FindObjectOfType<AltPoseDriver>();
        if (poseDriver != null)
        {
            poseDriver.SetTrackingMode(AltPoseDriver.TrackingMode.Mode1);
        }
    }

    void SetTrackingMode2()
    {
        Debug.Log("Tracking Mode 2");

        var poseDriver = FindObjectOfType<AltPoseDriver>();
        if (poseDriver != null)
        {
            poseDriver.SetTrackingMode(AltPoseDriver.TrackingMode.Mode2);
        }

    }

    void SetTrackingAltDirect()
    {
        Debug.Log("Tracking Mode Alt Direct");

        var poseDriver = FindObjectOfType<AltPoseDriver>();
        if (poseDriver != null)
        {
            poseDriver.SetTrackingMode(AltPoseDriver.TrackingMode.AltDirect);
        }

    }
}
