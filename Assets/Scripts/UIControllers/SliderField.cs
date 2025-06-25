using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Serialization;

public enum UnitsEditor
{
    Metric,
    Imperial,
    None
}

public enum UnitType
{
    Distance,
    Mass,
    Temperature,
    Light,
    None
}

public class SliderField : MonoBehaviour
{
    public bool startEnabled = true;
    
    EventSystem eventSystem;
    [Tooltip("Set this true if using metric or imperial measurements. Used as a flag to allow for unit conversions")]
    [SerializeField] bool usingUnitOfMeasure;
    [SerializeField] UnitsEditor SliderUnitDropdown = UnitsEditor.None;
    [SerializeField] UnitType UnitType = UnitType.None;

    private bool usingMetric;


    [Header("Title")][Space(20)]

    [Tooltip("Title")]
    [FormerlySerializedAs("titleName")]
    public string TitleName;

    [Tooltip("Applied to text after title value when using metric. Leave blank if not using")]
    [SerializeField] string title_metricSuffix;

    [Tooltip("Applied to text after title value when using imperial. Leave blank if not using")]
    [SerializeField] string title_imperialSuffix;

    [Header("Anchors")][Space(20)]

    [Tooltip("Whether or not slider anchors are to be used")]
    [SerializeField] bool usingAnchors;

    [Tooltip("Applied to text after anchor value when not using units of measurement. Leave blank if not using")]
    [SerializeField] string anchor_defaultSuffix;

    [Tooltip("Applied to text after anchor value when using metric. Leave blank if not using")]
    [SerializeField] string anchor_metricSuffix;

    

    [Tooltip("Applied to text after anchor value when using imperial. Leave blank if not using")]
    [SerializeField] string anchor_imperialSuffix;

    [Header("Field")][Space(20)]

    [Tooltip("Applied to text before field value. Leave blank if not using")]
    [SerializeField] string fieldPrefix;

    [Tooltip("Applied to text after field value. Leave blank if not using")]
    [SerializeField] string fieldSuffix;


    [Header("Values and Increments")][Space(20)]

    [Tooltip("When on, fires submit values event at start for initialization")]
    [SerializeField] bool submitValuesOnStart = true;

    [Tooltip("The nearest rounding point for the slider and field values. Set to zero to disable rounding increments")]
    [SerializeField] float valueIncrement;

    [Tooltip("The nearest rounding point for anchor text and min/max values. Set to zero to disable rounding increments")]
    [SerializeField] float anchorIncrement;

    [SerializeField] RectTransform hashContainerRt;
    [SerializeField] GameObject hashPrefab;

    [Tooltip("The increments at which hash marks appear for sliders not using a unit of measurement")]
    [SerializeField] float hashIncrement_default;
    [Tooltip("The increments at which hash marks appear for metric units")]
    [SerializeField] float hashIncrement_metric;
    [Tooltip("The increments at which hash marks appear for imperial units")]
    [SerializeField] float hashIncrement_imperial;
    float currentHashIncrement;
    // unit agnostic values
    public float startValue = 0f;
    [SerializeField] float minValue = 0f;
    [SerializeField] float maxValue = 10f;
    [SerializeField] float curValue;

    public bool SubmitValueOnSliderDrag = false;

    //store values privately to prevent corruption from floating point error durring unit conversion and rounding
    float minValue_metric;
    float minValue_imperial;
    float maxValue_metric;
    float maxValue_imperial;
    float startValue_metric;
    float startValue_imperial;

    [Header("Components")][Space(20)]


    //essential internal components assigned automatically
    [SerializeField] Slider slider;
    private GameObject sliderObject;
    [SerializeField] TMP_InputField inputField;
    private GameObject inputFieldObject;
    private PointerUpEventHandler pointerUpEventHandler;

    [SerializeField] TextMeshProUGUI header_Text;
    [SerializeField] TextMeshProUGUI minAnchor_Text;
    [SerializeField] TextMeshProUGUI midAnchor_Text;
    [SerializeField] TextMeshProUGUI maxAnchor_Text;
    [SerializeField] RectTransform startLineRect;
    [SerializeField] Image startLineImage;
    [SerializeField] RectTransform handleRect;

    [SerializeField] GameObject inputFieldNavigationUp;
    [SerializeField] GameObject inputFieldNavigationDown;

