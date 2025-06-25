using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
[Serializable]
public class ComponentInspector_Name : MonoBehaviour
{
    public Placer Placer;
    //Inspector inspector;
    [SerializeField] GameObject header;
    [SerializeField] TMP_InputField field;
    [SerializeField] GameObject UIContainer;
    [SerializeField] TextMeshProUGUI _label;

    private ObjectInfo _selObjectInfo = null;
    //private RectTransformInspector _rti;
    //private WindowLayoutController _wlc;

    public void Start()
    {
        if (Placer == null)
            Placer = Placer.GetDefault();

        Placer.SelectedObjectChanged += OnSelectedObjectChanged;

        //inspector = Inspector.instance;
        UIContainer = transform.GetChild(0).gameObject;
        field.onEndEdit.AddListener(SetDisplayName);
        //inspector.onNewTarget += OnNewTarget;
        //inspector.onClearTarget += OnClearTarget;
        field.onSelect.AddListener(DisableTruncate);

        //_rti = GetComponentInParent<RectTransformInspector>();
        //_wlc = GetComponentInParent<WindowLayoutController>();
        //_rti.onSizeChanged += OnSizeChange;
        //_wlc.onLayoutChanged += OnSizeChange;

    }

    /*

    //public abstract void UpdateInspector();
    /// <summary>
    /// Attempts to resize the children such that it should stretch properly.
    /// </summary>
    private void OnSizeChange()
    {
        if (this == null)
        {
            return;
        }
        RectTransform myRect = this.GetComponent<RectTransform>();
        //RectTransform parentRect = this.transform.parent.GetComponent<RectTransform>();
        //if(myRect != null && parentRect != null)
        //{
        //    Vector2 delta = myRect.sizeDelta;
        //    delta.x = parentRect.sizeDelta.x;
        //    myRect.sizeDelta = delta;
        //}
        Vector2 myDelta = myRect.sizeDelta;
        myDelta.x = _rti.cachedSize.x;
        myRect.sizeDelta = myDelta;
    }
    */

    private void OnSelectedObjectChanged(GameObject obj)
    {
        _selObjectInfo = null;
        if (obj != null)
            obj.TryGetComponent<ObjectInfo>(out _selObjectInfo);

        GetDisplayName();
        SetExposureLevel();
    }

    private void OnDestroy()
    {
        field.onEndEdit.RemoveListener(SetDisplayName);
        field.onSelect.RemoveListener(DisableTruncate);
        //inspector.onNewTarget -= OnNewTarget;
        //inspector.onClearTarget -= OnClearTarget;

        //Placer.SelectedObjectChanged -= OnSelectedObjectChanged;
        //_rti.onSizeChanged -= OnSizeChange;
        //_wlc.onLayoutChanged -= OnSizeChange;
    }

    /// <summary>
    ///  get display name from object info
    /// </summary>
    void GetDisplayName()
    {
        //field.text = inspector.targetInfo.componentInfo_Name.instanceDisplayName;
        if (_selObjectInfo == null)
            return;

        //field.text = inspector.targetInfo.displayName;
        field.text = _selObjectInfo.InstanceName;
    }

    /// <summary>
    /// Set display name from input field stop edit event
    /// </summary>
    /// <param name="value"></param>
    void SetDisplayName(string value)
    {
        //IEnumerator setDisplay = inspector.targetInfo.SetDisplayName(value);
        //inspector.targetInfo.StartCoroutine(setDisplay);
        //IEnumerator setUserSupName = inspector.targetInfo.SetUserSuppliedName(value);
        //inspector.targetInfo.StartCoroutine(setUserSupName);
        //inspector.targetInfo.SetUserSuppliedName(value);

        _selObjectInfo.InstanceName = value;

        _label.overflowMode = TextOverflowModes.Ellipsis;
        _label.rectTransform.anchoredPosition = Vector2.zero;
        /*
        inspector.targetInfo.componentInfo_Name.instanceDisplayName = value;
        inspector.targetInfo.displayName = value;
        //set name in hierarchy
        inspector.targetInfo.hierarchyItem.SetDisplayName(inspector.targetInfo.gameObject);
        Debug.Log("Call set display name");*/
    }

    private void DisableTruncate(string arg0)
    {
        _label.overflowMode = TextOverflowModes.Overflow;
    }

    public void SetExposureLevel()
    {
        if (_selObjectInfo == null)
        {
            NullExposureLevel();
            return;
        }

        if (!UIContainer.activeInHierarchy) 
            UIContainer.SetActive(true);

        // name Info
        switch (_selObjectInfo.componentInfo_Name.exposureLevel)
        {
            case Inspector.ExposureLevel.Editable:
                field.gameObject.SetActive(true);
                field.interactable = true;
                header.gameObject.SetActive(true);
                break;
            case Inspector.ExposureLevel.Visible:
                field.gameObject.SetActive(true);
                field.interactable = false;
                header.gameObject.SetActive(true);
                break;
            case Inspector.ExposureLevel.Hidden:
                field.gameObject.SetActive(false);
                header.gameObject.SetActive(false);
                break;
        }
    }

    void NullExposureLevel()
    {
        //field.gameObject.SetActive(false);
        //header.gameObject.SetActive(false);
        UIContainer.SetActive(false);
    }

    //void OnNewTarget()
    //{
    //    GetDisplayName();
    //    SetExposureLevel();
    //}

    //void OnClearTarget()
    //{
    //    NullExposureLevel();
    //}
}
