using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.GameFiles.GameRandomEvents.Blackout;

public class GlobalVisionShaderManager : MonoBehaviour
{
    public static GlobalVisionShaderManager Instance { get; private set; }

    [SerializeField] private int _maxActiveSources = 8; // Максимальное число обрабатываемых фонариков

    private readonly List<FlashlightVisionSource> _registeredSources = new();
    private readonly List<FlashlightVisionSource> _visibleSources = new();
    
    private readonly List<Vector4> _conesPosRange = new();
    private readonly List<Vector4> _conesDirAngle = new();
    private readonly Matrix4x4[] _matrices = new Matrix4x4[16];

    private ComputeBuffer _conesPosRangeBuffer;
    private ComputeBuffer _conesDirAngleBuffer;
    private int _bufferCapacity = 1;

    private Camera _mainCamera;
    private readonly Plane[] _frustumPlanes = new Plane[6];

    private static readonly int ConesPosRangeId = Shader.PropertyToID("_VisionConesPosRange");
    private static readonly int ConesDirAngleId = Shader.PropertyToID("_VisionConesDirAngle");
    private static readonly int MatricesId = Shader.PropertyToID("_VisionWorldToLightMatrices");
    private static readonly int ShadowMapId = Shader.PropertyToID("_VisionShadowMap");
    private static readonly int SourcesCountId = Shader.PropertyToID("_VisionSourcesCount");

    // Структура для сортировки списка без выделения памяти
    private struct SourceDistanceComparer : IComparer<FlashlightVisionSource>
    {
        public Vector3 CameraPosition;
        public int Compare(FlashlightVisionSource a, FlashlightVisionSource b)
        {
            float sqrA = (a.transform.position - CameraPosition).sqrMagnitude;
            float sqrB = (b.transform.position - CameraPosition).sqrMagnitude;
            return sqrA.CompareTo(sqrB);
        }
    }

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
        if (source != null && !_registeredSources.Contains(source))
            _registeredSources.Add(source);
    }

    public void UnregisterSource(FlashlightVisionSource source)
    {
        _registeredSources.Remove(source);
    }

    private void LateUpdate()
    {
        if (!_mainCamera) _mainCamera = Camera.main;
        if (!_mainCamera) return;

        GeometryUtility.CalculateFrustumPlanes(_mainCamera, _frustumPlanes);
        Vector3 camPos = _mainCamera.transform.position;

        _visibleSources.Clear();
        _conesPosRange.Clear();
        _conesDirAngle.Clear();

        // 1. Фильтрация выключенных и невидимых источников
        for (int i = _registeredSources.Count - 1; i >= 0; i--)
        {
            var source = _registeredSources[i];
            if (source == null)
            {
                _registeredSources.RemoveAt(i);
                continue;
            }

            source.IsVisibleToPlayer = false;

            if (!source.IsActive) continue;

            float range = source.LightSource.range;
            Bounds bounds = new Bounds(source.transform.position, new Vector3(range, range, range) * 2f);

            if (GeometryUtility.TestPlanesAABB(_frustumPlanes, bounds))
            {
                _visibleSources.Add(source);
            }
        }

        // 2. Сортировка по дистанции до игрока
        _visibleSources.Sort(new SourceDistanceComparer { CameraPosition = camPos });

        // 3. Выборка ближайших источников (не более _maxActiveSources)
        int count = Mathf.Min(_visibleSources.Count, _maxActiveSources);

        for (int i = 0; i < count; i++)
        {
            var source = _visibleSources[i];
            
            source.IsVisibleToPlayer = true;
            source.UpdateCameraAndMatrix();

            var t = source.transform;
            var light = source.LightSource;
            
            _conesPosRange.Add(new Vector4(t.position.x, t.position.y, t.position.z, light.range));
            _conesDirAngle.Add(new Vector4(t.forward.x, t.forward.y, t.forward.z, Mathf.Cos(light.spotAngle * 0.5f * Mathf.Deg2Rad)));
            _matrices[i] = source.WorldToLightMatrix;

            if (i == 0 && source.ShadowMap != null)
                Shader.SetGlobalTexture(ShadowMapId, source.ShadowMap);
        }

        Shader.SetGlobalInt(SourcesCountId, count);

        // 4. Отправка данных в шейдер
        if (count > 0)
        {
            EnsureBuffers(count);
            _conesPosRangeBuffer.SetData(_conesPosRange);
            _conesDirAngleBuffer.SetData(_conesDirAngle);

            Shader.SetGlobalBuffer(ConesPosRangeId, _conesPosRangeBuffer);
            Shader.SetGlobalBuffer(ConesDirAngleId, _conesDirAngleBuffer);
            Shader.SetGlobalMatrixArray(MatricesId, _matrices);
        }
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