    [SerializeField] Color EnabledColor = Color.white;
    [SerializeField] Color DisabledColor = Color.gray;

    [Tooltip("This event fires when the slider or the field values have been set")]
    public UnityEvent<float, bool> onSubmitValue;
    bool fieldSelected;

    const float METERS_TO_FEET = 3.28084f;
    const float KG_TO_LB = 2.20462f;
    const float LUMEN_TO_CANDELA = 1 / 12.57f;
    private bool _setExternally = false;
    private bool _initialized = false;
    private float _conversionFactor = 1;
    private bool _conversionFactorApplied = false;



    /// <summary>
    ///  Assign components
    /// </summary>
    private void Awake()
    {
        if(slider == null)
            slider = GetComponentInChildren<Slider>();
        inputField = GetComponentInChildren<TMP_InputField>();
        sliderObject = slider.gameObject;
        inputFieldObject = inputField.gameObject;
        pointerUpEventHandler = GetComponentInChildren<PointerUpEventHandler>();
        eventSystem = EventSystem.current;
        startLineImage = startLineRect.GetComponent<Image>();

        

        SetInteractableUI(startEnabled);
    }

    void Start()
    {
        //subscribe to events
        inputField.onEndEdit.AddListener(ChangeFromField);
        //inputField.onValueChanged.AddListener(ChangeFromField);
        slider.onValueChanged.AddListener(ChangeFromSlider);
        if(pointerUpEventHandler != null)
            pointerUpEventHandler.onPointerUp.AddListener(OnPointerUp);
        // store essential unit values before rounding and floating errors corrupt them
        if (usingUnitOfMeasure)
        {
            if (usingMetric)
            {
                //minValue_metric = minValue;
                //minValue_imperial = minValue * 3.28084f;
                //maxValue_metric = maxValue;
                //maxValue_imperial = maxValue * 3.28084f;
                //startValue_metric = startValue;
                //startValue_imperial = startValue * 3.28084f;
                currentHashIncrement = hashIncrement_metric;
            }
            else
            {
                //minValue_imperial = minValue;
                //minValue_metric = minValue / 3.28084f;
                //maxValue_imperial = maxValue;
                //maxValue_metric = maxValue / 3.28084f;
                //startValue_imperial = startValue;
                //startValue_metric = startValue / 3.28084f;
                currentHashIncrement = hashIncrement_imperial;
            }
        }
        else
        {
            currentHashIncrement = hashIncrement_default;
        }

        switch (UnitType)
        {
            case UnitType.Distance:
                _conversionFactor = METERS_TO_FEET;
                break;
            case UnitType.Mass:
                _conversionFactor = KG_TO_LB;
                break;
            case UnitType.Temperature:
                break;
            case UnitType.Light:
                _conversionFactor = LUMEN_TO_CANDELA;
                break;
            case UnitType.None:
                break;
            default:
                break;
        }
        _conversionFactorApplied = true;

        if (_initialized)
        {
            return;
        }

        

        switch (SliderUnitDropdown)
        {
            case UnitsEditor.Metric:
                currentHashIncrement = hashIncrement_metric;
                usingMetric = true;
                break;
            case UnitsEditor.Imperial:
                currentHashIncrement = hashIncrement_imperial;
                if (UnitType == UnitType.Temperature)
                {                    
                    minValue = (minValue * (9/5)) + 32;
                    maxValue = maxValue * (9 / 5) + 32;
                    startValue = (startValue * (9 / 5)) + 32;
                }
                else
                {
                    minValue = minValue / _conversionFactor;
                    maxValue = maxValue / _conversionFactor;
                    startValue = startValue / _conversionFactor;
                }
                usingMetric = false;
                break;
            case UnitsEditor.None:
                currentHashIncrement = hashIncrement_default;
                break;
            default:
                break;
        }

        //RoundNumbers();
        PopulateHashMarks();

        //set value limits
        slider.minValue = minValue;
        slider.maxValue = maxValue;

        

        //set start value
        if (valueIncrement > 0) 
            curValue = FloatExtensions.ToNearestMultiple(startValue, valueIncrement);
        else 
            curValue = startValue;

        //slider.value = curValue;
        slider.SetValueWithoutNotify(curValue);

        //inputField.text = fieldPrefix + curValue.ToString("") + fieldSuffix;
        inputField.text = fieldPrefix + GetValueForDisplay(curValue) + fieldSuffix;

        // position start line
        //if (startLineRect)
        //{
        //    startLineRect.anchorMin = handleRect.anchorMin;
        //    startLineRect.anchorMax = handleRect.anchorMax;
        //}





        //set anchor state
        minAnchor_Text.gameObject.SetActive(usingAnchors);
        midAnchor_Text.gameObject.SetActive(usingAnchors);
        maxAnchor_Text.gameObject.SetActive(usingAnchors);
        
        SetSupportText();

        if (submitValuesOnStart) onSubmitValue.Invoke(curValue, false);

        if (startLineRect)
        {
            startLineRect.anchorMin = handleRect.anchorMin;
            startLineRect.anchorMax = handleRect.anchorMax;
        }
    }

