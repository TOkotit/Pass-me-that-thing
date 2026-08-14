using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Scripts.GameFiles.Gameplay.View.UI.ScreenBuild;
using Game.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenWireMenuBinder : WindowBinder<ScreenWireMenuViewModel>
    {
        public const string OutlineImageClassName = "socket-outline";
        public const string SelectedOutlineImageClassName = "socket-outline--selected";

        [SerializeField] private UIDocument uiDocument;
        
        private VisualElement _root;
        private SelectionWheel _selectionWheel;
        private List<VisualElement> _hoverImContainer;
        private VisualElement _currentHoverImage;

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            
            _selectionWheel =  _root.Q<SelectionWheel>("SelectionWheel");
            _hoverImContainer = _root.Q<VisualElement>("HoverImContainer").Children().ToList();
        }

        private void Start()
        {
            ViewModel.RequestSetSprites(SetSprites);

            _selectionWheel.OnValueChanged += OnSelectionWheelChanged;
            _selectionWheel.OnPreviewValueChanged += UpdateHoverOutlineImage;
            ViewModel.BeforeExitScreen += BeforeExit;
        }

        private void BeforeExit(Action onComplete)
        {
            onComplete();
        }

        private void OnDestroy()
        {
            _selectionWheel.OnValueChanged -= OnSelectionWheelChanged;
            _selectionWheel.OnPreviewValueChanged -= UpdateHoverOutlineImage;
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
        
        public void UpdateHoverOutlineImage(int index, int lastIndex)
        {
            if (_currentHoverImage != null)
            {
                _currentHoverImage.RemoveFromClassList(SelectedOutlineImageClassName);
            }

            _currentHoverImage = _hoverImContainer[index];
            _currentHoverImage.AddToClassList(SelectedOutlineImageClassName);
        }
        
    }
}