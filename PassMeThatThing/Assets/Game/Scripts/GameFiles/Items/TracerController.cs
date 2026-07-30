using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    [RequireComponent(typeof(LineRenderer))]
    public class TracerController : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private LayerMask hitLayers = ~0;
        [SerializeField] private float maxDistance = 100f;

        [Header("Dissolve Animation")]
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Material Properties (shader floats)")]
        [SerializeField] private string pathDissolveName = "_Path_Dissolve";
        [SerializeField] private string dissolveName = "_Dissolve";

        [SerializeField] private LineRenderer lineRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private int _pathDissolveID;
        private int _dissolveID;
        private float _currentValue = 0f;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _pathDissolveID = Shader.PropertyToID(pathDissolveName);
            _dissolveID = Shader.PropertyToID(dissolveName);

            lineRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(_pathDissolveID, 0f);
            _propertyBlock.SetFloat(_dissolveID, 0f);
            lineRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void Shoot(Vector3 origin, Vector3 direction)
        {
            if (lineRenderer == null)
                Debug.LogError("lineRenderer is NULL!");
            else
                Debug.Log("lineRenderer is assigned: " + lineRenderer.gameObject.name);
            var endPoint = origin + direction * maxDistance;
            if (Physics.Raycast(origin, direction, out var hit, maxDistance, hitLayers))
            {
                endPoint = hit.point;
            }

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, endPoint);

            StopAllCoroutines();
            StartCoroutine(DissolveRoutine());
        }

        private System.Collections.IEnumerator DissolveRoutine()
        {
            var elapsed = 0f;
            _currentValue = 0f;
            lineRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(_pathDissolveID, 0f);
            _propertyBlock.SetFloat(_dissolveID, 0f);
            lineRenderer.SetPropertyBlock(_propertyBlock);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                _currentValue = dissolveCurve.Evaluate(t);

                lineRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetFloat(_pathDissolveID, _currentValue);
                _propertyBlock.SetFloat(_dissolveID, _currentValue);
                lineRenderer.SetPropertyBlock(_propertyBlock);

                yield return null;
            }

            lineRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(_pathDissolveID, 1f);
            _propertyBlock.SetFloat(_dissolveID, 1f);
            lineRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}