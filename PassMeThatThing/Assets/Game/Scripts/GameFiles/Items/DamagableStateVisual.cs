using System;
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

        [Inject] private DamageableStateColorSO _stateColors;

        private MaterialPropertyBlock _mpb;
        private int _colorId;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            _colorId = Shader.PropertyToID(_colorProperty);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            Subscribe();
            Refresh();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Subscribe();
            Refresh();
        }

        private void OnDestroy() => Unsubscribe();

        private void Subscribe()
        {
            if (damageable)
                damageable.DamagableModel.OnHealthChanged += OnHealthChanged;
        }

        private void Unsubscribe()
        {
            if (damageable)
                damageable.DamagableModel.OnHealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int current, int max) => Refresh();

        private void Refresh()
        {
            if (damageable?.DamagableModel?.HealthPool == null) return;

            var current = damageable.DamagableModel.HealthPool.CurrentHealth;
            var max = damageable.DamagableModel.HealthPool.MaxHealth;
            if (max <= 0) return;

            var healthRatio = Mathf.Clamp01((float)current / max);
            var damageIntensity = 1f - healthRatio;
            var damageColor = _stateColors.colors[StateSource.Damage];
            var tint = Color.Lerp(Color.white, damageColor, damageIntensity);

            foreach (var r in _renderers)
            {
                if (!r) continue;
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor(_colorId, tint);
                r.SetPropertyBlock(_mpb);
            }
        }

        private void ApplyColor(Color color)
        {
            foreach (var r in _renderers)
            {
                if (!r) continue;
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor(_colorId, color);
                r.SetPropertyBlock(_mpb);
            }
        }
    }
}