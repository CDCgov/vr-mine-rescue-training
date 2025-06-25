using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectableObjectAction
{
    string SelectableActionName { get; }

    void PerformSelectableObjectAction();
}
