using System;
using System.Collections.Generic;
using Game.Scripts.GameFiles.Entity.Buildings.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    [Serializable]
    public class ResourceVisualElement
    {
        [SerializeField] private Image resImage;
        [SerializeField] private TextMeshProUGUI resText;
    }
    
    public class RecipeElement : MonoBehaviour
    {
        [SerializeField] private Image resultImage;
        [SerializeField] private Text resultText;

        [SerializeField] private List<ResourceVisualElement> resources;

        public void SetData(Sprite rSprite, string rText, List<(Sprite, string)> resourcePairs)
        {
            resultImage.sprite = rSprite;
            resultText.text = rText;

            for (int i=0; i < resourcePairs.Count && i < resources.Count; i++)
            {
                
            }
        }
    }
}