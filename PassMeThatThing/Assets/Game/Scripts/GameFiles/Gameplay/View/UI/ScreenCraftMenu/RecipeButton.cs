using Enums;
using Game.Scripts.GameFiles.Entity.Buildings.Misc;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.UI.ScreenCraftMenu
{
    [UxmlElement("RecipeButton")]
    public partial class RecipeButton : Button
    {
        public string recipeId;

        public event Action<string> OnRecipeClick;

        public RecipeButton() : base()
        {
            RegisterCallback<ClickEvent>(OnClick);
        }

        private void OnClick(ClickEvent e)
        {
            //Debug.Log($"RecipeButton OnClick {recipeId}");
            OnRecipeClick?.Invoke(recipeId);
        }
    }
}