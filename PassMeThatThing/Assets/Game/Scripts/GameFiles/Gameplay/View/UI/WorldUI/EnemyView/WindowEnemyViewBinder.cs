using Assets.Game.Scripts.GameFiles.UIWorld;
using DG.Tweening;
using Game.UI;
using R3;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.WindowDescription
{
    public class WindowEnemyViewBinder : WorldWindowBinder<WindowEnemyViewViewModel>
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;
        private ProgressBar _healthBar;
        private ProgressBar _toughnessBar;
        private ProgressBar _attackBar;

        private CompositeDisposable _subs = new();

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _healthBar = _root.Q<ProgressBar>("EnemyHealth");
            _toughnessBar = _root.Q<ProgressBar>("EnemyToughness");
            _attackBar = _root.Q<ProgressBar>("EnemyElapsedAttack");
        }

        private void Start()
        {
            _subs.Add(ViewModel.enabled.Subscribe(SetVisibility));

            ViewModel.RequestSubCameraPos(ChangeRotation);


            ViewModel.RequestSubEnemyHealth(UpdateHealthBar);
            ViewModel.RequestSubEnemyToughness(UpdateToughnessBar);
            ViewModel.RequestSubEnemyAttack(UpdateElapsedAttackBar);

            ViewModel.enabled.Value = true;

            gameObject.transform.SetParent(ViewModel.parent);
            gameObject.transform.localPosition = ViewModel.windowOffset;
        }

        private void OnDestroy()
        {
            ViewModel.RequestUnSubCameraPos(ChangeRotation);
            
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

        public void UpdateHealthBar(int value, int maxValue)
        {
            _healthBar.value = (float)value / maxValue * 100;
            Debug.Log($"[UI] h {_healthBar.value}");
        }
        public void UpdateToughnessBar(int value, int maxValue)
        {
            _toughnessBar.value = (float)value / maxValue * 100;
            Debug.Log($"[UI] t {_healthBar.value}");
        }
        public void UpdateElapsedAttackBar(float value, float maxValue)
        {
            _attackBar.value = value / maxValue * 100;
            Debug.Log($"[UI] e {_healthBar.value}");
        }
    }
}