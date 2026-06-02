// Game.Entity.MainCharacter
using DI;
using Entity;
using Game.Scripts.GameFiles.Items;
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