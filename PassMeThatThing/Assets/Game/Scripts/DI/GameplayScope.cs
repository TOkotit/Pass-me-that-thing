using Entity;
using Game.Entity;
using Game.Gameplay.Root;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using Game.Gameplay.View.UI;
using Game.Scripts.GameFiles.Entity;
using Game.Scripts.GameFiles.Entity.Buildings;
using Game.Scripts.GameFiles.Entity.Buildings.Misc;
using Game.Scripts.GameFiles.Entity.Buildings.Misc.Craft;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Game.Scripts.GameFiles.Entity.Enemy;
using Game.Scripts.GameFiles.GlobalStageManager;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.Highlight;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using Game.Scripts.GameFiles.LevelGeneration.ItemSpawn;
using Game.Scripts.Systems;
using UnityEngine.Serialization;
using Game.Scripts.GameFiles.GameRandomEvents;

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
        [SerializeField] private ItemRarityDatabase rarityDatabase;
        
        [SerializeField] private ResourceDatabase resourceDatabase;
        [SerializeField] private WorkbenchItemRecipeDatabase recipeDatabase;
        
        
        [Header("Managers on gameplay scene")]
        [SerializeField] private GameRandomEventManager eventManager;
        [SerializeField] private GlobalStageManager globalStageManager;
        [SerializeField] private ItemPoolManager itemPoolManager;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private WireManager wireManager;
        [SerializeField] private CraftManager craftManager;
        [SerializeField] private GlobalInventoryManager globalInventoryManager;
        [SerializeField] private ParticlePoolManager particlePoolManager;
        
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("GameplayScope.Configure called");
            
            //databases
            builder.RegisterInstance(itemDatabase);
            builder.RegisterInstance(gameEventsDatabase);
            builder.RegisterInstance(enemyDatabase);
            builder.RegisterInstance(buildingDatabase);
            builder.RegisterInstance(turretDatabase);
            builder.RegisterInstance(resourceDatabase);
            builder.RegisterInstance(recipeDatabase);
            builder.RegisterInstance(rarityDatabase);
            //managers
            builder.RegisterComponent(eventManager);
            builder.RegisterComponent(globalStageManager);
            builder.RegisterComponent(itemPoolManager);
            builder.RegisterComponent(enemySpawner);
            builder.RegisterComponent(buildingManager);
            builder.RegisterComponent(wireManager);
            builder.RegisterComponent(craftManager);
            builder.RegisterComponent(globalInventoryManager);
            builder.RegisterComponent(particlePoolManager);
            
            builder.Register<DamageSystem>(Lifetime.Singleton);
            
            //registries with static instance
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
            
            builder.Register<PhysicsApplyer>(Lifetime.Singleton);
            builder.Register<PlayerInventoryModel>(Lifetime.Singleton);
            builder.Register<MCLocalModel>(Lifetime.Singleton);
            builder.Register<LocalBuildingHandlerModel>(Lifetime.Singleton);
            builder.Register<LocalWireHandlerModel>(Lifetime.Singleton);
            builder.Register<LocalCraftModel>(Lifetime.Singleton);
            
            builder.Register<PlayerReadyManager>(Lifetime.Singleton);
            
            builder.Register<GameoverHandler>(Lifetime.Singleton);
            builder.Register<LevelGraphBuilder>(Lifetime.Singleton);
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