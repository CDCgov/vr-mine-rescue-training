using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridPropertiesManager : MonoBehaviour
{

    float _gridSize;
    Color _gridColor;
    float _gridTransformXSize;
    float _gridTransformZSize;
    Material _gridMat;
    private const float FeetToMetersConversion = 0.3048f;
    private const float FeetToCentimetersConversion = 30.48f;

    Transform _gridTransform;
    public Transform _yLineTransform;
    public Transform _xAxisLineTransform;
    public Transform _zAxisLineTransform;

    public InspectorField _gridSpacingInspector;
    public InspectorField _gridLengthInspector;
    public InspectorField _gridWidthInspector;
    public Button _menuSaveButton;
    public MenuColorPicker _menuColorPicker;
    public string UnitSuffixMetricDistance = " meters";
    public string UnitSuffixImperialDistance = " feet";
    public string UnitSuffixMetricSpacing = " cm";
    public string UnitSuffixImperialSpacing = " ft";

    private Vector3 _initialScale;
    private float _conversionFactorDistance = 1;
    private float _conversionFactorSpacing = 100;
    private string _suffixDistance = "";
    private string _suffixSpacing = "";
    private bool _isMetric = false;
    private float _cachedWidth;
    private float _cachedLength;
    private float _cachedSpacing;
    private Color _cachedColor;
    private SystemManager SystemManager;
    private SystemConfig SystemConfig;
    

    private void Awake()
    {
        SystemManager = SystemManager.GetDefault();
        SystemConfig = SystemManager.SystemConfig;
        SetUnit(_isMetric);
        SetCurrentValues();        
        SubscribeToUIEvents();
        _cachedLength = _gridTransformXSize;
        _cachedWidth = _gridTransformZSize;
        _cachedSpacing = _gridSize;
        _cachedColor = _gridMat.GetColor("_Line_color");

        UpdateGrid();
    }

    void SubscribeToUIEvents()
    {
        _gridSpacingInspector.onSubmitValue.AddListener(ChangeGridCellSpacing);
        _gridLengthInspector.onSubmitValue.AddListener(ChangeGridSizeX);
        _gridWidthInspector.onSubmitValue.AddListener(ChangeGridSizeZ);
        _menuColorPicker.onColorSaved += ChangeGridColor;
    }

    private void OnDestroy()
    {
        if (_gridSpacingInspector != null)
        {
            _gridSpacingInspector.onSubmitValue.RemoveListener(ChangeGridCellSpacing);
        }
        if (_gridLengthInspector != null)
        {
            _gridLengthInspector.onSubmitValue.RemoveListener(ChangeGridSizeX);
        }
        if (_gridWidthInspector != null)
        {
            _gridWidthInspector.onSubmitValue.RemoveListener(ChangeGridSizeZ);
        }
        if (_menuColorPicker != null)
        {
            _menuColorPicker.onColorSaved -= ChangeGridColor;
        }


    }

    
    public void SetUnit(bool useMetric)
    {       
        _isMetric = useMetric;
        if (_isMetric)
        {
            _conversionFactorDistance = 1;
            _conversionFactorSpacing = 100;
            _suffixDistance = UnitSuffixMetricDistance;
            _suffixSpacing = UnitSuffixMetricSpacing;
        }
        else
        {
            _conversionFactorDistance = FeetToMetersConversion;
            _conversionFactorSpacing = FeetToCentimetersConversion;
            _suffixDistance = UnitSuffixImperialDistance;
            _suffixSpacing = UnitSuffixImperialSpacing;
        }
        //SetCurrentValues();
        _gridSpacingInspector.SetDisplayedValue(_gridSize / _conversionFactorSpacing, "", _suffixSpacing);
        _gridLengthInspector.SetDisplayedValue((_gridTransformXSize * 10) / _conversionFactorDistance, "", _suffixDistance);
        _gridWidthInspector.SetDisplayedValue((_gridTransformXSize * 10) / _conversionFactorDistance, "", _suffixDistance);
    }

    void SetCurrentValues()
    {
        _gridTransform = transform;
        _initialScale = transform.localScale;
        _gridMat = GetComponent<MeshRenderer>().material;


        //_gridTransformXSize = _gridTransform.localScale.x;
        //_gridTransformZSize = _gridTransform.localScale.z;
        //_gridSize = _gridMat.GetFloat("_grid_size_cm");
        //_gridColor = _gridMat.GetColor("_Line_color");
        _gridTransformXSize = SystemConfig.GridLength / 10.0f;//The Plane primitive was not a unit square!
        _gridTransformZSize = SystemConfig.GridWidth / 10.0f;
        _gridSize = SystemConfig.GridSpacing;
        _gridColor = SystemConfig.GridColor.ToColor();
        _gridColor.a = 0.3925f;
        Debug.Log($"Setting grid parameters: {_gridTransformXSize}, {_gridTransformZSize}, {_gridSize}, {_gridColor}");

        _gridSpacingInspector.SetDisplayedValue(_gridSize / _conversionFactorSpacing, "", _suffixSpacing);
        _gridLengthInspector.SetDisplayedValue((_gridTransformXSize*10) / _conversionFactorDistance, "", _suffixDistance);
        _gridWidthInspector.SetDisplayedValue((_gridTransformZSize*10) / _conversionFactorDistance, "", _suffixDistance);
        _menuColorPicker.SetColor(_gridColor,true);

        // replace with load from global parameters 
        // for now we check what is currently on the material/transform
    }

    public void ChangeGridColor(Color color)
    {
        _gridColor = color;
        UpdateGrid();
    }

    public void ChangeGridSizeX(float xSize, bool flag)
    {
        _gridTransformXSize = (xSize * _conversionFactorDistance)/10;   // Convert to meters, The Plane primitive was not a unit square!
        UpdateGrid();
    }

    public void ChangeGridSizeZ(float zSize, bool flag)
    {
        _gridTransformZSize = (zSize * _conversionFactorDistance)/10;   // Convert to meters
        UpdateGrid();
    }

    public void ChangeGridCellSpacing(float cellSize, bool flag)
    {
        _gridSize = cellSize * _conversionFactorSpacing;   // Convert to centimeters

        UpdateGrid();
    }

    void UpdateGrid()
    {
        _gridTransform.localScale = new Vector3(_gridTransformXSize, 1f, _gridTransformZSize);
        _xAxisLineTransform.localScale = new Vector3(_gridTransformXSize*(1 / _initialScale.x), 1f, _initialScale.z/_gridTransform.localScale.z);
        _zAxisLineTransform.localScale = new Vector3(_initialScale.x / _gridTransform.localScale.x, 1f, _gridTransformZSize*(1/_initialScale.z));
        _yLineTransform.localScale = new Vector3(5*_initialScale.x / _gridTransform.localScale.x, 100, 5*_initialScale.z / _gridTransform.localScale.z);
        _gridMat.SetFloat("_grid_size_cm",_gridSize);
        _gridMat.SetColor("_Line_color", _gridColor);
        //SaveParameters();
    }

    public void SaveParameters()
    {
        _cachedLength = _gridTransformZSize;
        _cachedWidth = _gridTransformXSize;
        _cachedSpacing = _gridSize;
        _cachedColor = _gridColor;

        SystemConfig.GridLength = _gridTransformXSize * 10.0f;
        SystemConfig.GridWidth = _gridTransformZSize * 10.0f;
        SystemConfig.GridSpacing = _gridSize;
        SystemConfig.GridColor = RGBColor.FromColor(_gridColor);
        SystemConfig.SaveConfig();
    }
    
    public void RevertParameters()
    {
        _gridTransformZSize = _cachedLength;
        _gridTransformXSize = _cachedWidth;
        _gridSize = _cachedSpacing;
        _gridColor = _cachedColor;

        _gridSpacingInspector.SetDisplayedValue(_gridSize / _conversionFactorSpacing, "", _suffixSpacing);
        _gridLengthInspector.SetDisplayedValue((_gridTransformXSize * 10) / _conversionFactorDistance, "", _suffixDistance);
        _gridWidthInspector.SetDisplayedValue((_gridTransformXSize * 10) / _conversionFactorDistance, "", _suffixDistance);
        _menuColorPicker.SetColor(_gridColor, true);
        UpdateGrid();
    }

    public void LoadParemetersFromConfig()
    {
        _gridTransformXSize = SystemConfig.GridLength;
        _gridTransformZSize = SystemConfig.GridWidth;
        _gridSize = SystemConfig.GridSpacing;
        _gridColor = SystemConfig.GridColor.ToColor();
    }
}
