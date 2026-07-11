using System;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenBuildBinder : WindowBinder<ScreenBuildViewModel>
    {
        [SerializeField] private SelectionWheel selectionWheel;
        [SerializeField] private GameObject buildPreviewContainer;

        private void Start()
        {
            ViewModel.RequestSetSprites(SetSprites);

            ViewModel.OnBuildingChooseWheelEnabled += UpdateState;

            selectionWheel.OnValueChanged += OnSelectionWheelChanged;
        }

        private void OnDestroy()
        {
            ViewModel.OnBuildingChooseWheelEnabled -= UpdateState;
            
            selectionWheel.OnValueChanged -= OnSelectionWheelChanged;
        }

        public void SetSprites(List<Sprite> sprites)
        {
            selectionWheel.SetImageSprites(sprites);
        }

        public void OnSelectionWheelChanged(int index, int lastIndex)
        {
            ViewModel.RequestStartPreviewBuilding(index);
        }

        public void UpdateState(bool isSelection)
        {
            selectionWheel.gameObject.SetActive(isSelection);
            buildPreviewContainer.gameObject.SetActive(!isSelection);
        }
        
    }
}