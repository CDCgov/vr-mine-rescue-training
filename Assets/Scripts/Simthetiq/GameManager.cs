using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// GameManager used to keep track of the inputmanager and Master Setting
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    public static BH20InputManager inputManager = null;
    public static MasterSetting masterSetting = null;
    public static float currentSpeed = 0f;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            masterSetting = FindObjectOfType<MasterSetting>();
            inputManager = FindObjectOfType<BH20InputManager>();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

    }
}
