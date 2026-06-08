using UnityEngine;

[RequireComponent(typeof(MineFanHost))]
public class DMMineFanSpeed : MonoBehaviour, ISelectableObjectAction
{
    public string ActionName;
    public float FanMultiplier = 1.0f;

    public string SelectableActionName => ActionName;

    private MineFanHost _fanHost;

    void Start()
    {
        TryGetComponent<MineFanHost>(out _fanHost);
    }

    public void PerformSelectableObjectAction()
    {
        _fanHost.ScaleFanSpeed(FanMultiplier);
    }
}
