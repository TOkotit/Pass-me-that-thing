using Game.UI;
using System.Collections;
using UnityEngine;
using R3;

namespace Assets.Game.Scripts.GameFiles.UIWorld
{
    public class WorldWindowViewModel : WindowViewModel
    {
        private readonly Subject<WorldWindowViewModel> _closeRequested = new();

        public Transform parent;
        public Vector3 position;
        public Quaternion rotation;

        public override string Id { get; }
        public new Observable<WorldWindowViewModel> CloseRequested => _closeRequested;
    }
}