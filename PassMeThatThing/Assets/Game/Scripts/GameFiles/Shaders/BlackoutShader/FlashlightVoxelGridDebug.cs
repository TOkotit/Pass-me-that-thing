using UnityEngine;

namespace Game.Scripts.GameFiles.GameRandomEvents.Blackout
{
    /// <summary>
    /// Воксельная сетка объёма Spot-света с учётом препятствий (рейкаст вместо честной shadow map).
    /// Шаг 1: только построение сетки + отрисовка в гизмо. В шейдер/буферы пока НЕ пробрасывается.
    /// Работает чисто локально (не NetworkBehaviour) — это визуальный дебаг, сети не требует.
    /// </summary>
    [DisallowMultipleComponent]
    public class FlashlightVoxelGridDebug : MonoBehaviour
    {
        [Header("Источник")]
        [SerializeField] private Light _light;

        [Header("Разрешение сетки")]
        [Tooltip("Кол-во вокселей по X и Y (поперёк конуса)")]
        [SerializeField, Min(2)] private int _resolutionAngular = 12;
        [Tooltip("Кол-во слоёв по Z (вдоль дальности)")]
        [SerializeField, Min(2)] private int _resolutionDepth = 16;

        [Header("Окклюзия")]
        [SerializeField] private LayerMask _obstacleMask = ~0;
        [Tooltip("Отступ от источника перед рейкастом, чтобы не цепляться за коллайдер самого игрока/фонарика")]
        [SerializeField] private float _castPadding = 0.08f;

        [Header("Троттлинг обновлений")]
        [Tooltip("Минимальный интервал между пересчётами, если источник двигался/поворачивался")]
        [SerializeField] private float _rebuildInterval = 0.15f;
        [Tooltip("Пересчитывать принудительно не реже этого интервала, даже если источник неподвижен (ловим динамические препятствия — двери и т.п.)")]
        [SerializeField] private float _maxStaticInterval = 1f;
        [SerializeField] private float _moveThreshold = 0.05f;
        [SerializeField] private float _rotateThresholdDeg = 2f;

        [Header("Гизмо")]
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] private bool _drawOccludedVoxels = false;
        [SerializeField] private float _gizmoVoxelSize = 0.08f;
        [SerializeField] private Color _litColor = new Color(1f, 0.85f, 0.2f, 0.6f);
        [SerializeField] private Color _occludedColor = new Color(1f, 0f, 0f, 0.15f);

        private bool[] _voxelLit;
        private bool[] _voxelValid;      // false = воксель вне круглого сечения конуса (угол квадрата)
        private Vector3[] _voxelWorldPos;
        private int _gx, _gy, _gz;

        private float _timer;
        private Vector3 _lastOrigin;
        private Quaternion _lastRot;
        private bool _hasBuiltOnce;

        private void Awake()
        {
            if (!_light) _light = GetComponent<Light>();
        }

        private void Update()
        {
            if (!_light || !_light.enabled) return;
            if (_light.type != LightType.Spot) return;

            _timer += Time.deltaTime;

            var moved = !_hasBuiltOnce || Vector3.Distance(_lastOrigin, transform.position) > _moveThreshold;
            var rotated = !_hasBuiltOnce || Quaternion.Angle(_lastRot, transform.rotation) > _rotateThresholdDeg;

            var dueToMovement = (moved || rotated) && _timer >= _rebuildInterval;
            var dueToStaticRefresh = _timer >= _maxStaticInterval;

            if (!dueToMovement && !dueToStaticRefresh) return;

            _timer = 0f;
            _lastOrigin = transform.position;
            _lastRot = transform.rotation;
            _hasBuiltOnce = true;

            RebuildVoxelGrid();
        }

        private void RebuildVoxelGrid()
        {
            var range = _light.range;
            var halfAngleRad = _light.spotAngle * 0.5f * Mathf.Deg2Rad;

            _gx = _resolutionAngular;
            _gy = _resolutionAngular;
            _gz = _resolutionDepth;

            var count = _gx * _gy * _gz;
            if (_voxelLit == null || _voxelLit.Length != count)
            {
                _voxelLit = new bool[count];
                _voxelValid = new bool[count];
                _voxelWorldPos = new Vector3[count];
            }

            var origin = transform.position;
            var fwd = transform.forward;
            var right = transform.right;
            var up = transform.up;

            for (var iz = 0; iz < _gz; iz++)
            {
                // квадратичное сгущение слоёв у источника
                var tZ = (iz + 0.5f) / _gz;
                var depth = tZ * tZ * range;
                var radiusAtDepth = Mathf.Tan(halfAngleRad) * depth;

                for (var iy = 0; iy < _gy; iy++)
                for (var ix = 0; ix < _gx; ix++)
                {
                    var idx = FlattenIndex(ix, iy, iz);

                    var uvx = (ix + 0.5f) / _gx * 2f - 1f;
                    var uvy = (iy + 0.5f) / _gy * 2f - 1f;

                    // вписываем круглое сечение конуса в квадратную сетку
                    if (uvx * uvx + uvy * uvy > 1f)
                    {
                        _voxelValid[idx] = false;
                        continue;
                    }

                    var localOffset = right * (uvx * radiusAtDepth) + up * (uvy * radiusAtDepth);
                    var worldPos = origin + fwd * depth + localOffset;

                    _voxelValid[idx] = true;
                    _voxelWorldPos[idx] = worldPos;
                    _voxelLit[idx] = TestVoxelVisibility(origin, worldPos);
                }
            }
        }

        private bool TestVoxelVisibility(Vector3 origin, Vector3 target)
        {
            var toTarget = target - origin;
            var dist = toTarget.magnitude;
            if (dist < 0.0001f) return true;

            var dir = toTarget / dist;
            var castOrigin = origin + dir * _castPadding;
            var castDist = Mathf.Max(0f, dist - _castPadding);

            // бинарный тест видимости — ровно то же самое, что depth-compare в shadow map,
            // только на CPU и через физические коллайдеры вместо depth-буфера
            return !Physics.Raycast(castOrigin, dir, castDist, _obstacleMask, QueryTriggerInteraction.Ignore);
        }

        private int FlattenIndex(int x, int y, int z) => (z * _gy + y) * _gx + x;

        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos || _voxelLit == null) return;

            var size = Vector3.one * _gizmoVoxelSize;

            for (var i = 0; i < _voxelLit.Length; i++)
            {
                if (!_voxelValid[i]) continue;

                if (_voxelLit[i])
                {
                    Gizmos.color = _litColor;
                    Gizmos.DrawCube(_voxelWorldPos[i], size);
                }
                else if (_drawOccludedVoxels)
                {
                    Gizmos.color = _occludedColor;
                    Gizmos.DrawCube(_voxelWorldPos[i], size);
                }
            }
        }
    }
}