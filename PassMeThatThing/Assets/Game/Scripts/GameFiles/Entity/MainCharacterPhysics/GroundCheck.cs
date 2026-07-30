using System;
using Mirror;
using UnityEngine;

public class GroundCheck : NetworkBehaviour
{
    
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private float _checkRadius = 0.2f;
    [SerializeField] private LayerMask _groundMask;
    
    private bool _isGrounded;
    public bool IsGrounded => _isGrounded;
    
    
    [SyncVar] private bool _touchesWater;

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
    public Action OnRunningOnItem;
    
    private void FixedUpdate()
    {
        _isGrounded = Physics.CheckSphere(_groundCheckPoint.position, _checkRadius, _groundMask);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water")) TouchesWater = true;
        if (other.CompareTag("Item")) OnRunningOnItem?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water")) TouchesWater = false;
    }
}