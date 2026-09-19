using Assets.Game.Scripts.GameFiles.GameRoot;
using Game.Scripts.GameFiles.GlobalStageManager;
using Game.UI;
using Mirror;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using VContainer;

namespace Game.Gameplay.View.UI.ScreenPauseMenu
{
    public class ScreenPauseMenuViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        private readonly GameInputManager _gameInputManager;
        private NetworkRoomManager _networkRoomManager;
        private GlobalStageManager _globalStageManager;

        public override string Id => "ScreenPauseMenu";
        
        public ScreenPauseMenuViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            _gameInputManager = container.Resolve<GameInputManager>();
            
            _gameInputManager.GameInput.UI.PauseMenu.performed += PauseMenuPerformed;


            if (container.Resolve<NetworkManagerContainer>().Instance is NetworkRoomManager roomManager)
            {
                _networkRoomManager = roomManager;
            }
            _globalStageManager = container.Resolve<GlobalStageManager>();
        }

        public override void Dispose()
        {
            // Debug.Log("Disposing ScreenPauseMenuViewModel");
            _gameInputManager.GameInput.UI.PauseMenu.performed -= PauseMenuPerformed;
        }
        

        public void PauseMenuPerformed(InputAction.CallbackContext c)
        {
            RequestGoToScreenGameplay();
        }

        public void RequestGoToScreenGameplay()
        {
            _uiManager.OpenScreenGameplay();
        }
        
        public void RequestGoToMainMenu()
        {
            //TODO добавить кнопку готовности для выхода назад в лобби
            if (_globalStageManager.isServer)
            {
                //_networkRoomManager.ServerChangeScene(_networkRoomManager.RoomScene);
                _networkRoomManager.StopHost();
            }
            else if (_globalStageManager.isClient)
            {
                _networkRoomManager.StopClient();
            }
        }
        
        public void RequestGoToScreenOptions()
        {
            _uiManager.OpenScreenOptions();
        }
    }
}