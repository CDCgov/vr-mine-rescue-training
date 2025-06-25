using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIDMMuteSelf : MonoBehaviour
{
    public PlayerManager PlayerManager;
#if DISSONANCE
    public Dissonance.DissonanceComms DissonanceComms;
    private Toggle _toggle;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault();

        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnValueChanged);

        _toggle.SetIsOnWithoutNotify(DissonanceComms.IsMuted);
    }

    private void OnValueChanged(bool val)
    {
        DissonanceComms.IsMuted = val;
    }
#endif
}
