using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ComponentInfo_TextTexture : ModularComponentInfo, ISaveableComponent, IInspectableComponent
{
    public TextTexture TextTexture;
    public DecalProjector DecalProjector;
    public Color DefaultColor;

    private Material _decalMat;
    private Color _color;

    [InspectableStringProperty("Text")]
    public string SprayPaintText
    {
        get => TextTexture.Text;
        set
        {
            TextTexture.Text = value;
            TextTexture.UpdateTexture();
        }
    }

    [InspectableColorProperty("Color")]
    public Color SprayPaintColor
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;

            if (_decalMat != null)
            {
                _decalMat.color = value;
            }
            //TextTexture.TextColor = value;
            //TextTexture.UpdateTexture();
        }
    }

    [InspectableNumericProperty("Opacity", MinValue = 0, MaxValue = 100, SliderControl = true, Units = NumericPropertyUnitType.Ratio)]
    public float Opacity
    {
        get => TextTexture.Opacity;
        set => TextTexture.Opacity = value;
    }

    public string ComponentInspectorTitle => "Spray Paint Text";

    public void Awake()
    {
        if (TextTexture == null)
            TryGetComponent<TextTexture>(out TextTexture);
        if (DecalProjector == null)
            TryGetComponent<DecalProjector>(out DecalProjector);

        _color = DefaultColor;
    }

    public void Start()
    {
        if (DecalProjector == null)
        {
            Debug.LogError($"No decal projector for TextTexture on {name}");
            return;
        }

        _decalMat = Instantiate<Material>(DecalProjector.material);
        _decalMat.color = _color;

        DecalProjector.material = _decalMat;
    }

    public void OnDestroy()
    {
        if (_decalMat != null)
            Destroy(_decalMat);   
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
            return;

        if (TextTexture == null)
        {
            Debug.LogError($"No TextTexture on {name}");
            return;
        }

        SprayPaintText = component.GetParamValueAsStringByName("SprayPaintText");
        SprayPaintColor = component.GetParamValueColor("Color", DefaultColor);
        Opacity = component.GetParamValueFloat("Opacity", 0.75f);
    }

    public string[] SaveInfo()
    {
        return new string[]
        {
            "SprayPaintText|" + SprayPaintText,
            "Color|#" + ColorUtility.ToHtmlStringRGBA(SprayPaintColor),
            "Opacity|" + Opacity.ToString("F4"),
        };
    }

    public string SaveName()
    {
        return "TextTexture";
    }
}
