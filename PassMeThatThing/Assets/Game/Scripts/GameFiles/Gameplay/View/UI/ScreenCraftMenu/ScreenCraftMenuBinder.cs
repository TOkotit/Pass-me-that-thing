using System.Collections.Generic;
using Assets.Game.Scripts.GameFiles.Gameplay.View.UI.ScreenCraftMenu;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.Buildings.Misc;
using Game.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenCraftMenuBinder : WindowBinder<ScreenCraftMenuViewModel>
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private VisualTreeAsset recipeViewPrefab;
        [SerializeField] private VisualTreeAsset recipeResViewPrefab;
        [SerializeField] private VisualTreeAsset resourceViewPrefab;

        private string _selectedRecipeId;
        private Dictionary<string, RecipeButton> _recipeButtonsByRecipeId = new();

        private VisualElement _root;
        private GroupBox _recipeContainer;
        private VisualElement _resourceContainer;

        private VisualElement _resultImg;
        private Label _resultText;
        private Button _craftButton;
        private Button _closeButton;

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;

            _recipeContainer = _root.Q<GroupBox>("RecipesContainer");
            _resourceContainer = _root.Q<VisualElement>("ResContainer");

            _resultImg = _root.Q<VisualElement>("CurrentResultIm");
            _resultText = _root.Q<Label>("CurrentResultLb");
            _craftButton = _root.Q<Button>("CraftBtn");
            _closeButton = _root.Q<Button>("CloseBtn");


        }
        
        private void Start()
        {
            ViewModel.RequestUpdateRecipes(UpdateRecipes);
            ViewModel.RequestSubForAvailableResources(UpdateResources);
            
            _craftButton.RegisterCallback<ClickEvent>(OnCraftClick);
            _closeButton.RegisterCallback<ClickEvent>(OnCloseClick);
        }

        private void OnDestroy()
        {
            ViewModel.RequestUnsubForAvailableResources(UpdateResources);

            _craftButton.UnregisterCallback<ClickEvent>(OnCraftClick);
            _closeButton.UnregisterCallback<ClickEvent>(OnCloseClick);

            foreach (var val in _recipeButtonsByRecipeId.Values)
            {
                val.OnRecipeClick -= OnRecipeClick;
            }
        }

        public void UpdateResources(IReadOnlyDictionary<Resource, float> d)
        {
            Debug.Log("[CRAFT] binder UpdateResources");
            _resourceContainer.Clear();
            foreach (var r in d)
            {
                var rData = ViewModel.resourceDatabase.GetResource(r.Key);
                
                var res = resourceViewPrefab.Instantiate();
                _resourceContainer.Add(res);

                res.Q<VisualElement>("ResImg").style.backgroundImage = new StyleBackground(rData.resourceImage);
                res.Q<Label>("NameLb").text = rData.resourceType.ToString();
                res.Q<Label>("AmountLb").text = r.Value.ToString();
            }
        }
        
        public void UpdateRecipes(List<WorkbenchItemRecipe> recipes)
        {
            _recipeContainer.Clear();

            var dropGroup = new RelativeDropgroup();
            _recipeContainer.Add(dropGroup);

            foreach (var r in recipes)
            {
                var recipeTemp = recipeViewPrefab.Instantiate();
                dropGroup.Content.Add(recipeTemp);

                var recipeButton = recipeTemp.Q<RecipeButton>("RecipeButton");

                recipeButton.recipeId = r.recipeId;
                recipeButton.OnRecipeClick += OnRecipeClick;
                _recipeButtonsByRecipeId.Add(r.recipeId, recipeButton);

                recipeTemp.Q<VisualElement>("ResultIm").style.backgroundImage = new StyleBackground(r.Item.ItemImage);
                recipeTemp.Q<Label>("ResultLb").text = r.Item.Id;
                
                //resources
                var recipeResContainer = recipeTemp.Q<GroupBox>("RecipeResContainer");

                foreach (var rp in r.Resources)
                {
                    var rData = ViewModel.resourceDatabase.GetResource(rp.resource);

                    var rRes = recipeResViewPrefab.Instantiate();
                    recipeResContainer.Add(rRes);

                    rRes.Q<VisualElement>("ResIm").style.backgroundImage = new StyleBackground(rData.resourceImage);
                    rRes.Q<Label>("ResLb").text = rp.amount.ToString();
                }
            }
        }
        
        public void UpdateResultInfo(Sprite sprite, string text)
        {
            _resultImg.style.backgroundImage = new StyleBackground(sprite);
            _resultText.text = text;
        }

        public void OnRecipeClick(string recipeId)
        {
            if (!string.IsNullOrEmpty(_selectedRecipeId))
            {
                if (_recipeButtonsByRecipeId.TryGetValue(_selectedRecipeId, out var recipeButton))
                {
                    recipeButton.RemoveFromClassList("recipe-button__selected");
                }
            }

            _recipeButtonsByRecipeId[recipeId].AddToClassList("recipe-button__selected");

            _selectedRecipeId = recipeId;

            var r = ViewModel.recipeDatabase.GetRecipe(recipeId);
            UpdateResultInfo(r.Item.ItemImage, r.Item.ItemName);
        }

        public void OnCraftClick(ClickEvent e)
        {
            if (!string.IsNullOrEmpty(_selectedRecipeId))
            {
                ViewModel.RequestCraft(_selectedRecipeId);
            }
        }

        public void OnCloseClick(ClickEvent e)
        {
            ViewModel.RequestGoToGameplay();
        }
    }
}