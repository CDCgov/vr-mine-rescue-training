using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class UISelectedPlayerName : SelectedPlayerControl
{

    private TMP_Text _text;

    // Start is called before the first frame update
    void Start()
    {
        if (_player == null)
            return;

        _text = GetComponent<TMP_Text>();
        _text.text = _player.Name;
    }

}
