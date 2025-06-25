using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnitionSource : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public VentilationManager VentilationManager;

    public GameObject VentGraphExplosionPrefab;
    public bool IgnitionSourceEnabled = true;
    public bool EnableExplosionVisual = true;
    public bool EnableExplosionAudio = true;
    public bool ExplosionExplosiveRangeOnly = false;

    private bool _explosionTriggered = false;
    private FireInteraction _fireInteraction = null;

    public void EnableIgnitionSource(bool enabled)
    {
        this.enabled = enabled;
        IgnitionSourceEnabled = enabled;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        TryGetComponent<FireInteraction>(out _fireInteraction);
    }

    private void OnEnable()
    {
        if (NetworkManager != null && NetworkManager.IsPlaybackMode)
            return;

        InvokeRepeating(nameof(CheckVentExplosion), 1.0f, 0.5f);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IgnitionSourceEnabled)
        {
            this.enabled = false;
            return;
        }

        //CheckVentExplosion();
    }

    void CheckVentExplosion()
    {
        if (NetworkManager != null && NetworkManager.IsPlaybackMode)
        {
            StopAllCoroutines();
            return;
        }

        if (!IgnitionSourceEnabled)
            return;

        if (_explosionTriggered)
            return;

        if (VentilationManager == null)
            return;

        if (_fireInteraction != null && _fireInteraction.FireExtinguished)
            return;

        if (!VentilationManager.GetMineAtmosphere(transform.position, out var atmo))
            return;

        if (atmo.Methane >= 0.05f && atmo.Methane <= 0.15f)
        {
            TriggerVentExplosion();
        }
    }

    void TriggerVentExplosion()
    {
        Debug.Log($"Ventilation explosion triggered!");
        _explosionTriggered = true;

        if (NetworkManager != null && NetworkManager.IsServer)
        {
            //VRNLogEvent explosionEvent = new VRNLogEvent();
            //NetworkManager.LogSessionEvent()
            NetworkManager.LogSessionEvent(VRNLogEventType.MineExplosion, $"Explosion Triggered by {gameObject.name}", 
                transform.position, transform.rotation, gameObject.name, VRNLogObjectType.Unknown);
        }

        if (VentGraphExplosionPrefab != null && EnableExplosionVisual)
        {
            var obj = Instantiate<GameObject>(VentGraphExplosionPrefab, transform.position, Quaternion.identity);

            if (obj.TryGetComponent<VentGraphExplosion>(out var ventExpl))
            {
                ventExpl.ExplosiveRangeOnly = ExplosionExplosiveRangeOnly;
                ventExpl.PlayAudioEffect = EnableExplosionAudio;
            }
        }
    }
}
