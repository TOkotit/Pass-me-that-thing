using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Game.Scripts.Enums;
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
        
        private WorkbenchItemRecipeDatabase _recipeDatabase;
        private LocalCraftModel _localCraftModel;
        


        public CraftManager craftManager;
        public ResourceDatabase resourceDatabase;

        public override string Id => "ScreenCraftMenu";


        public ScreenCraftMenuViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            _gameInput = container.Resolve<GameInputManager>();

            _localCraftModel = container.Resolve<LocalCraftModel>();
            
            _recipeDatabase = container.Resolve<WorkbenchItemRecipeDatabase>();


            craftManager = container.Resolve<CraftManager>();
            resourceDatabase = container.Resolve<ResourceDatabase>();

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

        public void RequestUpdateRecipes(Action<List<WorkbenchItemRecipe>> f)
        {
            f(_recipeDatabase.AllRecipes);
        }

        public void RequestCraft(string recipeId)
        {
            _localCraftModel.Craft(recipeId);
        }

        public void RequestSubForAvailableResources(Action f)
        {
            f();

            MainResourceStorage.Instance.OnResourcesChanged += f;
        }

        public void RequestUnsubForAvailableResources(Action f)
        {
            MainResourceStorage.Instance.OnResourcesChanged -= f;
        }
    }
}