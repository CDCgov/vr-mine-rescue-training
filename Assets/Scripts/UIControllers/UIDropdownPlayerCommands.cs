using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownPlayerCommand : TMP_Dropdown.OptionData
{
    public delegate void PlayerCommandDelegate(PlayerRepresentation player);

    public PlayerCommandDelegate Command;
}

[RequireComponent(typeof(TMP_Dropdown))]
public class UIDropdownPlayerCommands : MonoBehaviour, ISelectedPlayerView
{
    PlayerManager PlayerManager;
    PlayerRepresentation _player;
    TMP_Dropdown _dropdown;

    // Start is called before the first frame update
    void Start()
    {
        if(_player == null)
        {
            gameObject.SetActive(false);
            return;
        }
        if (!TryGetComponent<TMP_Dropdown>(out _dropdown))
        {
            Debug.LogError("No dropdown present on UIDropdownPlayerCommands object");
            Destroy(this);
            return;
        }

        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        AddOptions();

        _dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    void AddOptions()
    {
        _dropdown.options.Clear();
        
        _dropdown.options.Add(new DropdownPlayerCommand
        {
            text = "Fill BG4",
            Command = (player) =>
            {
                PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmSetBg4Pressure, 3000);
            },
        });
        _dropdown.options.Add(new DropdownPlayerCommand
        {
            text = "Drain BG4",
            Command = (player) =>
            {
                PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmSetBg4Pressure, 0);
            },
        });
        _dropdown.options.Add(new DropdownPlayerCommand
        {
            text = "Respawn Equipment",
            Command = (player) =>
            {
                //PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmResetEquipment, 0);
                PlayerManager.AssignPlayerRole(player.PlayerID, player.PlayerRole, player.PlayerDominantHand);
            },
        });

        _dropdown.options.Add(new DropdownPlayerCommand
        {
            text = "",
            Command = null
        });

        _dropdown.SetValueWithoutNotify(1000);
    }

    private void OnDropdownChanged(int selected)
    {
        Debug.Log($"UIDropdownPlayerCommand: selected {selected}");

        if (selected < 0 || selected >= _dropdown.options.Count)
            return;

        var option = _dropdown.options[selected];

        var playerCommand = option as DropdownPlayerCommand;
        if (playerCommand != null && _player != null)
        {
            playerCommand.Command(_player);
        }

        //AddOptions();
        //Debug.Log($"Selected value: {_dropdown.value}");
        //_dropdown.value = -1;
        _dropdown.SetValueWithoutNotify(1000);
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        _player = player;
    }
}
