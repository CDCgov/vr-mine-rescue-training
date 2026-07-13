using System;
using UnityEngine;

public static class VRMineUIEvents
{
    public static event Action<ExternalAssetDS> ExternalAssetSelected;
    public static event Action<CustomScenarioDS> CustomScenarioSelected;

    public static void RaiseExternalAssetSelected(ExternalAssetDS externalAssetDS)
    {
        ExternalAssetSelected?.Invoke(externalAssetDS);
    }

    public static void RaiseCustomScenarioSelected(CustomScenarioDS customScenarioDS)
    {
        CustomScenarioSelected?.Invoke(customScenarioDS);
    }
}
