using Assets.Game.Scripts.Enums;
using DG.Tweening;
using Game.Scripts.Enums;
using Game.UI;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.PopupDescription
{
    public class PopupDescriptionBinder : PopupBinder<PopupDescriptionViewModel>
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private Vector2 popupOffset = new Vector2(25f, 25f);
        [SerializeField] private VisualTreeAsset resourcePrefab;


        private VisualElement _root;
        private VisualElement _container;
        
        private VisualElement _mainTextContainer;
        private Label _text;

        private VisualElement _recycleContainer;
        private VisualElement _recycleResourceContainer;

        private VisualElement _healthContainer;
        private Label _healthText;

        private VisualElement _wireContainer;
        private Label _wireText;
        private VisualElement _wireNetIcon;
        private VisualElement _wirePortInputIcon;
        private VisualElement _wirePortOutputIcon;

        private CompositeDisposable _subs = new();

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _text = _root.Q<Label>("TextLb");
            _container = _root.Q<VisualElement>("Container");

            _mainTextContainer = _root.Q<VisualElement>("MainTextContainer");

            _recycleContainer = _root.Q<VisualElement>("RecycleContainer");
            _recycleResourceContainer = _root.Q<VisualElement>("ResourceContainer");

            _healthContainer = _root.Q<VisualElement>("HealthContainer");
            _healthText = _root.Q<Label>("HealthTextLb");

            _wireContainer = _root.Q<VisualElement>("WireContainer");
            _wireText = _root.Q<Label>("WireTextLb");
            _wireNetIcon = _root.Q<VisualElement>("WireNetIcon");
            _wirePortInputIcon = _root.Q<VisualElement>("WirePortInputIcon");
            _wirePortOutputIcon = _root.Q<VisualElement>("WirePortOutputIcon");
        }

        private void Start()
        {
            _subs.Add(ViewModel.enabled.Subscribe(SetVisibility));
            _subs.Add(ViewModel.screenPos.Subscribe(UpdatePosition));

            ViewModel.RequestSubDescriptionText(ChangeText);
            ViewModel.RequestSubItemRecycleResource(ChangeItemRecycleResource);
            ViewModel.RequestSubDescriptionMode(ChangeDescriptionMode);
        }

        private void OnDestroy()
        {
            _subs.Dispose();

            ViewModel.RequestUnSubDescriptionText(ChangeText);
            ViewModel.RequestUnSubItemRecycleResource(ChangeItemRecycleResource);
            ViewModel.RequestUnSubDescriptionMode(ChangeDescriptionMode);
        }

        public void UpdatePosition(Vector3 pos)
        {
            _container.style.left = pos.x + popupOffset.x;
            _container.style.top = _root.resolvedStyle.height - pos.y + popupOffset.y;
        }

        public void SetVisibility(bool v)
        {
            if (v)
            {
                _container.visible = v;
                _container.DOScale(1f, 0.2f).From(new Vector2(0f, 0f));
            }
            else
            {
                _container.DOScale(0f, 0.2f).From(new Vector2(1f, 1f)).OnComplete(() =>
                {
                    _container.visible = v;
                });
            }
        }

        public void ChangeText(string value)
        {
            _text.text = value;
            _healthText.text = value;
            _wireText.text = value;
        }

        public void ChangeItemRecycleResource(Dictionary<Resource, float> d)
        {
            _recycleResourceContainer.Clear();
            if (d.Count > 0)
            {
                foreach (var e in d)
                {
                    var rData = ViewModel.resourceDatabase.GetResource(e.Key);

                    var rRes = resourcePrefab.Instantiate();
                    _recycleResourceContainer.Add(rRes);

                    rRes.Q<VisualElement>("ResIm").style.backgroundImage = new StyleBackground(rData.resourceImage);
                    rRes.Q<Label>("ResLb").text = e.Value.ToString();
                }
            }
        }

        public void HideAllContainers()
        {
            var style = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            _mainTextContainer.style.display = style;
            _recycleContainer.style.display = style;
            _healthContainer.style.display = style;
            _wireContainer.style.display = style;
        }

        public void ChangeDescriptionMode(PopupDescriptionMode mode)
        {
            HideAllContainers();

            var flexStyle = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

            switch (mode)
            {
                case PopupDescriptionMode.Item:
                    _recycleContainer.style.display = flexStyle;
                    _mainTextContainer.style.display = flexStyle;
                    break;

                case PopupDescriptionMode.Building:
                    _healthContainer.style.display = flexStyle;
                    break;

                case PopupDescriptionMode.WirePortInput:
                    _wireContainer.style.display = flexStyle;
                    HideWireContainerIcons();
                    _wirePortInputIcon.style.display = flexStyle;
                    break;

                case PopupDescriptionMode.WirePortOutput:
                    _wireContainer.style.display = flexStyle;
                    HideWireContainerIcons();
                    _wirePortOutputIcon.style.display = flexStyle;
                    break;

                case PopupDescriptionMode.WireNet:
                    _wireContainer.style.display = flexStyle;
                    HideWireContainerIcons();
                    _wireNetIcon.style.display = flexStyle;
                    break;

                case PopupDescriptionMode.Other:
                    _mainTextContainer.style.display = flexStyle;
                    break;
            }
        }

        private void HideWireContainerIcons()
        {
            var style = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _wirePortInputIcon.style.display = style;
            _wirePortOutputIcon.style.display = style;
            _wireNetIcon.style.display = style;
        }
    }
}