using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnDestroyNetworkedObject : MonoBehaviour
{
    public NetworkedObjectManager NetworkedObjectManager;

    private Button _button;
    private UIContextData _context;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);

        _button = GetComponent<Button>();
        _context = GetComponentInParent<UIContextData>();
        if (_context == null)
        {
            Debug.LogError($"UIBtnDestroyNetworkedObject: Couldn't find UIContextData");
            return;
        }

        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (_context == null)
            return;

        var netObj = _context.GetVariable(UINetworkedObjectList.SelectedNetworkedObjectKey) as NetworkedObject;
        if (netObj != null)
        {
            Debug.Log($"UIBtnDestroyNetworkedObject: Sending destroy for object id {netObj.uniqueID}");
            NetworkedObjectManager.DestroyObject(netObj.uniqueID);
        }
    }
}
