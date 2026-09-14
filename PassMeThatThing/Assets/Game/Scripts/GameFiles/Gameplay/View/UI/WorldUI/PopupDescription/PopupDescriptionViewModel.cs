using Assets.Game.Scripts.Enums;
using Game.Entity;
using Game.Gameplay.View.UI;
using Game.Scripts.Enums;
using Game.UI;
using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.PopupDescription
{
    public class PopupDescriptionViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _gameplayUIManager;
        private readonly MCLocalModel _mcLocalModel;

        public ResourceDatabase resourceDatabase;

        public ReactiveProperty<bool> enabled = new();
        public ReactiveProperty<Vector3>  screenPos = new();

        public override string Id => "PopupDescription";

        public PopupDescriptionViewModel(GameplayUIManager gameplayUIManager, IObjectResolver container)
        {
            _gameplayUIManager = gameplayUIManager;
            _mcLocalModel = container.Resolve<MCLocalModel>();

            resourceDatabase = container.Resolve<ResourceDatabase>();
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

        public void RequestSubItemRecycleResource(Action<Dictionary<Resource, float>> f)
        {
            _mcLocalModel.OnItemRecycleResourcesChanged += f;
        }
        public void RequestUnSubItemRecycleResource(Action<Dictionary<Resource, float>> f)
        {
            _mcLocalModel.OnItemRecycleResourcesChanged -= f;
        }

        public void RequestSubDescriptionMode(Action<PopupDescriptionMode> f)
        {
            _mcLocalModel.OnPopupDescriptionModeChanged += f;
        }
        public void RequestUnSubDescriptionMode(Action<PopupDescriptionMode> f)
        {
            _mcLocalModel.OnPopupDescriptionModeChanged -= f;
        }
    }
}