using DG.Tweening;
using Game.UI;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.PopupDescription
{
    public class PopupDescriptionBinder : PopupBinder<PopupDescriptionViewModel>
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private Vector2 popupOffset = new Vector2(25f, 25f);
        private VisualElement _root;
        private VisualElement _container;
        private Label _text;

        private CompositeDisposable _subs = new();

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _text = _root.Q<Label>("TextLb");
            _container = _root.Q<VisualElement>("Container");
        }

        private void Start()
        {
            _subs.Add(ViewModel.enabled.Subscribe(SetVisibility));
            _subs.Add(ViewModel.screenPos.Subscribe(UpdatePosition));

            ViewModel.RequestSubDescriptionText(ChangeText);
        }

        private void OnDestroy()
        {
            _subs.Dispose();

            ViewModel.RequestUnSubDescriptionText(ChangeText);
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
            Debug.Log($"[WUI] ChangeText {value}");
            _text.text = value;
        }
    }
}