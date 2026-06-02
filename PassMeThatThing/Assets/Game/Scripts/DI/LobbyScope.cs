using DI;
using Game;
using Game.MainMenu.View.UI;
using Game.Scripts.GameFiles.Lobby.Root;
using R3;
using VContainer;
using VContainer.Unity;

namespace DI
{
    public class LobbyScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<LobbyUIRootViewModel>(Lifetime.Singleton);
            builder.Register<LobbyUIManager>(Lifetime.Singleton);
            
            
            
            builder.RegisterEntryPoint<LobbyEntryPoint>();
        }
    }
}