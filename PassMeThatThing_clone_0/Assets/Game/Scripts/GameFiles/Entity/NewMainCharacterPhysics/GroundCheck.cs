using Mirror;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private bool _isGrounded;

    public bool IsGrounded => _isGrounded;

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
            _isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
            _isGrounded = false;
    }
}