// Game/Entity/MainCharacter.cs
using DI;
using Entity;
using Entity.Entity;
using Game.Scripts.GameFiles.Entity;
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

        public MainCharacterModel MainCharacterModel => _model;

        private void Initialize()
        {
            var playerInteraction = GetComponent<PlayerInteraction>();
            var playerInventory = GetComponent<PlayerInventory>();
            _model.SetPlayerInteraction(playerInteraction);
            _model.SetPlayerInventory(playerInventory);
        }

        public override void OnStartClient()
        {
            Initialize();
        }

        public override void OnStartServer()
        {
            LifetimeScope.Find<GameplayScope>().Container.Inject(this);
            _model.HealthPool = new HealthPool(health);
            Initialize();
            _damagableRegistry.Register(this);
        }

        public override DamagableModel DamagableModel => _model;
    }
}