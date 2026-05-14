using Game;
using Game.Gameplay;
using Game.Gameplay.Root;
using VContainer;
using VContainer.Unity;
using Systems;
using UnityEngine;
using Game.Gameplay.View.UI;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.Highlight;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using MainCharacter;
using R3;
using Unity.VisualScripting;
using Utils;
using UIRoot;

namespace DI
{
    public class GameplayScope: LifetimeScope
    {
        [SerializeField] ItemDatabase itemDatabase;
        [SerializeField] private GameObject eventManagerPrefab;
        
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("GameplayScope.Configure called");
            
            builder.RegisterInstance(itemDatabase);

            builder.Register<PlayerInventoryModel>(Lifetime.Singleton);
            
            var gameEventManagerGo = Instantiate(eventManagerPrefab);
            DontDestroyOnLoad(gameEventManagerGo);
            
            var gameEventManagerComponent = gameEventManagerGo.GetComponent<GameEventManager>();
            if (!gameEventManagerComponent)
            {
                Debug.LogError("NetworkManager component not found on networkManager prefab.");
            }
            else
            {
                builder.RegisterComponent(gameEventManagerComponent);
            }
            
            builder.Register<GameplayUIRootViewModel>(Lifetime.Singleton);
            builder.Register<GameplayUIManager>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<GameplayEntryPoint>(Lifetime.Singleton);
            /*builder.Register<PhysicalItemRegistry>(Lifetime.Singleton);*/
            var physicalItemRegistry = new PhysicalItemRegistry();
            builder.RegisterInstance(physicalItemRegistry);
            
            var outlineRegistry = new OutlineRegistry();
            builder.RegisterInstance(outlineRegistry);
        }
    }
}