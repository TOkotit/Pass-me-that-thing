using Entity;
using Game.Entity;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.StatusEffects
{
    public class SlowStatusEffect : IStatusEffect
    {
        private readonly float _duration;          
        private readonly float _slowFactor;        
        private float _remainingTime;              
        private MainCharacter _ownerCharacter;     
        private float _originalMultiplier;         

        public float TickRate { get; set; } = 0.1f;
        public int Stacks { get; set; } = 1;       
        
        public SlowStatusEffect(float duration, float slowFactor)
        {
            _duration = duration;
            _slowFactor = Mathf.Clamp01(slowFactor);
        }

        public void OnApply(Damageable owner, int stackCount)
        {
            Stacks = 1;
            _remainingTime = _duration;

            if (owner is MainCharacter mainCharacter)
            {
                _ownerCharacter = mainCharacter;
                var model = mainCharacter.MainCharacterModel;

                _originalMultiplier = model.ExternalSpeedMultiplier;
                model.ExternalSpeedMultiplier = _originalMultiplier * (1f - _slowFactor);
            }
            else
            {
                _ownerCharacter = null;
            }
        }

        public void OnReapply(Damageable owner)
        {
            if (owner is MainCharacter mainCharacter && _ownerCharacter == mainCharacter)
            {
                _remainingTime = _duration;
            }
        }

        public void OnTick(Damageable owner)
        {
            if (!_ownerCharacter || _ownerCharacter != owner)
                return;

            _remainingTime -= TickRate;
            if (_remainingTime <= 0f)
            {
                _ownerCharacter.StatusEffectHandler?.RemoveEffect(this);
            }
        }

        public void OnEndEffect(Damageable owner)
        {
            if (owner is MainCharacter mainCharacter && _ownerCharacter == mainCharacter)
            {
                var model = mainCharacter.MainCharacterModel;
                model.ExternalSpeedMultiplier = _originalMultiplier;
            }
            _ownerCharacter = null;
        }
    }
}