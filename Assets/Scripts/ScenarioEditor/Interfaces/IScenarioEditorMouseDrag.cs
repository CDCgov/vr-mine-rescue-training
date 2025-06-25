using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScenarioEditorMouseDrag
{
    public void StartMouseDrag(Placer placer);
    public void ProcessMouseDrag(ScenarioCursorData prev, ScenarioCursorData current);
    public void CompleteMouseDrag();
}