    public void SetInteractableUI(bool isInteractable)
    {
        slider.interactable = isInteractable;
        inputField.interactable = isInteractable;
        header_Text.color = isInteractable ? EnabledColor : DisabledColor;
        startLineImage.color = isInteractable ? EnabledColor : DisabledColor;
        minAnchor_Text.color = isInteractable ? EnabledColor : DisabledColor;
        midAnchor_Text.color = isInteractable ? EnabledColor : DisabledColor;
        maxAnchor_Text.color = isInteractable ? EnabledColor : DisabledColor;
        inputField.textComponent.color = isInteractable ? EnabledColor : DisabledColor;


        slider.gameObject.SetActive(isInteractable);
        minAnchor_Text.gameObject.SetActive(isInteractable);
        midAnchor_Text.gameObject.SetActive(isInteractable);
        maxAnchor_Text.gameObject.SetActive(isInteractable);
    }

    public void SetNewMinValue(float newMin)
    {


        //if (usingUnitOfMeasure)
        //{
        //    if (usingMetric)
        //    {
        //        minValue_metric = minValue;
        //        minValue_imperial = minValue * 3.28084f;
        //    }
        //    else
        //    {
        //        minValue_imperial = minValue;
        //        minValue_metric = minValue / 3.28084f;
        //    }
        //}
        switch (SliderUnitDropdown)
        {
            case UnitsEditor.Imperial:
                newMin = newMin / METERS_TO_FEET;
                break;
            case UnitsEditor.Metric:
            case UnitsEditor.None:
            default:
                break;
        }
        minValue = newMin;

        if (curValue < minValue) { curValue = minValue; }

        //RoundNumbers();
        PopulateHashMarks();
        SetSupportText();

        //var tempValue = curValue;

        slider.minValue = minValue;
        //slider.maxValue = maxValue;

        //curValue = tempValue;

        //submit changes
        slider.value = curValue;

        //Change this to display content as Metric/Imperial
        //inputField.text = fieldPrefix + curValue.ToString("") + fieldSuffix;
        inputField.text = fieldPrefix + GetValueForDisplay(curValue) + fieldSuffix;

        onSubmitValue.Invoke(curValue, true);
    }

    public void SetSliderValues(float newMin, float newMax, float newStart)
    {
        //if (usingUnitOfMeasure)
        //{
        //    if (usingMetric)
        //    {
        //        minValue_metric = minValue;
        //        minValue_imperial = minValue * 3.28084f;
        //    }
        //    else
        //    {
        //        minValue_imperial = minValue;
        //        minValue_metric = minValue / 3.28084f;
        //    }
        //}

        if (!_initialized)
        {
            if (usingUnitOfMeasure)
            {
                if (usingMetric)
                {
                    //minValue_metric = minValue;
                    //minValue_imperial = minValue * 3.28084f;
                    //maxValue_metric = maxValue;
                    //maxValue_imperial = maxValue * 3.28084f;
                    //startValue_metric = startValue;
                    //startValue_imperial = startValue * 3.28084f;
                    currentHashIncrement = hashIncrement_metric;
                }
                else
                {
                    //minValue_imperial = minValue;
                    //minValue_metric = minValue / 3.28084f;
                    //maxValue_imperial = maxValue;
                    //maxValue_metric = maxValue / 3.28084f;
                    //startValue_imperial = startValue;
                    //startValue_metric = startValue / 3.28084f;
                    currentHashIncrement = hashIncrement_imperial;
                }
            }
            else
            {
                currentHashIncrement = hashIncrement_default;
            }
        }
        
        minValue = newMin;
        maxValue = newMax;
        startValue = newStart;

        slider.minValue = minValue;
        slider.maxValue = maxValue;
        
        SetCurrentValue(newStart, false);
        _initialized = true;
    }

