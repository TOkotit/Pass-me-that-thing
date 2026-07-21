using System;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.MainMenu.View.UI.ScreenMainMenu
{
    public class ScreenMainMenuBinder : WindowBinder<ScreenMainMenuViewModel>
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;
        private Button _hostBtn;
        private Button _joinBtn;
        private Button _optionsBtn;
        private Button _exitBtn;
        private TextField _ipInput;
        
        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _hostBtn = _root.Q("HostBtn") as Button;
            _joinBtn = _root.Q("JoinBtn") as Button;
            _optionsBtn = _root.Q("OptionsBtn") as Button;
            _exitBtn = _root.Q("ExitBtn") as Button;
            _ipInput = _root.Q("IpInput") as TextField;
        }

        private void Start()
        {
            _hostBtn.RegisterCallback<ClickEvent>(OnHostButtonClicked);
            _joinBtn.RegisterCallback<ClickEvent>(OnJoinButtonClicked);
            _optionsBtn.RegisterCallback<ClickEvent>(OnOptionsButtonClicked);
            _exitBtn.RegisterCallback<ClickEvent>(OnExitButtonClicked);
            
            _ipInput.RegisterCallback<ChangeEvent<string>>(OnIpTextChanged);
        }
        
        private void OnDestroy()
        {
            _hostBtn.UnregisterCallback<ClickEvent>(OnHostButtonClicked);
            _joinBtn.UnregisterCallback<ClickEvent>(OnJoinButtonClicked);
            _optionsBtn.UnregisterCallback<ClickEvent>(OnOptionsButtonClicked);
            _exitBtn.UnregisterCallback<ClickEvent>(OnExitButtonClicked);
            
            _ipInput.UnregisterCallback<ChangeEvent<string>>(OnIpTextChanged);
        }

        private void OnHostButtonClicked(ClickEvent e)
        {
            ViewModel.RequestHost();
        }
        
        private void OnJoinButtonClicked(ClickEvent e)
        {
            ViewModel.RequestJoin();
        }

        private void OnOptionsButtonClicked(ClickEvent e)
        {
            ViewModel.RequestGoToScreenOptions();
        }

        
        private void OnExitButtonClicked(ClickEvent e)
        {
            Debug.Log("OnExitButtonClicked");
        }

        private void OnIpTextChanged(ChangeEvent<string> e)
        {
            ViewModel.RequestIpAddress(e.newValue);
        }
        
    }
}