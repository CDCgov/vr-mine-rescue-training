using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class InspectorField : MonoBehaviour, IDragHandler
{
    EventSystem eventSystem;
    bool fieldSelected;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TextMeshProUGUI headerField;
    [SerializeField] GameObject inputFieldNavigationUp;
    [SerializeField] GameObject inputFieldNavigationDown;

    [SerializeField] GameObject inputFieldNavigationLeft;
    [SerializeField] GameObject inputFieldNavigationRight;


    [SerializeField] bool _clampRange;

    [SerializeField] float _minValue;
    [SerializeField] float _maxValue;
    [SerializeField] float _startValue;
    float _curValue;
    [SerializeField] bool _usePrefixSuffix;
    [SerializeField] string _fieldPrefix;
    [SerializeField] string _fieldSuffix;
    [SerializeField] string _HeaderText;


    public UnityEvent<float, bool> onSubmitValue;

    [SerializeField] bool _allowDrag = true;
    [SerializeField]float _valueIncrement = 0;

    public string NumericFormatString = "F3";

    public bool Interactable
    {
        get => inputField.interactable;
        set => inputField.interactable = value;
    }

    //tab to navigate between
    private void Awake()
    {
        eventSystem = EventSystem.current;
        if (inputField == null) inputField = GetComponent<TMP_InputField>();

        if (inputField != null) inputField.onEndEdit.AddListener(ChangeFromField);
        //inputField.text = _fieldPrefix + _startValue.ToString() + _fieldSuffix;
        //_curValue = _startValue;
        SetDisplayedValue(_startValue);
        Debug.Log($"Performing start value of inspector field {_curValue}");
    }
    //private void Start()
    //{
        
    //}
    private void OnDestroy()
    {
        if(inputField)inputField.onEndEdit.RemoveListener(ChangeFromField);
    }
    //private void Update()
    //{
    //    //if (eventSystem.sendNavigationEvents == true) UpdateNavigation();
    //}


    public float GetCurrentValue()
    {
        return _curValue;
    }

    public void SetDisplayedValue(float value)
    {
        if (inputField == null)
            return;

        _curValue = value;
        _startValue = _curValue;

        inputField.text = _curValue.ToString(NumericFormatString);
    }

    public void SetDisplayedValue(float value, string headerPrefix = "", string headerSuffix = "")
    {
        if (inputField == null)
            return;
        Debug.Log($"Setting displayed value? {value} with header? {headerSuffix}");
        _curValue = value;
        _startValue = _curValue;

        if (_usePrefixSuffix)
            inputField.text = _fieldPrefix + _curValue.ToString(NumericFormatString) + _fieldSuffix;
        else
            inputField.text = _curValue.ToString(NumericFormatString);

        headerField.text = headerPrefix + _HeaderText + headerSuffix;         
    }

    //void UpdateNavigation()
    //{
    //    GameObject currentSelected = eventSystem.currentSelectedGameObject;
    //    bool selected = currentSelected == this.gameObject;


    //    if (selected && fieldSelected)
    //    {
    //        if (Input.GetKeyDown(KeyCode.Tab))
    //        {
    //            if (Input.GetKey(KeyCode.LeftShift) && inputFieldNavigationLeft != null)
    //            {
    //                IEnumerator coroutine = Move(inputFieldNavigationLeft);
    //                StartCoroutine(coroutine);
    //            }
    //            else if (inputFieldNavigationRight != null)
    //            {
    //                IEnumerator coroutine = Move(inputFieldNavigationRight);
    //                StartCoroutine(coroutine);
    //            }
    //        }

    //        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) && inputFieldNavigationUp != null)
    //        {
    //            IEnumerator coroutine = Move(inputFieldNavigationUp);
    //            StartCoroutine(coroutine);
    //        }

    //        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) && inputFieldNavigationDown != null)
    //        {

    //            IEnumerator coroutine = Move(inputFieldNavigationDown);
    //            StartCoroutine(coroutine);
    //        }

    //    }

    //    if (!fieldSelected && selected)
    //    {
    //        IEnumerator coroutine = MarkSelected();
    //        StartCoroutine(coroutine);
    //    }

    //    else if (fieldSelected && selected)
    //    {
    //        fieldSelected = false;
    //    }
    //}

    //IEnumerator Move(GameObject target)
    //{
    //    yield return 0;

    //    if (target != null)
    //    {
    //        if (target.TryGetComponent(out InputField _field)) { _field.OnPointerClick(new PointerEventData(eventSystem)); }//if it's an input field, also set the text caret
    //        eventSystem.SetSelectedGameObject(target, new BaseEventData(eventSystem));
    //        //Debug.Log("move to " + target);
    //    }
    //    yield break;
    //}

    //IEnumerator MarkSelected()
    //{
    //    yield return null;

    //    if (gameObject != eventSystem.currentSelectedGameObject) yield break;

    //    fieldSelected = true;

    //    yield break;
    //}

    public void OnDrag(PointerEventData eventData)
    {
        if (inputField.interactable && _allowDrag)
        {
            //get parsed string data from field
            float parsedValue = float.Parse(inputField.text);
            parsedValue += (eventData.delta.x * 0.01f);
            //inputField.text = parsedValue.ToString();
            //inputField.onEndEdit.Invoke(inputField.text);
            ChangeFromField(parsedValue);
        }
    }

    public void ChangeFromField(string value)
    {
        var floatVal = float.Parse(value);
        ChangeFromField(floatVal);
    }

    public void ChangeFromField(float value)
    {
        //Debug.Log("Change from field :" + value);
        // validate change actually occured
        if (_curValue == value) 
            return;

        _curValue = value;

        // set limits
        if (_clampRange) 
            _curValue = Mathf.Clamp(_curValue, _minValue, _maxValue);

        //round value to increment
        if (_valueIncrement > 0 && _clampRange)
        {
            _curValue = Mathf.Clamp(FloatExtensions.ToNearestMultiple(_curValue, _valueIncrement), _minValue, _maxValue);
        }
        else if (_valueIncrement > 0 && !_clampRange)
        {
            _curValue = FloatExtensions.ToNearestMultiple(_curValue, _valueIncrement);
        }

        // apply value change after adjusment
        //if (_usePrefixSuffix)
        //    inputField.text = _fieldPrefix + _curValue.ToString() + _fieldSuffix;
        //else 
        //    inputField.text = _curValue.ToString();
        SetDisplayedValue(_curValue);

        // when submitting from field, we can submit without confirmation
        //Debug.Log("Change from field");
        onSubmitValue.Invoke(_curValue, true);

    }

    
}


