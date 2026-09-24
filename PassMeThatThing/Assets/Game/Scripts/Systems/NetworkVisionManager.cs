using System.Collections.Generic;
using Game.Scripts.GameFiles.GlobalStageManager;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class NetworkVisionManager : NetworkBehaviour
{

    [SyncVar(hook = nameof(OnGlobalPowerChanged))]
    private bool _isGlobalPowerOn = true;
    public bool IsGlobalPowerOn => _isGlobalPowerOn;
    
    [SyncVar(hook = nameof(OnGlobalStateChanged))]
    private bool _isGlobalStateValueFight = false;
    public bool IsGlobalStateValueFight => _isGlobalStateValueFight;
    public static UnityEvent<bool> OnGlobalPowerStateChanged = new();

    public static NetworkVisionManager Instance { get; private set; }
    public List<RoomLight> RoomLights = new();

    public void Awake()
    {
        if (!Instance) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    public void RegisterRoomLight(RoomLight roomLight)
    {
        if (RoomLights.Contains(roomLight)) return;
        RoomLights.Add(roomLight);
        var startingState = _isGlobalPowerOn ? (_isGlobalStateValueFight ? RoomLightState.Warning : RoomLightState.Common) : RoomLightState.Off;
        roomLight.SetLampState(startingState);
    }

    public void UnregisterRoomLight(RoomLight roomLight)
    {
        RoomLights.Remove(roomLight);
    }

    private void UpdateLamps()
    {

        var currentState = _isGlobalPowerOn ? (_isGlobalStateValueFight? RoomLightState.Warning : RoomLightState.Common)
            : RoomLightState.Off;
        
        Debug.Log($"[NetworkVisionManager] Updating Lamps on value {currentState}");
        foreach (var roomLight in RoomLights)
        {
            roomLight.SetLampState(currentState);
        }
    }
    
    private void OnGlobalPowerChanged(bool oldState, bool newState)
    {
        OnGlobalPowerStateChanged?.Invoke(newState);
        UpdateLamps();
    }

    private void OnGlobalStateChanged(bool oldState, bool newState)
    {
        UpdateLamps();
    }

    [Server]
    public void SetGlobalPower(bool state)
    {
        _isGlobalPowerOn = state;
        UpdateLamps();
    }

    [Server]
    public void SetGlobalStateValue(bool state)
    {
        Debug.Log($"[NetworkVisionManager] SetGlobalStateValue value {state}");
        _isGlobalStateValueFight = state;   
        UpdateLamps();
    }
}