using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnSelectPlayer : MonoBehaviour, ISelectedPlayerView
{
    public PlayerRepresentation PlayerToSelect;
    public Transform TargetTransform;

    public string SelectedPlayerVar = "SELECTED_PLAYER";

    private UIContextData _context;

    private Button _button;
    
    void Start()
    {
        _button = GetComponent<Button>();
        _context = transform.GetComponentInParent<UIContextData>();

        _button.onClick.AddListener(OnButtonClicked);

    }

    private void OnButtonClicked()
    {
        if (PlayerToSelect == null)
            return;

        if (_context != null && SelectedPlayerVar != null && SelectedPlayerVar.Length > 0)
        {
            _context.SetVariable(SelectedPlayerVar, PlayerToSelect);
        }

        if (TargetTransform != null)
        {
            var selectedPlayerInterfaces = TargetTransform.GetComponentsInChildren<ISelectedPlayerView>();
            foreach (var selPlayer in selectedPlayerInterfaces)
            {
                selPlayer.SetPlayer(PlayerToSelect);
            }
        }
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        if (PlayerToSelect == null)
            PlayerToSelect = player;
    }
}
