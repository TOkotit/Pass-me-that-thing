using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc.Craft
{
    public class CraftManager : NetworkBehaviour
    {
        [Inject] private WorkbenchItemRecipeDatabase _recipeDatabase;
        
        [Command(requiresAuthority =  false)]
        public void CmdCraft(Workbench workbench, string recipeID)
        {
            var recipe = _recipeDatabase.GetRecipe(recipeID);
            Craft(workbench, recipe);
        }

        
        [Server]
        private void Craft(Workbench workbench, WorkbenchItemRecipe recipe)
        {
            Debug.Log($"Recipe: {recipe}, Resources: {recipe?.Resources != null}, Count: {recipe?.Resources?.Count ?? -1}");

            foreach (var pair in recipe.Resources)
            {
                if (!MainResourceStorage.Instance.HasResource(pair.resource, pair.amount))
                {
                    Debug.Log("Not enough resources!");
                    return;
                }
            }

            foreach (var pair in recipe.Resources)
            {
                MainResourceStorage.Instance.RemoveResource(pair.resource, pair.amount);
            }
            workbench.Spawner.Item = recipe.Item;
            workbench.Spawner.Interact();
        }
    }
}