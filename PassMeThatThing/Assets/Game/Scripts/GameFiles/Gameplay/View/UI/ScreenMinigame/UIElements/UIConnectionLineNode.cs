using System;
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
        [SerializeField] private float baseValue = 10f;
        [SerializeField] private bool isInput; // true - вход false - выход

        [SerializeField] private Image nodeImage;
        
        public event Action<UIConnectionLineNode> OnConnectionNodeClicked;
        
        public string ParameterGroup => parameterGroup;
        public float CurrentValue { get; set; }
        public RectTransform RectTransform => transform as RectTransform;
        public bool IsInput => isInput;

        public Image NodeImage => nodeImage;

        private void Awake()
        {
            CurrentValue = baseValue;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnConnectionNodeClicked?.Invoke(this);
        }
        
        public void UpdateValueVisual()
        {
            Debug.Log($"Нода {name} обновила значение: {CurrentValue}");
        }
    }
}