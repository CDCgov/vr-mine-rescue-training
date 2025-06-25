using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VectorMineMap))]
public class MineMapMoveCamera : MonoBehaviour
{
    public ISceneCamera SceneCamera;

    private VectorMineMap _mineMap;

    private void Start()
    {
        if (SceneCamera == null)
            SceneCamera = FindObjectOfType<ScenarioEditorCamera>() as ISceneCamera;

        _mineMap = GetComponent<VectorMineMap>();        

        _mineMap.MapClicked += OnMapClicked;
    }

    private void OnDestroy()
    {
        if (_mineMap != null)
            _mineMap.MapClicked -= OnMapClicked;
    }

    private void OnMapClicked(MineMapClickedEventData obj)
    {
        if (obj.PointerEvent.button != UnityEngine.EventSystems.PointerEventData.InputButton.Left)
            return;

        if (SceneCamera == null)
            return;

        var pos = new Vector3(obj.WorldSpacePosition.x, 
            0, obj.WorldSpacePosition.z);

        SceneCamera.FocusTarget(pos, 4);
        //SceneCamera.PositionCamera(pos)
    }
}
