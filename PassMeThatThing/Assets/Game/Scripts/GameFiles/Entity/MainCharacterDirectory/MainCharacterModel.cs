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
        public PlayerInteraction PlayerInteraction => _playerInteraction;
        public PlayerInventory PlayerInventory => _playerInventory;

        private float _speed = 140f;
        private float _sprintMultiplier = 1.5f;
        private float _jumpHeight = 2f;
        private float _gravity = 9.81f;
        private float _baseCarry = 0.5f;
        
        public float Speed => _speed;
        public float SprintMultiplier => _sprintMultiplier;
        public float JumpHeight => _jumpHeight;
        public float Gravity => _gravity;
        public float BaseCarry => _baseCarry;

        public void SetStats(PlayerStats stats)
        {
            _speed = stats.Speed;
            _sprintMultiplier = stats.SprintMultiplier;
            _jumpHeight = stats.JumpHeight;
            _gravity = stats.Gravity;
            _baseCarry = stats.BaseCarry;
        }
        
        public void SetPlayerInteraction(PlayerInteraction playerInteraction)
        {
            _playerInteraction = playerInteraction;
        }

        public void SetPlayerInventory(PlayerInventory playerInventory)
        {
            _playerInventory = playerInventory;
        }
    }
}