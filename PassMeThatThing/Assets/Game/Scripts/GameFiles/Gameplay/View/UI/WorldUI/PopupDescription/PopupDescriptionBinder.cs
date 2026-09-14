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
        private Label _text;

        private VisualElement _recycleContainer;
        private VisualElement _recycleResourceContainer;

        private CompositeDisposable _subs = new();

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _text = _root.Q<Label>("TextLb");
            _container = _root.Q<VisualElement>("Container");

            _recycleContainer = _root.Q<VisualElement>("RecycleContainer");
            _recycleResourceContainer = _root.Q<VisualElement>("ResourceContainer");
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

        public void ChangeDescriptionMode(PopupDescriptionMode mode)
        {
            _recycleContainer.style.display
                = mode == PopupDescriptionMode.Item 
                ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
                : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }
    }
}