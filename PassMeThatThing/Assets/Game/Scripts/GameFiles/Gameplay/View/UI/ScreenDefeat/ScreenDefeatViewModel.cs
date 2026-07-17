using Game.UI;
using Mirror;
using VContainer;

namespace Game.Gameplay.View.UI.ScreenDefeat
{
    public class ScreenDefeatViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        private NetworkManager  _networkRoomManager;
        
        public override string Id => "ScreenDefeat";
        
        public ScreenDefeatViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            _networkRoomManager = container.Resolve<NetworkManager>();
        }

        public void RequestGoOffline()
        {
            _networkRoomManager.OnStopClient();
        }
    }
}