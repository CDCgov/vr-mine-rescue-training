using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScenarioEditorMouseMove
{
    public void OnScenarioEditorMouseMove(Placer placer, RaycastHit cursorHit, ScenarioCursorData cursorData);
}
