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
        [Inject]
        private DamagableRegistry _damagableRegistry;
        [Inject]
        private MainCharacterModel _model;
        public MainCharacterModel MainCharacterModel { get => _model; }

        public override void OnStartClient()
        {
            LifetimeScope.Find<GameplayScope>().Container.Inject(this);
            var playerInteraction = GetComponent<PlayerInteraction>();
            var playerInventory = GetComponent<PlayerInventory>();
            _model.SetPlayerInteraction(playerInteraction);
            _model.SetPlayerInventory(playerInventory);
            _damagableRegistry.Register(this);
        }
        
        public override void OnStartServer()
        {
            LifetimeScope.Find<GameplayScope>().Container.Inject(this);
            var playerInteraction = GetComponent<PlayerInteraction>();
            var playerInventory = GetComponent<PlayerInventory>();
            _model.SetPlayerInteraction(playerInteraction);
            _model.SetPlayerInventory(playerInventory);
            _damagableRegistry.Register(this);
        }
        public override DamagableModel DamagableModel { get => _model; }
    }
}