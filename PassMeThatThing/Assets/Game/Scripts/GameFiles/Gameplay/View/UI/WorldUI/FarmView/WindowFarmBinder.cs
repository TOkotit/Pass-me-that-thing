using Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.WindowDescription;
using Assets.Game.Scripts.GameFiles.UIWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.FarmView
{
    public class WindowFarmBinder : WorldWindowBinder<WindowFarmViewModel>
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;
        private ProgressBar _growProgress;


        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _growProgress = _root.Q<ProgressBar>("GrowProgress");
        }

        private void Start()
        {
            ViewModel.enabled.Value = true;

            gameObject.transform.SetParent(ViewModel.parent);
            gameObject.transform.localPosition = ViewModel.windowOffset;

            ViewModel.RequestSubCameraPos(ChangeRotation);
            ViewModel.RequestSubGrowPercent(UpdateGrowBar);
        }

        private void OnDestroy()
        {
            ViewModel.RequestUnSubCameraPos(ChangeRotation);
            ViewModel.RequestUnSubGrowPercent(UpdateGrowBar);
        }

        private void UpdateGrowBar(float value)
        {
            _growProgress.value = Mathf.Clamp(value*100,
                _growProgress.lowValue, _growProgress.highValue);
        }
    }
}
