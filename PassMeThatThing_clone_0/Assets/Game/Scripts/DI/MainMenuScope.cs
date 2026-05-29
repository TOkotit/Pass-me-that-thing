using DI;
using Game;
using Game.MainMenu.View.UI;
using R3;
using VContainer;
using VContainer.Unity;

namespace DI
{
    public class MainMenuScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        { 
            builder.Register<MainMenuUIRootViewModel>(Lifetime.Singleton);
            builder.Register<MainMenuUIManager>(Lifetime.Singleton);
            
            
            
            builder.RegisterEntryPoint<MainMenuEntryPoint>();
        }
    }
}