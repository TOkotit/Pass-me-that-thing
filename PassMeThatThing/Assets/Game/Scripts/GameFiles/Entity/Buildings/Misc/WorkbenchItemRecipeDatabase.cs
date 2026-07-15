using System;
using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    [CreateAssetMenu(fileName = "ItemRecipeDatabase", menuName = "RecipeDatabase") ]
    public class WorkbenchItemRecipeDatabase : ScriptableObject
    {
        [SerializeField] public List<WorkbenchItemRecipe> allRecipes = new ();
        
        public List<WorkbenchItemRecipe> AllRecipes => allRecipes;
    }

}