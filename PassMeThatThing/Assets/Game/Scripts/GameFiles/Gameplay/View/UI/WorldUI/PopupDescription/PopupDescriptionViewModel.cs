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
        private readonly GameplayUIManager _gameplayUIManager;
        private readonly MCLocalModel _mcLocalModel;

        public ReactiveProperty<bool> enabled = new();
        public ReactiveProperty<Vector3>  screenPos = new();

        public override string Id => "PopupDescription";

        public PopupDescriptionViewModel(GameplayUIManager gameplayUIManager, IObjectResolver container)
        {
            _gameplayUIManager = gameplayUIManager;
            _mcLocalModel = container.Resolve<MCLocalModel>();
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