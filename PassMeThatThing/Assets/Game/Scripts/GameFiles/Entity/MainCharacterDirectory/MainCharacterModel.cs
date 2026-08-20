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

        // Базовые значения из PlayerStats
        private float _baseMaxHealth;
        private float _baseSpeed;
        private float _baseSprintMultiplier;
        private float _baseJumpHeight;
        private float _baseGravity;
        private float _baseCarry;
        private float _baseMaxHoldDistance;
        private float _baseHoldSoftZone;
        private float _baseThrowForceGrow;
        private float _baseMaxThrowForce;
        private float _baseMinChargeTime;
        private float _baseJointSpring;
        private float _baseJointDamper;
        private float _baseAngularSpring;
        private float _baseAngularDamper;
        private float _baseHoldForce;
        private float _baseHoldDamping;
        private float _baseMaxAngularSpeed;
        private float _baseAngularResponsiveness;
        private float _baseStrength;

        // Текущие значения (после применения класса)
        private float _maxHealth;
        private float _speed;
        private float _sprintMultiplier;
        private float _jumpHeight;
        private float _gravity;
        private float _carry;
        private float _maxHoldDistance;
        private float _holdSoftZone;
        private float _throwForceGrow;
        private float _maxThrowForce;
        private float _minChargeTime;
        private float _jointSpring;
        private float _jointDamper;
        private float _angularSpring;
        private float _angularDamper;
        private float _holdForce;
        private float _holdDamping;
        private float _maxAngularSpeed;
        private float _angularResponsiveness;
        private float _strength;
        private float _baseFov;
        public float MaxHealth => _maxHealth;
        public float Speed => _speed;
        public float SprintMultiplier => _sprintMultiplier;
        public float JumpHeight => _jumpHeight;
        public float Gravity => _gravity;
        public float BaseCarry => _carry;
        public float MaxHoldDistance => _maxHoldDistance;
        public float HoldSoftZone => _holdSoftZone;
        public float ThrowForceGrow => _throwForceGrow;
        public float MaxThrowForce => _maxThrowForce;
        public float MinChargeTime => _minChargeTime;
        public float JointSpring => _jointSpring;
        public float JointDamper => _jointDamper;
        public float AngularSpring => _angularSpring;
        public float AngularDamper => _angularDamper;
        public float BaseHoldForce => _holdForce;
        public float HoldDamping => _holdDamping;
        public float MaxAngularSpeed => _maxAngularSpeed;
        public float AngularResponsiveness => _angularResponsiveness;
        public float Strength => _strength;
        
        public float BaseFov => _baseFov;

        public PlayerInteraction PlayerInteraction => _playerInteraction;
        public PlayerInventory PlayerInventory => _playerInventory;

        public void SetBaseStats(PlayerStats stats)
        {
            _baseFov = stats.BaseFov;
            _baseMaxHealth = stats.MaxHealth;
            _baseSpeed = stats.Speed;
            _baseSprintMultiplier = stats.SprintMultiplier;
            _baseJumpHeight = stats.JumpHeight;
            _baseGravity = stats.Gravity;
            _baseCarry = stats.BaseCarry;
            _baseMaxHoldDistance = stats.MaxHoldDistance;
            _baseHoldSoftZone = stats.HoldSoftZone;
            _baseThrowForceGrow = stats.ThrowForceGrow;
            _baseMaxThrowForce = stats.MaxThrowForce;
            _baseMinChargeTime = stats.MinChargeTime;
            _baseJointSpring = stats.JointSpring;
            _baseJointDamper = stats.JointDamper;
            _baseAngularSpring = stats.AngularSpring;
            _baseAngularDamper = stats.AngularDamper;
            _baseHoldForce = stats.BaseHoldForce;
            _baseHoldDamping = stats.HoldDamping;
            _baseMaxAngularSpeed = stats.MaxAngularSpeed;
            _baseAngularResponsiveness = stats.AngularResponsiveness;
            _baseStrength = stats.Strength;

            ResetToBase();
        }

        // Сброс к базовым значениям
        public void ResetToBase()
        {
            _maxHealth = _baseMaxHealth;
            _speed = _baseSpeed;
            _sprintMultiplier = _baseSprintMultiplier;
            _jumpHeight = _baseJumpHeight;
            _gravity = _baseGravity;
            _carry = _baseCarry;
            _maxHoldDistance = _baseMaxHoldDistance;
            _holdSoftZone = _baseHoldSoftZone;
            _throwForceGrow = _baseThrowForceGrow;
            _maxThrowForce = _baseMaxThrowForce;
            _minChargeTime = _baseMinChargeTime;
            _jointSpring = _baseJointSpring;
            _jointDamper = _baseJointDamper;
            _angularSpring = _baseAngularSpring;
            _angularDamper = _baseAngularDamper;
            _holdForce = _baseHoldForce;
            _holdDamping = _baseHoldDamping;
            _maxAngularSpeed = _baseMaxAngularSpeed;
            _angularResponsiveness = _baseAngularResponsiveness;
            _strength = _baseStrength;
        }

        // Методы для применения множителей (вызываются ClassManager)
        public void ApplyMultipliers(ClassStats classStats)
        {
            _maxHealth = _baseMaxHealth + classStats.maxHealthBonus;
            _speed = _baseSpeed * classStats.speedMultiplier;
            _sprintMultiplier = _baseSprintMultiplier * classStats.sprintMultiplier;
            _jumpHeight = _baseJumpHeight * classStats.jumpMultiplier;
            _gravity = _baseGravity * classStats.gravityMultiplier;
            _carry = _baseCarry * classStats.baseCarryMultiplier;
            _maxHoldDistance = _baseMaxHoldDistance * classStats.maxHoldDistanceMultiplier;
            _holdSoftZone = _baseHoldSoftZone * classStats.holdSoftZoneMultiplier;
            _throwForceGrow = _baseThrowForceGrow * classStats.throwForceGrowMultiplier;
            _maxThrowForce = _baseMaxThrowForce * classStats.maxThrowForceMultiplier;
            _minChargeTime = _baseMinChargeTime * classStats.minChargeTimeMultiplier;
            _jointSpring = _baseJointSpring * classStats.jointSpringMultiplier;
            _jointDamper = _baseJointDamper * classStats.jointDamperMultiplier;
            _angularSpring = _baseAngularSpring * classStats.angularSpringMultiplier;
            _angularDamper = _baseAngularDamper * classStats.angularDamperMultiplier;
            _holdForce = _baseHoldForce * classStats.baseHoldForceMultiplier;
            _holdDamping = _baseHoldDamping * classStats.holdDampingMultiplier;
            _maxAngularSpeed = _baseMaxAngularSpeed * classStats.maxAngularSpeedMultiplier;
            _angularResponsiveness = _baseAngularResponsiveness * classStats.angularResponsivenessMultiplier;
            _strength = _baseStrength * classStats.strengthMultiplier;
        }

        public void SetPlayerInteraction(PlayerInteraction playerInteraction) => _playerInteraction = playerInteraction;
        public void SetPlayerInventory(PlayerInventory playerInventory) => _playerInventory = playerInventory;
    }
}