using System.Collections.Generic;
using UnityEngine;

public class GlobalVisionShaderManager : MonoBehaviour
{
    public static GlobalVisionShaderManager Instance { get; private set; }

    private readonly List<Vector4> _activeConesPosRange = new();  
    private readonly List<Vector4> _activeConesDirAngle = new();  

    private ComputeBuffer _conesPosRangeBuffer;
    private ComputeBuffer _conesDirAngleBuffer;
    private int _conesBufferCapacity = 1;

    private static readonly int ConesPosRangeId = Shader.PropertyToID("_VisionConesPosRange");
    private static readonly int ConesDirAngleId = Shader.PropertyToID("_VisionConesDirAngle");
    private static readonly int ConesCountId = Shader.PropertyToID("_VisionConesCount");
    private static readonly int ZonesCountId = Shader.PropertyToID("_VisionZonesCount"); 

    private void Awake()
    {
        if (!Instance) Instance = this;
        else { Destroy(gameObject); return; }

        _conesPosRangeBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        _conesDirAngleBuffer = new ComputeBuffer(1, sizeof(float) * 4);

        Shader.SetGlobalBuffer(ConesPosRangeId, _conesPosRangeBuffer);
        Shader.SetGlobalBuffer(ConesDirAngleId, _conesDirAngleBuffer);
        Shader.SetGlobalInt(ConesCountId, 0);
        Shader.SetGlobalInt(ZonesCountId, 0);
    }

    public void AddConeZone(Vector3 position, Vector3 direction, float halfAngleDegrees, float range)
    {
        _activeConesPosRange.Add(new Vector4(position.x, position.y, position.z, range));

        var dir = direction.normalized;
        var cosHalfAngle = Mathf.Cos(halfAngleDegrees * Mathf.Deg2Rad);
        _activeConesDirAngle.Add(new Vector4(dir.x, dir.y, dir.z, cosHalfAngle));
    }

    private void LateUpdate()
    {
        var conesCount = _activeConesPosRange.Count;
        Shader.SetGlobalInt(ConesCountId, conesCount);

        if (conesCount > 0)
        {
            EnsureConeBuffers(conesCount);
            _conesPosRangeBuffer.SetData(_activeConesPosRange);
            _conesDirAngleBuffer.SetData(_activeConesDirAngle);

            Shader.SetGlobalBuffer(ConesPosRangeId, _conesPosRangeBuffer);
            Shader.SetGlobalBuffer(ConesDirAngleId, _conesDirAngleBuffer);
        }

        _activeConesPosRange.Clear();
        _activeConesDirAngle.Clear();
    }

    private void EnsureConeBuffers(int count)
    {
        if (_conesPosRangeBuffer != null && _conesBufferCapacity >= count) return;

        _conesPosRangeBuffer?.Release();
        _conesDirAngleBuffer?.Release();

        _conesBufferCapacity = Mathf.NextPowerOfTwo(Mathf.Max(1, count));
        _conesPosRangeBuffer = new ComputeBuffer(_conesBufferCapacity, sizeof(float) * 4);
        _conesDirAngleBuffer = new ComputeBuffer(_conesBufferCapacity, sizeof(float) * 4);
    }

    private void OnDestroy()
    {
        _conesPosRangeBuffer?.Release();
        _conesDirAngleBuffer?.Release();
    }
}