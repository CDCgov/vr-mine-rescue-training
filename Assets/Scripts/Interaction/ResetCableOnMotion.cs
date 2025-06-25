using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TensionedCable))]
public class ResetCableOnMotion : MonoBehaviour
{
    public float MaxMoveDistance = 2.0f;

    private TensionedCable _cable;
    private float _lastDistance = 0;

    // Start is called before the first frame update
    void Start()
    {
        _cable = GetComponent<TensionedCable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_cable.CableAnchorPoint == null || _cable.CableTarget == null)
            return;

        float dist = Vector3.Distance(_cable.CableAnchorPoint.position, _cable.CableTarget.position);
        float delta = Mathf.Abs(dist - _lastDistance);
        if (delta > MaxMoveDistance)
        {
            Debug.Log($"TensionedCable: Resetting cable {gameObject.name} due to motion");
            _cable.ResetCable();
        }

        _lastDistance = dist;
    }
}
