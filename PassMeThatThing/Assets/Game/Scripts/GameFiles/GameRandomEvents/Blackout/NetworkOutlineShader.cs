using System;
using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Room_Envieroments;
using Mirror;
using UnityEngine;

public class NetworkOutlineShader : NetworkBehaviour
{
    [SerializeField] private float radius = 10f;
    
    [SyncVar] 
    private bool _isActive = true;
    
    public bool IsActive => _isActive;

    private RoomController _roomController;
    
    private void Start()
    {
        var currentParent = transform.parent;
        
        while (currentParent != null)
        {
            if (currentParent.TryGetComponent(out _roomController))
            {
                _roomController.RegisterLight(this);
                return;
            }
            currentParent = currentParent.parent;
        }

        Debug.LogWarning($"RoomController не найден в родительских объектах для {gameObject.name}");
    }
    
    private void OnDestroy()
    {
        if (_roomController != null)
        {
            _roomController.UnregisterLight(this);
        }
    }
    
    
    [Server]
    public void SetVisionState(bool state)
    {
        _isActive = state;
    }
    
    private void Update()
    {
        if (!_isActive) return;
        if (GlobalVisionShaderManager.Instance == null) return;

        GlobalVisionShaderManager.Instance.AddZone(transform.position, radius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}