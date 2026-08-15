using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    [CreateAssetMenu(fileName = "ItemRecipeDatabase", menuName = "RecipeDatabase") ]
    public class WorkbenchItemRecipeDatabase : ScriptableObject
    {
        [SerializeField] public List<WorkbenchItemRecipe> allRecipes = new ();
        [SerializeField] private SerializedDictionary<string, List<WorkbenchItemRecipe>> recipesByCategoryId = new();

        public List<WorkbenchItemRecipe> AllRecipes => allRecipes;

        public Dictionary<string, List<WorkbenchItemRecipe>> RecipesByCategory => recipesByCategoryId;

        public WorkbenchItemRecipe GetRecipe(string id)
        {
            return allRecipes.Find(b => b.recipeId == id);
        }
    }

}