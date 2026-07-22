using System;
using Game.UI;
using UnityEngine;
using UnityEngine.UIElements;


namespace Game.Gameplay.View.UI.ScreenDefeat
{
    public class ScreenDefeatBinder : WindowBinder<ScreenDefeatViewModel>
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;
        private Button _leaveBtn;

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _leaveBtn = _root.Q<Button>("LeaveBtn");
        }

        private void Start()
        {
            _leaveBtn.RegisterCallback<ClickEvent>(Leave);
        }

        private void OnDestroy()
        {
            _leaveBtn.UnregisterCallback<ClickEvent>(Leave);
        }

        public void Leave(ClickEvent e)
        {
            ViewModel.RequestGoOffline();
        }
    }
}