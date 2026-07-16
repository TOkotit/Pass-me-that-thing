using System;
using System.Collections.Generic;
using Game.Scripts.GameFiles.Entity.Buildings.Misc;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenCraftMenuBinder : WindowBinder<ScreenCraftMenuViewModel>
    {
        [SerializeField] private RecipeViewElement recipeViewPrefab;
        [SerializeField] private GameObject recipesContainer;
        
        
        [SerializeField] private Image resultImage;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button craftButton;
        
        private List<RecipeViewElement> _recipes = new();
        
        private RecipeViewElement _selectedRecipe;
        
        private void Start()
        {
            ViewModel.RequestUpdateRecipes(UpdateRecipes);
            craftButton.onClick.AddListener(OnCraftClick);
        }

        private void OnDestroy()
        {
            craftButton.onClick.RemoveListener(OnCraftClick);
            
            foreach (var recipeViewElement in _recipes)
            {
                recipeViewElement.OnClick -= OnRecipeClick;
            }
        }

        public void UpdateRecipes(List<WorkbenchItemRecipe> recipes, ResourceDatabase resourceDatabase)
        {
            foreach (var r in recipes)
            {
                var recipeViewElement = Instantiate(recipeViewPrefab, recipesContainer.transform);
                
                recipeViewElement.OnClick += OnRecipeClick;
                recipeViewElement.RecipeId = r.recipeId;
                
                _recipes.Add(recipeViewElement);
                
                var resourcesData = new List<(Sprite, string)>();
                foreach (var rp in r.Resources)
                {
                    var rData = resourceDatabase.GetResource(rp.resource);
                    resourcesData.Add((rData.resourceImage, rp.amount.ToString()));
                }
                
                recipeViewElement.SetData(r.Item.ItemImage, r.Item.Id, resourcesData);
            }
        }

        public void UpdateResultInfo(RecipeViewElement recipeViewElement)
        {
            resultImage.sprite = recipeViewElement.ResultImage.sprite;
            resultText.text = recipeViewElement.ResultText.text;
        }

        public void OnRecipeClick(RecipeViewElement recipeViewElement, PointerEventData data)
        {
            _selectedRecipe = recipeViewElement;
            
            UpdateResultInfo(_selectedRecipe);
        }

        public void OnCraftClick()
        {
            ViewModel.RequestCraft(_selectedRecipe.RecipeId);
        }
        
    }
}