using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINetworkedObjectList : MonoBehaviour
{
    public const string SelectedNetworkedObjectKey = "SelectedNetworkedObject";

    public NetworkedObjectManager NetworkedObjectManager;

    public GameObject ListItemPrefab;

    private UIContextData _context;
    private bool _activeObjectListChanged = false;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);

        _context = transform.GetComponentInParent<UIContextData>();
        if (_context == null)
        {
            Debug.LogError($"Couldn't find UI Context Data for {gameObject.name}");
            this.enabled = false;
            return;
        }

        NetworkedObjectManager.ActiveObjectListChanged += OnActiveObjectListChanged;
    }

    private void OnEnable()
    {
        UpdateObjectList();
    }

    private void OnActiveObjectListChanged()
    {
        if (gameObject == null)
            return;

        _activeObjectListChanged = true;
        //UpdateObjectList();
    }

    private void LateUpdate()
    {
        //update object list at most once per frame
        if (_activeObjectListChanged)
        {
            _activeObjectListChanged = false;
            UpdateObjectList();
        }
    }

    private void UpdateObjectList()
    {
        if (NetworkedObjectManager == null)
            return;

        foreach (Transform xform in transform)
        {
            if (xform != ListItemPrefab.transform)
            {
                Destroy(xform.gameObject);
            }
        }

        var toggleGroup = gameObject.GetComponent<ToggleGroup>();
        if (toggleGroup == null)
            toggleGroup = gameObject.AddComponent<ToggleGroup>();
        
        foreach (var data in NetworkedObjectManager.GetActiveObjects())
        {
            var obj = GameObject.Instantiate<GameObject>(ListItemPrefab);
            obj.transform.SetParent(transform);

            var text = obj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text == null)
                break;
            
            var toggle = obj.GetComponent<Toggle>();
            if (toggle == null)
                break;

            if (data.AssociatedObj == null)
                continue;

            var netobj = data.AssociatedObj;
            text.text = $"{netobj.gameObject.name} : Owner:{netobj.OwnerClientID} : Authority:{netobj.HasAuthority}";

            toggle.group = toggleGroup;
            toggle.isOn = false;
            toggle.onValueChanged.AddListener((ison) =>
            {
                if (ison)
                {
                    _context.SetVariable(SelectedNetworkedObjectKey, netobj);
                }
            });

            obj.gameObject.SetActive(true);
        }
    }
}
