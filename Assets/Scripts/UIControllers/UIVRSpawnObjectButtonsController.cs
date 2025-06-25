using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;

public class UIVRSpawnObjectButtonsController : MonoBehaviour
{
    public NetworkManager NetworkManager;

    public GameObject ButtonPrefab;

    [System.Serializable]
    public struct SpawnObjectInfo
    {
        public string Name;
        public AssetReference AssetRef;
    }

    public List<SpawnObjectInfo> ObjectList;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        foreach (var info in ObjectList)
        {
            AddButton(info.Name, () => { SpawnObject(info); });
        }
    }

    void SpawnObject(SpawnObjectInfo info)
    {
        var pos = transform.position;
        if (Camera.main != null)
        {
            pos = Camera.main.transform.position;
            pos += Camera.main.transform.forward;
        }
        NetworkManager.SpawnObject(info.AssetRef.RuntimeKey.ToString(), System.Guid.NewGuid(), pos, Quaternion.identity, true);
    }

    void AddButton(string name, UnityAction handler)
    {
        try
        {
            var obj = GameObject.Instantiate<GameObject>(ButtonPrefab);
            var button = obj.GetComponent<Button>();
            var text = obj.GetComponentInChildren<TextMeshProUGUI>();

            button.onClick.AddListener(handler);
            text.text = name;

            obj.transform.SetParent(transform, false);
            obj.SetActive(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating button {name} : {ex.Message}");
        }
    }
}
