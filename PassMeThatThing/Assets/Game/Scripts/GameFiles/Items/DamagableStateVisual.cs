using System.Collections;
using System.Collections.Generic;
using Entity;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items
{
    public enum StateSource
    {
        Damage,
        Burn,
        Frost
    }
    
    public class DamageableStateView : NetworkBehaviour
    {
        [SerializeField] private List<Renderer> _renderers = new();
        [SerializeField] private Damageable damageable;
        [SerializeField] private string _colorProperty = "_BaseColor";

        [Inject] private DamageableStateColorData _stateColors;

        private MaterialPropertyBlock _mpb;
        private int _colorId = -1;
        private bool _subscribed;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            _colorId = Shader.PropertyToID(_colorProperty);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            TrySubscribe();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            TrySubscribe();
        }

        private void OnDestroy() => Unsubscribe();

        private void TrySubscribe()
        {
            if (_subscribed) return;

            if (!damageable)
            {
                StartCoroutine(WaitAndSubscribe());
                return;
            }

            damageable.HealthChanged -= OnHealthChanged;
            damageable.HealthChanged += OnHealthChanged;
            _subscribed = true;

            if (damageable.DamagableModel?.HealthPool != null)
            {
                OnHealthChanged(damageable.DamagableModel.HealthPool.CurrentHealth,
                                damageable.DamagableModel.HealthPool.MaxHealth);
            }
        }

        private IEnumerator WaitAndSubscribe()
        {
            while (!damageable)
                yield return null;

            TrySubscribe();
        }

        private void Unsubscribe()
        {
            if (damageable)
                damageable.HealthChanged -= OnHealthChanged;

            _subscribed = false;
        }

        private void OnHealthChanged(int current, int max) => Refresh(current, max);

        private void Refresh(int current, int max)
        {
            if (max <= 0) return;

            var healthRatio = Mathf.Clamp01((float)current / max);
            var damageIntensity = 1f - healthRatio;

            var damageColor = Color.gray;
            if (_stateColors && _stateColors.colors != null
                             && _stateColors.colors.TryGetValue(StateSource.Damage, out var c))
            {
                damageColor = c;
            }

            var tint = Color.Lerp(Color.white, damageColor, damageIntensity);
            ApplyColor(tint);
        }

        private void ApplyColor(Color color)
        {
            if (_renderers == null) return;

            foreach (var r in _renderers)
            {
                if (!r) continue;

                var mat = r.sharedMaterial;
                if (!mat || !mat.HasProperty(_colorId)) continue;

                r.GetPropertyBlock(_mpb);
                _mpb.SetColor(_colorId, color);
                r.SetPropertyBlock(_mpb);
            }
        }
    }
}