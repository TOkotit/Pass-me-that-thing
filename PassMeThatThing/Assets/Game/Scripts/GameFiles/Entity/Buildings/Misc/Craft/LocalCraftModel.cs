using System;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc.Craft
{
    public class LocalCraftModel
    {
        private string _selectedRecipeId;
        private Workbench _workbench;
        
        public event Action<string, Workbench> OnCraft;

        public void Craft(string recipeData)
        {
            _selectedRecipeId = recipeData;
            if (_workbench != null)
                OnCraft?.Invoke(_selectedRecipeId, _workbench);
        }

        public void SetWorkbench(Workbench workbench)
        {
            _workbench = workbench;
        }

        public void Clear()
        {
            _selectedRecipeId = null;
            _workbench = null;
        }
    }
}