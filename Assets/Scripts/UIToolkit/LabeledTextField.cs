using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class LabeledTextField : VisualElement
{
    [UxmlAttribute, CreateProperty]
    public string LabelText
    {
        get
        {
            return _label.text;
        }
        set
        {
            _label.text = value;
        }
    }

    [UxmlAttribute]
    [CreateProperty]
    public string Text
    {
        get
        {
            return _text.text;
        }
        set
        {
            _text.text = value;
        }
    }

    private Label _label;
    private Label _text;

    public LabeledTextField()
    {
        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

        var asset = Resources.Load<VisualTreeAsset>("LabeledTextField");
        asset.CloneTree(this);


        _label = this.Q<Label>("FieldLabel");
        _text = this.Q<Label>("FieldValue");

    }

    private void OnDetachFromPanel(DetachFromPanelEvent evt)
    {
        
    }

    private void OnAttachToPanel(AttachToPanelEvent evt)
    {
        
    }
}
