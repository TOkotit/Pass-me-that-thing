using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace MainCharacter_old
{
    public class LocalVisionShaderApplier : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Material visionMaterial;
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private float radius = 10f;
        [SerializeField, Range(0, 1f)] private float outlineWidth = 0.05f;

        [Header("References")]
        [SerializeField] private Transform targetTransform;

        private static readonly int PlayerPosId = Shader.PropertyToID("_GlobalVisionPos");
        private static readonly int RadiusId = Shader.PropertyToID("_Radius");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int EnabledId = Shader.PropertyToID("_Enabled");
        
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private bool _isActive;
        private readonly List<VisionData> _trackedRenderers = new List<VisionData>();

        private struct VisionData
        {
            public Renderer renderer;
            public Material[] originalMaterials;
            public Material[] instantiatedMaterials;
        }

        public override void OnStartLocalPlayer()
        {
            PrepareVisionData();
            Shader.SetGlobalFloat(RadiusId, 0f);
        }

        public override void OnStopLocalPlayer()
        {
            RestoreOriginalState();
            CleanupMaterials();
        }

        private void Update()
        {
            if (!_isActive || !isLocalPlayer) return;

            Vector3 pos = targetTransform ? targetTransform.position : transform.position;
            Shader.SetGlobalVector(PlayerPosId, pos);
            Shader.SetGlobalFloat(RadiusId, radius);
        }

        public void EnableVision()
        {
            if (_isActive) return;
            _isActive = true;

            foreach (var data in _trackedRenderers)
            {
                if (data.renderer != null)
                {
                    data.renderer.sharedMaterials = data.instantiatedMaterials;
                    foreach (var mat in data.instantiatedMaterials)
                    {
                        if (mat != null && mat.HasProperty(EnabledId))
                            mat.SetFloat(EnabledId, 1f);
                    }
                }
            }
        }

        public void DisableVision()
        {
            if (!_isActive) return;
            _isActive = false;

            Shader.SetGlobalFloat(RadiusId, 0f);

            foreach (var data in _trackedRenderers)
            {
                if (data.renderer != null)
                {
                    data.renderer.sharedMaterials = data.originalMaterials;
                }
            }
        }

        private void PrepareVisionData()
        {
            CleanupMaterials();
            var allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            
            foreach (var r in allRenderers)
            {
                // Оставляем только базовые проверки на вшивость
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (r.transform.IsChildOf(transform)) continue; // Чтобы не обводить самого себя
                if (r is CanvasRenderer) continue; // Игнорим UI

                _trackedRenderers.Add(new VisionData
                {
                    renderer = r,
                    originalMaterials = r.sharedMaterials,
                    instantiatedMaterials = CreateEffectMaterialArray(r)
                });
            }
            Debug.Log($"[Vision] Подготовка завершена. Объектов в списке: {_trackedRenderers.Count}");
        }

        private Material[] CreateEffectMaterialArray(Renderer r)
        {
            var originals = r.sharedMaterials;
            var newMats = new Material[originals.Length + 1];

            for (int i = 0; i < originals.Length; i++)
            {
                if (originals[i] == null) continue;

                var mat = new Material(visionMaterial);
                
                if (originals[i].HasProperty("_MainTex")) mat.SetTexture(BaseMapId, originals[i].mainTexture);
                else if (originals[i].HasProperty("_BaseMap")) mat.SetTexture(BaseMapId, originals[i].GetTexture("_BaseMap"));

                if (originals[i].HasProperty("_Color")) mat.SetColor(BaseColorId, originals[i].color);
                else if (originals[i].HasProperty("_BaseColor")) mat.SetColor(BaseColorId, originals[i].GetColor("_BaseColor"));

                newMats[i] = mat;
            }

            var outMat = new Material(outlineMaterial);
            outMat.SetFloat(OutlineWidthId, outlineWidth);
            outMat.SetFloat(EnabledId, 0f);
            newMats[newMats.Length - 1] = outMat;

            return newMats;
        }

        private void RestoreOriginalState()
        {
            foreach (var data in _trackedRenderers)
                if (data.renderer != null) data.renderer.sharedMaterials = data.originalMaterials;
        }

        private void CleanupMaterials()
        {
            foreach (var data in _trackedRenderers)
            {
                if (data.instantiatedMaterials == null) continue;
                foreach (var m in data.instantiatedMaterials) if (m != null) Destroy(m);
            }
            _trackedRenderers.Clear();
        }

        private void OnDestroy() => CleanupMaterials();
    }
}