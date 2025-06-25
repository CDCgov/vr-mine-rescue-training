using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISelectedVentObjControl : MonoBehaviour
{
    public enum VentObjType
    {
        Airway,
        Junction,
        Fire,
        Fan,
    }

    public VentilationManager VentilationManager;

    public VentObjType EnableIfSelected;

    // Start is called before the first frame update
    void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        VentilationManager.SelectedVentObjChanged += SelectedObjChanged;

        SelectedObjChanged(VentilationManager.SelectedVentUIObj);
    }

    private void OnDestroy()
    {
        VentilationManager.SelectedVentObjChanged -= SelectedObjChanged;
    }

    void SelectedObjChanged(VentUIObj obj)
    {
        bool enable = false;

        if (obj == null)
        {
            gameObject.SetActive(false);
            return;
        }

        switch (EnableIfSelected)
        {
            case VentObjType.Airway:
                enable = obj.VentObj is VentAirway;
                break;

            case VentObjType.Junction:
                enable = obj.VentObj is VentJunction;
                break;

            case VentObjType.Fire:
                enable = obj.VentObj is VentFire;
                break;

            case VentObjType.Fan:
                enable = obj.VentObj is VentFan;
                break;

        }

        gameObject.SetActive(enable);
    }
}
