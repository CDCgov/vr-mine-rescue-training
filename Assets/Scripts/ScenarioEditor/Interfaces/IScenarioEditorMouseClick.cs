using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScenarioEditorMouseClick
{
    public void OnScenarioEditorMouseDown(Placer placer, int button, RaycastHit cursorHit, ScenarioCursorData cursorData);
    public void OnScenarioEditorMouseUp(Placer placer, int button, RaycastHit cursorHit, ScenarioCursorData cursorData);

    public void OnScenarioEditorMouseFocusLost(Placer placer);
    public bool IsSelectionLocked { get; }
}
