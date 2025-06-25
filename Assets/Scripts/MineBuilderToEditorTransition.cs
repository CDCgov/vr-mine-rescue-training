using NIOSH_EditorLayers;
using UnityEngine;

public class MineBuilderToEditorTransition : MonoBehaviour
{
    public ScenarioEditorCanvasSwapper canvasSwapper;
    public ScenarioEditorCamera editorCamera;
    public ScenarioEditorViewportController viewportController;
    public AssetLibrary assetLibrary;
    //public HierarchyContainer hierarchyContainer;
    public LayerManager layerManager;

    public CanvasGroup assetPlacementCanvas;
    public string startingCameraSettingsLayer;
    public int startingLayerInt;

    public void InitializeScenarioEditorUI()
    {
        canvasSwapper.MoveToNewCanvasGroup(assetPlacementCanvas);
        editorCamera.ChangeCameraSettings(startingCameraSettingsLayer);
        viewportController.Initialize();
        assetLibrary.Initialize();
        //hierarchyContainer.Initialize();
        layerManager.ChangeLayerFromInt(startingLayerInt);
    }
}
