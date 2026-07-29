using System.Collections.Generic;
using System.Linq;
using Game.Scripts.GameFiles.LevelGeneration.Room_Envieroments;
using Mirror;
using UnityEngine;

public class GlobalVisionShaderManager : MonoBehaviour
{
    public static GlobalVisionShaderManager Instance { get; private set; }
    
    private readonly HashSet<RoomController> _allRooms = new();
    
    private readonly List<Vector4> _activeZones = new();
    private readonly Vector4[] _shaderData = new Vector4[64];

    private static readonly int VisionZonesId = Shader.PropertyToID("_VisionZones");
    private static readonly int VisionZonesCountId = Shader.PropertyToID("_VisionZonesCount");

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public void AddZone(Vector3 position, float radius)
    {
        if (_activeZones.Count >= 64) return;
        _activeZones.Add(new Vector4(position.x, position.y, position.z, radius));
    }
    
    public void RegisterRoom(RoomController room) => _allRooms.Add(room);
    public void UnregisterRoom(RoomController room) => _allRooms.Remove(room);
    
    private void LateUpdate()
    {
        var count = _activeZones.Count;
        
        for (var i = 0; i < count; i++)
        {
            _shaderData[i] = _activeZones[i];
        }

        Shader.SetGlobalVectorArray(VisionZonesId, _shaderData);
        Shader.SetGlobalInt(VisionZonesCountId, count);

        _activeZones.Clear();
    }
    
    [Server]
    public void SetAllRoomsStateServerOnly(bool state)
    {
        if (!NetworkServer.active) return;

        foreach (var room in _allRooms.Where(room => room != null))
        {
            room.SetLightsState(state);
        }
    }
}