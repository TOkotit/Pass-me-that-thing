using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace Game.MainMenu.View.UI.ScreenMainMenu
{
    public class ScreenMainMenuBinder : WindowBinder<ScreenMainMenuViewModel>
    {
        [SerializeField] private Button _btnHost;
        [SerializeField] private Button _btnJoin;
        [SerializeField] private Button _btnOptions;
        [SerializeField] private Button _btnExit;
        
        [SerializeField] private TMP_InputField _ipText;


        private void Start()
        {
            _btnHost?.onClick.AddListener(OnHostButtonClicked);
            _btnJoin?.onClick.AddListener(OnJoinButtonClicked);
            _btnOptions?.onClick.AddListener(OnOptionsButtonClicked);
            _btnExit?.onClick.AddListener(OnExitButtonClicked);
            
            _ipText?.onValueChanged.AddListener(OnIpTextChanged);
        }
        
        private void OnDestroy()
        {
            _btnHost?.onClick.RemoveListener(OnHostButtonClicked);
            _btnJoin?.onClick.RemoveListener(OnJoinButtonClicked);
            _btnOptions?.onClick.RemoveListener(OnOptionsButtonClicked);
            _btnExit?.onClick.RemoveListener(OnExitButtonClicked);
            
            _ipText?.onValueChanged.RemoveListener(OnIpTextChanged);
        }

        private void OnHostButtonClicked()
        {
            ViewModel.RequestHost();
        }
        
        private void OnJoinButtonClicked()
        {
            ViewModel.RequestJoin();
        }

        private void OnOptionsButtonClicked()
        {
            
        }

        
        private void OnExitButtonClicked()
        {
            
        }

        private void OnIpTextChanged(string value)
        {
            ViewModel.RequestIpAddress(value);
        }
        
    }
}