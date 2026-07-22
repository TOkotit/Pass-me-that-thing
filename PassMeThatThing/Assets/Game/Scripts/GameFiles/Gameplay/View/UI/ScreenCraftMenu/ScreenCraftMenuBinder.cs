using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.Buildings.Misc;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
// using Button = UnityEngine.UI.Button;
// using Image = UnityEngine.UI.Image;

namespace Game.Gameplay.View.UI.ScreenBuild
{
    public class ScreenCraftMenuBinder : WindowBinder<ScreenCraftMenuViewModel>
    {
        // [SerializeField] private RecipeViewElement recipeViewPrefab;
        // [SerializeField] private GameObject recipesContainer;
        //
        // [SerializeField] private Image resultImage;
        // [SerializeField] private TextMeshProUGUI resultText;
        // [SerializeField] private Button craftButton;
        //
        // [SerializeField] private ResourceViewElement resourceViewPrefab;
        // [SerializeField] private GameObject resourceContainer;

        private ResourceDatabase _resourceDatabase;
        private List<WorkbenchItemRecipe> _recipesData = new();
        
        private WorkbenchItemRecipe _selectedRecipe;
        
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private VisualTreeAsset recipeViewPrefab;
        [SerializeField] private VisualTreeAsset recipeResViewPrefab;
        [SerializeField] private VisualTreeAsset resourceViewPrefab;
        
        private VisualElement _root;
        private ListView _recipeContainer;
        private VisualElement _resourceContainer;

        private VisualElement _resultImg;
        private Label _resultText;
        private Button _craftButton;

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;

            _recipeContainer = _root.Q<ListView>("RecipesContainer");
            _resourceContainer = _root.Q<VisualElement>("ResContainer");

            _resultImg = _root.Q<VisualElement>("CurrentResultIm");
            _resultText = _root.Q<Label>("CurrentResultLb");
            _craftButton = _root.Q<Button>("CraftBtn");
            
        }
        
        private void Start()
        {
            ViewModel.RequestUpdateRecipes(UpdateRecipes);
            ViewModel.RequestAvailableResources(UpdateResources);

            InitRecipesList();
            
            _recipeContainer.selectedIndicesChanged += OnRecipeClick;
            
            _craftButton.RegisterCallback<ClickEvent>(OnCraftClick);
        }

        private void OnDestroy()
        {
            _recipeContainer.selectedIndicesChanged -= OnRecipeClick;
            _craftButton.UnregisterCallback<ClickEvent>(OnCraftClick);
        }

        public void UpdateResources(IReadOnlyDictionary<Resource,int> resources, ResourceDatabase resourceDatabase)
        {
            foreach (var r in resources)
            {
                var rData = resourceDatabase.GetResource(r.Key);
                
                var res = resourceViewPrefab.Instantiate();
                _resourceContainer.Add(res);

                res.Q<VisualElement>("ResImg").style.backgroundImage = new StyleBackground(rData.resourceImage);
                res.Q<Label>("NameLb").text = rData.resourceType.ToString();
                res.Q<Label>("AmountLb").text = r.Value.ToString();
            }
        }

        private void InitRecipesList()
        {
            _recipeContainer.makeItem = () => recipeViewPrefab.Instantiate();

            _recipeContainer.bindItem = (element, index) =>
            {
                var r = _recipesData[index];

                element.Q<VisualElement>("ResultIm").style.backgroundImage 
                    = new StyleBackground(r.Item.ItemImage);
                element.Q<Label>("ResultLb").text = r.Item.Id;
                
                var recipeResContainer = element.Q<GroupBox>("RecipeResContainer");
                
                foreach (var rp in r.Resources)
                {
                    var rData = _resourceDatabase.GetResource(rp.resource);
                    
                    var rRes = recipeResViewPrefab.Instantiate();
                    recipeResContainer.Add(rRes);
                    
                    rRes.Q<VisualElement>("ResIm").style.backgroundImage 
                        = new StyleBackground(rData.resourceImage);
                    rRes.Q<Label>("ResLb").text = rp.amount.ToString();
                }
            };
            
            _recipeContainer.itemsSource = _recipesData;
        }
        
        public void UpdateRecipes(List<WorkbenchItemRecipe> recipes, ResourceDatabase resourceDatabase)
        {
            _resourceDatabase = resourceDatabase;
            foreach (var r in recipes)
            {
                _recipesData.Add(r);
            }
            _recipeContainer.Rebuild();
        }
        
        public void UpdateResultInfo(Sprite sprite, string text)
        {
            _resultImg.style.backgroundImage = new StyleBackground(sprite);
            _resultText.text = text;
        }

        public void OnRecipeClick(IEnumerable<int> index)
        {
            _selectedRecipe = _recipesData[index.First()];
            
            UpdateResultInfo(_selectedRecipe.Item.ItemImage, _selectedRecipe.Item.ItemName);
        }

        public void OnCraftClick(ClickEvent e)
        {
            ViewModel.RequestCraft(_selectedRecipe.recipeId);
        }
        
    }
}