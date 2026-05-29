using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class GlobalVisionShaderManager : MonoBehaviour
{
    public static GlobalVisionShaderManager Instance { get; private set; }
    
    private readonly HashSet<NetworkOutlineShader> _allLamps = new();
    
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
    
    public void RegisterLamp(NetworkOutlineShader lamp) => _allLamps.Add(lamp);
    public void UnregisterLamp(NetworkOutlineShader lamp) => _allLamps.Remove(lamp);
    
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
    
    public void ToggleAllLampsServerOnly()
    {
        if (!NetworkServer.active) return;

        foreach (var lamp in _allLamps.Where(lamp => lamp != null))
        {
            lamp.SetVisionState(!lamp.IsActive);
        }
    }
}