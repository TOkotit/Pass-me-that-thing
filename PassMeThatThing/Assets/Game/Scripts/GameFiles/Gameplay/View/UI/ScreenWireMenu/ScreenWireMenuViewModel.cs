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
    public class ScreenWireMenuViewModel : WindowViewModel
    {
        private GameplayUIManager _uiManager;
        
        private BuildingsDatabase _buildingsDatabase;
        private LocalBuildingHandlerModel _handlerModel;
        private GameInputManager _gameInput;

        public event Action<Action> BeforeExitScreen;
        
        public override string Id => "ScreenWireMenu";


        public ScreenWireMenuViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            _buildingsDatabase =  container.Resolve<BuildingsDatabase>();
            _handlerModel = container.Resolve<LocalBuildingHandlerModel>();
            _gameInput = container.Resolve<GameInputManager>();
            
            
            _gameInput.GameInput.Gameplay.WireMenu.performed += OnPerformedWireMenu;
        }

        public override void Dispose()
        {
            _gameInput.GameInput.Gameplay.WireMenu.performed -= OnPerformedWireMenu;
        }

        public void RequestGoToGameplay()
        {
            BeforeExitScreen?.Invoke(() =>
            {
                _handlerModel.CancelBuildPreview();
                _uiManager.OpenScreenGameplay();
            });
        }
        
        public void RequestGoToBuildPreview()
        {
            BeforeExitScreen?.Invoke(() =>
            {
                _uiManager.OpenScreenBuild();
            });
        }
        
        private void OnPerformedWireMenu(InputAction.CallbackContext c) => RequestGoToGameplay();
        

        public void RequestSetSprites(Action<List<Sprite>> c)
        {
            c(_buildingsDatabase.miniBuildings.Select(b => b.buildingImage).ToList());
        }

        public void RequestBuildingChosen(int buildingIndex)
        {
            _handlerModel.StartBuildPreview(_buildingsDatabase.miniBuildings[buildingIndex].id);
            RequestGoToBuildPreview();
        }

        public void HideUi(Action onComplete)
        {
            
        }
        
    }
}