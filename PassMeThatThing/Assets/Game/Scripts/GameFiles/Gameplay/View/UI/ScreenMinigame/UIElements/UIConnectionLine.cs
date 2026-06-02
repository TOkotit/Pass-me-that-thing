using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    public class UIConnectionLine : MonoBehaviour
    {
        [SerializeField] private Image lineImage;
        
        private RectTransform _startPoint;
        private RectTransform _endPoint;
        private float _thickness;

        public void Setup(RectTransform start, RectTransform end, float thickness = 15f)
        {
            _startPoint = start;
            _endPoint = end;
            _thickness = thickness;
            UpdateLineVisual();
        }

        public void SetColor(Color color)
        {
            lineImage.color = color;
        }

        private void Update()
        {
            UpdateLineVisual();
        }

        private void UpdateLineVisual()
        {
            if (_startPoint == null || _endPoint == null) return;

            var pointA = _startPoint.position;
            var pointB = _endPoint.position;

            var difference = pointB - pointA;
            var distance = difference.magnitude;
            var angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

            lineImage.rectTransform.position = pointA + (difference / 2f);
            
            lineImage.rectTransform.sizeDelta = new Vector2(distance, _thickness);
            
            lineImage.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}