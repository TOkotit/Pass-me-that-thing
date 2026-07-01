using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;

using UnityEngine;

public class NetworkOutlineShader : NetworkBehaviour
{
    [SerializeField] private float radius = 10f;
    
    // [SyncVar] 
    private readonly SyncVar<bool> _isActive = new(true);
    
    public bool IsActive => _isActive.Value;


    
    private void OnEnable()
    {
        if (GlobalVisionShaderManager.Instance != null)
        {
            GlobalVisionShaderManager.Instance.RegisterLamp(this);
        }
    }

    private void OnDisable()
    {
        if (GlobalVisionShaderManager.Instance != null)
        {
            GlobalVisionShaderManager.Instance.UnregisterLamp(this);
        }
    }
    
    
    [Server]
    public void SetVisionState(bool state)
    {
        _isActive.Value = state;
    }
    
    private void Update()
    {
        if (!_isActive.Value) return;
        if (!GlobalVisionShaderManager.Instance) return;

        GlobalVisionShaderManager.Instance.AddZone(transform.position, radius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}