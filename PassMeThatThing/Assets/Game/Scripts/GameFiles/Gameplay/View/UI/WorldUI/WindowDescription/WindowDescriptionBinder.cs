using Assets.Game.Scripts.GameFiles.UIWorld;
using DG.Tweening;
using Game.UI;
using R3;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.WindowDescription
{
    public class WindowDescriptionBinder : WorldWindowBinder<WindowDescriptionViewModel>
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;
        private Label _text;

        private CompositeDisposable _subs = new();

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _text = _root.Q<Label>("TextLb");
        }

        private void Start()
        {
            _subs.Add(ViewModel.enabled.Subscribe(SetVisibility));

            ViewModel.RequestSubCameraPos(ChangeRotation);

            ViewModel.RequestSubDescriptionText(ChangeText);
        }

        private void OnDestroy()
        {
            ViewModel.RequestUnSubCameraPos(ChangeRotation);
            ViewModel.RequestUnSubDescriptionText(ChangeText);
            _subs.Dispose();
        }

        public void SetVisibility(bool v)
        {
            if (v)
            {
                _root.visible = v;
                transform.DOScale(1f, 0.2f).From(0f);
            }
            else
            {
                transform.DOScale(0f, 0.2f).From(1f).OnComplete(() =>
                {
                    _root.visible = v;
                });
            }
        }

        public void ChangeText(string value)
        {
            //Debug.Log($"[WUI] ChangeText {value}");
            _text.text = value;
        }
    }
}