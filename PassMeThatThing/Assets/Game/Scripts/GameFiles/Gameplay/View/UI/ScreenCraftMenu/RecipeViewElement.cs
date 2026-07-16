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
        public Image resImage;
        public TextMeshProUGUI resText;
    }
    
    public class RecipeViewElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image resultImage;
        [SerializeField] private TextMeshProUGUI resultText;

        [SerializeField] private List<ResourceViewElement> resources;

        public Image ResultImage => resultImage;

        public TextMeshProUGUI ResultText => resultText;
        
        public string RecipeId { get; set; }
        
        public event Action<RecipeViewElement, PointerEventData> OnClick;
        
        
        
        
        public void SetData(Sprite rSprite, string rText, List<(Sprite, string)> resourcePairs)
        {
            ResultImage.sprite = rSprite;
            ResultText.text = rText;

            for (int i=0; i < resourcePairs.Count && i < resources.Count; i++)
            {
                resources[i].resImage.sprite = resourcePairs[i].Item1;
                resources[i].resText.text = resourcePairs[i].Item2;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this, eventData);
        }
    }
}