    public void SetCurrentValue(float val, bool notify = false)
    {
        //switch (SliderUnitDropdown)
        //{
        //    case SliderUnitEditor.Imperial:
        //        val = val / METERS_TO_FEET;
        //        break;
        //    case SliderUnitEditor.Metric:
        //    case SliderUnitEditor.None:
        //    default:
        //        break;
        //}
        curValue = val;
        //if (curValue < minValue) { curValue = minValue; }

        if (slider == null)
        {
            startValue = val;
            return;
        }

        //RoundNumbers();
        PopulateHashMarks();
        SetSupportText();

        //Debug.Log($"Slider: {slider}");
        //slider.minValue = minValue;
        //slider.maxValue = maxValue;

        //submit changes
        slider.SetValueWithoutNotify(curValue);

        //inputField.text = fieldPrefix + curValue.ToString("") + fieldSuffix;
        //Debug.Log($"Setting {header_Text.text} to {GetValueForDisplay(curValue)}");
        inputField.text = fieldPrefix + GetValueForDisplay(curValue) + fieldSuffix;
        //Debug.Log($"{header_Text.text} was provided {curValue} should display {GetValueForDisplay(curValue)} but is actually showing {inputField.text}");
        startLineRect.anchorMin = handleRect.anchorMin;
        startLineRect.anchorMax = handleRect.anchorMax;
        if(notify)
            onSubmitValue.Invoke(curValue, true);
    }

    public void SetHashIncrement(float increment)
    {
        

        currentHashIncrement = increment;
    }

    //private void Update()
    //{
    //    if (eventSystem.sendNavigationEvents == true) UpdateNavigation();
    //}

    /// <summary>
    /// Unsubscribe from events
    /// </summary>
    void OnDestroy()
    {
        inputField.onEndEdit.RemoveListener(ChangeFromField);
        //inputField.onValueChanged.RemoveListener(ChangeFromField);
        slider.onValueChanged.RemoveListener(ChangeFromSlider);
        if(pointerUpEventHandler != null)
            pointerUpEventHandler.onPointerUp.RemoveListener(OnPointerUp);
    }

    
    //void UpdateNavigation()
    //{
    //    // Use tab to swap between slider and input
    //    if (Input.GetKeyDown(KeyCode.Tab))
    //    {
    //        GameObject currentSelected = eventSystem.currentSelectedGameObject;
    //        if (currentSelected == inputFieldObject)
    //        {
    //            // set to slider
    //            eventSystem.SetSelectedGameObject(sliderObject, new BaseEventData(eventSystem));
    //        }
    //        else if (currentSelected == sliderObject)
    //        {
    //            eventSystem.SetSelectedGameObject(inputFieldObject, new BaseEventData(eventSystem));
    //            inputField.OnPointerClick(new PointerEventData(eventSystem));
    //        }
    //    }

    //    // input fields don't navigate unless submitted, so implement an override
    //    if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) && fieldSelected && inputFieldObject == eventSystem.currentSelectedGameObject)
    //    {
    //        IEnumerator coroutine = MoveDown();
    //        StartCoroutine(coroutine);


    //    }
    //    else if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && fieldSelected && inputFieldObject == eventSystem.currentSelectedGameObject)
    //    {
    //        IEnumerator coroutine = MoveUp();
    //        StartCoroutine(coroutine);


    //    }
    //    if (!fieldSelected && inputFieldObject == eventSystem.currentSelectedGameObject)
    //    {
    //        IEnumerator coroutine = MarkSelected();
    //        StartCoroutine(coroutine);
    //    }
    //    else if (fieldSelected && inputFieldObject != eventSystem.currentSelectedGameObject)
    //    {
    //        fieldSelected = false;
    //    }
    //}
    
