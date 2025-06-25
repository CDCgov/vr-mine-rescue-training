using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class ComponentInspector_Transform : MonoBehaviour
{
    public Placer Placer;

    public GameObject PositionRow;
    public GameObject RotationRow;
    public GameObject ScaleRow;

    //public TMP_Text PositionHeader, RotationHeader, ScaleHeader;
    //public TMP_InputField PositionField_X, PositionField_Y, PositionField_Z, RotationField_X, RotationField_Y, RotationField_Z, ScaleField_X, ScaleField_Y, ScaleField_Z;
    public InspectorField PositionField_X;
    public InspectorField PositionField_Y;
    public InspectorField PositionField_Z;

    public InspectorField RotationField_X;
    public InspectorField RotationField_Y;
    public InspectorField RotationField_Z;

    public InspectorField ScaleField_X;
    public InspectorField ScaleField_Y;
    public InspectorField ScaleField_Z;

    public Button ResetPositionButton, ResetRotationButton, ResetScaleButton;

    private GameObject _UIContainer;
    private LayoutElement _layoutElement;
    private float _preferredHeight = 100;

    //private Inspector _inspector;
    private Vector3 _lastPosition = Vector3.zero;
    private Vector3 _lastScale = Vector3.zero;
    private Quaternion _lastRotation = Quaternion.identity;
    private Vector2 _startSize;
    private RectTransform _rt;

    private Transform _selTransform = null;
    private ObjectInfo _selObjInfo = null;
    private RectTransformInspector _rti;
    private WindowLayoutController _wlc;

    private void Start()
    {
        if (Placer == null)
            Placer = Placer.GetDefault();

        //_inspector = Inspector.instance;
        _rt = transform as RectTransform;
        _startSize = new Vector2(_rt.rect.width, _rt.rect.height);

        _UIContainer = transform.GetChild(0).gameObject;
        _layoutElement = GetComponent<LayoutElement>();
        if (_layoutElement != null)
            _preferredHeight = _layoutElement.preferredHeight;

        PositionField_X.onSubmitValue.AddListener(SetPositionFromField_X);
        PositionField_Y.onSubmitValue.AddListener(SetPositionFromField_Y);
        PositionField_Z.onSubmitValue.AddListener(SetPositionFromField_Z);

        RotationField_X.onSubmitValue.AddListener(SetRotationEulerFromField_X);
        RotationField_Y.onSubmitValue.AddListener(SetRotationEulerFromField_Y);
        RotationField_Z.onSubmitValue.AddListener(SetRotationEulerFromField_Z);

        ScaleField_X.onSubmitValue.AddListener(SetScaleFromField_X);
        ScaleField_Y.onSubmitValue.AddListener(SetScaleFromField_Y);
        ScaleField_Z.onSubmitValue.AddListener(SetScaleFromField_Z);

        ResetPositionButton.onClick.AddListener(ResetPositionFromButton);
        ResetRotationButton.onClick.AddListener(ResetRotationFromButton);
        ResetScaleButton.onClick.AddListener(ResetScaleFromButton);

        Placer.SelectedObjectChanged += OnSelectedObjectChanged;

        _rti = GetComponentInParent<RectTransformInspector>();
        _wlc = GetComponentInParent<WindowLayoutController>();
        //_rti.onSizeChanged += OnSizeChange;
        //_wlc.onLayoutChanged += OnSizeChange;

        //_inspector.onNewTarget += OnNewTarget;
        //_inspector.onClearTarget += OnClearTarget;

        if (Placer.SelectedObject == null)
            HideTransformInspector();
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

        //Change Container rect too
        //RectTransform containerRect = _UIContainer.GetComponent<RectTransform>();
        //Vector2 containerDelta = containerRect.sizeDelta;
        //containerDelta.x = _rti.cachedSize.x;
        //containerRect.sizeDelta = containerDelta;
    }
    */

    private void OnSelectedObjectChanged(GameObject obj)
    {
        _selObjInfo = null;
        _selTransform = null;

        if (obj == null)
        {
            HideTransformInspector();
            return;
        }

        obj.TryGetComponent<ObjectInfo>(out _selObjInfo);
        _selTransform = obj.transform;

        GetTransformInfo();
        SetExposureLevel();
    } 

    private void OnDestroy()
    {
        PositionField_X.onSubmitValue.RemoveListener(SetPositionFromField_X);
        PositionField_Y.onSubmitValue.RemoveListener(SetPositionFromField_Y);
        PositionField_Z.onSubmitValue.RemoveListener(SetPositionFromField_Z);

        RotationField_X.onSubmitValue.RemoveListener(SetRotationEulerFromField_X);
        RotationField_Y.onSubmitValue.RemoveListener(SetRotationEulerFromField_Y);
        RotationField_Z.onSubmitValue.RemoveListener(SetRotationEulerFromField_Z);

        ScaleField_X.onSubmitValue.RemoveListener(SetScaleFromField_X);
        ScaleField_Y.onSubmitValue.RemoveListener(SetScaleFromField_Y);
        ScaleField_Z.onSubmitValue.RemoveListener(SetScaleFromField_Z);

        ResetPositionButton.onClick.RemoveListener(ResetPositionFromButton);
        ResetRotationButton.onClick.RemoveListener(ResetRotationFromButton);
        ResetScaleButton.onClick.RemoveListener(ResetScaleFromButton);

        //_rti.onSizeChanged -= OnSizeChange;
        //_wlc.onLayoutChanged -= OnSizeChange;

        Placer.SelectedObjectChanged -= OnSelectedObjectChanged;

        //_inspector.onNewTarget -= OnNewTarget;
        //_inspector.onClearTarget -= OnClearTarget;
    }

    public void GetTransformInfo()
    {
        //Debug.Log("Updating Transform");
        if (_selTransform == null || _selObjInfo == null)
            return;

        if (_selTransform != null)
        {
            if (_selObjInfo.componentInfo_Transform.positionExposureLevel != Inspector.ExposureLevel.Hidden)
            {
                //PositionField_X.text = _selTransform.position.x.ToString();
                //PositionField_Y.text = _selTransform.position.y.ToString();
                //PositionField_Z.text = _selTransform.position.z.ToString();
                PositionField_X.SetDisplayedValue(_selTransform.position.x);
                PositionField_Y.SetDisplayedValue(_selTransform.position.y);
                PositionField_Z.SetDisplayedValue(_selTransform.position.z);
                _lastPosition = _selTransform.position;
            }
            if (_selObjInfo.componentInfo_Transform.rotationExposureLevel != Inspector.ExposureLevel.Hidden)
            {
                //RotationField_X.text = _selTransform.rotation.eulerAngles.x.ToString();
                //RotationField_Y.text = _selTransform.rotation.eulerAngles.y.ToString();
                //RotationField_Z.text = _selTransform.rotation.eulerAngles.z.ToString();

                var euler = _selTransform.rotation.eulerAngles;
                RotationField_X.SetDisplayedValue(euler.x);
                RotationField_Y.SetDisplayedValue(euler.y);
                RotationField_Z.SetDisplayedValue(euler.z);
                _lastRotation = _selTransform.rotation;
            }
            if (_selObjInfo.componentInfo_Transform.scaleExposureLevel != Inspector.ExposureLevel.Hidden)
            {
                //ScaleField_X.text = _selTransform.localScale.x.ToString();
                //ScaleField_Y.text = _selTransform.localScale.y.ToString();
                //ScaleField_Z.text = _selTransform.localScale.z.ToString();
                ScaleField_X.SetDisplayedValue(_selTransform.localScale.x);
                ScaleField_Y.SetDisplayedValue(_selTransform.localScale.y);
                ScaleField_Z.SetDisplayedValue(_selTransform.localScale.z);
                _lastScale = _selTransform.localScale;
            }

            //_selTransform.hasChanged = false;
        }

    }

    private void SetObjectPlaced()
    {
        //set the objected as placed to update any needed fields e.g. vent nodes update the vent graph

        if (_selTransform == null)
            return;

        if (_selTransform.TryGetComponent<PlacablePrefab>(out var placeable))
        {
            placeable.SetPlaced();
        }
    }

    public void SetPositionFromField_X(float value, bool val)
    {
        //float value = float.Parse(valueString);
        if (_selTransform != null) 
            _selTransform.position = new Vector3(value, _selTransform.position.y, _selTransform.position.z);

        SetObjectPlaced();
    }
    public void SetPositionFromField_Y(float value, bool val)
    {
        //float value = float.Parse(valueString);
        if (_selTransform != null) 
            _selTransform.position = new Vector3(_selTransform.position.x, value, _selTransform.position.z);

        SetObjectPlaced();
    }
    public void SetPositionFromField_Z(float value, bool val)
    {
        //float value = float.Parse(valueString);
        if (_selTransform != null) 
            _selTransform.position = new Vector3(_selTransform.position.x, _selTransform.position.y, value);

        SetObjectPlaced();
    }
    public void SetRotationEulerFromField_X(float value, bool val)
    {
        //float value = float.Parse(valueString);
        if (_selTransform != null) 
            _selTransform.eulerAngles = new Vector3(value, _selTransform.eulerAngles.y, _selTransform.eulerAngles.z);

        SetObjectPlaced();
    }
    public void SetRotationEulerFromField_Y(float value, bool val)
    {
        //float value = float.Parse(valueString);
        if (_selTransform != null) 
            _selTransform.eulerAngles = new Vector3(_selTransform.eulerAngles.x, value, _selTransform.eulerAngles.z);

        SetObjectPlaced();
    }
    public void SetRotationEulerFromField_Z(float value, bool val)
    {
        //float value = float.Parse(valueString);
        if (_selTransform != null) 
            _selTransform.eulerAngles = new Vector3(_selTransform.eulerAngles.x, _selTransform.eulerAngles.y, value);

        SetObjectPlaced();
    }
    public void SetScaleFromField_X(float value, bool val)
    {
        //float value = float.Parse(valueString);
        if (_selTransform != null) 
            _selTransform.localScale = new Vector3(value, _selTransform.localScale.y, _selTransform.localScale.z);

        SetObjectPlaced();
    }
    public void SetScaleFromField_Y(float value, bool val)
    {
        //float value = float.Parse(valueString);
        if (_selTransform != null) 
            _selTransform.localScale = new Vector3(_selTransform.localScale.x, value, _selTransform.localScale.z);

        SetObjectPlaced();
    }
    public void SetScaleFromField_Z(float value, bool val)
    {
        //float value = float.Parse(valueString);
        if (_selTransform != null) 
            _selTransform.localScale = new Vector3(_selTransform.localScale.x, _selTransform.localScale.y, value);

        SetObjectPlaced();
    }
    private void ResetPositionFromButton()
    {
        if (_selTransform != null) _selTransform.position = Vector3.zero;
    }
    private void ResetRotationFromButton()
    {
        if (_selTransform != null) _selTransform.rotation = Quaternion.identity;
        if (_selTransform != null) _selTransform.eulerAngles = Vector3.zero;
    }
    private void ResetScaleFromButton()
    {
        if (_selTransform != null) _selTransform.localScale = Vector3.one;
    }
    private void SetExposureLevel()
    {
        if (_selObjInfo == null)
        {
            HideTransformInspector();
            return;
        }

        //Transform info
        if (_selObjInfo.componentInfo_Transform.positionExposureLevel == Inspector.ExposureLevel.Hidden
            && _selObjInfo.componentInfo_Transform.rotationExposureLevel == Inspector.ExposureLevel.Hidden
            && _selObjInfo.componentInfo_Transform.scaleExposureLevel == Inspector.ExposureLevel.Hidden)
        {
            HideTransformInspector();
            return;
        }

        ShowTransformInspector();

        switch (_selObjInfo.componentInfo_Transform.positionExposureLevel)
        {
            case Inspector.ExposureLevel.Editable:
                PositionRow.SetActive(true);
                ResetPositionButton.gameObject.SetActive(true);
                //PositionField_X.gameObject.SetActive(true);
                //PositionField_Y.gameObject.SetActive(true);
                //PositionField_Z.gameObject.SetActive(true);
                //ResetPositionButton.gameObject.SetActive(true);
                PositionField_X.Interactable = true;
                PositionField_Y.Interactable = true;
                PositionField_Z.Interactable = true;

                break;
            case Inspector.ExposureLevel.Visible:
                PositionRow.SetActive(true);
                ResetPositionButton.gameObject.SetActive(false);
                //PositionField_X.gameObject.SetActive(true);
                //PositionField_Y.gameObject.SetActive(true);
                //PositionField_Z.gameObject.SetActive(true);
                //ResetPositionButton.gameObject.SetActive(false);
                PositionField_X.Interactable = false;
                PositionField_Y.Interactable = false;
                PositionField_Z.Interactable = false;

                break;
            case Inspector.ExposureLevel.Hidden:
                PositionRow.SetActive(false);
                //PositionField_X.gameObject.SetActive(false);
                //PositionField_Y.gameObject.SetActive(false);
                //PositionField_Z.gameObject.SetActive(false);
                //ResetPositionButton.gameObject.SetActive(false);

                break;
        }
        switch (_selObjInfo.componentInfo_Transform.rotationExposureLevel)
        {
            case Inspector.ExposureLevel.Editable:
                RotationRow.SetActive(true);
                ResetRotationButton.gameObject.SetActive(true);
                //RotationField_X.gameObject.SetActive(true);
                //RotationField_Y.gameObject.SetActive(true);
                //RotationField_Z.gameObject.SetActive(true);
                //ResetRotationButton.gameObject.SetActive(true);
                RotationField_X.Interactable = true;
                RotationField_Y.Interactable = true;
                RotationField_Z.Interactable = true;

                break;
            case Inspector.ExposureLevel.Visible:
                RotationRow.SetActive(true);
                ResetRotationButton.gameObject.SetActive(false);
                //RotationField_X.gameObject.SetActive(true);
                //RotationField_Y.gameObject.SetActive(true);
                //RotationField_Z.gameObject.SetActive(true);
                //ResetRotationButton.gameObject.SetActive(false);
                RotationField_X.Interactable = false;
                RotationField_Y.Interactable = false;
                RotationField_Z.Interactable = false;

                break;
            case Inspector.ExposureLevel.Hidden:
                RotationRow.SetActive(false);
                //RotationField_X.gameObject.SetActive(false);
                //RotationField_Y.gameObject.SetActive(false);
                //RotationField_Z.gameObject.SetActive(false);
                //ResetRotationButton.gameObject.SetActive(false);
                break;
        }
        switch (_selObjInfo.componentInfo_Transform.scaleExposureLevel)
        {
            case Inspector.ExposureLevel.Editable:
                ScaleRow.SetActive(true);
                ResetScaleButton.gameObject.SetActive(true);
                //ScaleField_X.gameObject.SetActive(true);
                //ScaleField_Y.gameObject.SetActive(true);
                //ScaleField_Z.gameObject.SetActive(true);
                //ResetScaleButton.gameObject.SetActive(true);
                ScaleField_X.Interactable = true;
                ScaleField_Y.Interactable = true;
                ScaleField_Z.Interactable = true;

                break;
            case Inspector.ExposureLevel.Visible:
                ScaleRow.gameObject.SetActive(true);
                ResetScaleButton.gameObject.SetActive(false);
                //ScaleField_X.gameObject.SetActive(true);
                //ScaleField_Y.gameObject.SetActive(true);
                //ScaleField_Z.gameObject.SetActive(true);
                //ResetScaleButton.gameObject.SetActive(false);
                ScaleField_X.Interactable = false;
                ScaleField_Y.Interactable = false;
                ScaleField_Z.Interactable = false;
                break;
            case Inspector.ExposureLevel.Hidden:
                ScaleRow.gameObject.SetActive(false);
                //ScaleField_X.gameObject.SetActive(false);
                //ScaleField_Y.gameObject.SetActive(false);
                //ScaleField_Z.gameObject.SetActive(false);
                //ResetScaleButton.gameObject.SetActive(false);
                break;
        }

    }

    void HideTransformInspector()
    {
        gameObject.SetActive(false);
        //_UIContainer.SetActive(false);
        //_rt.sizeDelta = new Vector2(_startSize.x, 0); //zero height to free up occupied space in the inspector layout group
        //_inspector.RebuildLayout(false);

        //if (_layoutElement != null)
        //{
        //    _layoutElement.preferredHeight = 0;
        //}

    }

    private void ShowTransformInspector()
    {
        gameObject.SetActive(true);
        //_UIContainer.SetActive(true);
        //_rt.sizeDelta = _startSize;

        //if (_layoutElement != null)
        //{
        //    _layoutElement.preferredHeight = _preferredHeight;
        //}
    }
    //void OnNewTarget()
    //{
    //   // Debug.Log("New Target Recieved: " + inspector.target);
    //    IEnumerator coroutine = GetTransformInfo();
    //    StartCoroutine(coroutine);
    //    SetExposureLevel();

    //}
    //void OnClearTarget()
    //{
    //    HideTransformInspector();
    //}
}
