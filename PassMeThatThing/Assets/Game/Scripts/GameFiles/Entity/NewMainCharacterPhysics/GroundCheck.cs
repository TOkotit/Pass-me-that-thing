using Mirror;
using UnityEngine;

public class GroundCheck : NetworkBehaviour
{
    [SyncVar] private bool _isGrounded;

    public bool IsGrounded => _isGrounded;

    private void OnCollisionStay(Collision collision)
    {
        if (!isServer) return;
        if (collision.collider.CompareTag("Ground"))
            _isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!isServer) return;
        if (collision.collider.CompareTag("Ground"))
            _isGrounded = false;
    }
}