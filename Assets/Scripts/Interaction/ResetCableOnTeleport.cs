using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCableOnTeleport : MonoBehaviour
{
    public TeleportManager TeleportManager;

    private TensionedCable[] _cables;

    // Start is called before the first frame update
    void Start()
    {
        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);

        TeleportManager.AfterTeleport += OnAfterTeleport;

        _cables = transform.GetComponentsInChildren<TensionedCable>();
    }

    private void OnAfterTeleport(Transform obj)
    {
        try
        {
            foreach (var cable in _cables)
            {
                if (cable != null && cable.gameObject != null)
                    cable.ResetCable();
            }


            StartCoroutine(DelayedReset());
        }
        catch (System.Exception) { }
    }

    private IEnumerator DelayedReset()
    {
        yield return new WaitForEndOfFrame();

        foreach (var cable in _cables)
        {
            if (cable != null && cable.gameObject != null)
                cable.ResetCable();
        }
    }
}
