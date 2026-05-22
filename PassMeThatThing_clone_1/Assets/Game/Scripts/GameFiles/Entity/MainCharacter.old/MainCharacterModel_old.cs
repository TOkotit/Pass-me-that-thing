using System.Collections.Generic;
using Entity;
using Enums;
using NUnit.Framework;
using R3;
using UnityEngine;

namespace MainCharacter_old
{
    public class MainCharacterModel_old : DamagableModel
    {
        private Stamina _stamina;
        public Stamina  Stamina { get => _stamina; set => _stamina = value; }
        
        private float _speed = 10;
        private float _jumpHeight = 7;
        private int _wallJumpCost = 10;
        private int _dashCost = 20;
        private float _dashSpeed = 50;
        private float _dashTime = 0.1f;
        private float _dashCooldown = 1f;
        private int _wallJumpCount = 1;
        private float _parryReloadDelay = 1f;
        private float _parryDuration = 0.5f;

        public float Speed {get => _speed; set => _speed = value; }
        public float JumpHeight {get => _jumpHeight; set => _jumpHeight = value; }
        public int WallJumpCost {get  => _wallJumpCost; set => _wallJumpCost = value; }
        public int DashCost { get => _dashCost; set => _dashCost = value; }
        public float DashSpeed {get  => _dashSpeed; set => _dashSpeed = value; }
        public float DashTime {get   => _dashTime; set => _dashTime = value; }
        public float DashCooldown {get  => _dashCooldown; set => _dashCooldown = value; }
        public int WallJumpCount { get => _wallJumpCount; set => _wallJumpCount = value; }
        public float ParryReloadDelay { get => _parryReloadDelay; set => _parryReloadDelay = value; }
        public float ParryDuration { get => _parryDuration; set => _parryDuration = value; }
        
        
        public MainCharacterModel_old(Stamina stamina, Health health) : base()
        {
            _stamina = stamina;
            //_health = health;
        }
    }
}