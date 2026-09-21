using Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants;
using Assets.Game.Scripts.GameFiles.UIWorld;
using Game.Entity;
using Game.Gameplay.View.UI;
using System;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.FarmView
{
    public class WindowFarmViewModel : WorldWindowViewModel
    {
        private GameplayUIManager _gameplayUIManager;
        private MCLocalModel _mcLocalModel;
        private Farm _farm;

        public override string Id => "WindowFarmView";

        public WindowFarmViewModel(GameplayUIManager gameplayUIManager,
            IObjectResolver container,
            Farm farm)
        {
            _gameplayUIManager = gameplayUIManager;
            _mcLocalModel = container.Resolve<MCLocalModel>();
            _farm = farm;
            parent = farm.FarmView.BarC;
        }

        public void RequestSubCameraPos(Action<Vector3> f)
        {
            _mcLocalModel.OnCameraPositionChanged += f;
        }
        public void RequestUnSubCameraPos(Action<Vector3> f)
        {
            _mcLocalModel.OnCameraPositionChanged -= f;
        }

        public void RequestSubGrowPercent(Action<float> f)
        {
            _farm.OnGrowTimeElapsedPercentChanged += f;
        }

        public void RequestUnSubGrowPercent(Action<float> f)
        {
            _farm.OnGrowTimeElapsedPercentChanged -= f;
        }
    }
}