    /// <summary>
    /// Assign to input field event to change slider value before submiting
    /// </summary>
    /// <param name="value"></param>
    public void ChangeFromField(string value)
    {
        float _lastCurValue = curValue;

        if(!float.TryParse(value, out var convertedValue))
        {
            Debug.LogError($"Bad entry value in SliderField!");
        }
        switch (SliderUnitDropdown)
        {
            
            case UnitsEditor.Imperial:
                switch (UnitType)
                {
                    case UnitType.Distance:
                        convertedValue = convertedValue / METERS_TO_FEET;
                        break;
                    case UnitType.Mass:
                        convertedValue = convertedValue / KG_TO_LB;
                        break;
                    case UnitType.Temperature:
                        convertedValue = (convertedValue - 32) * (5 / 9);
                        break;
                    case UnitType.Light:
                        convertedValue = convertedValue / LUMEN_TO_CANDELA;
                        break;
                    case UnitType.None:
                        break;
                    default:
                        break;
                }
                
                break;
            case UnitsEditor.Metric:
            case UnitsEditor.None:
            default:
                break;
        }

        //curValue = float.Parse(value);
        curValue = convertedValue;

        // validate change actually occured
        if (curValue == _lastCurValue) return;
        
        // set limits
        curValue = Mathf.Clamp(curValue, minValue, maxValue);

        //round value to increment
        if (valueIncrement > 0) curValue = Mathf.Clamp(FloatExtensions.ToNearestMultiple(curValue, valueIncrement),minValue,maxValue);

        // apply value change after adjusment
        //inputField.text = fieldPrefix + curValue.ToString() + fieldSuffix;
        inputField.text = fieldPrefix + GetValueForDisplay(curValue) + fieldSuffix;
        slider.value = curValue;

        // when submitting from field, we can submit without confirmation
        Debug.Log("Change from field");
        onSubmitValue.Invoke(curValue, true);
    }

