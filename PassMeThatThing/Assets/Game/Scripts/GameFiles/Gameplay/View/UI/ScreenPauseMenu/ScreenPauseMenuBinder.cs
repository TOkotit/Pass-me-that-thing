using System;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Gameplay.View.UI.ScreenPauseMenu
{
    public class ScreenPauseMenuBinder : WindowBinder<ScreenPauseMenuViewModel>
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;
        private Button _continueBtn;
        private Button _optionsBtn;
        private Button _exitBtn;

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _continueBtn = _root.Q<Button>("ContinueBtn");
            _optionsBtn = _root.Q<Button>("OptionsBtn");
            _exitBtn = _root.Q<Button>("ExitBtn");
        }

        private void Start()
        {
            _continueBtn.RegisterCallback<ClickEvent>(OnContinueButtonClicked);
            _optionsBtn.RegisterCallback<ClickEvent>(OnOptionsButtonClicked);
            _exitBtn.RegisterCallback<ClickEvent>(OnExitButtonClicked);
        }

        private void OnDestroy()
        {
            _continueBtn.UnregisterCallback<ClickEvent>(OnContinueButtonClicked);
            _optionsBtn.UnregisterCallback<ClickEvent>(OnOptionsButtonClicked);
            _exitBtn.UnregisterCallback<ClickEvent>(OnExitButtonClicked);
        }

        private void OnContinueButtonClicked(ClickEvent e)
        {
            ViewModel.RequestGoToScreenGameplay();
        }
        
        private void OnOptionsButtonClicked(ClickEvent e)
        {
            ViewModel.RequestGoToScreenOptions();
        }
        
        private void OnExitButtonClicked(ClickEvent e)
        {
            ViewModel.RequestGoToMainMenu();
        }
    }
}