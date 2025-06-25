using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnSetUpdateRate : UIContextBase
{
    public NetworkManager NetworkManager;
    public string ObjUpdateRateVar = "OBJECT_UPDATE_RATE";
    public string VRUpdateRateVar = "VR_UPDATE_RATE";

    private Button _button;

    protected override void Start()
    {
        base.Start();
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);

        //_context.ContextDataChanged += OnContextDataChanged;
    }

    private void OnButtonClicked()
    {
        ChangeUpdateRate();
    }

    //private void OnContextDataChanged(string var)
    //{
    //    if (var == ObjUpdateRateVar || var == VRUpdateRateVar)
    //    {
    //        ChangeUpdateRate();
    //    }
    //}

    private void ChangeUpdateRate()
    {
        if (_context == null || NetworkManager == null)
            return;

        var objUpdateRate = _context.GetFloatVariable(ObjUpdateRateVar);
        var vrUpdateRate = _context.GetFloatVariable(VRUpdateRateVar);

        NetworkManager.SetNetworkUpdateRate(objUpdateRate, vrUpdateRate, true);
    }
}
