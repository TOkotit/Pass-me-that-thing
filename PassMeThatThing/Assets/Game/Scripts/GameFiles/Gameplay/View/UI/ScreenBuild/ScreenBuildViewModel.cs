using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Game.Scripts.GameFiles.Entity.Buildings;
using Game.UI;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenBuildViewModel : WindowViewModel
    {
        private GameplayUIManager _uiManager;
        
        private BuildingsDatabase _buildingsDatabase;
        private LocalBuildingHandlerModel _handlerModel;
        private GameInputManager _gameInput;

        
        public override string Id => "ScreenBuild";


        public ScreenBuildViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            _buildingsDatabase =  container.Resolve<BuildingsDatabase>();
            _handlerModel = container.Resolve<LocalBuildingHandlerModel>();
            _gameInput = container.Resolve<GameInputManager>();

            

            _gameInput.GameInput.Gameplay.ConfirmBuilding.performed += OnPerformedConfirmBuilding;
            _gameInput.GameInput.Gameplay.CancelBuilding.performed += OnPerformedCancelBuilding;
        }

        public override void Dispose()
        {

            _gameInput.GameInput.Gameplay.ConfirmBuilding.performed -= OnPerformedConfirmBuilding;
            _gameInput.GameInput.Gameplay.CancelBuilding.performed -= OnPerformedCancelBuilding;
        }
        
        private void OnPerformedBuildMenu(InputAction.CallbackContext c) => RequestGoToGameplay();
        private void OnPerformedConfirmBuilding(InputAction.CallbackContext c) => RequestConfirmBuilding();
        private void OnPerformedCancelBuilding(InputAction.CallbackContext c) => RequestCancelBuilding();

        public void RequestGoToGameplay()
        {
            _handlerModel.CancelBuildPreview();
        }

        public void RequestConfirmBuilding()
        {
            _handlerModel.ConfirmBuildPreview();
        }
        
        public void RequestCancelBuilding()
        {
            _handlerModel.CancelBuildPreview();
        }
    }
}