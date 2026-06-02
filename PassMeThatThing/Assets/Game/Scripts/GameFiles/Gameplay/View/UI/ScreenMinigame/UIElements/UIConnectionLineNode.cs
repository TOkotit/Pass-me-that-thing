using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    public class UIConnectionLineNode : MonoBehaviour, IPointerClickHandler
    {
        // [SerializeField] private UIConnectionManager manager;
        
        [Header("Параметры ноды")]
        [SerializeField] private string parameterGroup = "Power";
        [SerializeField] private Color nodeColor;
        [SerializeField] private float baseValue = 10f;
        [SerializeField] private bool isInput; // true - вход false - выход

        [SerializeField] private Image nodeImage;
        
        public event Action<UIConnectionLineNode> OnConnectionNodeClicked;
        
        public string ParameterGroup => parameterGroup;
        public float CurrentValue { get; set; }
        public RectTransform RectTransform => transform as RectTransform;
        public bool IsInput => isInput;

        public Image NodeImage => nodeImage;

        public Color NodeColor => nodeColor;

        private void Awake()
        {
            CurrentValue = baseValue;
        }

        public void SetupNode(Color color)
        {
            parameterGroup = color.ToString();
            nodeColor = color;
            nodeImage.color = color;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            
            OnConnectionNodeClicked?.Invoke(this);
        }
        
        public void UpdateValueVisual(bool isSelected)
        {
            if (isSelected)
            {
                transform.DOScale(1.5f, 0.3f).From(1f).SetEase(Ease.OutBack);
            }
            else
            {
                transform.DOScale(1, 0.3f).From(1.5f).SetEase(Ease.OutBack);
            }
            
        }
    }
}