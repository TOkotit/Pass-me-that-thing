using System;
using System.Collections.Generic;
using Game.Scripts.GameFiles.Entity.Buildings.Misc;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenCraftMenuBinder : WindowBinder<ScreenCraftMenuViewModel>
    {
        [SerializeField] private RecipeElement recipePrefab;
        [SerializeField] private GameObject recipesContainer;
        
        
        [SerializeField] private Image resultImage;
        [SerializeField] private TextMeshProUGUI resultText;
        
        private List<RecipeElement> _recipes = new();
        
        private void Start()
        {
            
        }

        private void OnDestroy()
        {
            
        }


        public void UpdateRecipes(List<WorkbenchItemRecipe> recipes, ResourceDatabase resourceDatabase)
        {
            foreach (var r in recipes)
            {
                var instance = Instantiate(recipePrefab, recipesContainer.transform);
                _recipes.Add(instance);
                
                var resoursesData = new List<(Sprite, string)>();
                
                foreach (var rp in r.Resources)
                {
                    var rData = resourceDatabase.GetResource(rp.resource);
                    resoursesData.Add((rData.resourceImage, rData.resourceName));
                }
                
                instance.SetData(r.Item.ItemImage, r.Item.Id, resoursesData);
            }
        }
    }
}