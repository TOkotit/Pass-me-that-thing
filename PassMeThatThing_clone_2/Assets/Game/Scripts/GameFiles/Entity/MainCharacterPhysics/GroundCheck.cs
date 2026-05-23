using System;
using Mirror;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private bool _isGrounded;
    private bool _touchesWater;
    public bool IsGrounded => _isGrounded;

    public bool TouchesWater
    {
        get => _touchesWater;
        private set
        {
            _touchesWater = value;
            if (value)
                OnWaterTouched?.Invoke();
        }
    }
    
    public Action OnWaterTouched;
    
    private void OnCollisionStay(Collision collision)
    {
        CheckContact(collision.collider, true);
    }

    private void OnCollisionExit(Collision collision)
    {
        CheckContact(collision.collider, false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        CheckContact(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        CheckContact(other, false);

    }
    
    private void CheckContact(Collider other, bool state)
    {
        if (other.CompareTag("Ground"))
        {
            _isGrounded = state;
        }

        if (other.CompareTag("Water"))
        {
            TouchesWater = state;
        }
    }
    
}