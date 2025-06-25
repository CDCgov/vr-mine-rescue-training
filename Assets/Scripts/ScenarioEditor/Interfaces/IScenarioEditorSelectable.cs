using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScenarioEditorSelectable
{
    public void ScenarioEditorSelectedObject(Placer placer, bool selected);
}
