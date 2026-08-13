using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobalVisionShaderManager : MonoBehaviour
{
 public static GlobalVisionShaderManager Instance { get; private set; }

    private const int BaseSamples = 24;
    private const int MaxObstacles = 12;
    private const int CornersPerObstacle = 2;
    private const int RaysPerCorner = 3;

    private const int MaxBoundaryPoints = BaseSamples + MaxObstacles * CornersPerObstacle * RaysPerCorner; // 96

    private const int VerticalLayers = 5;
    private const int TotalLayers = VerticalLayers * 2 + 1; 

    private const int BaseSamplesVertical = 16;
    private const int MeridianCount = 10;
    private const int MeridianBoundaryPoints = BaseSamplesVertical + MaxObstacles * CornersPerObstacle * RaysPerCorner; // 88

    private const float CornerEpsilonRad = 0.01f;

    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float occlusionRescanInterval = 1f;
    [SerializeField] private float wallPadding = 0.15f;
    [SerializeField] private float verticalStep = 1f;

    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoRingColor = new Color(1f, 1f, 0f, 0.6f);
    [SerializeField] private Color gizmoMeridianColor = new Color(0.3f, 0.6f, 1f, 0.6f);

    private readonly List<Vector4> _activeZones = new();

    private ComputeBuffer _boundaryBuffer;
    private ComputeBuffer _meridianBuffer;
    private ComputeBuffer _zonesBuffer;
    private int _bufferCapacity;

    private Vector2[] _boundaryData = Array.Empty<Vector2>();
    private Vector2[] _meridianData = Array.Empty<Vector2>();
    private Vector4[] _zonesData = Array.Empty<Vector4>();
    private int _lastActiveCount;
    private float _timer;

    private static readonly int BoundaryBufferId = Shader.PropertyToID("_VisionBoundaryBuffer");
    private static readonly int MeridianBufferId = Shader.PropertyToID("_VisionMeridianBuffer");
    private static readonly int ZonesBufferId = Shader.PropertyToID("_VisionZonesBuffer");
    private static readonly int ZonesCountId = Shader.PropertyToID("_VisionZonesCount");
    private static readonly int VerticalStepId = Shader.PropertyToID("_VisionVerticalStep");

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddZone(Vector3 position, float radius)
    {
        _activeZones.Add(new Vector4(position.x, position.y, position.z, radius));
    }

    private void LateUpdate()
    {
        var count = _activeZones.Count;
        Shader.SetGlobalInt(ZonesCountId, count);
        Shader.SetGlobalFloat(VerticalStepId, verticalStep);

        if (count == 0)
        {
            _lastActiveCount = 0;
            _activeZones.Clear();
            return;
        }

        EnsureBuffers(count);
        _lastActiveCount = count;

        for (var i = 0; i < count; i++)
            _zonesData[i] = _activeZones[i];
        _zonesBuffer.SetData(_zonesData, 0, 0, count);

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            RescanAll(count);
            _boundaryBuffer.SetData(_boundaryData, 0, 0, count * TotalLayers * MaxBoundaryPoints);
            _meridianBuffer.SetData(_meridianData, 0, 0, count * MeridianCount * MeridianBoundaryPoints);
            _timer = occlusionRescanInterval;
        }

        Shader.SetGlobalBuffer(ZonesBufferId, _zonesBuffer);
        Shader.SetGlobalBuffer(BoundaryBufferId, _boundaryBuffer);
        Shader.SetGlobalBuffer(MeridianBufferId, _meridianBuffer);

        _activeZones.Clear();
    }

    private void EnsureBuffers(int count)
    {
        if (_boundaryBuffer != null && _bufferCapacity >= count) return;

        _boundaryBuffer?.Release();
        _meridianBuffer?.Release();
        _zonesBuffer?.Release();

        _bufferCapacity = Mathf.NextPowerOfTwo(Mathf.Max(1, count));

        var ringPoints = TotalLayers * MaxBoundaryPoints;
        _boundaryBuffer = new ComputeBuffer(_bufferCapacity * ringPoints, sizeof(float) * 2);
        _boundaryData = new Vector2[_bufferCapacity * ringPoints];

        var meridianPoints = MeridianCount * MeridianBoundaryPoints;
        _meridianBuffer = new ComputeBuffer(_bufferCapacity * meridianPoints, sizeof(float) * 2);
        _meridianData = new Vector2[_bufferCapacity * meridianPoints];

        _zonesBuffer = new ComputeBuffer(_bufferCapacity, sizeof(float) * 4);
        _zonesData = new Vector4[_bufferCapacity];
    }

    private void RescanAll(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var origin = _activeZones[i];
            var range = _activeZones[i].w;

            var obstacles = Physics.OverlapSphere(origin, range, obstacleMask)
                .OrderBy(c => Vector3.Distance(origin, c.ClosestPoint(origin)))
                .Take(MaxObstacles)
                .ToList();

            for (var layer = -VerticalLayers; layer <= VerticalLayers; layer++)
            {
                var layerHeight = origin.y + layer * verticalStep;
                var layerOrigin = new Vector3(origin.x, layerHeight, origin.z);
                BuildRingForLayer(i, layer, layerOrigin, range, obstacles);
            }

            for (var m = 0; m < MeridianCount; m++)
            {
                BuildMeridian(i, m, origin, range, obstacles);
            }
        }
    }

    private void BuildRingForLayer(int lightIndex, int layer, Vector3 layerOrigin, float range, List<Collider> obstacles)
    {
        var angles = new List<float>(MaxBoundaryPoints);

        for (var b = 0; b < BaseSamples; b++)
            angles.Add((b / (float)BaseSamples) * Mathf.PI * 2f);

        foreach (var b in obstacles.Select(col => col.bounds))
        {
            Vector3[] corners =
            {
                new(b.min.x, layerOrigin.y, b.min.z),
                new(b.min.x, layerOrigin.y, b.max.z),
                new(b.max.x, layerOrigin.y, b.min.z),
                new(b.max.x, layerOrigin.y, b.max.z),
            };

            var nearestCorners = corners
                .OrderBy(c => Vector3.Distance(layerOrigin, c))
                .Take(CornersPerObstacle);

            foreach (var corner in nearestCorners)
            {
                var delta = corner - layerOrigin;
                var cornerAngle = Mathf.Atan2(delta.z, delta.x);

                angles.Add(cornerAngle - CornerEpsilonRad);
                angles.Add(cornerAngle);
                angles.Add(cornerAngle + CornerEpsilonRad);
            }
        }

        while (angles.Count < MaxBoundaryPoints)
            angles.Add(angles[angles.Count % BaseSamples] + 0.0001f * angles.Count);
        if (angles.Count > MaxBoundaryPoints)
            angles.RemoveRange(MaxBoundaryPoints, angles.Count - MaxBoundaryPoints);

        for (var a = 0; a < angles.Count; a++)
        {
            var ang = angles[a] % (Mathf.PI * 2f);
            if (ang < 0) ang += Mathf.PI * 2f;
            angles[a] = ang;
        }
        angles.Sort();

        var layerArrayIndex = layer + VerticalLayers;
        var baseIndex = (lightIndex * TotalLayers + layerArrayIndex) * MaxBoundaryPoints;

        for (var a = 0; a < MaxBoundaryPoints; a++)
        {
            var angle = angles[a];
            var dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            var heightDiff = layerOrigin.y - _zonesData[lightIndex].y;
            var maxHorizontalDist = Mathf.Sqrt(Mathf.Max(0f, range * range - heightDiff * heightDiff));

            var maxDist = maxHorizontalDist;
            if (Physics.Raycast(layerOrigin + dir * 0.05f, dir, out RaycastHit hit, Mathf.Max(0.01f, maxDist - 0.05f), obstacleMask))
                maxDist = hit.distance + wallPadding;

            _boundaryData[baseIndex + a] = new Vector2(angle, maxDist);
        }
    }

    private void BuildMeridian(int lightIndex, int meridianIndex, Vector3 origin, float range, List<Collider> obstacles)
    {
        var planeAzimuth = meridianIndex * (Mathf.PI / MeridianCount);
        var planeDir = new Vector3(Mathf.Cos(planeAzimuth), 0f, Mathf.Sin(planeAzimuth));
        var up = Vector3.up;

        var phis = new List<float>(MeridianBoundaryPoints);

        for (var b = 0; b < BaseSamplesVertical; b++)
            phis.Add((b / (float)BaseSamplesVertical) * Mathf.PI * 2f);

        foreach (var col in obstacles)
        {
            var bnd = col.bounds;
            var corners = GetBoxCorners3D(bnd);

            var nearestCorners = corners
                .OrderBy(c => Vector3.Distance(origin, c))
                .Take(CornersPerObstacle);

            foreach (var corner in nearestCorners)
            {
                var delta = corner - origin;
                var r = Vector3.Dot(new Vector3(delta.x, 0f, delta.z), planeDir); // проекция на плоскость (может быть отрицательной)
                var h = delta.y;
                var phi = Mathf.Atan2(h, r);

                phis.Add(phi - CornerEpsilonRad);
                phis.Add(phi);
                phis.Add(phi + CornerEpsilonRad);
            }
        }

        while (phis.Count < MeridianBoundaryPoints)
            phis.Add(phis[phis.Count % BaseSamplesVertical] + 0.0001f * phis.Count);
        if (phis.Count > MeridianBoundaryPoints)
            phis.RemoveRange(MeridianBoundaryPoints, phis.Count - MeridianBoundaryPoints);

        for (var a = 0; a < phis.Count; a++)
        {
            var p = phis[a] % (Mathf.PI * 2f);
            if (p < 0) p += Mathf.PI * 2f;
            phis[a] = p;
        }
        phis.Sort();

        var baseIndex = (lightIndex * MeridianCount + meridianIndex) * MeridianBoundaryPoints;

        for (var a = 0; a < MeridianBoundaryPoints; a++)
        {
            var phi = phis[a];
            var dir3D = planeDir * Mathf.Cos(phi) + up * Mathf.Sin(phi);

            var maxDist = range;
            if (Physics.Raycast(origin + dir3D * 0.05f, dir3D, out RaycastHit hit, range - 0.05f, obstacleMask))
                maxDist = hit.distance + wallPadding;

            _meridianData[baseIndex + a] = new Vector2(phi, maxDist);
        }
    }

    private static Vector3[] GetBoxCorners3D(Bounds b)
    {
        return new[]
        {
            new Vector3(b.min.x, b.min.y, b.min.z),
            new Vector3(b.min.x, b.min.y, b.max.z),
            new Vector3(b.max.x, b.min.y, b.min.z),
            new Vector3(b.max.x, b.min.y, b.max.z),
            new Vector3(b.min.x, b.max.y, b.min.z),
            new Vector3(b.min.x, b.max.y, b.max.z),
            new Vector3(b.max.x, b.max.y, b.min.z),
            new Vector3(b.max.x, b.max.y, b.max.z),
        };
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (_boundaryData == null || _lastActiveCount == 0) return;

        for (var i = 0; i < _lastActiveCount; i++)
        {
            Vector3 origin = _zonesData[i];

            Gizmos.color = gizmoRingColor;
            for (var layer = -VerticalLayers; layer <= VerticalLayers; layer++)
            {
                var layerArrayIndex = layer + VerticalLayers;
                var baseIndex = (i * TotalLayers + layerArrayIndex) * MaxBoundaryPoints;
                var layerY = origin.y + layer * verticalStep;

                for (var a = 0; a < MaxBoundaryPoints; a++)
                {
                    var p0 = _boundaryData[baseIndex + a];
                    var p1 = _boundaryData[baseIndex + (a + 1) % MaxBoundaryPoints];

                    var w0 = new Vector3(origin.x + Mathf.Cos(p0.x) * p0.y, layerY, origin.z + Mathf.Sin(p0.x) * p0.y);
                    var w1 = new Vector3(origin.x + Mathf.Cos(p1.x) * p1.y, layerY, origin.z + Mathf.Sin(p1.x) * p1.y);

                    Gizmos.DrawLine(w0, w1);
                }
            }

            Gizmos.color = gizmoMeridianColor;
            for (var m = 0; m < MeridianCount; m++)
            {
                var planeAzimuth = m * (Mathf.PI / MeridianCount);
                var planeDir = new Vector3(Mathf.Cos(planeAzimuth), 0f, Mathf.Sin(planeAzimuth));
                var up = Vector3.up;

                var baseIndex = (i * MeridianCount + m) * MeridianBoundaryPoints;

                for (var a = 0; a < MeridianBoundaryPoints; a++)
                {
                    var p0 = _meridianData[baseIndex + a];
                    var p1 = _meridianData[baseIndex + (a + 1) % MeridianBoundaryPoints];

                    var w0 = origin + planeDir * Mathf.Cos(p0.x) * p0.y + up * Mathf.Sin(p0.x) * p0.y;
                    var w1 = origin + planeDir * Mathf.Cos(p1.x) * p1.y + up * Mathf.Sin(p1.x) * p1.y;

                    Gizmos.DrawLine(w0, w1);
                }
            }
        }
    }

    private void OnDestroy()
    {
        _boundaryBuffer?.Release();
        _meridianBuffer?.Release();
        _zonesBuffer?.Release();
    }
}