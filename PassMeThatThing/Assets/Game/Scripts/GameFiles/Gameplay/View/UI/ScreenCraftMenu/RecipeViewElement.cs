using System;
using System.Collections.Generic;
using Game.Scripts.GameFiles.Entity.Buildings.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    [Serializable]
    public class ResourceViewElement
    {
        [SerializeField] private Image resImage;
        [SerializeField] private TextMeshProUGUI resText;
    }
    
    public class RecipeViewElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image resultImage;
        [SerializeField] private Text resultText;

        [SerializeField] private List<ResourceViewElement> resources;
        
        

        public Image ResultImage => resultImage;

        public Text ResultText => resultText;
        
        public string RecipeId { get; set; }
        
        public event Action<RecipeViewElement, PointerEventData> OnClick;
        
        
        
        
        public void SetData(Sprite rSprite, string rText, List<(Sprite, string)> resourcePairs)
        {
            ResultImage.sprite = rSprite;
            ResultText.text = rText;

            for (int i=0; i < resourcePairs.Count && i < resources.Count; i++)
            {
                
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this, eventData);
        }
    }
}