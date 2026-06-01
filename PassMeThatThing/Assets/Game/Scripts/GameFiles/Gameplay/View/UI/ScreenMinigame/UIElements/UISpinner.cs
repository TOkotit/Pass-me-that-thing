using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    public class UISpinner : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;
        private Vector2 _lastMousePos;

        public event Action<float> OnValueDeltaChanged;
        public event Action<float> OnValueChanged;
        
        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentRectTransform = transform.parent as RectTransform;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _lastMousePos = GetMousePosRelativeToCenter(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var currentMousePos = GetMousePosRelativeToCenter(eventData);

            if (currentMousePos.magnitude > 0.1f && _lastMousePos.magnitude > 0.1f)
            {
                var angleDelta = Vector2.SignedAngle(_lastMousePos, currentMousePos);

                _rectTransform.Rotate(Vector3.forward, angleDelta);
                OnValueDeltaChanged?.Invoke(angleDelta);
                OnValueChanged?.Invoke(_rectTransform.localEulerAngles.z / 360f);
                _lastMousePos = currentMousePos;
            }
        }
        
        private Vector2 GetMousePosRelativeToCenter(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out var localPoint
            );

            return localPoint - (Vector2)_rectTransform.localPosition;
        }
    }
}