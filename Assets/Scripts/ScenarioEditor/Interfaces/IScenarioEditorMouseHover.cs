using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScenarioEditorMouseHover
{
    public void ScenarioEdtiorMouseHoverBegin(Placer placer, RaycastHit cursorHit, ScenarioCursorData cursorData);
    public void ScenarioEdtiorMouseHoverEnd(Placer placer, RaycastHit cursorHit, ScenarioCursorData cursorData);
}
