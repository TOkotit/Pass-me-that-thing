using System;
using System.Collections.Generic;
using Entity;
using JetBrains.Annotations;
using Systems;
using UnityEngine;
using VContainer;

namespace MainCharacter_old
{
    public class MainCharacter_old : Damagable
    {
        private MainCharacterModel_old _mainCharacterModel;
        [Inject] private CharacterController _characterController;
        [Inject]
        private void SetupModel(Stamina_old stamina, Health_old healthOld, MovementStatsSO stats, [CanBeNull] MainCharacterModel_old mainCharacterModel, CombatStatsSO  combatStats)
        {
            _mainCharacterModel = mainCharacterModel;
            mainCharacterModel.Stamina = stamina;
            //mainCharacterModel.Health_old = healthOld;
            
            _mainCharacterModel.DashCooldown = stats.DashCooldown;
            _mainCharacterModel.DashSpeed = stats.DashSpeed;
            _mainCharacterModel.DashTime = stats.DashTime;
            _mainCharacterModel.DashCost = stats.DashCost;
            _mainCharacterModel.WallJumpCount = stats.WallJumpCount;
            _mainCharacterModel.WallJumpCost = stats.@WallJumpCost;
            _mainCharacterModel.Speed = stats.Speed;
            _mainCharacterModel.JumpHeight = stats.JumpHeight;
            _mainCharacterModel.ParryReloadDelay = combatStats.ParryReloadDelay;
            _mainCharacterModel.ParryDuration = combatStats.ParryDuration;
        }
        public override DamagableModel DamagableModel => _mainCharacterModel;
        public override void OnDeath()
        {
            throw new NotImplementedException();
        }

        public override void OnHealthChanged(int diff)
        {
            throw new NotImplementedException();
        }

        public MainCharacterModel_old MainCharacterModel => _mainCharacterModel;
        [SerializeField] private GameObject arms;
        public GameObject Arms => arms;
        
    }
}