using Game.UI;
using ObservableCollections;
using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.UIWorld
{
    public class WorldUIRootViewModel : IDisposable
    {
        public IObservableCollection<WorldWindowViewModel> OpenedWorldWindows => _openedWorldWindows;

        private readonly ObservableList<WorldWindowViewModel> _openedWorldWindows = new();

        private readonly Dictionary<WorldWindowViewModel, IDisposable> _worldWindowsSubs = new();

        public void Dispose()
        {
           CloseAllWorldWindows();
        }

        public void OpenWorldWindow(WorldWindowViewModel worldWindowViewModel)
        {
            if (_openedWorldWindows.Contains(worldWindowViewModel))
            {
                Debug.Log("[WUI] _openedWorldWindows.Contains");
                return;
            }

            var subscription = worldWindowViewModel.CloseRequested.Subscribe(CloseWorldWindow);
            _worldWindowsSubs.Add(worldWindowViewModel, subscription);
            _openedWorldWindows.Add(worldWindowViewModel);
        }

        public void CloseWorldWindow(WorldWindowViewModel worldWindowViewModel)
        {
            if (_openedWorldWindows.Contains(worldWindowViewModel))
            {
                worldWindowViewModel.Dispose();
                _openedWorldWindows.Remove(worldWindowViewModel);

                var subscription = _worldWindowsSubs[worldWindowViewModel];
                subscription?.Dispose();
                _worldWindowsSubs.Remove(worldWindowViewModel);
            }
        }

        public void CloseAllWorldWindows()
        {
            foreach (var openedWindow in _openedWorldWindows)
            {
                _worldWindowsSubs[openedWindow]?.Dispose();
                _worldWindowsSubs.Remove(openedWindow);

                openedWindow.Dispose();
            }
            _openedWorldWindows.Clear();
        }
        
    }
}