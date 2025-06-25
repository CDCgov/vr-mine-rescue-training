using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;

public class SpawnObjectListController : MonoBehaviour, IMinimizableWindow
{
    public NetworkManager NetworkManager;
    public SpawnableObjectList SpawnableObjects;
    public GameObject ItemPrefab;
    public RectTransform ActionButtonsParent;
    public ToggleGroup ToggleGroup;

    [System.NonSerialized]
    public SpawnableObject SelectedObject = null;

    public event Action<string> TitleChanged;

    private Dictionary<string, GameObject> _actionButtons = new Dictionary<string, GameObject>();

    private bool _validMouseDownPos = false;
    private Vector3 _posMouseDown;

    private List<SpawnableObject> _spawnableObjects;

    // Use this for initialization
    async void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        _spawnableObjects = new List<SpawnableObject>();

        var handle = Addressables.LoadResourceLocationsAsync("SpawnableObjectList");
        var objectLists = await handle.Task;
        
        if (objectLists != null)
        {
            foreach (var listResource in objectLists)
            {
                var listHandle = Addressables.LoadAssetAsync<SpawnableObjectList>(listResource.PrimaryKey);
                var list = await listHandle.Task;

                AddSpawnableObjects(list);

                Addressables.Release(listHandle);
            }
        }
        Addressables.Release(handle);

        CreateSpawnItemButtons();
    }

    void AddSpawnableObjects(SpawnableObjectList list)
    {
        _spawnableObjects.AddRange(list.ObjectList);
    }

    void CreateSpawnItemButtons()
    {
        _spawnableObjects.Sort((a, b) =>
        {
            return string.Compare(a.ObjectName, b.ObjectName);
        });

        foreach (var obj in _spawnableObjects)
        {
            AddSpawnItemButton(obj.ObjectName, (bool toggled) => {
                if (toggled)
                {
                    SelectSpawnItem(obj);
                }
                else
                {
                    SelectSpawnItem(null);
                }
            });
        }
    }

    bool PerformRaycast(out Vector3 pos)
    {
        pos = Vector3.zero;

        var camera = Camera.main;
        if (camera == null)
            return false;

        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask("Floor", "Default"), 
            QueryTriggerInteraction.Ignore))
        {
            pos = hit.point;
            return true;
        }

        return false;
    }

    protected virtual void Update()
    {
        if (SelectedObject == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            _validMouseDownPos = false;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (SelectedObject.SpanEntry)
            {
                _validMouseDownPos = PerformRaycast(out _posMouseDown);
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("No main camera to use for spawn raycast");
                SelectedObject = null;
                return;
            }

            Debug.Log($"Spawning object using camera {camera.name}");


            SelectedObject.Spawn(camera, LayerMask.GetMask("Floor", "Default"), NetworkManager);
            //_selectedObject = null;
        }
        else if (_validMouseDownPos && SelectedObject.SpanEntry && Input.GetMouseButtonUp(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 pos2;
            if (PerformRaycast(out pos2))
            {
                _ = SelectedObject.SpawnSpannedObject(_posMouseDown, pos2, LayerMask.GetMask("Floor", "Default"), NetworkManager);
            }
        }
    }

    void SelectSpawnItem(SpawnableObject obj)
    {
        SelectedObject = obj;
    }

    public void ClearSelection()
    {
        foreach (var obj in _actionButtons.Values)
        {
            var toggle = obj.GetComponent<Toggle>();
            toggle.isOn = false;
        }

        SelectedObject = null;
    }

    void AddSpawnItemButton(string text, UnityAction<bool> action)
    {
        //Debug.Log($"Adding spawn button {text}");
        if (_actionButtons.ContainsKey(text))
            return;

        var obj = Instantiate<GameObject>(ItemPrefab);
        var toggle = obj.GetComponent<Toggle>();
        var txt = obj.GetComponentInChildren<TextMeshProUGUI>();

        txt.text = text;
        toggle.onValueChanged.AddListener(action);
        toggle.group = ToggleGroup;

        obj.SetActive(true);
        obj.transform.SetParent(ActionButtonsParent, false);

        _actionButtons[text] = obj;
    }

    public string GetTitle()
    {
        return "Spawn Obj";
    }

    public void Minimize(bool minimize)
    {
        gameObject.SetActive(minimize);
    }

    public void ToggleMinimize()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    public void AssignTaskbarButton(Button button)
    {

    }
}
