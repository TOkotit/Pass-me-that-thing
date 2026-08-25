using Game.Entity;
using Game.Gameplay.View.UI;
using Game.UI;
using R3;
using System;
using System.Collections;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.PopupDescription
{
    public class PopupDescriptionViewModel : WindowViewModel
    {
        private GameplayUIManager _gameplayUIManager;
        private readonly MCLocalModel _mcLocalModel;

        public ReactiveProperty<bool> enabled = new();
        public ReactiveProperty<Vector3>  screenPos = new();

        public override string Id => "PopupDescription";

        public PopupDescriptionViewModel(GameplayUIManager gameplayUIManager, IObjectResolver container)
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