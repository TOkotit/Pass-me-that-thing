using DG.Tweening;
using Game.UI;
using System.Collections;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.UIWorld
{
    public abstract class WorldWindowBinder<T> : MonoBehaviour, IWorldWindowBinder
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

        public void UpdatePosition(Vector3 pos)
        {
            transform.position = pos + ViewModel.windowOffset;
        }

        public void ChangeRotation(Vector3 lookPos)
        {
            if (ViewModel.enabled.Value)
            {
                var dir = (transform.position - lookPos).normalized;
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }
}