using Game.UI;
using System.Collections;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.UIWorld
{
    public abstract class WorldWindowBinder<T> : MonoBehaviour
        where T : WorldWindowViewModel
    {
        protected T ViewModel;

        public void Bind(WorldWindowViewModel viewModel)
        {
            ViewModel = (T)viewModel;

            OnBind(ViewModel);
        }

        public virtual void Close()
        {
            Destroy(gameObject);
        }

        protected virtual void OnBind(T viewModel) { }
    }
}