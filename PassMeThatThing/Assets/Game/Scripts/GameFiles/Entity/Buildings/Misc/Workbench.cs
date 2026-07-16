using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    public class Workbench : NetworkBehaviour, Interactable
    {
        [SerializeField] private WorkbenchItemRecipe recipe;
        [SerializeField] private ItemSpawner spawner;

        public ItemSpawner Spawner => spawner;

        public void Interact()
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
            Spawner.Item = recipe.Item;
            Spawner.Interact();
        }
        
        public void SrbToggle()
        {
            
        }

        public void InteractWithItem(PhysicalItem item)
        {
            
        }
    }
}