using Game.Scripts.GameFiles.GlobalStageManager;
using Game.UI;
using Mirror;
using VContainer;

namespace Game.Gameplay.View.UI.ScreenDefeat
{
    public class ScreenDefeatViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        private NetworkRoomManager  _networkRoomManager;
        private GlobalStageManager  _globalStageManager;
        public override string Id => "ScreenDefeat";
        
        public ScreenDefeatViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            if (container.Resolve<NetworkManager>() is NetworkRoomManager roomManager)
            {
                _networkRoomManager = roomManager;
            }
            _globalStageManager = container.Resolve<GlobalStageManager>();
        }

        public void RequestGoOffline()
        {
            //TODO добавить кнопку готовности для выхода назад в лобби
            if (_globalStageManager.isServer)
            {
                _networkRoomManager.ServerChangeScene(_networkRoomManager.RoomScene);
            }
            else if (_globalStageManager.isClient)
            {
                _networkRoomManager.StopClient();
            }
        }
    }
}