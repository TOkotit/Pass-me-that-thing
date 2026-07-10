using System;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenBuildBinder : WindowBinder<ScreenBuildViewModel>
    {
        [SerializeField] private SelectionWheel selectionWheel;

        private void Start()
        {
            ViewModel.RequestSetSprites(SetSprites);

            selectionWheel.OnValueChanged += OnSelectionWheelChanged;
        }

        private void OnDestroy()
        {
            selectionWheel.OnValueChanged -= OnSelectionWheelChanged;
        }

        public void SetSprites(List<Sprite> sprites)
        {
            selectionWheel.SetImageSprites(sprites);
        }

        public void OnSelectionWheelChanged(int index, int lastIndex)
        {
            
        }
    }
}