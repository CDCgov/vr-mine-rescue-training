using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Google.Protobuf;
using Unity.VectorGraphics;
using UnityEngine.Serialization;

[HasCommandConsoleCommands]
[CreateAssetMenu(fileName = "MineMapSymbol", menuName = "VRMine/MineSymbol", order = 0)]
public class MineMapSymbol : ScriptableObject
{
	public enum SymbolType
	{
		Prefab,   
		Smoke,
		UnsafeRoof,
        GasCheck,
        Door,
        PermanentStopping,
        Refuge,
	}

	public SymbolType MineSymbolType;

	public GameObject SymbolPrefab;
	public Vector2 Size = new Vector2(20, 20);
	public Color Color = Color.white;
	public bool IgnoreRotation = false;
	public bool SpanEntry = false;
    [FormerlySerializedAs("ScaleAcross")]
    public bool SpanDiagonal = false;
    public bool SnapToIntersection = false;
	public bool PreserveAspect = false;
    public float FontSize = -1;

	[TextArea(3,5)]
	public string SymbolText;

	[System.NonSerialized]
	public Vector3 WorldPosition;
	[System.NonSerialized]
	public Quaternion WorldRotation;
	[System.NonSerialized]
	public bool ShowOnMapMan = false;
    [System.NonSerialized]
    public bool DoNotDelete = false;
	[System.NonSerialized]
	public long SymbolID;
	[System.NonSerialized]
	public string AddressableKey;
    [System.NonSerialized]
    public bool IsAnnotation = false;

    [System.NonSerialized]
    public MineMapSymbol ChildSymbol;
    [System.NonSerialized]
    public MineMapSymbol ParentSymbol;

    public bool AllowManualRotations = false;
    public string ClockWiseSymbolKey;
    public string CounterClockwiseSymbolKey;
    public bool AllowFlipSymbol = false;
    public string FlipSymbolKey;
    public List<string> AvailableAnnotations;
    public bool AllowColorChange = false;
    public string DisplayName;

	protected TextMeshProUGUI _text;
	

	public virtual GameObject Instantiate()
	{
        if (SymbolPrefab == null)
        {
            Debug.LogError($"MineMapSymbol: Missing prefab on {name}");
            return null;
        }

		var obj = GameObject.Instantiate<GameObject>(SymbolPrefab);
		SetColor(obj, Color);

		if (PreserveAspect && !SpanEntry)
		{
			var svg = obj.GetComponentInChildren<SVGImage>();
			if (svg != null)
			{
				svg.preserveAspect = true;
			}
		}

		_text = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (_text != null)
        {
            _text.text = SymbolText;

            if (FontSize > 0 && _text.enableAutoSizing)
            {
                _text.fontSizeMax = FontSize;
            }
        }

		SetColor(obj, Color);

		return obj;
	}

	public virtual void SetColor(GameObject instance, Color newColor)
	{
		//var svg = instance.GetComponentInChildren<SVGImage>();
		//if (svg != null)
		//{
		//	svg.color = newColor;
		//}

        var svgs = instance.GetComponentsInChildren<SVGImage>();
        foreach (var svg in svgs)
        {
            svg.color = newColor;
        }

		var text = instance.GetComponentsInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            foreach (var textItem in text)
            {
                textItem.color = newColor;
            }
            //text.color = newColor;
        }

		Color = newColor;
	}

	public virtual VRNSymbolData GetSymbolData()
	{
		try
		{
			if (SymbolText == null)
				SymbolText = "";
			if (AddressableKey == null)
				AddressableKey = "";

			VRNSymbolData data = new VRNSymbolData
			{
				SymbolID = this.SymbolID,
				SymbolClass = "",
				Addressable = AddressableKey,
				Size = new VRNVector2 { X = this.Size.x, Y = this.Size.y },
				Color = new VRNColor { R = this.Color.r, G = this.Color.g, B = this.Color.b },
				IgnoreRotation = this.IgnoreRotation,
				SpanEntry = this.SpanEntry,
				PreserveAspect = this.PreserveAspect,
				SymbolText = this.SymbolText,
				WorldPosition = this.WorldPosition.ToVRNVector3(),
				WorldRotation = this.WorldRotation.ToVRNQuaternion(),
			};

			data.SymbolData = ByteString.Empty;

			//if (data.SymbolText == null)
			//    data.SymbolText = "";
			//if (data.Addressable == null)
			//    data.Addressable = "";

			return data;
		}
		catch (System.Exception ex)
		{
			Debug.LogError(ex.Message);
			return null;
		}

	}

	

}
