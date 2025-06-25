using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using UnityEngine;

public static class UndoManager
{
    static UndoRedo undo = new UndoRedo();
    static List<GameObject> garbageBin = new List<GameObject>(); // TODO Put destroyed object handler in its own class, maybe with object pooling as well?

    public static int maxUndoStored { get { return undo.maxUndoStored; } set { undo.maxUndoStored = value; } }

    public static void Clear()
    {
        undo.Clear();
    }

    public static void Undo()
    {
        undo.Undo();
    }

    public static void Redo()
    {
        undo.Redo();
    }

    public static void Insert(ICommand command)
    {
        undo.Insert(command);
    }

    public static void Execute(ICommand command)
    {
        undo.Execute(command);
    }

    public static void AddGameObjectToBin(GameObject obj)
    {
        if(!garbageBin.Contains(obj))
        {
            garbageBin.Add(obj);
        }
    }

    public static void RemoveGameObjectFromBin(GameObject obj)
    {
        if (!garbageBin.Contains(obj))
        {
            garbageBin.Remove(obj);
        }
    }

    public static void ClearBin()
    {
        foreach(GameObject obj in garbageBin)
        {
            GameObject.Destroy(obj);
        }
        garbageBin.Clear();
    }
}
