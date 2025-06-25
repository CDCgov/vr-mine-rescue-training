using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBtnCableChangeNodeType : UIButtonBase
{
    public RuntimeCableEditor RuntimeCableEditor;
    public NodeAddMode NodeType;

    protected override void Start()
    {
        base.Start();

        if (RuntimeCableEditor == null)
            RuntimeCableEditor = RuntimeCableEditor.GetDefault(gameObject);
    }

    protected override void OnButtonClicked()
    {
        if (RuntimeCableEditor == null)
            return;

        RuntimeCableEditor.ChangeBoltType(NodeType);
    }
}
