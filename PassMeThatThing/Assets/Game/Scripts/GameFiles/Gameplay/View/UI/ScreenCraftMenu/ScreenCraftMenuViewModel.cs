using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Game.Scripts.GameFiles.Entity.Buildings;
using Game.Scripts.GameFiles.Entity.Buildings.Misc;
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

        
        public override string Id => "ScreenCraftMenu";


        public ScreenCraftMenuViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            _gameInput = container.Resolve<GameInputManager>();
            
            _resourceDatabase = container.Resolve<ResourceDatabase>();
            _recipeDatabase = container.Resolve<WorkbenchItemRecipeDatabase>();
        }

        public override void Dispose()
        {

        }

        public void RequestUpdateRecipes(Action<List<WorkbenchItemRecipe> , ResourceDatabase> f)
        {
            
        }
        
    }
}