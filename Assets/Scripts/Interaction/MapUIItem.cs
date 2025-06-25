using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapUIItem : MonoBehaviour
{
    public Image SpriteToUse;
    public TextMeshProUGUI Label;

    public GameObject DeleteButton;
    public GameObject CancelButton;

    public void OnClick()
    {
        DeleteButton.SetActive(true);
        CancelButton.SetActive(true);
    }

    public void OnDelete()
    {
        Destroy(gameObject);
    }

    public void OnCancel()
    {
        DeleteButton.SetActive(false);
        CancelButton.SetActive(false);
    }
}
