using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.UI;
using UnityEngine;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenWireMenuBinder : WindowBinder<ScreenWireMenuViewModel>
    {
        [SerializeField] private SelectionWheel selectionWheel;
        
        private void Start()
        {
            ViewModel.RequestSetSprites(SetSprites);

            selectionWheel.OnValueChanged += OnSelectionWheelChanged;
            ViewModel.BeforeExitScreen += HideSelectionWheel;
            
            selectionWheel.transform.DOLocalMoveY(0f, 0.3f).From(selectionWheel.RectTransform.rect.height/5).SetEase(Ease.OutQuad);
        }

        private void HideSelectionWheel(Action onComplete)
        {
            selectionWheel.transform.DOLocalMoveY(selectionWheel.RectTransform.rect.height/5, 0.3f)
                .From(0f).SetEase(Ease.OutQuad).OnComplete(() => onComplete());
        }

        private void OnDestroy()
        {
            selectionWheel.OnValueChanged -= OnSelectionWheelChanged;
            ViewModel.BeforeExitScreen -= HideSelectionWheel;
        }

        public void SetSprites(List<Sprite> sprites)
        {
            selectionWheel.SetImageSprites(sprites);
        }

        public void OnSelectionWheelChanged(int index, int lastIndex)
        {
            ViewModel.RequestBuildingChosen(index);
        }
        
        
        
    }
}