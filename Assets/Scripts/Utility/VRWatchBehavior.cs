using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class VRWatchBehavior : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public ControllerType ControllerType = ControllerType.LeftController;
    public GameObject WatchObject;
    public bool IsMapboardWatch = false;

    private MeshRenderer _labelMeshRenderer;
    private TextMeshPro _tmProLabel;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }

        _labelMeshRenderer = GetComponent<MeshRenderer>();
        _tmProLabel = GetComponent<TextMeshPro>();
        PlayerManager.CurrentPlayer.PlayerHandednessChanged += CurrentPlayer_PlayerHandednessChanged;

        HideWatch(PlayerManager.CurrentPlayer.PlayerDominantHand);
    }

    private void OnDestroy()
    {
        if (PlayerManager != null && PlayerManager.CurrentPlayer != null)
        {
            PlayerManager.CurrentPlayer.PlayerHandednessChanged -= CurrentPlayer_PlayerHandednessChanged;
        }
    }

    private void CurrentPlayer_PlayerHandednessChanged(PlayerDominantHand obj)
    {
        HideWatch(obj);
    }

    private void HideWatch(PlayerDominantHand playerDominantHand)
    {
        if (IsMapboardWatch)
        {
            return;
        }
        switch (playerDominantHand)
        {
            case PlayerDominantHand.RightHanded:
                if (ControllerType == ControllerType.LeftController)
                {
                    _labelMeshRenderer.enabled = true;
                    WatchObject.SetActive(true);
                }
                else
                {
                    _labelMeshRenderer.enabled = false;
                    WatchObject.SetActive(false);
                }
                break;
            case PlayerDominantHand.LeftHanded:
                if (ControllerType == ControllerType.RightController)
                {
                    _labelMeshRenderer.enabled = true;
                    WatchObject.SetActive(true);
                }
                else
                {
                    _labelMeshRenderer.enabled = false;
                    WatchObject.SetActive(false);
                }
                break;
            default:
                break;
        }
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(UpdateWatchText), 1.0f, 1.0f);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    void UpdateWatchText()
    {
        if (!_labelMeshRenderer.enabled)
            return;

        DateTime dateTime = System.DateTime.Now;
        int hour = ((dateTime.Hour + 11) % 12) + 1;
        string minute = dateTime.Minute.ToString("00");
        if (dateTime.Second % 2 == 0)
        {
            _tmProLabel.text = $"{hour}:{minute}";
        }
        else
        {
            _tmProLabel.text = $"{hour} {minute}";
        }

    }

    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == "DebriefMainScene")
            {
                _labelMeshRenderer.enabled = false;
            }
        }
    }
}
