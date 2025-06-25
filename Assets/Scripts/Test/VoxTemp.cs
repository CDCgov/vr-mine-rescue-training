using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoxTemp : MonoBehaviour {

    public Light myLight;
    public AudioSource audio;

    [SerializeField]
    private string[] m_Keywords;

    private KeywordRecognizer m_Recognizer;
    // Use this for initialization
    void Start () {
        m_Recognizer = new KeywordRecognizer(m_Keywords);
        m_Recognizer.OnPhraseRecognized += OnPhraseRecognized;
        m_Recognizer.Start();
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
        builder.AppendFormat("\tTimestamp: {0}{1}", args.phraseStartTime, Environment.NewLine);
        builder.AppendFormat("\tDuration: {0} seconds{1}", args.phraseDuration.TotalSeconds, Environment.NewLine);
        Debug.Log(builder.ToString());
        switch (args.text)
        {
            case "On":
                myLight.enabled = true;
                break;
            case "Off":
                myLight.enabled = false;
                break;
            case "Phone":
                audio.Play();
                break;
            default:
                break;
        }
    }
}
