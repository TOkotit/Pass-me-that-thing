using Game.Scripts.GameFiles.GlobalStageManager;
using Game.UI;
using Mirror;
using VContainer;

namespace Game.Gameplay.View.UI.ScreenDefeat
{
    public class ScreenDefeatViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        private NetworkManager  _networkRoomManager;
        private GlobalStageManager  _globalStageManager;
        public override string Id => "ScreenDefeat";
        
        public ScreenDefeatViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            _networkRoomManager = container.Resolve<NetworkManager>();
            _globalStageManager = container.Resolve<GlobalStageManager>();
        }

        public void RequestGoOffline()
        {
            if (_globalStageManager.isClient && _globalStageManager.isServer)
            {
                _networkRoomManager.StopHost();
            }
            else if (_globalStageManager.isClient)
            {
                _networkRoomManager.StopClient();
            }
            
        }
    }
}