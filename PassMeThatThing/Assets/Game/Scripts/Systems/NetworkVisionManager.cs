using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Room_Envieroments;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkVisionManager : NetworkBehaviour
{

    [SyncVar(hook = nameof(OnGlobalPowerChanged))]
    private bool _isGlobalPowerOn = true;
    public bool IsGlobalPowerOn => _isGlobalPowerOn;
    public static event System.Action<bool> OnGlobalPowerStateChanged;

    public static NetworkVisionManager Instance { get; private set; }
    public List<RoomLight> RoomLights = new();

    public void Awake()
    {
        if (!Instance) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    public void RegisterRoomLight(RoomLight roomLight)
    {
        if(!RoomLights.Contains(roomLight))
        {
            RoomLights.Add(roomLight);
            roomLight.SetActiveLocal(_isGlobalPowerOn);
        }
    }

    public void UnregisterRoomLight(RoomLight roomLight)
    {
        RoomLights.Remove(roomLight);
    }
    
    private void OnGlobalPowerChanged(bool oldState, bool newState)
    {
        OnGlobalPowerStateChanged?.Invoke(newState);

        for (var i = RoomLights.Count - 1; i >= 0; i--)
        {
            var light =  RoomLights[i];
            if (light) light.SetActiveLocal(newState);
            else RoomLights.RemoveAt(i);
        }
    }

    [Server]
    public void SetGlobalPower(bool state)
    {
        _isGlobalPowerOn = state;
    }
}