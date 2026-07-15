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
    public class ScreenCraftMenuViewModel : WindowViewModel
    {
        private GameplayUIManager _uiManager;

        private GameInputManager _gameInput;

        
        public override string Id => "ScreenCraftMenu";


        public ScreenCraftMenuViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            
        }

        public override void Dispose()
        {

        }


        
    }
}