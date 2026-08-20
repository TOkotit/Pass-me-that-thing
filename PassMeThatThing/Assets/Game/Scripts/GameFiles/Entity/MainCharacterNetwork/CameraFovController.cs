using System.Collections;
using UnityEngine;

namespace MainCharacterNetwork
{
    public class CameraFovController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float smoothSpeed = 10f;

        private float _baseFov = 60f;
        private float _currentFovIncrease = 0f;
        private float _targetFov;
        private bool _isInitialized;

        public void Initialize(float baseFov)
        {
            _baseFov = baseFov;
            _targetFov = baseFov;
            if (targetCamera)
                targetCamera.fieldOfView = _baseFov;
            _isInitialized = true;
        }

        /// <summary>
        /// Временно увеличивает FOV (например, при выстреле).
        /// </summary>
        /// <param name="amount">Насколько увеличить за один вызов.</param>
        /// <param name="maxIncrease">Максимально допустимое суммарное увеличение от этого источника.</param>
        /// <param name="duration">Время, за которое увеличение плавно вернётся к нулю.</param>
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
            targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, _targetFov, Time.deltaTime * smoothSpeed);
        }
    }
}