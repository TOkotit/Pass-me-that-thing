using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.GlobalStageManager;
using Game.Scripts.GameFiles.Items;
using Game.UI;
using R3;
using VContainer;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    public class ScreenMinigameViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        
        private readonly PlayerInventoryModel  _playerInventoryModel;
        private readonly ItemDatabase _itemDatabase;
        private readonly GameRandomEventManager _gameRandomEventManager;
        private readonly GameEventsDatabase _gameEventsDatabase;
        private readonly GlobalStageManager _globalStageManager;
        
        private readonly CompositeDisposable _subscriptions = new();
        
        public override string Id => "ScreenMinigame";
        
        public ScreenMinigameViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            _playerInventoryModel = container.Resolve<PlayerInventoryModel>();
            _itemDatabase =  container.Resolve<ItemDatabase>();
            _gameEventsDatabase  = container.Resolve<GameEventsDatabase>();
            
            _gameRandomEventManager =  container.Resolve<GameRandomEventManager>();
            _globalStageManager = container.Resolve<GlobalStageManager>();
        }
        
        
    }
}