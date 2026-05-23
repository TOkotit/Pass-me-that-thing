using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkOutlineTarget : NetworkBehaviour
{
    [SerializeField] private float radius = 10f;

    [SyncVar] 
    private bool _isActive = true;
    
    public bool IsActive => _isActive;

    [Server]
    public void SetVisionState(bool state)
    {
        _isActive = state;
    }
    
    private void Update()
    {
        if (_isActive && GlobalVisionManager.Instance != null)
        {
            GlobalVisionManager.Instance.AddZone(transform.position, radius);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}