using Entity;
using Game.Entity;
using Game.Gameplay.Root;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using Game.Gameplay.View.UI;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.GlobalStageManager;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.Highlight;
using Game.Scripts.GameFiles.Items.ItemPhysics;

namespace DI
{
    public class GameplayScope: LifetimeScope
    {
        [SerializeField] ItemDatabase itemDatabase;
        [SerializeField] GameEventsDatabase gameEventsDatabase;
        [SerializeField] private EnemyDatabase enemyDatabase;
        [SerializeField] private GameRandomEventManager eventManagerPrefab;
        [SerializeField] private GlobalStageManager globalStageManagerPrefab;
        
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("GameplayScope.Configure called");
            
            builder.RegisterInstance(itemDatabase);
            builder.RegisterInstance(gameEventsDatabase);
            builder.RegisterInstance(enemyDatabase);

            builder.Register<PlayerInventoryModel>(Lifetime.Singleton);
            
            
            builder.RegisterComponent(eventManagerPrefab);
            builder.RegisterComponent(globalStageManagerPrefab);
            
            var physicalItemRegistry = new PhysicalItemRegistry();
            builder.RegisterInstance(physicalItemRegistry);
            
            var outlineRegistry = new OutlineRegistry();
            builder.RegisterInstance(outlineRegistry);
            
            var damagableRegistry = new DamagableRegistry();
            builder.RegisterInstance(damagableRegistry);
            
            var interactableRegistry = new InteractableRegistry();
            builder.RegisterInstance(interactableRegistry);

            var eventTerminalRegistry = new EventTerminalsRegistry();
            builder.RegisterInstance(eventTerminalRegistry);
            
            builder.Register<MainCharacterModel>(Lifetime.Transient);
            
            builder.Register<GameplayUIRootViewModel>(Lifetime.Singleton);
            builder.Register<GameplayUIManager>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<GameplayEntryPoint>(Lifetime.Singleton);
        }
    }
}