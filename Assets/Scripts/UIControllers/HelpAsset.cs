using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "ScriptableObject/HelpAsset")]
public class HelpAsset : ScriptableObject
{
    public Sprite HelpSprite;
    public string HelpString;
}
