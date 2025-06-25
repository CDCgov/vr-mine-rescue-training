using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerZoneEventLogger : MonoBehaviour
{
    public NetworkManager NetworkManager;

    public Transform PlayerTransform;
    public Bounds CheckBounds;
    public float MinTimeBetweenLogs = 3.0f;
    public float MinDistBetweenLogs = 2.0f;

    private float _nextLogTime;
    private Vector3 _lastLogPosition;
    private bool _lastLogPositionValid;
    private int _layerMask;
    private Collider[] _colliders;

    private void Awake()
    {
        _colliders = new Collider[50];
        _layerMask = LayerMask.GetMask("RoofBolts");
        _nextLogTime = float.MinValue;
        _lastLogPosition = Vector3.zero;
        _lastLogPositionValid = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        if (PlayerTransform == null)
            PlayerTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time < _nextLogTime)
            return;

        if (_lastLogPositionValid)
        {
            var dist = Vector3.Distance(PlayerTransform.position, _lastLogPosition);
            if (dist < MinDistBetweenLogs)
                return;
        }

        if (!CheckUnderBadRoof())
            return;

        Debug.Log($"Logging Bad Roof Zone violation by {gameObject.name}");

        LogBadRoofEvent();

    }

    private bool CheckUnderBadRoof()
    {
        int numHits = Physics.OverlapBoxNonAlloc(PlayerTransform.TransformPoint(CheckBounds.center), CheckBounds.extents, _colliders, Quaternion.identity, _layerMask, QueryTriggerInteraction.Collide);
        if (numHits <= 0)
            return false;

        for (int i = 0; i < numHits; i++)
        {
            var collider = _colliders[i];

            if (!collider.TryGetComponent<SoundingType>(out var sounding))
                continue;

            if (sounding.SoundMaterial == SoundType.BadRoof)
                return true;
        }

        return false;
    }

    private void LogBadRoofEvent()
    {
        if (NetworkManager == null)
            return;

        _lastLogPosition = PlayerTransform.position;
        _nextLogTime = Time.time + MinTimeBetweenLogs;
        _lastLogPositionValid = true;

        NetworkManager.LogSessionEvent(VRNLogEventType.ZoneViolation, "Bad Roof Zone", PlayerTransform.position, PlayerTransform.rotation, "Bad Roof Zone", VRNLogObjectType.Unknown);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Gizmos.DrawWireCube(CheckBounds.center, CheckBounds.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
