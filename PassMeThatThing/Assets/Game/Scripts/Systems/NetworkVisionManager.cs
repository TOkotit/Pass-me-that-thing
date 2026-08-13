using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Room_Envieroments;
using Mirror;
using UnityEngine;

public class NetworkVisionManager : NetworkBehaviour
{
    public static NetworkVisionManager Instance { get; private set; }

    private readonly SyncDictionary<int, bool> _roomPowerStates = new();

    [SyncVar]
    private bool _isGlobalPowerOn = true;
    public bool IsGlobalPowerOn => _isGlobalPowerOn;

    private readonly Dictionary<int, RoomController> _localRooms = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    public override void OnStartClient()
    {
        _roomPowerStates.OnChange  += OnRoomPowerStateChanged;

        foreach (var kvp in _roomPowerStates)
        {
            ApplyRoomState(kvp.Key, kvp.Value);
        }
    }

    private void OnRoomPowerStateChanged(SyncDictionary<int, bool>.Operation op, int roomId, bool state)
    {
        if (op == SyncDictionary<int, bool>.Operation.OP_ADD || op == SyncDictionary<int, bool>.Operation.OP_SET)
        {
            ApplyRoomState(roomId, state);
        }
    }

    private void ApplyRoomState(int roomId, bool state)
    {
        if (_localRooms.TryGetValue(roomId, out var room) && room != null)
        {
            room.ApplyPowerState(state);
        }
    }


    public void RegisterRoomLocal(int roomId, RoomController room)
    {
        _localRooms[roomId] = room;

        if (_roomPowerStates.TryGetValue(roomId, out var state))
        {
            room.ApplyPowerState(state);
        }
        else if (isServer)
        {
            _roomPowerStates[roomId] = true;
        }
    }

    public void UnregisterRoomLocal(int roomId)
    {
        _localRooms.Remove(roomId);
    }

    [Server]
    public void SetRoomPower(int roomId, bool state)
    {
        _roomPowerStates[roomId] = state;
    }

    [Server]
    public void SetAllRoomsPower(bool state)
    {
        _isGlobalPowerOn = state;

        var ids = new List<int>(_roomPowerStates.Keys);
        foreach (var id in ids)
        {
            _roomPowerStates[id] = state;
        }
    }
}