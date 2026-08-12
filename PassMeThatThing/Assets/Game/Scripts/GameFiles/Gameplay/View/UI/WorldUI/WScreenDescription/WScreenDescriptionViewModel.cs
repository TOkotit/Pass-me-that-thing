using Game.Gameplay.View.UI;
using Game.UI;
using System.Collections;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.WScreenDescription
{
    public class WScreenDescriptionViewModel : WindowViewModel
    {
        
        private GameplayUIManager _gameplayUIManager;
        public override string Id => "WScreenDescription";


        public WScreenDescriptionViewModel(GameplayUIManager gameplayUIManager, IObjectResolver container)
        {
            _gameplayUIManager = gameplayUIManager;

        }

        

        
    }
}