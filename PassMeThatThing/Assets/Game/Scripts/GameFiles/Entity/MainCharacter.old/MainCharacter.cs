using System;
using System.Collections.Generic;
using Entity;
using JetBrains.Annotations;
using Systems;
using UnityEngine;
using VContainer;

namespace MainCharacter
{
    public class MainCharacter : Damagable
    {
        private MainCharacterModel _mainCharacterModel;
        [Inject] private CharacterController _characterController;
        [Inject]
        private void SetupModel(Stamina stamina, Health health, MovementStatsSO stats, [CanBeNull] MainCharacterModel mainCharacterModel, CombatStatsSO  combatStats)
        {
            _mainCharacterModel = mainCharacterModel;
            mainCharacterModel.Stamina = stamina;
            //mainCharacterModel.Health = health;
            
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
        public MainCharacterModel MainCharacterModel => _mainCharacterModel;
        [SerializeField] private GameObject arms;
        public GameObject Arms => arms;
        
    }
}