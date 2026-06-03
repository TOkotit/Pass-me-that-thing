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

        public override void OnStartClient()
        {
            var scope = LifetimeScope.Find<GameplayScope>();
            if (scope)
                scope.Container.Inject(this);
            base.OnStartClient(); 
            Initialize();
            
        }
        public override void OnStartServer()
        {
            LifetimeScope.Find<GameplayScope>().Container.Inject(this);
            Initialize();
            _damagableRegistry.Register(this);
        }

        public override void OnDeath()
        {
            verticalAlign.CmdSetConsciousness(0);
            verticalAlign.Consciousness = 0f;
            verticalAlign.LockConsciousness = true;
            movement.LockUpMovement();
            mCamera.IsCameraRotating = false;
            Debug.Log("Вот тут смерть");
            
            if (!isLocalPlayer) return;
            _localModel.IsDead = true;
        }

        public override void OnHealthChanged(int currentHealth)
        {
            if (!isLocalPlayer) return;
            
            Debug.Log("Health: " + currentHealth);
            _localModel.Health = currentHealth;
            if (DamagableModel != null && DamagableModel?.HealthPool != null)
            {
                _localModel.MaxHealth = DamagableModel.HealthPool.MaxHealth;
            }
        }
    }
}