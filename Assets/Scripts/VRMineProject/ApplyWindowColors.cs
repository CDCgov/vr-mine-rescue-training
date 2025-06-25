using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ApplyWindowColors : MonoBehaviour 
{
    void Start () 
    {
        UIColorPalette colors = Resources.Load<UIColorPalette>("GUI/ColorPalette");

        Text[] textObjs = GetComponentsInChildren<Text>(true);

        foreach (Text text in textObjs)
        {
            text.color = colors.TextColor;
        }

        Image[] images = GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            if (image.gameObject.GetComponent<Button>() != null)
                continue;

            image.color = colors.BackgroundColor;
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            button.colors = colors.ButtonColors;
        }

        InputField[] inputs = GetComponentsInChildren<InputField>(true);
        foreach (InputField input in inputs)
        {
            input.colors = colors.ButtonColors;
            input.selectionColor = colors.InputSelectionColor;
        }

        Slider[] sliders = GetComponentsInChildren<Slider>(true);
        foreach (Slider slider in sliders)
        {
            Image[] sliderImages = slider.GetComponentsInChildren<Image>();
            foreach (Image image in sliderImages)
            {
                image.color = Color.white;
            }

            Transform bgTransform = slider.transform.Find("Background");
            Image bgImage = bgTransform.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = Color.black;
            }

            if (slider.fillRect != null)
            {
                Image img = slider.fillRect.GetComponent<Image>();
                if (img != null)
                {
                    img.color = Color.white;
                }
            }
        }

    }	
}