using Assets.Game.Scripts.GameFiles.UIWorld;
using Game.Entity;
using Game.Gameplay.View.UI;
using Game.UI;
using System;
using System.Collections;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.WindowDescription
{
    public class WindowDescriptionViewModel : WorldWindowViewModel
    {
        
        private GameplayUIManager _gameplayUIManager;
        private readonly MCLocalModel _mcLocalModel;

        public override string Id => "WindowDescription";

        public WindowDescriptionViewModel(GameplayUIManager gameplayUIManager, IObjectResolver container)
        {
            _gameplayUIManager = gameplayUIManager;
            _mcLocalModel = container.Resolve<MCLocalModel>();
        }

        public void RequestSubCameraPos(Action<Vector3> f)
        {
            _mcLocalModel.OnCameraPositionChanged += f;
        }
        public void RequestUnSubCameraPos(Action<Vector3> f)
        {
            _mcLocalModel.OnCameraPositionChanged -= f;
        }

        public void RequestSubDescriptionText(Action<string> f)
        {
            f(_mcLocalModel.CurrentInteractableText);

            _mcLocalModel.OnCurrentInteractableTextChanged += f;
        }
        public void RequestUnSubDescriptionText(Action<string> f)
        {
            _mcLocalModel.OnCurrentInteractableTextChanged -= f;
        }

    }
}