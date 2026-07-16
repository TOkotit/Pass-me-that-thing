using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Game.Scripts.GameFiles.Entity.Buildings;
using Game.Scripts.GameFiles.Entity.Buildings.Misc;
using Game.Scripts.GameFiles.Entity.Buildings.Misc.Craft;
using Game.UI;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenCraftMenuViewModel : WindowViewModel
    {
        private GameplayUIManager _uiManager;

        private GameInputManager _gameInput;
        private ResourceDatabase _resourceDatabase;
        private WorkbenchItemRecipeDatabase _recipeDatabase;
        private LocalCraftModel _localCraftModel;
        
        public override string Id => "ScreenCraftMenu";


        public ScreenCraftMenuViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            _gameInput = container.Resolve<GameInputManager>();

            _localCraftModel = container.Resolve<LocalCraftModel>();
            
            _resourceDatabase = container.Resolve<ResourceDatabase>();
            _recipeDatabase = container.Resolve<WorkbenchItemRecipeDatabase>();


            _gameInput.GameInput.UI.PauseMenu.performed += OnPauseClicked;
        }

        public override void Dispose()
        {
            _gameInput.GameInput.UI.PauseMenu.performed -= OnPauseClicked;
        }

        public void OnPauseClicked(InputAction.CallbackContext c) => RequestGoToGameplay();

        public void RequestGoToGameplay()
        {
            _localCraftModel.Clear();
            _uiManager.OpenScreenGameplay();
        }

        public void RequestUpdateRecipes(Action<List<WorkbenchItemRecipe> , ResourceDatabase> f)
        {
            f(_recipeDatabase.AllRecipes, _resourceDatabase);
        }

        public void RequestCraft(string recipeId)
        {
            _localCraftModel.Craft(recipeId);
        }
        
    }
}