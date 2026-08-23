using Game.Gameplay.View.UI;
using Game.Scripts.GameFiles.Entity.Buildings.Misc.Craft;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    public class Workbench : NetworkBehaviour, Interactable
    {
        // [SerializeField] private WorkbenchItemRecipe recipe;
        [SerializeField] private ItemSpawner spawner;
        
        [Inject] private LocalCraftModel _craftModel;
        [Inject] private GameplayUIManager _uiManager;

        public ItemSpawner Spawner => spawner;

        public void Interact()
        {
            _craftModel.SetWorkbench(this);
            _uiManager.OpenScreenCraft();
        }
        
        public void SrbToggle()
        {
            
        }

        public void InteractWithItem(PhysicalItem item)
        {

        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
        }
    }
}