    /// <summary>
    /// Assign to slider event to change field value before submiting
    /// </summary>
    /// <param name="value"></param>
    public void ChangeFromSlider(float value)
    {
        float _lastCurValue = curValue;
        curValue = value;

        // validate change actually occured
        if (curValue == _lastCurValue && !slider.interactable) return;

        // set limits
        curValue = Mathf.Clamp(curValue, minValue, maxValue);

        // round value to increment
        if (valueIncrement > 0) curValue = Mathf.Clamp(FloatExtensions.ToNearestMultiple(curValue, valueIncrement), minValue, maxValue);

        // apply value change after adjusment
        //inputField.text = fieldPrefix + curValue.ToString("") + fieldSuffix;
        inputField.text = fieldPrefix + GetValueForDisplay(curValue) + fieldSuffix;
        slider.value = curValue;
        
        // if using keys to change slider, submit changes 
        if (sliderObject == eventSystem.currentSelectedGameObject && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))) onSubmitValue.Invoke(slider.value, true);

        //Debug.Log("Change from slider");
        //changes from slider can occur rapidly, so submitting is handled seperately onCursorUp to prevent spamming changes
        if (SubmitValueOnSliderDrag)
            onSubmitValue?.Invoke(curValue, true);
    }

    /// <summary>
    /// Converts directly from feet or meters
    /// </summary>
    /// <param name="usingMetric"></param>
    public void ChangeUnitOfMeasure(bool _useMetric)
    {
        // don't allow conversion to same unit type, or non unit
        if (_useMetric == usingMetric || !usingUnitOfMeasure) return;
        
        usingMetric = _useMetric;
        
        //minValue = 
        if (usingMetric)
        {
            //minValue = minValue_metric;
            //maxValue = maxValue_metric;
            //startValue = startValue_metric;
            // rounding twice can move the value. Prevent this from happening if at start value
            //if (curValue == startValue_imperial.ToNearestMultiple(valueIncrement)) curValue = startValue_metric;
            //else curValue /= 3.28084f;
            currentHashIncrement = hashIncrement_metric;
            SliderUnitDropdown = UnitsEditor.Metric;
        }
        else // using feet
        {
            
            //minValue = minValue_imperial;
            //maxValue = maxValue_imperial;
            //startValue = startValue_imperial;
            //// rounding twice can move the value. Prevent this from happening if at start value
            //if (curValue == startValue_metric.ToNearestMultiple(valueIncrement)) curValue = startValue_imperial;
            //else curValue *= 3.28084f;
            currentHashIncrement = hashIncrement_imperial;
            SliderUnitDropdown = UnitsEditor.Imperial;
        }

        //RoundNumbers();
        PopulateHashMarks();
        SetSupportText();

        //set value limits
        //changing slider range will impact cur value, so cache and set after slider change
        //var tempValue = curValue;

        //slider.minValue = minValue;
        //slider.maxValue = maxValue;

        //curValue = tempValue;

        ////submit changes
        //slider.value = curValue;
        ////inputField.text = fieldPrefix + curValue.ToString("") + fieldSuffix;
        inputField.text = fieldPrefix + GetValueForDisplay(curValue) + fieldSuffix;
        //Debug.Log("submit value metric change to " + curValue);
        //onSubmitValue.Invoke(curValue, false);
    }
   
    /// <summary>
    /// Reset values to a start value based on unit of measure
    /// </summary>
    public void ResetValues()
    {

        curValue = startValue;

        //submit changes
        slider.value = curValue;
        //inputField.text = fieldPrefix + curValue.ToString("") + fieldSuffix;
        inputField.text = fieldPrefix + GetValueForDisplay(curValue) + fieldSuffix;
        //Debug.Log("submit value metric change to " + curValue);
        onSubmitValue.Invoke(curValue, true);
    }

    public void ForceValue(float value)
    {
        startValue = value;
        ResetValues();
    }

    /// <summary>
    /// Helper function to round essential values all at once. Done at start and during unit conversion.
    /// </summary>
    void RoundNumbers()
    {

        if (anchorIncrement > 0)
        {
            minValue = FloatExtensions.ToNearestMultiple(minValue, anchorIncrement);
            maxValue = FloatExtensions.ToNearestMultiple(maxValue, anchorIncrement);
        }
        if (valueIncrement > 0)
        {
            startValue = Mathf.Clamp(FloatExtensions.ToNearestMultiple(startValue, valueIncrement), minValue, maxValue);
            curValue = Mathf.Clamp(FloatExtensions.ToNearestMultiple(curValue, valueIncrement), minValue, maxValue);
        }
    }

    /// <summary>
    /// Set the text for the header and anchors
    /// </summary>
    void SetSupportText()
    {
        
        string anchorSuffix = ("");
        string titleSuffix = ("");

        
        // set unit string and symbol
        if (!usingUnitOfMeasure)
        {
            titleSuffix = "";
            anchorSuffix = anchor_defaultSuffix;
        }
        else if (usingMetric)
        {
            titleSuffix = title_metricSuffix;
            anchorSuffix = anchor_metricSuffix;
        }
        else
        {
            titleSuffix = title_imperialSuffix;
            anchorSuffix = anchor_imperialSuffix;
        }

        // set anchor values
        if (usingAnchors)
        {
            float midValue = ((maxValue - minValue) / 2) + minValue;
            midValue = FloatExtensions.ToNearestMultiple(midValue, anchorIncrement);

            //Changing this to display unit specfic values
            //string minValueString = minValue.ToString();
            //string midValueString = midValue.ToString();
            //string maxValueString = maxValue.ToString();
            string minValueString = GetValueForDisplay(minValue);
            string midValueString = GetValueForDisplay(midValue);
            string maxValueString = GetValueForDisplay(maxValue);
            minAnchor_Text.text = (minValueString + " " + anchorSuffix);
            midAnchor_Text.text = (midValueString + " " + anchorSuffix);
            maxAnchor_Text.text = (maxValueString + " " + anchorSuffix);
        }

        // set header
        header_Text.text = (TitleName + " " + titleSuffix);
    }
    
    /// <summary>
    /// Changes from slider can occur rapidly, so submitting is handled seperately onCursorUp to prevent spamming changes which will hurt performance
    /// </summary>
    void OnPointerUp()
    {
        //Debug.Log("Change from slider");
        if (!slider.interactable)
        {
            return;
        }
        onSubmitValue.Invoke(slider.value, true);
    }

    public void UpdateHashMarks()
    {
        PopulateHashMarks();
    }

    void PopulateHashMarks()
    {
        if (currentHashIncrement <= 0) return;

        if (hashContainerRt.childCount > 0) foreach (Transform child in hashContainerRt) Destroy(child.gameObject);

        float valueRange = maxValue - minValue;
        float numberOfHashMarksF;
        //switch (SliderUnitDropdown)
        //{
        //    case SliderUnitEditor.Metric:
        //        numberOfHashMarksF = valueRange / currentHashIncrement;
        //        break;
        //    case SliderUnitEditor.Imperial:
        //        numberOfHashMarksF = valueRange / (currentHashIncrement / METERS_TO_FEET);
        //        break;
        //    case SliderUnitEditor.None:
        //        numberOfHashMarksF = valueRange / currentHashIncrement;
        //        break;
        //    default:
        //        numberOfHashMarksF = valueRange / currentHashIncrement;
        //        break;
        //}
        if (usingUnitOfMeasure)
        {
            numberOfHashMarksF = valueRange / (usingMetric ? currentHashIncrement : currentHashIncrement / METERS_TO_FEET);
        }
        else
        {
            numberOfHashMarksF = valueRange / currentHashIncrement;
        }
        int numberOfHashMarks = (int)numberOfHashMarksF;
        float relativeIncrement = hashContainerRt.rect.width / numberOfHashMarksF;
        float hashPlacement = minValue;


        for (int i = 0; i < numberOfHashMarks; i++) 
        {
            // start from minimum and round up
            if (i == 0)
            {
                hashPlacement = minValue.ToNearestMultiple(relativeIncrement, FloatExtensions.ROUNDING.UP);
            }
            else
            {
                hashPlacement += relativeIncrement;
                //hashPlacement = hashContainerRt.rect.width / hashPlacement;
            }

            GameObject hash = Instantiate(hashPrefab, hashContainerRt);
            hash.GetComponent<RectTransform>().anchoredPosition = new Vector3(hashPlacement,-17,0);
        }
    }

    //IEnumerator MoveUp()
    //{
    //    yield return 0;


    //    if (inputFieldNavigationUp != null)
    //    {
    //        InputField _inputfield = inputFieldNavigationUp.GetComponent<InputField>();
    //        if (_inputfield != null) _inputfield.OnPointerClick(new PointerEventData(eventSystem));  //if it's an input field, also set the text caret

    //        eventSystem.SetSelectedGameObject(inputFieldNavigationUp.gameObject, new BaseEventData(eventSystem));
    //        Debug.Log("move up");
            
    //    }
    //    yield break;
    //}
    //IEnumerator MoveDown()
    //{
    //    yield return 0;
    //    if (inputFieldNavigationDown != null)
    //    {
    //        InputField _inputfield = inputFieldNavigationDown.GetComponent<InputField>();
    //        if (_inputfield != null) _inputfield.OnPointerClick(new PointerEventData(eventSystem));  //if it's an input field, also set the text caret
    //        {
    //            eventSystem.SetSelectedGameObject(inputFieldNavigationDown.gameObject, new BaseEventData(eventSystem));
    //            Debug.Log("move down");
    //        }
    //    }
    //    yield break;
    //}

    //IEnumerator MarkSelected()
    //{
    //    yield return null;
        
    //    if(inputFieldObject != eventSystem.currentSelectedGameObject) yield break;
        
    //    fieldSelected = true;

    //    yield break;
    //}

    public float GetMin()
    {
        return minValue;
    }
    public float GetMax()
    {
        return maxValue;
    }
    public float GetStart()
    {
        return startValue;
    }
    public float GetCurrentValue()
    {
        return curValue;
    }

    string GetValueForDisplay(float val)
    {
        if (!_conversionFactorApplied)
        {
            switch (UnitType)
            {
                case UnitType.Distance:
                    _conversionFactor = METERS_TO_FEET;
                    break;
                case UnitType.Mass:
                    _conversionFactor = KG_TO_LB;
                    break;
                case UnitType.Temperature:
                    break;
                case UnitType.Light:
                    _conversionFactor = LUMEN_TO_CANDELA;
                    break;
                case UnitType.None:
                    break;
                default:
                    break;
            }
            _conversionFactorApplied = true;
        }
        if (usingUnitOfMeasure)
        {
            if (!usingMetric)
            {
                if (UnitType == UnitType.Temperature)
                {
                    float convert = Mathf.Round((val * 9/5) + 32);
                    return convert.ToString(convert % 1 == 0 ? "0" : "0.#");
                }
                else
                {
                    float convert = Mathf.Round(val * _conversionFactor);
                    return convert.ToString(convert % 1 == 0 ? "0" : "0.#");
                }
            }
        }
        return val.ToString(val % 1 == 0 ? "0" : "0.##");
    }
}
    

