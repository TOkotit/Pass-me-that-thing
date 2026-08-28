using Game.UI;
using System.Collections;
using UnityEngine;
using R3;

namespace Assets.Game.Scripts.GameFiles.UIWorld
{
    public class WorldWindowViewModel : WindowViewModel
    {
        private readonly Subject<WorldWindowViewModel> _closeRequested = new();

        public ReactiveProperty<bool> enabled = new();
        public Transform parent;
        public virtual Vector3 windowOffset => Vector3.up * 1.5f;

        public override string Id { get; }
        public new Observable<WorldWindowViewModel> CloseRequested => _closeRequested;


    }
}