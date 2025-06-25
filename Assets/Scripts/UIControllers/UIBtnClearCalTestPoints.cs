using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnClearCalTestPoints : MonoBehaviour
{
    //public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;

    private Button _button;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _button = GetComponent<Button>();

        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        foreach (var player in PlayerManager.PlayerList.Values)
        {
            player.CalTestPoint = Vector3.zero;
        }
    }
}
