using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Scripts.GameFiles.Gameplay.View.UI.ScreenBuild;
using Game.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenWireMenuBinder : WindowBinder<ScreenWireMenuViewModel>
    {
        [SerializeField] private UIDocument uiDocument;
        
        private VisualElement _root;
        private SelectionWheel _selectionWheel;

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            
            _selectionWheel =  _root.Q<SelectionWheel>("SelectionWheel");
        }

        private void Start()
        {
            ViewModel.RequestSetSprites(SetSprites);

            _selectionWheel.OnValueChanged += OnSelectionWheelChanged;
            ViewModel.BeforeExitScreen += BeforeExit;
        }

        private void BeforeExit(Action onComplete)
        {
            onComplete();
        }

        private void OnDestroy()
        {
            _selectionWheel.OnValueChanged -= OnSelectionWheelChanged;
            ViewModel.BeforeExitScreen -= BeforeExit;
        }

        public void SetSprites(List<Sprite> sprites)
        {
            _selectionWheel.SetImageSprites(sprites);
            
        }

        public void OnSelectionWheelChanged(int index, int lastIndex)
        {
            Debug.Log($"OnSelectionWheelChanged {index} / {lastIndex}");
            ViewModel.RequestBuildingChosen(index);
        }
        
        
        
    }
}