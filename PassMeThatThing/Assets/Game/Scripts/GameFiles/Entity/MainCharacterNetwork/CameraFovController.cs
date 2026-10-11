using System.Collections;
using UnityEngine;

namespace MainCharacterNetwork
{
    public class CameraFovController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float smoothSpeed = 10f;

        [Header("Safety")]
        [SerializeField] private float minValidFov = 10f;
        [SerializeField] private float maxValidFov = 170f;

        private float _baseFov = 60f;
        private float _currentFovIncrease = 0f;
        private float _targetFov;
        private bool _isInitialized;

        public void Initialize(float baseFov)
        {
            if (float.IsNaN(baseFov) || float.IsInfinity(baseFov) ||
                baseFov < minValidFov || baseFov > maxValidFov)
            {
                Debug.LogWarning($"[CameraFovController] Некорректный baseFov = {baseFov}. " +
                                 $"Использую fallback 60.");
                baseFov = 60f;
            }

            _baseFov = baseFov;
            _currentFovIncrease = 0f;
            _targetFov = _baseFov;

            if (targetCamera)
            {
                if (float.IsNaN(targetCamera.fieldOfView) ||
                    targetCamera.fieldOfView < minValidFov ||
                    targetCamera.fieldOfView > maxValidFov)
                {
                    targetCamera.fieldOfView = _baseFov;
                }
            }
            else
            {
                Debug.LogError($"[CameraFovController] targetCamera == null на {name}. " +
                               $"FOV не будет работать.");
            }

            _isInitialized = true;
        }

        public void AddFovKick(float amount, float maxIncrease, float duration)
        {
            if (!_isInitialized) return;

            _currentFovIncrease = Mathf.Clamp(_currentFovIncrease + amount, 0f, maxIncrease);
            _targetFov = _baseFov + _currentFovIncrease;

            StopAllCoroutines();
            StartCoroutine(FovReturnRoutine(duration));
        }

        private IEnumerator FovReturnRoutine(float duration)
        {
            var startIncrease = _currentFovIncrease;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                _currentFovIncrease = Mathf.Lerp(startIncrease, 0f, t);
                _targetFov = _baseFov + _currentFovIncrease;
                yield return null;
            }
            _currentFovIncrease = 0f;
            _targetFov = _baseFov;
        }

        private void LateUpdate()
        {
            if (!_isInitialized || !targetCamera) return;
            var currentFov = targetCamera.fieldOfView;
            if (float.IsNaN(currentFov) ||
                currentFov < minValidFov ||
                currentFov > maxValidFov)
            {
                Debug.LogWarning($"[CameraFovController] Camera FOV сломан ({currentFov}), " +
                                 $"сброс на {_baseFov}");
                targetCamera.fieldOfView = _baseFov;
                return;
            }

            if (float.IsNaN(_targetFov) ||
                _targetFov < minValidFov ||
                _targetFov > maxValidFov)
            {
                Debug.LogWarning($"[CameraFovController] _targetFov сломан ({_targetFov}), " +
                                 $"сброс на {_baseFov}");
                _targetFov = _baseFov;
            }

            var t = Mathf.Clamp01(Time.deltaTime * smoothSpeed);
            targetCamera.fieldOfView = Mathf.Lerp(currentFov, _targetFov, t);
        }
    }
}