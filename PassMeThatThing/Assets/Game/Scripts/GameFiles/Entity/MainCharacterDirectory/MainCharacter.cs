// Game.Entity.MainCharacter
using DI;
using Entity;
using Game.Scripts.GameFiles.Items;
using MainCharacter_old;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Entity
{
    public class MainCharacter : Damagable
    {
        [Inject] private DamagableRegistry _damagableRegistry;
        [Inject] private MainCharacterModel _model;
        [Inject] private MCLocalModel _localModel;
        [SerializeField] private BodyVerticalAlign verticalAlign;
        [SerializeField] private MainCharacterMovement movement;
        [SerializeField] private MainCharacterCamera mCamera;

        public MainCharacterModel MainCharacterModel => _model;

        public override DamagableModel DamagableModel => _model;

        private void Initialize()
        {
            var playerInteraction = GetComponent<PlayerInteraction>();
            var playerInventory = GetComponent<PlayerInventory>();
            _model.SetPlayerInteraction(playerInteraction);
            _model.SetPlayerInventory(playerInventory);
        }

        public new void Start()
        {
            base.Start();
            Initialize();
            
            if (isServer)
                ServerSetMaxHealth(100); //SO
        }

        // public override void OnStartClient()
        // {
        //     Initialize();
        // }
        // public override void OnStartServer()
        // {
        //     Initialize();
        //     _damagableRegistry.Register(this);
        // }

        public override void OnDeath()
        {
            verticalAlign.CmdSetConsciousness(0);
            verticalAlign.Consciousness = 0f;
            verticalAlign.LockConsciousness = true;
            movement.LockUpMovement();
            mCamera.IsCameraRotating = false;
            
            
            if (!isLocalPlayer) return;
            
            Debug.Log("[MainCharacter] OnDeath");
            
            _localModel.IsDead = true;
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            if (!isLocalPlayer) return;
            
            Debug.Log($"[MainCharacter] OnHealthChanged {currentHealth}");
            
            _localModel.Health = currentHealth;
            if (DamagableModel != null && DamagableModel?.HealthPool != null)
            {
                _localModel.MaxHealth = DamagableModel.HealthPool.MaxHealth;
            }
        }
    }
}