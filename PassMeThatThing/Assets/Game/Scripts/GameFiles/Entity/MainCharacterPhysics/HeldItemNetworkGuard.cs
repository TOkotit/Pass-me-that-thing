using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkTransformReliable))]
public class HeldItemNetworkGuard : NetworkBehaviour
{
    private NetworkRigidbodyReliable _nr;
    private Rigidbody _rb;
    private bool _held;

    private void Awake()
    {
        _nr = GetComponent<NetworkRigidbodyReliable>();
        _rb = GetComponent<Rigidbody>();
    }
    
    public void SetHeld(bool held)
    {
        if (_held == held) return;
        _held = held;

        if (!isClient || isServer || !isOwned) return;

        if (held)
        {
            // порядок критичен
            _nr.enabled = false;
            if (_rb) _rb.isKinematic = false;
        }
        else
        {
            if (_rb) _rb.isKinematic = true;
            _nr.enabled = true;
        }
    }
}