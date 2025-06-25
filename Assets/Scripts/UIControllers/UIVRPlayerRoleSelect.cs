using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIVRPlayerRoleSelect : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public GameObject ButtonPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        AddButton("Captain", () => { SetPlayerRole(VRNPlayerRole.Captain); });
        AddButton("GasMan", () => { SetPlayerRole(VRNPlayerRole.GasMan); });
        AddButton("MapMan", () => { SetPlayerRole(VRNPlayerRole.MapMan); });
        AddButton("TailCapt", () => { SetPlayerRole(VRNPlayerRole.TailCaptain); });
    }


    void SetPlayerRole(VRNPlayerRole role)
    {
        var currentPlayer = PlayerManager.CurrentPlayer;
        if (currentPlayer != null)
        {
            PlayerManager.AssignPlayerRole(currentPlayer.PlayerID, role, currentPlayer.PlayerDominantHand);
        }
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
}
