using Entity;
using Game.Entity.Stats;
using Game.Scripts.GameFiles.Items;
using VContainer;

namespace Game.Entity
{
    public class MainCharacterModel : DamagableModel
    {
        private PlayerInteraction _playerInteraction;
        private PlayerInventory _playerInventory;

        private float _speed = 140f;
        private float _sprintMultiplier = 1.5f;
        private float _jumpHeight = 2f;
        private float _gravity = 9.81f;

        private float _baseCarry = 15f;
        private float _maxHoldDistance = 3.5f;
        private float _holdSoftZone = 3f;
        private float _throwForceGrow = 5f;
        private float _maxThrowForce = 15f;
        private float _minChargeTime = 0.3f;

        private float _jointSpring = 3000f;
        private float _jointDamper = 50f;
        private float _angularSpring = 500f;
        private float _angularDamper = 10f;

        private float _baseHoldForce = 500f;
        private float _holdDamping = 50f;
        private float _maxAngularSpeed = 20f;
        private float _angularResponsiveness = 0.6f;

        private float _strength = 10f;

        public float Speed => _speed;
        public float SprintMultiplier => _sprintMultiplier;
        public float JumpHeight => _jumpHeight;
        public float Gravity => _gravity;
        public float BaseCarry => _baseCarry;
        public float MaxHoldDistance => _maxHoldDistance;
        public float HoldSoftZone => _holdSoftZone;
        public float ThrowForceGrow => _throwForceGrow;
        public float MaxThrowForce => _maxThrowForce;
        public float MinChargeTime => _minChargeTime;
        public float JointSpring => _jointSpring;
        public float JointDamper => _jointDamper;
        public float AngularSpring => _angularSpring;
        public float AngularDamper => _angularDamper;
        public float BaseHoldForce => _baseHoldForce;
        public float HoldDamping => _holdDamping;
        public float MaxAngularSpeed => _maxAngularSpeed;
        public float AngularResponsiveness => _angularResponsiveness;
        public float Strength => _strength;

        public PlayerInteraction PlayerInteraction => _playerInteraction;
        public PlayerInventory PlayerInventory => _playerInventory;

        public void SetStats(PlayerStats stats)
        {
            _speed = stats.Speed;
            _sprintMultiplier = stats.SprintMultiplier;
            _jumpHeight = stats.JumpHeight;
            _gravity = stats.Gravity;
            _baseCarry = stats.BaseCarry;
            _maxHoldDistance = stats.MaxHoldDistance;
            _holdSoftZone = stats.HoldSoftZone;
            _throwForceGrow = stats.ThrowForceGrow;
            _maxThrowForce = stats.MaxThrowForce;
            _minChargeTime = stats.MinChargeTime;
            _jointSpring = stats.JointSpring;
            _jointDamper = stats.JointDamper;
            _angularSpring = stats.AngularSpring;
            _angularDamper = stats.AngularDamper;
            _baseHoldForce = stats.BaseHoldForce;
            _holdDamping = stats.HoldDamping;
            _maxAngularSpeed = stats.MaxAngularSpeed;
            _angularResponsiveness = stats.AngularResponsiveness;
            _strength = stats.Strength;
        }

        public void SetPlayerInteraction(PlayerInteraction playerInteraction) => _playerInteraction = playerInteraction;
        public void SetPlayerInventory(PlayerInventory playerInventory) => _playerInventory = playerInventory;
    }
}