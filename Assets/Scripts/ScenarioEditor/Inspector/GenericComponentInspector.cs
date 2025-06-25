using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;

public class GenericComponentInspector : MonoBehaviour, IComponentInspector
{
    public TMP_Text TitleText;
    public Transform ControlContainer;

    public GameObject SliderFieldPrefab;
    public GameObject ToggleFieldPrefab;
    public GameObject StringFieldPrefab;
    public GameObject FloatFieldPrefab;
    public GameObject ColorFieldPrefab;
    public GameObject MaterialIDPrefab;

    private ModularComponentInfo _component;
    private IInspectableComponent _inspect;

    public void SetInspectedComponent(ModularComponentInfo component)
    {
        _component = component;
        _inspect = component as IInspectableComponent;

        BuildInspector();
    }

    void Start()
    {
        if (_component != null)
            BuildInspector();
    }

    void BuildInspector()
    {
        if (_component == null)
            return;

        if (ControlContainer == null)
            ControlContainer = transform;

        SetTitle();
        ClearControls();
        AddControls();
    }

    private void SetTitle()
    {
        if (TitleText == null)
            return;

        if (_inspect != null)
            TitleText.text = _inspect.ComponentInspectorTitle;
        else
            TitleText.text = _component.name;
    }

    private void ClearControls()
    {
        if (ControlContainer == null)
            return;

        foreach (Transform child in ControlContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private delegate void SetDelegate<T>(T val);
    private delegate T GetDelegate<T>();

    private void CreateDelegates<T>(PropertyInfo prop, out SetDelegate<T> setDel, out GetDelegate<T> getDel)
    {
        var getMethod = prop.GetGetMethod();
        var setMethod = prop.GetSetMethod();

        setDel = (SetDelegate<T>)System.Delegate.CreateDelegate(typeof(SetDelegate<T>), _component, setMethod);
        getDel = (GetDelegate<T>)System.Delegate.CreateDelegate(typeof(GetDelegate<T>), _component, getMethod);
    }

    private GameObject InstantiateControlPrefab(GameObject prefab, string tooltip)
    {
        var compInsp = Instantiate<GameObject>(prefab);
        compInsp.transform.SetParent(ControlContainer, false);

        if (compInsp.TryGetComponent<MenuTooltip>(out var menuTooltip))
        {
            menuTooltip.TooltipTextString = tooltip;
        }
        else if (!string.IsNullOrEmpty(tooltip))
        {
            menuTooltip = compInsp.AddComponent<MenuTooltip>();
            menuTooltip.TooltipTextString = tooltip;
        }

        return compInsp;
    }


    private void AddControls()
    {
        var t = _component.GetType();
        Debug.Log($"Adding inspector controls for type {t.Name} component {_component.name}");

        var fieldEnabled = _component as IInspectableFieldEnabled;

        var props = t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var prop in props)
        {
            var attrib = System.Attribute.GetCustomAttribute(prop, typeof(InspectablePropertyAttribute)) as InspectablePropertyAttribute;
            if (attrib == null)
                continue;

            if (fieldEnabled != null)
            {
                if (!fieldEnabled.IsFieldInspectorEnabled(prop.Name))
                    continue;
            }

            if (attrib is InspectableNumericPropertyAttribute)
                CreateNumericSliderControl(prop, (InspectableNumericPropertyAttribute)attrib);
            else if (attrib is InspectableBoolPropertyAttribute)
                CreateBoolControl(prop, (InspectableBoolPropertyAttribute)attrib);
            else if (attrib is InspectableStringPropertyAttribute)
                CreateStringControl(prop, (InspectableStringPropertyAttribute)attrib);
            else if (attrib is InspectableColorPropertyAttribute)
                CreateColorControl(prop, (InspectableColorPropertyAttribute)attrib);
            else if (attrib is InspectableMaterialIDPropertyAttribute)
                CreateMaterialIDControl(prop, (InspectableMaterialIDPropertyAttribute)attrib);
        }
    }

    private void CreateStringControl(PropertyInfo prop, InspectableStringPropertyAttribute attrib)
    {
        CreateDelegates<string>(prop, out var setDel, out var getDel);

        var compInsp = InstantiateControlPrefab(StringFieldPrefab, attrib.Tooltip);

        var stringField = compInsp.GetComponent<StringField>();

        stringField.SetLabelText(attrib.DisplayName);
        stringField.SetValueWithoutNotify(getDel());

        stringField.ValueChanged += (val) =>
        {
            setDel(val);
        };
    }

    private void CreateColorControl(PropertyInfo prop, InspectableColorPropertyAttribute attrib)
    {
        CreateDelegates<Color>(prop, out var setDel, out var getDel);

        var compInsp = InstantiateControlPrefab(ColorFieldPrefab, attrib.Tooltip);

        var field = compInsp.GetComponent<ColorField>();

        field.SetLabelText(attrib.DisplayName);
        field.SetColorWithoutNotify(getDel());

        field.ColorChanged += (val) =>
        {
            setDel(val);
        };
    }

    private void CreateBoolControl(PropertyInfo prop, InspectableBoolPropertyAttribute attrib)
    {
        CreateDelegates<bool>(prop, out var setDel, out var getDel);

        var compInsp = InstantiateControlPrefab(ToggleFieldPrefab, attrib.Tooltip);

        var toggleSwitch = compInsp.GetComponent<ToggleSwitch>();

        toggleSwitch.SetLabelText(attrib.DisplayName);
        if (!string.IsNullOrWhiteSpace(attrib.OnText))
            toggleSwitch.OnStateText = attrib.OnText;
        if (!string.IsNullOrWhiteSpace(attrib.OffText))
            toggleSwitch.OffStateText = attrib.OffText;

        toggleSwitch.ToggleWithoutNotify(getDel());

        toggleSwitch.ToggleControl.onValueChanged.AddListener((toggleState) =>
        {
            setDel(toggleState);
        });
        
    }


    private void CreateNumericSliderControl(PropertyInfo prop, InspectableNumericPropertyAttribute attrib)
    {
        switch (System.Type.GetTypeCode(prop.PropertyType))
        {
            case System.TypeCode.Double:
                CreateNumericSliderControlDouble(prop, attrib);
                break;

            case System.TypeCode.Single:
                CreateNumericSliderControlFloat(prop, attrib);
                break;
        }
    }

    private SliderField CreateNumericSliderControl(InspectableNumericPropertyAttribute attrib, float curValue)
    {
        var compInsp = InstantiateControlPrefab(SliderFieldPrefab, attrib.Tooltip);
        var slider = compInsp.GetComponent<SliderField>();

        slider.TitleName = attrib.DisplayName;
        slider.startValue = curValue;
        slider.SetSliderValues(attrib.MinValue, attrib.MaxValue, curValue);        

        return slider;
    }

    private void CreateNumericSliderControlFloat(PropertyInfo prop, InspectableNumericPropertyAttribute attrib)
    {
        CreateDelegates<float>(prop, out var setDel, out var getDel);

        float curValue = ConvertToDisplayUnits(getDel(), attrib);
        var slider = CreateNumericSliderControl(attrib, curValue);

        slider.onSubmitValue.AddListener((val, b) =>
        {
            val = ConvertFromDisplayUnits(val, attrib);
            setDel(val);
        });
    }

    private void CreateNumericSliderControlDouble(PropertyInfo prop, InspectableNumericPropertyAttribute attrib)
    {
        CreateDelegates<double>(prop, out var setDel, out var getDel);

        float curValue = (float)getDel();
        var slider = CreateNumericSliderControl(attrib, curValue);
        
        slider.onSubmitValue.AddListener((val, b) =>
        {
            val = ConvertFromDisplayUnits(val, attrib);
            setDel(val);
        });
    }

    private void CreateMaterialIDControl(PropertyInfo prop, InspectableMaterialIDPropertyAttribute attrib)
    {
        CreateDelegates<string>(prop, out var setDel, out var getDel);

        var compInsp = InstantiateControlPrefab(MaterialIDPrefab, attrib.Tooltip);

        var header = compInsp.transform.Find("Header_TMP");
        if (header != null && header.TryGetComponent<TMP_Text>(out var headerText))
        {
            headerText.text = attrib.DisplayName;
        }

        var dropdown = compInsp.GetComponentInChildren<UIDropdownMaterialID>();

        if (dropdown != null)
        {
            dropdown.SetSelected(getDel());
            dropdown.ValueChanged += (val) =>
            {
                setDel(val);
            };
        }
    }

    private static float ConvertToDisplayUnits(float val, InspectableNumericPropertyAttribute attrib)
    {
        switch (attrib.Units)
        {
            case NumericPropertyUnitType.Ratio:
                val = val * 100.0f;
                return Mathf.Clamp(val, 0, 100);
            default:
                return val;
        }
    }

    private static float ConvertFromDisplayUnits(float val, InspectableNumericPropertyAttribute attrib)
    {
        switch (attrib.Units)
        {
            case NumericPropertyUnitType.Ratio:
                val = (val - attrib.MinValue) / (attrib.MaxValue - attrib.MinValue);
                return Mathf.Clamp(val, 0, 1);
            default:
                return val;
        }
    }
}
