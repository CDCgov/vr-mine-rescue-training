using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class UITextNetworkedObjectInfo : MonoBehaviour
{
    private TMP_Text _text;
    private UIContextData _context;

    private StringBuilder _sb;


    // Start is called before the first frame update
    void Start()
    {
        _context = GetComponentInParent<UIContextData>();
        if (_context == null)
        {
            Debug.LogError($"UITextNetworkedObjectInfo: Couldn't find UIContextData");
            this.enabled = false;
            return;
        }

        _sb = new StringBuilder();
        _text = GetComponent<TMP_Text>();

        _context.ContextDataChanged += OnContextDataChanged;
        UpdateText();
    }

    private void OnContextDataChanged(string contextDataKey)
    {
        if (contextDataKey == UINetworkedObjectList.SelectedNetworkedObjectKey)
            UpdateText();
    }

    void UpdateText()
    {
        if (_text == null)
            return;

        _text.text = "";

        if (_context == null)
            return;

        var netObj = _context.GetVariable(UINetworkedObjectList.SelectedNetworkedObjectKey) as NetworkedObject;
        if (netObj == null)
            return;

        _sb.Clear();
        netObj.GetObjectInfo(_sb);

        _text.text = _sb.ToString();
    }
}

