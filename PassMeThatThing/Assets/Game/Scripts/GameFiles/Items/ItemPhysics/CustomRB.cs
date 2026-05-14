using UnityEngine;
using Mirror;

public class CustomRB : NetworkBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Item"))
        {
            Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
            float pushForce = 10f;
            Vector3 force = pushDirection * pushForce;
            CmdApplyForce(force);
        }
    }

    [Command(requiresAuthority = false)] 
    private void CmdApplyForce(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }
}