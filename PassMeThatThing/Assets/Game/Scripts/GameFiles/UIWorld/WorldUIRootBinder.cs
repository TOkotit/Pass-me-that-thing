using Assets.Game.Scripts.GameFiles.UIWorld;
using Game.UI;
using ObservableCollections;
using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.UIWorld
{
    public class WorldUIRootBinder : MonoBehaviour
    {
        [SerializeField] private WorldUIRootContainer windowsContainer;

        private readonly CompositeDisposable _subscriptions = new();

        public void Bind(WorldUIRootViewModel viewModel)
        {
            //Debug.Log("[WUI] Bind");

            foreach (var openedPopup in viewModel.OpenedWorldWindows)
            {
                windowsContainer.OpenWorldWindow(openedPopup);
            }

            _subscriptions.Add(viewModel.OpenedWorldWindows.ObserveAdd().Subscribe(e =>
            {
                windowsContainer.OpenWorldWindow(e.Value);
            }));

            _subscriptions.Add(viewModel.OpenedWorldWindows.ObserveRemove().Subscribe(e =>
            {
                windowsContainer.CloseWorldWindow(e.Value);
            }));

            OnBind(viewModel);
        }

        protected virtual void OnBind(WorldUIRootViewModel viewModel) { }

        private void OnDestroy()
        {
            _subscriptions.Dispose();
        }
    }
}