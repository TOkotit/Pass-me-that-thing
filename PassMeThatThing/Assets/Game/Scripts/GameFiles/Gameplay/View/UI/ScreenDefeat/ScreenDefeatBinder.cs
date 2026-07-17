using System;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI.ScreenDefeat
{
    public class ScreenDefeatBinder : WindowBinder<ScreenDefeatViewModel>
    {
        [SerializeField]  private Button leaveButton;

        private void Start()
        {
            leaveButton.onClick.AddListener(Leave);
        }

        private void OnDestroy()
        {
            leaveButton.onClick.RemoveListener(Leave);
        }

        public void Leave()
        {
            ViewModel.RequestGoOffline();
        }
    }
}