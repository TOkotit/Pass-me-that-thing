using Game.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.WScreenDescription
{
    public class WScreenDescriptionBinder : WindowBinder<WScreenDescriptionViewModel>
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
        }
    }
}