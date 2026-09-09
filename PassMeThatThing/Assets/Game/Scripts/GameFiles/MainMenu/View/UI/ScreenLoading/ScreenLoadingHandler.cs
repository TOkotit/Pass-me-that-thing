using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Game.Scripts.GameFiles.MainMenu.View.UI.ScreenLoading
{
    public class ScreenLoadingHandler : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;
        private List<VisualElement> _dots=new();

        private Coroutine _loadingCoroutine;
        private bool _isDotsAnim = true;
        private int _currentDotIndex = 0;

        private WaitForSeconds _sleep = new WaitForSeconds(0.5f);

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;

            _dots.Add(_root.Q<VisualElement>("dot1"));
            _dots.Add(_root.Q<VisualElement>("dot2"));
            _dots.Add(_root.Q<VisualElement>("dot3"));
        }

        private void Start()
        {
            _loadingCoroutine = StartCoroutine(DotsAnimation());
        }
        private void OnDestroy()
        {
            StopCoroutine(_loadingCoroutine);
        }

        private IEnumerator DotsAnimation()
        {
            while (_isDotsAnim)
            {
                _dots[_currentDotIndex].style.opacity = 0;
                _currentDotIndex++;
                if (_currentDotIndex >= _dots.Count) _currentDotIndex = 0;
                _dots[_currentDotIndex].style.opacity = 100;
                yield return _sleep;
            }
        }
    }
}
