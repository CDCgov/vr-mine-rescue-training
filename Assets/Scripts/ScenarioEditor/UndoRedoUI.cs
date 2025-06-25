using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoRedoUI : MonoBehaviour
{

    private void Start()
    {
        UndoManager.maxUndoStored = 100;
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            PerformUndoAction();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
        {
            PerformRedoAction();
        }
    }

    public void PerformUndoAction()
    {
        UndoManager.Undo();
    }

    public void PerformRedoAction()
    {
        UndoManager.Redo();
    }
}
