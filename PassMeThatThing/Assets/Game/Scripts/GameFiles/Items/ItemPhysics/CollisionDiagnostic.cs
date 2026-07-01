using FishNet.Object;

using UnityEngine;

public class CollisionDiagnostic : NetworkBehaviour
{
    public override void OnStartClient()
    {
        if (IsOwner || (!IsServerStarted && !IsOwner) || (IsServerStarted && !IsOwner))
            Debug.Log($"[{name}] Layer: {LayerMask.LayerToName(gameObject.layer)}");
        
        if (IsServerStarted) return;

        var rb = GetComponent<Rigidbody>();
        var cols = GetComponentsInChildren<Collider>();

        Debug.Log($"=== {name} CLIENT DIAGNOSTIC ===");
        Debug.Log($"Layer: {LayerMask.LayerToName(gameObject.layer)}");
        Debug.Log($"Rigidbody: isKinematic={rb?.isKinematic}, detection={rb?.collisionDetectionMode}");

        foreach (var c in cols)
            Debug.Log($"Collider {c.name}: enabled={c.enabled}, isTrigger={c.isTrigger}, layer={LayerMask.LayerToName(c.gameObject.layer)}");
    }

    private void OnCollisionEnter(Collision collision) =>
        Debug.Log($"[{name}] Collision with {collision.collider.name}", gameObject);

    private void OnTriggerEnter(Collider other) =>
        Debug.Log($"[{name}] Trigger with {other.name}", gameObject);
}