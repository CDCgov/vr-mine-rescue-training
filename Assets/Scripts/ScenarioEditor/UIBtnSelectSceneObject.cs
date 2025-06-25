using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBtnSelectSceneObject : UIButtonBase
{
    public Placer Placer;
    public TMP_Text ObjectNameTextField;

    public MenuTooltip _tooltip;

    public GameObject SceneGameObject
    {
        get
        {
            if (_sceneObject == null)
                return null;

            return _sceneObject.gameObject;
        }
    }

    private PlacablePrefab _sceneObject;
    private ObjectInfo _sceneObjectInfo;

    protected override void Start()
    {
        base.Start();

        if(_tooltip == null)
            TryGetComponent<MenuTooltip>(out _tooltip);

        OnSelectedObjectChanged(Placer.SelectedObject);
    }

    public void SetSceneObject(PlacablePrefab obj)
    {
        ClearSceneObject();

        if (obj == null)
            return;

        if (!obj.TryGetComponent<ObjectInfo>(out _sceneObjectInfo))
        {
            _sceneObjectInfo = null;
            return;
        }
        _sceneObject = obj;

        _sceneObjectInfo.ObjectInfoChanged += UpdateObjectInfo;

        UpdateObjectInfo();

        OnSelectedObjectChanged(Placer.SelectedObject);
    }

    public void ClearSceneObject()
    {
        if (_sceneObjectInfo != null)
        {
            _sceneObjectInfo.ObjectInfoChanged -= UpdateObjectInfo;
        }

        _sceneObject = null;
        _sceneObjectInfo = null;
    }

    private void UpdateObjectInfo()
    {
        if (ObjectNameTextField != null)
            ObjectNameTextField.text = _sceneObjectInfo.InstanceName;

        if (_tooltip != null)
            _tooltip.TooltipTextString = _sceneObjectInfo.InstanceName;
    }

    private void OnEnable()
    {
        if (Placer == null)
        {
            Placer = Placer.GetDefault();
        }

        Placer.SelectedObjectChanged += OnSelectedObjectChanged;

        OnSelectedObjectChanged(Placer.SelectedObject);

    }

    private void OnDisable()
    {
        ClearSceneObject();

        if (Placer != null)
            Placer.SelectedObjectChanged -= OnSelectedObjectChanged;
    }

    private void OnSelectedObjectChanged(GameObject obj)
    {
        if (gameObject == null || !gameObject.activeSelf || ObjectNameTextField == null ||
            _sceneObject == null || !_sceneObject.gameObject.activeSelf)
            return;

        if (obj == _sceneObject.gameObject)
        {
            ObjectNameTextField.color = Color.yellow;
        }
        else
        {
            ObjectNameTextField.color = Color.white;
        }
    }

    protected override void OnButtonClicked()
    {
        if (Placer.SelectedObject != _sceneObject.gameObject)
        {
            Placer.SelectObject(_sceneObject.gameObject);
        }
        else
        {
            Placer.FocusSelectedObject();
        }
    }
}
