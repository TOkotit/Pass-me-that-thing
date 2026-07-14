using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class SelectionElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image image;
        
        private float _normalScale = 1f;
        private float _selectedScale = 1.2f;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOScale(_selectedScale, 0.3f).From(_normalScale).SetEase(Ease.OutElastic);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(_normalScale, 0.3f).From(_selectedScale).SetEase(Ease.OutElastic);
        }
    }
}