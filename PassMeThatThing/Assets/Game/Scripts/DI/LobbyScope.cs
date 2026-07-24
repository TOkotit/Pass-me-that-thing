using DI;
using Game;
using Game.MainMenu.View.UI;
using Game.Scripts.GameFiles.Lobby.Root;
using Mirror;
using R3;
using Root;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DI
{
    public class LobbyScope : LifetimeScope
    {
        [SerializeField] private NetworkIdentity networkIdentity;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(networkIdentity);

            builder.Register<RoomViewHandler>(Lifetime.Singleton);
            
            builder.Register<LobbyUIRootViewModel>(Lifetime.Singleton);
            builder.Register<LobbyUIManager>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<LobbyEntryPoint>();
        }
    }
}