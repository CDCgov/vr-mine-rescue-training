using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIRightSnapButton : MonoBehaviour
{
    private Button _button;
    private bool _activated = false;

    public bool Activated
    {
        get { return _activated; }
        set
        {
            _activated = value;
        }
    }

    public Image ButtonImage;
    public Sprite OpenSprite;
    public Sprite ClosedSprite;
    public TimelineController TimelineController;

    // Start is called before the first frame update
    void Start()
    {
        if (TimelineController == null)
            TimelineController = TimelineController.GetDefault(gameObject);
        _button = GetComponent<Button>();
        _button.onClick.AddListener(RightSnapClicked);

        if (ButtonImage == null)
        {
            ButtonImage = GetComponentInChildren<Image>();
        }
    }

    public void RightSnapClicked()
    {
        //_activated = !_activated;

        //if (_activated)
        //{
        //    ButtonImage.sprite = ClosedSprite;
        //}
        //else
        //{
        //    ButtonImage.sprite = OpenSprite;
        //}
        _activated = !_activated;
        TimelineController.SnapRightEventSlider(_activated);
    }

    public void SnappedIcon()
    {
        ButtonImage.sprite = ClosedSprite;
        _activated = true;
    }

    public void ReleasedIcon()
    {
        ButtonImage.sprite = OpenSprite;
        _activated = false;
    }
}
