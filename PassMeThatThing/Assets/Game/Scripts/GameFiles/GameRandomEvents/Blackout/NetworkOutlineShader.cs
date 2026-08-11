using System;
using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Room_Envieroments;
using Mirror;
using UnityEngine;

public class OutlineShader : MonoBehaviour
{
    [SerializeField] private float radius = 10f;

    public bool IsActive { get; private set; } = true;

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

    private void OnDestroy() => _roomController?.UnregisterLight(this);

    public void SetActiveLocal(bool state) => IsActive = state;

    private void Update()
    {
        if (!IsActive) return;
        if (GlobalVisionShaderManager.Instance == null) return;

        GlobalVisionShaderManager.Instance.AddZone(transform.position, radius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}