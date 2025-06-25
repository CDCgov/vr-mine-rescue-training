using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[CreateAssetMenu]
public class UIColorPalette : ScriptableObject 
{
    public Color TextColor;
    public Color BackgroundColor;
    /*public Color ButtonNormal;
    public Color ButtonHighlighted;
    public Color ButtonPressed;
    public Color ButtonDisabled;*/
    public ColorBlock ButtonColors;
    public Color InputSelectionColor;
}