using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIListLoadableAssetErrors : MonoBehaviour
{
    public LoadableAssetManager LoadableAssetManager;

    public GameObject ErrorMessagePrefab;
    public Transform ContentParentTransform;


    // Start is called before the first frame update
    void Start()
    {
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        LoadableAssetManager.LoadableAssetErrors.CollectionChanged += OnErrorListChanged;
    }

    private void OnErrorListChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        ClearList();
        AddAllMessages();
    }

    private void ClearList()
    {
        if (ContentParentTransform == null)
            return;

        foreach (Transform childTransform in ContentParentTransform)
        {
            if (!childTransform.gameObject.activeSelf)
                continue;

            Destroy(childTransform.gameObject);
        }
    }

    private void AddAllMessages()
    {
        foreach (var msg in LoadableAssetManager.LoadableAssetErrors)
        {
            AddLogMessage(msg);
        }
    }

    private void AddLogMessage(LoadableLogMessageData msg)
    {
        if (ErrorMessagePrefab == null || ContentParentTransform == null)
            return;

        var newObj = Instantiate<GameObject>(ErrorMessagePrefab, ContentParentTransform);

        string color;
        switch (msg.LogType)
        {
            case LogType.Error:
                color = "red";
                break;

            default:
                color = "yellow";
                break;
        }
        
        if (newObj.TryGetComponent<TMP_Text>(out var txt))
        {
            txt.text = $"<color=\"{color}\">{msg.LogType}</color>: {msg.Message}";
        }

        newObj.SetActive(true);
    }

}
