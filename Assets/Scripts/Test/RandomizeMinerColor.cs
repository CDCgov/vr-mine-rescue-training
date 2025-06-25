using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MinerColorChanger))]
public class RandomizeMinerColor : MonoBehaviour
{
    public bool RandomOrientation = true;

    private MinerColorChanger _colorChanger;
    
    // Start is called before the first frame update
    void Start()
    {
        Color color = Color.HSVToRGB(Random.value, 1.0f, 1.0f);
        //var text = System.Convert.ToChar(Random.Range(33, 123)).ToString();
        var text = Random.Range(1, 99).ToString();

        _colorChanger = GetComponent<MinerColorChanger>();

        _colorChanger.MinerColor = color;
        _colorChanger.UpdateMiner();


        
        var textTextures = gameObject.GetComponentsInChildren<TextTexture>();
        if (textTextures != null)
        {
            foreach (var textTexture in textTextures)
            {
                textTexture.Text = text;
                textTexture.UpdateTexture();
            }
        }

        if (RandomOrientation)
        {
            transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        }
    }

}
