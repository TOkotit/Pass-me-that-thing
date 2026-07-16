using Entity;
using Game.Entity;
using Game.Gameplay.Root;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using Game.Gameplay.View.UI;
using Game.Scripts.GameFiles.Entity.Buildings;
using Game.Scripts.GameFiles.Entity.Buildings.Misc;
using Game.Scripts.GameFiles.Entity.Buildings.Misc.Craft;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Game.Scripts.GameFiles.Entity.Enemy;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.GlobalStageManager;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.Highlight;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Game.Scripts.Systems;

namespace DI
{
    public class GameplayScope: LifetimeScope
    {
        public static IObjectResolver Resolver { get; private set; }
        
        [Header("Databases")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private GameEventsDatabase gameEventsDatabase;
        [SerializeField] private EnemyDatabase enemyDatabase;
        [SerializeField] private BuildingsDatabase buildingDatabase;
        [SerializeField] private TurretDatabase turretDatabase;
        
        [SerializeField] private ResourceDatabase resourceDatabase;
        [SerializeField] private WorkbenchItemRecipeDatabase recipeDatabase;
        
        [Header("Managers on gameplay scene")]
        [SerializeField] private GameRandomEventManager eventManagerPrefab;
        [SerializeField] private GlobalStageManager globalStageManagerPrefab;
        [SerializeField] private EnemySpawner enemySpawnerPrefab;
        [SerializeField] private BuildingManager buildingManagerPrefab;
        [SerializeField] private WireManager wireManager;
        [SerializeField] private CraftManager craftManager;
        
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("GameplayScope.Configure called");
            
            builder.RegisterInstance(itemDatabase);
            builder.RegisterInstance(gameEventsDatabase);
            builder.RegisterInstance(enemyDatabase);
            builder.RegisterInstance(buildingDatabase);
            builder.RegisterInstance(turretDatabase);
            builder.RegisterInstance(resourceDatabase);
            builder.RegisterInstance(recipeDatabase);

            builder.Register<PlayerInventoryModel>(Lifetime.Singleton);
            
            builder.RegisterComponent(eventManagerPrefab);
            builder.RegisterComponent(globalStageManagerPrefab);
            builder.RegisterComponent(enemySpawnerPrefab);
            builder.RegisterComponent(buildingManagerPrefab);
            builder.RegisterComponent(wireManager);
            builder.RegisterComponent(craftManager);
            
            var damageSystem = new DamageSystem();
            builder.RegisterInstance(damageSystem);
            
            var physicalItemRegistry = new PhysicalItemRegistry();
            builder.RegisterInstance(physicalItemRegistry);
            
            var outlineRegistry = new OutlineRegistry();
            builder.RegisterInstance(outlineRegistry);
            
            var damagableRegistry = new DamagableRegistry();
            builder.RegisterInstance(damagableRegistry);

            var enemyTargetsRegistry = new TargetsRegistry();
            builder.RegisterInstance(enemyTargetsRegistry);
            
            var interactableRegistry = new InteractableRegistry();
            builder.RegisterInstance(interactableRegistry);

            var eventTerminalRegistry = new EventTerminalsRegistry();
            builder.RegisterInstance(eventTerminalRegistry);
            
            builder.Register<MainCharacterModel>(Lifetime.Transient);
            builder.Register<VaultDoorDamagableModel>(Lifetime.Transient);
            builder.Register<DamagableModel>(Lifetime.Transient);
            builder.Register<MCLocalModel>(Lifetime.Singleton);
            builder.Register<LocalBuildingHandlerModel>(Lifetime.Singleton);
            builder.Register<LocalWireHandlerModel>(Lifetime.Singleton);
            builder.Register<LocalCraftModel>(Lifetime.Singleton);
            
            builder.Register<GameplayUIRootViewModel>(Lifetime.Singleton);
            builder.Register<GameplayUIManager>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<GameplayEntryPoint>(Lifetime.Singleton);
        }
        
        private void Awake()
        {
            base.Awake();
            Resolver = Container;
        }

        protected override void OnDestroy()
        {
            if (Resolver == Container)
                Resolver = null;
            
            base.OnDestroy();
        }
    }
}