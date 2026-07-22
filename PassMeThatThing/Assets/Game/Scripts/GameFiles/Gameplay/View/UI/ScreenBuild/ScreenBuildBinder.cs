using System;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenBuildBinder : WindowBinder<ScreenBuildViewModel>
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;
        private Label _confirmBindLb;
        private Label _cancelBindLb;

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            
            _cancelBindLb = _root.Q<Label>("CancelBindLb");
            _confirmBindLb = _root.Q<Label>("ConfirmBindLb");
        }

        private void Start()
        {
            var binds = ViewModel.RequestLoadBuildBindings();
            
            _confirmBindLb.text = binds.Item1;
            _cancelBindLb.text = binds.Item2;
        }
        
    }
}