using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelpItemController : MonoBehaviour
{
    [SerializeField]
    public Image HelpItemImage;
    [SerializeField]
    public TextMeshProUGUI HelpItemLabel;

    public void SetHelpItem(Sprite sprite, string labelText)
    {
        if (sprite != null)
        {
            HelpItemImage.sprite = sprite;
        }
        else
        {
            HelpItemImage.enabled = false;
        }
        if (labelText != null)
        {
            HelpItemLabel.text = labelText;
        }
        else
        {
            HelpItemLabel.text = "";
        }
    }
}
