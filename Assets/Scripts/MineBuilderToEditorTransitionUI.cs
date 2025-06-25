using NIOSH_EditorLayers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBuilderToEditorTransitionUI : UIButtonBase
{
    public MineBuilderToEditorTransition mineBuilderToEditorTransition;
    protected override void OnButtonClicked()
    {
        mineBuilderToEditorTransition.InitializeScenarioEditorUI();
    }
}
