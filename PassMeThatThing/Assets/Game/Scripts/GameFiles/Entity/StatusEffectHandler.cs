using System.Collections.Generic;
using Entity;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity
{
    public class StatusEffectHandler : NetworkBehaviour
    {
        [SerializeField] private Damageable owner; 
        private readonly List<IStatusEffect> _statusEffects = new List<IStatusEffect>();
        private readonly Dictionary<IStatusEffect, float> _tickTimers = new Dictionary<IStatusEffect, float>();

        private void Update()
        {
            if (!isServer) return;

            var delta = Time.deltaTime;

            for (int i = _statusEffects.Count - 1; i >= 0; i--)
            {
                var effect = _statusEffects[i];

                if (!_tickTimers.TryGetValue(effect, out float timer))
                    timer = 0f;

                timer += delta;

                if (timer >= effect.TickRate)
                {
                    timer -= effect.TickRate;
                    _tickTimers[effect] = timer;
                    effect.OnTick(owner);
                }
                else
                {
                    _tickTimers[effect] = timer;
                }
            }
        }

        public void AddEffect(IStatusEffect effect, int stackCount = 1)
        {
            if (effect == null || stackCount <= 0) return;

            var existing = _statusEffects.Find(e => e.GetType() == effect.GetType());

            if (existing != null)
            {
                existing.OnReapply(owner); 
            }
            else
            {
                effect.OnApply(owner, stackCount); 
                _statusEffects.Add(effect);
                _tickTimers[effect] = 0f;
            }
        }

        public void RemoveEffect(IStatusEffect effect)
        {
            if (effect == null) return;

            effect.OnEndEffect(owner);
            _statusEffects.Remove(effect);
            _tickTimers.Remove(effect);
        }

        public void ClearAllEffects()
        {
            foreach (var effect in _statusEffects)
            {
                effect.OnEndEffect(owner);
            }
            _statusEffects.Clear();
            _tickTimers.Clear();
        }
    }
}