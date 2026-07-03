using System;
using Mirror;
using UnityEngine;

public class GroundCheck : NetworkBehaviour
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
        Debug.LogWarning(_isGrounded);
    } 

    private void OnCollisionExit(Collision collision)
    {
        CheckContact(collision.collider, false);
        
        Debug.LogWarning(_isGrounded);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        CheckContact(other, true);
        Debug.LogWarning(_isGrounded);
    }

    private void OnTriggerExit(Collider other)
    {
        CheckContact(other, false);
        Debug.LogWarning(_isGrounded);
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