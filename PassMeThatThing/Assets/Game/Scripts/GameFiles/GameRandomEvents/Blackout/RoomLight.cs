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
    private RoomController _roomController;

    private void Start()
    {
        if (!_lightComponent) _lightComponent = GetComponentInChildren<Light>(true);
        
        StartCoroutine(FindRoomControllerCoroutine());
    }
    
    private IEnumerator FindRoomControllerCoroutine()
    {
        while (!_roomController)
        {
            if (transform.parent)
            {
                _roomController = GetComponentInParent<RoomController>();
                
                if (_roomController)
                {
                    _roomController.RegisterLight(this);
                    Debug.Log($"<color=green>[RoomLight]</color> Успешно зарегистрирована в комнате {_roomController.RoomId}");
                    yield break;
                }
            }
            
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnDestroy() => _roomController?.UnregisterLight(this);

    public void SetActiveLocal(bool state)
    {
        IsActive = state;
        
        if (_lampOnPrefab) 
            _lampOnPrefab.SetActive(state);
            
        if (_lampOffPrefab) 
            _lampOffPrefab.SetActive(!state);
            
        if (_lightComponent) 
            _lightComponent.enabled = state;
    }
}