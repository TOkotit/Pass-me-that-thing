using Game.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LightTransport;

namespace Assets.Game.Scripts.GameFiles.UIWorld
{
    public class WorldUIRootContainer : MonoBehaviour
    {
        [SerializeField] private Transform defaultWorldWindowsParent;

        private readonly Dictionary<WorldWindowViewModel, IWorldWindowBinder> 
            _openedWorldWindowBinders = new();

        public void OpenWorldWindow(WorldWindowViewModel viewModel)
        {
            var prefabPath = GetPrefabPath(viewModel);
            var prefab = Resources.Load<GameObject>(prefabPath);

            var createdWindow = Instantiate(prefab, defaultWorldWindowsParent);

            var binder = createdWindow.GetComponent<IWorldWindowBinder>();
            binder.Bind(viewModel);
            
            _openedWorldWindowBinders.Add(viewModel, binder);
        }

        public void CloseWorldWindow(WorldWindowViewModel viewModel)
        {
            var binder = _openedWorldWindowBinders[viewModel];

            binder?.Close();
            _openedWorldWindowBinders.Remove(viewModel);
        }


        private static string GetPrefabPath(WorldWindowViewModel viewModel)
        {
            return $"Prefabs/UI/World/{viewModel.Id}";
            
        }

    }
}