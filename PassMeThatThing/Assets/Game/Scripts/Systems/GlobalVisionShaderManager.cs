using System.Collections.Generic;
using UnityEngine;

public class GlobalVisionShaderManager : MonoBehaviour
{
    public static GlobalVisionShaderManager Instance { get; private set; }

    private readonly List<Vector4> _activeZones = new();
    private ComputeBuffer _buffer;
    private int _bufferCapacity;

    private static readonly int VisionZonesBufferId = Shader.PropertyToID("_VisionZonesBuffer");
    private static readonly int VisionZonesCountId = Shader.PropertyToID("_VisionZonesCount");

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    public void AddZone(Vector3 position, float radius)
    {
        _activeZones.Add(new Vector4(position.x, position.y, position.z, radius));
    }

    private void LateUpdate()
    {
        int count = _activeZones.Count;

        if (count == 0)
        {
            Shader.SetGlobalInt(VisionZonesCountId, 0);
            _activeZones.Clear();
            return;
        }

        if (_buffer == null || _bufferCapacity < count)
        {
            _buffer?.Release();
            _bufferCapacity = Mathf.NextPowerOfTwo(count);
            _buffer = new ComputeBuffer(_bufferCapacity, sizeof(float) * 4);
        }

        _buffer.SetData(_activeZones, 0, 0, count);

        Shader.SetGlobalBuffer(VisionZonesBufferId, _buffer);
        Shader.SetGlobalInt(VisionZonesCountId, count);

        _activeZones.Clear();
    }

    private void OnDestroy()
    {
        _buffer?.Release();
        _buffer = null;
    }
}