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
        private Dictionary<Resource, float> _resources = new();

        private VisualElement _root;
        private GroupBox _recipeContainer;
        private VisualElement _resourceContainer;

        private VisualElement _resultImg;
        private Label _resultText;
        private List<VisualElement> _resourcesPreview = new();
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

            for (var i=1; i<=5; i++)
                _resourcesPreview.Add(_root.Q<VisualElement>($"Res{i}"));

            ClearResourcesPreview();
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
            //Debug.Log("[CRAFT] binder UpdateResources");

            _resources.Clear();
            _resourceContainer.Clear();
            foreach (var r in d)
            {
                _resources.Add(r.Key, r.Value);

                var rData = ViewModel.resourceDatabase.GetResource(r.Key);
                
                var res = resourceViewPrefab.Instantiate();
                _resourceContainer.Add(res);

                res.Q<VisualElement>("ResImg").style.backgroundImage = new StyleBackground(rData.resourceImage);
                res.Q<Label>("AmountLb").text = r.Value.ToString();
            }

            UpdateResourcesPreview();
        }
        
        public void UpdateRecipes(Dictionary<string, List<WorkbenchItemRecipe>> recipes)
        {
            _recipeContainer.Clear();

            
            foreach (var category in recipes)
            {
                var dropGroup = new RelativeDropgroup();
                dropGroup.SetLabel(category.Key);
                _recipeContainer.Add(dropGroup);

                foreach (var r in category.Value)
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
            
        }
        
        public void OnRecipeClick(string recipeId)
        {
            if (!string.IsNullOrEmpty(_selectedRecipeId)
                && _recipeButtonsByRecipeId.TryGetValue(_selectedRecipeId, out var recipeButton))
            {
                recipeButton.RemoveFromClassList("recipe-button__selected");
            }

            _recipeButtonsByRecipeId[recipeId].AddToClassList("recipe-button__selected");

            _selectedRecipeId = recipeId;
            
            UpdateResultInfo();
            UpdateResourcesPreview();
        }
        public void UpdateResultInfo()
        {
            var r = ViewModel.recipeDatabase.GetRecipe(_selectedRecipeId);

            _resultImg.style.backgroundImage = new StyleBackground(r.Item.ItemImage);
            _resultText.text = r.Item.ItemName;
        }

        public void ClearResourcesPreview()
        {
            for (int i = 0; i < _resourcesPreview.Count; i++)
            {
                _resourcesPreview[i].Q<VisualElement>("ResImg").style.backgroundImage
                    = new StyleBackground();
                _resourcesPreview[i].Q<Label>("AmountLb").text
                    = "";

                _resourcesPreview[i].Q<GroupBox>("GroupBox").AddToClassList("resource-preview__not-success");
            }
        }

        public void UpdateResourcesPreview()
        {
            if (string.IsNullOrEmpty(_selectedRecipeId)) return;

            var r = ViewModel.recipeDatabase.GetRecipe(_selectedRecipeId);

            for (int i = 0; i < _resourcesPreview.Count; i++)
            {
                if (i < r.Resources.Count)
                {
                    var rData = ViewModel.resourceDatabase.GetResource(r.Resources[i].resource);

                    _resourcesPreview[i].Q<VisualElement>("ResImg").style.backgroundImage
                        = new StyleBackground(rData.resourceImage);

                    var curAmount = 0f;

                    _resources.TryGetValue(r.Resources[i].resource, out curAmount);

                    _resourcesPreview[i].Q<Label>("AmountLb").text
                        = $"{curAmount}/{r.Resources[i].amount}";

                    if (curAmount < r.Resources[i].amount)
                        _resourcesPreview[i].Q<GroupBox>("GroupBox").AddToClassList("resource-preview__not-success");
                    else
                        _resourcesPreview[i].Q<GroupBox>("GroupBox").RemoveFromClassList("resource-preview__not-success");
                }
                else
                {
                    _resourcesPreview[i].Q<VisualElement>("ResImg").style.backgroundImage
                        = new StyleBackground();
                    _resourcesPreview[i].Q<Label>("AmountLb").text
                        = "";

                    _resourcesPreview[i].Q<GroupBox>("GroupBox").AddToClassList("resource-preview__not-success");
                    
                }
            }
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