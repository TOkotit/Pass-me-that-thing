using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.GameFiles.GameRandomEvents.Blackout;

public class GlobalVisionShaderManager : MonoBehaviour
{
    public static GlobalVisionShaderManager Instance { get; private set; }

    private readonly List<FlashlightVisionSource> _registeredSources = new();
    
    private readonly List<Vector4> _conesPosRange = new();
    private readonly List<Vector4> _conesDirAngle = new();
    private readonly Matrix4x4[] _matrices = new Matrix4x4[16];

    private ComputeBuffer _conesPosRangeBuffer;
    private ComputeBuffer _conesDirAngleBuffer;
    private int _bufferCapacity = 1;

    private static readonly int ConesPosRangeId = Shader.PropertyToID("_VisionConesPosRange");
    private static readonly int ConesDirAngleId = Shader.PropertyToID("_VisionConesDirAngle");
    private static readonly int MatricesId = Shader.PropertyToID("_VisionWorldToLightMatrices");
    private static readonly int ShadowMapId = Shader.PropertyToID("_VisionShadowMap");
    private static readonly int SourcesCountId = Shader.PropertyToID("_VisionSourcesCount");

    private void Awake()
    {
        if (!Instance) Instance = this;
        else { Destroy(gameObject); return; }

        _conesPosRangeBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        _conesDirAngleBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        Shader.SetGlobalInt(SourcesCountId, 0);
    }

    public void RegisterSource(FlashlightVisionSource source)
    {
        if (!_registeredSources.Contains(source) && _registeredSources.Count < 16)
            _registeredSources.Add(source);
    }

    private void LateUpdate()
    {
        int count = _registeredSources.Count;
        Shader.SetGlobalInt(SourcesCountId, count);

        if (count > 0)
        {
            EnsureBuffers(count);
            _conesPosRange.Clear();
            _conesDirAngle.Clear();

            for (int i = 0; i < count; i++)
            {
                var source = _registeredSources[i];
                var t = source.transform;
                var light = source.LightSource;
                
                _conesPosRange.Add(new Vector4(t.position.x, t.position.y, t.position.z, light.range));
                _conesDirAngle.Add(new Vector4(t.forward.x, t.forward.y, t.forward.z, Mathf.Cos(light.spotAngle * 0.5f * Mathf.Deg2Rad)));
                _matrices[i] = source.WorldToLightMatrix;
            }

            _conesPosRangeBuffer.SetData(_conesPosRange);
            _conesDirAngleBuffer.SetData(_conesDirAngle);

            Shader.SetGlobalBuffer(ConesPosRangeId, _conesPosRangeBuffer);
            Shader.SetGlobalBuffer(ConesDirAngleId, _conesDirAngleBuffer);
            Shader.SetGlobalMatrixArray(MatricesId, _matrices);
            
            if (_registeredSources[0].ShadowMap != null)
                Shader.SetGlobalTexture(ShadowMapId, _registeredSources[0].ShadowMap);
        }

        _registeredSources.Clear();
    }

    private void EnsureBuffers(int count)
    {
        if (_conesPosRangeBuffer != null && _bufferCapacity >= count) return;
        _conesPosRangeBuffer?.Release();
        _conesDirAngleBuffer?.Release();
        _bufferCapacity = Mathf.NextPowerOfTwo(Mathf.Max(1, count));
        _conesPosRangeBuffer = new ComputeBuffer(_bufferCapacity, sizeof(float) * 4);
        _conesDirAngleBuffer = new ComputeBuffer(_bufferCapacity, sizeof(float) * 4);
    }

    private void OnDestroy()
    {
        _conesPosRangeBuffer?.Release();
        _conesDirAngleBuffer?.Release();
    }
}