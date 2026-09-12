using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Room_Envieroments;
using Mirror;
using UnityEngine;


[RequireComponent(typeof(Light))]
public class RoomLight : MonoBehaviour
{
    [SerializeField] private Light _lightComponent;
    [SerializeField] private GameObject _lampOnPrefab;
    [SerializeField] private GameObject _lampOffPrefab;

    public bool IsActive { get; private set; } = true;

    private void Start()
    {
        if (!_lightComponent) _lightComponent = GetComponentInChildren<Light>(true);

        if (NetworkVisionManager.Instance)
        {
            NetworkVisionManager.Instance.RegisterRoomLight(this);
        }
        else
        {
            Debug.LogWarning($"[RoomLight] NetworkVisionManager не найден для {{gameObject.name}}");
        }
    }

    private void OnDestroy()
    {
        if (NetworkVisionManager.Instance)
        {
            NetworkVisionManager.Instance.UnregisterRoomLight(this);
        }
    }

    public void SetActiveLocal(bool state)
    {
        Debug.Log($"[RoomLight] SetActiveLocal was called");
        IsActive = state;
        
        if (_lampOnPrefab) 
            _lampOnPrefab.SetActive(state);
            
        if (_lampOffPrefab) 
            _lampOffPrefab.SetActive(!state);
            
        if (_lightComponent) 
            _lightComponent.enabled = state;
    }
}