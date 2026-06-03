using System;
using UnityEngine;

public static class ExternalAssetEditorEvents
{
    public static event Action<ExternalAssetDS> ExternalAssetSelected;

    public static void RaiseExternalAssetSelected(ExternalAssetDS externalAssetDS)
    {
        ExternalAssetSelected?.Invoke(externalAssetDS);
    }
}
