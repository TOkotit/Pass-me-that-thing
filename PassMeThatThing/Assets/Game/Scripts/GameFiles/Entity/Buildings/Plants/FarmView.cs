using Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.FarmView;
using Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.WindowDescription;
using Game.Gameplay.View.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants
{
    public class FarmView : MonoBehaviour
    {
        [SerializeField] private Transform barC;

        [Inject] private GameplayUIManager _gameplayUIManager;
        private WindowFarmViewModel _windowViewModel;

        public Transform BarC => barC;

        public void InitUI(Farm farm)
        {
            _windowViewModel = _gameplayUIManager.OpenWindowFarmView(farm);
        }

        private void OnDestroy()
        {
            _gameplayUIManager.CloseWindow(_windowViewModel);
        }
    }
}
